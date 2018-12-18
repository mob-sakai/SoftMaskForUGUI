using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions.Demos
{
	public class SoftMask_Demo : MonoBehaviour
	{
		[SerializeField] RawImage softMaskBufferViewer;
		[SerializeField] SoftMask softMask;


		// Use this for initialization
		void OnEnable()
		{
			softMaskBufferViewer.texture = softMask.softMaskBuffer;
		}
	
		// Update is called once per frame
		void Update()
		{
		
		}

		public void SetWorldSpase(bool flag)
		{
			if(flag)
			{
				GetComponent<Canvas> ().renderMode = RenderMode.ScreenSpaceCamera;
				GetComponent<Canvas> ().renderMode = RenderMode.WorldSpace;
				transform.rotation = Quaternion.Euler (new Vector3 (0, 6, 0));
			}
		}

		public void SetScreenSpase (bool flag)
		{
			if (flag)
			{
				GetComponent<Canvas> ().renderMode = RenderMode.ScreenSpaceCamera;
			}
		}

		public void SetOverlay (bool flag)
		{
			if (flag)
			{
				GetComponent<Canvas> ().renderMode = RenderMode.ScreenSpaceOverlay;
			}
		}
	}
}