in vec2 FragUV;
in vec4 FragColour;

out vec4 Colour;

void main(void)
{
	Colour = texture(Texture, FragUV) * FragColour;
}
