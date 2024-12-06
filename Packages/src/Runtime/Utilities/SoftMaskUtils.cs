using Coffee.UISoftMaskInternal;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
#if URP_ENABLE
using UnityEngine.Rendering.Universal;
#endif
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
        public static readonly UISoftMaskInternal.ObjectPool<CommandBuffer> commandBufferPool =
            new UISoftMaskInternal.ObjectPool<CommandBuffer>(
                () => new CommandBuffer(),
                x => x != null,
                x => x.Clear());

        /// <summary>
        /// Object pool for MaterialPropertyBlock instances.
        /// </summary>
        public static readonly UISoftMaskInternal.ObjectPool<MaterialPropertyBlock> materialPropertyBlockPool =
            new UISoftMaskInternal.ObjectPool<MaterialPropertyBlock>(
                () => new MaterialPropertyBlock(),
                x => x != null,
                x => x.Clear());

        private static Material s_SoftMaskingMaterialAdd;
        private static Material s_SoftMaskingMaterialSub;
        private static readonly int s_SoftMaskableStereo = Shader.PropertyToID("_SoftMaskableStereo");
        private static readonly int s_SoftMaskOutsideColor = Shader.PropertyToID("_SoftMaskOutsideColor");
        private static readonly int s_SoftMaskTex = Shader.PropertyToID("_SoftMaskTex");
        private static readonly int s_SoftMaskColor = Shader.PropertyToID("_SoftMaskColor");
        private static readonly int s_MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int s_ColorMask = Shader.PropertyToID("_ColorMask");
        private static readonly int s_BlendOp = Shader.PropertyToID("_BlendOp");
        private static readonly int s_ThresholdMin = Shader.PropertyToID("_ThresholdMin");
        private static readonly int s_ThresholdMax = Shader.PropertyToID("_ThresholdMax");
        private static readonly int s_RenderScale = Shader.PropertyToID("_RenderScale");
        private static readonly int s_DynamicResolutionScale = Shader.PropertyToID("_DynamicResolutionScale");
        private static float s_CurrentRenderScale = 1;
        private static Vector2 s_CurrentDynamicResolutionScale = Vector2.one;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void InitializeOnLoadMethod()
        {
            s_CurrentRenderScale = 1;
            Shader.SetGlobalFloat(s_RenderScale, s_CurrentRenderScale);
#if URP_ENABLE
            UIExtraCallbacks.onBeforeCanvasRebuild += () =>
            {
                // Discard variations lesser than 0.05f.
                var renderScale = 1f;
                if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urpAsset
                    && 0.05f < Mathf.Abs(urpAsset.renderScale - 1))
                {
                    renderScale = urpAsset.renderScale;
                }

                if (!Mathf.Approximately(s_CurrentRenderScale, renderScale))
                {
                    s_CurrentRenderScale = renderScale;
                    Shader.SetGlobalFloat(s_RenderScale, s_CurrentRenderScale);
                }
            };
#endif
            UIExtraCallbacks.onBeforeCanvasRebuild += () =>
            {
                var s = new Vector2(ScalableBufferManager.widthScaleFactor, ScalableBufferManager.heightScaleFactor);
                s.x = Mathf.Clamp(Mathf.CeilToInt(s.x / 0.05f) * 0.05f, 0.25f, 1.0f);
                s.y = Mathf.Clamp(Mathf.CeilToInt(s.y / 0.05f) * 0.05f, 0.25f, 1.0f);

#if URP_ENABLE
                if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urpAsset
                    && Mathf.Abs(urpAsset.renderScale - 1) <= 0.05f)
                {
                    s = Vector2.one;
                }
#endif

                if (s_CurrentDynamicResolutionScale != s)
                {
                    s_CurrentDynamicResolutionScale = s;
                    Shader.SetGlobalVector(s_DynamicResolutionScale, s_CurrentDynamicResolutionScale);
                }
            };

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
        }

#if TMP_ENABLE
        private static void UpdateSubMeshUI(TextMeshProUGUI text, bool show, float aa, MinMax01 softness)
        {
            var subMeshes = UISoftMaskInternal.ListPool<TMP_SubMeshUI>.Rent();
            text.GetComponentsInChildren(subMeshes, 1);

            for (var i = 0; i < subMeshes.Count; i++)
            {
                var maskingShape = subMeshes[i].GetOrAddComponent<MaskingShape>();
                maskingShape.hideFlags = UISoftMaskProjectSettings.hideFlagsForTemp;
                maskingShape.antiAliasingThreshold = aa;
                maskingShape.softnessRange = softness;
                maskingShape.showMaskGraphic = show;
            }

            UISoftMaskInternal.ListPool<TMP_SubMeshUI>.Return(ref subMeshes);
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

#if UNITY_EDITOR
            UISoftMaskProjectSettings.shaderRegistry.RegisterVariant(mat, "UI > Soft Mask");
#endif
            return mat;
        }

        public static Material CreateSoftMaskable(
            Material baseMat,
            Texture softMaskBuffer,
            int softMaskDepth,
            int stencilBits,
            bool isStereo)
        {
            Profiler.BeginSample("(SM4UI)[SoftMaskableMaterial] Create > Create New Material");
            var mat = new Material(baseMat)
            {
                shader = UISoftMaskProjectSettings.shaderRegistry.FindOptionalShader(baseMat.shader,
                    "(SoftMaskable)",
                    "Hidden/{0} (SoftMaskable)",
                    "Hidden/UI/Default (SoftMaskable)"),
                hideFlags = HideFlags.HideAndDontSave
            };
            Profiler.EndSample();

            Profiler.BeginSample("(SM4UI)[SoftMaskableMaterial] Create > Set Properties");
            mat.SetTexture(s_SoftMaskTex, softMaskBuffer);
            mat.SetInt(s_SoftMaskableStereo, isStereo ? 1 : 0);
            mat.SetVector(s_SoftMaskColor, new Vector4(
                0 <= softMaskDepth ? 1 : 0,
                1 <= softMaskDepth ? 1 : 0,
                2 <= softMaskDepth ? 1 : 0,
                3 <= softMaskDepth ? 1 : 0
            ));
            mat.EnableKeyword("SOFTMASKABLE");
            Profiler.EndSample();

#if UNITY_EDITOR
            UISoftMaskProjectSettings.shaderRegistry.RegisterVariant(mat, "UI > Soft Mask");
            mat.EnableKeyword("SOFTMASK_EDITOR");
            mat.SetVector(s_SoftMaskOutsideColor,
                UISoftMaskProjectSettings.useStencilOutsideScreen ? Vector4.one : Vector4.zero);
#endif
            return mat;
        }
    }
}
