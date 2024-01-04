using Coffee.UISoftMaskInternal;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Coffee.UISoftMask
{
    [CustomEditor(typeof(SoftMask), true)]
    [CanEditMultipleObjects]
    public class SoftMaskEditor : Editor
    {
        private const string k_PrefsPreview = "k_PrefsPreview";
        private const int k_PreviewSize = 220;
        private static readonly GUIContent s_ContentMaskingMode = new GUIContent();
        private SerializedProperty _alphaHitTest;
        private SerializedProperty _antiAliasingThreshold;
        private SerializedProperty _downSamplingRate;
        private SerializedProperty _maskingMode;
        private bool _preview;
        private SerializedProperty _showMaskGraphic;
        private SerializedProperty _softMaskingRange;

        protected void OnEnable()
        {
            _maskingMode = serializedObject.FindProperty("m_MaskingMode");
            _antiAliasingThreshold = serializedObject.FindProperty("m_AntiAliasingThreshold");
            _showMaskGraphic = serializedObject.FindProperty("m_ShowMaskGraphic");
            _downSamplingRate = serializedObject.FindProperty("m_DownSamplingRate");
            _alphaHitTest = serializedObject.FindProperty("m_AlphaHitTest");
            _softMaskingRange = serializedObject.FindProperty("m_SoftMaskingRange");
            _preview = EditorPrefs.GetBool(k_PrefsPreview, false);

            s_ContentMaskingMode.text = _maskingMode.displayName;
            s_ContentMaskingMode.tooltip = _maskingMode.tooltip;

            SoftMask.onRenderSoftMaskBuffer += OnRenderSoftMaskBuffer;
        }

        private void OnDisable()
        {
            SoftMask.onRenderSoftMaskBuffer -= OnRenderSoftMaskBuffer;
        }

        public override void OnInspectorGUI()
        {
            var current = target as SoftMask;
            if (current == null) return;

            if (!current.graphic || !current.graphic.IsActive())
            {
                EditorGUILayout.HelpBox("Masking disabled due to Graphic component being disabled.",
                    MessageType.Warning);
            }

            EditorGUILayout.PropertyField(_maskingMode);
            OpenProjectSettings(current);

            if (_maskingMode.intValue == (int)SoftMask.MaskingMode.SoftMasking)
            {
                EditorGUILayout.PropertyField(_showMaskGraphic);
                EditorGUILayout.PropertyField(_alphaHitTest);
                EditorGUILayout.PropertyField(_downSamplingRate);
                EditorGUILayout.PropertyField(_softMaskingRange);

                FixUiMaskIssue(current); // Fix 'UIMask' issue.
                DrawSoftMaskBuffer(); // Preview soft mask buffer.
            }
            else
            {
                EditorGUILayout.PropertyField(_alphaHitTest);
                EditorGUILayout.PropertyField(_antiAliasingThreshold);
            }

            serializedObject.ApplyModifiedProperties();

            // Draw alpha hit test warning
            if (current.alphaHitTest)
            {
                Utils.DrawAlphaHitTestWarning(current.graphic);
            }
        }

        private void OnRenderSoftMaskBuffer(SoftMask softMask)
        {
            if (softMask != target) return;

            Repaint();
        }

        private static void OpenProjectSettings(SoftMask current)
        {
            if (current.maskingMode != SoftMask.MaskingMode.SoftMasking) return;
            if (UISoftMaskProjectSettings.instance.m_SoftMaskEnabled) return;

            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("SoftMasking is disabled in the project settings.", MessageType.Warning);
            if (GUILayout.Button("Open"))
            {
                SettingsService.OpenProjectSettings("Project/UI/Soft Mask");
            }

            GUILayout.EndHorizontal();
        }

        private static void FixUiMaskIssue(SoftMask current)
        {
            if (current.graphic is Image currentImage && IsMaskUI(currentImage.sprite))
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(
                    "SoftMask does not recommend to use 'UIMask' sprite as a source image.\n" +
                    "(It contains only small alpha pixels.)\n" +
                    "Do you want to use 'UISprite' instead?",
                    MessageType.Warning);
                if (GUILayout.Button("Fix"))
                {
                    currentImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                }

                GUILayout.EndHorizontal();
            }
        }

        private static bool IsMaskUI(Object obj)
        {
            return obj
                   && obj.name == "UIMask"
                   && AssetDatabase.GetAssetPath(obj) == "Resources/unity_builtin_extra";
        }

        private void DrawSoftMaskBuffer()
        {
            var current = target as SoftMask;
            if (current == null || !current.SoftMaskingEnabled()) return;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                if (_preview != (_preview = EditorGUILayout.ToggleLeft("Preview Soft Mask Buffer", _preview)))
                {
                    EditorPrefs.SetBool(k_PrefsPreview, _preview);
                }

                if (_preview)
                {
                    var tex = current._softMaskBuffer;
                    var depth = current.softMaskDepth;
                    var colorMask = GetColorMask(depth);

                    if (tex)
                    {
                        GUILayout.Label($"{tex.name} (Depth: {depth} {colorMask})");
                        var aspectRatio = (float)tex.width / tex.height;
                        EditorGUI.DrawPreviewTexture(
                            GUILayoutUtility.GetRect(k_PreviewSize, k_PreviewSize / aspectRatio), tex, null,
                            ScaleMode.ScaleToFit, aspectRatio, 0, colorMask);
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private static ColorWriteMask GetColorMask(int depth)
        {
            switch (depth)
            {
                case 0: return ColorWriteMask.Red;
                case 1: return ColorWriteMask.Red | ColorWriteMask.Green;
                case 2: return ColorWriteMask.Red | ColorWriteMask.Green | ColorWriteMask.Blue;
                default: return ColorWriteMask.All;
            }
        }

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

            EditorGUILayout.EndHorizontal();
        }

        //%%%% Context menu for editor %%%%
        [MenuItem("CONTEXT/" + nameof(Mask) + "/Convert To " + nameof(SoftMask), true)]
        private static bool _ConvertToSoftMask(MenuCommand command)
        {
            return command.context.CanConvertTo<SoftMask>();
        }

        [MenuItem("CONTEXT/" + nameof(Mask) + "/Convert To " + nameof(SoftMask), false)]
        private static void ConvertToSoftMask(MenuCommand command)
        {
            command.context.ConvertTo<SoftMask>();
        }

        [MenuItem("CONTEXT/" + nameof(SoftMask) + "/Convert To " + nameof(Mask), true)]
        private static bool _ConvertToMask(MenuCommand command)
        {
            return command.context.CanConvertTo<Mask>();
        }

        [MenuItem("CONTEXT/" + nameof(SoftMask) + "/Convert To " + nameof(Mask), false)]
        private static void ConvertToMask(MenuCommand command)
        {
            command.context.ConvertTo<Mask>();
        }
    }
}
