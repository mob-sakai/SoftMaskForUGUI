using System;
using System.Collections.Generic;
using Coffee.UISoftMask.Internal.AssetModification;

namespace Coffee.UISoftMask
{
    internal class UISoftMaskModifierRunner : Runner
    {
        public UISoftMaskModifierRunner()
            : base("SoftMask For UGUI", new List<(string, Func<string, Modifier>)>
            {
                (".unity", x => new SceneModifier
                {
                    path = x,
                    componentModifiers = new IComponentModifier[]
                    {
                        new SoftMaskComponentModifier_Softness(),
                        new SoftMaskComponentModifier_PartOfParent(),
                        new SoftMaskableComponentModifier()
                    }
                }),
                (".prefab", x => new PrefabModifier
                {
                    path = x,
                    componentModifiers = new IComponentModifier[]
                    {
                        new SoftMaskComponentModifier_Softness(),
                        new SoftMaskComponentModifier_PartOfParent(),
                        new SoftMaskableComponentModifier()
                    }
                }),
                (".shader", SoftMaskableShaderModifier.Create)
            })
        {
        }
    }
}
