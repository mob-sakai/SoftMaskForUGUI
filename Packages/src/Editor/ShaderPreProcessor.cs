using System.Collections.Generic;
using Coffee.UISoftMaskInternal;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Coffee.UISoftMask
{
    internal class ShaderPreProcessor : ShaderPreProcessorBase
    {
        private static readonly string[] s_IgnoredKeywords =
        {
            "UNITY_UI_ALPHACLIP",
            "UNITY_UI_CLIP_RECT"
        };

        public override int callbackOrder => 1;

        public override void OnProcessShader(Shader shader, ShaderSnippetData snippet,
            IList<ShaderCompilerData> data)
        {
            // If the shader is not SoftMask/SoftMaskable, do nothing.
            var name = shader.name;
            if (name != "Hidden/UI/SoftMask" && !SoftMaskUtils.IsSoftMaskableShaderName(name)) return;

            // Remove the 'SOFTMASK_EDITOR' shader variants.
            var editor = new ShaderKeyword(shader, "SOFTMASK_EDITOR");
            StripUnusedVariantsIf(data, d => d.shaderKeywordSet.IsEnabled(editor));

            // If strip shader variants is disabled in the project, do nothing.
            if (!UISoftMaskProjectSettings.instance.m_StripShaderVariants) return;

            // If soft mask is disabled in the project, remove the all shader variants.
            var softMaskDisabled = !UISoftMaskProjectSettings.instance.m_SoftMaskEnabled;
            if (softMaskDisabled)
            {
                StripUnusedVariantsIf(data, true);
                return;
            }

            // Log
            if (snippet.shaderType == ShaderType.Fragment)
            {
                Log(shader, data, s_IgnoredKeywords);
            }
        }
    }
}
