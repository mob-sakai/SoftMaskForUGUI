using Coffee.UISoftMaskInternal;
using UnityEngine;

namespace Coffee.UISoftMask.Demos
{
    public class SoftMask_Demo_MaskingShape : MonoBehaviour
    {
        [SerializeField] private MaskingShape m_MaskingShape;

        public void SetMaskingMethod(int value)
        {
            if (m_MaskingShape == null) return;

            m_MaskingShape.maskingMethod = (MaskingShape.MaskingMethod)value;
        }

        public void SetSoftnessRange(MinMax01 value)
        {
            if (m_MaskingShape == null) return;

            m_MaskingShape.softnessRange = value;
        }

        public void SetShowMaskGraphic(bool value)
        {
            if (m_MaskingShape == null) return;

            m_MaskingShape.showMaskGraphic = value;
        }
    }
}
