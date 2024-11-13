#pragma warning disable CS0414
using System.Linq;
using Coffee.UISoftMaskInternal;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_MODULE_VR
using UnityEngine.XR;
#endif
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

namespace Coffee.UISoftMask
{
    public class UISoftMaskProjectSettings : PreloadedProjectSettings<UISoftMaskProjectSettings>
    {
        public enum FallbackBehavior
        {
            DefaultSoftMaskable,
            None
        }

        private static bool s_UseStereoMock;

        [Header("Setting")]
        [Tooltip("Enable SoftMask globally.")]
        [SerializeField]
        internal bool m_SoftMaskEnabled = true;

        [Tooltip("Enable stereo rendering for VR devices.")]
        [SerializeField]
        private bool m_StereoEnabled = true;

        [Tooltip("Behavior when SoftMaskable shader is not found.")]
        [SerializeField]
        private FallbackBehavior m_FallbackBehavior;

        [Tooltip("Sensitivity of transform that automatically rebuilds the soft mask buffer.")]
        [SerializeField]
        private TransformSensitivity m_TransformSensitivity = TransformSensitivity.Medium;

        [Header("Editor")]
        [Tooltip(
            "In the Scene view, objects outside the screen are displayed as stencil masks, allowing for more intuitive editing.")]
        [SerializeField]
        private bool m_UseStencilOutsideScreen = true;

        [Tooltip("Hide the automatically generated components.\n" +
                 "  - SoftMaskable\n" +
                 "  - MaskingShapeContainer\n" +
                 "  - TerminalMaskingShape")]
        [SerializeField]
        private bool m_HideGeneratedComponents = true;

        [Header("Shader")]
        [Tooltip("Automatically include shaders required for SoftMask.")]
        [SerializeField]
        private bool m_AutoIncludeShaders = true;

        [Tooltip("Strip unused shader variants in the build.")]
        [SerializeField]
        internal bool m_StripShaderVariants = true;

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

        public static FallbackBehavior fallbackBehavior => instance.m_FallbackBehavior;

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

#if UNITY_2023_1_OR_NEWER
            foreach (var softMask in FindObjectsByType<SoftMask>(FindObjectsInactive.Include, FindObjectsSortMode.None))
#else
            foreach (var softMask in FindObjectsOfType<SoftMask>())
#endif
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
        private void OnValidate()
        {
            ResetAllSoftMasks();
            ResetAllHideFlags<MaskingShapeContainer>(hideFlagsForTemp);
            ResetAllHideFlags<TerminalMaskingShape>(hideFlagsForTemp);
        }

        private void Reset()
        {
            ReloadShaders(false);
        }

        private static void ResetAllHideFlags<T>(HideFlags flags) where T : Component
        {
#if UNITY_2023_1_OR_NEWER
            foreach (var component in FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
#else
            foreach (var component in FindObjectsOfType<T>())
#endif
            {
                Debug.Log($"{component}, {component.hideFlags}");
                component.hideFlags = flags;
                EditorUtility.SetDirty(component);
            }
        }

        internal void ReloadShaders(bool force)
        {
            if (!force && !m_AutoIncludeShaders) return;

            foreach (var shader in AssetDatabase.FindAssets("t:Shader")
                         .Select(AssetDatabase.GUIDToAssetPath)
                         .Select(AssetDatabase.LoadAssetAtPath<Shader>)
                         .Where(CanIncludeShader))
            {
                AlwaysIncludedShadersProxy.Add(shader);
            }
        }

        internal static bool CanIncludeShader(Shader shader)
        {
            if (!shader) return false;

            var name = shader.name;
            return SoftMaskUtils.IsSoftMaskableShaderName(name)
                   || name == "Hidden/UI/SoftMask"
                   || name == "Hidden/UI/TerminalMaskingShape";
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return new PreloadedProjectSettingsProvider("Project/UI/Soft Mask");
        }

        private class PreprocessBuildWithReport : IPreprocessBuildWithReport
        {
            int IOrderedCallback.callbackOrder => 0;

            void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
            {
                instance.ReloadShaders(false);
            }
        }
#endif
    }
}
