using System;
using Coffee.UISoftMaskInternal;
using UnityEngine;

namespace Coffee.UISoftMask.Demos
{
    public class SoftMask_Demo_SoftMask : MonoBehaviour
    {
        [SerializeField] private SoftMask[] m_SoftMasks;

        public void SetMaskingMode(int value)
        {
            if (m_SoftMasks == null) return;

            foreach (var sm in m_SoftMasks)
            {
                if (sm != null) sm.maskingMode = (SoftMask.MaskingMode)value;
            }
        }

        public void SetDownSamplingRate(int value)
        {
            if (m_SoftMasks == null) return;

            foreach (var sm in m_SoftMasks)
            {
                if (sm != null) sm.downSamplingRate = (SoftMask.DownSamplingRate)value;
            }
        }

        public void SetTransformSensitivity(int index)
        {
            var values = (TransformSensitivity[])Enum.GetValues(typeof(TransformSensitivity));
            if (index < 0 || values.Length <= index) return;

            UISoftMaskProjectSettings.transformSensitivity = values[index];
        }

        public void SetSoftnessRange(MinMax01 value)
        {
            if (m_SoftMasks == null) return;

            foreach (var sm in m_SoftMasks)
            {
                if (sm != null) sm.softnessRange = value;
            }
        }
    }
}
