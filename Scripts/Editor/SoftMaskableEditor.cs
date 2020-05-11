using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;
using System;
using System.Reflection;
using Object = UnityEngine.Object;
using MaskIntr = UnityEngine.SpriteMaskInteraction;
using System.IO;

namespace Coffee.UISoftMask
{
    /// <summary>
    /// SoftMaskable editor.
    /// </summary>
    [CustomEditor(typeof(SoftMaskable))]
    [CanEditMultipleObjects]
    public class SoftMaskableEditor : Editor
    {
        public enum MaskInteraction : int
        {
            VisibleInsideMask = (1 << 0) + (1 << 2) + (1 << 4) + (1 << 6),
            VisibleOutsideMask = (2 << 0) + (2 << 2) + (2 << 4) + (2 << 6),
            Custom = -1,
        }

        MaskInteraction maskInteraction
        {
            get
            {
                int value = _spMaskInteraction.intValue;
                return _custom
                    ? MaskInteraction.Custom
                    : System.Enum.IsDefined(typeof(MaskInteraction), value)
                        ? (MaskInteraction) value
                        : MaskInteraction.Custom;
            }
            set
            {
                _custom = (value == MaskInteraction.Custom);
                if (!_custom)
                {
                    _spMaskInteraction.intValue = (int) value;
                }
            }
        }

        bool _custom = false;

        static readonly List<Graphic> s_Graphics = new List<Graphic>();
        SerializedProperty _spMaskInteraction;
        List<Mask> tmpMasks = new List<Mask>();
        static GUIContent s_MaskWarning;


        private void OnEnable()
        {
            _spMaskInteraction = serializedObject.FindProperty("m_MaskInteraction");
            _custom = (maskInteraction == MaskInteraction.Custom);
            s_MaskWarning = new GUIContent(EditorGUIUtility.FindTexture("console.warnicon.sml"),
                "This is not a SoftMask component.");
        }


        private void DrawMaskInteractions()
        {
            var softMaskable = target as SoftMaskable;
            if (softMaskable == null) return;

            softMaskable.GetComponentsInParent<Mask>(true, tmpMasks);
            tmpMasks.RemoveAll(x => !x.enabled);
            tmpMasks.Reverse();

            maskInteraction = (MaskInteraction) EditorGUILayout.EnumPopup("Mask Interaction", maskInteraction);
            if (!_custom) return;

            var l = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 45;

            using (var ccs = new EditorGUI.ChangeCheckScope())
            {
                int intr0 = DrawMaskInteraction(0);
                int intr1 = DrawMaskInteraction(1);
                int intr2 = DrawMaskInteraction(2);
                int intr3 = DrawMaskInteraction(3);

                if (ccs.changed)
                {
                    _spMaskInteraction.intValue = (intr0 << 0) + (intr1 << 2) + (intr2 << 4) + (intr3 << 6);
                }
            }

            EditorGUIUtility.labelWidth = l;
        }


        private int DrawMaskInteraction(int layer)
        {
            Mask mask = layer < tmpMasks.Count ? tmpMasks[layer] : null;
            MaskIntr intr = (MaskIntr) ((_spMaskInteraction.intValue >> layer * 2) & 0x3);
            if (!mask)
            {
                return (int) intr;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(mask is SoftMask ? GUIContent.none : s_MaskWarning, GUILayout.Width(16));
                GUILayout.Space(-5);
                EditorGUILayout.ObjectField("Mask " + layer, mask, typeof(Mask), false);
                GUILayout.Space(-15);
                return (int) (MaskIntr) EditorGUILayout.EnumPopup(intr);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            DrawMaskInteractions();

            serializedObject.ApplyModifiedProperties();

            var current = target as SoftMaskable;

            current.GetComponentsInChildren<Graphic>(true, s_Graphics);
            var fixTargets = s_Graphics.Where(x =>
                x.gameObject != current.gameObject && !x.GetComponent<SoftMaskable>() &&
                (!x.GetComponent<Mask>() || x.GetComponent<Mask>().showMaskGraphic)).ToList();
            if (0 < fixTargets.Count)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(
                    "There are child Graphics that does not have a SoftMaskable component.\nAdd SoftMaskable component to them.",
                    MessageType.Warning);
                GUILayout.BeginVertical();
                if (GUILayout.Button("Fix"))
                {
                    foreach (var p in fixTargets)
                    {
                        p.gameObject.AddComponent<SoftMaskable>();
                    }
                }

                if (GUILayout.Button("Ping"))
                {
                    EditorGUIUtility.PingObject(fixTargets[0]);
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            if (!DetectMask(current.transform.parent))
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("This is unnecessary SoftMaskable.\nCan't find any SoftMask components above.",
                    MessageType.Warning);
                if (GUILayout.Button("Remove", GUILayout.Height(40)))
                {
                    DestroyImmediate(current);

                    EditorUtils.MarkPrefabDirty();
                }

                GUILayout.EndHorizontal();
            }
        }

        static bool DetectMask(Transform transform)
        {
            while (transform)
            {
                if (transform.GetComponent<SoftMask>()) return true;

                transform = transform.parent;
            }

            return false;
        }
    }
}
