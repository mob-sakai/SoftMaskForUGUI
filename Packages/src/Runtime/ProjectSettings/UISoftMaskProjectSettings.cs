#pragma warning disable CS0414
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
        [Tooltip(
            "In the Scene view, objects outside the screen are displayed as stencil masks, allowing for more intuitive editing.")]
        [SerializeField]
        private bool m_UseStencilOutsideScreen = true;

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

        public static bool useStencilOutsideScreen =>
#if UNITY_EDITOR
            instance.m_UseStencilOutsideScreen;
#else
            false;
#endif

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
            ? HideFlags.DontSave | HideFlags.NotEditable | HideFlags.HideInHierarchy | HideFlags.HideInInspector
            : HideFlags.DontSave | HideFlags.NotEditable;

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
            var softMasks = ListPool<SoftMask>.Rent();
            var components = ListPool<IMaskable>.Rent();

            foreach (var softMask in Misc.FindObjectsOfType<SoftMask>())
            {
                // #208: Accessing game object transform hierarchy before loading of scene has completed.
                if (!softMask.gameObject.scene.isLoaded) continue;

                softMask.GetComponentsInParent(true, softMasks);
                if (1 < softMasks.Count) continue;

                softMask.GetComponentsInChildren(true, components);
                components.ForEach(c => c.RecalculateMasking());
            }

            ListPool<IMaskable>.Return(ref components);
            ListPool<SoftMask>.Return(ref softMasks);
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
#endif
    }
}
