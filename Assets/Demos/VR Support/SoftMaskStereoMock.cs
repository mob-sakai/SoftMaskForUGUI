using UnityEngine;

namespace Coffee.UISoftMask.Demos
{
    public class SoftMaskStereoMock : MonoBehaviour
    {
        private void OnEnable()
        {
            UISoftMaskProjectSettings.useStereoMock = true;
        }

        private void OnDisable()
        {
            UISoftMaskProjectSettings.useStereoMock = false;
        }
    }
}
