using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
#endif

namespace Coffee.UISoftMaskInternal
{
    /// <summary>
    /// Utility class for managing temporary render textures.
    /// </summary>
    internal static class RenderTextureRepository
    {
        private static readonly ObjectRepository<RenderTexture> s_Repository = new ObjectRepository<RenderTexture>();
#if UNITY_2021_3_OR_NEWER
        private static readonly GraphicsFormat s_StencilFormat = GraphicsFormatUtility.GetDepthStencilFormat(0, 8);
#endif

        public static int count => s_Repository.count;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Clear()
        {
            s_Repository.Clear();
        }
#endif

        /// <summary>
        /// Retrieves a cached RenderTexture based on the hash.
        /// </summary>
        public static bool Valid(Hash128 hash, RenderTexture rt)
        {
            Profiler.BeginSample("(COF)[RTRepository] Valid");
            var ret = s_Repository.Valid(hash, rt);
            Profiler.EndSample();
            return ret;
        }

        /// <summary>
        /// Adds or retrieves a cached RenderTexture based on the hash.
        /// </summary>
        public static void Get<T>(Hash128 hash, ref RenderTexture rt, Func<T, RenderTexture> onCreate, T source)
        {
            Profiler.BeginSample("(COF)[RTRepository] Get");
            s_Repository.Get(hash, ref rt, onCreate, source);
            Profiler.EndSample();
        }

        /// <summary>
        /// Adds or retrieves a cached RenderTexture based on the hash.
        /// </summary>
        public static RenderTextureDescriptor GetDescriptor(Vector2Int size, bool useStencil,
            RenderTextureFormat format = RenderTextureFormat.ARGB32)
        {
            Profiler.BeginSample("(COF)[RTRepository] GetDescriptor");
            var graphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(format, RenderTextureReadWrite.Default);
            var rtd = new RenderTextureDescriptor(
                Mathf.Max(8, size.x),
                Mathf.Max(8, size.y),
                graphicsFormat,
                useStencil ? 24 : 0)
            {
                sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear,
                mipCount = -1,
#if UNITY_2021_3_OR_NEWER
                depthStencilFormat = useStencil ? s_StencilFormat : GraphicsFormat.None
#endif
            };

            Profiler.EndSample();
            return rtd;
        }

        /// <summary>
        /// Releases the RenderTexture buffer.
        /// </summary>
        public static void Release(ref RenderTexture rt)
        {
            Profiler.BeginSample("(COF)[RTRepository] Release");
            s_Repository.Release(ref rt);
            Profiler.EndSample();
        }

        public static Vector2Int GetPreferSize(int displayIndex, Vector2Int size, int downSamplingRate)
        {
            var aspect = (float)size.x / size.y;
            var screenSize = GetScreenSize(displayIndex);

            // Clamp to screen size.
            size.x = Mathf.Clamp(size.x, 8, screenSize.x);
            size.y = Mathf.Clamp(size.y, 8, screenSize.y);

            if (downSamplingRate <= 0)
            {
                if (size.x < size.y)
                {
                    size.x = Mathf.CeilToInt(size.y * aspect);
                }
                else
                {
                    size.y = Mathf.CeilToInt(size.x / aspect);
                }

                return size;
            }

            if (size.x < size.y)
            {
                size.y = Mathf.NextPowerOfTwo(size.y / 2) / downSamplingRate;
                size.x = Mathf.CeilToInt(size.y * aspect);
            }
            else
            {
                size.x = Mathf.NextPowerOfTwo(size.x / 2) / downSamplingRate;
                size.y = Mathf.CeilToInt(size.x / aspect);
            }

            return size;
        }

        public static Vector2Int GetScreenSize(int displayIndex, int downSamplingRate)
        {
            return GetPreferSize(displayIndex, GetScreenSize(displayIndex), downSamplingRate);
        }

        public static Vector2Int GetScreenSize(int displayIndex)
        {
#if UNITY_EDITOR
            int activeIndex = Display.activeEditorGameViewTarget;
            int maxIndex = Mathf.Max(displayIndex, activeIndex);
            if (s_CachedEditorResolutions.Count <= maxIndex)
            {
                for(var i = s_CachedEditorResolutions.Count; i < maxIndex + 1; i++)
                {
                    if(i != activeIndex && TryFindGameViewResolution(i, out var resolution))
                    {
                        s_CachedEditorResolutions.Add(resolution);
                    }
                    else
                    {
                        s_CachedEditorResolutions.Add(new Vector2Int());
                    }
                }
            }
            s_CachedEditorResolutions[activeIndex] = new Vector2Int(Display.main.renderingWidth, Display.main.renderingHeight);

            return s_CachedEditorResolutions[displayIndex];
#else
            var display = default(Display);
            if (displayIndex >= Display.displays.Length || displayIndex < 0)
            {
                display = Display.main;
            }
            else
            {
                display = Display.displays[displayIndex];
            }
            return new Vector2Int(display.renderingWidth, display.renderingHeight);
#endif
        }

#if UNITY_EDITOR
        static List<Vector2Int> s_CachedEditorResolutions = new();

        static readonly System.Type s_GvType =
            typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameView");

        static readonly PropertyInfo s_TargetSize =
            s_GvType?.GetProperty("targetSize",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        static readonly PropertyInfo s_DisplayField =
            s_GvType?.GetProperty("targetDisplay",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        static bool TryFindGameViewResolution(int displayIndex, out Vector2Int resolution)
        {
            resolution = default;

            if (s_GvType == null || s_TargetSize == null || s_DisplayField == null)
                return false;

            var views = Resources.FindObjectsOfTypeAll(s_GvType);
            foreach (var v in views)
            {
                var windowDisplayIndex = (int)s_DisplayField.GetValue(v);
                if (windowDisplayIndex != displayIndex)
                    continue;

                resolution = Vector2Int.RoundToInt((Vector2)s_TargetSize.GetValue(v));
                return true;
            }

            return false;
        }
#endif
    }
}
