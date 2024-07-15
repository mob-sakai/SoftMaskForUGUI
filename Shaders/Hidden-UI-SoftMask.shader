Shader "Hidden/UI/SoftMask"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        [Enum (UnityEngine.Rendering.BlendOp)] _BlendOp ("BlendOp", float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

        Cull Off
        ZWrite Off
        Blend SrcColor One
        BlendOp [_BlendOp]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _ThresholdMin;
            float _ThresholdMax;
            float4 _ColorMask;

            float invLerp(const float from, const float to, const float value)
            {
                return saturate(max(0, value - from) / max(0.000000001, to - from));
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                const half maxValue = max(_ThresholdMin, _ThresholdMax);
                const half minValue = min(_ThresholdMin, _ThresholdMax);
                const half alpha = invLerp(minValue, maxValue, tex2D(_MainTex, i.uv).a);
                return alpha * _ColorMask;
            }
            ENDCG
        }
    }
}