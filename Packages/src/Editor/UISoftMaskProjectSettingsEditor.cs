using Coffee.UISoftMaskInternal;
using UnityEditor;
using UnityEngine;

namespace Coffee.UISoftMask
{
    [CustomEditor(typeof(UISoftMaskProjectSettings))]
    internal class UISoftMaskProjectSettingsEditor : Editor
    {
        private static readonly GUIContent s_ContentUpgrade = new GUIContent("Upgrade");
        private static readonly GUIContent s_ContentUpgradeButton = new GUIContent("Upgrade All Assets For V2");

        public override void OnInspectorGUI()
        {
            var prevEnabled = UISoftMaskProjectSettings.softMaskEnabled;
            base.OnInspectorGUI();
            if (prevEnabled != UISoftMaskProjectSettings.softMaskEnabled)
            {
                UISoftMaskProjectSettings.ResetAllSoftMasks();
            }

            // Draw SoftMask/SoftMaskable/TerminalShape Shaders;
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Included Shaders");
                if (GUILayout.Button("Reset", EditorStyles.miniButton, GUILayout.Width(80)))
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
                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20)))
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
