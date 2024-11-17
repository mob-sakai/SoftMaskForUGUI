using System.Text;
using System.Text.RegularExpressions;
using Coffee.UISoftMaskInternal.AssetModification;
using UnityEditor;
using UnityEngine;

namespace Coffee.UISoftMask
{
    internal class SoftMaskableShaderModifier : TextAssetModifier
    {
        private class ShaderModifier : ITextModifier
        {
            public bool ModifyText(StringBuilder sb, string text)
            {
                // #include "Packages/com.coffee.softmask-for-ugui/Shaders/SoftMask.cginc" // Add for soft mask
                // #pragma shader_feature_local _ SOFTMASK_EDITOR // Add for soft mask
                // #pragma shader_feature_local _ SOFTMASKABLE // Add for soft mask
                if (text.Contains("/SoftMask.cginc\""))
                {
                    var space = Regex.Match(text, @"^\s*").Value;
                    sb.AppendLine(text);
                    sb.Append(space).AppendLine("#pragma shader_feature_local _ SOFTMASK_EDITOR // Add for soft mask");
                    sb.Append(space).AppendLine("#pragma shader_feature_local _ SOFTMASKABLE // Add for soft mask");
                    return true;
                }

                // Remove the following line:
                // #pragma shader_feature_local _ SOFTMASK_EDITOR // Add for soft mask
                // #pragma shader_feature_local _ SOFTMASKABLE // Add for soft mask
                if (Regex.IsMatch(text, @"#pragma.*\s*(SOFTMASK_EDITOR|SOFTMASKABLE)"))
                {
                    return true;
                }

                // color.a *= SoftMask(input.IN.vertex, IN.worldPosition);
                // -> color.a *= SoftMask(input.IN.vertex, IN.worldPosition, color.a);
                var match = Regex.Match(text, @"(\s*([^. \t]+).*\*=\s*SoftMask\(.*[^.][^a])\);.*$");
                if (match.Success)
                {
                    sb.Append(match.Groups[1].Value)
                        .Append(", ")
                        .Append(match.Groups[2].Value)
                        .AppendLine(".a); // Add for soft mask");
                    return true;
                }

                return false;
            }
        }

        protected override string id => "Shader";

        public static Modifier Create(string path)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (!shader || !shader.name.Contains("(SoftMaskable)")) return null;

            return new SoftMaskableShaderModifier
            {
                path = path,
                textModifiers = new ITextModifier[]
                {
                    new ShaderModifier()
                }
            };
        }

        protected override bool RunModify(bool dryRun)
        {
            var modified = base.RunModify(dryRun);
            if (!dryRun && modified)
            {
                AssetDatabase.ImportAsset(path);
            }

            return modified;
        }
    }
}
