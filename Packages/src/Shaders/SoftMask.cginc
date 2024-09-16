#ifndef UI_SOFT_MASK_INCLUDED
#define UI_SOFT_MASK_INCLUDED

uniform sampler2D _SoftMaskTex;
uniform half4 _SoftMaskColor;
uniform float _AlphaClipThreshold;
uniform float4 _SoftMaskOutsideColor;
uniform int _SoftMaskableEnable;
uniform int _SoftMaskableStereo;
uniform float4x4 _GameVP;
uniform float4x4 _GameTVP;
uniform float4x4 _GameVP_2;
uniform float4x4 _GameTVP_2;
uniform int _SoftMaskInGameView;
uniform int _SoftMaskInSceneView;
uniform int _MaskingShapeSubtract;

float Approximately(float4x4 a, float4x4 b)
{
    float4x4 d = abs(a - b);
    return step(
        max(d._m00,max(d._m01,max(d._m02,max(d._m03,
        max(d._m10,max(d._m11,max(d._m12,max(d._m13,
        max(d._m20,max(d._m21,max(d._m22,max(d._m23,
        max(d._m30,max(d._m31,max(d._m32,d._m33))))))))))))))),
        0.1);
}

float2 WorldToUv(float4 worldPos, float offset)
{
    worldPos = mul(unity_ObjectToWorld, worldPos);
    float4x4 gameVp = lerp(_GameVP, _GameVP_2, unity_StereoEyeIndex);
    float4x4 gameTvp = lerp(_GameTVP, _GameTVP_2, unity_StereoEyeIndex);

    float isSceneView = 1 - Approximately(UNITY_MATRIX_VP, gameVp);

    float4 clipPos = mul(UNITY_MATRIX_VP, worldPos);
    float4 clipPosG = mul(gameTvp, worldPos);
    return lerp(
        clipPos.xy / clipPos.w * 0.5 + 0.5,
        clipPosG.xy / clipPosG.w * 0.5 + offset,
        isSceneView);
}

float2 ScreenToUv(const float2 screenPos)
{
    half2 uv = screenPos;
    #if UNITY_PRETRANSFORM_TO_DISPLAY_ORIENTATION
    float ratio = _ScreenParams.x / _ScreenParams.y;
    switch (UNITY_DISPLAY_ORIENTATION_PRETRANSFORM)
    {
        case UNITY_DISPLAY_ORIENTATION_PRETRANSFORM_90:
            return half2((1 - uv.y) / ratio, uv.x * ratio);
        case UNITY_DISPLAY_ORIENTATION_PRETRANSFORM_180:
            return half2(1 - uv.x, 1 - uv.y);
        case UNITY_DISPLAY_ORIENTATION_PRETRANSFORM_270:
            return half2((uv.y + ratio - 1) / ratio, 1 - uv.x * ratio);
    }
    #endif

    return uv;
}

float2 ClipToUv(const float2 clipPos)
{
    half2 screenPos = clipPos / _ScreenParams.xy;
    #if UNITY_UV_STARTS_AT_TOP
    if (0 <= _ProjectionParams.x)
        screenPos.y = 1 - screenPos.y;
    #endif
    return ScreenToUv(screenPos);
}

float SoftMaskSample(float2 uv, float a)
{
    if (_SoftMaskableEnable == 0)
    {
        return 1;
    }

    if (_SoftMaskableStereo)
    {
        uv = lerp(half2(uv.x / 2, uv.y), half2(uv.x / 2 + 0.5, uv.y), unity_StereoEyeIndex);
    }

    half4 mask = tex2D(_SoftMaskTex, uv);
    half4 alpha = saturate(lerp(half4(1, 1, 1, 1),
                                lerp(mask, half4(1, 1, 1, 1) - mask, _SoftMaskColor - half4(1, 1, 1, 1)),
                                _SoftMaskColor));
    #if SOFTMASK_EDITOR
    int inScreen = step(0, uv.x) * step(uv.x, 1) * step(0, uv.y) * step(uv.y, 1);
    alpha = lerp(_SoftMaskOutsideColor, alpha, inScreen);
    #ifdef UNITY_UI_ALPHACLIP
    if (_MaskingShapeSubtract == 1)
    {
        if (_SoftMaskInSceneView == 1)
        {
            clip (a - _AlphaClipThreshold - 0.001);
            return 1;
        }
        clip (a * alpha.x * alpha.y * alpha.z * alpha.w - _AlphaClipThreshold - 0.001);
    }
    #endif
    #endif

    return alpha.x * alpha.y * alpha.z * alpha.w;
}

void SoftMaskForGraph_float(float4 ScreenPos, float4 WorldPos, float InAlpha, out float A)
{
    #if SOFTMASK_EDITOR
    if (_SoftMaskInGameView == 1)
    {
        A = SoftMaskSample(ScreenToUv(ScreenPos.xy), 1) * InAlpha;
    }
    else if (_SoftMaskInSceneView == 1)
    {
        A = InAlpha;
    }
    else
    {
        A = SoftMaskSample(WorldToUv(WorldPos, 0.5), InAlpha) * InAlpha;
    }
    #else
    A = SoftMaskSample(ScreenToUv(ScreenPos.xy), 1) * InAlpha;
    #endif
}

#if SOFTMASK_EDITOR
#define EDITOR_ONLY(x) x
#define SOFTMASK_EDITOR_ONLY(x) x
#define SoftMask(_, worldPos, alpha) SoftMaskSample(WorldToUv(worldPos, 0.5), alpha)
#else
#define EDITOR_ONLY(_)
#define SOFTMASK_EDITOR_ONLY(_)
#define SoftMask(clipPos, _, __) SoftMaskSample(ClipToUv(clipPos), 1)
#endif
#endif // UI_SOFT_MASK_INCLUDED
