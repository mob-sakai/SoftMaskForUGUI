#ifndef UI_SOFTMASK_INCLUDED
#define UI_SOFTMASK_INCLUDED

sampler2D _SoftMaskTex;
fixed _SoftMaskInverse;
float _Stencil;
float4x4 _SceneView;
float4x4 _SceneProj;


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

half SoftMask(float4 clipPos)
{
	half2 view = clipPos.xy/_ScreenParams.xy;
	#if UNITY_UV_STARTS_AT_TOP
        view.y = 1.0 - view.y;
    #endif
	
	half alpha =
		lerp(1, tex2D(_SoftMaskTex, view).a, step(15, _Stencil))
		* lerp(1, tex2D(_SoftMaskTex, view).b, step(7, _Stencil))
		* lerp(1, tex2D(_SoftMaskTex, view).g, step(3, _Stencil))
		* lerp(1, tex2D(_SoftMaskTex, view).r, step(1, _Stencil));

	alpha = lerp(alpha, 1 - alpha, _SoftMaskInverse);

	#if SOFTMASK_EDITOR
	fixed isSceneView = max(Approximately(UNITY_MATRIX_V, _SceneView), Approximately(UNITY_MATRIX_P, _SceneProj));
	alpha = lerp(alpha, 1, isSceneView);
	#endif

	return alpha;
}


#endif // UI_SOFTMASK_INCLUDED
