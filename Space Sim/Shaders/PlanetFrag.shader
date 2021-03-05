const float pi = 3.141592653589793238462643383;

in vec2 FragUV;
in vec4 FragColour;
out vec4 Colour;

void main(void)
{

	// this is to put 0, 0 in the middle and make the x and y axis match the screen x and y
	vec2 uv = - 2.0 * (FragUV - vec2(0.5));

	float radius = length(uv);

	if (radius > 1.0)
	{
		Colour = vec4(0.0);
	}
	else 
	{
		vec3 normal = vec3(uv.x, uv.y, sqrt(1.0 - uv.x * uv.x - uv.y * uv.y)); // normal to hemisphere
		vec3 rotnorm = normal * rotmat3D; // rotated normal
		// map texture to sphere + rotate Y to rotate and wrap around
		vec2 TexCoords = vec2(0.5 + atan(rotnorm.z, rotnorm.x) / (2.0 * pi), 0.5 - asin(rotnorm.y) / pi);
		// dot product normal and light direction * texture colour
		vec3 Col = texture(Texture, TexCoords).xyz;
		if (DoLighting) Col *= dot(normal, normalize(lightDir));
		Colour = vec4(Col, 1.0);
	}

}
