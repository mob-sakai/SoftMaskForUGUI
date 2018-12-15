using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	/// <summary>
	/// Soft maskable.
	/// Add this component to Graphic under SoftMask for smooth masking.
	/// </summary>
	[ExecuteInEditMode]
	public class SoftMaskable : MonoBehaviour, IMaterialModifier, ICanvasRaycastFilter
	{
		//################################
		// Constant or Static Members.
		//################################
		static List<SoftMaskable> s_ActiveSoftMaskables;
		static Material defaultMaterial = null;


		//################################
		// Serialize Members.
		//################################
		[Tooltip("The graphic will be visible only in areas where no mask is present.")]
		[SerializeField] bool m_Inverse = false;


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
			var parentTransform = transform;
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

				if (m_Inverse)
				{
					result.SetFloat(s_SoftMaskInverseId, 1);
					result.SetInt(s_StencilCompId, (int)CompareFunction.Always);
				}
				else
				{
					result.SetFloat(s_SoftMaskInverseId, 0);
					result.SetInt(s_StencilCompId, (int)CompareFunction.Equal);
				}

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
				return false;

			return _softMask.IsRaycastLocationValid(sp, eventCamera, graphic) != m_Inverse;
		}

		/// <summary>
		/// The graphic will be visible only in areas where no mask is present.
		/// </summary>
		public bool inverse
		{
			get { return m_Inverse; }
			set
			{
				if (m_Inverse != value)
				{
					m_Inverse = value;
					graphic.SetMaterialDirty();
				}
			}
		}

		/// <summary>
		/// The graphic associated with the soft mask.
		/// </summary>
		public Graphic graphic{ get { return _graphic ? _graphic : _graphic = GetComponent<Graphic>(); } }


		//################################
		// Private Members.
		//################################
		Graphic _graphic = null;
		SoftMask _softMask = null;
		Material _maskMaterial = null;
		static int s_SoftMaskTexId;
		static int s_StencilCompId;
		static int s_SoftMaskInverseId;

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
		
			foreach (var sm in s_ActiveSoftMaskables)
			{
				if(sm)
				{
					Material mat = sm._maskMaterial;
					if (mat)
					{
						mat.SetMatrix ("_SceneView", w2c);
						mat.SetMatrix ("_SceneProj", prj);
					}
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
				#endif

				s_SoftMaskTexId = Shader.PropertyToID("_SoftMaskTex");
				s_StencilCompId = Shader.PropertyToID("_StencilComp");
				s_SoftMaskInverseId = Shader.PropertyToID("_SoftMaskInverse");
			}
			s_ActiveSoftMaskables.Add(this);


			var g = graphic;
			if (g)
			{
				if (!g.material || g.material == Graphic.defaultGraphicMaterial)
				{
					g.material = defaultMaterial ?? (defaultMaterial = new Material(Resources.Load<Shader>("UI-Default-SoftMask")));
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
				if (g.material == defaultMaterial)
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
	}
}