using UnityEditor;
using UnityEngine;

namespace Coffee.UISoftMask
{
    [CustomEditor(typeof(RectTransformFitter))]
    [CanEditMultipleObjects]
    public class RectTransformFitterEditor : Editor
    {
        private static readonly GUIContent s_TargetPropertiesContent = new GUIContent("Target Properties");
        private SerializedProperty _target;
        private SerializedProperty _targetProperties;

        protected void OnEnable()
        {
            _target = serializedObject.FindProperty("m_Target");
            _targetProperties = serializedObject.FindProperty("m_TargetProperties");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Target
            EditorGUILayout.PropertyField(_target);

            // TargetProperties
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = _targetProperties.hasMultipleDifferentValues;
                var value = (RectTransformFitter.RectTransformProperties)_targetProperties.intValue;
                value = (RectTransformFitter.RectTransformProperties)EditorGUILayout.EnumFlagsField(
                    s_TargetPropertiesContent, value);
                EditorGUI.showMixedValue = false;

                if (EditorGUI.EndChangeCheck())
                {
                    _targetProperties.intValue = (int)value;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
