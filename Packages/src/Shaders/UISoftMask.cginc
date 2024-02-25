#ifndef UI_SOFT_MASK_INCLUDED
#define UI_SOFT_MASK_INCLUDED

sampler2D _SoftMaskTex;
half4 _SoftMaskColor;
float _AlphaClipThreshold;
fixed _SoftMaskInside;
fixed4 _SoftMaskOutsideColor;

void SoftMaskClip(float alpha)
{
    #if UI_SOFT_MASKABLE_EDITOR
    clip (alpha - _AlphaClipThreshold * _SoftMaskInside);
    #else
    clip(alpha);
    #endif
}

float SoftMaskSample(float2 uv)
{
    #if UI_SOFT_MASKABLE_STEREO
    uv = lerp(half2(uv.x/2, uv.y), half2(uv.x/2 +0.5, uv.y), unity_StereoEyeIndex);
    #endif
    half4 mask = tex2D(_SoftMaskTex, uv);
    half4 alpha = saturate(lerp(half4(1, 1, 1, 1),
                                lerp(mask, half4(1, 1, 1, 1) - mask, _SoftMaskColor - half4(1, 1, 1, 1)),
                                _SoftMaskColor));
    #if UI_SOFT_MASKABLE_EDITOR
        _SoftMaskInside = step(0, uv.x) * step(uv.x, 1) * step(0, uv.y) * step(uv.y, 1);
        alpha = lerp(_SoftMaskOutsideColor, alpha, _SoftMaskInside);
    #endif

    return alpha.x * alpha.y * alpha.z * alpha.w;
}

#if UI_SOFT_MASKABLE
// vv UI_SOFT_MASKABLE vv
float2 ClipToUv(float4 clipPos)
{
    half2 uv = clipPos.xy / _ScreenParams.xy;
    #if UNITY_UV_STARTS_AT_TOP
    uv.y = lerp(uv.y, 1 - uv.y, step(0, _ProjectionParams.x));
    #endif

    return uv;
}

#define UI_SOFT_MASKABLE_EDITOR_ONLY(_)
#define SoftMask(clipPos, _) SoftMaskSample(ClipToUv(clipPos))
// ^^ UI_SOFT_MASKABLE ^^
#elif UI_SOFT_MASKABLE_EDITOR
// vv UI_SOFT_MASKABLE_EDITOR vv
float4x4 _GameVP;
float4x4 _GameTVP;
float4x4 _GameVP_2;
float4x4 _GameTVP_2;
fixed Approximately(float4x4 a, float4x4 b)
{
    float4x4 d = abs(a - b);
    return step(
        max(d._m00,max(d._m01,max(d._m02,max(d._m03,
        max(d._m10,max(d._m11,max(d._m12,max(d._m13,
        max(d._m20,max(d._m21,max(d._m22,max(d._m23,
        max(d._m30,max(d._m31,max(d._m32,d._m33))))))))))))))),
        0.1);
}

float2 WorldToUv(float4 worldPos)
{
    float4x4 gameVp = lerp(_GameVP, _GameVP_2, unity_StereoEyeIndex);
    float4x4 gameTvp = lerp(_GameTVP, _GameTVP_2, unity_StereoEyeIndex);
    
    fixed isSceneView = 1 - Approximately(UNITY_MATRIX_VP, gameVp);
    
    float4 clipPos = mul(UNITY_MATRIX_VP, worldPos);
    float4 clipPosG = mul(gameTvp, worldPos);
    return lerp(
        clipPos.xy / clipPos.w * 0.5 + 0.5,
        clipPosG.xy / clipPosG.w * 0.5 + 0.5,
        isSceneView);
}

#define UI_SOFT_MASKABLE_EDITOR_ONLY(x) x
#define SoftMask(_, worldPos) SoftMaskSample(WorldToUv(worldPos))
// ^^ UI_SOFT_MASKABLE_EDITOR
#else
#define UI_SOFT_MASKABLE_EDITOR_ONLY(_)
#define SoftMask(_, __) 1

#endif

#ifndef UIGammaToLinear
half3 UIGammaToLinear(half3 value)
{
    half3 low = 0.0849710 * value - 0.000163029;
    half3 high = value * (value * (value * 0.265885 + 0.736584) - 0.00980184) + 0.00319697;

    // We should be 0.5 away from any actual gamma value stored in an 8 bit channel
    const half3 split = 0.0725490; // Equals 18.5 / 255
    return (value < split) ? low : high;
}
#endif

#endif // UI_SOFT_MASK_INCLUDED
