#if TMP_ENABLE
using Coffee.UISoftMaskInternal;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Coffee.UISoftMask
{
    internal sealed class TmpProxy : GraphicProxy
    {
#if UNITY_6000_6_OR_NEWER
        private static readonly VertexHelper s_VertexHelper = new VertexHelper();
#endif

        /// <summary>
        /// Check if the graphic is valid for this proxy.
        /// </summary>
        protected override bool IsValid(Graphic graphic)
        {
            if (graphic == null) return false;
            if (graphic is TextMeshProUGUI || graphic is TMP_SubMeshUI) return true;

            return false;
        }

        public override void SetMaterialDirty(Graphic graphic)
        {
            graphic.SetMaterialDirty();
            UpdateMeshUI(graphic);
        }

        public override Texture GetMainTexture(Graphic graphic)
        {
            var cr = graphic.canvasRenderer;
            if (cr == null || cr.materialCount == 0) return null;

            var mat = cr.GetMaterial(0);
            if (mat == null) return null;

            return mat.mainTexture;
        }

        public override float GetAlpha(Graphic graphic)
        {
            if (graphic is TMP_SubMeshUI sub)
            {
                return sub.textComponent.color.a;
            }

            return graphic.color.a;
        }

        private static void UpdateSubMeshUI(TextMeshProUGUI text, ISoftMasking parent)
        {
            var subMeshes = InternalListPool<TMP_SubMeshUI>.Rent();
            text.GetComponentsInChildren(subMeshes, 1);
            if (subMeshes.Count == 0)
            {
                InternalListPool<TMP_SubMeshUI>.Return(ref subMeshes);
                return;
            }

            for (var i = 0; i < subMeshes.Count; i++)
            {
                var maskingShape = subMeshes[i].GetOrAddComponent<MaskingShape>();
                maskingShape.hideFlags = HideFlags.NotEditable;
                maskingShape.enabled = parent.enabled;
                maskingShape.parent = parent;
#if UNITY_6000_6_OR_NEWER
                subMeshes[i].mesh.CopyTo(s_VertexHelper);
                (maskingShape as IMeshModifier).ModifyMesh(s_VertexHelper);
#else
#pragma warning disable CS0618
                (maskingShape as IMeshModifier).ModifyMesh(subMeshes[i].mesh);
#pragma warning restore CS0618
#endif
            }

            InternalListPool<TMP_SubMeshUI>.Return(ref subMeshes);
        }

        private static void UpdateMeshUI(Object obj)
        {
            if (!(obj is TextMeshProUGUI text)) return;

            if (text.TryGetComponent<SoftMask>(out var sm))
            {
#if UNITY_6000_6_OR_NEWER
                text.mesh.CopyTo(s_VertexHelper);
                (sm as IMeshModifier).ModifyMesh(s_VertexHelper);
#else
#pragma warning disable CS0618
                (sm as IMeshModifier).ModifyMesh(text.mesh);
#pragma warning restore CS0618
#endif
                UpdateSubMeshUI(text, sm);
            }
            else if (text.TryGetComponent<MaskingShape>(out var ms))
            {
#if UNITY_6000_6_OR_NEWER
                text.mesh.CopyTo(s_VertexHelper);
                (ms as IMeshModifier).ModifyMesh(s_VertexHelper);
#else
#pragma warning disable CS0618
                (ms as IMeshModifier).ModifyMesh(text.mesh);
#pragma warning restore CS0618
#endif
                UpdateSubMeshUI(text, ms);
            }
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void InitializeOnLoad()
        {
            Register(new TmpProxy());
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(UpdateMeshUI);
        }
    }
}
#endif
