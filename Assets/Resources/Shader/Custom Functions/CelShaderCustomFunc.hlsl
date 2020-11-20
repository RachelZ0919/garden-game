#ifndef CEL_SHADER_CUSTOM_FUNC_INCLUDED
#define CEL_SHADER_CUSTOM_FUNC_INCLUDED

void MainLight_half(float3 WorldPos, out half3 Direction, out half3 LightColor, out half Attenuation)
{
#if SHADERGRAPH_PREVIEW
	Direction = half3(0.5,0.5,0);
	LightColor = 1;
	Attenuation = 1;
#else
	#if SHADOWS_SCREEN
		half4 clipPos = TransformWorldToClip(WorldPos);
		half4 shadowCoord = ComputeScreenPos(clipPos);
	#else
		half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
	#endif
	Light mainLight = GetMainLight(shadowCoord);
	Direction = mainLight.direction;
	LightColor = mainLight.color;
	Attenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
#endif
}

#endif