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
using DeepCopy;
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

/* A Handle is a pointer for openGL. i kept the name because it made it simpler for me to understand.
 * 
 * managed vs unmanaged code:
 * managed code is executed with CLR which is responsible for managing memory, performing type verification and garbage collection
 * unmanaged code is executed outside CLR
 * unmanaged code is declared with the unsafe keyword
 */

/* it means the programmer must:
 * make sure the casting is done right
 * calling the memory allocation function
 * making sure the memory is released when the work is done
 */ 

/* fixed:
 * a fixed statement can be applied to a pointer which means the CLR garbage collector ignores the pointer
 * I think it still overwrites the value.
 */


/* THING TO DO:
 * Z index to decide which goes in front. - Mostly a change to window. - useful especially for buttons which must be in front
 * Write better shaders for planets - could do some 3d mapping
 * FixToScreenSpace 
 * 
 */


namespace Graphics
{
    abstract class RenderObject2D<Vertex> : Node2D where Vertex : unmanaged
    {
        private readonly int VertexArrayHandle;
        private readonly int VertexBufferHandle;
        private readonly int TextureHandle;

        private readonly int VertexCount; // number of vertices, used in render
        private readonly int VertexSize; // size of vertex in bytes
        private int VertexLength; // number of data points in vertex

        public ShaderProgram ShaderProgram;

        public PolygonMode PolygonMode = PolygonMode.Fill;
        public MaterialFace MaterialFace = MaterialFace.Front;

        private int z_index = 1;
        public Action<int> Set_Z_Index;
        public int Z_index
        {
            get => z_index; 
            set => Set_Z_Index(value);
        }



        public RenderObject2D(float Rotation, Vector2 Scale, Vector2 Position, Vertex[] Vertices, DeepCopy<Matrix3> CameraCopy, DeepCopy<float> TimeCopy, string Texture, string VertexShader, string FragmentShader) : base(Rotation, Scale, Position)
        {
            Set_Z_Index = value => z_index = value;

            ShaderProgram = new ShaderProgram(VertexShader, FragmentShader);
            Init_BufferArray(out VertexArrayHandle, out VertexBufferHandle, out VertexSize, Vertices);
            this.VertexCount = Vertices.Length;
            
            TextureHandle = Init_Textures(Texture);

            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform"));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", CameraCopy));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Both, "Time", TimeCopy));
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "Texture", TextureHandle));

            ShaderProgram.CompileProgram();

            ShaderProgram["transform"].SetUniform((IDeepCopy)TransformCopy);
        }

        /// <summary>
        /// recognises a new attribute in an array for passing to the buffer.
        /// </summary>
        /// <param name="ArrayHandle">The OpenGL Handle ID.</param>
        /// <param name="Size">The number of elements contained in this attirbute.</param>
        /// <param name="Location">The index when delivered to the shader.</param>
        /// <param name="Offset">The relative offset in bytes of where the attribute starts.</param>
        private void LoadBufferAttribute<T>(string Name, int ArrayHandle, int Size, int Location, ref int Offset)
        {
            GL.VertexArrayAttribBinding(ArrayHandle, Location, 0);
            GL.EnableVertexArrayAttrib(ArrayHandle, Location);
            
            // Handle, index when delivered to shader, number of elements, contains floats, already normalized so false, relative offset from 0 in bytes = 0 
            GL.VertexArrayAttribFormat(ArrayHandle, Location, Size, VertexAttribType.Float, false, Offset);
            Offset += Size * 4; // number of elements * float size

            // adds parameter in shader program.
            // this will add it to the scripts.
            ShaderProgram.AddVertexParameter(new VertexParameter<T>(ShaderTarget.Vertex, Name));
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
            /* using system.reflection allows me to get the size of a struct using sizeof()
             * struct needs to be 'unmanaged' to use sizeof()
             */
            unsafe { VertexSize = sizeof(Vertex); }
            
            // generate new opengl handles
            ArrayHandle = GL.GenVertexArray();
            BufferHandle = GL.GenBuffer();
            
            // bind new array and buffer
            GL.BindVertexArray(ArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, BufferHandle);
            
            // create new buffer storage of vertice data
            GL.NamedBufferStorage(BufferHandle, VertexSize * Vertices.Length, Vertices, BufferStorageFlags.MapWriteBit);


            // iterates through struct members
            FieldInfo[] FieldInfoArray = typeof(Vertex).GetFields();
            VertexLength = FieldInfoArray.Length;

            int RelativeOffset = 0;
            for (int location = 0; location < VertexLength; location++)
            {
                Type T = FieldInfoArray[location].FieldType;
                string Name = FieldInfoArray[location].Name;
                if (T == typeof(Vector2)) LoadBufferAttribute<Vector2>(Name, ArrayHandle, 2, location, ref RelativeOffset);
                else if (T == typeof(Vector3)) LoadBufferAttribute<Vector3>(Name, ArrayHandle, 3, location, ref RelativeOffset);
                else if (T == typeof(Vector4)) LoadBufferAttribute<Vector4>(Name, ArrayHandle, 4, location, ref RelativeOffset);
                else if (T == typeof(Color4)) LoadBufferAttribute<Vector4>(Name, ArrayHandle, 4, location, ref RelativeOffset);
                else if (T == typeof(float)) LoadBufferAttribute<float>(Name, ArrayHandle, 1, location, ref RelativeOffset);
                else if (T == typeof(Int32)) LoadBufferAttribute<Int32>(Name, ArrayHandle, 1, location, ref RelativeOffset);
                else throw new Exception("RenderObject failed to load type " + T.ToString() + " into buffer array"); 
                // shouldnt be needed as its already known that its unmanaged

            }
            // link the vertex array and buffer and provide the step size(stride) as size of Vertex
            GL.VertexArrayVertexBuffer(ArrayHandle, 0, BufferHandle, IntPtr.Zero, VertexSize);
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
        public void Render()
        {
            GL.PolygonMode(MaterialFace, PolygonMode);
            // tell openGL to use this objects program
            ShaderProgram.UseProgram();
            
            GL.BindVertexArray(VertexArrayHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, VertexCount);
        }
        
        /// <summary>
        /// Called on each frame update.
        /// </summary>
        /// <param name="delta">Time since process was last called.</param>
        public abstract void Process(float delta);


        public virtual void OnMouseDown(MouseButtonEventArgs e) { }
        public virtual void OnMouseUp(MouseButtonEventArgs e) { }
        public virtual void OnMouseMove(MouseMoveEventArgs e) { }
    }
}
