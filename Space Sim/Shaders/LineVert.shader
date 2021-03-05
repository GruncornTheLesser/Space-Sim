out vec4 FragColour; // colour from vertices

void main(void)
{
	gl_Position = vec4((camera * vec3(VertUV, 1)).xy, 0, 1); // ignore 3d Z axis
	FragColour = VertColour;
}