out vec4 FragColour; // colour from vertices
out vec2 FragUV; // coordinates in frag space

void main(void)
{
	gl_Position = vec4((camera * vec3(VertUV, 1)).xy, 0, 1); // ignore 3d Z axis
	FragColour = VertColour;
	FragUV = vec2(0.0);
}