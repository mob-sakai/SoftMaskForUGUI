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
        public enum FallbackBehavior
        {
            DefaultSoftMaskable,
            None
        }

        public enum TransformSensitivity
        {
            Low,
            Medium,
            High
        }

        private static bool s_UseStereoMock;

        [Header("Setting")]
        [SerializeField]
        internal bool m_SoftMaskEnabled = true;

        [SerializeField]
        private bool m_StereoEnabled = true;

        [SerializeField]
        private FallbackBehavior m_FallbackBehavior;

        [SerializeField]
        private TransformSensitivity m_TransformSensitivity = TransformSensitivity.Medium;

        [Header("Editor")]
        [SerializeField]
        private bool m_EnabledInEditMode = true;

        [Header("Shader")]
        [SerializeField]
        private bool m_AutoIncludeShaders = true;

        [SerializeField]
        internal bool m_StripShaderVariants = true;

        public static bool softMaskEnabled
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying && !instance.m_EnabledInEditMode) return false;
#endif
                return instance.m_SoftMaskEnabled;
            }
        }

#if UNITY_MODULE_VR
        public static bool stereoEnabled => softMaskEnabled && instance.m_StereoEnabled && XRSettings.enabled;
#else
        public static bool stereoEnabled => false;
#endif

        public static FallbackBehavior fallbackBehavior => instance.m_FallbackBehavior;

        public static TransformSensitivity transformSensitivity
        {
            get => instance.m_TransformSensitivity;
            set => instance.m_TransformSensitivity = value;
        }

        public static float sensitivity
        {
            get
            {
                switch (instance.m_TransformSensitivity)
                {
                    case TransformSensitivity.Low: return 1f / (1 << 2);
                    case TransformSensitivity.Medium: return 1f / (1 << 5);
                    case TransformSensitivity.High: return 1f / (1 << 12);
                    default: return 1f / (1 << (int)instance.m_TransformSensitivity);
                }
            }
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

        internal static void ResetAllSoftMasks()
        {
            var softMasks = ListPool<SoftMask>.Rent();
            var components = ListPool<IMaskable>.Rent();
            foreach (var softMask in FindObjectsOfType<SoftMask>())
            {
                softMask.GetComponentsInParent(true, softMasks);
                if (1 < softMasks.Count) continue;

                softMask.GetComponentsInChildren(true, components);
                components.ForEach(c => c.RecalculateMasking());
            }

            ListPool<IMaskable>.Return(ref components);
            ListPool<SoftMask>.Return(ref softMasks);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            ReloadShaders(false);
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
            return name.EndsWith(" (SoftMaskable)")
                   || name == "Hidden/UI/SoftMask"
                   || name == "Hidden/UI/TerminalMaskingShape";
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return new PreloadedProjectSettingsProvider("Project/UI/Soft Mask");
        }
#endif
    }
}
