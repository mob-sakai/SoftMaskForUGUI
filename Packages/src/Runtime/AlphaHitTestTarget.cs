using Coffee.UISoftMaskInternal;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UISoftMask
{
    /// <summary>
    /// Alpha-based hit testing on a UI element
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    [DisallowMultipleComponent]
    [Icon("Packages/com.coffee.softmask-for-ugui/Icons/SoftMaskIcon.png")]
    public class AlphaHitTestTarget : MonoBehaviour, ICanvasRaycastFilter
    {
        private Graphic _graphic;

        /// <summary>
        /// Graphic component on this GameObject.
        /// </summary>
        public Graphic graphic => _graphic || TryGetComponent(out _graphic) ? _graphic : null;

        private void OnEnable() { }

        private void OnDisable() { }

        /// <summary>
        /// This method is called during canvas ray-casting to determine if the hit point is valid or not.
        /// </summary>
        bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            // Check if this component is active and enabled, if the Graphic component is not null, and if the Graphic is active.
            if (!isActiveAndEnabled || !graphic || !graphic.IsActive()) return true;

            // Perform alpha-based hit testing.
            return Utils.AlphaHitTestValid(graphic, sp, eventCamera, 0.01f);
        }
    }
}
