using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace Coffee.UISoftMask.Demos
{
    [ExecuteAlways]
    [RequireComponent(typeof(AspectRatioFitter))]
    public class SoftMask_Demo_VRSupport_BufferImage : RawImage
    {
        [SerializeField] private SoftMask[] m_SoftMasks;
        private AspectRatioFitter _aspectRatioFitter;

        private void LateUpdate()
        {
            foreach (var softMask in m_SoftMasks)
            {
                if (!softMask || !softMask.SoftMaskingEnabled() || !softMask.hasSoftMaskBuffer) continue;

                texture = softMask.softMaskBuffer;
                _aspectRatioFitter.aspectRatio = (float)Screen.width / Screen.height;
                return;
            }

            texture = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _aspectRatioFitter = GetComponent<AspectRatioFitter>();
            foreach (var softMask in m_SoftMasks)
            {
                softMask.clearColor = Color.black;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SoftMask_Demo_VRSupport_BufferImage))]
    internal class SoftMask_Demo_VRSupport_BufferImageEditor : RawImageEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SoftMasks"), true);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
