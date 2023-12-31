using Coffee.UISoftMask.Internal.AssetModification;
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

            var cginc = AssetDatabase.GUIDToAssetPath("c0f7e0d8262ac42cc9ec30a5aea12d72");
            return new SoftMaskableShaderModifier
            {
                path = path,
                textModifiers = new ITextModifier[]
                {
                    new TextReplaceModifier(
                        @"#include.*/SoftMask.cginc.*$",
                        $"#include \"{cginc}\" // Add for soft mask"),
                    new TextReplaceModifier(
                        @"^(\s+)#pragma\s+shader_feature(_local)?\s+_+\s+SOFTMASK_EDITOR.*$",
                        @"$1#pragma multi_compile_local UI_SOFT_MASKABLE UI_SOFT_MASKABLE_EDITOR // Add for soft mask
$1#pragma multi_compile_local _ UI_SOFT_MASKABLE_STEREO // Add for soft mask (stereo)")
                }
            };
        }
    }
}
