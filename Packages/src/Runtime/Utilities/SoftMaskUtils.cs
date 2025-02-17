using Coffee.UISoftMaskInternal;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UI;
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
        public static readonly InternalObjectPool<CommandBuffer> commandBufferPool =
            new InternalObjectPool<CommandBuffer>(
                () => new CommandBuffer(),
                x => x != null,
                x => x.Clear());

        /// <summary>
        /// Object pool for MaterialPropertyBlock instances.
        /// </summary>
        public static readonly InternalObjectPool<MaterialPropertyBlock> materialPropertyBlockPool =
            new InternalObjectPool<MaterialPropertyBlock>(
                () => new MaterialPropertyBlock(),
                x => x != null,
                x => x.Clear());

        private static Material s_SoftMaskingMaterialAdd;
        private static Material s_SoftMaskingMaterialSub;
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

#if UNITY_EDITOR
            if (!Misc.isBatchOrBuilding)
            {
                // Enable the 'SOFTMASK_EDITOR' keyword only when drawing in the scene view camera.
                RenderPipelineManager.beginCameraRendering += (_, c) => EnableSoftMaskEditor(true, c);
                RenderPipelineManager.endCameraRendering += (_, c) => EnableSoftMaskEditor(false, c);
                Camera.onPreRender += c => EnableSoftMaskEditor(true, c);
                Camera.onPostRender += c => EnableSoftMaskEditor(false, c);

                void EnableSoftMaskEditor(bool begin, Camera cam)
                {
                    if (cam.cameraType == CameraType.SceneView)
                    {
                        if (begin)
                        {
                            Shader.EnableKeyword("SOFTMASK_EDITOR");
                        }
                        else
                        {
                            Shader.DisableKeyword("SOFTMASK_EDITOR");
                        }
                    }
                }
            }
#endif

#if TMP_ENABLE
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(UpdateMeshUI);
#endif
        }

        public static void UpdateMeshUI(Object obj)
        {
#if TMP_ENABLE
            if (!(obj is TextMeshProUGUI text)) return;

            if (text.TryGetComponent<SoftMask>(out var sm))
            {
#pragma warning disable CS0618
                (sm as IMeshModifier).ModifyMesh(text.mesh);
#pragma warning restore CS0618
                UpdateSubMeshUI(text, sm.enabled, sm.showMaskGraphic, sm.antiAliasingThreshold, sm.softnessRange,
                    MaskingShape.MaskingMethod.Additive);
            }
            else if (text.TryGetComponent<MaskingShape>(out var ms))
            {
#pragma warning disable CS0618
                (ms as IMeshModifier).ModifyMesh(text.mesh);
#pragma warning restore CS0618
                UpdateSubMeshUI(text, ms.enabled, ms.showMaskGraphic, ms.antiAliasingThreshold, ms.softnessRange,
                    ms.maskingMethod);
            }
        }

        private static void UpdateSubMeshUI(TextMeshProUGUI text, bool enabled, bool show, float aa, MinMax01 softness,
            MaskingShape.MaskingMethod method)
        {
            var subMeshes = InternalListPool<TMP_SubMeshUI>.Rent();
            text.GetComponentsInChildren(subMeshes, 1);

            for (var i = 0; i < subMeshes.Count; i++)
            {
                var maskingShape = subMeshes[i].GetOrAddComponent<MaskingShape>();
                maskingShape.hideFlags = HideFlags.NotEditable;
                maskingShape.enabled = enabled;
                maskingShape.maskingMethod = method;
                maskingShape.antiAliasingThreshold = aa;
                maskingShape.softnessRange = softness;
                maskingShape.showMaskGraphic = show;
#pragma warning disable CS0618
                (maskingShape as IMeshModifier).ModifyMesh(subMeshes[i].mesh);
#pragma warning restore CS0618
            }

            InternalListPool<TMP_SubMeshUI>.Return(ref subMeshes);
        }
#else
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
    }
}
