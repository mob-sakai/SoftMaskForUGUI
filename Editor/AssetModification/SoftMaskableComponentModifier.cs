using Coffee.UISoftMaskInternal.AssetModification;
using UnityEditor;
using UnityEngine;

namespace Coffee.UISoftMask
{
    internal class SoftMaskableComponentModifier : ComponentModifier<SoftMaskable>
    {
        protected override bool ModifyComponent(SoftMaskable c, bool dryRun)
        {
            if (c.hideFlags.HasFlag(HideFlags.DontSave | HideFlags.NotEditable)) return false;

            if (!dryRun)
            {
                var go = c.gameObject;
                Object.DestroyImmediate(c);
                EditorUtility.SetDirty(go);
            }

            return true;
        }

        public override string Report()
        {
            return "  -> SoftMaskable component is now auto-generated object. Remove them.\n";
        }
    }
}
