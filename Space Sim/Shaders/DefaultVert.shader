const int Frames = 60;
const float Time = 0;
const float FrameRate = 60.0;
out vec4 FragColour; // colour from vertices
out vec2 FragUV; // coordinates in frag space

void main(void)
{
	gl_Position = vec4((camera * transform * vec3(VertUV, 1)).xy, 0, 1); // ignore 3d Z axis
	FragColour = VertColour;

	// ignore 3d Z axis could do this with 3d axis and remove the need for huge textures
	FragUV = vec2((TextureUV.x / Frames) + (1.0 / Frames) * (int(round(Time * FrameRate)) % Frames), TextureUV.y);
}

//#version 450 core



//layout(location = 0) in vec2 VertUV;
//layout(location = 1) in vec2 TextureUV;
//layout(location = 2) in vec4 VertColour;

//layout(location = 3) uniform mat3 transform;
//layout(location = 4) uniform mat3 Camera;
//layout(location = 5) uniform float Time;
//layout(location = 6) uniform float FrameRate;

