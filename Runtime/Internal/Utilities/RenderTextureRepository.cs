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
        private static readonly ObjectRepository<RenderTexture> s_Repository =
            new ObjectRepository<RenderTexture>(RenderTexture.ReleaseTemporary);

        private static readonly GraphicsFormat s_GraphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Default);

#if UNITY_2021_3_OR_NEWER
        private static readonly GraphicsFormat s_StencilFormat = GraphicsFormatUtility.GetDepthStencilFormat(0, 8);
#endif

        public static int count => s_Repository.count;

        private static bool ShouldToRelease(RenderTexture buffer, Vector2Int size, bool useStencil)
        {
            if (!buffer) return false;
            if (buffer.width != size.x || buffer.height != size.y) return true;
#if UNITY_2021_3_OR_NEWER
            if (useStencil != (buffer.depthStencilFormat != GraphicsFormat.None)) return true;
#else
            if (useStencil != 0 < buffer.depth) return true;
#endif
            return false;
        }

        public static RenderTexture Get(int id, Vector2 size, int rate, ref RenderTexture buffer, bool useStencil)
        {
            var preferSize = GetPreferSize(new Vector2Int(
                Mathf.Max(8, Mathf.RoundToInt(size.x)),
                Mathf.Max(8, Mathf.RoundToInt(size.y))), rate);

            Profiler.BeginSample("(COF)[RTRepository] Get > ShouldToRelease");

            if (ShouldToRelease(buffer, preferSize, useStencil))
            {
                s_Repository.Release(ref buffer);
            }

            Profiler.EndSample();

            Profiler.BeginSample("(COF)[RTRepository] Get > Valid");
            var hash = new Hash128((uint)id, 0, 0, 0);
            if (s_Repository.Valid(hash, buffer))
            {
                Profiler.EndSample();
                return buffer;
            }

            Profiler.EndSample();

            Profiler.BeginSample("(COF)[RTRepository] Get > Create 0");
            var rtd = new RenderTextureDescriptor(
                preferSize.x,
                preferSize.y,
                s_GraphicsFormat,
                useStencil ? 24 : 0);
            rtd.sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear;
            rtd.mipCount = -1;
#if UNITY_2021_3_OR_NEWER
            rtd.depthStencilFormat = useStencil ? s_StencilFormat : GraphicsFormat.None;
#endif
            s_Repository.Get(hash, ref buffer, x => RenderTexture.GetTemporary(x), rtd);
            Profiler.EndSample();
            return buffer;
        }

        /// <summary>
        /// Releases the RenderTexture buffer.
        /// </summary>
        public static void Release(ref RenderTexture buffer)
        {
            Profiler.BeginSample("(COF)[RTRepository] Release");
            s_Repository.Release(ref buffer);
            Profiler.EndSample();
        }

        private static Vector2Int GetPreferSize(Vector2Int size, int downSamplingRate)
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
