using Coffee.UISoftMaskInternal;
using UnityEngine;

namespace Coffee.UISoftMask.Demos
{
    public class SoftMask_Demo_MaskingShape : MonoBehaviour
    {
        [SerializeField] private MaskingShape m_MaskingShape;

        public void SetMaskingMethod(int value)
        {
            m_MaskingShape.maskingMethod = (MaskingShape.MaskingMethod)value;
        }

        public void SetSoftnessRange(MinMax01 value)
        {
            m_MaskingShape.softnessRange = value;
        }

        public void SetShowMaskGraphic(bool value)
        {
            m_MaskingShape.showMaskGraphic = value;
        }
    }
}
