using UnityEngine;

namespace Coffee.UISoftMask.Demos
{
    public class SoftMask_Demo_Canvas : MonoBehaviour
    {
        public void EnableCameraPan(bool flag)
        {
            var canvas = GetRootCanvas();
            if (canvas == null || canvas.worldCamera == null) return;

            var camera = canvas.worldCamera;
            var angles = camera.transform.rotation.eulerAngles;
            angles.y = flag ? -5 : 0;
            camera.transform.rotation = Quaternion.Euler(angles);
        }

        public void EnableCameraTilt(bool flag)
        {
            var canvas = GetRootCanvas();
            if (canvas == null || canvas.worldCamera == null) return;

            var camera = canvas.worldCamera;
            var angles = camera.transform.rotation.eulerAngles;
            angles.x = flag ? 10 : 0;
            camera.transform.rotation = Quaternion.Euler(angles);
        }

        public void EnableCameraRoll(bool flag)
        {
            var canvas = GetRootCanvas();
            if (canvas == null || canvas.worldCamera == null) return;

            var camera = canvas.worldCamera;
            var angles = camera.transform.rotation.eulerAngles;
            angles.z = flag ? 10 : 0;
            camera.transform.rotation = Quaternion.Euler(angles);
        }

        public void SetRenderMode(int mode)
        {
            var canvas = GetRootCanvas();
            if (canvas == null) return;

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

        private Canvas GetRootCanvas()
        {
            var canvas = GetComponentInParent<Canvas>();
            return canvas != null ? canvas.rootCanvas : null;
        }
    }
}
