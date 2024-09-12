#if SHADERGRAPH_ENABLE
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Coffee.UISoftMask
{
    [HelpURL("https://github.com/mob-sakai/SoftMaskForUGUI")]
    [DisallowMultipleRendererFeature("SoftMaskable ShaderGraph Support (Editor)")]
    [Tooltip("Enable SoftMaskable ShaderGraph support in the editor. This will not affect runtime behavior.")]
    public class SoftMaskableShaderGraphSupportFeature : ScriptableRendererFeature
    {
#if UNITY_EDITOR
        private class Pass : ScriptableRenderPass
        {
            private static readonly int s_SoftMaskInGameView = Shader.PropertyToID("_SoftMaskInGameView");
            private static readonly int s_SoftMaskInSceneView = Shader.PropertyToID("_SoftMaskInSceneView");

            private class Data
            {
                public bool isGameView;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                using (var builder = renderGraph.AddRasterRenderPass<Data>("SoftMaskableShaderGraphSupport", out var d))
                {
                    var cam = frameData.Get<UniversalCameraData>();
                    d.isGameView = cam.cameraType == CameraType.Game;
                    builder.AllowPassCulling(false);
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc<Data>((x, context) =>
                    {
                        context.cmd.SetGlobalInt(s_SoftMaskInGameView, x.isGameView ? 1 : 0);
                        context.cmd.SetGlobalInt(s_SoftMaskInSceneView, !x.isGameView ? 1 : 0);
                    });
                }
            }

            [Obsolete(
                "This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.",
                false)]
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var isGameView = renderingData.cameraData.cameraType == CameraType.Game;
                cmd.SetGlobalInt(s_SoftMaskInGameView, isGameView ? 1 : 0);
                cmd.SetGlobalInt(s_SoftMaskInSceneView, !isGameView ? 1 : 0);
            }

            [Obsolete(
                "This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.",
                false)]
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                cmd.SetGlobalInt(s_SoftMaskInGameView, 0);
                cmd.SetGlobalInt(s_SoftMaskInSceneView, 0);
            }
        }

        private Pass _pass;

        public override void Create()
        {
            _pass = new Pass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_pass);
        }
#else
        public override void Create()
        {
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
        }
#endif
    }
}
#endif
