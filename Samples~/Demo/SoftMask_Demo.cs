using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UISoftMask.Demos
{
    public class SoftMask_Demo : MonoBehaviour
    {
        [SerializeField] RawImage[] softMaskBufferViewer;
        [SerializeField] SoftMask[] softMask;
        [SerializeField] Text text;
        [SerializeField] GameObject title;


        // Use this for initialization
        void OnEnable()
        {
            title.SetActive(true);

            text.text = string.Format("GPU: {0}\nDeviceType: {1}\nShaderLevel: {2}\nUVStartsAtTop: {3}",
                SystemInfo.graphicsDeviceName,
                SystemInfo.graphicsDeviceType,
                SystemInfo.graphicsShaderLevel,
                SystemInfo.graphicsUVStartsAtTop);

            for (int i = 0; i < softMask.Length; i++)
            {
                softMaskBufferViewer[i].texture = softMask[i].softMaskBuffer;
            }
        }

        public void SetWorldSpase(bool flag)
        {
            if (flag)
            {
                GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                transform.rotation = Quaternion.Euler(new Vector3(0, 6, 0));
            }
        }

        public void SetScreenSpase(bool flag)
        {
            if (flag)
            {
                GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            }
        }

        public void SetOverlay(bool flag)
        {
            if (flag)
            {
                GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }
    }
}
