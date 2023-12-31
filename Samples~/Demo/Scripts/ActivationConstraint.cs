using System;
using UnityEngine;

namespace Coffee.UISoftMask.Demos
{
    [ExecuteAlways]
    public class ActivationConstraint : MonoBehaviour
    {
        [SerializeField]
        private bool m_Reverse;

        [SerializeField]
        private GameObject[] m_Targets = Array.Empty<GameObject>();

        private void OnEnable()
        {
            SetEnableTargets(true);
        }

        private void OnDisable()
        {
            SetEnableTargets(false);
        }

        private void SetEnableTargets(bool enable)
        {
            if (m_Targets == null) return;
            foreach (var target in m_Targets)
            {
                if (target) target.SetActive(enable != m_Reverse);
            }
        }
    }
}
