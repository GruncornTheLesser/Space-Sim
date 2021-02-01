
in vec2 FragUV;
in vec4 FragColour;

out vec4 Colour;


const bool HorizontalBlur = true;
const float threshold = 0.3;
const float weight[5] = float[](1.5, 0.8, 0.4, 0.1, 0.05);
void main(void)
{
	vec2 PixelSize = 1.0 / textureSize(Texture, 0); // size of pixel in texture
	vec4 PixelCol = texture(Texture, FragUV) * FragColour;

	if (HorizontalBlur) {
		for(int i = 1; i < 5; ++i)
		{
			vec4 Left = texture(Texture, FragUV - vec2(PixelSize.x * i, 0)) * FragColour * weight[i];
			if ((Left.r + Left.g + Left.b) / 3.0 > threshold) PixelCol += Left;

			vec4 Right = texture(Texture, FragUV + vec2(PixelSize.x * i, 0)) * FragColour * weight[i];
			if ((Right.r + Right.g + Right.b) / 3.0 > threshold) PixelCol += Right;

		}
	}
	else
	{
		for(int i = 1; i < 5; ++i)
		{
			vec4 Up = texture(Texture, FragUV - vec2(0, PixelSize.y * i)) * FragColour * weight[i];
			if ((Up.r + Up.g + Up.b) / 3.0 > threshold) PixelCol += Up;

			vec4 Down = texture(Texture, FragUV + vec2(0, PixelSize.y * i)) * FragColour * weight[i];
			if ((Down.r + Down.g + Down.b) / 3.0 > threshold) PixelCol += Down;

		}
	}
	Colour = PixelCol;
}
