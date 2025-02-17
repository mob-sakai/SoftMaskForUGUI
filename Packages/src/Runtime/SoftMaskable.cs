using System;
using Coffee.UISoftMaskInternal;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Coffee.UISoftMask
{
    [ExecuteAlways]
    [Icon("Packages/com.coffee.softmask-for-ugui/Icons/SoftMaskIcon.png")]
    public class SoftMaskable : MonoBehaviour, IMaterialModifier, IMaskable
    {
#if UNITY_EDITOR
        private static readonly int s_AlphaClipThreshold = Shader.PropertyToID("_AlphaClipThreshold");
        private static readonly int s_MaskingShapeSubtract = Shader.PropertyToID("_MaskingShapeSubtract");
#endif
        private static readonly int s_AllowDynamicResolution = Shader.PropertyToID("_AllowDynamicResolution");
        private static readonly int s_AllowRenderScale = Shader.PropertyToID("_AllowRenderScale");
        private static readonly int s_SoftMaskingPower = Shader.PropertyToID("_SoftMaskingPower");
        private const float k_PowerMin = 0.5f;
        private const float k_PowerMax = 5f;

        [Tooltip("The graphic is ignored when soft-masking.")]
        [SerializeField]
        private bool m_IgnoreSelf;

        [Tooltip("The child graphics are ignored when soft-masking.")]
        [SerializeField]
        private bool m_IgnoreChildren;

        [Tooltip("Soft masking power.\n" +
                 "The higher this value, the faster it becomes transparent.\n" +
                 "If overlapping objects appear see-through, please adjust this value.")]
        [SerializeField]
        [PowerRange(k_PowerMin, k_PowerMax, 10)]
        private float m_Power = 1f;

        /// <summary>
        /// The graphic is ignored when soft-masking.
        /// </summary>
        public bool ignoreSelf
        {
            get => m_IgnoreSelf;
            set
            {
                if (m_IgnoreSelf == value) return;
                m_IgnoreSelf = value;
                UpdateHideFlags();
                SetMaterialDirty();
            }
        }

        /// <summary>
        /// The child graphics are ignored when soft-masking.
        /// </summary>
        public bool ignoreChildren
        {
            get => m_IgnoreChildren;
            set
            {
                if (m_IgnoreChildren == value) return;
                m_IgnoreChildren = value;
                UpdateHideFlags();
                SetMaterialDirtyForChildren();
            }
        }

        /// <summary>
        /// The graphic is ignored when soft-masking.
        /// </summary>
        public bool ignored
        {
            get
            {
                if (m_IgnoreSelf) return true;

                RecalculateStencilIfNeeded();
                if (!_softMask || !_softMask.isActiveAndEnabled) return true;

                var tr = transform.parent;
                while (tr)
                {
                    if (tr.TryGetComponent<SoftMaskable>(out var parent) && parent.ignoreChildren)
                    {
                        return true;
                    }

                    tr = tr.parent;
                }

                return false;
            }
        }

        /// <summary>
        /// Soft masking power.
        /// The higher this value, the faster it becomes transparent.
        /// If overlapping objects appear see-through, please adjust this value.
        /// </summary>
        public float power
        {
            get => m_Power;
            set
            {
                value = Mathf.Clamp(value, k_PowerMin, k_PowerMax);
                if (Mathf.Approximately(m_Power, value)) return;
                m_Power = value;
                SetMaterialDirty();
            }
        }

        private Action _checkGraphic;
        private MaskableGraphic _graphic;
        private Material _maskableMaterial;
        private bool _shouldRecalculateStencil;
        private Mask _mask;
        private SoftMask _softMask;
        private int _softMaskDepth;
        private int _stencilBits;

        private bool isTerminal => _graphic is TerminalMaskingShape;

        private void OnEnable()
        {
            UpdateHideFlags();
            this.AddComponentOnChildren<SoftMaskable>(false);
            _shouldRecalculateStencil = true;
            if (TryGetComponent(out _graphic))
            {
                // Check if the graphic is before this component.
                var components = InternalListPool<Component>.Rent();
                GetComponents(components);
                var gIndex = components.IndexOf(_graphic);
                var sIndex = components.IndexOf(this);
                InternalListPool<Component>.Return(ref components);
                if (sIndex < gIndex)
                {
                    gameObject.AddComponent<SoftMaskable>();
                    Misc.Destroy(this);
                    return;
                }

                _graphic.SetMaterialDirty();
            }
            else
            {
                UIExtraCallbacks.onBeforeCanvasRebuild += _checkGraphic ?? (_checkGraphic = CheckGraphic);
            }

#if UNITY_EDITOR
            UIExtraCallbacks.onAfterCanvasRebuild +=
                _updateSceneViewMatrix ?? (_updateSceneViewMatrix = UpdateSceneViewMatrix);
#endif
        }

        private void OnDisable()
        {
            UIExtraCallbacks.onBeforeCanvasRebuild -= _checkGraphic ?? (_checkGraphic = CheckGraphic);
            if (_graphic)
            {
                _graphic.SetMaterialDirty();
            }

            _graphic = null;
            _mask = null;
            _softMask = null;
            MaterialRepository.Release(ref _maskableMaterial);

#if UNITY_EDITOR
            UIExtraCallbacks.onAfterCanvasRebuild -=
                _updateSceneViewMatrix ?? (_updateSceneViewMatrix = UpdateSceneViewMatrix);
#endif
        }

        private void OnDestroy()
        {
            _graphic = null;
            _maskableMaterial = null;
            _mask = null;
            _softMask = null;
            _checkGraphic = null;

#if UNITY_EDITOR
            _updateSceneViewMatrix = null;
#endif
        }

        private void OnTransformChildrenChanged()
        {
            this.AddComponentOnChildren<SoftMaskable>(false);
        }

        private void OnTransformParentChanged()
        {
            _shouldRecalculateStencil = true;
        }

        void IMaskable.RecalculateMasking()
        {
            _shouldRecalculateStencil = true;
        }

        Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial)
        {
            if (!UISoftMaskProjectSettings.softMaskEnabled)
            {
                MaterialRepository.Release(ref _maskableMaterial);
                return baseMaterial;
            }

            if (!isActiveAndEnabled || !_graphic || !_graphic.canvas || !_graphic.maskable || isTerminal ||
                baseMaterial == null || ignored)
            {
                MaterialRepository.Release(ref _maskableMaterial);
                return baseMaterial;
            }

            RecalculateStencilIfNeeded();
            var softMaskDepth = _softMask ? _softMask.softMaskDepth : -1;
            if (softMaskDepth < 0 || 4 <= softMaskDepth)
            {
                _softMaskDepth = -1;
                MaterialRepository.Release(ref _maskableMaterial);
                return baseMaterial;
            }

            _softMaskDepth = softMaskDepth;
            if (0 <= _softMaskDepth
                && _softMask == _mask
                && _softMask.SoftMaskingEnabled()
                && TryGetComponent<MaskingShape>(out var shape)
                && shape.isActiveAndEnabled
                && shape.maskingMethod == MaskingShape.MaskingMethod.Subtract)
            {
                _softMaskDepth -= 1;
            }

            Profiler.BeginSample("(SM4UI)[SoftMaskable] GetModifiedMaterial");
            var isStereo = UISoftMaskProjectSettings.stereoEnabled && _graphic.canvas.IsStereoCanvas();
            var useStencil = UISoftMaskProjectSettings.useStencilOutsideScreen;
            var localId = (uint)(Mathf.InverseLerp(k_PowerMin, k_PowerMax, power) * (1 << 10));
#if UNITY_EDITOR
            var threshold = 0f;
            var subtract = false;
            if (useStencil)
            {
                if (TryGetComponent(out MaskingShape s) && s.maskingMethod == MaskingShape.MaskingMethod.Subtract)
                {
                    threshold = s.softnessRange.average;
                    subtract = true;
                }
                else
                {
                    threshold = _softMask.softnessRange.average;
                }
            }

            localId = (uint)(Mathf.Clamp01(threshold) * (1 << 8) + (subtract ? 1 << 9 : 0) + (localId << 10));
#endif
            var hash = new Hash128(
                (uint)baseMaterial.GetInstanceID(),
                (uint)_softMask.softMaskBuffer.GetInstanceID(),
                (uint)(_stencilBits + (isStereo ? 1 << 8 : 0) + (useStencil ? 1 << 9 : 0) + (_softMaskDepth << 10)),
                localId);
            MaterialRepository.Get(hash, ref _maskableMaterial,
                x => SoftMaskUtils.CreateSoftMaskable(x.baseMaterial, x.softMaskBuffer, x._softMaskDepth,
                    x._stencilBits, x.isStereo),
                (baseMaterial, _softMask.softMaskBuffer, _softMaskDepth, _stencilBits, isStereo));
            Profiler.EndSample();

#if UNITY_EDITOR
            _maskableMaterial.SetFloat(s_AlphaClipThreshold, threshold);
            _maskableMaterial.SetInt(s_MaskingShapeSubtract, subtract ? 1 : 0);
#endif
            _maskableMaterial.SetInt(s_AllowDynamicResolution, _softMask.allowDynamicResolution ? 1 : 0);
            _maskableMaterial.SetInt(s_AllowRenderScale, _softMask.allowRenderScale ? 1 : 0);
            _maskableMaterial.SetFloat(s_SoftMaskingPower, power);
            return _maskableMaterial;
        }

        private void RecalculateStencilIfNeeded()
        {
            if (!isActiveAndEnabled)
            {
                _softMask = null;
                _stencilBits = 0;
                return;
            }

            if (!_shouldRecalculateStencil) return;
            _shouldRecalculateStencil = false;
            var useStencil = UISoftMaskProjectSettings.useStencilOutsideScreen;
            _stencilBits = Utils.GetStencilBits(transform, false, useStencil, out _mask, out _softMask);
        }

        private void CheckGraphic()
        {
            if (_graphic || !TryGetComponent(out _graphic)) return;

            UIExtraCallbacks.onBeforeCanvasRebuild -= _checkGraphic ?? (_checkGraphic = CheckGraphic);
            gameObject.AddComponent<SoftMaskable>();
            Misc.Destroy(this);
        }

        public void SetMaterialDirty()
        {
            if (!isActiveAndEnabled || !_graphic) return;
            _graphic.SetMaterialDirty();
            Misc.QueuePlayerLoopUpdate();
        }

        public void SetMaterialDirtyForChildren()
        {
            if (!isActiveAndEnabled || !_graphic) return;

            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent<SoftMaskable>(out var child))
                {
                    child.SetMaterialDirty();
                    child.SetMaterialDirtyForChildren();
                }
            }
        }

        private void UpdateHideFlags()
        {
            hideFlags = ignoreSelf || ignoreChildren || !Mathf.Approximately(power, 1f)
                ? HideFlags.None
                : HideFlags.DontSaveInEditor;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateHideFlags();
        }

        private Action _updateSceneViewMatrix;
        private static readonly int s_GameVp = Shader.PropertyToID("_GameVP");
        private static readonly int s_GameTvp = Shader.PropertyToID("_GameTVP");
        private static readonly int s_GameVp2 = Shader.PropertyToID("_GameVP_2");
        private static readonly int s_GameTvp2 = Shader.PropertyToID("_GameTVP_2");
        private void UpdateSceneViewMatrix()
        {
            if (!_graphic || !_graphic.canvas || !_maskableMaterial) return;

            var mat = _graphic.GetMaterialForRendering();
            if (!mat || FrameCache.TryGet(mat, nameof(UpdateSceneViewMatrix), out bool _))
            {
                return;
            }

            var canvas = _graphic.canvas.rootCanvas;
            var isStereo = UISoftMaskProjectSettings.stereoEnabled && canvas.IsStereoCanvas();
            if (!FrameCache.TryGet(canvas, "GameVp", out Matrix4x4 gameVp) ||
                !FrameCache.TryGet(canvas, "GameTvp", out Matrix4x4 gameTvp))
            {
                Profiler.BeginSample("(SM4UI)[SoftMaskable] (Editor) UpdateSceneViewMatrix > Calc GameVp & GameTvp");
                var rt = canvas.transform as RectTransform;
                var cam = canvas.worldCamera;
                if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay && cam)
                {
                    var eye = isStereo ? Camera.MonoOrStereoscopicEye.Left : Camera.MonoOrStereoscopicEye.Mono;
                    canvas.GetViewProjectionMatrix(eye, out var vMatrix, out var pMatrix);
                    gameVp = gameTvp = pMatrix * vMatrix;
                }
                else if (rt != null)
                {
                    var pos = rt.position;
                    var scale = rt.localScale.x;
                    var size = rt.sizeDelta;
                    gameVp = Matrix4x4.TRS(new Vector3(0, 0, 0.5f), Quaternion.identity,
                        new Vector3(2 / size.x, 2 / size.y, 0.0005f * scale));
                    gameTvp = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity,
                        new Vector3(1 / pos.x, 1 / pos.y, -2 / 2000f)) * Matrix4x4.Translate(-pos);
                }
                else
                {
                    gameVp = gameTvp = Matrix4x4.identity;
                }

                FrameCache.Set(canvas, "GameVp", gameVp);
                FrameCache.Set(canvas, "GameTvp", gameTvp);
                Profiler.EndSample();
            }

            // Set view and projection matrices.
            Profiler.BeginSample("(SM4UI)[SoftMaskable] (Editor) UpdateSceneViewMatrix > Set matrices");
            mat.SetMatrix(s_GameVp, gameVp);
            mat.SetMatrix(s_GameTvp, gameTvp);
            Profiler.EndSample();

            // Calc Right eye matrices.
            if (isStereo)
            {
                if (!FrameCache.TryGet(canvas, "GameVp2", out gameVp))
                {
                    Profiler.BeginSample("(SM4UI)[SoftMaskable] (Editor) UpdateSceneViewMatrix > Calc GameVp2");
                    var eye = Camera.MonoOrStereoscopicEye.Right;
                    canvas.GetViewProjectionMatrix(eye, out var vMatrix, out var pMatrix);
                    gameVp = pMatrix * vMatrix;

                    FrameCache.Set(canvas, "GameVp2", gameVp);
                    Profiler.EndSample();
                }

                mat.SetMatrix(s_GameVp2, gameVp);
                mat.SetMatrix(s_GameTvp2, gameVp);
            }

            FrameCache.Set(mat, nameof(UpdateSceneViewMatrix), true);
        }
#endif
    }
}
