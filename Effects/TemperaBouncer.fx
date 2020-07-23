sampler uImage0 : register(s0); // The texture that you are currently drawing.
sampler uImage1 : register(s1); // A secondary texture that you can use for various purposes. This is usually a noise map.
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect; // The position and size of the currently drawn frame.
float2 uWorldPosition;
float uDirection;
float3 uLightSource; // Used for reflective dyes.
float2 uImageSize0;
float2 uImageSize1;

float4 temperaBouncer(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	float4 color = tex2D(uImage0,coords);
	float lum = (color.r + color.g + color.b)/3;
	float4 targetColor = {0,0,0,.5};
	targetColor.rgb = uColor.rgb * lum;
	return sampleColor * targetColor * color.a;
}

technique temperaBouncerTechique{
	pass temperaBouncer{
		PixelShader = compile ps_2_0 temperaBouncer();
	}
}