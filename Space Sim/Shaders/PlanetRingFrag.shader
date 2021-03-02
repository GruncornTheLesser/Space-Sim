in vec2 FragUV;
in vec4 FragColour;

out vec4 Colour;

void main(void)
{
	vec2 UV = FragUV;

	Colour = texture(Texture, UV) * FragColour;
}
