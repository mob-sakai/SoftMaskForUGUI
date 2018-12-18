Shader "Hidden/SoftMask" {

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	ColorMask [_ColorMask]

	Pass {  
		CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 2.0
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Softness;

			fixed4 frag (v2f_img i) : SV_Target
			{
				return saturate(tex2D(_MainTex, i.uv).a/_Softness);
			}
		ENDCG
	}
}

}
