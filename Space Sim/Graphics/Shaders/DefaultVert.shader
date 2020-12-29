#version 450 core

layout(location = 0) in vec2 VertUV;
layout(location = 1) in vec2 TextureUV;
layout(location = 2) in vec4 VertColour;

layout(location = 3) uniform mat3 transform;
layout(location = 4) uniform mat3 camera;

out vec4 FragColour;
out vec2 FragUV;

void main(void)
{
	gl_Position = vec4((camera * transform * vec3(VertUV, 1)).xy, 0, 1); // ignore 3d Z axis
	FragColour = VertColour;
	FragUV = TextureUV;
}