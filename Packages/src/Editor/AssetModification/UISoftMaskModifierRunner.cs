using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Coffee.UISoftMaskInternal.AssetModification;
using UnityEditor;

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
                        new SoftMaskComponentModifier_Alpha(),
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
                        new SoftMaskComponentModifier_Alpha(),
                        new SoftMaskComponentModifier_PartOfParent(),
                        new SoftMaskableComponentModifier()
                    }
                }),
                (".shader", SoftMaskableShaderModifier.Create),
                (".shader", x => new AlwaysIncludedShadersModifier()
                {
                    path = x,
                    excludePattern =
                        new Regex(@"(Hidden/UI/SoftMask|Hidden/UI/TerminalMaskingShape|\(SoftMaskable\))")
                })
            })
        {
        }

        public override void Run(string[] assetPaths, bool dryRun)
        {
            if (!dryRun)
            {
                AssetDatabase.ImportAsset("Packages/com.coffee.softmask-for-ugui/Shaders",
                    ImportAssetOptions.ImportRecursive);
            }

            base.Run(assetPaths, dryRun);
        }
    }
}
