in vec2 FragUV;
in vec4 FragColour;

out vec4 Colour;

void main(void)
{
	vec4 I = texture(InsideTexture, FragUV) * FragColour;
	vec4 B = texture(BorderTexture, FragUV) * FragColour;
	if (FragUV.x < 0.5) Colour = B;
	else Colour = I * vec4(1.0, 0.0, 0.0, 1.0);//vec4(1.0);
}
