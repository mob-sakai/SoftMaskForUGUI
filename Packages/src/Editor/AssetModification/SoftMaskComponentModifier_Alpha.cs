using Coffee.UISoftMaskInternal.AssetModification;
using UnityEditor;
using UnityEngine;

#pragma warning disable CS0612, CS0618 // Type or member is obsolete

namespace Coffee.UISoftMask
{
    internal class SoftMaskComponentModifier_Alpha : ComponentModifier<SoftMask>
    {
        protected override bool ModifyComponent(SoftMask softMask, bool dryRun)
        {
            if (softMask.m_Alpha < 0 || 1 <= softMask.m_Alpha) return false;

            if (!dryRun)
            {
                softMask.alpha = softMask.m_Alpha;
                softMask.m_Alpha = -1;

                EditorUtility.SetDirty(softMask.gameObject);
            }

            return true;
        }

        public override string Report()
        {
            return "  -> SoftMask.alpha API has been changed. Use Graphic.color.a instead.";
        }
    }
}
