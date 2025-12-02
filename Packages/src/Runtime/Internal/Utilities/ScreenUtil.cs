using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Coffee.UISoftMaskInternal
{
    /// <summary>
    /// Utility methods for screen and display information.
    /// </summary>
    internal static class ScreenUtil
    {
        /// <summary>
        /// Gets the display index used by a Graphic's canvas.
        /// </summary>
        public static int GetDisplayIndex(Graphic graphic)
        {
            var canvas = graphic.canvas;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return canvas.targetDisplay;
            }

            if (canvas.worldCamera && canvas.worldCamera.targetTexture == null)
            {
                return canvas.worldCamera.targetDisplay;
            }

            return 0;
        }

        /// <summary>
        /// Gets the preferred render texture size for a display.
        /// </summary>
        public static Vector2Int GetPreferSize(int displayIndex, Vector2Int size, int downSamplingRate)
        {
            var aspect = (float)size.x / size.y;
            var screenSize = GetScreenSize(displayIndex);

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

        /// <summary>
        /// Gets the preferred screen size after downsampling.
        /// </summary>
        public static Vector2Int GetScreenSize(int displayIndex, int downSamplingRate)
        {
            return GetPreferSize(displayIndex, GetScreenSize(displayIndex), downSamplingRate);
        }

        /// <summary>
        /// Gets the screen size for a display.
        /// </summary>
        public static Vector2Int GetScreenSize(int displayIndex)
        {
            Display display;
            if (displayIndex < 0 || Display.displays.Length <= displayIndex)
            {
                display = Display.main;
            }
            else
            {
                display = Display.displays[displayIndex];
            }

            var width = display.renderingWidth;
            var height = display.renderingHeight;

#if UNITY_EDITOR
            if ((width <= 0 || height <= 0) && displayIndex == 0 && !Application.isPlaying && Camera.current == null)
            {
                var screenResolution = UnityStats.screenRes;
                var separator = screenResolution.IndexOf('x');
                if (0 < separator && separator < screenResolution.Length - 1)
                {
                    int.TryParse(screenResolution.Substring(0, separator), out width);
                    int.TryParse(screenResolution.Substring(separator + 1), out height);
                }
            }
#endif

            if (width <= 0 || height <= 0)
            {
                width = display.systemWidth;
                height = display.systemHeight;
            }

            if (width <= 0 || height <= 0)
            {
                width = Screen.width;
                height = Screen.height;
            }

            return new Vector2Int(Mathf.Max(8, width), Mathf.Max(8, height));
        }
    }
}
