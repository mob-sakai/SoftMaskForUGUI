using Coffee.UISoftMaskInternal;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.U2D;
#endif

namespace Coffee.UISoftMask
{
    internal static class Utils
    {
        /// <summary>
        /// Update the smooth edge effect for the graphic.
        /// </summary>
        public static void UpdateAntiAlias(Graphic graphic, bool enabled, float threshold)
        {
            if (!graphic) return;

            var canvasRenderer = graphic.canvasRenderer;
            var currentColor = canvasRenderer.GetColor();
            var targetAlpha = 1f;
            if (enabled)
            {
                var currentAlpha = graphic.color.a * canvasRenderer.GetInheritedAlpha();
                if (0 < currentAlpha)
                {
                    // Adjust the alpha value for a smooth edge based on the threshold.
                    threshold = Mathf.Clamp01(threshold);
                    targetAlpha = Mathf.Lerp(1f / 255, 0.1f, threshold) / currentAlpha;
                }
            }

            // Update the alpha value of the canvas renderer's color to achieve the smooth edge effect.
            if (!Mathf.Approximately(currentColor.a, targetAlpha))
            {
                currentColor.a = Mathf.Clamp01(targetAlpha);
                canvasRenderer.SetColor(currentColor);
            }
        }

        public static int GetStencilBits(Transform transform, bool includeSelf, bool useStencil,
            out Mask nearestMask, out SoftMask nearestSoftMask)
        {
            Profiler.BeginSample("(SM4UI)[SoftMaskable] GetStencilBits");
            nearestMask = null;
            nearestSoftMask = null;
            var stopAfter = MaskUtilities.FindRootSortOverrideCanvas(transform);
            if (transform == stopAfter)
            {
                Profiler.EndSample();
                return 0;
            }

            var depth = 0;
            var stencilBits = 0;
            var tr = includeSelf ? transform : transform.parent;
            while (tr)
            {
                if (tr.TryGetComponent<Mask>(out var mask) && mask.MaskEnabled())
                {
                    if (!nearestMask)
                    {
                        nearestMask = mask;
                        if (FrameCache.TryGet(nearestMask, nameof(GetStencilBits), out stencilBits))
                        {
                            FrameCache.TryGet(nearestMask, nameof(GetStencilBits), out nearestSoftMask);

                            Profiler.EndSample();
                            return stencilBits;
                        }
                    }

                    stencilBits = 0 < depth++ ? stencilBits << 1 : 0;
                    if (mask is SoftMask softMask && softMask.SoftMaskingEnabled())
                    {
                        if (!nearestSoftMask)
                        {
                            nearestSoftMask = softMask;
                        }

                        if (useStencil)
                        {
                            stencilBits++;
                        }
                    }
                    else
                    {
                        stencilBits++;
                    }
                }

                if (tr == stopAfter) break;
                tr = tr.parent;
            }

            stencilBits = Mathf.Min(stencilBits, 255);
            Profiler.EndSample();
            if (nearestMask)
            {
                FrameCache.Set(nearestMask, nameof(GetStencilBits), stencilBits);
                FrameCache.Set(nearestMask, nameof(GetStencilBits), nearestSoftMask);
            }

            return stencilBits;
        }

        public static bool AlphaHitTestValid(Graphic src, Vector2 sp, Camera eventCamera, float threshold)
        {
            if (!src || !src.IsActive()) return false;
            if (!(src is Image || src is RawImage)) return true;

            if (FrameCache.TryGet(src, nameof(AlphaHitTestValid), out bool valid))
            {
                return valid;
            }

            if (src is Image image)
            {
                valid = AlphaHitTestValid(image, sp, eventCamera, threshold);
            }
            else if (src is RawImage rawImage)
            {
                valid = AlphaHitTestValid(rawImage, sp, eventCamera, threshold);
            }

            FrameCache.Set(src, nameof(AlphaHitTestValid), valid);
            return valid;
        }

        private static bool AlphaHitTestValid(Image src, Vector2 sp, Camera eventCamera, float threshold)
        {
            if (!src) return true;

            var texture = src.overrideSprite.GetActualTexture();
            if (!texture) return true;
            if (GraphicsFormatUtility.IsCrunchFormat(texture.format) || !texture.isReadable) return false;

            var origin = src.alphaHitTestMinimumThreshold;
            if (0 < origin && origin <= 1) return true;

            Profiler.BeginSample("(SM4UI)[Utils] AlphaHitTestValid (Image)");
            src.alphaHitTestMinimumThreshold = threshold;
            var valid = src.IsRaycastLocationValid(sp, eventCamera);
            src.alphaHitTestMinimumThreshold = origin;
            Profiler.EndSample();
            return valid;
        }

        private static bool AlphaHitTestValid(RawImage src, Vector2 sp, Camera eventCamera, float threshold)
        {
            var texture = src.texture as Texture2D;
            if (texture == null || !texture.isReadable) return true;

            Profiler.BeginSample("(SM4UI)[Utils] AlphaHitTestValid (RawImage)");

            var rt = src.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, sp, eventCamera, out var lp))
            {
                Profiler.EndSample();
                return false;
            }

            var rect = src.GetPixelAdjustedRect();
            var x = Mathf.Repeat((lp.x + rt.pivot.x * rect.width) / rect.width * src.uvRect.width + src.uvRect.x, 1);
            var y = Mathf.Repeat((lp.y + rt.pivot.y * rect.height) / rect.height * src.uvRect.height + src.uvRect.y, 1);

            try
            {
                return threshold < texture.GetPixelBilinear(x, y).a;
            }
            catch
            {
                return true;
            }
            finally
            {
                Profiler.EndSample();
            }
        }

#if UNITY_EDITOR
        private static string GetWarningMessage(Graphic src)
        {
            if (!(src is Image || src is RawImage))
            {
                return $"{src.GetType().Name} is not supported type for alpha hit test.";
            }

            if (src is Image image && image)
            {
                var atlas = image.overrideSprite
                    ? image.overrideSprite.GetActiveAtlas()
                    : null;
                if (atlas && atlas.GetPackingSettings().enableTightPacking)
                {
                    return $"Tight packed sprite atlas '{atlas.name}' is not supported.";
                }
            }

            var tex = src.GetActualMainTexture();
            if (!tex)
            {
                return "No texture is assigned.";
            }

            if (!(tex is Texture2D))
            {
                return $"The texture '{tex.name}' is not Texture2D.";
            }

            if (!tex.isReadable)
            {
                return $"The texture '{tex.name}' is not readable";
            }

            return "";
        }

        public static void DrawAlphaHitTestWarning(Graphic graphic)
        {
            if (!graphic) return;

            var warn = GetWarningMessage(graphic);
            if (string.IsNullOrEmpty(warn)) return;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.HelpBox(warn, MessageType.Warning);
                if (GUILayout.Button("Select"))
                {
                    if (graphic is Image image && image)
                    {
                        var sprite = image.overrideSprite;
                        if (sprite)
                        {
                            Selection.activeObject = sprite.GetActiveAtlas();
                        }

                        if (!Selection.activeObject)
                        {
                            Selection.activeObject = image.GetActualMainTexture();
                        }
                    }
                    else
                    {
                        Selection.activeObject = graphic.GetActualMainTexture();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
#endif
    }
}
