using UnityEngine;

public class ActivateMultipleMonitor : MonoBehaviour
{
    private void Start()
    {
        Display.displays[0].Activate();
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }
    }
}
