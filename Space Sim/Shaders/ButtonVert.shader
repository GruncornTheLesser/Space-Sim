#version 450 core

const int Frames = 60;

layout(location = 0) in vec2 VertUV;
layout(location = 1) in vec2 TextureUV;
layout(location = 2) in vec4 VertColour;

layout(location = 3) uniform mat3 transform;
layout(location = 4) uniform mat3 camera;
layout(location = 5) uniform float Time;
layout(location = 6) uniform float FrameRate;

out vec4 FragColour; // colour from vertices
out vec2 FragUV; // coordinates in frag space

void main(void)
{
	gl_Position = vec4((camera * transform * vec3(VertUV, 1)).xy, 0, 1); // ignore 3d Z axis
	FragColour = VertColour;
	FragUV = TextureUV;
}