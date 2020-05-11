using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using MaskIntr = UnityEngine.SpriteMaskInteraction;

namespace Coffee.UISoftMask
{
    /// <summary>
    /// Soft maskable.
    /// Add this component to Graphic under SoftMask for smooth masking.
    /// </summary>
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
	[ExecuteInEditMode]
# endif
    public class SoftMaskable : MonoBehaviour, IMaterialModifier, ICanvasRaycastFilter
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
# endif
    {
        const int kVisibleInside = (1 << 0) + (1 << 2) + (1 << 4) + (1 << 6);
        const int kVisibleOutside = (2 << 0) + (2 << 2) + (2 << 4) + (2 << 6);
        static readonly Hash128 k_InvalidHash = new Hash128();

        static int s_SoftMaskTexId;
        static int s_StencilCompId;
        static int s_MaskInteractionId;
        static List<SoftMaskable> s_ActiveSoftMaskables;
        static int[] s_Interactions = new int[4];

        [Tooltip("The graphic will be visible only in areas where no mask is present.")]
        [System.Obsolete]
        [HideInInspector]
        [SerializeField]
        bool m_Inverse = false;

        [Tooltip("The interaction for each masks.")] [HideInInspector] [SerializeField]
        int m_MaskInteraction = kVisibleInside;

        [Tooltip("Use stencil to mask.")] [SerializeField]
        bool m_UseStencil = false;

        [Tooltip("Use soft-masked raycast target.\n\nNote: This option is expensive.")] [SerializeField]
        bool m_RaycastFilter = false;

        Graphic _graphic = null;
        SoftMask _softMask = null;
        Material _maskMaterial = null;
        Hash128 _effectMaterialHash;

        /// <summary>
        /// The graphic will be visible only in areas where no mask is present.
        /// </summary>
        public bool inverse
        {
            get { return m_MaskInteraction == kVisibleOutside; }
            set
            {
                var intValue = value ? kVisibleOutside : kVisibleInside;
                if (m_MaskInteraction == intValue) return;
                m_MaskInteraction = intValue;
                graphic.SetMaterialDirtyEx();
            }
        }

        /// <summary>
        /// Use soft-masked raycast target. This option is expensive.
        /// </summary>
        public bool raycastFilter
        {
            get { return m_RaycastFilter; }
            set { m_RaycastFilter = value; }
        }

        /// <summary>
        /// The graphic associated with the soft mask.
        /// </summary>
        public Graphic graphic
        {
            get { return _graphic ? _graphic : _graphic = GetComponent<Graphic>(); }
        }

        /// <summary>
        /// Perform material modification in this function.
        /// </summary>
        /// <returns>Modified material.</returns>
        /// <param name="baseMaterial">Configured Material.</param>
        Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial)
        {
            _softMask = null;

            // Unregister the previous material
            MaterialCache.Unregister(_effectMaterialHash);
            _effectMaterialHash = k_InvalidHash;

            // If this component is disabled, the material is returned as is.
            if (!isActiveAndEnabled) return baseMaterial;

            // Find the nearest parent softmask.
            var parentTransform = transform.parent;
            while (parentTransform)
            {
                var sm = parentTransform.GetComponent<SoftMask>();
                if (sm && sm.enabled)
                {
                    _softMask = sm;
                    break;
                }

                parentTransform = parentTransform.parent;
            }

            // If the parents do not have a soft mask component, the material is returned as is.
            if (!_softMask) return baseMaterial;

            // Generate soft maskable material.
            _effectMaterialHash = new Hash128(
                (uint) baseMaterial.GetInstanceID(),
                (uint) _softMask.GetInstanceID(),
                (uint) m_MaskInteraction,
                (uint) (m_UseStencil ? 1 : 0)
            );

            // Generate soft maskable material.
            var modifiedMaterial = MaterialCache.Register(baseMaterial, _effectMaterialHash, mat =>
            {
                mat.shader = Shader.Find(string.Format("Hidden/{0} (SoftMaskable)", mat.shader.name));
#if UNITY_EDITOR
                mat.EnableKeyword("SOFTMASK_EDITOR");
#endif
                mat.SetTexture(s_SoftMaskTexId, _softMask.softMaskBuffer);
                mat.SetInt(s_StencilCompId,
                    m_UseStencil ? (int) CompareFunction.Equal : (int) CompareFunction.Always);
                mat.SetVector(s_MaskInteractionId, new Vector4(
                    (m_MaskInteraction & 0x3),
                    ((m_MaskInteraction >> 2) & 0x3),
                    ((m_MaskInteraction >> 4) & 0x3),
                    ((m_MaskInteraction >> 6) & 0x3)
                ));
            });
            _maskMaterial = modifiedMaterial;

            return modifiedMaterial;
        }

        /// <summary>
        /// Given a point and a camera is the raycast valid.
        /// </summary>
        /// <returns>Valid.</returns>
        /// <param name="sp">Screen position.</param>
        /// <param name="eventCamera">Raycast camera.</param>
        bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (!isActiveAndEnabled || !_softMask)
                return true;
            if (!RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, sp, eventCamera))
                return false;
            if (!m_RaycastFilter)
                return true;

            var sm = _softMask;
            for (var i = 0; i < 4; i++)
            {
                s_Interactions[i] = sm ? ((m_MaskInteraction >> i * 2) & 0x3) : 0;
                sm = sm ? sm.parent : null;
            }

            return _softMask.IsRaycastLocationValid(sp, eventCamera, graphic, s_Interactions);
        }

        /// <summary>
        /// Set the interaction for each mask.
        /// </summary>
        public void SetMaskInteraction(SpriteMaskInteraction intr)
        {
            SetMaskInteraction(intr, intr, intr, intr);
        }

        /// <summary>
        /// Set the interaction for each mask.
        /// </summary>
        public void SetMaskInteraction(SpriteMaskInteraction layer0, SpriteMaskInteraction layer1,
            SpriteMaskInteraction layer2, SpriteMaskInteraction layer3)
        {
            m_MaskInteraction = (int) layer0 + ((int) layer1 << 2) + ((int) layer2 << 4) + ((int) layer3 << 6);
            graphic.SetMaterialDirtyEx();
        }


        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            // Register.
            if (s_ActiveSoftMaskables == null)
            {
                s_ActiveSoftMaskables = new List<SoftMaskable>();

                s_SoftMaskTexId = Shader.PropertyToID("_SoftMaskTex");
                s_StencilCompId = Shader.PropertyToID("_StencilComp");
                s_MaskInteractionId = Shader.PropertyToID("_MaskInteraction");
            }

            s_ActiveSoftMaskables.Add(this);

            graphic.SetMaterialDirtyEx();
            _softMask = null;
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled.
        /// </summary>
        private void OnDisable()
        {
            s_ActiveSoftMaskables.Remove(this);

            graphic.SetMaterialDirtyEx();
            _softMask = null;

            MaterialCache.Unregister(_effectMaterialHash);
            _effectMaterialHash = k_InvalidHash;
        }

#if UNITY_EDITOR
        /// <summary>
        /// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
        /// </summary>
        private void OnValidate()
        {
            graphic.SetMaterialDirtyEx();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#pragma warning disable 0612
            if (m_Inverse)
            {
                m_Inverse = false;
                m_MaskInteraction = (2 << 0) + (2 << 2) + (2 << 4) + (2 << 6);
            }
#pragma warning restore 0612

            var current = this;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (current && graphic && graphic.material && graphic.material.shader &&
                    graphic.material.shader.name == "Hidden/UI/Default (SoftMaskable)")
                {
                    graphic.material = null;
                    graphic.SetMaterialDirtyEx();
                }
            };
#endif
        }
    }
}
