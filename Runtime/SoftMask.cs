using System;
using System.Collections.Generic;
using Coffee.UISoftMask.Internal;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Coffee.UISoftMask
{
    /// <summary>
    /// SoftMask.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class SoftMask : Mask, IMeshModifier, IMaskable, IMaskingShapeContainerOwner
    {
        /// <summary>
        /// Down sampling rate.
        /// </summary>
        public enum DownSamplingRate
        {
            None = 0,
            x1 = 1,
            x2 = 2,
            x4 = 4,
            x8 = 8
        }

        /// <summary>
        /// Masking mode.<br />
        /// <b>SoftMasking</b>: Use soft masking.<br />
        /// <b>AntiAliasing</b>: Use anti-aliasing.
        /// </summary>
        public enum MaskingMode
        {
            SoftMasking,
            AntiAliasing,
            Normal
        }

        public static Action<SoftMask> onRenderSoftMaskBuffer = null;
        private static readonly Camera.MonoOrStereoscopicEye[] s_MonoEyes = { Camera.MonoOrStereoscopicEye.Mono };

        private static readonly Camera.MonoOrStereoscopicEye[] s_StereoEyes =
            { Camera.MonoOrStereoscopicEye.Left, Camera.MonoOrStereoscopicEye.Right };

        [Tooltip("Masking mode\n\n" +
                 "SoftMasking: Use RenderTexture as a soft mask buffer. The alpha of the masking graphic can be used.\n" +
                 "AntiAliasing: Suppress the jaggedness of the masking graphic. The masking graphic cannot be displayed.")]
        [SerializeField]
        private MaskingMode m_MaskingMode = MaskingMode.SoftMasking;

        [Tooltip("Enable alpha hit test.")]
        [SerializeField]
        private bool m_AlphaHitTest;

        [Tooltip("The threshold for soft masking.")]
        [SerializeField]
        private MinMax01 m_SoftMaskingRange = new MinMax01(0, 1f);

        [Tooltip("The down sampling rate for soft mask buffer.")]
        [SerializeField]
        private DownSamplingRate m_DownSamplingRate = DownSamplingRate.x1;

        [Tooltip("The threshold for anti-alias masking.")] [SerializeField] [Range(0f, 1f)]
        private float m_AntiAliasingThreshold;

        [SerializeField] [Obsolete]
        private float m_Softness = -1;

        [SerializeField] [Obsolete]
        private bool m_PartOfParent;

        private CommandBuffer _cb;

        private List<SoftMask> _children = ListPool<SoftMask>.Rent();
        private bool _hasSoftMaskBufferDrawn;
        private Mesh _mesh;
        private MaterialPropertyBlock _mpb;
        private Action _onBeforeCanvasRebuild;
        private SoftMask _parent;
        private Matrix4x4 _prevTransformMatrix;
        private Action _renderSoftMaskBuffer;
        private Canvas _rootCanvas;
        private Action _setDirtyAndNotify;
        private Action _setSoftMaskDirty;
        private UnityAction _setSoftMaskDirty2;
        private MaskingShapeContainer _shapeContainer;
        internal RenderTexture _softMaskBuffer;
        private UnityAction _updateParentSoftMask;
        private CanvasViewChangeTrigger _viewChangeTrigger;

        /// <summary>
        /// Masking mode<br />
        /// <b>SoftMasking</b>: Use RenderTexture as a soft mask buffer. The alpha of the masking graphic can be used.<br />
        /// <b>AntiAliasing</b>: Suppress the jaggedness of the masking graphic. The masking graphic cannot be displayed.
        /// </summary>
        public MaskingMode maskingMode
        {
            get => m_MaskingMode;
            set
            {
                if (m_MaskingMode == value) return;

                m_MaskingMode = value;
                AddSoftMaskableOnChildren();
                UpdateAntiAlias();
                SetDirtyAndNotify();
            }
        }

        public DownSamplingRate downSamplingRate
        {
            get => m_DownSamplingRate;
            set
            {
                if (m_DownSamplingRate == value) return;
                m_DownSamplingRate = value;
                SetSoftMaskDirty();
            }
        }

        /// <summary>
        /// Threshold for anti-alias masking.
        /// </summary>
        public float antiAliasingThreshold
        {
            get => m_AntiAliasingThreshold;
            set => m_AntiAliasingThreshold = value;
        }

        /// <summary>
        /// Enable alpha hit test.
        /// </summary>
        public bool alphaHitTest
        {
            get => m_AlphaHitTest;
            set => m_AlphaHitTest = value;
        }

        /// <summary>
        /// The soft mask depth.
        /// </summary>
        public int softMaskDepth
        {
            get
            {
                var depth = -1;
                for (var current = this; current; current = current._parent)
                {
                    if (current.SoftMaskingEnabled())
                    {
                        depth++;
                    }
                }

                return depth;
            }
        }

        /// <summary>
        /// Is the soft mask a part of parent soft mask?
        /// </summary>
        [Obsolete]
        public bool partOfParent
        {
            get => m_PartOfParent;
            set => m_PartOfParent = value;
        }


        /// <summary>
        /// The value used by the soft mask to select the area of influence defined over the soft mask's graphic.
        /// </summary>
        [Obsolete]
        public float softness
        {
            get => m_Softness;
            set => m_Softness = value;
        }

        public bool hasSoftMaskBuffer => _softMaskBuffer;

        /// <summary>
        /// The soft mask buffer.
        /// </summary>
        public RenderTexture softMaskBuffer
        {
            get
            {
                if (SoftMaskingEnabled())
                {
                    var id = GetInstanceID();
                    var size = RenderTextureRepository.GetScreenSize();
                    var rate = (int)downSamplingRate;
                    return RenderTextureRepository.Get(id, size, rate, ref _softMaskBuffer, false);
                }

                RenderTextureRepository.Release(ref _softMaskBuffer);
                return null;
            }
        }

        /// <summary>
        /// The threshold for soft masking.
        /// </summary>
        public MinMax01 softMaskingRange
        {
            get => m_SoftMaskingRange;
            set
            {
                if (m_SoftMaskingRange.Approximately(value)) return;

                m_SoftMaskingRange = value;
                SetSoftMaskDirty();
            }
        }

        /// <summary>
        /// Clear color for the soft mask buffer.
        /// </summary>
        public Color clearColor
        {
            get;
            set;
        }

        public bool isDirty
        {
            get;
            private set;
        }

        /// <summary>
        /// Called when the component is enabled.
        /// </summary>
        protected override void OnEnable()
        {
            UIExtraCallbacks.onBeforeCanvasRebuild += _onBeforeCanvasRebuild ??= OnBeforeCanvasRebuild;
            UIExtraCallbacks.onAfterCanvasRebuild += _renderSoftMaskBuffer ??= RenderSoftMaskBuffer;
            SoftMaskUtils.onChangeBufferSize += _setDirtyAndNotify ??= SetDirtyAndNotify;

            if (graphic)
            {
                graphic.RegisterDirtyMaterialCallback(_updateParentSoftMask ??= UpdateParentSoftMask);
                graphic.RegisterDirtyVerticesCallback(_setSoftMaskDirty2 ??= SetSoftMaskDirty);
                graphic.SetVerticesDirty();
            }

            AddSoftMaskableOnChildren();
            OnCanvasHierarchyChanged();
            _shapeContainer = GetComponent<MaskingShapeContainer>();

            base.OnEnable();
        }

        /// <summary>
        /// Called when the component is disabled.
        /// </summary>
        protected override void OnDisable()
        {
            UIExtraCallbacks.onBeforeCanvasRebuild -= _onBeforeCanvasRebuild ??= OnBeforeCanvasRebuild;
            UIExtraCallbacks.onAfterCanvasRebuild -= _renderSoftMaskBuffer ??= RenderSoftMaskBuffer;
            SoftMaskUtils.onChangeBufferSize -= _setDirtyAndNotify ??= SetDirtyAndNotify;

            if (graphic)
            {
                graphic.UnregisterDirtyMaterialCallback(_updateParentSoftMask ??= UpdateParentSoftMask);
                graphic.UnregisterDirtyVerticesCallback(_setSoftMaskDirty2 ??= SetSoftMaskDirty);
                graphic.SetVerticesDirty();
            }

            UpdateParentSoftMask(null);
            _children.Clear();

            SoftMaskUtils.meshPool.Return(ref _mesh);
            SoftMaskUtils.materialPropertyBlockPool.Return(ref _mpb);
            SoftMaskUtils.commandBufferPool.Return(ref _cb);
            RenderTextureRepository.Release(ref _softMaskBuffer);

            UpdateCanvasViewChangeTrigger(null);
            _rootCanvas = null;
            _shapeContainer = null;

            UpdateAntiAlias();

            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            ListPool<SoftMask>.Return(ref _children);
            _onBeforeCanvasRebuild = null;
            _setDirtyAndNotify = null;
            _renderSoftMaskBuffer = null;
            _setSoftMaskDirty = null;
            _setSoftMaskDirty2 = null;
            _updateParentSoftMask = null;
        }

        /// <summary>
        /// Called when the state of the parent Canvas is changed.
        /// </summary>
        protected override void OnCanvasHierarchyChanged()
        {
            if (!isActiveAndEnabled) return;
            _rootCanvas = this.GetRootComponent<Canvas>();
            UpdateCanvasViewChangeTrigger(null);
        }

        /// <summary>
        /// Call from unity if animation properties have changed.
        /// </summary>
        protected override void OnDidApplyAnimationProperties()
        {
            SetSoftMaskDirty();
        }

        /// <summary>
        /// This callback is called if an associated RectTransform has its dimensions changed. The call is also made to all child
        /// rect transforms, even if the child transform itself doesn't change - as it could have, depending on its anchoring.
        /// </summary>
        protected override void OnRectTransformDimensionsChange()
        {
            SetSoftMaskDirty();
        }

        protected void OnTransformChildrenChanged()
        {
            AddSoftMaskableOnChildren();
        }

        protected override void OnTransformParentChanged()
        {
            UpdateParentSoftMask();
            UpdateCanvasViewChangeTrigger(CanvasViewChangeTrigger.Find(transform));
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            AddSoftMaskableOnChildren();
            SetDirtyAndNotify();
            UpdateAntiAlias();

            if (graphic)
            {
                graphic.SetVerticesDirty();
            }
        }
#endif

        void IMaskable.RecalculateMasking()
        {
            SetSoftMaskDirty();
            if (!SoftMaskingEnabled() && _softMaskBuffer)
            {
                RenderTextureRepository.Release(ref _softMaskBuffer);
            }
        }

        void IMaskingShapeContainerOwner.Register(MaskingShapeContainer container)
        {
            _shapeContainer = container;
        }

        void IMeshModifier.ModifyMesh(Mesh mesh)
        {
        }

        void IMeshModifier.ModifyMesh(VertexHelper verts)
        {
            if (!SoftMaskingEnabled())
            {
                SoftMaskUtils.meshPool.Return(ref _mesh);
                return;
            }

            Profiler.BeginSample("(SM4UI)[SoftMask] ModifyMesh");
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

        private void AddSoftMaskableOnChildren()
        {
            if (!isActiveAndEnabled || !SoftMaskingEnabled()) return;

            this.AddComponentOnChildren<SoftMaskable>(HideFlags.DontSave | HideFlags.NotEditable, true);
        }

        private void OnBeforeCanvasRebuild()
        {
            switch (maskingMode)
            {
                // SoftMasking mode: If transform or view has changed, set dirty flag.
                case MaskingMode.SoftMasking:
                {
                    if (transform.HasChanged(ref _prevTransformMatrix, UISoftMaskProjectSettings.sensitivity))
                    {
                        SetSoftMaskDirty();
                    }

                    if (!_viewChangeTrigger && _rootCanvas && _rootCanvas.renderMode == RenderMode.WorldSpace)
                    {
                        UpdateCanvasViewChangeTrigger(CanvasViewChangeTrigger.Find(transform));
                        SetSoftMaskDirty();
                    }

                    break;
                }
                // AntiAliasing mode: Update anti-aliasing for graphic.
                case MaskingMode.AntiAliasing:
                {
                    if (!this || !graphic) return;
                    Utils.UpdateAntiAlias(graphic, isActiveAndEnabled, antiAliasingThreshold);
                    break;
                }
            }
        }

        private void UpdateCanvasViewChangeTrigger(CanvasViewChangeTrigger trigger)
        {
            if (_viewChangeTrigger != trigger)
            {
                Logging.Log(this, $"UpdateCanvasViewChangeTrigger: {_viewChangeTrigger} -> {trigger}");

                if (_viewChangeTrigger)
                {
                    _viewChangeTrigger.onViewChange -= _setSoftMaskDirty ??= SetSoftMaskDirty;
                }

                if (trigger)
                {
                    trigger.onViewChange += _setSoftMaskDirty ??= SetSoftMaskDirty;
                }
            }

            _viewChangeTrigger = trigger;
        }

        public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (FrameCache.TryGet(this, nameof(IsRaycastLocationValid), out bool valid))
            {
                return valid;
            }

            if (!isActiveAndEnabled)
            {
                FrameCache.Set(this, nameof(IsRaycastLocationValid), true);
                return true;
            }

            // Check parent
            if (_parent && !_parent.IsRaycastLocationValid(sp, eventCamera))
            {
                FrameCache.Set(this, nameof(IsRaycastLocationValid), false);
                return false;
            }

            Profiler.BeginSample("(SM4UI)[SoftMask] IsRaycastLocationValid > Base");
            valid = base.IsRaycastLocationValid(sp, eventCamera);
            Profiler.EndSample();

            if (!SoftMaskingEnabled())
            {
                FrameCache.Set(this, nameof(IsRaycastLocationValid), valid);
                return valid;
            }

            if (valid && alphaHitTest)
            {
                Profiler.BeginSample("(SM4UI)[SoftMask] IsRaycastLocationValid > Alpha hit test");
                valid = Utils.AlphaHitTestValid(graphic, sp, eventCamera, 0.01f);
                Profiler.EndSample();
            }

            if (_shapeContainer)
            {
                Profiler.BeginSample("(SM4UI)[SoftMask] IsRaycastLocationValid > Shapes");
                valid |= _shapeContainer.IsInside(sp, eventCamera, false, 0.5f);
                Profiler.EndSample();
            }

            FrameCache.Set(this, nameof(IsRaycastLocationValid), valid);
            return valid;
        }

        public override Material GetModifiedMaterial(Material baseMaterial)
        {
            if (SoftMaskingEnabled())
            {
                return showMaskGraphic ? baseMaterial : null;
            }

            return base.GetModifiedMaterial(baseMaterial);
        }

        private void SetDirtyAndNotify()
        {
            SetSoftMaskDirty();
            MaskUtilities.NotifyStencilStateChanged(this);
        }

        public void SetSoftMaskDirty()
        {
            if (isDirty) return;

            Logging.LogIf(!isDirty, this, $"! SetSoftMaskDirty {GetInstanceID()}");
            isDirty = true;
            for (var i = _children.Count - 1; i >= 0; i--)
            {
                if (_children[i])
                {
                    _children[i].SetSoftMaskDirty();
                }
                else
                {
                    _children.RemoveAt(i);
                }
            }
        }

        public bool SoftMaskingEnabled()
        {
            return GetActualMaskingMode() == MaskingMode.SoftMasking && MaskEnabled();
        }

        public bool AntiAliasingEnabled()
        {
            return GetActualMaskingMode() == MaskingMode.AntiAliasing && MaskEnabled();
        }

        internal MaskingMode GetActualMaskingMode()
        {
            return maskingMode == MaskingMode.Normal
                ? MaskingMode.Normal
                : UISoftMaskProjectSettings.softMaskEnabled && maskingMode == MaskingMode.SoftMasking
                    ? MaskingMode.SoftMasking
                    : MaskingMode.AntiAliasing;
        }

        private void UpdateParentSoftMask()
        {
            if (SoftMaskingEnabled())
            {
                var stopAfter = MaskUtilities.FindRootSortOverrideCanvas(transform);
                var parentSoftMask =
                    transform.GetComponentInParent<SoftMask>(false, stopAfter, x => x.SoftMaskingEnabled());
                UpdateParentSoftMask(parentSoftMask);
            }
            else
            {
                UpdateParentSoftMask(null);
            }
        }

        private void UpdateParentSoftMask(SoftMask newParent)
        {
            if (_parent && _parent._children.Contains(this))
            {
                _parent._children.Remove(this);
            }

            if (newParent && !newParent._children.Contains(this))
            {
                newParent._children.Add(this);
            }

            if (_parent != newParent)
            {
                SetSoftMaskDirty();
            }

            _parent = newParent;
        }

        private void UpdateAntiAlias()
        {
            var useAA = isActiveAndEnabled && maskingMode == MaskingMode.AntiAliasing;
            Utils.UpdateAntiAlias(graphic, useAA, antiAliasingThreshold);
        }

        private bool IsInScreen()
        {
            if (graphic && graphic.IsInScreen()) return true;
            if (_shapeContainer && _shapeContainer.IsInScreen()) return true;

            return false;
        }

        private void RenderSoftMaskBuffer()
        {
            if (!SoftMaskingEnabled()) return;

            if (FrameCache.TryGet(this, nameof(RenderSoftMaskBuffer), out bool _)) return;
            FrameCache.Set(this, nameof(RenderSoftMaskBuffer), true);

            if (!isDirty) return;
            isDirty = false;

            if (_parent)
            {
                _parent.RenderSoftMaskBuffer();
            }

            var depth = softMaskDepth;
            if (depth < 0 || 4 <= depth) return;

            if (_cb == null)
            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Rent cb");
                _cb = SoftMaskUtils.commandBufferPool.Rent();
                _cb.name = "[SoftMask] SoftMaskBuffer";
                Profiler.EndSample();
            }

            if (_mpb == null)
            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Rent mpb");
                _mpb = SoftMaskUtils.materialPropertyBlockPool.Rent();
                _mpb.Clear();
                Profiler.EndSample();
            }

            if (!IsInScreen())
            {
                if (_hasSoftMaskBufferDrawn)
                {
                    Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Clear");

                    _cb.Clear();
                    _cb.SetRenderTarget(softMaskBuffer);
                    if (softMaskDepth == 0)
                    {
                        _cb.ClearRenderTarget(true, true, clearColor);
                    }

                    Graphics.ExecuteCommandBuffer(_cb);
                    Profiler.EndSample();
                }

                _hasSoftMaskBufferDrawn = false;
                return;
            }

            Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Init command buffer");
            _cb.Clear();
            _cb.SetRenderTarget(softMaskBuffer);
            if (softMaskDepth == 0)
            {
                _cb.ClearRenderTarget(true, true, clearColor);
            }

            Profiler.EndSample();

            var eyes = graphic.canvas.IsStereoCanvas() ? s_StereoEyes : s_MonoEyes;
            for (var i = 0; i < eyes.Length; i++)
            {
                RenderSoftMaskBuffer(eyes[i]);
            }

            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Execute command buffer");
                Graphics.ExecuteCommandBuffer(_cb);
                _hasSoftMaskBufferDrawn = true;
                Logging.Log(this, $" >>>> SoftMaskBuffer '{softMaskBuffer.name}' will render.");
                Profiler.EndSample();
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.QueuePlayerLoopUpdate();
            }
#endif

            onRenderSoftMaskBuffer?.Invoke(this);
        }

        private void RenderSoftMaskBuffer(Camera.MonoOrStereoscopicEye eye)
        {
            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > SetVpMatricesCommandBuffer");
                graphic.canvas.rootCanvas.GetViewProjectionMatrix(eye, out var viewMatrix, out var projectionMatrix);
                _cb.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                Profiler.EndSample();

                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > ApplyMaterialPropertyBlock");
                SoftMaskUtils.ApplyMaterialPropertyBlock(_mpb, softMaskDepth, graphic.mainTexture, softMaskingRange);
                Profiler.EndSample();
            }

            if (eye != Camera.MonoOrStereoscopicEye.Right && _parent)
            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Copy texture from parent");
                if (_parent.softMaskBuffer)
                {
                    _cb.CopyTexture(_parent.softMaskBuffer, softMaskBuffer);
                }

                Profiler.EndSample();
            }

            if (eye != Camera.MonoOrStereoscopicEye.Mono)
            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Set viewport");
                var w = softMaskBuffer.width * 0.5f;
                var h = softMaskBuffer.height;
                _cb.SetViewport(new Rect(w * (int)eye, 0f, w, h));
                Profiler.EndSample();
            }

            if (_mesh)
            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Draw mesh");
                var softMaterial = SoftMaskUtils.GetSoftMaskingMaterial(MaskingShape.MaskingMethod.Additive);
                _cb.DrawMesh(_mesh, transform.localToWorldMatrix, softMaterial, 0, 0, _mpb);
                Profiler.EndSample();
            }

            if (_shapeContainer)
            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Draw shapes");
                _shapeContainer.DrawSoftMaskBuffer(_cb, softMaskDepth);
                Profiler.EndSample();
            }
        }
    }
}
