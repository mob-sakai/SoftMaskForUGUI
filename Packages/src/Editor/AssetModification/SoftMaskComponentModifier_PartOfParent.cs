using Coffee.UISoftMask.Internal.AssetModification;
using UnityEditor;
using UnityEngine;

#pragma warning disable CS0612 // Type or member is obsolete

namespace Coffee.UISoftMask
{
    internal class SoftMaskComponentModifier_PartOfParent : ComponentModifier<SoftMask>
    {
        protected override bool ModifyComponent(SoftMask softMask, bool dryRun)
        {
            if (!softMask.partOfParent) return false;

            if (!dryRun)
            {
                var go = softMask.gameObject;
                Object.DestroyImmediate(softMask);

                if (!go.TryGetComponent<MaskingShape>(out _))
                {
                    go.AddComponent<MaskingShape>();
                }

                EditorUtility.SetDirty(go);
            }

            return true;
        }

        public override string Report()
        {
            return "  -> SoftMask.partOfParent is obsolete. Use MaskingShape component instead.\n";
        }
    }
}
