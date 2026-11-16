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
                maskingShape.ModifyMesh(subMeshes[i].mesh);
            }

            InternalListPool<TMP_SubMeshUI>.Return(ref subMeshes);
        }

        private static void UpdateMeshUI(Object obj)
        {
            if (!(obj is TextMeshProUGUI text)) return;

            if (text.TryGetComponent<SoftMask>(out var sm))
            {
                sm.ModifyMesh(text.mesh);
                UpdateSubMeshUI(text, sm);
            }
            else if (text.TryGetComponent<MaskingShape>(out var ms))
            {
                ms.ModifyMesh(text.mesh);
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
