using UnityEngine;

namespace Coffee.UISoftMask
{
    [ExecuteAlways]
    public class ScreenResolution : MonoBehaviour
    {
        [SerializeField]
        private bool m_ApplyOnEnable;

        [SerializeField]
        private Vector2Int m_Size = new Vector2Int(1920, 1080);

        [SerializeField]
        private bool m_FullScreen;

        public bool applyOnEnable
        {
            get => m_ApplyOnEnable;
            set => m_ApplyOnEnable = value;
        }

        public Vector2Int size
        {
            get => m_Size;
            set => m_Size = value;
        }

        public bool fullScreen
        {
            get => m_FullScreen;
            set => m_FullScreen = value;
        }

        private void OnEnable()
        {
            if (m_ApplyOnEnable)
            {
                Apply();
            }
        }

        public void Apply()
        {
            Screen.SetResolution(size.x, size.y, fullScreen);
        }
    }
}
