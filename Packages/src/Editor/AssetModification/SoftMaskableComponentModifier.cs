using Coffee.UISoftMaskInternal.AssetModification;
using UnityEditor;
using UnityEngine;

namespace Coffee.UISoftMask
{
    internal class SoftMaskableComponentModifier : ComponentModifier<SoftMaskable>
    {
        protected override bool ModifyComponent(SoftMaskable c, bool dryRun)
        {
            // Skip if the component is hidden.
            if ((c.hideFlags & HideFlags.DontSave) != 0) return false;

            // Skip if the component is ignored.
            if (c.ignoreSelf || c.ignoreChildren) return false;

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
