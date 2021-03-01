const float pi = 3.141592653589793238462643383;

in vec2 FragUV;
in vec4 FragColour;
out vec4 Colour;
vec4 AlphaBlend(vec4 C1, vec4 C2)
{
	float DeltaAlpha = max(0, C2.a - C1.a);
	return vec4(C1.rgb * (1.0 - DeltaAlpha) + DeltaAlpha * C2.rgb, min(1, C1.a + C2.a));
}
vec3 Lerp(vec3 v1, vec3 v2, float delta) 
{
	return (1 - delta) * v2 + v1 * delta;
}
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
		vec3 rotnorm = normal * rotmat3D;
		// map texture to sphere + rotate Y to rotate and wrap around
		vec2 TexCoords = vec2(0.5 + atan(rotnorm.z, rotnorm.x) / (2.0 * pi), 0.5 - asin(rotnorm.y) / pi);
		
		vec3 DayCol = texture(Day, TexCoords).xyz;
		vec3 NightCol = texture(Night, TexCoords).xyz;
		
		// dot product normal and light direction * texture colour
		float brightness = max(0.05, dot(normal, normalize(lightDir)));
		Colour = vec4(Lerp(DayCol, NightCol * dot(normal, vec3(0, 0, 1)), brightness), 1);
	}
}

