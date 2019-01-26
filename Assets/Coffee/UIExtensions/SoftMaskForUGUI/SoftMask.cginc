#ifndef UI_SOFTMASK_INCLUDED
#define UI_SOFTMASK_INCLUDED

sampler2D _SoftMaskTex;
float _Stencil;
float4x4 _SceneView;
float4x4 _SceneProj;
half4 _MaskInteraction;

fixed Approximately(float4x4 a, float4x4 b)
{
	float4x4 d = abs(a - b);
	return step(
		max(d._m00,max(d._m01,max(d._m02,max(d._m03,
		max(d._m10,max(d._m11,max(d._m12,max(d._m13,
		max(d._m20,max(d._m21,max(d._m22,max(d._m23,
		max(d._m30,max(d._m31,max(d._m32,d._m33))))))))))))))),
		0.01);
}

fixed GetMaskAlpha(fixed alpha, fixed stencilId, fixed interaction)
{
	fixed onStencil = step(stencilId, _Stencil);
	alpha = lerp(1, alpha, onStencil * step(1, interaction));
	return lerp(alpha, 1 - alpha, onStencil * step(2, interaction));
}

half SoftMask(float4 clipPos)
{
	half2 view = clipPos.xy/_ScreenParams.xy;
	#if UNITY_UV_STARTS_AT_TOP
		view.y = 1.0 - view.y;
	#endif

	fixed4 mask = tex2D(_SoftMaskTex, view);
	half alpha = GetMaskAlpha(mask.x, 1, _MaskInteraction.x)
		* GetMaskAlpha(mask.y, 3, _MaskInteraction.y)
		* GetMaskAlpha(mask.z, 7, _MaskInteraction.z)
		* GetMaskAlpha(mask.w, 15, _MaskInteraction.w);

	#if SOFTMASK_EDITOR
	fixed isSceneView = max(Approximately(UNITY_MATRIX_V, _SceneView), Approximately(UNITY_MATRIX_P, _SceneProj));
	alpha = lerp(alpha, 1, isSceneView);
	#endif

	return alpha;
}

#endif // UI_SOFTMASK_INCLUDED