using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;


namespace Coffee.UIExtensions.Editors
{
	/// <summary>
	/// SoftMask editor.
	/// </summary>
	[CustomEditor(typeof(SoftMask))]
	[CanEditMultipleObjects]
	public class SoftMaskEditor : Editor
	{
		const string k_PrefsPreview = "SoftMaskEditor_Preview";
		static readonly List<Graphic> s_Graphics = new List<Graphic> ();
		static bool s_Preview;

		private void OnEnable ()
		{
			s_Preview = EditorPrefs.GetBool (k_PrefsPreview, false);
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			var current = target as SoftMask;
			current.GetComponentsInChildren<Graphic> (true, s_Graphics);
			var fixTargets = s_Graphics.Where (x => x.gameObject != current.gameObject && !x.GetComponent<SoftMaskable> () && (!x.GetComponent<Mask> () || x.GetComponent<Mask> ().showMaskGraphic)).ToList ();
			if (0 < fixTargets.Count)
			{
				GUILayout.BeginHorizontal ();
				EditorGUILayout.HelpBox ("There are child Graphicss that does not have a SoftMaskable component.\nAdd SoftMaskable component to them.", MessageType.Warning);
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
					EditorGUIUtility.PingObject (fixTargets[0]);
				}
				GUILayout.EndVertical ();
				GUILayout.EndHorizontal ();
			}

			// Preview buffer.
			GUILayout.BeginHorizontal (EditorStyles.helpBox);
			if (s_Preview != (s_Preview = EditorGUILayout.ToggleLeft ("Preview Buffer", s_Preview, GUILayout.MaxWidth (EditorGUIUtility.labelWidth))))
			{
				EditorPrefs.SetBool (k_PrefsPreview, s_Preview);
			}
			if (s_Preview)
			{
				var tex = current.softMaskBuffer;
				var wtdth = tex.width * 64 / tex.height;
				EditorGUI.DrawPreviewTexture (GUILayoutUtility.GetRect (wtdth, 64), tex, null, ScaleMode.ScaleToFit);
				Repaint ();
			}
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
		}


		//%%%% Context menu for editor %%%%
		[MenuItem("CONTEXT/Mask/Convert To SoftMask", true)]
		static bool _ConvertToSoftMask(MenuCommand command)
		{
			return CanConvertTo<SoftMask>(command.context);
		}

		[MenuItem("CONTEXT/Mask/Convert To SoftMask", false)]
		static void ConvertToSoftMask(MenuCommand command)
		{
			ConvertTo<SoftMask>(command.context);
		}

		[MenuItem("CONTEXT/Mask/Convert To Mask", true)]
		static bool _ConvertToMask(MenuCommand command)
		{
			return CanConvertTo<Mask>(command.context);
		}

		[MenuItem("CONTEXT/Mask/Convert To Mask", false)]
		static void ConvertToMask(MenuCommand command)
		{
			ConvertTo<Mask>(command.context);
		}

		/// <summary>
		/// Verify whether it can be converted to the specified component.
		/// </summary>
		protected static bool CanConvertTo<T>(Object context)
			where T : MonoBehaviour
		{
			return context && context.GetType() != typeof(T);
		}

		/// <summary>
		/// Convert to the specified component.
		/// </summary>
		protected static void ConvertTo<T>(Object context) where T : MonoBehaviour
		{
			var target = context as MonoBehaviour;
			var so = new SerializedObject(target);
			so.Update();

			bool oldEnable = target.enabled;
			target.enabled = false;

			// Find MonoScript of the specified component.
			foreach (var script in Resources.FindObjectsOfTypeAll<MonoScript>())
			{
				if (script.GetClass() != typeof(T))
					continue;

				// Set 'm_Script' to convert.
				so.FindProperty("m_Script").objectReferenceValue = script;
				so.ApplyModifiedProperties();
				break;
			}

			(so.targetObject as MonoBehaviour).enabled = oldEnable;
		}
	}
}