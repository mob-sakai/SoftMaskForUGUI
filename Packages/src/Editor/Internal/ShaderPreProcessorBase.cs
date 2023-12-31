using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

namespace Coffee.UISoftMask.Internal
{
    internal abstract class ShaderPreProcessorBase : IPreprocessShaders
    {
        public abstract int callbackOrder { get; }

        public abstract void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data);

        protected static void Log(Shader shader, IList<ShaderCompilerData> data, IEnumerable<string> ignoredKeywords)
        {
            Console.WriteLine($"[{shader.name}] {data.Count} variants available:");
            foreach (var (platform, keywords) in data
                         .Select(d => (d.shaderCompilerPlatform, GetKeyWords(d, ignoredKeywords)))
                         .OrderBy(t => t.Item2))
            {
                Console.WriteLine($"  - {platform}: {keywords}");
            }

            Console.WriteLine();
        }

        protected static void StripUnusedVariantsIf(IList<ShaderCompilerData> data, bool condition)
        {
            if (condition)
            {
                data.Clear();
            }
        }

        protected static void StripUnusedVariantsIf(IList<ShaderCompilerData> data, Predicate<ShaderCompilerData> pred)
        {
            for (var i = data.Count - 1; i >= 0; --i)
            {
                if (pred(data[i]))
                {
                    data.RemoveAt(i);
                }
            }
        }

        private static string GetKeyWords(ShaderCompilerData data, IEnumerable<string> ignoredKeywords)
        {
            return string.Join("|", data.shaderKeywordSet.GetShaderKeywords()
                .Select(x => x.name)
                .Where(k => ignoredKeywords == null || !ignoredKeywords.Contains(k))
                .OrderBy(k => k));
        }
    }
}
