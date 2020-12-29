using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
/*
* OpenGL Hardware Pipeline
* ====================CPU====================
* yo OpenGL, go draw this cool stuff.
*                      V
* ====================GPU====================
* Vertex Shader - code to animate and position each vertex
*                      V
[X]Tesselation Control (optional) - specify how many vertices to generate
*                      V
[X]Tesselation (optional) - generate vertices
*                      V
[X]Tesselation Evaluation Shader (optional) position new vertices
*                      V
[X]Geometry Shader (optional) - generate or delete geometry
*                      V
* Clipping - remove stuff thats out of frame
*                      V
* Rasterization - primitive triangles are turned into 2d bitmap of pixels
*      V
* Fragment Shader - code to colour each pixel
*                      V
* Blending - overlapping pixels are blended into 1
*                      V
* Frame Buffer - loads 2d image into frame buffer
* 
* 
* 
* 
* A Handle is a pointer for openGL. I think.
*/
namespace Graphics
{
    class RenderObject<Vertex> : Node2D where Vertex : unmanaged
    {
        // an ints to identify Texture in OpenGL(pointer???)
        private readonly int VertexArrayHandle;
        private readonly int VertexBufferHandle;
        private readonly int TextureHandle; 
        private readonly int ProgramHandle;

        private readonly int VertexCount; // number of vertices
        private readonly int VertexSize; // size of vertex in bytes
        private int VertexLength; // number of data points in vertex

        //public int Z_index; decides which goes in front

        // still assumes a 2D render object in constructor
        public RenderObject(float Rotation, Vector2 Scale, Vector2 Position, Vertex[] Vertices) : base(Rotation, Scale, Position)
        {
            this.VertexCount = Vertices.Length;

            Init_BufferArray(out VertexArrayHandle, out VertexBufferHandle, out VertexSize, Vertices);

            /* THING TO DO:
             * Parameters needs to passing from constructor
             * Planets and default will change
             */
            TextureHandle = Init_Textures("Planet");
            ProgramHandle = Init_Program("Default", "Default");

            // fixes texture at edges
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // makes pixel perfect
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        }
        public RenderObject(float Rotation, float ScaleX, float ScaleY, float PositionX, float PositionY, Vertex[] Vertices) : base(Rotation, new Vector2(ScaleX, ScaleY), new Vector2(PositionX, PositionY))
        {
            this.VertexCount = Vertices.Length;

            Init_BufferArray(out VertexArrayHandle, out VertexBufferHandle, out VertexSize, Vertices);

            TextureHandle = Init_Textures("Planet");
            ProgramHandle = Init_Program("Default", "Default");
            
            // fixes texture at edges
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // makes pixel perfect
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        }

        private void LoadBufferAttribute(int ArrayHandle, int Size, int Index, ref int Offset)
        {
            GL.VertexArrayAttribBinding(ArrayHandle, Index, 0);
            GL.EnableVertexArrayAttrib(ArrayHandle, Index);
            // Handle, index when delivered to shader, number of elements, contains floats, already normalized so false, relative offset from 0 in bytes = 0 
            GL.VertexArrayAttribFormat(ArrayHandle, Index, Size, VertexAttribType.Float, false, Offset);
            Offset += Size * 4; // number of elements * float size 
        }
        private void Init_BufferArray(out int ArrayHandle, out int BufferHandle, out int VertexSize, Vertex[] Vertices)
        {
            /* struct needs to be 'unmanaged' to use sizeof()
             * dont really know what that means but whatever
             * its also unsafe meaning it could potential have different results on different systems
             * it should be fine as long as the vertex doesnt contain strings or characters. 
             * float and ints are relatively safe.
             */
            unsafe { VertexSize = sizeof(Vertex); }
            
            ArrayHandle = GL.GenVertexArray();
            BufferHandle = GL.GenBuffer();
            
            GL.BindVertexArray(ArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, BufferHandle);
            // create new buffer storage of vertice data
            GL.NamedBufferStorage(BufferHandle, VertexSize * Vertices.Length, Vertices, BufferStorageFlags.MapWriteBit);

            /* THING TO DO:
             * needs to adapt to custom Vertex
             * 
             * iterate through struct members
             * this is gross
             */
            FieldInfo[] FieldInfoArray = typeof(Vertex).GetFields();
            int RelativeOffset = 0;
            VertexLength = FieldInfoArray.Length;
            for (int i = 0; i < VertexLength; i++)
            {
                Type T = FieldInfoArray[i].FieldType;

                if (T == typeof(Vector2)) LoadBufferAttribute(ArrayHandle, 2, i, ref RelativeOffset);
                else if (T == typeof(Vector3)) LoadBufferAttribute(ArrayHandle, 3, i, ref RelativeOffset);
                else if (T == typeof(Vector4)) LoadBufferAttribute(ArrayHandle, 4, i, ref RelativeOffset);
                else if (T == typeof(Color4)) LoadBufferAttribute(ArrayHandle, 4, i, ref RelativeOffset);
                else if (T == typeof(float)) LoadBufferAttribute(ArrayHandle, 1, i, ref RelativeOffset);
                else if (T == typeof(Int32)) LoadBufferAttribute(ArrayHandle, 1, i, ref RelativeOffset);
                else throw new Exception("RenderObject cannot load type " + T.ToString() + " into buffer reliably"); // shouldnt be needed as its already known that its unmanaged

            }
            // link the vertex array and buffer and provide the step size(stride) as size of Vertex
            GL.VertexArrayVertexBuffer(ArrayHandle, 0, BufferHandle, IntPtr.Zero, VertexSize);
        }

        private int Init_Program(string VertName, string FragName)
        {
            // creates new program
            int Handle = GL.CreateProgram();

            // compile new shaders
            int Vert = Load_Shader(ShaderType.VertexShader, @"Graphics\Shaders\" + VertName + "Vert.shader");
            int Frag = Load_Shader(ShaderType.FragmentShader, @"Graphics\Shaders\" + FragName + "Frag.shader");

            // attach new shaders
            GL.AttachShader(Handle, Vert);
            GL.AttachShader(Handle, Frag);

            // link new shaders
            GL.LinkProgram(Handle);

            // check for error linking shaders to program
            string info = GL.GetProgramInfoLog(Handle);
            if (!string.IsNullOrWhiteSpace(info)) throw new Exception($"Failed to link shaders to program: {info}");

            // detach and delete both shaders
            GL.DetachShader(Handle, Vert);
            GL.DetachShader(Handle, Frag);
            GL.DeleteShader(Vert);
            GL.DeleteShader(Frag);

            return Handle;
        }
        private int Load_Shader(ShaderType type, string path)
        {
            // create new shader object in OpenGL
            int NewShaderHandle = GL.CreateShader(type);

            // get code from file
            string code = File.ReadAllText(path);

            // attaches shader and code
            GL.ShaderSource(NewShaderHandle, code);

            // compiles shader code
            GL.CompileShader(NewShaderHandle);

            // checks if compilation worked
            string info = GL.GetShaderInfoLog(NewShaderHandle);
            if (!string.IsNullOrWhiteSpace(info)) throw new Exception($"Failed to compile {type} shader: {info}");

            return NewShaderHandle;
        }

        private int Init_Textures(string name)
        {
            /* THING TO DO
             * 
             * Needs to load multiple textures
             */


            int width, height, Handle;
            float[] data = Load_Texture(out width, out height, "Graphics/Textures/" + name + ".png");
            GL.CreateTextures(TextureTarget.Texture2D, 1, out Handle);
            // level of mipmap, format, width, height
            GL.TextureStorage2D(Handle, 1, SizedInternalFormat.Rgba32f, width, height);

            // bind texture to slot
            GL.BindTexture(TextureTarget.Texture2D, Handle);
            // level ???, offset x, offset y, width, height, format, type, serialized data
            GL.TextureSubImage2D(Handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.Float, data);

            return Handle;
        }
        private float[] Load_Texture(out int width, out int height, string path)
        {
            Bitmap BMP = (Bitmap)Image.FromFile(path);
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

        public void Render(Node2D Camera, float Time)
        {
            GL.UseProgram(ProgramHandle);

            // pass in renderobject transform matrix

            /* Problem Problem Problem
             * needs to change Node depending on whether its 3d or 2d
             * Will keep as matrix 4 and ignore the 3d values when 2d
             * Also Perspective Matrix???
             */
            GL.UniformMatrix3(VertexLength, true, ref Transform_Matrix);
            GL.UniformMatrix3(VertexLength + 1, true, ref Camera.Transform_Matrix);
            GL.Uniform1(VertexLength + 2, Time);
            /* https://opentk.net/learn/chapter1/6-multiple-textures.html
             * Sampler2D variable is uniform but isn't assigned the same way
             * this is assigned a location value with a texture sampler 
             * known as a texture unit.
             * 
             * allows use of more than 1 texture to be passed.
             * 
             * This is then bound with BindTextures() to the current active 
             * texture unit.
             * 
             * idk its weird openGL stuff. 
             */

            GL.BindTextures(0, 1, new int[1] { TextureHandle });
            GL.BindVertexArray(VertexArrayHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, VertexCount);
        }
        public virtual void Process(float delta) { }
    }
}
