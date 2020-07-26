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

int4 paintedColor(int4 sampleColor : COLOR0, int2 coords : TEXCOORD0) : COLOR0{
	int4 color = tex2D(uImage0,coords);
	int lum = (color.r + color.g + color.b)/3;
	color.rgb = lum * uColor;
	return color * sampleColor * uOpacity;
}

technique paintedBuff{
	pass paintedColor{
		PixelShader = compile ps_2_0 paintedColor();
	}
}