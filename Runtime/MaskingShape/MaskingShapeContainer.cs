using System;
using System.Collections.Generic;
using Coffee.UISoftMaskInternal;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Coffee.UISoftMask
{
    public interface IMaskingShapeContainerOwner
    {
        void Register(MaskingShapeContainer container);
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class MaskingShapeContainer : MonoBehaviour, ICanvasRaycastFilter, IMaterialModifier
    {
        [SerializeField]
        private List<MaskingShape> m_MaskingShapes = new List<MaskingShape>();

        private Action _checkTransformChanged;

        private bool _dirty;
        private Mask _mask;
        private bool _needTerminal;
        private TerminalMaskingShape _terminal;

        private void OnEnable()
        {
            UIExtraCallbacks.onBeforeCanvasRebuild +=
                _checkTransformChanged ?? (_checkTransformChanged = CheckTransformChanged);
            hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            SetContainerDirty();

            if (_mask is IMaskingShapeContainerOwner owner)
            {
                owner.Register(this);
            }

            _dirty = true;
            m_MaskingShapes.RemoveAll(x => !x);
            m_MaskingShapes.Sort();
        }

        private void OnDisable()
        {
            UIExtraCallbacks.onBeforeCanvasRebuild -=
                _checkTransformChanged ?? (_checkTransformChanged = CheckTransformChanged);
            _dirty = false;
            _needTerminal = false;
        }

        private void OnDestroy()
        {
            _mask = null;
            _terminal = null;
            m_MaskingShapes.Clear();
            _checkTransformChanged = null;
        }

        bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            const string key = nameof(ICanvasRaycastFilter.IsRaycastLocationValid);
            if (FrameCache.TryGet(this, key, out bool valid))
            {
                return valid;
            }

            // If the mask is owner of the container, skip the raycast.
            if (!_mask || !_mask.MaskEnabled() || _mask is IMaskingShapeContainerOwner)
            {
                FrameCache.Set(this, key, true);
                return true;
            }

            valid = IsInside(sp, eventCamera, true);

            FrameCache.Set(this, key, valid);
            return valid;
        }

        Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial)
        {
            // If the terminal exists, remove pop instruction (the operation to revert the stencil).
            if (isActiveAndEnabled && _mask && _mask.MaskEnabled() && _needTerminal)
            {
                _mask.graphic.canvasRenderer.hasPopInstruction = false;
                _mask.graphic.canvasRenderer.popMaterialCount = 0;
            }

            return baseMaterial;
        }

        public bool IsInside(Vector2 sp, Camera eventCamera, bool defaultValid = false, float threshold = 0.01f)
        {
            if (FrameCache.TryGet(this, nameof(IsInside), defaultValid ? 0 : 1, out bool valid))
            {
                return valid;
            }

            Profiler.BeginSample("(SM4UI)[MaskingShapeContainer] IsInside");
            valid = defaultValid;
            for (var i = 0; i < m_MaskingShapes.Count; i++)
            {
                if (!m_MaskingShapes[i] || !m_MaskingShapes[i].IsInside(sp, eventCamera, threshold)) continue;
                switch (m_MaskingShapes[i].maskingMethod)
                {
                    case MaskingShape.MaskingMethod.Additive:
                        valid = true;
                        break;
                    case MaskingShape.MaskingMethod.Subtract:
                        valid = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            FrameCache.Set(this, nameof(IsInside), defaultValid ? 0 : 1, valid);
            Profiler.EndSample();

            return valid;
        }

        public void SetContainerDirty()
        {
            Logging.LogIf(!_dirty, this, $"! SetContainerDirty {GetInstanceID()}");

            if (!_mask)
            {
                TryGetComponent(out _mask);
            }

            _dirty = true;
        }

        private void CheckTransformChanged()
        {
            var softMask = _mask as SoftMask;
            _needTerminal = false;
            for (var i = 0; i < m_MaskingShapes.Count; i++)
            {
                var shape = m_MaskingShapes[i];
                if (!shape || !shape.graphic || !shape.graphic.IsInScreen()) continue;

                if (shape.hasTransformChanged)
                {
                    _dirty = true;
                }

                if (shape.maskingMethod == MaskingShape.MaskingMethod.Additive)
                {
                    _needTerminal = true;
                }
            }

            if (_dirty && _mask && _mask.MaskEnabled())
            {
                if (softMask)
                {
                    softMask.SetSoftMaskDirty();
                }
                else
                {
                    _mask.graphic.SetMaterialDirty();
                }
            }

            _dirty = false;

            if (!_mask.MaskEnabled() || (softMask && softMask.SoftMaskingEnabled()))
            {
                _needTerminal = false;
            }

            if (_needTerminal && !_terminal)
            {
                _terminal = FindTerminal();
            }

            if (_terminal)
            {
                _terminal.enabled = _needTerminal;
                _terminal.transform.SetAsLastSibling();
            }
        }

        public bool IsInScreen()
        {
            for (var i = 0; i < m_MaskingShapes.Count; i++)
            {
                var shape = m_MaskingShapes[i];
                if (shape && shape.graphic && shape.graphic.IsInScreen())
                {
                    return true;
                }
            }

            return false;
        }

        public void DrawSoftMaskBuffer(CommandBuffer cb, int softMaskDepth)
        {
            for (var i = m_MaskingShapes.Count - 1; i >= 0; i--)
            {
                var shape = m_MaskingShapes[i];
                if (!shape) m_MaskingShapes.RemoveAtFast(i);
            }

            m_MaskingShapes.Sort();

            for (var i = 0; i < m_MaskingShapes.Count; i++)
            {
                m_MaskingShapes[i].DrawSoftMaskBuffer(cb, softMaskDepth);
            }
        }

        public void Register(MaskingShape shape)
        {
            if (!shape || m_MaskingShapes.Contains(shape)) return;

            m_MaskingShapes.Add(shape);
            _dirty = true;
            Logging.Log(this, $"Register #{m_MaskingShapes.Count}: {shape} {shape.GetInstanceID()}");
        }

        public void Unregister(MaskingShape shape)
        {
            if (!m_MaskingShapes.Contains(shape)) return;

            m_MaskingShapes.Remove(shape);
            _dirty = true;
            Logging.Log(this, $"Unregister #{m_MaskingShapes.Count}: {shape} {shape.GetInstanceID()}");
        }

        private TerminalMaskingShape FindTerminal()
        {
            var count = transform.childCount;
            for (var i = 0; i < count; i++)
            {
                if (transform.GetChild(i).TryGetComponent<TerminalMaskingShape>(out var terminal))
                {
                    return terminal;
                }
            }

            var go = new GameObject("[generated] TerminalMaskingShape");
            go.transform.SetParent(transform, false);
            go.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;

            return go.AddComponent<TerminalMaskingShape>();
        }
    }
}
