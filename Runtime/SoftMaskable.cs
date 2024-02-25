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
        private Action _checkGraphic;
        private MaskableGraphic _graphic;
        private Material _maskableMaterial;
        private Action _setMaterialDirtyIfNeeded;
        private bool _shouldRecalculateStencil;
        private SoftMask _softMask;
        private int _softMaskDepth;
        private int _stencilBits;

#if UNITY_EDITOR
        private Action _updateSceneViewMatrix;
#endif

        private bool isTerminal => _graphic is TerminalMaskingShape;

        private void OnEnable()
        {
            this.AddComponentOnChildren<SoftMaskable>(HideFlags.DontSave | HideFlags.NotEditable, false);

            hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            _shouldRecalculateStencil = true;
            SoftMaskUtils.onChangeBufferSize +=
                _setMaterialDirtyIfNeeded ?? (_setMaterialDirtyIfNeeded = SetMaterialDirtyIfNeeded);
            if (TryGetComponent(out _graphic))
            {
                _graphic.SetMaterialDirty();
            }
            else
            {
                UIExtraCallbacks.onBeforeCanvasRebuild +=
                    _checkGraphic ?? (_checkGraphic = CheckGraphic);
            }

#if UNITY_EDITOR
            UIExtraCallbacks.onAfterCanvasRebuild +=
                _updateSceneViewMatrix ?? (_updateSceneViewMatrix = UpdateSceneViewMatrix);
#endif
        }

        private void OnDisable()
        {
            SoftMaskUtils.onChangeBufferSize -=
                _setMaterialDirtyIfNeeded ?? (_setMaterialDirtyIfNeeded = SetMaterialDirtyIfNeeded);
            UIExtraCallbacks.onBeforeCanvasRebuild -=
                _checkGraphic ?? (_checkGraphic = CheckGraphic);
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
            _setMaterialDirtyIfNeeded = null;
            _checkGraphic = null;

#if UNITY_EDITOR
            _updateSceneViewMatrix = null;
#endif
        }

        private void OnTransformChildrenChanged()
        {
            this.AddComponentOnChildren<SoftMaskable>(HideFlags.DontSave | HideFlags.NotEditable, false);
        }

        void IMaskable.RecalculateMasking()
        {
            _shouldRecalculateStencil = true;
        }

        Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial)
        {
#if UNITY_EDITOR
            if (!UISoftMaskProjectSettings.softMaskEnabled)
            {
                MaterialRepository.Release(ref _maskableMaterial);
                return baseMaterial;
            }
#endif

            if (!isActiveAndEnabled || !_graphic || !_graphic.maskable || isTerminal || baseMaterial == null)
            {
                MaterialRepository.Release(ref _maskableMaterial);
                return baseMaterial;
            }

            RecalculateStencilIfNeeded();
            var softMask = _softMask;
            _softMaskDepth = softMask ? softMask.softMaskDepth : -1;

            var useStencil = UISoftMaskProjectSettings.useStencilOutsideScreen;
            var localId = 0u;
            if (!softMask || _softMaskDepth < 0 || 4 <= _softMaskDepth)
            {
                MaterialRepository.Release(ref _maskableMaterial);
                return baseMaterial;
            }

            Profiler.BeginSample("(SM4UI)[SoftMaskable] GetModifiedMaterial");

            var isStereo = Application.isPlaying && _graphic.canvas.IsStereoCanvas();
            var hash = new Hash128(
                (uint)baseMaterial.GetInstanceID(),
                (uint)softMask.softMaskBuffer.GetInstanceID(),
                (uint)_stencilBits + (isStereo ? 1 << 8 : 0u) + ((uint)_softMaskDepth << 9),
                localId);
            MaterialRepository.Get(hash, ref _maskableMaterial,
                x => SoftMaskUtils.CreateSoftMaskable(x.baseMaterial, x.softMaskBuffer, x._softMaskDepth,
                    x._stencilBits, x.isStereo, UISoftMaskProjectSettings.fallbackBehavior),
                (baseMaterial, softMask.softMaskBuffer, _softMaskDepth, _stencilBits, isStereo));
            Profiler.EndSample();


#if UNITY_EDITOR
            if (useStencil)
            {
                var threshold = _softMask ? _softMask.softMaskingRange.min : 0;
                if (TryGetComponent(out MaskingShape shape))
                {
                    threshold = shape.maskingMethod == MaskingShape.MaskingMethod.Additive
                        ? 0
                        : (shape.softMaskingRange.max + shape.softMaskingRange.min) / 2;
                }
#if TMP_ENABLE
                if (_graphic is TextMeshProUGUI)
                {
                    threshold -= 0.25f;
                }
#endif

                _maskableMaterial.SetFloat(ShaderPropertyIds.alphaClipThreshold, threshold);
            }
            else
            {
                _maskableMaterial.SetFloat(ShaderPropertyIds.alphaClipThreshold, 0);
            }
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

        private void SetMaterialDirtyIfNeeded()
        {
            if (_graphic && _maskableMaterial)
            {
                _graphic.SetMaterialDirty();
            }
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
            _maskableMaterial.SetMatrix(ShaderPropertyIds.gameVpId, gameVp);
            _maskableMaterial.SetMatrix(ShaderPropertyIds.gameTvpId, gameTvp);
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

                _maskableMaterial.SetMatrix(ShaderPropertyIds.gameVp2Id, gameVp);
                _maskableMaterial.SetMatrix(ShaderPropertyIds.gameTvp2Id, gameVp);
            }

            FrameCache.Set(_maskableMaterial, nameof(UpdateSceneViewMatrix), true);
        }
#endif
    }
}
