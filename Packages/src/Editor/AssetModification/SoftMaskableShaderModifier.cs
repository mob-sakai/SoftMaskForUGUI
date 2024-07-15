using Coffee.UISoftMaskInternal.AssetModification;
using UnityEditor;
using UnityEngine;

namespace Coffee.UISoftMask
{
    internal class SoftMaskableShaderModifier : TextAssetModifier
    {
        protected override string id => "Shader";

        public static Modifier Create(string path)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (!shader || !shader.name.Contains(" (SoftMaskable)")) return null;

            return new SoftMaskableShaderModifier
            {
                path = path,
                textModifiers = new ITextModifier[]
                {
                    // color.a *= SoftMask(input.IN.vertex, IN.worldPosition);
                    // -> color.a *= SoftMask(input.IN.vertex, IN.worldPosition, color.a);
                    new TextReplaceModifier(
                        @"(([^. \t]+).*\*=\s*SoftMask\(.*[^.][^a])\);.*$",
                        $"$1, $2.a); // Add for soft mask")
                }
            };
        }
    }
}
