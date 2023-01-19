using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
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
    [RequireComponent(typeof(Graphic))]
    public class SoftMaskable : MonoBehaviour, IMaterialModifier, ICanvasRaycastFilter
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
# endif
    {
        private const int kVisibleInside = (1 << 0) + (1 << 2) + (1 << 4) + (1 << 6);
        private const int kVisibleOutside = (2 << 0) + (2 << 2) + (2 << 4) + (2 << 6);
        private static readonly Hash128 k_InvalidHash = new Hash128();

        private static int s_SoftMaskTexId;
        private static int s_StencilCompId;
        private static int s_MaskInteractionId;
        private static int s_GameVPId;
        private static int s_GameTVPId;
        private static List<SoftMaskable> s_ActiveSoftMaskables;
        private static int[] s_Interactions = new int[4];

        [SerializeField, HideInInspector, System.Obsolete]
        private bool m_Inverse;

        [SerializeField, Tooltip("The interaction for each masks."), HideInInspector]
        private int m_MaskInteraction = kVisibleInside;

        [SerializeField, Tooltip("Use stencil to mask.")]
        private bool m_UseStencil = true;

        [SerializeField, Tooltip("Use soft-masked raycast target.\n\nNote: This option is expensive.")]
        private bool m_RaycastFilter;

        private Graphic _graphic;
        private SoftMask _softMask;
        private Hash128 _effectMaterialHash;

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
        /// Use stencil to mask.
        /// </summary>
        public bool useStencil
        {
            get { return m_UseStencil; }
            set
            {
                if (m_UseStencil == value) return;
                m_UseStencil = value;
                graphic.SetMaterialDirtyEx();
            }
        }

        /// <summary>
        /// The graphic associated with the soft mask.
        /// </summary>
        public Graphic graphic
        {
            get { return _graphic ? _graphic : _graphic = GetComponent<Graphic>(); }
        }

        public SoftMask softMask
        {
            get { return _softMask ? _softMask : _softMask = this.GetComponentInParentEx<SoftMask>(); }
        }

        public Material modifiedMaterial { get; private set; }

        /// <summary>
        /// Perform material modification in this function.
        /// </summary>
        /// <returns>Modified material.</returns>
        /// <param name="baseMaterial">Configured Material.</param>
        Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial)
        {
            _softMask = null;
            modifiedMaterial = null;

            // If this component is disabled, the material is returned as is.
            // If the parents do not have a soft mask component, the material is returned as is.
            if (!isActiveAndEnabled || !softMask)
            {
                MaterialCache.Unregister(_effectMaterialHash);
                _effectMaterialHash = k_InvalidHash;
                return baseMaterial;
            }

            // Generate soft maskable material.
            var previousHash = _effectMaterialHash;
            _effectMaterialHash = new Hash128(
                (uint) baseMaterial.GetInstanceID(),
                (uint) softMask.GetInstanceID(),
                (uint) m_MaskInteraction,
                (uint) (m_UseStencil ? 1 : 0)
            );

            // Generate soft maskable material.
            modifiedMaterial = MaterialCache.Register(baseMaterial, _effectMaterialHash, mat =>
            {
                mat.shader = Shader.Find(string.Format("Hidden/{0} (SoftMaskable)", mat.shader.name));
                mat.SetTexture(s_SoftMaskTexId, softMask.softMaskBuffer);
                mat.SetInt(s_StencilCompId, m_UseStencil ? (int) CompareFunction.Equal : (int) CompareFunction.Always);

#if UNITY_EDITOR
                mat.EnableKeyword("SOFTMASK_EDITOR");
                UpdateMaterialForSceneView(mat);
#endif

                var root = MaskUtilities.FindRootSortOverrideCanvas(transform);
                var stencil = MaskUtilities.GetStencilDepth(transform, root);
                mat.SetVector(s_MaskInteractionId, new Vector4(
                    1 <= stencil ? (m_MaskInteraction >> 0 & 0x3) : 0,
                    2 <= stencil ? (m_MaskInteraction >> 2 & 0x3) : 0,
                    3 <= stencil ? (m_MaskInteraction >> 4 & 0x3) : 0,
                    4 <= stencil ? (m_MaskInteraction >> 6 & 0x3) : 0
                ));
            });

            // Unregister the previous material.
            MaterialCache.Unregister(previousHash);

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
            if (!isActiveAndEnabled || !softMask)
                return true;
            if (!RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, sp, eventCamera))
                return false;

            if (m_RaycastFilter)
            {
                var sm = _softMask;
                for (var i = 0; i < 4; i++)
                {
                    s_Interactions[i] = sm ? ((m_MaskInteraction >> i * 2) & 0x3) : 0;
                    sm = sm ? sm.parent : null;
                }

                return _softMask.IsRaycastLocationValid(sp, eventCamera, graphic, s_Interactions);
            }
            else
            {
                var sm = _softMask;
                for (var i = 0; i < 4; i++)
                {
                    if (!sm) break;

                    s_Interactions[i] = sm ? ((m_MaskInteraction >> i * 2) & 0x3) : 0;
                    var interaction = s_Interactions[i] == 1;
                    var rt = sm.transform as RectTransform;
                    var inRect = RectTransformUtility.RectangleContainsScreenPoint(rt, sp, eventCamera);
                    if (!sm.ignoreSelfGraphic && interaction != inRect) return false;

                    foreach (var child in sm._children)
                    {
                        if (!child) continue;

                        var childRt = child.transform as RectTransform;
                        var inRectChild = RectTransformUtility.RectangleContainsScreenPoint(childRt, sp, eventCamera);
                        if (!child.ignoreSelfGraphic && interaction != inRectChild) return false;
                    }

                    sm = sm ? sm.parent : null;
                }

                return true;
            }
        }

        /// <summary>
        /// Set the interaction for each mask.
        /// </summary>
        public void SetMaskInteraction(MaskIntr intr)
        {
            SetMaskInteraction(intr, intr, intr, intr);
        }

        /// <summary>
        /// Set the interaction for each mask.
        /// </summary>
        public void SetMaskInteraction(MaskIntr layer0, MaskIntr layer1, MaskIntr layer2, MaskIntr layer3)
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

#if UNITY_EDITOR
                s_GameVPId = Shader.PropertyToID("_GameVP");
                s_GameTVPId = Shader.PropertyToID("_GameTVP");
                UnityEditor.SceneView.beforeSceneGui -= SceneView_beforeSceneGui; // For safety
                UnityEditor.SceneView.beforeSceneGui += SceneView_beforeSceneGui;
#endif
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
#if UNITY_EDITOR
            UnityEditor.SceneView.beforeSceneGui -= SceneView_beforeSceneGui;
#endif
        }

#if UNITY_EDITOR
        private void UpdateMaterialForSceneView(Material mat)
        {
            if(!mat || !graphic || !graphic.canvas || !mat.shader || !mat.shader.name.EndsWith(" (SoftMaskable)")) return;
            Debug.Log("UpdateMaterialForSceneView");
            // Set view and projection matrices.
            Profiler.BeginSample("Set view and projection matrices");
            var c = graphic.canvas.rootCanvas;
            var cam = c.worldCamera ?? Camera.main;
            if (c && c.renderMode != RenderMode.ScreenSpaceOverlay && cam)
            {
                var p = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
                var pv = p * cam.worldToCameraMatrix;
                mat.SetMatrix(s_GameVPId, pv);
                mat.SetMatrix(s_GameTVPId, pv);
            }
            else
            {
                var pos = c.transform.position;
                var scale = c.transform.localScale.x;
                var size = (c.transform as RectTransform).sizeDelta;
                var gameVp = Matrix4x4.TRS(new Vector3(0, 0, 0.5f), Quaternion.identity, new Vector3(2 / size.x, 2 / size.y, 0.0005f * scale));
                var gameTvp = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1 / pos.x, 1 / pos.y, -2 / 2000f)) * Matrix4x4.Translate(-pos);

                mat.SetMatrix(s_GameVPId, gameVp);
                mat.SetMatrix(s_GameTVPId, gameTvp);
            }
            Profiler.EndSample();
        }

        private void SceneView_beforeSceneGui(UnityEditor.SceneView obj)
        {
            
            var parentCanvas = GetComponentInParent<Canvas>(); // Don't think we can cache this in case this go is moved to another parent
            if (parentCanvas != null && parentCanvas.enabled) // Only do this expensive call if the UI element is active
                UpdateMaterialForSceneView(modifiedMaterial);
        }


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
                m_MaskInteraction = kVisibleOutside;
            }
#pragma warning restore 0612

            var current = this;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (!current) return;
                if (!graphic) return;
                if (graphic.name.Contains("TMP SubMeshUI")) return;
                if (!graphic.material) return;
                if (!graphic.material.shader) return;
                if (graphic.material.shader.name != "Hidden/UI/Default (SoftMaskable)") return;

                graphic.material = null;
                graphic.SetMaterialDirtyEx();
            };
        }
#endif
    }
}
