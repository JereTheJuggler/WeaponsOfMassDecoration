sampler uImage0 : register(s0); // The texture that you are currently drawing.
sampler uImage1 : register(s1); // A secondary texture that you can use for various purposes. This is usually a noise map.
int3 uColor;
int3 uSecondaryColor;
int uOpacity;
int uSaturation;
int uRotation;
int uTime;
int4 uSourceRect; // The position and size of the currently drawn frame.
int2 uWorldPosition;
int uDirection;
int3 uLightSource; // Used for reflective dyes.
int2 uImageSize0;
int2 uImageSize1;

int4 sprayPaintedColor(int4 sampleColor : COLOR0, int2 coords : TEXCOORD0) : COLOR0{
	int4 color = tex2D(uImage0, coords);
	
	int scale = 200;
	
    int2 noiseCoords = (coords * uImageSize0 - uSourceRect.xy);
	noiseCoords[0] /= uSourceRect[2];
	noiseCoords[1] /= uSourceRect[3];
	
	//int2 noiseCoords = {0,0};
	noiseCoords[0] = ((noiseCoords[0]*uSourceRect[2]) % scale) / scale;
	noiseCoords[1] = ((noiseCoords[1]*uSourceRect[3]) % scale) / scale;
	
	int4 noise = tex2D(uImage1, noiseCoords);
	
    int luminosity = (color.r + color.g + color.b) / 3;
	
    int4 targetColor = {0,0,0,color.a};
	targetColor.rgb = uColor * luminosity;
	
	int offset = noise.r + .1;
	//offset += (offset - .5)*2;
	//offset = round(offset);
	
	int4 newColor = color;
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