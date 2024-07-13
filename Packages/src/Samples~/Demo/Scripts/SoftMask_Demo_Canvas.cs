using UnityEngine;

namespace Coffee.UISoftMask.Demos
{
    public class SoftMask_Demo_Canvas : MonoBehaviour
    {
        public void EnableCameraPan(bool flag)
        {
            var canvas = GetComponentInParent<Canvas>().rootCanvas;
            var angles = canvas.worldCamera.transform.rotation.eulerAngles;
            angles.y = flag ? -5 : 0;
            canvas.worldCamera.transform.rotation = Quaternion.Euler(angles);
        }

        public void EnableCameraTilt(bool flag)
        {
            var canvas = GetComponentInParent<Canvas>().rootCanvas;
            var angles = canvas.worldCamera.transform.rotation.eulerAngles;
            angles.x = flag ? 10 : 0;
            canvas.worldCamera.transform.rotation = Quaternion.Euler(angles);
        }

        public void EnableCameraRoll(bool flag)
        {
            var canvas = GetComponentInParent<Canvas>().rootCanvas;
            var angles = canvas.worldCamera.transform.rotation.eulerAngles;
            angles.z = flag ? 10 : 0;
            canvas.worldCamera.transform.rotation = Quaternion.Euler(angles);
        }

        public void SetRenderMode(int mode)
        {
            var canvas = GetComponentInParent<Canvas>().rootCanvas;
            if (canvas.renderMode == (RenderMode)mode)
            {
                return;
            }

            if (mode == (int)RenderMode.WorldSpace)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.transform.rotation = Quaternion.Euler(new Vector3(0, 6, 0));
            }
            else
            {
                canvas.renderMode = (RenderMode)mode;
            }
        }
    }
}
