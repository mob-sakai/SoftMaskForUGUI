using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UISoftMask.Demos
{
    [RequireComponent(typeof(Text))]
    public class CurrentShaderName : MonoBehaviour
    {
        [SerializeField] private Graphic m_Graphic;
        private bool _dirty;
        private Text _text;

        private IEnumerator Start()
        {
            _text = GetComponent<Text>();
            _dirty = true;
            m_Graphic.RegisterDirtyMaterialCallback(() => _dirty = true);

            var eof = new WaitForEndOfFrame();
            while (true)
            {
                yield return eof;
                if (_dirty)
                {
                    _text.text = $"Current Shader: <color=orange>{m_Graphic.materialForRendering.shader.name}</color>";
                }
            }
        }
    }
}
