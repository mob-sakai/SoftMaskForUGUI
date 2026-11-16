using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Coffee.UISoftMaskInternal.AssetModification
{
    internal class AlwaysIncludedShadersModifier : Modifier
    {
        public Regex includePattern;
        public Regex excludePattern;
        protected override string id => "AlwaysIncludedShaders";
        private bool _toExclude;
        private bool _toInclude;

        protected override string ModificationReport()
        {
            if (_toExclude)
            {
                return "-> excluded";
            }

            if (_toInclude)
            {
                return "-> included";
            }

            return string.Empty;
        }

        protected override bool RunModify(bool dryRun)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader == null) return false;

            var included = AlwaysIncludedShadersProxy.GetShaders()
                .Contains(shader);
            var shouldExclude = excludePattern?.IsMatch(shader.name) ?? false;
            var shouldInclude = includePattern?.IsMatch(shader.name) ?? false;

            if (shouldExclude && included)
            {
                _toExclude = true;
                if (!dryRun)
                {
                    AlwaysIncludedShadersProxy.Remove(shader);
                }

                return true;
            }

            if (shouldInclude && !included)
            {
                _toInclude = true;
                if (!dryRun)
                {
                    AlwaysIncludedShadersProxy.Add(shader);
                }

                return true;
            }

            return false;
        }

        private static class AlwaysIncludedShadersProxy
        {
            private static GraphicsSettings s_GraphicsSettings;

            private static SerializedProperty GetSerializedProperty()
            {
                if (s_GraphicsSettings == null)
                {
                    s_GraphicsSettings = typeof(GraphicsSettings).GetMethod("GetGraphicsSettings",
                            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                        ?.Invoke(null, null) as GraphicsSettings;
                }

                if (s_GraphicsSettings == null)
                {
                    Debug.LogError("Could not retrieve GraphicsSettings.");
                    return null;
                }

                var property = new SerializedObject(s_GraphicsSettings).FindProperty("m_AlwaysIncludedShaders");
                if (property == null) Debug.LogError("Could not retrieve the always-included shaders setting.");
                return property;
            }

            public static IEnumerable<Shader> GetShaders()
            {
                var sp = GetSerializedProperty();
                if (sp == null) yield break;

                for (var i = 0; i < sp.arraySize; i++)
                {
                    if (sp.GetArrayElementAtIndex(i).objectReferenceValue is Shader shader)
                    {
                        yield return shader;
                    }
                }
            }

            public static void Remove(Shader shader)
            {
                if (shader == null) return;

                var changed = false;
                var sp = GetSerializedProperty();
                if (sp == null) return;

                for (var i = 0; i < sp.arraySize; i++)
                {
                    // Found the shader to remove.
                    if (sp.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                    {
                        sp.DeleteArrayElementAtIndex(i);
                        changed = true;
                    }
                }

                if (changed)
                {
                    ShrinkArray(sp);
                    sp.serializedObject.ApplyModifiedProperties();
                }
            }

            public static void Add(Shader shader)
            {
                if (shader == null) return;

                var changed = true;
                var sp = GetSerializedProperty();
                if (sp == null) return;

                for (var i = 0; i < sp.arraySize; i++)
                {
                    // Already included.
                    if (sp.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                    {
                        changed = false;
                        break;
                    }
                }

                if (changed)
                {
                    var index = sp.arraySize;
                    sp.InsertArrayElementAtIndex(index);
                    sp.GetArrayElementAtIndex(index).objectReferenceValue = shader;
                    ShrinkArray(sp);
                    sp.serializedObject.ApplyModifiedProperties();
                }
            }

            private static void ShrinkArray(SerializedProperty sp)
            {
                var removed = 0;
                for (var i = 0; i < sp.arraySize; i++)
                {
                    if (sp.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    {
                        removed++;
                    }
                    else if (0 < removed)
                    {
                        sp.MoveArrayElement(i, i - removed);
                    }
                }

                sp.arraySize -= removed;
            }
        }
    }
}
