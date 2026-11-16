#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Coffee.UISoftMaskInternal
{
    internal static class AlwaysIncludedShadersProxy
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
                if (sp.GetArrayElementAtIndex(i).objectReferenceValue is Shader shader && shader != null)
                {
                    yield return shader;
                }
            }
        }

        public static void Remove(Shader shader)
        {
            if (shader == null) return;

            var sp = GetSerializedProperty();
            if (sp == null) return;

            for (var i = 0; i < sp.arraySize; i++)
            {
                if (sp.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                {
                    sp.DeleteArrayElementAtIndex(i);
                }
            }

            ShrinkArray(sp);
            sp.serializedObject.ApplyModifiedProperties();
        }

        public static void Add(Shader shader)
        {
            if (shader == null) return;

            var sp = GetSerializedProperty();
            if (sp == null) return;

            for (var i = 0; i < sp.arraySize; i++)
            {
                if (sp.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                {
                    return;
                }
            }

            var index = sp.arraySize;
            sp.InsertArrayElementAtIndex(index);
            sp.GetArrayElementAtIndex(index).objectReferenceValue = shader;
            ShrinkArray(sp);
            sp.serializedObject.ApplyModifiedProperties();
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
#endif
