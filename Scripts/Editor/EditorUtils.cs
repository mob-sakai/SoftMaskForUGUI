#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace Coffee.UISoftMask
{
    internal static class EditorUtils
    {
        internal static void MarkPrefabDirty()
        {
#if UNITY_2018_3_OR_NEWER
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage == null) return;
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
#endif
        }

        /// <summary>
        /// Verify whether it can be converted to the specified component.
        /// </summary>
        internal static bool CanConvertTo<T>(Object context) where T : MonoBehaviour
        {
            return context && context.GetType() != typeof(T);
        }

        /// <summary>
        /// Convert to the specified component.
        /// </summary>
        internal static void ConvertTo<T>(Object context) where T : MonoBehaviour
        {
            var target = context as MonoBehaviour;
            var so = new SerializedObject(target);
            so.Update();

            bool oldEnable = target.enabled;
            target.enabled = false;

            // Find MonoScript of the specified component.
            foreach (var script in Resources.FindObjectsOfTypeAll<MonoScript>())
            {
                if (script.GetClass() != typeof(T))
                    continue;

                // Set 'm_Script' to convert.
                so.FindProperty("m_Script").objectReferenceValue = script;
                so.ApplyModifiedProperties();
                break;
            }

            (so.targetObject as MonoBehaviour).enabled = oldEnable;
        }
    }
}
