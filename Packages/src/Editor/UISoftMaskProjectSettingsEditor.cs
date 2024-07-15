using Coffee.UISoftMaskInternal;
using UnityEditor;
using UnityEngine;

namespace Coffee.UISoftMask
{
    [CustomEditor(typeof(UISoftMaskProjectSettings))]
    internal class UISoftMaskProjectSettingsEditor : Editor
    {
        private static readonly GUIContent s_ContentRemove = new GUIContent("-");
        private static readonly GUIContent s_ContentReset = new GUIContent("Reset");
        private static readonly GUIContent s_ContentIncluded = new GUIContent("Included Shaders");
        private static readonly GUIContent s_ContentUpgrade = new GUIContent("Upgrade");
        private static readonly GUIContent s_ContentUpgradeButton = new GUIContent("Upgrade All Assets V1 to V2");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Draw SoftMask/SoftMaskable/TerminalShape Shaders;
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(s_ContentIncluded);
                if (GUILayout.Button(s_ContentReset, EditorStyles.miniButton, GUILayout.Width(80)))
                {
                    UISoftMaskProjectSettings.instance.ReloadShaders(true);
                }
            }
            EditorGUILayout.EndHorizontal();

            foreach (var shader in AlwaysIncludedShadersProxy.GetShaders())
            {
                if (!UISoftMaskProjectSettings.CanIncludeShader(shader)) continue;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(shader, typeof(Shader), false);
                if (GUILayout.Button(s_ContentRemove, EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    AlwaysIncludedShadersProxy.Remove(shader);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(s_ContentUpgrade, EditorStyles.boldLabel);
            if (GUILayout.Button(s_ContentUpgradeButton))
            {
                EditorApplication.delayCall += () => new UISoftMaskModifierRunner().RunIfUserWantsTo();
            }
        }
    }
}
