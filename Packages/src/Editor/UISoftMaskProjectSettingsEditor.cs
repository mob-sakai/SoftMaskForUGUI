using Coffee.UISoftMaskInternal;
using UnityEditor;
using UnityEngine;

namespace Coffee.UISoftMask
{
    [CustomEditor(typeof(UISoftMaskProjectSettings))]
    internal class UISoftMaskProjectSettingsEditor : Editor
    {
        private ShaderVariantRegistryEditor _shaderVariantRegistryEditor;

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 180;
            base.OnInspectorGUI();

            // Shader registry.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shader", EditorStyles.boldLabel);
            if (_shaderVariantRegistryEditor == null)
            {
                var property = serializedObject.FindProperty("m_ShaderVariantRegistry");
                _shaderVariantRegistryEditor = new ShaderVariantRegistryEditor(property, "(SoftMaskable)");
            }

            _shaderVariantRegistryEditor.Draw();
            serializedObject.ApplyModifiedProperties();

            // Upgrade All Assets For V3.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Upgrade", EditorStyles.boldLabel);
            if (GUILayout.Button("Upgrade All Assets For V3"))
            {
                EditorApplication.delayCall += () => new UISoftMaskModifierRunner().RunIfUserWantsTo();
            }

            GUILayout.FlexibleSpace();
        }
    }
}
