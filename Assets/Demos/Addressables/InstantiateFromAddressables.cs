using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class InstantiateFromAddressables : MonoBehaviour
{
    [SerializeField]
    private string m_Key;

    [SerializeField]
    private Text m_Log;

    private readonly StringBuilder _sb = new StringBuilder(1024);

    public void Execute()
    {
        StartCoroutine(Co_Execute());
    }

    private IEnumerator Co_Execute()
    {
        if (string.IsNullOrEmpty(m_Key))
        {
            AddLog($"Key is empty.");
        }

        AddLog($"Instantiating '{m_Key}'...");
        var handle = Addressables.InstantiateAsync(m_Key, transform);

        while (handle.IsDone == false)
        {
            yield return new WaitForSeconds(0.1f);
            AddLog($"Progress: {handle.PercentComplete:P}");
        }

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            AddLog($"Instantiated '{m_Key}' successfully.");
        }
        else
        {
            AddLog($"Failed to instantiate '{m_Key}'.");
            AddLog($"{handle.OperationException.Message}");
        }
    }

    private void AddLog(string message)
    {
        _sb.AppendLine(message);
        if (m_Log)
            m_Log.text = _sb.ToString();
    }
}
