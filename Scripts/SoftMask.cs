using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Coffee.UISoftMask
{
    /// <summary>
    /// Soft mask.
    /// Use instead of Mask for smooth masking.
    /// </summary>
    public class SoftMask : Mask, IMeshModifier
    {
        /// <summary>
        /// Desampling rate.
        /// </summary>
        public enum DesamplingRate
        {
            None = 0,
            x1 = 1,
            x2 = 2,
            x4 = 4,
            x8 = 8,
        }

        private static readonly List<SoftMask>[] s_TmpSoftMasks = new List<SoftMask>[]
        {
            new List<SoftMask>(),
            new List<SoftMask>(),
            new List<SoftMask>(),
            new List<SoftMask>(),
        };

        private static readonly Color[] s_ClearColors = new Color[]
        {
            new Color(0, 0, 0, 0),
            new Color(1, 0, 0, 0),
            new Color(1, 1, 0, 0),
            new Color(1, 1, 1, 0),
        };

        private static bool s_UVStartsAtTop;
        private static Shader s_SoftMaskShader;
        private static Texture2D s_ReadTexture;
        private static readonly List<SoftMask> s_ActiveSoftMasks = new List<SoftMask>();
        private static readonly List<SoftMask> s_TempRelatables = new List<SoftMask>();
        private static readonly Dictionary<int, Matrix4x4> s_PreviousViewProjectionMatrices = new Dictionary<int, Matrix4x4>();
        private static readonly Dictionary<int, Matrix4x4> s_NowViewProjectionMatrices = new Dictionary<int, Matrix4x4>();
        private static int s_StencilCompId;
        private static int s_ColorMaskId;
        private static int s_MainTexId;
        private static int s_SoftnessId;
        private static int s_GameVPId;
        private static int s_GameTVPId;
        private static int s_Alpha;
        private MaterialPropertyBlock _mpb;
        private CommandBuffer _cb;
        private Material _material;
        private RenderTexture _softMaskBuffer;
        private int _stencilDepth;
        private Mesh _mesh;
        private SoftMask _parent;
        internal readonly List<SoftMask> _children = new List<SoftMask>();
        private bool _hasChanged = false;
        private bool _hasStencilStateChanged = false;


        [SerializeField, Tooltip("The desampling rate for soft mask buffer.")]
        private DesamplingRate m_DesamplingRate = DesamplingRate.x1;

        [SerializeField, Range(0.01f, 1), Tooltip("The value used by the soft mask to select the area of influence defined over the soft mask's graphic.")]
        private float m_Softness = 1;

        [SerializeField, Range(0f, 1f), Tooltip("The transparency of the whole masked graphic.")]
        private float m_Alpha = 1;

        [SerializeField, Tooltip("Should the soft mask ignore parent soft masks?")]
        private bool m_IgnoreParent = false;

        [SerializeField, Tooltip("Is the soft mask a part of parent soft mask?")]
        private bool m_PartOfParent = false;

        [SerializeField, Tooltip("Self graphic will not be drawn to soft mask buffer.")]
        private bool m_IgnoreSelfGraphic;


        [SerializeField, Tooltip("Self graphic will not be written to stencil buffer.")]
        private bool m_IgnoreSelfStencil;

        /// <summary>
        /// The desampling rate for soft mask buffer.
        /// </summary>
        public DesamplingRate desamplingRate
        {
            get { return m_DesamplingRate; }
            set
            {
                if (m_DesamplingRate == value) return;
                m_DesamplingRate = value;
                hasChanged = true;
            }
        }

        /// <summary>
        /// The value used by the soft mask to select the area of influence defined over the soft mask's graphic.
        /// </summary>
        public float softness
        {
            get { return m_Softness; }
            set
            {
                value = Mathf.Clamp01(value);
                if (Mathf.Approximately(m_Softness, value)) return;
                m_Softness = value;
                hasChanged = true;
            }
        }

        /// <summary>
        /// The transparency of the whole masked graphic.
        /// </summary>
        public float alpha
        {
            get { return m_Alpha; }
            set
            {
                value = Mathf.Clamp01(value);
                if (Mathf.Approximately(m_Alpha, value)) return;
                m_Alpha = value;
                hasChanged = true;
            }
        }

        /// <summary>
        /// Should the soft mask ignore parent soft masks?
        /// </summary>
        /// <value>If set to true the soft mask will ignore any parent soft mask settings.</value>
        public bool ignoreParent
        {
            get { return m_IgnoreParent; }
            set
            {
                if (m_IgnoreParent == value) return;
                m_IgnoreParent = value;
                hasChanged = true;
                OnTransformParentChanged();
            }
        }

        /// <summary>
        /// Is the soft mask a part of parent soft mask?
        /// </summary>
        public bool partOfParent
        {
            get { return m_PartOfParent; }
            set
            {
                if (m_PartOfParent == value) return;
                m_PartOfParent = value;
                hasChanged = true;
                OnTransformParentChanged();
            }
        }

        /// <summary>
        /// The soft mask buffer.
        /// </summary>
        public RenderTexture softMaskBuffer
        {
            get
            {
                if (_parent)
                {
                    ReleaseRt(ref _softMaskBuffer);
                    return _parent.softMaskBuffer;
                }

                // Check the size of soft mask buffer.
                int w, h;
                GetDesamplingSize(m_DesamplingRate, out w, out h);
                if (_softMaskBuffer && (_softMaskBuffer.width != w || _softMaskBuffer.height != h))
                {
                    ReleaseRt(ref _softMaskBuffer);
                }

                if (!_softMaskBuffer)
                {
                    _softMaskBuffer = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32,
                        RenderTextureReadWrite.Default);
                    hasChanged = true;
                    _hasStencilStateChanged = true;
                }

                return _softMaskBuffer;
            }
        }

        public bool hasChanged
        {
            get { return _parent ? _parent.hasChanged : _hasChanged; }
            private set
            {
                if (_parent)
                {
                    _parent.hasChanged = value;
                }

                _hasChanged = value;
            }
        }

        public SoftMask parent
        {
            get { return _parent; }
        }

        public bool ignoreSelfGraphic
        {
            get { return m_IgnoreSelfGraphic; }
            set
            {
                if (m_IgnoreSelfGraphic == value) return;
                m_IgnoreSelfGraphic = value;
                hasChanged = true;
                graphic.SetVerticesDirtyEx();
            }
        }

        public bool ignoreSelfStencil
        {
            get { return m_IgnoreSelfStencil; }
            set
            {
                if (m_IgnoreSelfStencil == value) return;
                m_IgnoreSelfStencil = value;
                hasChanged = true;
                graphic.SetVerticesDirtyEx();
                graphic.SetMaterialDirtyEx();
            }
        }



        Material material
        {
            get
            {
                return _material
                    ? _material
                    : _material =
                        new Material(s_SoftMaskShader
                                ? s_SoftMaskShader
                                : s_SoftMaskShader = Resources.Load<Shader>("SoftMask"))
                            {hideFlags = HideFlags.HideAndDontSave};
            }
        }

        Mesh mesh
        {
            get { return _mesh ? _mesh : _mesh = new Mesh() {hideFlags = HideFlags.HideAndDontSave}; }
        }


        /// <summary>
        /// Perform material modification in this function.
        /// </summary>
        /// <returns>Modified material.</returns>
        /// <param name="baseMaterial">Configured Material.</param>
        public override Material GetModifiedMaterial(Material baseMaterial)
        {
            hasChanged = true;
            if (ignoreSelfStencil) return baseMaterial;

            var result = base.GetModifiedMaterial(baseMaterial);
            if (m_IgnoreParent && result != baseMaterial)
            {
                result.SetInt(s_StencilCompId, (int) CompareFunction.Always);
            }

            return result;
        }


        /// <summary>
        /// Call used to modify mesh.
        /// </summary>
        void IMeshModifier.ModifyMesh(Mesh mesh)
        {
            hasChanged = true;
            _mesh = mesh;
        }

        /// <summary>
        /// Call used to modify mesh.
        /// </summary>
        void IMeshModifier.ModifyMesh(VertexHelper verts)
        {
            if (isActiveAndEnabled)
            {
                if (ignoreSelfGraphic)
                {
                    verts.Clear();
                    verts.FillMesh(mesh);
                }
                else if (ignoreSelfStencil)
                {
                    verts.FillMesh(mesh);
                    verts.Clear();
                }
                else
                {
                    verts.FillMesh(mesh);
                }
            }

            hasChanged = true;
        }

        /// <summary>
        /// Given a point and a camera is the raycast valid.
        /// </summary>
        /// <returns>Valid.</returns>
        /// <param name="sp">Screen position.</param>
        /// <param name="eventCamera">Raycast camera.</param>
        /// <param name="g">Target graphic.</param>
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera, Graphic g, int[] interactions)
        {
            if (!isActiveAndEnabled || (g == graphic && !g.raycastTarget)) return true;

            int x = (int) ((softMaskBuffer.width - 1) * Mathf.Clamp01(sp.x / Screen.width));
            int y = s_UVStartsAtTop
                ? (int) ((softMaskBuffer.height - 1) * (1 - Mathf.Clamp01(sp.y / Screen.height)))
                : (int) ((softMaskBuffer.height - 1) * Mathf.Clamp01(sp.y / Screen.height));
            return 0.5f < GetPixelValue(x, y, interactions);
        }

        public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return true;
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            hasChanged = true;

            // Register.
            if (s_ActiveSoftMasks.Count == 0)
            {
                Canvas.willRenderCanvases += UpdateMaskTextures;

                if (s_StencilCompId == 0)
                {
                    s_UVStartsAtTop = SystemInfo.graphicsUVStartsAtTop;
                    s_StencilCompId = Shader.PropertyToID("_StencilComp");
                    s_ColorMaskId = Shader.PropertyToID("_ColorMask");
                    s_MainTexId = Shader.PropertyToID("_MainTex");
                    s_SoftnessId = Shader.PropertyToID("_Softness");
                    s_Alpha = Shader.PropertyToID("_Alpha");
#if UNITY_EDITOR
                    s_GameVPId = Shader.PropertyToID("_GameVP");
                    s_GameTVPId = Shader.PropertyToID("_GameTVP");
#endif
                }
            }

            s_ActiveSoftMasks.Add(this);

            // Reset the parent-child relation.
            GetComponentsInChildren<SoftMask>(false, s_TempRelatables);
            for (int i = s_TempRelatables.Count - 1; 0 <= i; i--)
            {
                s_TempRelatables[i].OnTransformParentChanged();
            }

            s_TempRelatables.Clear();

            // Create objects.
            _mpb = new MaterialPropertyBlock();
            _cb = new CommandBuffer();

            graphic.SetVerticesDirtyEx();

            base.OnEnable();
            _hasStencilStateChanged = false;
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled.
        /// </summary>
        protected override void OnDisable()
        {
            // Unregister.
            s_ActiveSoftMasks.Remove(this);
            if (s_ActiveSoftMasks.Count == 0)
            {
                Canvas.willRenderCanvases -= UpdateMaskTextures;
            }

            // Reset the parent-child relation.
            for (int i = _children.Count - 1; 0 <= i; i--)
            {
                _children[i].SetParent(_parent);
            }

            _children.Clear();
            SetParent(null);

            // Destroy objects.
            _mpb.Clear();
            _mpb = null;
            _cb.Release();
            _cb = null;

            ReleaseObject(_mesh);
            _mesh = null;
            ReleaseObject(_material);
            _material = null;
            ReleaseRt(ref _softMaskBuffer);

            base.OnDisable();
            _hasStencilStateChanged = false;
        }

        /// <summary>
        /// This function is called when the parent property of the transform of the GameObject has changed.
        /// </summary>
        protected override void OnTransformParentChanged()
        {
            hasChanged = true;
            SoftMask newParent = null;
            if (isActiveAndEnabled && !m_IgnoreParent)
            {
                var parentTransform = transform.parent;
                while (parentTransform && (!newParent || !newParent.enabled))
                {
                    newParent = parentTransform.GetComponent<SoftMask>();
                    parentTransform = parentTransform.parent;
                }
            }

            SetParent(newParent);
            hasChanged = true;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            hasChanged = true;
        }

#if UNITY_EDITOR
        /// <summary>
        /// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
        /// </summary>
        protected override void OnValidate()
        {
            graphic.SetVerticesDirtyEx();
            graphic.SetMaterialDirtyEx();
            OnTransformParentChanged();
            base.OnValidate();
            _hasStencilStateChanged = false;
        }
#endif

        /// <summary>
        /// Update all soft mask textures.
        /// </summary>
        static void UpdateMaskTextures()
        {
            Profiler.BeginSample("UpdateMaskTextures");
            foreach (var sm in s_ActiveSoftMasks)
            {
                if (!sm || sm._hasChanged)
                    continue;

                var canvas = sm.graphic.canvas;
                if (!canvas)
                    continue;

                if (canvas.renderMode == RenderMode.WorldSpace)
                {
                    var cam = canvas.worldCamera;
                    if (!cam)
                        continue;

                    Profiler.BeginSample("Check view projection matrix changed (world space)");
                    var nowVP = cam.projectionMatrix * cam.worldToCameraMatrix;
                    var previousVP = default(Matrix4x4);
                    var id = cam.GetInstanceID();
                    s_PreviousViewProjectionMatrices.TryGetValue(id, out previousVP);
                    s_NowViewProjectionMatrices[id] = nowVP;

                    if (previousVP != nowVP)
                    {
                        sm.hasChanged = true;
                    }

                    Profiler.EndSample();
                }

                var rt = sm.rectTransform;
                if (rt.hasChanged)
                {
                    rt.hasChanged = false;
                    sm.hasChanged = true;
                }
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    sm.hasChanged = true;
                }
#endif
            }

            Profiler.BeginSample("Update changed soft masks");
            foreach (var sm in s_ActiveSoftMasks)
            {
                if (!sm || !sm._hasChanged)
                    continue;

                sm._hasChanged = false;
                if (sm._parent) continue;
                sm.UpdateMaskTexture();

                if (!sm._hasStencilStateChanged) continue;
                sm._hasStencilStateChanged = false;

                Profiler.BeginSample("Notify stencil state changed");
                MaskUtilities.NotifyStencilStateChanged(sm);
                Profiler.EndSample();
            }

            Profiler.EndSample();

            Profiler.BeginSample("Update previous view projection matrices");
            s_PreviousViewProjectionMatrices.Clear();
            foreach (var kv in s_NowViewProjectionMatrices)
            {
                s_PreviousViewProjectionMatrices.Add(kv.Key, kv.Value);
            }

            s_NowViewProjectionMatrices.Clear();
            Profiler.EndSample();

            Profiler.EndSample();
        }

        /// <summary>
        /// Update the mask texture.
        /// </summary>
        private void UpdateMaskTexture()
        {
            if (!graphic || !graphic.canvas) return;
            Profiler.BeginSample("UpdateMaskTexture");


            _stencilDepth =
                MaskUtilities.GetStencilDepth(transform, MaskUtilities.FindRootSortOverrideCanvas(transform));

            // Collect children soft masks.
            Profiler.BeginSample("Collect children soft masks");
            var depth = 0;
            s_TmpSoftMasks[0].Add(this);
            while (_stencilDepth + depth < 3)
            {
                var count = s_TmpSoftMasks[depth].Count;
                for (var i = 0; i < count; i++)
                {
                    List<SoftMask> children = s_TmpSoftMasks[depth][i]._children;
                    var childCount = children.Count;
                    for (var j = 0; j < childCount; j++)
                    {
                        var child = children[j];
                        var childDepth = child.m_PartOfParent ? depth : depth + 1;
                        s_TmpSoftMasks[childDepth].Add(child);
                    }
                }

                depth++;
            }

            Profiler.EndSample();

            // CommandBuffer.
            Profiler.BeginSample("Initialize CommandBuffer");
            _cb.Clear();
            _cb.SetRenderTarget(softMaskBuffer);
            _cb.ClearRenderTarget(false, true, s_ClearColors[_stencilDepth]);
            Profiler.EndSample();

            // Set view and projection matrices.
            Profiler.BeginSample("Set view and projection matrices");
            var c = graphic.canvas.rootCanvas;
            var cam = c.worldCamera ?? Camera.main;
            if (c && c.renderMode != RenderMode.ScreenSpaceOverlay && cam)
            {
                _cb.SetViewProjectionMatrices(cam.worldToCameraMatrix,
                    GL.GetGPUProjectionMatrix(cam.projectionMatrix, false));

#if UNITY_EDITOR
                var pv = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix;
                _cb.SetGlobalMatrix(s_GameVPId, pv);
                _cb.SetGlobalMatrix(s_GameTVPId, pv);
#endif
            }
            else
            {
                var pos = c.transform.position;
                var vm = Matrix4x4.TRS(new Vector3(-pos.x, -pos.y, -1000), Quaternion.identity, new Vector3(1, 1, -1f));
                var pm = Matrix4x4.TRS(new Vector3(0, 0, -1), Quaternion.identity,
                    new Vector3(1 / pos.x, 1 / pos.y, -2 / 10000f));
                _cb.SetViewProjectionMatrices(vm, pm);

#if UNITY_EDITOR
                var scale = c.transform.localScale.x;
                var size = (c.transform as RectTransform).sizeDelta;
                _cb.SetGlobalMatrix(s_GameVPId,
                    Matrix4x4.TRS(new Vector3(0, 0, 0.5f), Quaternion.identity,
                        new Vector3(2 / size.x, 2 / size.y, 0.0005f * scale)));
                _cb.SetGlobalMatrix(s_GameTVPId,
                    Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity,
                        new Vector3(1 / pos.x, 1 / pos.y, -2 / 2000f)) * Matrix4x4.Translate(-pos));
#endif
            }

            Profiler.EndSample();

            // Draw soft masks.
            Profiler.BeginSample("Draw Mesh");
            for (var i = 0; i < s_TmpSoftMasks.Length; i++)
            {
                var count = s_TmpSoftMasks[i].Count;
                for (var j = 0; j < count; j++)
                {
                    var sm = s_TmpSoftMasks[i][j];

                    if (i != 0)
                    {
                        sm._stencilDepth = MaskUtilities.GetStencilDepth(sm.transform,
                            MaskUtilities.FindRootSortOverrideCanvas(sm.transform));
                    }

                    // Set material property.
                    sm.material.SetInt(s_ColorMaskId, (int) 1 << (3 - _stencilDepth - i));
                    sm._mpb.SetTexture(s_MainTexId, sm.graphic.mainTexture);
                    sm._mpb.SetFloat(s_SoftnessId, sm.m_Softness);
                    sm._mpb.SetFloat(s_Alpha, sm.m_Alpha);

                    // Draw mesh.
                    _cb.DrawMesh(sm.mesh, sm.transform.localToWorldMatrix, sm.material, 0, 0, sm._mpb);
                }

                s_TmpSoftMasks[i].Clear();
            }

            Profiler.EndSample();

            Graphics.ExecuteCommandBuffer(_cb);
            Profiler.EndSample();
        }

        /// <summary>
        /// Gets the size of the desampling.
        /// </summary>
        private static void GetDesamplingSize(DesamplingRate rate, out int w, out int h)
        {
#if UNITY_EDITOR
            var res = UnityEditor.UnityStats.screenRes.Split('x');
            w = int.Parse(res[0]);
            h = int.Parse(res[1]);
#else
			w = Screen.width;
			h = Screen.height;
#endif

            if (rate == DesamplingRate.None)
                return;

            var aspect = (float) w / h;
            if (w < h)
            {
                h = Mathf.ClosestPowerOfTwo(h / (int) rate);
                w = Mathf.CeilToInt(h * aspect);
            }
            else
            {
                w = Mathf.ClosestPowerOfTwo(w / (int) rate);
                h = Mathf.CeilToInt(w / aspect);
            }
        }

        /// <summary>
        /// Release the specified obj.
        /// </summary>
        /// <param name="obj">Object.</param>
        private static void ReleaseRt(ref RenderTexture tmpRT)
        {
            if (!tmpRT) return;
            tmpRT.Release();
            RenderTexture.ReleaseTemporary(tmpRT);
            tmpRT = null;
        }

        /// <summary>
        /// Release the specified obj.
        /// </summary>
        /// <param name="obj">Object.</param>
        private static void ReleaseObject(Object obj)
        {
            if (!obj) return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(obj);
            else
#endif
                Destroy(obj);
            obj = null;
        }


        /// <summary>
        /// Set the parent of the soft mask.
        /// </summary>
        /// <param name="newParent">The parent soft mask to use.</param>
        private void SetParent(SoftMask newParent)
        {
            if (_parent != newParent && this != newParent)
            {
                if (_parent && _parent._children.Contains(this))
                {
                    _parent._children.Remove(this);
                    _parent._children.RemoveAll(x => x == null);
                }

                _parent = newParent;
            }

            if (_parent && !_parent._children.Contains(this))
            {
                _parent._children.Add(this);
            }
        }

        /// <summary>
        /// Gets the pixel value.
        /// </summary>
        private float GetPixelValue(int x, int y, int[] interactions)
        {
            if (!s_ReadTexture)
            {
                s_ReadTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            }

            var currentRt = RenderTexture.active;

            RenderTexture.active = softMaskBuffer;
            s_ReadTexture.ReadPixels(new Rect(x, y, 1, 1), 0, 0);
            s_ReadTexture.Apply(false, false);
            RenderTexture.active = currentRt;

            var colors = s_ReadTexture.GetRawTextureData();

            for (int i = 0; i < 4; i++)
            {
                switch (interactions[(i + 3) % 4])
                {
                    case 0:
                        colors[i] = 255;
                        break;
                    case 2:
                        colors[i] = (byte) (255 - colors[i]);
                        break;
                }
            }

            switch (_stencilDepth)
            {
                case 0: return (colors[1] / 255f);
                case 1: return (colors[1] / 255f) * (colors[2] / 255f);
                case 2: return (colors[1] / 255f) * (colors[2] / 255f) * (colors[3] / 255f);
                case 3: return (colors[1] / 255f) * (colors[2] / 255f) * (colors[3] / 255f) * (colors[0] / 255f);
                default: return 0;
            }
        }
    }
}
