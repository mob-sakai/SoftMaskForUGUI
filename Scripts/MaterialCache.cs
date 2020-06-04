using System.Collections.Generic;
using System;
using UnityEngine;

namespace Coffee.UISoftMask
{
    internal class MaterialEntry
    {
        public Material material;
        public int referenceCount;

        public void Release()
        {
            if (material)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEngine.Object.DestroyImmediate(material, false);
                else
#endif
                    UnityEngine.Object.Destroy(material);
            }

            material = null;
        }
    }

    internal static class MaterialCache
    {
        static readonly Dictionary<Hash128, MaterialEntry> s_MaterialMap = new Dictionary<Hash128, MaterialEntry>();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void ClearCache()
        {
            foreach (var entry in s_MaterialMap.Values)
            {
                entry.Release();
            }

            s_MaterialMap.Clear();
        }
#endif

        public static Material Register(Material material, Hash128 hash, Action<Material> onModify)
        {
            if (!hash.isValid) return null;

            MaterialEntry entry;
            if (!s_MaterialMap.TryGetValue(hash, out entry))
            {
                entry = new MaterialEntry()
                {
                    material = new Material(material)
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                    },
                };

                onModify(entry.material);
                s_MaterialMap.Add(hash, entry);
            }

            entry.referenceCount++;
            //Debug.LogFormat("Register: {0}, {1} (Total: {2})", hash, entry.referenceCount, materialMap.Count);
            return entry.material;
        }

        public static void Unregister(Hash128 hash)
        {
            MaterialEntry entry;
            if (!hash.isValid || !s_MaterialMap.TryGetValue(hash, out entry)) return;
            //Debug.LogFormat("Unregister: {0}, {1}", hash, entry.referenceCount -1);

            if (--entry.referenceCount > 0) return;

            entry.Release();
            s_MaterialMap.Remove(hash);
            //Debug.LogFormat("Unregister: Release Emtry: {0}, {1} (Total: {2})", hash, entry.referenceCount, materialMap.Count);
        }
    }
}
