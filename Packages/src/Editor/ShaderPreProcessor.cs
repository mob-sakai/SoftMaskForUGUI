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
            // If the shader is not SoftMask/softMaskable, do nothing.
            var type = GetShaderType(shader);
            if (type == ShaderType.None) return;

            // Remove the 'UI_SOFT_MASKABLE_EDITOR' shader variants.
            var editor = new ShaderKeyword(shader, "UI_SOFT_MASKABLE_EDITOR");
            StripUnusedVariantsIf(data, d => d.shaderKeywordSet.IsEnabled(editor));

            // If the shader is separated soft-maskable, remove non-soft-maskable variants.
            if (type == ShaderType.SeparatedSoftMaskable)
            {
                var softMaskable = new ShaderKeyword(shader, "UI_SOFT_MASKABLE");
                StripUnusedVariantsIf(data, d => !d.shaderKeywordSet.IsEnabled(softMaskable));
            }

            // If strip shader variants is disabled in the project, do nothing.
            if (!UISoftMaskProjectSettings.instance.m_StripShaderVariants) return;

            // If soft mask is disabled in the project, remove the all shader variants.
            var softMaskDisabled = !UISoftMaskProjectSettings.instance.m_SoftMaskEnabled;
            if (softMaskDisabled)
            {
                StripUnusedVariantsIf(data, true);
                return;
            }

            // If stereo is disabled in the project, remove the 'UI_SOFT_MASKABLE_STEREO' shader variants.
            if (!UISoftMaskProjectSettings.stereoEnabled)
            {
                var stereo = new ShaderKeyword(shader, "UI_SOFT_MASKABLE_STEREO");
                StripUnusedVariantsIf(data, d => d.shaderKeywordSet.IsEnabled(stereo));
            }

            // Log
            if (snippet.shaderType == UnityEditor.Rendering.ShaderType.Fragment)
            {
                Log(shader, data, s_IgnoredKeywords);
            }
        }

        private static ShaderType GetShaderType(Shader shader)
        {
            if (!shader) return ShaderType.None;
            var name = shader.name;
            if (name == "Hidden/UI/SoftMask") return ShaderType.SoftMask;
            if (!name.EndsWith(" (SoftMaskable)")) return ShaderType.None;
            return name.StartsWith("Hidden/")
                ? ShaderType.SeparatedSoftMaskable
                : ShaderType.HybridSoftMaskable;
        }

        private enum ShaderType
        {
            None,
            SoftMask,
            HybridSoftMaskable,
            SeparatedSoftMaskable
        }
    }
}
