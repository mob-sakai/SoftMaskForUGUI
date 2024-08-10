using System;
using System.Collections.Generic;
using Coffee.UISoftMaskInternal;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
#if TMP_ENABLE
using TMPro;
#endif

namespace Coffee.UISoftMask
{
    /// <summary>
    /// Utility class containing various functions and resources for soft masking.
    /// </summary>
    internal static class SoftMaskUtils
    {
        /// <summary>
        /// Object pool for CommandBuffer instances.
        /// </summary>
        public static readonly ObjectPool<CommandBuffer> commandBufferPool =
            new ObjectPool<CommandBuffer>(
                () => new CommandBuffer(),
                x => x != null,
                x => x.Clear());

        /// <summary>
        /// Object pool for MaterialPropertyBlock instances.
        /// </summary>
        public static readonly ObjectPool<MaterialPropertyBlock> materialPropertyBlockPool =
            new ObjectPool<MaterialPropertyBlock>(
                () => new MaterialPropertyBlock(),
                x => x != null,
                x => x.Clear());

        private static Material s_SoftMaskingMaterialAdd;
        private static Material s_SoftMaskingMaterialSub;
        private static readonly int s_SoftMaskableStereo = Shader.PropertyToID("_SoftMaskableStereo");
        private static readonly int s_SoftMaskableEnable = Shader.PropertyToID("_SoftMaskableEnable");
        private static readonly int s_SoftMaskOutsideColor = Shader.PropertyToID("_SoftMaskOutsideColor");
        private static readonly int s_SoftMaskTex = Shader.PropertyToID("_SoftMaskTex");
        private static readonly int s_SoftMaskColor = Shader.PropertyToID("_SoftMaskColor");
        private static readonly int s_MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int s_ColorMask = Shader.PropertyToID("_ColorMask");
        private static readonly int s_BlendOp = Shader.PropertyToID("_BlendOp");
        private static readonly int s_StencilReadMask = Shader.PropertyToID("_StencilReadMask");
        private static readonly int s_ThresholdMin = Shader.PropertyToID("_ThresholdMin");
        private static readonly int s_ThresholdMax = Shader.PropertyToID("_ThresholdMax");

        private static readonly string[] s_SoftMaskableShaderNameFormats =
        {
            "{0}",
            "Hidden/{0} (SoftMaskable)",
            "{0} (SoftMaskable)"
        };

        private static readonly Dictionary<int, string> s_SoftMaskableShaderNames = new Dictionary<int, string>();

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void InitializeOnLoadMethod()
        {
#if TMP_ENABLE
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(obj =>
            {
                if (!(obj is TextMeshProUGUI text)) return;

                if (text.TryGetComponent<SoftMask>(out var sm))
                {
                    UpdateSubMeshUI(text, sm.showMaskGraphic, sm.antiAliasingThreshold, sm.softnessRange);
                }
                else if (text.TryGetComponent<MaskingShape>(out var ms))
                {
                    UpdateSubMeshUI(text, ms.showMaskGraphic, ms.antiAliasingThreshold, ms.softnessRange);
                }
            });
#endif

#if UNITY_EDITOR
            EditorApplication.projectChanged += s_SoftMaskableShaderNames.Clear;
#endif
        }

#if TMP_ENABLE
        private static void UpdateSubMeshUI(TextMeshProUGUI text, bool show, float aa, MinMax01 softness)
        {
            var subMeshes = ListPool<TMP_SubMeshUI>.Rent();
            text.GetComponentsInChildren(subMeshes, 1);

            for (var i = 0; i < subMeshes.Count; i++)
            {
                var maskingShape = subMeshes[i].GetOrAddComponent<MaskingShape>();
                maskingShape.hideFlags = UISoftMaskProjectSettings.hideFlagsForTemp;
                maskingShape.antiAliasingThreshold = aa;
                maskingShape.softnessRange = softness;
                maskingShape.showMaskGraphic = show;
            }

            ListPool<TMP_SubMeshUI>.Return(ref subMeshes);
        }
#endif

        /// <summary>
        /// Applies properties to a MaterialPropertyBlock for soft masking.
        /// </summary>
        public static void ApplyMaterialPropertyBlock(MaterialPropertyBlock mpb, int depth, Texture texture,
            MinMax01 threshold, float alpha)
        {
            Profiler.BeginSample("(SM4UI)[SoftMaskUtils] ApplyMaterialPropertyBlock");
            var colorMask = Vector4.zero;
            colorMask[depth] = alpha;
            mpb.SetVector(s_ColorMask, colorMask);
            mpb.SetTexture(s_MainTex, texture ? texture : null);
            mpb.SetFloat(s_ThresholdMin, threshold.min);
            mpb.SetFloat(s_ThresholdMax, threshold.max);
            Profiler.EndSample();
        }

        /// <summary>
        /// Gets the soft masking material based on the masking method.
        /// </summary>
        public static Material GetSoftMaskingMaterial(MaskingShape.MaskingMethod method)
        {
            return method == MaskingShape.MaskingMethod.Additive
                ? GetSoftMaskingMaterial(ref s_SoftMaskingMaterialAdd, BlendOp.Add)
                : GetSoftMaskingMaterial(ref s_SoftMaskingMaterialSub, BlendOp.ReverseSubtract);
        }

        /// <summary>
        /// Gets or creates the soft masking material with a specific blend operation.
        /// </summary>
        private static Material GetSoftMaskingMaterial(ref Material mat, BlendOp op)
        {
            if (mat) return mat;

            mat = new Material(Shader.Find("Hidden/UI/SoftMask"))
            {
                hideFlags = HideFlags.DontSave | HideFlags.NotEditable
            };
            mat.SetInt(s_BlendOp, (int)op);
            return mat;
        }

        public static Material CreateSoftMaskable(
            Material baseMat,
            Texture softMaskBuffer,
            int softMaskDepth,
            int stencilBits,
            bool isStereo,
            UISoftMaskProjectSettings.FallbackBehavior fallbackBehavior)
        {
            Profiler.BeginSample("(SM4UI)[SoftMaskableMaterial] Create > Create New Material");
            var mat = new Material(baseMat)
            {
                shader = GetSoftMaskableShader(baseMat.shader, fallbackBehavior),
                hideFlags = HideFlags.HideAndDontSave
            };
            Profiler.EndSample();

            Profiler.BeginSample("(SM4UI)[SoftMaskableMaterial] Create > Set Properties");
            mat.SetTexture(s_SoftMaskTex, softMaskBuffer);
            mat.SetInt(s_SoftMaskableStereo, isStereo ? 1 : 0);
            mat.SetInt(s_SoftMaskableEnable, 1);
            mat.SetInt(s_StencilReadMask, stencilBits);
            mat.SetVector(s_SoftMaskColor, new Vector4(
                0 <= softMaskDepth ? 1 : 0,
                1 <= softMaskDepth ? 1 : 0,
                2 <= softMaskDepth ? 1 : 0,
                3 <= softMaskDepth ? 1 : 0
            ));
            Profiler.EndSample();

#if UNITY_EDITOR
            mat.EnableKeyword("SOFTMASK_EDITOR");
            mat.SetVector(s_SoftMaskOutsideColor,
                UISoftMaskProjectSettings.useStencilOutsideScreen ? Vector4.one : Vector4.zero);
#endif
            return mat;
        }

        public static Shader GetSoftMaskableShader(Shader baseShader,
            UISoftMaskProjectSettings.FallbackBehavior fallback)
        {
            Profiler.BeginSample("(SM4UI)[SoftMaskUtils] GetSoftMaskableShader > From cache");
            var id = baseShader.GetInstanceID();
            if (s_SoftMaskableShaderNames.TryGetValue(id, out var shaderName))
            {
                var shader = Shader.Find(shaderName);
                Profiler.EndSample();

                return shader;
            }

            Profiler.EndSample();

            Profiler.BeginSample("(SM4UI)[SoftMaskableMaterial] GetSoftMaskableShader > Find soft maskable shader");
            shaderName = baseShader.name;
            for (var i = 0; i < s_SoftMaskableShaderNameFormats.Length; i++)
            {
                var name = string.Format(s_SoftMaskableShaderNameFormats[i], shaderName);
                if (!name.Contains("(SoftMaskable)", StringComparison.Ordinal)) continue;

                var shader = Shader.Find(name);
                if (!shader) continue;

                s_SoftMaskableShaderNames.Add(id, name);
                Profiler.EndSample();
                return shader;
            }

            Profiler.EndSample();

            Profiler.BeginSample("(SM4UI)[SoftMaskableMaterial] GetSoftMaskableShader > Fallback");
            switch (fallback)
            {
                case UISoftMaskProjectSettings.FallbackBehavior.DefaultSoftMaskable:
                {
                    s_SoftMaskableShaderNames.Add(id, "Hidden/UI/Default (SoftMaskable)");
                    var shader = Shader.Find("Hidden/UI/Default (SoftMaskable)");
                    Profiler.EndSample();
                    return shader;
                }
                case UISoftMaskProjectSettings.FallbackBehavior.None:
                {
                    s_SoftMaskableShaderNames.Add(id, shaderName);
                    Profiler.EndSample();
                    return baseShader;
                }
                default:
                    Profiler.EndSample();
                    throw new ArgumentOutOfRangeException(nameof(fallback), fallback, null);
            }
        }
    }
}
