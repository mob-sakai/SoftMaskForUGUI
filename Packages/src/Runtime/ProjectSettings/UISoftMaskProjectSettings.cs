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
    [Icon("Packages/com.coffee.softmask-for-ugui/Icons/SoftMaskIcon.png")]
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

        protected override void OnEnable()
        {
            base.OnEnable();
#if UNITY_EDITOR
            SetupSamplesForShaderVariantRegistry();
            m_ShaderVariantRegistry.ClearCache();
#endif
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
        protected override void OnCreateAsset()
        {
            m_ShaderVariantRegistry.InitializeIfNeeded(this, "(SoftMaskable)");
        }

        protected override void OnInitialize()
        {
            m_ShaderVariantRegistry.InitializeIfNeeded(this, "(SoftMaskable)");
        }

        private void OnValidate()
        {
            ResetAllSoftMasks();
            ResetAllHideFlags<MaskingShapeContainer>(hideFlagsForTemp);
            ResetAllHideFlags<TerminalMaskingShape>(hideFlagsForTemp);
        }

        private void SetupSamplesForShaderVariantRegistry()
        {
#if UNITY_2023_2_OR_NEWER
            const string tmpSupport = "TextMeshPro Support (Unity 6)";
#else
            const string tmpSupport = "TextMeshPro Support";
#endif
            m_ShaderVariantRegistry.RegisterSamples(new[]
            {
                // TextMeshPro Support
                ("Hidden/TextMeshPro/Bitmap (SoftMaskable)", tmpSupport),
                ("Hidden/TextMeshPro/Mobile/Bitmap (SoftMaskable)", tmpSupport),
                ("Hidden/TextMeshPro/Distance Field (SoftMaskable)", tmpSupport),
                ("Hidden/TextMeshPro/Mobile/Distance Field (SoftMaskable)", tmpSupport),
                // Spine Support
                ("Hidden/Spine/SkeletonGraphic (SoftMaskable)", "Spine Support"),
                ("Hidden/Spine/SkeletonGraphic Fill (SoftMaskable)", "Spine Support"),
                ("Hidden/Spine/SkeletonGraphic Grayscale (SoftMaskable)", "Spine Support"),
                ("Hidden/Spine/SkeletonGraphic Multiply (SoftMaskable)", "Spine Support"),
                ("Hidden/Spine/SkeletonGraphic Screen (SoftMaskable)", "Spine Support")
            });
        }

        private void Refresh()
        {
            m_ShaderVariantRegistry.ClearCache();
            MaterialRepository.Clear();
            foreach (var c in Misc.FindObjectsOfType<SoftMaskable>()
                         .Concat(Misc.GetAllComponentsInPrefabStage<SoftMaskable>()))
            {
                c.SetMaterialDirty();
            }

            EditorApplication.QueuePlayerLoopUpdate();
        }

        private void Reset()
        {
            m_ShaderVariantRegistry.InitializeIfNeeded(this, "(SoftMaskable)");
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
            private static void OnPostprocessAllAssets(string[] _, string[] __, string[] ___, string[] ____)
            {
                if (Misc.isBatchOrBuilding) return;

                instance.Refresh();
            }
        }
#endif
    }
}
