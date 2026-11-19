using UnityEngine;

public class ScaleTest : MonoBehaviour
{
    public RectTransform m_Target;

    public void Scale(float scale)
    {
        m_Target.localScale = new Vector3(scale, scale, scale);
    }
}
