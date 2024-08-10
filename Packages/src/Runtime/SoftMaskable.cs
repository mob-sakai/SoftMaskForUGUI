using System;
using Coffee.UISoftMaskInternal;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Coffee.UISoftMask
{
    [ExecuteAlways]
    public class SoftMaskable : MonoBehaviour, IMaterialModifier, IMaskable
    {
        private static readonly int s_AlphaClipThreshold = Shader.PropertyToID("_AlphaClipThreshold");
        private Action _checkGraphic;
        private MaskableGraphic _graphic;
        private Material _maskableMaterial;
        private bool _shouldRecalculateStencil;
        private SoftMask _softMask;
        private int _softMaskDepth;
        private int _stencilBits;

#if UNITY_EDITOR
        private Action _updateSceneViewMatrix;
        private static readonly int s_GameVp = Shader.PropertyToID("_GameVP");
        private static readonly int s_GameTvp = Shader.PropertyToID("_GameTVP");
        private static readonly int s_GameVp2 = Shader.PropertyToID("_GameVP_2");
        private static readonly int s_GameTvp2 = Shader.PropertyToID("_GameTVP_2");
#endif

        private bool isTerminal => _graphic is TerminalMaskingShape;

        private void OnEnable()
        {
            hideFlags = UISoftMaskProjectSettings.hideFlagsForTemp;
            this.AddComponentOnChildren<SoftMaskable>(hideFlags, false);
            _shouldRecalculateStencil = true;
            if (TryGetComponent(out _graphic))
            {
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
            _softMask = null;
            _checkGraphic = null;

#if UNITY_EDITOR
            _updateSceneViewMatrix = null;
#endif
        }

        private void OnTransformChildrenChanged()
        {
            this.AddComponentOnChildren<SoftMaskable>(UISoftMaskProjectSettings.hideFlagsForTemp, false);
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
                baseMaterial == null)
            {
                MaterialRepository.Release(ref _maskableMaterial);
                return baseMaterial;
            }

            RecalculateStencilIfNeeded();
            _softMaskDepth = _softMask ? _softMask.softMaskDepth : -1;

            var useStencil = UISoftMaskProjectSettings.useStencilOutsideScreen;
            var localId = 0u;
#if UNITY_EDITOR
            if (useStencil)
            {
                if (_softMask && _softMaskDepth < 0)
                {
                    MaterialRepository.Release(ref _maskableMaterial);
                    return baseMaterial;
                }

                if (!_softMask && (!TryGetComponent(out _softMask) || !_softMask.SoftMaskingEnabled()))
                {
                    MaterialRepository.Release(ref _maskableMaterial);
                    return baseMaterial;
                }

                localId = (uint)GetInstanceID() + (UISoftMaskProjectSettings.useStencilOutsideScreen ? 1u : 0u);
            }
            else
#endif

            if (!_softMask || _softMaskDepth < 0 || 4 <= _softMaskDepth)
            {
                MaterialRepository.Release(ref _maskableMaterial);
                return baseMaterial;
            }

            Profiler.BeginSample("(SM4UI)[SoftMaskable] GetModifiedMaterial");

            var isStereo = Application.isPlaying && _graphic.canvas.IsStereoCanvas();
            var hash = new Hash128(
                (uint)baseMaterial.GetInstanceID(),
                (uint)_softMask.softMaskBuffer.GetInstanceID(),
                (uint)_stencilBits + (isStereo ? 1 << 8 : 0u) + ((uint)_softMaskDepth << 9),
                localId);
            MaterialRepository.Get(hash, ref _maskableMaterial,
                x => SoftMaskUtils.CreateSoftMaskable(x.baseMaterial, x.softMaskBuffer, x._softMaskDepth,
                    x._stencilBits, x.isStereo, UISoftMaskProjectSettings.fallbackBehavior),
                (baseMaterial, _softMask.softMaskBuffer, _softMaskDepth, _stencilBits, isStereo));
            Profiler.EndSample();

#if UNITY_EDITOR
            var threshold = 0f;
            if (useStencil)
            {
                if (TryGetComponent(out MaskingShape s) && s.maskingMethod == MaskingShape.MaskingMethod.Subtract)
                {
                    threshold = s.softnessRange.average;
                }
                else if (_softMask)
                {
                    threshold = _softMask.softnessRange.average;
                }
            }

            _maskableMaterial.SetFloat(s_AlphaClipThreshold, threshold);
#endif

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
            _stencilBits = Utils.GetStencilBits(transform, false, useStencil, out var _, out _softMask);
        }

        private void CheckGraphic()
        {
            if (_graphic || !TryGetComponent(out _graphic)) return;

            UIExtraCallbacks.onBeforeCanvasRebuild -= _checkGraphic ?? (_checkGraphic = CheckGraphic);
            gameObject.AddComponent<SoftMaskable>();
            Misc.Destroy(this);
        }

#if UNITY_EDITOR
        private void UpdateSceneViewMatrix()
        {
            if (!_graphic || !_graphic.canvas || !_maskableMaterial) return;
            if (FrameCache.TryGet(_maskableMaterial, nameof(UpdateSceneViewMatrix), out bool _))
            {
                return;
            }

            var canvas = _graphic.canvas.rootCanvas;
            if (!FrameCache.TryGet(canvas, "GameVp", out Matrix4x4 gameVp) ||
                !FrameCache.TryGet(canvas, "GameTvp", out Matrix4x4 gameTvp))
            {
                Profiler.BeginSample("(SM4UI)[SoftMaskable] (Editor) UpdateSceneViewMatrix > Calc GameVp & GameTvp");
                var rt = canvas.transform as RectTransform;
                var cam = canvas.worldCamera;
                if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay && cam)
                {
                    var eye = canvas.IsStereoCanvas()
                        ? Camera.MonoOrStereoscopicEye.Left
                        : Camera.MonoOrStereoscopicEye.Mono;
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
            _maskableMaterial.SetMatrix(s_GameVp, gameVp);
            _maskableMaterial.SetMatrix(s_GameTvp, gameTvp);
            Profiler.EndSample();

            // Calc Right eye matrices.
            if (canvas.IsStereoCanvas())
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

                _maskableMaterial.SetMatrix(s_GameVp2, gameVp);
                _maskableMaterial.SetMatrix(s_GameTvp2, gameVp);
            }

            FrameCache.Set(_maskableMaterial, nameof(UpdateSceneViewMatrix), true);
        }
#endif
    }
}
