using System;
using System.Collections.Generic;
using Coffee.UISoftMask;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DynamicResolution : MonoBehaviour
{
    [SerializeField] private Canvas m_Canvas;

    private float _horizontalScale = 1.0f;
    private float _verticalScale = 1.0f;
    private readonly List<SoftMask> _softMasks = new List<SoftMask>();

    private void Start()
    {
        m_Canvas.GetComponentsInChildren(true, _softMasks);
    }

    private void OnDisable()
    {
        SetRenderScale(1.0f);
        ScalableBufferManager.ResizeBuffers(1, 1);
    }

    public void SetHorizontalScale(float scale)
    {
        _horizontalScale = scale;
        ScalableBufferManager.ResizeBuffers(_horizontalScale, _verticalScale);
        NotifyStencilStateChanged();
    }

    public void SetVerticalScale(float scale)
    {
        _verticalScale = scale;
        ScalableBufferManager.ResizeBuffers(_horizontalScale, _verticalScale);
        NotifyStencilStateChanged();
    }

    public void SetRenderScale(float scale)
    {
        var pipelineAsset = GraphicsSettings.currentRenderPipeline;
        if (pipelineAsset)
        {
            pipelineAsset
                .GetType()
                .GetProperty("renderScale")
                .SetValue(pipelineAsset, scale);
            NotifyStencilStateChanged();
        }
    }

    public void SetRenderMode(int renderMode)
    {
        var mode = (RenderMode)renderMode;
        if (mode == RenderMode.WorldSpace)
        {
            SetRenderMode((int)RenderMode.ScreenSpaceCamera);
            m_Canvas.renderMode = RenderMode.WorldSpace;
            m_Canvas.worldCamera.transform.rotation = Quaternion.Euler(5, 5, 5);
        }
        else
        {
            m_Canvas.renderMode = mode;
            m_Canvas.worldCamera.transform.rotation = Quaternion.identity;
        }

        NotifyStencilStateChanged();
    }

    public void SetAllowDynamicResolution(bool value)
    {
        m_Canvas.worldCamera.allowDynamicResolution = value;
        NotifyStencilStateChanged();
    }

    private void NotifyStencilStateChanged()
    {
        foreach (var softMask in _softMasks)
        {
            if (!softMask || !softMask.isActiveAndEnabled) continue;
            MaskUtilities.NotifyStencilStateChanged(softMask);
        }
    }
}
