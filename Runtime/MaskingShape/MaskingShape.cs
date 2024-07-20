using System;
using Coffee.UISoftMaskInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
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

        [Tooltip("Masking method.")]
        [SerializeField]
        private MaskingMethod m_MaskingMethod = MaskingMethod.Additive;

        [Tooltip("Show the graphic that is associated with the Mask render area.")]
        [SerializeField]
        private bool m_ShowMaskGraphic;

        [Tooltip("The transparent part of the mask cannot be clicked.\n" +
                 "This can be achieved by enabling Read/Write enabled in the Texture Import Settings for the texture.\n\n" +
                 "NOTE: Enable this only if necessary, as it will require more graphics memory and processing time.")]
        [SerializeField]
        private bool m_AlphaHitTest;

        [Tooltip("The threshold for anti-alias masking.\n" +
                 "The smaller this value, the less jagged it is.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float m_AntiAliasingThreshold;

        [Tooltip("The minimum and maximum alpha values used for soft masking.\n" +
                 "The larger the gap between these values, the stronger the softness effect.")]
        [SerializeField]
        private MinMax01 m_SoftnessRange = new MinMax01(0, 1f);

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
        private int _stencilBits;
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

        /// <summary>
        /// Show the graphic that is associated with the Mask render area.
        /// </summary>
        public bool showMaskGraphic
        {
            get => m_ShowMaskGraphic;
            set
            {
                if (m_ShowMaskGraphic == value) return;
                m_ShowMaskGraphic = value;
                if (graphic)
                {
                    graphic.SetMaterialDirty();
                }
            }
        }

        /// <summary>
        /// The transparent part of the mask cannot be clicked.
        /// This can be achieved by enabling Read/Write enabled in the Texture Import Settings for the texture.
        /// <para></para>
        /// NOTE: Enable this only if necessary, as it will require more graphics memory and processing time.
        /// </summary>
        public bool alphaHitTest
        {
            get => m_AlphaHitTest;
            set => m_AlphaHitTest = value;
        }

        /// <summary>
        /// Threshold for anti-alias masking.
        /// The smaller this value, the less jagged it is.
        /// </summary>
        public float antiAliasingThreshold
        {
            get => m_AntiAliasingThreshold;
            set => m_AntiAliasingThreshold = value;
        }

        /// <summary>
        /// The minimum and maximum alpha values used for soft masking.
        /// The larger the gap between these values, the stronger the softness effect.
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

        /// <summary>
        /// The transparency of the masking graphic.
        /// </summary>
        public float alpha
        {
            get => graphic ? graphic.color.a : 1;
            set
            {
                value = Mathf.Clamp01(value);
                if (!this || Mathf.Approximately(alpha, value)) return;

                if (graphic)
                {
                    var color = graphic.color;
                    color.a = value;
                    graphic.color = color;
                }
            }
        }

        protected override void OnEnable()
        {
            UpdateContainer();

            if (graphic)
            {
                graphic.RegisterDirtyMaterialCallback(_updateContainer ?? (_updateContainer = UpdateContainer));
                graphic.RegisterDirtyVerticesCallback(_setContainerDirty ?? (_setContainerDirty = SetContainerDirty));
                graphic.RegisterDirtyLayoutCallback(_setContainerDirty ?? (_setContainerDirty = SetContainerDirty));

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

            MeshExtensions.Return(ref _mesh);
            SoftMaskUtils.materialPropertyBlockPool.Return(ref _mpb);

            SetContainerDirty();
            UpdateContainer();
            RegisterAntiAliasingIfNeeded();

            if (graphic)
            {
                graphic.UnregisterDirtyMaterialCallback(_updateContainer ?? (_updateContainer = UpdateContainer));
                graphic.UnregisterDirtyVerticesCallback(_setContainerDirty ?? (_setContainerDirty = SetContainerDirty));
                graphic.UnregisterDirtyLayoutCallback(_setContainerDirty ?? (_setContainerDirty = SetContainerDirty));

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
            if (_stencilBits == 0 && !_mask)
            {
                StencilMaterial.Remove(_maskMaterial);
                _maskMaterial = null;
                return null;
            }

            // Mask material
            Material maskMat = null;
            var colorMask = m_ShowMaskGraphic ? ColorWriteMask.All : 0;
            if (SoftMaskingEnabled() && !UISoftMaskProjectSettings.useStencilOutsideScreen)
            {
                if (m_ShowMaskGraphic)
                {
                    Profiler.BeginSample(
                        "(SM4UI)[MaskingShape)] GetModifiedMaterial > StencilMaterial.Add for SoftMask");
                    maskMat = StencilMaterial.Add(baseMaterial, _stencilBits, StencilOp.Keep, CompareFunction.Equal,
                        colorMask, _stencilBits, _stencilBits);
                    Profiler.EndSample();
                }
            }
            else
            {
                Profiler.BeginSample("(SM4UI)[MaskingShape)] GetModifiedMaterial > StencilMaterial.Add");
                switch (maskingMethod)
                {
                    case MaskingMethod.Additive:
                        maskMat = StencilMaterial.Add(baseMaterial, _stencilBits, StencilOp.Replace,
                            CompareFunction.NotEqual, colorMask, _stencilBits, _stencilBits);
                        break;
                    case MaskingMethod.Subtract:
                        maskMat = StencilMaterial.Add(baseMaterial, _stencilBits, StencilOp.Invert,
                            CompareFunction.Equal, colorMask, _stencilBits, _stencilBits);
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
                _mesh = MeshExtensions.Rent();
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
                _stencilBits = 0;
                return;
            }

            if (!_shouldRecalculateStencil) return;
            _shouldRecalculateStencil = false;
            var useStencil = UISoftMaskProjectSettings.useStencilOutsideScreen;
            _stencilBits = Utils.GetStencilBits(transform, true, useStencil, out _mask, out var _);
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
                var useStencil = UISoftMaskProjectSettings.useStencilOutsideScreen;
                Utils.GetStencilBits(transform, false, useStencil, out mask, out var _);
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
            var texture = graphic.mainTexture;
            var mesh = _mesh;
            if (!mesh) return;
            if (!graphic.IsInScreen()) return;

            Profiler.BeginSample("(SM4UI)[MaskingShape)] DrawSoftMaskBuffer");
            if (_mpb == null)
            {
                _mpb = SoftMaskUtils.materialPropertyBlockPool.Rent();
            }

            SoftMaskUtils.ApplyMaterialPropertyBlock(_mpb, depth, texture, softnessRange);
            var softMaterial = SoftMaskUtils.GetSoftMaskingMaterial(maskingMethod);

            cb.DrawMesh(mesh, transform.localToWorldMatrix, softMaterial, 0, 0, _mpb);
            Profiler.EndSample();
        }

        private void RegisterAntiAliasingIfNeeded()
        {
            if (_antiAliasingRegistered == AntiAliasingEnabled()) return;
            if (!_antiAliasingRegistered)
            {
                _antiAliasingRegistered = true;
                UIExtraCallbacks.onBeforeCanvasRebuild +=
                    _updateAntiAliasing ?? (_updateAntiAliasing = UpdateAntiAliasing);
                UpdateAntiAliasing();
            }
            else
            {
                _antiAliasingRegistered = false;
                UIExtraCallbacks.onBeforeCanvasRebuild -=
                    _updateAntiAliasing ?? (_updateAntiAliasing = UpdateAntiAliasing);
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
