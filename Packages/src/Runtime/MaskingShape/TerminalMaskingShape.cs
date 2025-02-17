using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Coffee.UISoftMask
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    [Icon("Packages/com.coffee.softmask-for-ugui/Icons/SoftMaskIcon.png")]
    public class TerminalMaskingShape : MaskableGraphic, ILayoutElement, ILayoutIgnorer, IMaskable
    {
        private static Material s_SharedTerminalMaterial;
        private Mask _mask;
        private Mask _parentMask;
        private bool _shouldRecalculateStencil;
        private int _stencilBits;

        public override bool raycastTarget
        {
            get => false;
            set { }
        }

        protected override void OnEnable()
        {
            if (!s_SharedTerminalMaterial)
            {
                s_SharedTerminalMaterial = new Material(Shader.Find("Hidden/UI/TerminalMaskingShape"))
                {
                    hideFlags = HideFlags.DontSave | HideFlags.NotEditable
                };
            }

            material = s_SharedTerminalMaterial;
            transform.parent.TryGetComponent(out _parentMask);
            _shouldRecalculateStencil = true;
            hideFlags = UISoftMaskProjectSettings.hideFlagsForTemp;

#if UNITY_EDITOR
            UISoftMaskProjectSettings.shaderRegistry.RegisterVariant(s_SharedTerminalMaterial, "UI > Soft Mask");
#endif
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_parentMask && _parentMask.MaskEnabled())
            {
                _parentMask.graphic.SetMaterialDirty();
            }
        }

        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
        }

        void ILayoutElement.CalculateLayoutInputVertical()
        {
        }

        float ILayoutElement.minWidth => 0;

        float ILayoutElement.preferredWidth => 0;

        float ILayoutElement.flexibleWidth => 0;

        float ILayoutElement.minHeight => 0;

        float ILayoutElement.preferredHeight => 0;

        float ILayoutElement.flexibleHeight => 0;

        int ILayoutElement.layoutPriority => 0;

        bool ILayoutIgnorer.ignoreLayout => true;

        void IMaskable.RecalculateMasking()
        {
            _shouldRecalculateStencil = true;
        }

        public override Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!IsActive())
            {
                StencilMaterial.Remove(m_MaskMaterial);
                m_MaskMaterial = null;
                return null;
            }

            RecalculateStencilIfNeeded();
            if ((_stencilBits == 0 && !_mask) || _parentMask != _mask)
            {
                StencilMaterial.Remove(m_MaskMaterial);
                m_MaskMaterial = null;
                return null;
            }

            var maskMat = StencilMaterial.Add(baseMaterial, _stencilBits, StencilOp.Zero, CompareFunction.Equal, 0,
                _stencilBits, Utils.GetHighestBit(_stencilBits));

            StencilMaterial.Remove(m_MaskMaterial);
            m_MaskMaterial = maskMat;
            return maskMat;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (!IsActive()) return;

            // Full-screen rendering.
            Profiler.BeginSample("(SM4UI)[TerminalMaskingShape)] OnPopulateMesh");
            vh.AddVert(new Vector3(-999999, -999999), new Color32(255, 255, 255, 255), new Vector2(0, 0));
            vh.AddVert(new Vector3(-999999, +999999), new Color32(255, 255, 255, 255), new Vector2(0, 1));
            vh.AddVert(new Vector3(+999999, +999999), new Color32(255, 255, 255, 255), new Vector2(1, 1));
            vh.AddVert(new Vector3(+999999, -999999), new Color32(255, 255, 255, 255), new Vector2(1, 0));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
            Profiler.EndSample();
        }

        private void RecalculateStencilIfNeeded()
        {
            if (!isActiveAndEnabled)
            {
                _mask = null;
                _stencilBits = 0;
                return;
            }

            if (!_shouldRecalculateStencil) return;
            _shouldRecalculateStencil = false;
            var useStencil = UISoftMaskProjectSettings.useStencilOutsideScreen;
            _stencilBits = Utils.GetStencilBits(transform, false, useStencil, out _mask, out var _);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TerminalMaskingShape))]
    internal class TerminalMaskingShapeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
        }
    }
#endif
}
