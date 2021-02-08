out vec4 FragColour; // colour from vertices
out vec2 FragUV; // coordinates in frag space

// requires Vertex2D transform, camera, Time, Texture 
void main(void)
{
	gl_Position = vec4((camera * transform * vec3(VertUV, 1)).xy, 0, 1); // ignore 3d Z axis
	FragColour = VertColour;
	FragUV = TextureUV;
}