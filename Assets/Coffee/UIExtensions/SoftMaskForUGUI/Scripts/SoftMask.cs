using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	/// <summary>
	/// Soft mask.
	/// Use instead of Mask for smooth masking.
	/// </summary>
	public class SoftMask : Mask, IMeshModifier, ICanvasRaycastFilter
	{
		//################################
		// Constant or Static Members.
		//################################
		/// <summary>
		/// Desampling rate.
		/// </summary>
		public enum DesamplingRate
		{
			None = 0,
			x1 = 1,
			x2 = 2,
			x4 = 4,
			x8 = 8,
		}

		static readonly List<SoftMask>[] s_TmpSoftMasks = new List<SoftMask>[]
		{
			new List<SoftMask>(),
			new List<SoftMask>(),
			new List<SoftMask>(),
			new List<SoftMask>(),
		};

		static readonly Color[] s_ClearColors = new Color[]
		{
			new Color(0, 0, 0, 0),
			new Color(1, 0, 0, 0),
			new Color(1, 1, 0, 0),
			new Color(1, 1, 1, 0),
		};


		//################################
		// Serialize Members.
		//################################
		[Tooltip("The desampling rate for soft mask buffer.")]
		[SerializeField] DesamplingRate m_DesamplingRate = DesamplingRate.None;
		[Tooltip("The value used by the soft mask to select the area of influence defined over the soft mask's graphic.")]
		[SerializeField][Range(0.01f, 1)] float m_Softness = 1;
		[Tooltip("Should the soft mask ignore parent soft masks?")]
		[SerializeField] bool m_IgnoreParent = false;


		//################################
		// Public Members.
		//################################
		/// <summary>
		/// The desampling rate for soft mask buffer.
		/// </summary>
		public DesamplingRate desamplingRate
		{
			get { return m_DesamplingRate; }
			set
			{
				if (m_DesamplingRate != value)
				{
					m_DesamplingRate = value;
				}
			}
		}

		/// <summary>
		/// The value used by the soft mask to select the area of influence defined over the soft mask's graphic.
		/// </summary>
		public float softness
		{
			get { return m_Softness; }
			set
			{
				value = Mathf.Clamp01(value);
				if (m_Softness != value)
				{
					m_Softness = value;
				}
			}
		}

		/// <summary>
		/// Should the soft mask ignore parent soft masks?
		/// </summary>
		/// <value>If set to true the soft mask will ignore any parent soft mask settings.</value>
		public bool ignoreParent
		{
			get { return m_IgnoreParent; }
			set
			{
				if (m_IgnoreParent != value)
				{
					m_IgnoreParent = value;
					OnTransformParentChanged();
				}
			}
		}

		/// <summary>
		/// The soft mask buffer.
		/// </summary>
		public RenderTexture softMaskBuffer
		{
			get
			{
				if (_parent)
				{
					ReleaseRT(ref _softMaskBuffer);
					return _parent.softMaskBuffer;
				}

				// Check the size of soft mask buffer.
				int w, h;
				GetDesamplingSize(m_DesamplingRate, out w, out h);
				if (_softMaskBuffer && (_softMaskBuffer.width != w || _softMaskBuffer.height != h))
				{
					ReleaseRT(ref _softMaskBuffer);
				}

				if(!_softMaskBuffer)
				{
					_softMaskBuffer = RenderTexture.GetTemporary (w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
					hasChanged = true;
				}

				return _softMaskBuffer;
			}
		}

		public bool hasChanged
		{
			get
			{
				return _parent ? _parent.hasChanged : _hasChanged;
			}
			private set
			{
				if(_parent)
				{
					_parent.hasChanged = value;
				}
				_hasChanged = value;
			}
		}

		/// <summary>
		/// Perform material modification in this function.
		/// </summary>
		/// <returns>Modified material.</returns>
		/// <param name="baseMaterial">Configured Material.</param>
		public override Material GetModifiedMaterial(Material baseMaterial)
		{
			hasChanged = true;
			var result = base.GetModifiedMaterial(baseMaterial);
			if (m_IgnoreParent && result != baseMaterial)
			{
				result.SetInt(s_StencilCompId, (int)CompareFunction.Always);
			}
			return result;
		}


		/// <summary>
		/// Call used to modify mesh.
		/// </summary>
		void IMeshModifier.ModifyMesh(Mesh mesh)
		{
			hasChanged = true;
			_mesh = mesh;
		}

		/// <summary>
		/// Call used to modify mesh.
		/// </summary>
		void IMeshModifier.ModifyMesh(VertexHelper verts)
		{
			if (isActiveAndEnabled)
			{
				verts.FillMesh(mesh);
			}
			hasChanged = true;
		}

		/// <summary>
		/// Given a point and a camera is the raycast valid.
		/// </summary>
		/// <returns>Valid.</returns>
		/// <param name="sp">Screen position.</param>
		/// <param name="eventCamera">Raycast camera.</param>
		/// <param name="g">Target graphic.</param>
		public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera, Graphic g)
		{
			if (!isActiveAndEnabled || (g == graphic && !g.raycastTarget))
			{
				return true;
			}
			if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, sp, eventCamera))
			{
				return false;
			}

			int x = (int)(softMaskBuffer.width * sp.x / Screen.width);
			int y = (int)(softMaskBuffer.height * sp.y / Screen.height);
			return 0.5f < GetPixelValue(x, y);
		}

		/// <summary>
		/// Given a point and a camera is the raycast valid.
		/// </summary>
		/// <returns>Valid.</returns>
		/// <param name="sp">Screen position.</param>
		/// <param name="eventCamera">Raycast camera.</param>
		public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			return IsRaycastLocationValid(sp, eventCamera, graphic);
		}


		//################################
		// Protected Members.
		//################################

		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		protected override void OnEnable()
		{
			hasChanged = true;

			// Register.
			if (s_ActiveSoftMasks.Count == 0)
			{
				Canvas.willRenderCanvases += UpdateMaskTextures;

				if (s_StencilCompId == 0)
				{
					s_StencilCompId = Shader.PropertyToID("_StencilComp");
					s_ColorMaskId = Shader.PropertyToID("_ColorMask");
					s_MainTexId = Shader.PropertyToID("_MainTex");
					s_SoftnessId = Shader.PropertyToID("_Softness");
				}
			}
			s_ActiveSoftMasks.Add(this);

			// Reset the parent-child relation.
			GetComponentsInChildren<SoftMask>(false, s_TempRelatables);
			for (int i = s_TempRelatables.Count - 1; 0 <= i; i--)
			{
				s_TempRelatables[i].OnTransformParentChanged();
			}
			s_TempRelatables.Clear();

			// Create objects.
			_mpb = new MaterialPropertyBlock();
			_cb = new CommandBuffer();

			graphic.SetVerticesDirty();

			base.OnEnable();
		}

		/// <summary>
		/// This function is called when the behaviour becomes disabled.
		/// </summary>
		protected override void OnDisable()
		{
			// Unregister.
			s_ActiveSoftMasks.Remove(this);
			if (s_ActiveSoftMasks.Count == 0)
			{
				Canvas.willRenderCanvases -= UpdateMaskTextures;
			}

			// Reset the parent-child relation.
			for (int i = _children.Count - 1; 0 <= i; i--)
			{
				_children[i].SetParent(_parent);
			}
			_children.Clear();
			SetParent(null);

			// Destroy objects.
			_mpb.Clear();
			_mpb = null;
			_cb.Release();
			_cb = null;

			ReleaseObject(_mesh);
			_mesh = null;
			ReleaseObject(_material);
			_material = null;
			ReleaseRT(ref _softMaskBuffer);

			base.OnDisable();
		}

		/// <summary>
		/// This function is called when the parent property of the transform of the GameObject has changed.
		/// </summary>
		protected override void OnTransformParentChanged()
		{
			hasChanged = true;
			SoftMask newParent = null;
			if (isActiveAndEnabled && !m_IgnoreParent)
			{
				var parentTransform = transform.parent;
				while (parentTransform && (!newParent || !newParent.enabled))
				{
					newParent = parentTransform.GetComponent<SoftMask>();
					parentTransform = parentTransform.parent;
				}
			}
			SetParent(newParent);
			hasChanged = true;
		}

		protected override void OnRectTransformDimensionsChange ()
		{
			hasChanged = true;
		}

#if UNITY_EDITOR
		/// <summary>
		/// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
		/// </summary>
		protected override void OnValidate()
		{
			graphic.SetMaterialDirty();
			OnTransformParentChanged();
			base.OnValidate();
		}
		#endif

		//################################
		// Private Members.
		//################################
		static Shader s_SoftMaskShader;
		static Texture2D s_ReadTexture;
		static List<SoftMask> s_ActiveSoftMasks = new List<SoftMask>();
		static List<SoftMask> s_TempRelatables = new List<SoftMask>();
		static int s_StencilCompId;
		static int s_ColorMaskId;
		static int s_MainTexId;
		static int s_SoftnessId;
		MaterialPropertyBlock _mpb;
		CommandBuffer _cb;
		Material _material;
		RenderTexture _softMaskBuffer;
		int _stencilDepth;
		Mesh _mesh;
		SoftMask _parent;
		List<SoftMask> _children = new List<SoftMask>();
		bool _hasChanged = false;

		Material material { get { return _material ? _material : _material = new Material(s_SoftMaskShader ? s_SoftMaskShader : s_SoftMaskShader = Resources.Load<Shader>("SoftMask")){ hideFlags = HideFlags.HideAndDontSave }; } }

		Mesh mesh { get { return _mesh ? _mesh : _mesh = new Mesh(){ hideFlags = HideFlags.HideAndDontSave }; } }

		/// <summary>
		/// Update all soft mask textures.
		/// </summary>
		static void UpdateMaskTextures()
		{
			foreach (var sm in s_ActiveSoftMasks)
			{
				if (!sm || sm._hasChanged)
					continue;

				var rt = sm.rectTransform;
				if(rt.hasChanged)
				{
					rt.hasChanged = false;
					sm.hasChanged = true;
				}
			}

			foreach (var sm in s_ActiveSoftMasks)
			{
				if (!sm || !sm._hasChanged)
					continue;

				sm._hasChanged = false;
				if (!sm._parent)
				{
					sm.UpdateMaskTexture ();
				}
			}
		}

		/// <summary>
		/// Update the mask texture.
		/// </summary>
		void UpdateMaskTexture()
		{
			Transform stopAfter = MaskUtilities.FindRootSortOverrideCanvas(transform);
			_stencilDepth = MaskUtilities.GetStencilDepth(transform, stopAfter);

			// Collect children soft masks.
			int depth = 0;
			s_TmpSoftMasks[0].Add(this);
			while (_stencilDepth + depth < 3)
			{
				int count = s_TmpSoftMasks[depth].Count;
				for (int i = 0; i < count; i++)
				{
					s_TmpSoftMasks[depth + 1].AddRange(s_TmpSoftMasks[depth][i]._children);
				}
				depth++;
			}

			// Clear.
			_cb.Clear();
			_cb.SetRenderTarget(softMaskBuffer);
			_cb.ClearRenderTarget(false, true, s_ClearColors[_stencilDepth]);

			// Set view and projection matrices.
			var c = graphic.canvas;
			if (c && c.renderMode != RenderMode.ScreenSpaceOverlay && c.worldCamera)
			{
				_cb.SetViewProjectionMatrices(c.worldCamera.worldToCameraMatrix, c.worldCamera.projectionMatrix);
			}
			else
			{
				_cb.SetViewMatrix(Matrix4x4.TRS(new Vector3(-1, -1, 0), Quaternion.identity, new Vector3(2f / Screen.width, 2f / Screen.height, 1f)));
			}

			// Draw soft masks.
			for (int i = 0; i < s_TmpSoftMasks.Length; i++)
			{
				int count = s_TmpSoftMasks[i].Count;
				for (int j = 0; j < count; j++)
				{
					var sm = s_TmpSoftMasks[i][j];

					// Set material property.
					sm.material.SetInt(s_ColorMaskId, (int)1 << (3 - _stencilDepth - i));
					sm._mpb.SetTexture(s_MainTexId, sm.graphic.mainTexture);
					sm._mpb.SetFloat(s_SoftnessId, sm.m_Softness);

					// Draw mesh.
					_cb.DrawMesh(sm.mesh, sm.transform.localToWorldMatrix, sm.material, 0, 0, sm._mpb);
				}
				s_TmpSoftMasks[i].Clear();
			}

			Graphics.ExecuteCommandBuffer(_cb);
		}

		/// <summary>
		/// Gets the size of the desampling.
		/// </summary>
		void GetDesamplingSize(DesamplingRate rate, out int w, out int h)
		{
#if UNITY_EDITOR
			var res = UnityEditor.UnityStats.screenRes.Split('x');
			w = int.Parse(res[0]);
			h = int.Parse(res[1]);
#else
			w = Screen.width;
			h = Screen.height;
#endif

			if (rate == DesamplingRate.None)
				return;

			float aspect = (float)w / h;
			if (w < h)
			{
				h = Mathf.ClosestPowerOfTwo(h / (int)rate);
				w = Mathf.CeilToInt(h * aspect);
			}
			else
			{
				w = Mathf.ClosestPowerOfTwo(w / (int)rate);
				h = Mathf.CeilToInt(w / aspect);
			}
		}

		/// <summary>
		/// Release the specified obj.
		/// </summary>
		/// <param name="obj">Object.</param>
		void ReleaseRT(ref RenderTexture tmpRT)
		{
			if (tmpRT)
			{
				RenderTexture.ReleaseTemporary(tmpRT);
				tmpRT = null;
			}
		}

		/// <summary>
		/// Release the specified obj.
		/// </summary>
		/// <param name="obj">Object.</param>
		void ReleaseObject(Object obj)
		{
			if (obj)
			{
				#if UNITY_EDITOR
				if (!Application.isPlaying)
					DestroyImmediate(obj);
				else
				#endif
				Destroy(obj);
				obj = null;
			}
		}


		/// <summary>
		/// Set the parent of the soft mask.
		/// </summary>
		/// <param name="newParent">The parent soft mask to use.</param>
		void SetParent(SoftMask newParent)
		{
			if (_parent != newParent && this != newParent)
			{
				if (_parent && _parent._children.Contains(this))
				{
					_parent._children.Remove(this);
					_parent._children.RemoveAll(x => x == null);
				}
				_parent = newParent;
			}

			if (_parent && !_parent._children.Contains(this))
			{
				_parent._children.Add(this);
			}
		}

		/// <summary>
		/// Gets the pixel value.
		/// </summary>
		float GetPixelValue(int x, int y)
		{
			if (!s_ReadTexture)
			{
				s_ReadTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			}
			var currentRT = RenderTexture.active;

			RenderTexture.active = softMaskBuffer;
			s_ReadTexture.ReadPixels(new Rect(x, y, 1, 1), 0, 0);
			s_ReadTexture.Apply(false, false);
			RenderTexture.active = currentRT;

			var colors = s_ReadTexture.GetRawTextureData();
			switch (_stencilDepth)
			{
				case 0:
					return (colors[1] / 255f);
				case 1:
					return (colors[1] / 255f) * (colors[2] / 255f);
				case 2:
					return (colors[1] / 255f) * (colors[2] / 255f) * (colors[3] / 255f);
				case 3:
					return (colors[1] / 255f) * (colors[2] / 255f) * (colors[3] / 255f) * (colors[0] / 255f);
				default:
					return 0;
			}
		}
	}
}