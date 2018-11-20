using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;


namespace Coffee.UIExtensions.Editors
{
	/// <summary>
	/// SoftMask editor.
	/// </summary>
	[CustomEditor(typeof(SoftMask))]
	[CanEditMultipleObjects]
	public class SoftMaskEditor : Editor
	{
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