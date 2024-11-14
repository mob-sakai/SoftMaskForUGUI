using System;
using System.Collections.Generic;
using Coffee.UISoftMaskInternal;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
#if URP_ENABLE
using UnityEngine.Rendering.Universal;
#endif
using UnityEngine.UI;

namespace Coffee.UISoftMask
{
    /// <summary>
    /// SoftMask.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class SoftMask : Mask, IMeshModifier, IMaskable, IMaskingShapeContainerOwner, ISerializationCallbackReceiver
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

        [Tooltip("Masking mode.\n\n" +
                 "SoftMasking: Use RenderTexture as a soft mask buffer. The alpha of the masking graphic can be used.\n" +
                 "AntiAliasing: Suppress the jaggedness of the masking graphic. The masking graphic cannot be displayed.\n" +
                 "Normal: Same as Mask component's stencil mask.")]
        [SerializeField]
        private MaskingMode m_MaskingMode = MaskingMode.SoftMasking;

        [Tooltip("The transparent part of the mask cannot be clicked.\n" +
                 "This can be achieved by enabling Read/Write enabled in the Texture Import Settings for the texture.\n\n" +
                 "NOTE: Enable this only if necessary, as it will require more graphics memory and processing time.")]
        [SerializeField]
        private bool m_AlphaHitTest;

        [Tooltip("The minimum and maximum alpha values used for soft masking.\n" +
                 "The larger the gap between these values, the stronger the softness effect.")]
        [SerializeField]
        private MinMax01 m_SoftnessRange = new MinMax01(0, 1f);

        [Tooltip("The down sampling rate for soft mask buffer.\n" +
                 "The higher this value, the lower the quality of the soft masking, but the performance will improve.")]
        [SerializeField]
        private DownSamplingRate m_DownSamplingRate = DownSamplingRate.x1;

        [Tooltip("The threshold for anti-alias masking.\n" +
                 "The smaller this value, the less jagged it is.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float m_AntiAliasingThreshold;

        [SerializeField]
        [Obsolete]
        internal float m_Alpha = -1;

        [SerializeField]
        [Obsolete]
        internal float m_Softness = -1;

        [SerializeField]
        [Obsolete]
        private bool m_PartOfParent;

        private CanvasGroup _canvasGroup;
        private CommandBuffer _cb;
        private List<SoftMask> _children;
        private bool _hasResolutionChanged;
        private bool _hasSoftMaskBufferDrawn;
        private Mesh _mesh;
        private MaterialPropertyBlock _mpb;
        private Action _onBeforeCanvasRebuild;
        private Action _onCanvasViewChanged;
        private SoftMask _parent;
        private Matrix4x4 _prevTransformMatrix;
        private Action _renderSoftMaskBuffer;
        private Canvas _rootCanvas;
        private UnityAction _setSoftMaskDirty;
        private MaskingShapeContainer _shapeContainer;
        internal RenderTexture _softMaskBuffer;
        private UnityAction _updateParentSoftMask;
        private CanvasViewChangeTrigger _viewChangeTrigger;

        private List<SoftMask> children =>
            _children != null ? _children : _children = UISoftMaskInternal.ListPool<SoftMask>.Rent();

        /// <summary>
        /// Masking mode.<br />
        /// <b>SoftMasking</b>: Use RenderTexture as a soft mask buffer. The alpha of the masking graphic can be used.<br />
        /// <b>AntiAliasing</b>: Suppress the jaggedness of the masking graphic. The masking graphic cannot be displayed.<br />
        /// <b>Normal</b>: Same as Mask component's stencil mask.
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

                if (graphic)
                {
                    graphic.SetMaterialDirty();
                }
            }
        }

        public DownSamplingRate downSamplingRate
        {
            get => m_DownSamplingRate;
            set
            {
                if (m_DownSamplingRate == value) return;
                m_DownSamplingRate = value;
                SetDirtyAndNotify();
            }
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
        [Obsolete("Use MaskingShape component instead.", false)]
        public bool partOfParent
        {
            get => m_PartOfParent;
            set => m_PartOfParent = value;
        }

        /// <summary>
        /// The value used by the soft mask to select the area of influence defined over the soft mask's graphic.
        /// </summary>
        [Obsolete("Use softnessRange instead.", false)]
        public float softness
        {
            get => softnessRange.max;
            set
            {
                softnessRange = new MinMax01(0, Mathf.Clamp01(value));
                m_Softness = -1;
            }
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
                    var size = RenderTextureRepository.GetScreenSize((int)downSamplingRate);
                    var hash = new Hash128((uint)GetInstanceID(), (uint)size.x, (uint)size.y, 0);
                    if (!RenderTextureRepository.Valid(hash, _softMaskBuffer))
                    {
                        RenderTextureRepository.Get(hash, ref _softMaskBuffer,
                            x => new RenderTexture(RenderTextureRepository.GetDescriptor(x, false))
                            {
                                hideFlags = HideFlags.DontSave
                            }, size);
                    }

                    return _softMaskBuffer;
                }

                RenderTextureRepository.Release(ref _softMaskBuffer);
                return null;
            }
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
                SetSoftMaskDirty();
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

                isDirty = true;
                if (graphic)
                {
                    var color = graphic.color;
                    color.a = value;
                    graphic.color = color;
                }
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

        public bool allowRenderScale
        {
            get
            {
#if URP_ENABLE
                if (_rootCanvas
                    && _rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                    && _rootCanvas.worldCamera
                    && GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset)
                {
                    return true;
                }

#endif
                return false;
            }
        }

        public bool allowDynamicResolution
        {
            get
            {
                var isSupported =
#if UNITY_XBOXONE || UNITY_PS5 || UNITY_PS4 || UNITY_SWITCH || UNITY_IOS || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                    true;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_TVOS
                    SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal;
#elif UNITY_ANDROID
                    SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan;
#elif UNITY_WSA
                    SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12;
#else
                    false;
#endif

                return isSupported
                       && _rootCanvas
                       && _rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                       && _rootCanvas.worldCamera
                       && _rootCanvas.worldCamera.allowDynamicResolution;
            }
        }

        /// <summary>
        /// Called when the component is enabled.
        /// </summary>
        protected override void OnEnable()
        {
            UIExtraCallbacks.onBeforeCanvasRebuild +=
                _onBeforeCanvasRebuild ?? (_onBeforeCanvasRebuild = OnBeforeCanvasRebuild);
            UIExtraCallbacks.onAfterCanvasRebuild +=
                _renderSoftMaskBuffer ?? (_renderSoftMaskBuffer = RenderSoftMaskBuffer);

            if (graphic)
            {
                graphic.RegisterDirtyMaterialCallback(
                    _updateParentSoftMask ?? (_updateParentSoftMask = UpdateParentSoftMask));
                graphic.RegisterDirtyVerticesCallback(
                    _setSoftMaskDirty ?? (_setSoftMaskDirty = SetSoftMaskDirty));
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
            UIExtraCallbacks.onBeforeCanvasRebuild -=
                _onBeforeCanvasRebuild ?? (_onBeforeCanvasRebuild = OnBeforeCanvasRebuild);
            UIExtraCallbacks.onAfterCanvasRebuild -=
                _renderSoftMaskBuffer ?? (_renderSoftMaskBuffer = RenderSoftMaskBuffer);

            if (graphic)
            {
                graphic.UnregisterDirtyMaterialCallback(
                    _updateParentSoftMask ?? (_updateParentSoftMask = UpdateParentSoftMask));
                graphic.UnregisterDirtyVerticesCallback(
                    _setSoftMaskDirty ?? (_setSoftMaskDirty = SetSoftMaskDirty));
                graphic.SetVerticesDirty();
            }

            UpdateParentSoftMask(null);
            children.Clear();

            MeshExtensions.Return(ref _mesh);
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
            UISoftMaskInternal.ListPool<SoftMask>.Return(ref _children);
            _onBeforeCanvasRebuild = null;
            _renderSoftMaskBuffer = null;
            _setSoftMaskDirty = null;
            _onCanvasViewChanged = null;
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
            SetDirtyAndNotifyIfBufferSizeChanged();
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
            SetSoftMaskDirty();
            UpdateAntiAlias();

            if (graphic)
            {
                graphic.SetVerticesDirty();
            }

            var list = UISoftMaskInternal.ListPool<IMaskable>.Rent();
            GetComponents(list);
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] == null) continue;

                list[i].RecalculateMasking();
            }

            UISoftMaskInternal.ListPool<IMaskable>.Return(ref list);
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
                MeshExtensions.Return(ref _mesh);
                return;
            }

            Profiler.BeginSample("(SM4UI)[SoftMask] ModifyMesh");
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

        private void SetDirtyAndNotifyIfBufferSizeChanged()
        {
            if (!SoftMaskingEnabled() || !_softMaskBuffer) return;

            Logging.Log(this, "SetDirtyAndNotifyIfBufferSizeChanged");
            var size = RenderTextureRepository.GetScreenSize((int)downSamplingRate);
            var hash = new Hash128((uint)GetInstanceID(), (uint)size.x, (uint)size.y, 0);
            if (RenderTextureRepository.Valid(hash, _softMaskBuffer)) return;

            // If the size of the soft mask buffer is changed, reset the SoftMaskable.
            SetDirtyAndNotify();
        }

        private void AddSoftMaskableOnChildren()
        {
            if (!isActiveAndEnabled || !SoftMaskingEnabled()) return;

            this.AddComponentOnChildren<SoftMaskable>(true);
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

                    if (!_viewChangeTrigger && _rootCanvas)
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
                    _viewChangeTrigger.onCanvasViewChanged -=
                        _onCanvasViewChanged ?? (_onCanvasViewChanged = OnCanvasViewChanged);
                }

                if (trigger)
                {
                    trigger.onCanvasViewChanged +=
                        _onCanvasViewChanged ?? (_onCanvasViewChanged = OnCanvasViewChanged);
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
            if (!isActiveAndEnabled || !graphic || !graphic.canvas) return baseMaterial;

            return base.GetModifiedMaterial(baseMaterial);
        }

        private void SetDirtyAndNotify()
        {
            if (!this || !isActiveAndEnabled) return;

            SetSoftMaskDirty();
            MaskUtilities.NotifyStencilStateChanged(this);
        }

        private void OnCanvasViewChanged()
        {
            _hasResolutionChanged = true;
            SetDirtyAndNotify();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.QueuePlayerLoopUpdate();
            }
#endif
        }

        public void SetSoftMaskDirty()
        {
            if (isDirty || !this || !isActiveAndEnabled) return;

            Logging.LogIf(!isDirty, this, $"! SetSoftMaskDirty {GetInstanceID()}");
            isDirty = true;
            for (var i = children.Count - 1; i >= 0; i--)
            {
                if (children[i])
                {
                    children[i].SetSoftMaskDirty();
                }
                else
                {
                    children.RemoveAt(i);
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
            if (_parent && _parent.children.Contains(this))
            {
                _parent.children.Remove(this);
            }

            if (newParent && !newParent.children.Contains(this))
            {
                newParent.children.Add(this);
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
            if (!SoftMaskingEnabled() || !graphic || !graphic.canvas) return;

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
                if (_hasSoftMaskBufferDrawn || _hasResolutionChanged)
                {
                    Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Clear");
                    _cb.Clear();
                    _cb.SetRenderTarget(softMaskBuffer);
                    _cb.ClearRenderTarget(true, true, clearColor);
                    Graphics.ExecuteCommandBuffer(_cb);
                    Profiler.EndSample();
                }

                _hasSoftMaskBufferDrawn = false;
                _hasResolutionChanged = false;
                return;
            }

            Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Init command buffer");
            _cb.Clear();
            _cb.SetRenderTarget(softMaskBuffer);
            if (softMaskDepth == 0 || _hasResolutionChanged)
            {
                _cb.ClearRenderTarget(true, true, clearColor);
            }

            Profiler.EndSample();

            var eyes = UISoftMaskProjectSettings.stereoEnabled && graphic.canvas.IsStereoCanvas()
                ? s_StereoEyes
                : s_MonoEyes;
            for (var i = 0; i < eyes.Length; i++)
            {
                RenderSoftMaskBuffer(_cb, eyes[i]);
            }

            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Execute command buffer");
                Graphics.ExecuteCommandBuffer(_cb);
                _hasSoftMaskBufferDrawn = true;
                _hasResolutionChanged = false;
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

        private void RenderSoftMaskBuffer(CommandBuffer cb, Camera.MonoOrStereoscopicEye eye)
        {
            if (_hasResolutionChanged)
            {
                var p = _parent;
                while (p)
                {
                    p.RenderSoftMaskBuffer(cb, eye);
                    p = p._parent;
                }
            }

            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > SetVpMatricesCommandBuffer");
                graphic.canvas.rootCanvas.GetViewProjectionMatrix(eye, out var viewMatrix, out var projectionMatrix);
                cb.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                Profiler.EndSample();

                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > ApplyMaterialPropertyBlock");
                var texture = graphic.mainTexture;
                SoftMaskUtils.ApplyMaterialPropertyBlock(_mpb, softMaskDepth, texture, softnessRange, alpha);
                Profiler.EndSample();
            }

            if (!_hasResolutionChanged && eye != Camera.MonoOrStereoscopicEye.Right && _parent)
            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Copy texture from parent");
                if (_parent.softMaskBuffer)
                {
                    _cb.Blit(_parent.softMaskBuffer, softMaskBuffer);
                }

                Profiler.EndSample();
            }

            if (eye != Camera.MonoOrStereoscopicEye.Mono)
            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Set viewport");
                var w = softMaskBuffer.width * 0.5f;
                var h = softMaskBuffer.height;
                cb.SetViewport(new Rect(w * (int)eye, 0f, w, h));
                Profiler.EndSample();
            }

            var mesh = _mesh;
            if (mesh)
            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Draw mesh");
                var softMaterial = SoftMaskUtils.GetSoftMaskingMaterial(MaskingShape.MaskingMethod.Additive);
                cb.DrawMesh(mesh, transform.localToWorldMatrix, softMaterial, 0, 0, _mpb);
                Profiler.EndSample();
            }

            if (_shapeContainer)
            {
                Profiler.BeginSample("(SM4UI)[SoftMask] RenderSoftMaskBuffer > Draw shapes");
                _shapeContainer.DrawSoftMaskBuffer(cb, softMaskDepth);
                Profiler.EndSample();
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#pragma warning disable CS0612, CS0618 // Type or member is obsolete
            if (0 <= m_Softness)
            {
                m_SoftnessRange = new MinMax01(0, Mathf.Clamp01(m_Softness));
                m_Softness = -1;
            }

            if (m_PartOfParent)
            {
                Debug.LogWarning(
                    $"[SoftMask] The 'partOfParent' property is obsolete. Use MaskingShape component instead.", this);
            }
#pragma warning restore CS0612, CS0618 // Type or member is obsolete
        }
    }
}
