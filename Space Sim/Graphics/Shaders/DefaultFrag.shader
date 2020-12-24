#version 450 core

in vec4 VertColour;
in vec2 UV;
out vec4 FragColour;

void main(void)
{
	FragColour = VertColour;
}