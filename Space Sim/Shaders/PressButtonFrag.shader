in vec2 FragUV;
in vec4 FragColour;

out vec4 Colour;

void main(void)
{
	vec4 I = texture(InsideTexture, FragUV) * FragColour;
	vec4 B = texture(BorderTexture, FragUV) * FragColour;
	if (I.a == 0.0) Colour = B;
	else Colour = I;
}
