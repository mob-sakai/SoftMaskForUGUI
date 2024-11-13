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
            foreach (var sm in m_SoftMasks)
            {
                sm.maskingMode = (SoftMask.MaskingMode)value;
            }
        }

        public void SetDownSamplingRate(int value)
        {
            foreach (var sm in m_SoftMasks)
            {
                sm.downSamplingRate = (SoftMask.DownSamplingRate)value;
            }
        }

        public void SetTransformSensitivity(int index)
        {
            var values = (TransformSensitivity[])Enum.GetValues(typeof(TransformSensitivity));
            UISoftMaskProjectSettings.transformSensitivity = values[index];
        }

        public void SetSoftnessRange(MinMax01 value)
        {
            foreach (var sm in m_SoftMasks)
            {
                sm.softnessRange = value;
            }
        }
    }
}
