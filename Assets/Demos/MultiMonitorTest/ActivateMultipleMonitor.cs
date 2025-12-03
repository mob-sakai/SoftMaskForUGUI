using UnityEngine;

public class ActivateMultipleMonitor : MonoBehaviour
{
    void Start()
    {
        Display.displays[0].Activate();
        Display.displays[1].Activate();     
    }
}
