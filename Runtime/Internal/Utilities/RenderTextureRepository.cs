using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;

namespace Coffee.UISoftMaskInternal
{
    /// <summary>
    /// Utility class for managing temporary render textures.
    /// </summary>
    internal static class RenderTextureRepository
    {
        private static readonly ObjectRepository<RenderTexture> s_Repository = new ObjectRepository<RenderTexture>();

        private static readonly GraphicsFormat s_GraphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Default);

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
        public static RenderTextureDescriptor GetDescriptor(Vector2Int size, bool useStencil)
        {
            Profiler.BeginSample("(COF)[RTRepository] GetDescriptor");
            var rtd = new RenderTextureDescriptor(
                size.x,
                size.y,
                s_GraphicsFormat,
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

        public static Vector2Int GetPreferSize(Vector2Int size, int downSamplingRate)
        {
            var aspect = (float)size.x / size.y;
            var screenSize = GetScreenSize();

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

        public static Vector2Int GetScreenSize(int downSamplingRate)
        {
            return GetPreferSize(GetScreenSize(), downSamplingRate);
        }

        public static Vector2Int GetScreenSize()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !Camera.current)
            {
                var res = UnityStats.screenRes.Split('x');
                return new Vector2Int(Mathf.Max(8, int.Parse(res[0])), Mathf.Max(8, int.Parse(res[1])));
            }
#endif
            return new Vector2Int(Screen.width, Screen.height);
        }
    }
}
