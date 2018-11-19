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
			GetComponent<Canvas>().renderMode = flag ? RenderMode.WorldSpace : RenderMode.ScreenSpaceCamera;
			transform.rotation = flag ? Quaternion.Euler(new Vector3(0, 6, 0)) : Quaternion.identity;
		}
	}
}