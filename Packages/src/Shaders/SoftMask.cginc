#ifndef UI_SOFT_MASK_INCLUDED
#define UI_SOFT_MASK_INCLUDED

uniform sampler2D _SoftMaskTex;
uniform half4 _SoftMaskColor;
uniform float _AlphaClipThreshold;
uniform float4 _SoftMaskOutsideColor;
uniform int _SoftMaskableStereo;
uniform float4x4 _GameVP;
uniform float4x4 _GameVP_2;
uniform int _MaskingShapeSubtract;
uniform float _RenderScale;
uniform float2 _DynamicResolutionScale;
uniform int _AllowRenderScale;
uniform int _AllowDynamicResolution;
uniform float _SoftMaskingPower;

float2 WorldToUv(float4 worldPos)
{
    worldPos = mul(unity_ObjectToWorld, worldPos);
    float4x4 gameVp = unity_StereoEyeIndex ? _GameVP_2 : _GameVP;
    float4 clipPos = mul(gameVp, worldPos);
    return clipPos.xy / clipPos.w * 0.5 + 0.5;
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
    if (_SoftMaskableStereo)
    {
        uv = lerp(half2(uv.x / 2, uv.y), half2(uv.x / 2 + 0.5, uv.y), unity_StereoEyeIndex);
    }

    #if !SOFTMASK_EDITOR
    if (_AllowRenderScale == 1)
    {
        uv /= _RenderScale;
    }
    if (_AllowDynamicResolution == 1)
    {
        uv /= _DynamicResolutionScale;
    }
    #endif
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
        clip (a * alpha.x * alpha.y * alpha.z * alpha.w - _AlphaClipThreshold - 0.001);
    }
    #endif
    #endif

    return pow(alpha.x * alpha.y * alpha.z * alpha.w, _SoftMaskingPower);
}

void SoftMaskForGraph_float(float4 ScreenPos, float4 WorldPos, float InAlpha, out float A)
{
    #if !SOFTMASKABLE
    A = InAlpha;
    #elif SOFTMASK_EDITOR
    A = SoftMaskSample(WorldToUv(WorldPos), InAlpha) * InAlpha;
    #else
    A = SoftMaskSample(ScreenToUv(ScreenPos.xy), 1) * InAlpha;
    #endif
}

#if !SOFTMASKABLE
#define EDITOR_ONLY(_)
#define SOFTMASK_EDITOR_ONLY(_)
#define SoftMask(_, __, ___) 1
#elif SOFTMASK_EDITOR
#define EDITOR_ONLY(x) x
#define SOFTMASK_EDITOR_ONLY(x) x
#define SoftMask(_, worldPos, alpha) SoftMaskSample(WorldToUv(worldPos), alpha)
#else
#define EDITOR_ONLY(_)
#define SOFTMASK_EDITOR_ONLY(_)
#define SoftMask(clipPos, _, __) SoftMaskSample(ClipToUv(clipPos), 1)
#endif
#else
#endif // UI_SOFT_MASK_INCLUDED
