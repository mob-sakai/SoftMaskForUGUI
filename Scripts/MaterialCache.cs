using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UISoftMask
{
    internal class MaterialCache
    {
        public delegate void ModifyAction(Material material, Graphic graphic);

        static Dictionary<Hash128, MaterialEntry> materialMap = new Dictionary<Hash128, MaterialEntry>();

        private class MaterialEntry
        {
            public Material material;
            public int referenceCount;

            public void Release()
            {
                if (material)
                {
                    UnityEngine.Object.DestroyImmediate(material, false);
                }

                material = null;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void ClearCache()
        {
            foreach (var entry in materialMap.Values)
            {
                entry.Release();
            }

            materialMap.Clear();
        }
#endif

        public static Material Register(Material material, Hash128 hash, Action<Material> onModify)
        {
            if (!hash.isValid) return null;

            MaterialEntry entry;
            if (!materialMap.TryGetValue(hash, out entry))
            {
                entry = new MaterialEntry()
                {
                    material = new Material(material)
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                    },
                };

                onModify(entry.material);
                materialMap.Add(hash, entry);
            }

            entry.referenceCount++;
            //Debug.LogFormat("Register: {0}, {1} (Total: {2})", hash, entry.referenceCount, materialMap.Count);
            return entry.material;
        }

        public static void Unregister(Hash128 hash)
        {
            MaterialEntry entry;
            if (!hash.isValid || !materialMap.TryGetValue(hash, out entry)) return;
            //Debug.LogFormat("Unregister: {0}, {1}", hash, entry.referenceCount -1);

            if (--entry.referenceCount > 0) return;

            entry.Release();
            materialMap.Remove(hash);
            //Debug.LogFormat("Unregister: Release Emtry: {0}, {1} (Total: {2})", hash, entry.referenceCount, materialMap.Count);
        }
    }
}
