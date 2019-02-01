using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using MaskIntr = UnityEngine.SpriteMaskInteraction;
using UnityEngine.Serialization;

namespace Coffee.UIExtensions
{
	/// <summary>
	/// Soft maskable.
	/// Add this component to Graphic under SoftMask for smooth masking.
	/// </summary>
	[ExecuteInEditMode]
	public class SoftMaskable : MonoBehaviour, IMaterialModifier, ICanvasRaycastFilter, ISerializationCallbackReceiver
	{
		//################################
		// Constant or Static Members.
		//################################
		const int kVisibleInside = (1 << 0) + (1 << 2) + (1 << 4) + (1 << 6);
		const int kVisibleOutside = (2 << 0) + (2 << 2) + (2 << 4) + (2 << 6);


		//################################
		// Serialize Members.
		//################################
		[Tooltip("The graphic will be visible only in areas where no mask is present.")]
		[System.Obsolete]
		[HideInInspector]
		[SerializeField] bool m_Inverse = false;
		[Tooltip("The interaction for each masks.")]
		[HideInInspector]
		[SerializeField] int m_MaskInteraction = kVisibleInside;
		[Tooltip("Use stencil for masking.")]
		[SerializeField] bool m_UseStencil = false;


		//################################
		// Public Members.
		//################################
		/// <summary>
		/// Perform material modification in this function.
		/// </summary>
		/// <returns>Modified material.</returns>
		/// <param name="baseMaterial">Configured Material.</param>
		public Material GetModifiedMaterial(Material baseMaterial)
		{
			_softMask = null;
			if (!isActiveAndEnabled)
			{
				return baseMaterial;
			}

			// Find the nearest parent softmask.
			var parentTransform = transform.parent;
			while (parentTransform)
			{
				var sm = parentTransform.GetComponent<SoftMask>();
				if (sm && sm.enabled)
				{
					_softMask = sm;
					break;
				}
				parentTransform = parentTransform.parent;
			}

			Material result = baseMaterial;
			if (_softMask)
			{
				result = new Material(baseMaterial);
				result.hideFlags = HideFlags.HideAndDontSave;
				result.SetTexture(s_SoftMaskTexId, _softMask.softMaskBuffer);
				result.SetInt(s_StencilCompId, m_UseStencil ? (int)CompareFunction.Equal : (int)CompareFunction.Always);
				result.SetVector(s_MaskInteractionId, new Vector4(
						(m_MaskInteraction & 0x3),
						((m_MaskInteraction >> 2) & 0x3),
						((m_MaskInteraction >> 4) & 0x3),
						((m_MaskInteraction >> 6) & 0x3)
					));

				StencilMaterial.Remove(baseMaterial);
				ReleaseMaterial(ref _maskMaterial);
				_maskMaterial = result;

				#if UNITY_EDITOR
				result.EnableKeyword("SOFTMASK_EDITOR");
				UpdateSceneViewMatrixForShader();
				#endif
			}
			else
			{
				baseMaterial.SetTexture(s_SoftMaskTexId, Texture2D.whiteTexture);
			}

			return result;
		}

		/// <summary>
		/// Given a point and a camera is the raycast valid.
		/// </summary>
		/// <returns>Valid.</returns>
		/// <param name="sp">Screen position.</param>
		/// <param name="eventCamera">Raycast camera.</param>
		public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			if (!isActiveAndEnabled || !_softMask)
				return true;
		
			if (!RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, sp, eventCamera))
			{
				return false;
			}

			var sm = _softMask;
			for (int i = 0; i < 4; i++)
			{
				s_Interactions[i] = sm ? ((m_MaskInteraction >> i * 2) & 0x3) : 0;
				sm = sm ? sm.parent : null;
			}

			return _softMask.IsRaycastLocationValid(sp, eventCamera, graphic, s_Interactions);
		}


		/// <summary>
		/// The graphic will be visible only in areas where no mask is present.
		/// </summary>
		[System.Obsolete("Use SetMaskInteractions method instead.")]
		public bool inverse
		{
			get { return m_MaskInteraction == kVisibleOutside; }
			set
			{
				int intValue = value ? kVisibleOutside : kVisibleInside;
				if (m_MaskInteraction != intValue)
				{
					m_MaskInteraction = intValue;
					graphic.SetMaterialDirty();
				}
			}
		}

		/// <summary>
		/// The graphic associated with the soft mask.
		/// </summary>
		public Graphic graphic{ get { return _graphic ? _graphic : _graphic = GetComponent<Graphic>(); } }

		/// <summary>
		/// Set the interaction for each mask.
		/// </summary>
		public void SetMaskInteraction(SpriteMaskInteraction intr)
		{
			SetMaskInteraction(intr, intr, intr, intr);
		}

		/// <summary>
		/// Set the interaction for each mask.
		/// </summary>
		public void SetMaskInteraction(SpriteMaskInteraction layer0, SpriteMaskInteraction layer1, SpriteMaskInteraction layer2, SpriteMaskInteraction layer3)
		{
			m_MaskInteraction = (int)layer0 + ((int)layer1 << 2) + ((int)layer2 << 4) + ((int)layer3 << 6);
			if (graphic)
			{
				graphic.SetMaterialDirty();
			}
		}

		//################################
		// Private Members.
		//################################
		Graphic _graphic = null;
		SoftMask _softMask = null;
		Material _maskMaterial = null;
		static int s_SoftMaskTexId;
		static int s_StencilCompId;
		static int s_MaskInteractionId;
		static int s_SceneVId;
		static int s_ScenePId;
		static int s_GameVPId;
		static List<SoftMaskable> s_ActiveSoftMaskables;
		static int[] s_Interactions = new int[4];
		static Material s_DefaultMaterial;

		#if UNITY_EDITOR
		/// <summary>
		/// Update the scene view matrix for shader.
		/// </summary>
		static void UpdateSceneViewMatrixForShader()
		{
			UnityEditor.SceneView sceneView = UnityEditor.SceneView.lastActiveSceneView;
			if (!sceneView || !sceneView.camera)
			{
				return;
			}

			Camera cam = sceneView.camera;
			Matrix4x4 w2c = cam.worldToCameraMatrix;
			Matrix4x4 prj = cam.projectionMatrix;
		
			s_ActiveSoftMaskables.RemoveAll(x=>!x);
			foreach (var sm in s_ActiveSoftMaskables)
			{
				if (!sm || !sm._maskMaterial || !sm.graphic || !sm.graphic.canvas)
				{
					continue;
				}

				Material mat = sm._maskMaterial;
				mat.SetMatrix(s_SceneVId, w2c);
				mat.SetMatrix(s_ScenePId, prj);

				var c = sm.graphic.canvas.rootCanvas;
				if (c.renderMode != RenderMode.ScreenSpaceOverlay && c.worldCamera)
				{
					var wcam = c.worldCamera;
					var pv = wcam.projectionMatrix * wcam.worldToCameraMatrix;
					mat.SetMatrix(s_GameVPId, pv);
				}
				else
				{
					var pos = c.transform.localPosition;
					var pv = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1 / pos.x, 1 / pos.y, -2 / 1000f)) * Matrix4x4.Translate(-pos);
					mat.SetMatrix(s_GameVPId, pv);
				}
			}
		}

		/// <summary>
		/// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
		/// </summary>
		void OnValidate()
		{
			if (graphic)
			{
				graphic.SetMaterialDirty();
			}
		}
		#endif

		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		void OnEnable()
		{
			// Register.
			if (s_ActiveSoftMaskables == null)
			{
				s_ActiveSoftMaskables = new List<SoftMaskable>();

				#if UNITY_EDITOR
				UnityEditor.EditorApplication.update += UpdateSceneViewMatrixForShader;
				s_SceneVId = Shader.PropertyToID("_SceneV");
				s_ScenePId = Shader.PropertyToID("_SceneP");
				s_GameVPId = Shader.PropertyToID("_GameVP");
				#endif

				s_SoftMaskTexId = Shader.PropertyToID("_SoftMaskTex");
				s_StencilCompId = Shader.PropertyToID("_StencilComp");
				s_MaskInteractionId = Shader.PropertyToID("_MaskInteraction");
			}
			s_ActiveSoftMaskables.Add(this);


			var g = graphic;
			if (g)
			{
				if (!g.material || g.material == Graphic.defaultGraphicMaterial)
				{
					g.material = s_DefaultMaterial ?? (s_DefaultMaterial = new Material(Resources.Load<Shader>("UI-Default-SoftMask")) { hideFlags = HideFlags.HideAndDontSave, });
				}
				g.SetMaterialDirty();
			}
			_softMask = null;
		}

		/// <summary>
		/// This function is called when the behaviour becomes disabled.
		/// </summary>
		void OnDisable()
		{
			s_ActiveSoftMaskables.Remove(this);

			var g = graphic;
			if (g)
			{
				if (g.material == s_DefaultMaterial)
				{
					g.material = null;
				}
				g.SetMaterialDirty();
			}
			ReleaseMaterial(ref _maskMaterial);

			_softMask = null;
		}

		/// <summary>
		/// Release the material.
		/// </summary>
		void ReleaseMaterial(ref Material mat)
		{
			if (mat)
			{
				StencilMaterial.Remove(mat);

				#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					DestroyImmediate(mat);
				}
				else
				#endif
				{
					Destroy(mat);
				}
				mat = null;
			}
		}


		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			#pragma warning disable 0612
			if (m_Inverse)
			{
				m_Inverse = false;
				m_MaskInteraction = (2 << 0) + (2 << 2) + (2 << 4) + (2 << 6);
			}
			#pragma warning restore 0612
		}
	}
}