using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class SliderValue : MonoBehaviour
{
    [SerializeField]
    private Text m_Text;

    [SerializeField]
    private Slider m_Slider;

    private void OnEnable()
    {
        m_Slider.onValueChanged.AddListener(OnValueChanged);
        OnValueChanged(m_Slider.value);
    }

    private void OnDisable()
    {
        m_Slider.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        m_Text.text = value.ToString("F2");
    }
}
