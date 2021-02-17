in vec2 FragUV;
in vec4 FragColour;

out vec4 Colour;

const float border = 0.0;

vec4 AlphaBlend(vec4 C1, vec4 C2)
{
	float DeltaAlpha = max(0, C2.a - C1.a);
	return vec4(C1.rgb * (1.0 - DeltaAlpha) + DeltaAlpha * C2.rgb, min(1, C1.a + C2.a));
}

float SelectTile(vec2 FragUV, float XYratio, int Tile) 
{
	return (mod(FragUV.x * XYratio, 1) + Tile) / 3.0;
}

void main(void)
{
	
	//create icon UV
	vec2 IconUV = FragUV;
	IconUV.x = (FragUV.x * XYratio) + 0.5 - Percentage * XYratio;
	
	vec4 I;
	if (IconUV.x < 1.0 && IconUV.x > 0.0 && IconUV.y < 1.0 && IconUV.y > 0.0) I = texture(IconTexture, IconUV) * FragColour;
	else I = vec4(0.0);


	// create scale UV
	vec2 ScalUV = FragUV;
	if (FragUV.x * XYratio < 1.0) ScalUV.x = SelectTile(FragUV, XYratio, 0); // distance from start -> uses tile 0
	else if ((1.0 - FragUV.x) * XYratio < 1.0) ScalUV.x = SelectTile(1.0 - FragUV, -XYratio, 2); // distance from end so also needs to reverse texture hence "-XYratio" -> uses tile 2
	else ScalUV.x = SelectTile(FragUV, XYratio, 1); // distance from start tiled -> uses tile 1

	vec4 S = texture(ScaleTexture, ScalUV) * FragColour;

	Colour = AlphaBlend(I, S);
}


