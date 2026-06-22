using Coffee.UISoftMaskInternal;
using UnityEditor;
using UnityEngine;

namespace Coffee.UISoftMask
{
    [CustomEditor(typeof(UISoftMaskProjectSettings))]
    internal class UISoftMaskProjectSettingsEditor : PreloadedProjectSettingsEditor
    {
        private ShaderVariantRegistryEditor _shaderVariantRegistryEditor;

        protected override void OnEnable()
        {
            base.OnEnable();
            _shaderVariantRegistryEditor = ShaderVariantRegistryEditor.Create(serializedObject, "(SoftMaskable)",
                () =>
                {
                    UISoftMaskProjectSettings.shaderRegistry
                        .RegisterOptionalShaders(UISoftMaskProjectSettings.instance);
                });
        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 180;
            base.OnInspectorGUI();

            // Shader registry.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shader", EditorStyles.boldLabel);
            _shaderVariantRegistryEditor.Draw();

            DrawPreLoadSettingsInBuild("SoftMask");

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
