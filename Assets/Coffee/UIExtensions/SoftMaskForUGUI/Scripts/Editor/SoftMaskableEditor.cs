using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;
using System;
using System.Reflection;
using Object = UnityEngine.Object;
using MaskIntr = UnityEngine.SpriteMaskInteraction;
using System.IO;

namespace Coffee.UIExtensions.Editors
{
	/// <summary>
	/// SoftMaskable editor.
	/// </summary>
	[CustomEditor (typeof (SoftMaskable))]
	[CanEditMultipleObjects]
	public class SoftMaskableEditor : Editor
	{
		//################################
		// Constant or Static Members.
		//################################
		public enum MaskInteraction : int
		{
			VisibleInsideMask = (1 << 0) + (1 << 2) + (1 << 4) + (1 << 6),
			VisibleOutsideMask = (2 << 0) + (2 << 2) + (2 << 4) + (2 << 6),
			Custom = -1,
		}

		MaskInteraction maskInteraction
		{
			get
			{
				int value = _spMaskInteraction.intValue;
				return _custom
					? MaskInteraction.Custom
						: System.Enum.IsDefined(typeof(MaskInteraction), value)
						? (MaskInteraction)value
						: MaskInteraction.Custom;
			}
			set
			{
				_custom = (value == MaskInteraction.Custom);
				if (!_custom)
				{
					_spMaskInteraction.intValue = (int)value;
				}
			}
		}
		bool _custom = false;

		static readonly Type s_TypeTMPro = AppDomain.CurrentDomain.GetAssemblies ().SelectMany (x => x.GetTypes ()).FirstOrDefault (x => x.Name == "TMP_Text");
		static readonly Type s_TypeTMP_SpriteAsset = AppDomain.CurrentDomain.GetAssemblies ().SelectMany (x => x.GetTypes ()).FirstOrDefault (x => x.Name == "TMP_SpriteAsset");
		static readonly Type s_TypeTMProSettings = AppDomain.CurrentDomain.GetAssemblies ().SelectMany (x => x.GetTypes ()).FirstOrDefault (x => x.Name == "TMP_Settings");
		static readonly Type s_TypeTMP_SubMesh = AppDomain.CurrentDomain.GetAssemblies ().SelectMany (x => x.GetTypes ()).FirstOrDefault (x => x.Name == "TMP_SubMesh");
		static readonly Type s_TypeTMP_SubMeshUI = AppDomain.CurrentDomain.GetAssemblies ().SelectMany (x => x.GetTypes ()).FirstOrDefault (x => x.Name == "TMP_SubMeshUI");
		static PropertyInfo s_PiFontSharedMaterial;
		static PropertyInfo s_PiFontSharedMaterials;
		static PropertyInfo s_PiSpriteAsset;
		static PropertyInfo s_PiRichText;
		static PropertyInfo s_PiText;
		static PropertyInfo s_PiDefaultFontAssetPath;
		static PropertyInfo s_PiDefaultSpriteAssetPath;
		static FieldInfo s_FiMaterial;
		static MethodInfo s_miGetSpriteAsset;
		static readonly List<Graphic> s_Graphics = new List<Graphic> ();
		Shader _shader;
		Shader _mobileShader;
		Shader _spriteShader;
		List<MaterialEditor> _materialEditors = new List<MaterialEditor> ();
		SerializedProperty _spMaskInteraction;

		void OnEnable ()
		{
			_spMaskInteraction = serializedObject.FindProperty("m_MaskInteraction");
			_custom = (maskInteraction == MaskInteraction.Custom);

			ClearMaterialEditors ();

			_shader = Shader.Find ("TextMeshPro/Distance Field (SoftMaskable)");
			_mobileShader = Shader.Find ("TextMeshPro/Mobile/Distance Field (SoftMaskable)");
			_spriteShader = Shader.Find ("TextMeshPro/Sprite (SoftMaskable)");

			if(s_TypeTMPro != null)
			{
				s_PiFontSharedMaterial = s_TypeTMPro.GetProperty ("fontSharedMaterial");
				s_PiSpriteAsset = s_TypeTMPro.GetProperty ("spriteAsset");
				s_PiRichText = s_TypeTMPro.GetProperty ("richText");
				s_PiText = s_TypeTMPro.GetProperty ("text");
				s_FiMaterial = s_TypeTMP_SpriteAsset.GetField ("material");
				s_PiFontSharedMaterials = s_TypeTMPro.GetProperty ("fontSharedMaterials");
				s_miGetSpriteAsset = s_TypeTMProSettings.GetMethod ("GetSpriteAsset", BindingFlags.Static | BindingFlags.Public);

				s_PiDefaultFontAssetPath = s_TypeTMProSettings.GetProperty ("defaultFontAssetPath", BindingFlags.Static | BindingFlags.Public);
				s_PiDefaultSpriteAssetPath = s_TypeTMProSettings.GetProperty ("defaultSpriteAssetPath", BindingFlags.Static | BindingFlags.Public);
			}

			s_MaskWarning = new GUIContent(EditorGUIUtility.FindTexture("console.warnicon.sml"), "This component is not SoftMask. Use SoftMask instead of Mask.");
		}
		
		void OnDisable ()
		{
			ClearMaterialEditors ();
		}

		List<Mask> tmpMasks = new List<Mask>();

		void DrawMaskInteractions()
		{
			(target as SoftMaskable).GetComponentsInParent<Mask>(true, tmpMasks);
			tmpMasks.RemoveAll(x => !x.enabled);
			tmpMasks.Reverse();

			maskInteraction = (MaskInteraction)EditorGUILayout.EnumPopup("Mask Interaction", maskInteraction);
			if (_custom)
			{
				var l = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 45;

				using (var ccs = new EditorGUI.ChangeCheckScope())
				{
					int intr0 = DrawMaskInteraction(0);
					int intr1 = DrawMaskInteraction(1);
					int intr2 = DrawMaskInteraction(2);
					int intr3 = DrawMaskInteraction(3);

					if (ccs.changed) {
						_spMaskInteraction.intValue = (intr0 << 0) + (intr1 << 2) + (intr2 << 4) + (intr3 << 6);
					}
				}

				EditorGUIUtility.labelWidth = l;
			}
		}

		static GUIContent s_MaskWarning = new GUIContent();

		int DrawMaskInteraction(int layer)
		{
			Mask mask = layer < tmpMasks.Count ? tmpMasks[layer] : null;
			MaskIntr intr = (MaskIntr)((_spMaskInteraction.intValue >> layer * 2) & 0x3);
			if (!mask)
			{
				return (int)intr;
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField(mask is SoftMask ? GUIContent.none : s_MaskWarning, GUILayout.Width(16));
				GUILayout.Space(-5);
				EditorGUILayout.ObjectField("Mask " + layer, mask, typeof(Mask), false);
				GUILayout.Space(-15);
				return (int)(MaskIntr)EditorGUILayout.EnumPopup(intr);
			}
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			serializedObject.Update();
			DrawMaskInteractions();

//			maskInteraction = (MaskInteraction)EditorGUILayout.EnumPopup("Mask Interaction", maskInteraction);
			serializedObject.ApplyModifiedProperties();
			/*
			EditorGUI.indentLevel++;
			var l = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 60;
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.ObjectField("Mask 0", null, typeof(Mask), false);
				EditorGUILayout.EnumPopup (MaskIntr.None);
			}
			EditorGUIUtility.labelWidth = l;
			EditorGUI.indentLevel--;

			var spMaskInteraction = serializedObject.FindProperty ("m_MaskInteraction");
			MaskIntr intr0 = (MaskIntr)((spMaskInteraction.intValue >> 0) & 0x3);
			MaskIntr intr1 = (MaskIntr)((spMaskInteraction.intValue >> 2) & 0x3);
			MaskIntr intr2 = (MaskIntr)((spMaskInteraction.intValue >> 4) & 0x3);
			MaskIntr intr3 = (MaskIntr)((spMaskInteraction.intValue >> 6) & 0x3);

			using (var ccs = new EditorGUI.ChangeCheckScope ()) {

				intr0 = (MaskIntr)EditorGUILayout.EnumPopup ("Layer 0", intr0);
				intr1 = (MaskIntr)EditorGUILayout.EnumPopup ("Layer 1", intr1);
				intr2 = (MaskIntr)EditorGUILayout.EnumPopup ("Layer 2", intr2);
				intr3 = (MaskIntr)EditorGUILayout.EnumPopup ("Layer 3", intr3);

				if (ccs.changed) {
					current.SetMaskInteractions (intr0,intr1,intr2,intr3);
				}
			}
			*/

//			spMaskInteraction.intValue = (intr0 << 0) | (intr1 << 2) | (intr2 << 4) | (intr3 << 6);
//
//			serializedObject.ApplyModifiedProperties ();



			var current = target as SoftMaskable;

			current.GetComponentsInChildren<Graphic> (true, s_Graphics);
			var fixTargets = s_Graphics.Where (x => x.gameObject != current.gameObject && !x.GetComponent<SoftMaskable> () && (!x.GetComponent<Mask> () || x.GetComponent<Mask> ().showMaskGraphic)).ToList ();
			if (0 < fixTargets.Count)
			{
				GUILayout.BeginHorizontal ();
				EditorGUILayout.HelpBox ("There are child Graphics that does not have a SoftMaskable component.\nAdd SoftMaskable component to them.", MessageType.Warning);
				GUILayout.BeginVertical ();
				if (GUILayout.Button ("Fix"))
				{
					foreach (var p in fixTargets)
					{
						p.gameObject.AddComponent<SoftMaskable> ();
					}
				}
				if (GUILayout.Button ("Ping"))
				{
					EditorGUIUtility.PingObject (fixTargets [0]);
				}
				GUILayout.EndVertical ();
				GUILayout.EndHorizontal ();
			}

			if(s_TypeTMPro != null)
			{
				ShowTMProWarning (_shader, _mobileShader, _spriteShader, m => { });
				var textMeshPro = current.GetComponent (s_TypeTMPro);
				if (textMeshPro != null)
				{
					Material [] fontSharedMaterials = s_PiFontSharedMaterials.GetValue (textMeshPro, new object [0]) as Material [];
					ShowMaterialEditors (fontSharedMaterials, 1, fontSharedMaterials.Length - 1);
				}
			}
			
			if (!DetectMask (current.transform.parent))
			{
				GUILayout.BeginHorizontal ();
				EditorGUILayout.HelpBox ("This is unnecessary SoftMaskable.\nCan't find any SoftMask components above.", MessageType.Warning);
				if (GUILayout.Button ("Remove", GUILayout.Height (40)))
				{
					DestroyImmediate (current);
					
					Utils.MarkPrefabDirty ();
				}
				GUILayout.EndHorizontal ();
			}
		}

		static bool DetectMask (Transform transform)
		{
			if (transform == null)
			{
				return false;
			}

			if (transform.GetComponent<SoftMask> () != null)
			{
				return true;
			}

			return DetectMask (transform.parent);
		}

		void ClearMaterialEditors ()
		{
			foreach (var e in _materialEditors)
			{
				if (e)
				{
					DestroyImmediate (e);
				}
			}
			_materialEditors.Clear ();
		}

		protected void ShowMaterialEditors (Material [] materials, int startIndex, int count)
		{
			for (int i = 0; i < count; i++)
			{
				if (_materialEditors.Count == i)
				{
					_materialEditors.Add (null);
				}

				var mat = materials [startIndex + i];
				var editor = _materialEditors [i];
				if (editor && editor.target != mat)
				{
					DestroyImmediate (editor);
					editor = null;
				}

				if (!editor)
				{
					editor = _materialEditors [i] = Editor.CreateEditor (mat) as MaterialEditor;
				}

				editor.DrawHeader ();
				editor.OnInspectorGUI ();
			}
		}

		public void ShowTMProWarning (Shader shader, Shader mobileShader, Shader spriteShader, System.Action<Material> onCreatedMaterial)
		{
			var current = target as SoftMaskable;
			var textMeshPro = current.GetComponent (s_TypeTMPro);
			if (textMeshPro == null)
			{
				return;
			}

			Material fontSharedMaterial = s_PiFontSharedMaterial.GetValue (textMeshPro, new object [0]) as Material;
			if (fontSharedMaterial == null)
			{
				return;
			}

			// Is the material preset for dissolve?
			Material m = fontSharedMaterial;
			if (m.shader != shader && m.shader != mobileShader)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.HelpBox (string.Format ("{0} requires '{1}' or '{2}' as a shader for material preset.", current.GetType ().Name, shader.name, mobileShader.name), MessageType.Warning);
				if (GUILayout.Button ("Fix"))
				{
					var correctShader = m.shader.name.Contains ("Mobile") ? mobileShader : shader;
					m = ModifyTMProMaterialPreset (m, correctShader, onCreatedMaterial);
					s_PiFontSharedMaterial.SetValue (textMeshPro, m, new object [0]);
				}
				EditorGUILayout.EndHorizontal ();
				return;
			}

			// Is the sprite asset for dissolve?
			object spriteAsset = s_PiSpriteAsset.GetValue (textMeshPro, new object [0]) ?? s_miGetSpriteAsset.Invoke (null, new object [0]);
			//TMP_SpriteAsset spriteAsset = textMeshPro.spriteAsset ?? TMP_Settings.GetSpriteAsset ();
			m = s_FiMaterial.GetValue (spriteAsset) as Material;
			bool hasSprite = (bool)s_PiRichText.GetValue (textMeshPro, new object [0]) && (s_PiText.GetValue (textMeshPro, new object [0]) as string).Contains ("<sprite=");
			if (m && m.shader != spriteShader && hasSprite)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.HelpBox (string.Format ("{0} requires '{1}' as a shader for sprite asset.", GetType ().Name, spriteShader.name), MessageType.Warning);
				if (GUILayout.Button ("Fix"))
				{
					current.GetComponentsInChildren (s_TypeTMP_SubMesh).Select (x => x.gameObject).ToList ().ForEach (DestroyImmediate);
					current.GetComponentsInChildren (s_TypeTMP_SubMeshUI).Select (x => x.gameObject).ToList ().ForEach (DestroyImmediate);
					spriteAsset = ModifyTMProSpriteAsset (m, _spriteShader, mat => { });
					s_PiSpriteAsset.SetValue (textMeshPro, spriteAsset, new object [0]);
				}
				EditorGUILayout.EndHorizontal ();
				return;
			}
		}

		Material ModifyTMProMaterialPreset (Material baseMaterial, Shader shader, System.Action<Material> onCreatedMaterial)
		{
			string path = AssetDatabase.GetAssetPath (baseMaterial);
			string filename = Path.GetFileNameWithoutExtension (path) + " (" + typeof (SoftMaskable).Name + ")";
			string defaultAssetPath = s_PiDefaultFontAssetPath.GetValue (null, new object [0]) as string;
			Material mat = Resources.Load<Material> (defaultAssetPath + filename);
			if (!mat)
			{
				mat = new Material (baseMaterial)
				{
					shaderKeywords = baseMaterial.shaderKeywords,
					shader = shader,
				};
				onCreatedMaterial (mat);
				AssetDatabase.CreateAsset (mat, Path.GetDirectoryName (path) + "/" + filename + ".mat");

				EditorUtility.FocusProjectWindow ();
				EditorGUIUtility.PingObject (mat);
			}
			else
			{
				mat.shader = shader;
			}
			EditorUtility.SetDirty (mat);
			return mat;
		}

		object ModifyTMProSpriteAsset (Material baseMaterial, Shader shader, System.Action<Material> onCreatedMaterial)
		{
			string path = AssetDatabase.GetAssetPath (baseMaterial);
			string filename = Path.GetFileNameWithoutExtension (path) + " (" + typeof (SoftMaskable).Name + ")";
			string defaultAssetPath = s_PiDefaultSpriteAssetPath.GetValue (null, new object [0]) as string;
			Object spriteAsset = Resources.Load (defaultAssetPath + filename, s_TypeTMP_SpriteAsset);
			if (spriteAsset == null)
			{
				AssetDatabase.CopyAsset (path, Path.GetDirectoryName (path) + "/" + filename + ".mat");
				spriteAsset = Resources.Load (defaultAssetPath + filename, s_TypeTMP_SpriteAsset);
				Material m = s_FiMaterial.GetValue (spriteAsset) as Material;
				m.shader = shader;
				m.name = shader.name;
				onCreatedMaterial (m);

				EditorUtility.FocusProjectWindow ();
				EditorGUIUtility.PingObject (spriteAsset);
			}
			else
			{
				Material m = s_FiMaterial.GetValue (spriteAsset) as Material;
				m.shader = shader;
			}
			EditorUtility.SetDirty (spriteAsset);
			return spriteAsset;
		}
	}
}