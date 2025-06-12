using System;
using Coffee.UISoftMaskInternal;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Coffee.UISoftMask
{
    /// <summary>
    /// Fits the RectTransform to another RectTransform.
    /// The target RectTransform must not be a child of this RectTransform.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [Icon("Packages/com.coffee.softmask-for-ugui/Icons/SoftMaskIcon.png")]
    [DisallowMultipleComponent]
    public sealed class RectTransformFitter : MonoBehaviour, ILayoutElement, ILayoutIgnorer
    {
        [Flags]
        public enum RectTransformProperties
        {
            PositionX = DrivenTransformProperties.AnchoredPositionX,
            PositionY = DrivenTransformProperties.AnchoredPositionY,
            PositionZ = DrivenTransformProperties.AnchoredPositionZ,
            Position2D = PositionY | PositionX,
            Position = PositionY | PositionX | PositionZ,
            Rotation = DrivenTransformProperties.Rotation,
            ScaleX = DrivenTransformProperties.ScaleX,
            ScaleY = DrivenTransformProperties.ScaleY,
            ScaleZ = DrivenTransformProperties.ScaleZ,
            Scale = ScaleZ | ScaleY | ScaleX,
            SizeDeltaX = DrivenTransformProperties.SizeDeltaX,
            SizeDeltaY = DrivenTransformProperties.SizeDeltaY,
            SizeDelta = SizeDeltaY | SizeDeltaX
        }

        [Tooltip("Target RectTransform to fit.")]
        [SerializeField]
        private RectTransform m_Target;

        [Tooltip("Target RectTransform properties.")]
        [SerializeField]
        private RectTransformProperties m_TargetProperties = RectTransformProperties.Position
                                                             | RectTransformProperties.Rotation
                                                             | RectTransformProperties.Scale
                                                             | RectTransformProperties.SizeDelta;

        private Action _fit;

        private RectTransform _rectTransform;
        private DrivenRectTransformTracker _tracker;

        /// <summary>
        /// Target RectTransform to fit.
        /// </summary>
        public RectTransform target
        {
            get => m_Target;
            set => m_Target = value;
        }

        /// <summary>
        /// Target RectTransform properties.
        /// </summary>
        public RectTransformProperties targetProperties
        {
            get => m_TargetProperties;
            set
            {
                if (m_TargetProperties == value) return;
                m_TargetProperties = value;
                OnValidate();
            }
        }

        /// <summary>
        /// Called when the component is enabled.
        /// </summary>
        private void OnEnable()
        {
            _rectTransform = GetComponent<RectTransform>();
            UIExtraCallbacks.onBeforeCanvasRebuild += _fit ?? (_fit = Fit);
            OnValidate();
        }

        /// <summary>
        /// Called when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            UIExtraCallbacks.onBeforeCanvasRebuild -= _fit ?? (_fit = Fit);
            OnValidate();
        }

        /// <summary>
        /// Called when the component is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            _rectTransform = null;
            _fit = null;
        }

        private void OnValidate()
        {
            _tracker.Clear();
            if (!isActiveAndEnabled) return;

            var trackValue = (DrivenTransformProperties)m_TargetProperties;
            if (0 < (m_TargetProperties & (RectTransformProperties.SizeDelta | RectTransformProperties.Position)))
            {
                trackValue |= DrivenTransformProperties.Pivot | DrivenTransformProperties.Anchors;
            }

            _tracker.Add(this, _rectTransform, trackValue);
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

        private void Fit()
        {
            // TODO; Child warning.
            if (!m_Target || !_rectTransform || m_Target.IsChildOf(_rectTransform)) return;

            Profiler.BeginSample("(SM4UI)[RectTransformFitter] Fit");

            // Position
            if (0 < (m_TargetProperties & RectTransformProperties.Position))
            {
                var targetPosition = m_Target.position;
                var position = _rectTransform.position;
                if (0 < (m_TargetProperties & RectTransformProperties.PositionX))
                {
                    position.x = targetPosition.x;
                }

                if (0 < (m_TargetProperties & RectTransformProperties.PositionY))
                {
                    position.y = targetPosition.y;
                }

                if (0 < (m_TargetProperties & RectTransformProperties.PositionZ))
                {
                    position.z = targetPosition.z;
                }

                _rectTransform.position = position;
            }

            // Rotation
            if (0 < (m_TargetProperties & RectTransformProperties.Rotation))
            {
                _rectTransform.rotation = m_Target.rotation;
            }

            // Scale
            if (0 < (m_TargetProperties & RectTransformProperties.Scale))
            {
                var parent = _rectTransform.parent;
                var s1 = m_Target.lossyScale;
                var s2 = parent ? parent.lossyScale : Vector3.one;
                var localScale = _rectTransform.localScale;
                if (0 < (m_TargetProperties & RectTransformProperties.ScaleX))
                {
                    localScale.x = Mathf.Approximately(s2.x, 0) ? 1 : s1.x / s2.x;
                }

                if (0 < (m_TargetProperties & RectTransformProperties.ScaleY))
                {
                    localScale.y = Mathf.Approximately(s2.y, 0) ? 1 : s1.y / s2.y;
                }

                if (0 < (m_TargetProperties & RectTransformProperties.ScaleZ))
                {
                    localScale.z = Mathf.Approximately(s2.z, 0) ? 1 : s1.z / s2.z;
                }

                _rectTransform.localScale = localScale;
            }

            // SizeDelta
            if (0 < (m_TargetProperties & RectTransformProperties.SizeDelta))
            {
                var targetSize = m_Target.rect.size;
                var sizeDelta = _rectTransform.sizeDelta;
                if (0 < (m_TargetProperties & RectTransformProperties.SizeDeltaX))
                {
                    sizeDelta.x = targetSize.x;
                }

                if (0 < (m_TargetProperties & RectTransformProperties.SizeDeltaY))
                {
                    sizeDelta.y = targetSize.y;
                }

                _rectTransform.sizeDelta = sizeDelta;
            }

            // Pivot & Anchor
            if (0 < (m_TargetProperties & (RectTransformProperties.SizeDelta | RectTransformProperties.Position)))
            {
                _rectTransform.anchorMax = _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                _rectTransform.pivot = m_Target.pivot;
            }

            Profiler.EndSample();
        }
    }
}
