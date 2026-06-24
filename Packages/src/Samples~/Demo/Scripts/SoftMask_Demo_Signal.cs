using System;
using Coffee.UISoftMaskInternal;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace Coffee.UISoftMask.Demos
{
    [ExecuteAlways]
    public class SoftMask_Demo_Signal : RawImage
    {
        [SerializeField] private SoftMask m_SoftMask;

        private Action _checkDirty;
        public static int bakedCount { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!Application.isPlaying) return;

            UIExtraCallbacks.onBeforeCanvasRebuild += _checkDirty ?? (_checkDirty = CheckDirty);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UIExtraCallbacks.onBeforeCanvasRebuild -= _checkDirty ?? (_checkDirty = CheckDirty);
            canvasRenderer.SetAlpha(1);
        }


#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#elif UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void InitializeOnLoad()
        {
            bakedCount = 0;
            SoftMask.onRenderSoftMaskBuffer -= OnRenderSoftMaskBuffer;
            SoftMask.onRenderSoftMaskBuffer += OnRenderSoftMaskBuffer;
        }

        private static void OnRenderSoftMaskBuffer(SoftMask _)
        {
            bakedCount++;
        }

        private void CheckDirty()
        {
            if (!canvasRenderer || !m_SoftMask) return;

            canvasRenderer.SetAlpha(m_SoftMask.isDirty ? 1 : 0);
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(SoftMask_Demo_Signal))]
        private class SoftMask_Demo_SignalEditor : RawImageEditor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SoftMask"), true);

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
