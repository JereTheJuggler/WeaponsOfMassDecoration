sampler uImage0 : register(s0); // The texture that you are currently drawing.
sampler uImage1 : register(s1); // A secondary texture that you can use for various purposes. This is usually a noise map.
float4 uShaderSpecificData;
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

float4 sprayPaintedColor(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0{
	float4 color = tex2D(uImage0, coords);
	
	if(color.a == 0)
		return color;

	float scale = 200;

    float2 noiseCoords = (coords * uImageSize0 - uSourceRect.xy);
	noiseCoords[0] /= uSourceRect[2];
	noiseCoords[1] /= uSourceRect[3];
	
	//float2 noiseCoords = {0,0};
	noiseCoords[0] = ((noiseCoords[0]*uSourceRect[2]) % scale) / scale;
	noiseCoords[1] = ((noiseCoords[1]*uSourceRect[3]) % scale) / scale;
	
	float4 noise = tex2D(uImage1, noiseCoords);
	
    float luminosity = (color.r + color.g + color.b) / 3;
	
    float4 targetColor = {0,0,0,color.a};
	targetColor.rgb = uColor * luminosity;
	
	float offset = noise.r + .1;
	//offset += (offset - .5)*2;
	//offset = round(offset);
	
	float4 newColor = color;
	if(offset > .4)
		newColor = targetColor;
	//lerp(color,targetColor,offset);
	
    return newColor * sampleColor * color.a;
}

technique sprayPaintedBuff{
	pass sprayPaintedColor{
		PixelShader = compile ps_2_0 sprayPaintedColor();
	}
}