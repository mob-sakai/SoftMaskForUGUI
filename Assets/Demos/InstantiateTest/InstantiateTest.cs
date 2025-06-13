using System.Collections;
using UnityEngine;

public class InstantiateTest : MonoBehaviour
{
    [SerializeField] private GameObject m_Prefab;
    [SerializeField] private int m_InstanceCount = 100;
    [SerializeField] private Transform m_Parent;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2);

        for (var i = 0; i < m_InstanceCount; i++)
        {
            var instance = Instantiate(m_Prefab, m_Parent);
            instance.name = $"{m_Prefab.name}_{i}";
        }

        Debug.Break();
    }
}
