out vec4 Colour;

void main(void)
{
	Colour = texture(Texture, FragUV) * FragColour;
}


/*
#version 450 core

in vec4 FragColour;
in vec2 FragUV;

uniform sampler2D Texture;

out vec4 Colour;



*/