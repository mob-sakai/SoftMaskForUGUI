#include "Packages/com.esotericsoftware.spine.spine-unity/Runtime/spine-unity/Shaders/SkeletonGraphic/CGIncludes/Spine-SkeletonGraphic-NormalPass.cginc"
#include "Packages/com.coffee.softmask-for-ugui/Shaders/SoftMask.cginc" // Add for soft mask

#ifndef UI_SOFT_MASK_SPINE_INCLUDED
#define UI_SOFT_MASK_SPINE_INCLUDED

fixed4 frag_softmask (VertexOutput IN) : SV_Target
{
    half4 texColor = tex2D(_MainTex, IN.texcoord);

    #if defined(_STRAIGHT_ALPHA_INPUT)
    texColor.rgb *= texColor.a;
    #endif

    half4 color = (texColor + _TextureSampleAdd) * IN.color;
    color *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

    color *= SoftMask(IN.vertex, IN.worldPosition, color.a); // Add for soft mask

    #ifdef UNITY_UI_ALPHACLIP
    clip (color.a - 0.001);
    #endif

    #ifdef ENABLE_FILL
    color.rgb = lerp(color.rgb, (_FillColor.rgb * color.a), _FillPhase); // make sure to PMA _FillColor.
    #endif
    #ifdef ENABLE_GRAYSCALE
    color.rgb = lerp(color.rgb, dot(color.rgb, float3(0.3, 0.59, 0.11)), _GrayPhase);
    #endif
    return color;
}

#endif // UI_SOFT_MASK_SPINE_INCLUDED
