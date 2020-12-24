#version 450 core

layout(location = 0) in vec2 VertPosition;
layout(location = 1) in vec4 Colour;
layout(location = 2) uniform mat3 transform;

out vec4 VertColour;
out vec2 UV;
void main(void)
{
	// Logic goes here


	gl_Position = vec4((transform * vec3(VertPosition, 1)).xy, 0, 1); // ignore 3d Z axis
	VertColour = Colour;
	UV = VertPosition;
}