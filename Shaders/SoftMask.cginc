#ifndef UI_SOFTMASK_INCLUDED
#define UI_SOFTMASK_INCLUDED

sampler2D _SoftMaskTex;
float _Stencil;
float4x4 _GameVP;
float4x4 _GameTVP;
half4 _MaskInteraction;

#if SOFTMASK_EDITOR
float SoftMaskInternal(float4 clipPos, float4 wpos)
#else
float SoftMaskInternal(float4 clipPos)
#endif
{
	half2 view = clipPos.xy/_ScreenParams.xy;
	#if SOFTMASK_EDITOR
		fixed isSceneView = any(UNITY_MATRIX_VP - _GameVP);
		float4 cpos = mul(_GameTVP, mul(UNITY_MATRIX_M, wpos));
		view = lerp(view, cpos.xy / cpos.w * 0.5 + 0.5, isSceneView);
		#if UNITY_UV_STARTS_AT_TOP
			view.y = lerp(view.y, 1 - view.y, step(0, _ProjectionParams.x));
		#endif
	#elif UNITY_UV_STARTS_AT_TOP
		view.y = lerp(view.y, 1 - view.y, step(0, _ProjectionParams.x));
	#endif

	fixed4 mask = tex2D(_SoftMaskTex, view);
	half4 alpha = saturate(lerp(fixed4(1, 1, 1, 1), lerp(mask, 1 - mask, _MaskInteraction - 1), _MaskInteraction))
	#if SOFTMASK_EDITOR
		* step(0, view.x) * step(view.x, 1) * step(0, view.y) * step(view.y, 1)
	#endif
		;

	return alpha.x * alpha.y * alpha.z * alpha.w;
}

#if SOFTMASK_EDITOR
	#define SOFTMASK_EDITOR_ONLY(x) x
	#define SoftMask(clipPos, worldPosition) SoftMaskInternal(clipPos, worldPosition)
#else
	#define SOFTMASK_EDITOR_ONLY(x)
	#define SoftMask(clipPos, worldPosition) SoftMaskInternal(clipPos)
#endif

#endif // UI_SOFTMASK_INCLUDED
