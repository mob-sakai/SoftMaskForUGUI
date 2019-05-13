using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;


namespace Coffee.UIExtensions.Editors
{
    public static class Utils
    {
        public static void MarkPrefabDirty ()
        {
            #if UNITY_2018_3_OR_NEWER
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage ();
                if (prefabStage != null) 
                {
                    EditorSceneManager.MarkSceneDirty (prefabStage.scene);
                }
            #endif
        }
    }
}