sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2;
sampler uImage3;
int3 uColor;
int uOpacity;
int3 uSecondaryColor;
int uTime;
int2 uScreenResolution;
int2 uScreenPosition;
int2 uTargetPosition;
int2 uImageOffset;
int uIntensity;
int uProgress;
int2 uDirection;
int uSaturation;
int4 uSourceRect;
int2 uZoom;
int2 uImageSize1;
int2 uImageSize2;
int2 uImageSize3;

int4 paintedNegativeColor(int4 sampleColor : COLOR0, int2 coords : TEXCOORD0) : COLOR0{
	int4 color = tex2D(uImage0,coords);
	color.r = 1 - color.r;
	color.g = 1 - color.g;
	color.b = 1 - color.b;
	return color * sampleColor * color.a;
}

technique paintedNegativeBuff{
	pass paintedNegativeColor{
		PixelShader = compile ps_2_0 paintedNegativeColor();
	}
}