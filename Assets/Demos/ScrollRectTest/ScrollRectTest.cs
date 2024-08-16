using Coffee.UISoftMask;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectTest : MonoBehaviour
{
    [SerializeField] private Canvas m_Canvas;

    [Header("Clone")]
    [SerializeField] private GameObject m_Origin;

    [SerializeField] private Transform m_Parent;
    [SerializeField] private int m_CloneCount = 10;

    private void Start()
    {
        for (var i = 0; i < m_CloneCount; i++)
        {
            var go = Instantiate(m_Origin, m_Parent, false);
            go.name = $"{m_Origin.name} ({i})";
        }
    }

    public void AddItem()
    {
        var go = Instantiate(m_Origin, m_Parent, false);
        go.name = $"{m_Origin.name} (Added)";
    }

    public void SetMaskingMode(int mode)
    {
        foreach (var c in m_Canvas.GetComponentsInChildren<SoftMask>())
        {
            c.maskingMode = (SoftMask.MaskingMode)mode;
        }
    }

    public void EnableRectMask2D(bool flag)
    {
        foreach (var c in m_Canvas.GetComponentsInChildren<RectMask2D>())
        {
            c.enabled = flag;
        }
    }
}
