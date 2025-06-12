#pragma warning disable CS0414
using System.Linq;
using Coffee.UISoftMaskInternal;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_MODULE_VR
using UnityEngine.XR;
#endif

namespace Coffee.UISoftMask
{
    public class UISoftMaskProjectSettings : PreloadedProjectSettings<UISoftMaskProjectSettings>
    {
        private static bool s_UseStereoMock;

        [Header("Setting")]
        [Tooltip("Enable SoftMask globally.")]
        [SerializeField]
        internal bool m_SoftMaskEnabled = true;

        [Tooltip("Enable stereo rendering for VR devices.")]
        [SerializeField]
        private bool m_StereoEnabled = true;

        [Tooltip("Sensitivity of transform that automatically rebuilds the soft mask buffer.")]
        [SerializeField]
        private TransformSensitivity m_TransformSensitivity = TransformSensitivity.Medium;

        [Header("Editor")]
        [Tooltip("Hide the automatically generated components.\n" +
                 "  - MaskingShapeContainer\n" +
                 "  - TerminalMaskingShape")]
        [SerializeField]
        private bool m_HideGeneratedComponents = true;

        [HideInInspector]
        [SerializeField]
        private ShaderVariantRegistry m_ShaderVariantRegistry = new ShaderVariantRegistry();

        public static ShaderVariantRegistry shaderRegistry => instance.m_ShaderVariantRegistry;

        public static ShaderVariantCollection shaderVariantCollection => shaderRegistry.shaderVariantCollection;

        public static bool softMaskEnabled => instance.m_SoftMaskEnabled;

        public static bool useStencilOutsideScreen => true;

#if UNITY_MODULE_VR
        public static bool stereoEnabled =>
#if UNITY_EDITOR
            Application.isPlaying &&
#endif
            softMaskEnabled && instance.m_StereoEnabled && XRSettings.enabled;
#else
        public static bool stereoEnabled => false;
#endif

        public static HideFlags hideFlagsForTemp => instance.m_HideGeneratedComponents
            ? HideFlags.DontSaveInEditor | HideFlags.NotEditable | HideFlags.HideInHierarchy | HideFlags.HideInInspector
            : HideFlags.DontSaveInEditor | HideFlags.NotEditable;

        public static TransformSensitivity transformSensitivity
        {
            get => instance.m_TransformSensitivity;
            set => instance.m_TransformSensitivity = value;
        }

        public static bool useStereoMock
        {
            get => s_UseStereoMock;
            set
            {
                if (s_UseStereoMock == value) return;
                s_UseStereoMock = value;
                ResetAllSoftMasks();
            }
        }

        private static void ResetAllSoftMasks()
        {
            var softMasks = InternalListPool<SoftMask>.Rent();
            var components = InternalListPool<IMaskable>.Rent();

            foreach (var softMask in Misc.FindObjectsOfType<SoftMask>())
            {
                // #208: Accessing game object transform hierarchy before loading of scene has completed.
                if (!softMask.gameObject.scene.isLoaded) continue;

                softMask.GetComponentsInParent(true, softMasks);
                if (1 < softMasks.Count) continue;

                softMask.GetComponentsInChildren(true, components);
                components.ForEach(c => c.RecalculateMasking());
            }

            InternalListPool<IMaskable>.Return(ref components);
            InternalListPool<SoftMask>.Return(ref softMasks);
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
#if !LEGACY_TMP_ENABLE
            const string tmpSupport = "TextMeshPro Support (Unity 6)";
            const string version = "v3.3.0 (Unity 6)";
#else
            const string tmpSupport = "TextMeshPro Support";
            const string version = "v3.3.0";
#endif
            ShaderSampleImporter.RegisterShaderSamples(new[]
            {
                // TextMeshPro Support/TextMeshPro Support (Unity 6)
                ("Hidden/TextMeshPro/Distance Field SSD (SoftMaskable)", tmpSupport, version),
                ("Hidden/TextMeshPro/Mobile/Distance Field SSD (SoftMaskable)", tmpSupport, version),
                ("Hidden/TextMeshPro/Distance Field Overlay (SoftMaskable)", tmpSupport, version),
                ("Hidden/TextMeshPro/Mobile/Distance Field Overlay (SoftMaskable)", tmpSupport, version),
                ("Hidden/TextMeshPro/Bitmap (SoftMaskable)", tmpSupport, version),
                ("Hidden/TextMeshPro/Mobile/Bitmap (SoftMaskable)", tmpSupport, version),
                ("Hidden/TextMeshPro/Distance Field (SoftMaskable)", tmpSupport, version),
                ("Hidden/TextMeshPro/Mobile/Distance Field (SoftMaskable)", tmpSupport, version),
                // Spine Support
                ("Hidden/Spine/SkeletonGraphic (SoftMaskable)", "Spine Support", "v3.3.0"),
                ("Hidden/Spine/SkeletonGraphic Fill (SoftMaskable)", "Spine Support", "v3.3.0"),
                ("Hidden/Spine/SkeletonGraphic Grayscale (SoftMaskable)", "Spine Support", "v3.3.0"),
                ("Hidden/Spine/SkeletonGraphic Multiply (SoftMaskable)", "Spine Support", "v3.3.0"),
                ("Hidden/Spine/SkeletonGraphic Screen (SoftMaskable)", "Spine Support", "v3.3.0")
            });
            ShaderSampleImporter.RegisterDeprecatedShaders(new[]
            {
                ("e65241fa80a374114b3f55ed746c04d9", "Hidden-TMP_SDF-Mobile-SoftMaskable-Unity6.shader"),
                ("935b7be1c88464d2eb87204fdfab5a38", "Hidden-TMP_SDF-SoftMaskable-Unity6.shader")
            });
            EditorApplication.update += ShaderSampleImporter.Update;

            CgincPathSync.RegisterShaders("/(TMPro|TMPro_Properties).cginc", new[]
            {
                "Hidden/TextMeshPro/Distance Field (SoftMaskable)",
                "Hidden/TextMeshPro/Mobile/Distance Field (SoftMaskable)",
                "Hidden/TextMeshPro/Distance Field SSD (SoftMaskable)",
                "Hidden/TextMeshPro/Mobile/Distance Field SSD (SoftMaskable)",
                "Hidden/TextMeshPro/Distance Field Overlay (SoftMaskable)",
                "Hidden/TextMeshPro/Mobile/Distance Field Overlay (SoftMaskable)",
                "Hidden/TextMeshPro/Bitmap (SoftMaskable)",
                "Hidden/TextMeshPro/Mobile/Bitmap (SoftMaskable)",
                "Hidden/TextMeshPro/Distance Field (SoftMaskable)",
                "Hidden/TextMeshPro/Mobile/Distance Field (SoftMaskable)"
            });
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_ShaderVariantRegistry.onShaderRequested = ShaderSampleImporter.ImportShaderIfSelected;
            m_ShaderVariantRegistry.ClearCache();
        }

        protected override void OnCreateAsset()
        {
            m_ShaderVariantRegistry.InitializeIfNeeded(this);
            m_ShaderVariantRegistry.RegisterOptionalShaders(this);
        }

        protected override void OnInitialize()
        {
            m_ShaderVariantRegistry.InitializeIfNeeded(this);
        }

        private void Reset()
        {
            m_ShaderVariantRegistry.InitializeIfNeeded(this);
            m_ShaderVariantRegistry.RegisterOptionalShaders(this);
        }

        private void OnValidate()
        {
            ResetAllSoftMasks();
            ResetAllHideFlags<MaskingShapeContainer>(hideFlagsForTemp);
            ResetAllHideFlags<TerminalMaskingShape>(hideFlagsForTemp);
        }

        private static void ResetAllHideFlags<T>(HideFlags flags) where T : Component
        {
            foreach (var component in Misc.FindObjectsOfType<T>())
            {
                component.hideFlags = flags;
                EditorUtility.SetDirty(component);
            }
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return new PreloadedProjectSettingsProvider("Project/UI/Soft Mask");
        }

        private class Postprocessor : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] ___, string[] ____)
            {
                if (Misc.isBatchOrBuilding) return;

                // Register optional shaders when shaders are imported.
                if (imported.Concat(deleted).Any(path => path.EndsWith(".shader")))
                {
                    MaterialRepository.Clear();

                    // Refresh
                    if (hasInstance)
                    {
                        shaderRegistry.ClearCache();
                        shaderRegistry.RegisterOptionalShaders(instance);
                    }

                    // Refresh all UIEffect instances.
                    foreach (var c in Misc.FindObjectsOfType<SoftMaskable>()
                                 .Concat(Misc.GetAllComponentsInPrefabStage<SoftMaskable>()))
                    {
                        c.ReleaseMaterial();
                        c.SetMaterialDirty();
                    }
                }
            }
        }
#endif
    }
}
