sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2;
sampler uImage3;
float3 uColor;
float uSaturation;
float3 uSecondaryColor;
float uTime;
float uOpacity;
float4 uShaderSpecificData;
float4 uSourceRect;
float2 uWorldPosition;
float2 uImageSize0;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uImageOffset;
float uIntensity;
float uProgress;
float2 uDirection;
float2 uZoom;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;

float4 paintedColor(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	float4 color = tex2D(uImage0,coords);
	float lum = (color.r + color.g + color.b)/3;
	color.rgb = lum * uColor;
	color.a = color.a * uOpacity;
	return color * sampleColor;
}

technique paintedBuff{
	pass paintedColor{
		PixelShader = compile ps_2_0 paintedColor();
	}
}