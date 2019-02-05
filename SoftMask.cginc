#ifndef UI_SOFTMASK_INCLUDED
#define UI_SOFTMASK_INCLUDED

sampler2D _SoftMaskTex;
float _Stencil;
float4x4 _SceneV;
float4x4 _SceneP;
float4x4 _GameVP;
half4 _MaskInteraction;

fixed Approximately(float4x4 a, float4x4 b)
{
	float4x4 d = abs(a - b);
	return step(
		max(d._m00,max(d._m01,max(d._m02,max(d._m03,
		max(d._m10,max(d._m11,max(d._m12,max(d._m13,
		max(d._m20,max(d._m21,max(d._m22,max(d._m23,
		max(d._m30,max(d._m31,max(d._m32,d._m33))))))))))))))),
		1);
}

float GetMaskAlpha(float alpha, int stencilId, float interaction)
{
	fixed onStencil = step(stencilId, _Stencil);
	alpha = lerp(1, alpha, onStencil * step(1, interaction));
	return lerp(alpha, 1 - alpha, onStencil * step(2, interaction));
}

#if SOFTMASK_EDITOR
float SoftMaskInternal(float4 clipPos, float4 wpos)
#else
float SoftMaskInternal(float4 clipPos)
#endif
{
	half2 view = clipPos.xy/_ScreenParams.xy;
	#if SOFTMASK_EDITOR
		fixed isSceneView = max(Approximately(UNITY_MATRIX_V, _SceneV), Approximately(UNITY_MATRIX_P, _SceneP));
		float4 cpos = mul(_GameVP, mul(UNITY_MATRIX_M, wpos));
		view = lerp(view, cpos.xy / cpos.w * 0.5 + 0.5, isSceneView);
	#endif

	#if UNITY_UV_STARTS_AT_TOP
		view.y = 1.0 - view.y;
	#endif

	fixed4 mask = tex2D(_SoftMaskTex, view);
	half alpha = GetMaskAlpha(mask.x, 1, _MaskInteraction.x)
		* GetMaskAlpha(mask.y, 3, _MaskInteraction.y)
		* GetMaskAlpha(mask.z, 7, _MaskInteraction.z)
		* GetMaskAlpha(mask.w, 15, _MaskInteraction.w)
	#if SOFTMASK_EDITOR
		* step(0, view.x) * step(view.x, 1) * step(0, view.y) * step(view.y, 1)
	#endif
		;

	return alpha;
}

#if SOFTMASK_EDITOR
	#define SOFTMASK_EDITOR_ONLY(x) x
	#define SoftMask(clipPos, worldPosition) SoftMaskInternal(clipPos, worldPosition)
#else
	#define SOFTMASK_EDITOR_ONLY(x) 
	#define SoftMask(clipPos, worldPosition) SoftMaskInternal(clipPos)
#endif

#endif // UI_SOFTMASK_INCLUDED