using UnityEditor;

namespace Coffee.UISoftMask
{
    [CustomEditor(typeof(AlphaHitTestTarget), true)]
    [CanEditMultipleObjects]
    public class AlphaHitTestTargetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var current = target as AlphaHitTestTarget;
            if (!current) return;

            Utils.DrawAlphaHitTestWarning(current.graphic);
        }
    }
}
