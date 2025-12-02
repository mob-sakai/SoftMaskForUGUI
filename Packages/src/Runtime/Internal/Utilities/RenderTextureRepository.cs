using System;
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
#if UNITY_2021_3_OR_NEWER
        private static readonly GraphicsFormat s_StencilFormat = GraphicsFormatUtility.GetDepthStencilFormat(0, 8);
#endif

        public static int count => s_Repository.count;

#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
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
    }
}
