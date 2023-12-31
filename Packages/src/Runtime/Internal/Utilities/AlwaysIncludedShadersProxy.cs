#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Coffee.UISoftMask.Internal
{
    internal static class AlwaysIncludedShadersProxy
    {
        private static GraphicsSettings s_GraphicsSettings;

        private static SerializedProperty GetSerializedProperty()
        {
            if (!s_GraphicsSettings)
            {
                s_GraphicsSettings = typeof(GraphicsSettings).GetMethod("GetGraphicsSettings",
                        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                    ?.Invoke(null, null) as GraphicsSettings;
            }

            return new SerializedObject(s_GraphicsSettings).FindProperty("m_AlwaysIncludedShaders");
        }

        public static IEnumerable<Shader> GetShaders()
        {
            var sp = GetSerializedProperty();
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
            var sp = GetSerializedProperty();
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
            var sp = GetSerializedProperty();
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
                if (!sp.GetArrayElementAtIndex(i).objectReferenceValue)
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
