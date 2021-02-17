using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
namespace Graphics
{
    static class TextureManager
    {
        public static int TexturesLoaded = 0;


        private static Dictionary<string, int> _TextureDict = new Dictionary<string, int>();
        public static int Get(string path)
        {
            if (_TextureDict.ContainsKey(path)) return _TextureDict[path];
            else 
            {
                int NewTexture = Init_Textures(path);
                _TextureDict[path] = NewTexture;
                return NewTexture;
            }
        }
        /// <summary>
        /// opens image file and creates texture.
        /// </summary>
        /// <param name="name">The name of the texture file in: @Graphics/Textures/[name].png</param>
        /// <returns>The texture handle ID for OpenGL.</returns>
        private static int Init_Textures(string path)
        {
            int width, height, Handle;
            float[] data = Load_Texture(out width, out height, path);
            GL.CreateTextures(TextureTarget.Texture2D, 1, out Handle);
            // level of mipmap, format, width, height
            GL.TextureStorage2D(Handle, 1, SizedInternalFormat.Rgba32f, width, height);

            // bind texture to slot
            GL.BindTexture(TextureTarget.Texture2D, Handle);
            // level of detail maybe???, offset x, offset y, width, height, format, type, serialized data
            GL.TextureSubImage2D(Handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.Float, data);

            // fixes texture at edges
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // makes pixel perfect
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            return Handle;
        }

        /// <summary>
        /// Serializes image read from file for openGl buffer.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="path">The path to the image.</param>
        /// <returns>The serialised data read from the file in rgba format.</returns>
        private static float[] Load_Texture(out int width, out int height, string path)
        {
            Bitmap BMP = (Bitmap)System.Drawing.Image.FromFile(path);
            width = BMP.Width;
            height = BMP.Height;
            float[] Serialized_Data = new float[width * height * 4];
            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color P = BMP.GetPixel(x, y);
                    Serialized_Data[index++] = P.R / 255f;
                    Serialized_Data[index++] = P.G / 255f;
                    Serialized_Data[index++] = P.B / 255f;
                    Serialized_Data[index++] = P.A / 255f;
                }
            }

            return Serialized_Data;
        }

    }
}
