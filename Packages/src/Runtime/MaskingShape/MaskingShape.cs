using System;
using Coffee.UISoftMask.Internal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Coffee.UISoftMask
{
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    [DisallowMultipleComponent]
    public class MaskingShape : UIBehaviour, IMeshModifier, IMaterialModifier, IComparable<MaskingShape>, IMaskable
    {
        public enum MaskingMethod
        {
            Additive,
            Subtract
        }

        [Tooltip("Masking method.")] [SerializeField]
        private MaskingMethod m_MaskingMethod = MaskingMethod.Additive;

        [Tooltip("Show the masking shape graphic.")] [SerializeField]
        private bool m_ShowMaskGraphic;

        [Tooltip("Enable alpha hit test.")] [SerializeField]
        public bool m_AlphaHitTest;

        [Tooltip("Enable anti-alias masking.")] [SerializeField] [Range(0f, 1f)]
        public float m_AntiAliasingThreshold = 0.5f;

        [Tooltip("The range for soft masking.")]
        [SerializeField]
        public MinMax01 m_SoftnessRange = new MinMax01(0, 1f);

        private bool _antiAliasingRegistered;

        private MaskingShapeContainer _container;
        private Graphic _graphic;
        private Mask _mask;
        private Material _maskMaterial;
        private Mesh _mesh;
        private MaterialPropertyBlock _mpb;
        private Matrix4x4 _prevTransformMatrix;
        private UnityAction _setContainerDirty;
        private bool _shouldRecalculateStencil;
        private int _stencilDepth;
        private Action _updateAntiAliasing;
        private UnityAction _updateContainer;

        public Graphic graphic => _graphic || TryGetComponent(out _graphic) ? _graphic : null;

        public bool hasTransformChanged =>
            transform.HasChanged(ref _prevTransformMatrix, UISoftMaskProjectSettings.sensitivity);

        public MaskingMethod maskingMethod
        {
            get => m_MaskingMethod;
            set
            {
                if (m_MaskingMethod == value) return;
                m_MaskingMethod = value;

                SetContainerDirty();
                if (graphic)
                {
                    graphic.SetMaterialDirty();
                }
            }
        }

        public bool showMaskGraphic
        {
            get => m_ShowMaskGraphic;
            set
            {
                m_ShowMaskGraphic = value;
                if (graphic)
                {
                    graphic.SetMaterialDirty();
                }
            }
        }

        public bool alphaHitTest
        {
            get => m_AlphaHitTest;
            set => m_AlphaHitTest = value;
        }

        public float antiAliasingThreshold
        {
            get => m_AntiAliasingThreshold;
            set => m_AntiAliasingThreshold = value;
        }

        /// <summary>
        /// The range for soft masking.
        /// </summary>
        public MinMax01 softnessRange
        {
            get => m_SoftnessRange;
            set
            {
                if (m_SoftnessRange.Approximately(value)) return;

                m_SoftnessRange = value;
                SetContainerDirty();
            }
        }

        protected override void OnEnable()
        {
            UpdateContainer();

            if (graphic)
            {
                graphic.RegisterDirtyMaterialCallback(_updateContainer ??= UpdateContainer);
                graphic.RegisterDirtyVerticesCallback(_setContainerDirty ??= SetContainerDirty);
                graphic.RegisterDirtyLayoutCallback(_setContainerDirty ??= SetContainerDirty);

                graphic.SetMaterialDirty();
                graphic.SetVerticesDirty();
            }

            RegisterAntiAliasingIfNeeded();
            _shouldRecalculateStencil = true;
        }

        protected override void OnDisable()
        {
            _mask = null;
            StencilMaterial.Remove(_maskMaterial);
            _maskMaterial = null;

            SoftMaskUtils.meshPool.Return(ref _mesh);
            SoftMaskUtils.materialPropertyBlockPool.Return(ref _mpb);

            SetContainerDirty();
            UpdateContainer();
            RegisterAntiAliasingIfNeeded();

            if (graphic)
            {
                graphic.UnregisterDirtyMaterialCallback(_updateContainer ??= UpdateContainer);
                graphic.UnregisterDirtyVerticesCallback(_setContainerDirty ??= SetContainerDirty);
                graphic.UnregisterDirtyLayoutCallback(_setContainerDirty ??= SetContainerDirty);

                graphic.SetMaterialDirty();
                graphic.SetVerticesDirty();
            }
        }

        protected override void OnCanvasHierarchyChanged()
        {
            UpdateContainer();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            SetContainerDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetContainerDirty();
        }

        protected override void OnTransformParentChanged()
        {
            UpdateContainer();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetContainerDirty();
            RegisterAntiAliasingIfNeeded();
            UpdateAntiAliasing();

            if (graphic)
            {
                graphic.SetMaterialDirty();
            }
        }
#endif

        int IComparable<MaskingShape>.CompareTo(MaskingShape other)
        {
            if (this == other) return 0;
            if (!this && other) return -1;
            if (this && !other) return 1;

            var depth = graphic ? graphic.depth : -1;
            var otherDepth = other.graphic ? other.graphic.depth : -1;
            if (depth != -1 && otherDepth != -1)
            {
                return depth - otherDepth;
            }

            return transform.CompareHierarchyIndex(other.transform, _container ? _container.transform : null);
        }

        void IMaskable.RecalculateMasking()
        {
            _shouldRecalculateStencil = true;
            UpdateContainer();
            RegisterAntiAliasingIfNeeded();
        }

        Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial)
        {
            if (!isActiveAndEnabled)
            {
                return baseMaterial;
            }

            // Not in mask.
            RecalculateStencilIfNeeded();
            if (_stencilDepth <= 0)
            {
                StencilMaterial.Remove(_maskMaterial);
                _maskMaterial = null;
                return null;
            }

            var colorMask = m_ShowMaskGraphic ? ColorWriteMask.All : 0;
            var stencilBit = 1 << (_stencilDepth - 1);

            // Mask material
            Material maskMat = null;
            if (SoftMaskingEnabled())
            {
                if (m_ShowMaskGraphic)
                {
                    Profiler.BeginSample(
                        "(SM4UI)[MaskingShape)] GetModifiedMaterial > StencilMaterial.Add for SoftMask");
                    maskMat = StencilMaterial.Add(baseMaterial, stencilBit, StencilOp.Keep, CompareFunction.Equal,
                        colorMask, stencilBit, stencilBit);
                    Profiler.EndSample();
                }
            }
            else
            {
                Profiler.BeginSample("(SM4UI)[MaskingShape)] GetModifiedMaterial > StencilMaterial.Add");
                switch (maskingMethod)
                {
                    case MaskingMethod.Additive:
                        maskMat = StencilMaterial.Add(baseMaterial, stencilBit, StencilOp.Replace,
                            CompareFunction.NotEqual, colorMask, stencilBit, stencilBit);
                        break;
                    case MaskingMethod.Subtract:
                        maskMat = StencilMaterial.Add(baseMaterial, stencilBit, StencilOp.Invert,
                            CompareFunction.Equal, colorMask, stencilBit, stencilBit);
                        break;
                }

                Profiler.EndSample();
            }

            StencilMaterial.Remove(_maskMaterial);
            _maskMaterial = maskMat;
            return _maskMaterial;
        }

        void IMeshModifier.ModifyMesh(Mesh mesh)
        {
        }

        void IMeshModifier.ModifyMesh(VertexHelper verts)
        {
            if (!isActiveAndEnabled) return;


            Profiler.BeginSample("(SM4UI)[MaskingShape)] ModifyMesh");
            if (!_mesh)
            {
                _mesh = SoftMaskUtils.meshPool.Rent();
            }

            _mesh.Clear(false);
            verts.FillMesh(_mesh);
            _mesh.RecalculateBounds();

            Profiler.EndSample();

            Logging.Log(this, " >>>> Graphic mesh is modified.");
        }


        internal bool SoftMaskingEnabled()
        {
            return isActiveAndEnabled && _mask is SoftMask softMask && softMask.SoftMaskingEnabled();
        }

        internal bool AntiAliasingEnabled()
        {
            return isActiveAndEnabled && _mask is SoftMask softMask && softMask.AntiAliasingEnabled();
        }

        private void RecalculateStencilIfNeeded()
        {
            if (!isActiveAndEnabled)
            {
                _mask = null;
                _stencilDepth = -1;
                return;
            }

            if (!_shouldRecalculateStencil) return;
            _shouldRecalculateStencil = false;
            _stencilDepth = Utils.GetStencilDepthAndMask(transform, true, out _mask);
        }

        private void SetContainerDirty()
        {
            if (_container)
            {
                _container.SetContainerDirty();
            }
        }

        private void UpdateContainer()
        {
            Mask mask = null;
            if (isActiveAndEnabled)
            {
                Utils.GetStencilDepthAndMask(transform, false, out mask);
            }

            var newContainer = mask.GetOrAddComponent<MaskingShapeContainer>();
            if (newContainer != _container)
            {
                if (_container)
                {
                    _container.Unregister(this);
                }

                if (newContainer)
                {
                    newContainer.Register(this);
                }
            }

            _container = newContainer;
        }

        internal bool IsInside(Vector2 sp, Camera eventCamera, float threshold = 0.01f)
        {
            if (!isActiveAndEnabled) return false;

            {
                Profiler.BeginSample("(SM4UI)[MaskingShape)] IsInside > Rectangle");
                var inRectangle =
                    RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, sp, eventCamera);
                Profiler.EndSample();
                if (!inRectangle) return false;
            }

            if (alphaHitTest)
            {
                Profiler.BeginSample("(SM4UI)[MaskingShape)] IsInside > Alpha Hit Test");
                var hit = Utils.AlphaHitTestValid(graphic, sp, eventCamera, threshold);
                Profiler.EndSample();
                if (!hit) return false;
            }

            return true;
        }

        internal void DrawSoftMaskBuffer(CommandBuffer cb, int depth)
        {
            if (!_mesh) return;
            if (!graphic.IsInScreen()) return;

            Profiler.BeginSample("(SM4UI)[MaskingShape)] DrawSoftMaskBuffer");
            if (_mpb == null)
            {
                _mpb = SoftMaskUtils.materialPropertyBlockPool.Rent();
            }

            SoftMaskUtils.ApplyMaterialPropertyBlock(_mpb, depth, graphic.mainTexture, softnessRange);
            var softMaterial = SoftMaskUtils.GetSoftMaskingMaterial(maskingMethod);

            cb.DrawMesh(_mesh, transform.localToWorldMatrix, softMaterial, 0, 0, _mpb);
            Profiler.EndSample();
        }

        private void RegisterAntiAliasingIfNeeded()
        {
            if (_antiAliasingRegistered == AntiAliasingEnabled()) return;
            if (!_antiAliasingRegistered)
            {
                _antiAliasingRegistered = true;
                UIExtraCallbacks.onBeforeCanvasRebuild += _updateAntiAliasing ??= UpdateAntiAliasing;
                UpdateAntiAliasing();
            }
            else
            {
                _antiAliasingRegistered = false;
                UIExtraCallbacks.onBeforeCanvasRebuild -= _updateAntiAliasing ??= UpdateAntiAliasing;
                UpdateAntiAliasing();
            }
        }

        private void UpdateAntiAliasing()
        {
            if (!this || !_graphic) return;

            RecalculateStencilIfNeeded();

            if (AntiAliasingEnabled())
            {
                Utils.UpdateAntiAlias(_graphic, true, antiAliasingThreshold);
            }
            else
            {
                Utils.UpdateAntiAlias(_graphic, false, 0);
            }
        }
    }
}
