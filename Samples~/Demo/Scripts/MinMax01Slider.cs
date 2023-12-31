using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace Coffee.UISoftMask
{
    [ExecuteAlways]
    public class MinMax01Slider : Selectable, IDragHandler, IInitializePotentialDragHandler
    {
        [SerializeField]
        private Slider m_MinSlider;

        [SerializeField]
        private Slider m_MaxSlider;

        [SerializeField]
        private RectTransform m_Background;

        [SerializeField]
        private MinMax01SliderEvent m_OnValueChanged = new MinMax01SliderEvent();

        public MinMax01 value
        {
            get
            {
                if (!m_MinSlider || !m_MaxSlider) return new MinMax01(0, 1);

                return new MinMax01(m_MinSlider.value, m_MaxSlider.value);
            }
            set
            {
                if (!m_MinSlider || !m_MaxSlider) return;
                if (Mathf.Approximately(value.min, m_MinSlider.value) &&
                    Mathf.Approximately(value.max, m_MaxSlider.value))
                {
                    return;
                }

                m_MinSlider.SetValueWithoutNotify(value.min);
                m_MaxSlider.SetValueWithoutNotify(value.max);
                InvokeValueChanged();
            }
        }

        public new bool interactable
        {
            get => base.interactable;
            set
            {
                base.interactable = value;
                if (m_MinSlider) m_MinSlider.interactable = value;
                if (m_MaxSlider) m_MaxSlider.interactable = value;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_MinSlider)
            {
                m_MinSlider.onValueChanged.AddListener(OnMinValueChanged);
                m_MinSlider.minValue = 0;
                m_MinSlider.maxValue = 1;
                m_MinSlider.wholeNumbers = false;
            }

            if (m_MaxSlider)
            {
                m_MaxSlider.onValueChanged.AddListener(OnMaxValueChanged);
                m_MaxSlider.minValue = 0;
                m_MaxSlider.maxValue = 1;
                m_MaxSlider.wholeNumbers = false;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (m_MinSlider)
            {
                m_MinSlider.onValueChanged.RemoveListener(OnMinValueChanged);
            }

            if (m_MaxSlider)
            {
                m_MaxSlider.onValueChanged.RemoveListener(OnMaxValueChanged);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
            {
                return;
            }

            UpdateDrag(eventData, eventData.pressEventCamera);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        private void OnMinValueChanged(float minValue)
        {
            if (!m_MinSlider || !m_MaxSlider) return;

            m_MinSlider.SetValueWithoutNotify(Mathf.Min(minValue, m_MaxSlider.value));
            InvokeValueChanged();
        }

        private void OnMaxValueChanged(float maxValue)
        {
            if (!m_MinSlider || !m_MaxSlider) return;

            m_MaxSlider.SetValueWithoutNotify(Mathf.Max(maxValue, m_MinSlider.value));
            InvokeValueChanged();
        }

        private void InvokeValueChanged()
        {
            if (!m_MinSlider || !m_MaxSlider) return;

            m_OnValueChanged.Invoke(new MinMax01(m_MinSlider.value, m_MaxSlider.value));
        }

        private void UpdateDrag(PointerEventData eventData, Camera cam)
        {
            if (!m_MinSlider || !m_MaxSlider || !m_Background) return;


            var axisIndex = (int)m_MinSlider.direction / 2;
            var fillSize = m_Background.rect.size[axisIndex];
            if (fillSize <= 0) return;

            var current = eventData.position;
            var prev = eventData.position - eventData.delta;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Background, current, cam, out var currentPos)
                || !RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Background, prev, cam, out var prevPos))
            {
                return;
            }

            var delta = (currentPos[axisIndex] - prevPos[axisIndex]) / fillSize;
            if (delta < 0)
            {
                AddDelta(delta, m_MinSlider, m_MaxSlider);
            }
            else if (0 < delta)
            {
                AddDelta(delta, m_MaxSlider, m_MinSlider);
            }
        }

        private void AddDelta(float delta, Slider primary, Slider secondary)
        {
            var current = primary.value;
            var next = Mathf.Clamp01(current + delta);
            if (Mathf.Approximately(current, next)) return;

            var newMaxValue = Mathf.Clamp01(secondary.value + (next - current));
            primary.SetValueWithoutNotify(next);
            secondary.SetValueWithoutNotify(newMaxValue);
            InvokeValueChanged();
        }

        private bool MayDrag(PointerEventData eventData)
        {
            return isActiveAndEnabled
                   && eventData.button == PointerEventData.InputButton.Left
                   && m_MinSlider && m_MinSlider.IsInteractable()
                   && m_MaxSlider && m_MaxSlider.IsInteractable();
        }

        [Serializable]
        public class MinMax01SliderEvent : UnityEvent<MinMax01>
        {
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (m_MinSlider) m_MinSlider.interactable = interactable;
            if (m_MaxSlider) m_MaxSlider.interactable = interactable;
        }

        [CustomEditor(typeof(MinMax01Slider))]
        private class MinMaxSliderEditor : SelectableEditor
        {
            private readonly GUIContent _valueLabel = new GUIContent("Value");
            private SerializedProperty _background;
            private SerializedProperty _maxSlider;
            private SerializedProperty _minSlider;
            private SerializedProperty _onValueChanged;

            protected override void OnEnable()
            {
                base.OnEnable();
                _minSlider = serializedObject.FindProperty("m_MinSlider");
                _maxSlider = serializedObject.FindProperty("m_MaxSlider");
                _background = serializedObject.FindProperty("m_Background");
                _onValueChanged = serializedObject.FindProperty("m_OnValueChanged");
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.PropertyField(_minSlider);
                EditorGUILayout.PropertyField(_maxSlider);
                EditorGUILayout.PropertyField(_background);

                var minSlider = _minSlider.objectReferenceValue as Slider;
                var maxSlider = _maxSlider.objectReferenceValue as Slider;

                if (minSlider && maxSlider)
                {
                    var minValue = minSlider.value;
                    var maxValue = maxSlider.value;
                    if (MinMaxRangeDrawer.DrawLayout(_valueLabel, ref minValue, ref maxValue))
                    {
                        minSlider.value = minValue;
                        maxSlider.value = maxValue;
                    }
                }

                EditorGUILayout.PropertyField(_onValueChanged);

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
