using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using OpenTK.Windowing.Common;
using Shaders;
using DeepCopy;
using OpenTK.Windowing.GraphicsLibraryFramework;
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
 * static Texture manager object
 */


namespace Graphics
{

    /// <summary>
    /// An Object that renders onto the screen.
    /// Button hitbox doesnt change on window rescale
    /// </summary>


    abstract class RenderObject2D : Node2D
    {
        public ShaderProgram ShaderProgram;

        private readonly int VertexArrayHandle;
        private readonly int VertexBufferHandle;

        // private readonly int VertexCount; // number of vertices, used in render

        private bool fixtoscreen = false;
        private int z_index = 1;
        public bool FixToScreen
        {
            set
            {
                fixtoscreen = value;
                if (fixtoscreen) ShaderProgram["camera"].SetUniform(new DeepCopy<Matrix3>(GameObjects.Window.Get_BaseMat));
                else ShaderProgram["camera"].SetUniform(new DeepCopy<Matrix3>(GameObjects.Window.Get_CamMat));
            }
            get => fixtoscreen;
        }
        public int Z_index
        {
            get => z_index;
            set => Set_Z_Index(value);
        }
        public Action<int> Set_Z_Index; // used in RenderObjects List to update list when z index changes 
        public PrimitiveType RenderingType = PrimitiveType.Triangles;

        private Vertex2D[] vertices;
        public Vertex2D[] Vertices
        {
            get => vertices;
            set
            {
                vertices = value;

                GL.BindVertexArray(VertexArrayHandle);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertex2D.SizeInBytes * vertices.Length), vertices, BufferUsageHint.StreamDraw);

            }
        }
        public RenderObject2D(Vertex2D[] Vertices, string VertexShader, string FragmentShader) : base(0, 1, 1, 0, 0)
        {
            AttachEvents();

            Set_Z_Index = value => z_index = value;
            
            // initiate the shader program with the file paths to the shaders
            ShaderProgram = new ShaderProgram(VertexShader, FragmentShader);

            // Buffer array is the buffer that stores the vertices. this requires shaderprogram to be initiated because it adds in the shader parameters of the vertices
            Init_BufferArray(out VertexArrayHandle, out VertexBufferHandle, Vertices);

            // shader uniforms added in derived object
        }

        /// <summary>
        /// constructs square render object without passing Shader Program parameters
        /// </summary>
        public RenderObject2D() : base(0, 1, 1, 0, 0)
        {
            AttachEvents();
            Set_Z_Index = value => z_index = value;

            ShaderProgram = new ShaderProgram("Default", "Default");

            Vertex2D[] Square = new Vertex2D[6] {
                new Vertex2D(-1, 1, 0, 0, 1, 1, 1, 1),
                new Vertex2D(-1,-1, 0, 1, 1, 1, 1, 1),
                new Vertex2D( 1,-1, 1, 1, 1, 1, 1, 1),

                new Vertex2D( 1,-1, 1, 1, 1, 1, 1, 1),
                new Vertex2D(-1, 1, 0, 0, 1, 1, 1, 1),
                new Vertex2D( 1, 1, 1, 0, 1, 1, 1, 1),
                };

            Init_BufferArray(out VertexArrayHandle, out VertexBufferHandle, Square);
            
            // shader uniforms added in derived object
        }

        public void AttachEvents()
        {
            EventManager.MouseDown += OnMouseDown;
            EventManager.MouseUp += OnMouseUp;
            EventManager.MouseWheel += OnMouseWheel;
            EventManager.MouseMove += OnMouseMove;
            EventManager.Process += OnProcess;
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
            // bind to current location starting from 0 and add an attribute
            GL.VertexArrayAttribBinding(ArrayHandle, Location, 0);
            GL.EnableVertexArrayAttrib(ArrayHandle, Location);
            
            // set up format of the attribute
            // Handle, index when delivered to shader, number of elements, contains floats, already normalized so false, relative offset from 0 in bytes = 0 
            GL.VertexArrayAttribFormat(ArrayHandle, Location, Size, VertexAttribType.Float, false, Offset);
            Offset += Size * 4; // number of elements * float size (in bytes)

            // adds parameter in shader program.
            ShaderProgram.AddVertexParameter(new VertexParameter<T>(ShaderTarget.Vertex, Name));
        }
        private void LoadBufferAttribute<T>(ref int Location, ref int ByteOffset, int ArrayHandle, string Name) where T : unmanaged
        {
            int ByteSize; // the size of this type in bytes
            unsafe { ByteSize = sizeof(T); }

            GL.VertexArrayAttribBinding(ArrayHandle, Location, 0); // generates a new attribute binding to location vertex buffer array
            GL.EnableVertexArrayAttrib(ArrayHandle, Location); // enables the attribute binding
            GL.VertexArrayAttribFormat(ArrayHandle, Location, ByteSize / 4, VertexAttribType.Float, false, ByteOffset);

            Location++;
            ByteOffset += ByteSize;

            // adds parameter in shader program.
            ShaderProgram.AddVertexParameter(new VertexParameter<T>(ShaderTarget.Vertex, Name));

        }
        /// <summary>
        /// Creates an array and buffer in openGl such that the data in the vertices can be unpacked.
        /// </summary>
        /// <param name="ArrayHandle">The OpenGL Handle ID for the array.</param>
        /// <param name="BufferHandle">The OpenGL Handle ID for the buffer.</param>
        private void Init_BufferArray(out int ArrayHandle, out int BufferHandle, Vertex2D[] Vertices)
        {
            // generates and binds vertex array object
            ArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(ArrayHandle);
            
            // add vertex attributes in openGl and shaderprogram
            int Location = 0;
            int ByteOffset = 0;
            LoadBufferAttribute<Vector2>(ref Location, ref ByteOffset, ArrayHandle, "VertUV");
            LoadBufferAttribute<Vector2>(ref Location, ref ByteOffset, ArrayHandle, "TextureUV");
            LoadBufferAttribute<Vector4>(ref Location, ref ByteOffset, ArrayHandle, "VertColour");
            
            // generates and binds vertex buffer object
            BufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, BufferHandle); // use this buffer

            GL.VertexArrayVertexBuffer(ArrayHandle, 0, BufferHandle, IntPtr.Zero, Vertex2D.SizeInBytes); // assigns vertice data

            this.Vertices = Vertices;
        }

        /// <summary>
        /// opens image file and creates texture.
        /// </summary>
        /// <param name="name">The name of the texture file in: @Graphics/Textures/[name].png</param>
        /// <returns>The texture handle ID for OpenGL.</returns>
        protected int Init_Textures(string name)
        {
            int width, height, Handle;
            float[] data = Load_Texture(out width, out height, "Textures/" + name + ".png");
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
        private float[] Load_Texture(out int width, out int height, string path)
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

        /// <summary>
        /// Show this object on the screen.
        /// </summary>
        public void Render()
        {
            TextureManager.TexturesLoaded = 0;
            // tell openGL to use this objects program
            ShaderProgram.UseProgram();

            // use current vertex array
            GL.BindVertexArray(VertexArrayHandle);

            // draw these vertices in triangles, 0 to the number of vertices
            GL.DrawArrays(RenderingType, 0, Vertices.Length);
        }
        
        /// <summary>
        /// Called on each frame update.
        /// </summary>
        /// <param name="delta">Time since process was last called.</param>
        public abstract void OnProcess(float delta);

        /// <summary>
        /// called when the mouse is clicked
        /// </summary>
        /// <param name="MouseState">the state of the mouse.</param>
        /// <param name="e">event arguments of the button click.</param>
        public virtual void OnMouseDown(MouseState MouseState, MouseButtonEventArgs e) { }
        
        /// <summary>
        /// when the mouse button releases
        /// </summary>
        /// <param name="MouseState">the state of the mouse.</param>
        /// <param name="e">event arguments of the button click.</param>
        public virtual void OnMouseUp(MouseState MouseState, MouseButtonEventArgs e) { }
        
        /// <summary>
        /// when the mouse moves.
        /// </summary>
        /// <param name="MouseState">the state of the mouse.</param>
        /// <param name="e">event arguments of the button move.</param>
        public virtual void OnMouseMove(MouseState MouseState, MouseMoveEventArgs e) { }
        
        /// <summary>
        /// called when mouse wheel scrolls.
        /// </summary>
        /// <param name="MouseState">the state of the mouse.</param>
        /// <param name="e">event arguments of the wheel scroll.</param>
        public virtual void OnMouseWheel(MouseState MouseState, MouseWheelEventArgs e) { }
    }
}
