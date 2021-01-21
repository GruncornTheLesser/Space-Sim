using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using OpenTK.Windowing.Common;
using Shaders;
/* OpenGL Hardware Pipeline
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
 [~] Blending - overlapping pixels are blended into 1
 *                      V
 [~] Frame Buffer - loads 2d image into frame buffer
 * ==================RETURN===================
 [X] means not doing it
 [~] means its done by openGl 
 */

/* A Handle is a pointer for openGL. I think. it points to a location in memory.
 * 
 * managed vs unmanaged code:
 * managed code is executed with CLR which is responsible for managing memory, performing type verification and garbage collection
 * unmanaged code is executed outside CLR
 * unmanaged code is declared with the unsafe keyword
 * 
 * it means the programmer must:
 * make sure the casting is done right
 * calling the memory allocation function
 * making sure the memory is released when the work is done
 * 
 * fixed:
 * a fixed statement can be applied to a pointer which means the CLR garbage collector ignores the pointer
 * I think it still overwrites the value.
 */


/* THING TO DO:
 * Z index to decide which goes in front. - Mostly a change to window. - useful especially for buttons which must be in front
 * Write better shaders for planets - could do some 3d mapping
 * FixToScreenSpace 
 */


namespace Graphics
{
    class RenderObject2D<Vertex> : Node2D where Vertex : unmanaged
    {
        private readonly int VertexArrayHandle;
        private readonly int VertexBufferHandle;
        private readonly int TextureHandle; 

        private readonly int VertexCount; // number of vertices
        private readonly int VertexSize; // size of vertex in bytes
        private int VertexLength; // number of data points in vertex

        private ShaderProgram ShaderProgram;

        public PolygonMode PolygonMode = PolygonMode.Fill;
        public MaterialFace MaterialFace = MaterialFace.Front;
        
        public bool FixToScreenSpace = false;


        

        public RenderObject2D(float Rotation, Vector2 Scale, Vector2 Position, Vertex[] Vertices, string Texture, string VertexShader, string FragmentShader) : base(Rotation, Scale, Position)
        {
            this.VertexCount = Vertices.Length;
            ShaderProgram = new ShaderProgram(VertexShader, FragmentShader);
            
            Init_BufferArray(out VertexArrayHandle, out VertexBufferHandle, out VertexSize, Vertices);
            TextureHandle = Init_Textures(Texture);

            Matrix3 M = Transform_Matrix;
            unsafe
            {
                fixed (Matrix3* TM = &Transform_Matrix) 
                {

                    ShaderProgram.AddParameter(new Mat3Uniform(ShaderTarget.Vertex, "camera", &M));
                    ShaderProgram.AddParameter(new Mat3Uniform(ShaderTarget.Vertex, "transform", TM));
                    ShaderProgram.AddParameter(new TextureUniform(ShaderTarget.Fragment, "Texture", TextureHandle));

                }
            }            

            ShaderProgram.CompileProgram();
            
            
            //ProgramHandle = Init_Program(VertexShader, FragmentShader);
            
            
            //ShaderProgram["Camera"] = new Mat3Parameter(ParameterType.Vertex, TypeQualifier.Uniform, Shaders.ValueType.Mat3, "Camera", &Test);

        }
        public RenderObject2D(float Rotation, float ScaleX, float ScaleY, float PositionX, float PositionY, Vertex[] Vertices, string Texture, string VertexShader, string FragmentShader) : base(Rotation, new Vector2(ScaleX, ScaleY), new Vector2(PositionX, PositionY))
        {
            this.VertexCount = Vertices.Length;
            ShaderProgram = new ShaderProgram(VertexShader, FragmentShader);
            Init_BufferArray(out VertexArrayHandle, out VertexBufferHandle, out VertexSize, Vertices);

            TextureHandle = Init_Textures(Texture);
            //ProgramHandle = Init_Program(VertexShader, FragmentShader);
            

        }

        /// <summary>
        /// recognises a new attribute in an array for passing to the buffer.
        /// </summary>
        /// <param name="ArrayHandle">The OpenGL Handle ID.</param>
        /// <param name="Size">The number of elements contained in this attirbute.</param>
        /// <param name="Location">The index when delivered to the shader.</param>
        /// <param name="Offset">The relative offset in bytes of where the attribute starts.</param>
        private void LoadBufferAttribute(string Name, Shaders.ValueType Type, int ArrayHandle, int Size, int Location, ref int Offset)
        {
            GL.VertexArrayAttribBinding(ArrayHandle, Location, 0);
            GL.EnableVertexArrayAttrib(ArrayHandle, Location);
            // Handle, index when delivered to shader, number of elements, contains floats, already normalized so false, relative offset from 0 in bytes = 0 
            GL.VertexArrayAttribFormat(ArrayHandle, Location, Size, VertexAttribType.Float, false, Offset);
            Offset += Size * 4; // number of elements * float size


            ShaderProgram.AddParameter(new VertexParameter(ShaderTarget.Vertex, Type, Name));
            


            
             
        }
        
        /// <summary>
        /// Creates an array and buffer in openGl such that the data in the vertices can be unpacked.
        /// </summary>
        /// <param name="ArrayHandle">The OpenGL Handle ID for the array.</param>
        /// <param name="BufferHandle">The OpenGL Handle ID for the buffer.</param>
        /// <param name="VertexSize">The size of the vertex in bytes</param>
        /// <param name="Vertices">The array of vertices.</param>
        private void Init_BufferArray(out int ArrayHandle, out int BufferHandle, out int VertexSize, Vertex[] Vertices)
        {
            /* struct needs to be 'unmanaged' to use sizeof()
             * dont really know what that means but whatever
             * its also unsafe meaning it could potential have different results on different systems
             * it should be fine as long as the vertex doesnt contain strings? and maybe not char?. 
             * floats, ints and bytes are safe.
             */
            unsafe { VertexSize = sizeof(Vertex); }
            
            ArrayHandle = GL.GenVertexArray();
            BufferHandle = GL.GenBuffer();
            
            GL.BindVertexArray(ArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, BufferHandle);
            // create new buffer storage of vertice data
            GL.NamedBufferStorage(BufferHandle, VertexSize * Vertices.Length, Vertices, BufferStorageFlags.MapWriteBit);


            /* iterates through struct members
             * this is gross
             */
            FieldInfo[] FieldInfoArray = typeof(Vertex).GetFields();
            VertexLength = FieldInfoArray.Length;

            int RelativeOffset = 0;
            for (int location = 0; location < VertexLength; location++)
            {
                Type T = FieldInfoArray[location].FieldType;
                string N = FieldInfoArray[location].Name;
                if (T == typeof(Vector2)) LoadBufferAttribute(N, Shaders.ValueType.Vec2, ArrayHandle, 2, location, ref RelativeOffset);
                else if (T == typeof(Vector3)) LoadBufferAttribute(N, Shaders.ValueType.Vec3, ArrayHandle, 3, location, ref RelativeOffset);
                else if (T == typeof(Vector4)) LoadBufferAttribute(N, Shaders.ValueType.Vec4, ArrayHandle, 4, location, ref RelativeOffset);
                else if (T == typeof(Color4)) LoadBufferAttribute(N, Shaders.ValueType.Vec4, ArrayHandle, 4, location, ref RelativeOffset);
                else if (T == typeof(float)) LoadBufferAttribute(N, Shaders.ValueType.Float, ArrayHandle, 1, location, ref RelativeOffset);
                else if (T == typeof(Int32)) LoadBufferAttribute(N, Shaders.ValueType.Int, ArrayHandle, 1, location, ref RelativeOffset);
                else throw new Exception("RenderObject cannot load type " + T.ToString() + " into buffer reliably"); 
                // shouldnt be needed as its already known that its unmanaged

            }
            // link the vertex array and buffer and provide the step size(stride) as size of Vertex
            GL.VertexArrayVertexBuffer(ArrayHandle, 0, BufferHandle, IntPtr.Zero, VertexSize);
        }

        
        
        /// <summary>
        /// compiles and error-checks shaders and attaches them together in a program in OpenGL.
        /// </summary>
        /// <param name="VertName">The name of the vertex shader file in:@\Graphics\Shaders\[name].shader</param>
        /// <param name="FragName">The name of the fragment shader file in:@\Graphics\Shaders\[name].shader</param>
        /// <returns>The program handle ID for OpenGL.</returns>
        private int Init_Program(string VertName, string FragName)
        {
            // creates new program
            int Handle = GL.CreateProgram();

            // compile new shaders
            int Vert = Load_Shader(ShaderType.VertexShader, @"Shaders\" + VertName + "Vert.shader");
            int Frag = Load_Shader(ShaderType.FragmentShader, @"Shaders\" + FragName + "Frag.shader");

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
        
        /// <summary>
        /// Loads code in file and compiles it.
        /// </summary>
        /// <param name="type">the type of shader i.e. vertex or fragment</param>
        /// <param name="path">The file path to the code.</param>
        /// <returns>The shader handle ID for OpenGL.</returns>
        private int Load_Shader(ShaderType type, string path)
        {
            // THING TO DO:
            // should write in and out variables in code automatically by whats in the vertex
            // or just find a better way to do it. this works as long as i dont need any new parameters in the shader.
            // could create a shader object which holds the parameters of the vertex and uniforms that need to be passed.


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



        /// <summary>
        /// opens image file and creates texture.
        /// </summary>
        /// <param name="name">The name of the texture file in: @Graphics/Textures/[name].png</param>
        /// <returns>The texture handle ID for OpenGL.</returns>
        private int Init_Textures(string name)
        {
            int width, height, Handle;
            float[] data = Load_Texture(out width, out height, "Textures/" + name + ".png");
            GL.CreateTextures(TextureTarget.Texture2D, 1, out Handle);
            // level of mipmap, format, width, height
            GL.TextureStorage2D(Handle, 1, SizedInternalFormat.Rgba32f, width, height);

            // bind texture to slot
            GL.BindTexture(TextureTarget.Texture2D, Handle);
            // level ???, offset x, offset y, width, height, format, type, serialized data
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
        /// Serializes image read from file for openGl buffer. assigns values to width and height.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="path">The path to the image.</param>
        /// <returns>The serialised data read from the file in rgba format.</returns>
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

        /// <summary>
        /// Show this object on the screen.
        /// </summary>
        /// <param name="Camera">The Camera transform matrix.</param>
        /// <param name="Time"></param>
        public void Render(Matrix3 Camera, float Time)
        {
            GL.PolygonMode(MaterialFace, PolygonMode);
            // tell openGL to use this objects program
            ShaderProgram.UseProgram();
            
            //GL.UseProgram(ProgramHandle);

            // pass in uniforms
            
            // matrix transforms
            //GL.UniformMatrix3(VertexLength, true, ref Transform_Matrix); // location 3

            //GL.UniformMatrix3(VertexLength + 1, true, ref Camera); // location 4

            // shader variables
            //GL.Uniform1(VertexLength + 2, Time); // location 5
            //GL.Uniform1(VertexLength + 3, 60f); // location 6

            //GL.BindTextures(0, 1, new int[1] { TextureHandle });

            GL.BindVertexArray(VertexArrayHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, VertexCount);
        }
        
        /// <summary>
        /// Called on each frame update.
        /// </summary>
        /// <param name="delta">Time since process was last called.</param>
        public virtual void Process(float delta) { }



        public virtual void OnMouseDown(MouseButtonEventArgs e) { }
        public virtual void OnMouseUp(MouseButtonEventArgs e) { }
        public virtual void OnMouseMove(MouseMoveEventArgs e) { }
    }
}
