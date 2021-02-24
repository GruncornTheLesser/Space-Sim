using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
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
 * capabilty to set individual vertice and use BufferSubData -> means it doesnt need to create a new Buffer each time
 * Hide Object
 */


namespace Graphics
{

    /// <summary>
    /// An Object that renders onto the screen.
    /// </summary>
    abstract class RenderObject2D : Transform
    {
        #region OpenGLHandles
        /// <summary>
        /// a vertex array object is an object that contains one or more vertex buffer objects and is designed to store vertice information. This means the vertex attributes have been given 'locations', among other things, for identifcation.
        /// </summary>
        private readonly int VertexArrayHandle; // vertex array object handle
        
        
        
        /// <summary>
        /// the raw data of the vertices streamed into a float buffer. The vertices havent been identified.
        /// </summary>
        private readonly int VertexBufferHandle; // vertex buffer object handle
        #endregion

        #region Properties
        /// <summary>
        /// fixes this render object to the screen. -> positions between -1 and 1 will appear on the screen independent of camera position. 
        /// for both fixed and unfixed aspect ratio is maintained.
        /// </summary>
        public bool FixToScreen
        {
            get => fixtoscreen;
            set
            {
                fixtoscreen = value;
                if (fixtoscreen) ShaderProgram["camera"].SetUniform(new DeepCopy<Matrix3>(() => RenderWindow.Camera.BaseMatrix)); // change to use base matrix of camera ie scaled to maintain aspect ratio but not translated or rotated
                else ShaderProgram["camera"].SetUniform(new DeepCopy<Matrix3>(() => RenderWindow.Camera.Transform_Matrix)); // change to use camera matrix
            }
        }
        private bool fixtoscreen = false;

        /// <summary>
        /// adds and removes this object from the render list
        /// </summary>
        public bool Visible
        {
            get => visible;
            set => Set_visible(value);
            
        }
        private bool visible = true;
        public Action<bool> Set_visible;

        /// <summary>
        /// determines which render object appears on top. 
        /// </summary>
        public int Z_index
        {
            get => z_index;
            set => Set_Z_Index(value); // uses the action -> 'z_index = value' defined in constructor
        }
        private int z_index = 1;
        /// <summary>
        /// called when this objects z index is changed
        /// </summary>
        public Action<int> Set_Z_Index;
        
        /// <summary>
        /// individual vertices belonging to this render object.
        /// Its best to set vertice in one go as it reduces the number of times it must be bound and new data passed in
        /// </summary>
        public Vertex2D[] VertexArray
        {
            get => vertexarray;
            set
            {
                vertexarray = value;
                // updates vertex buffer object
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle); // use this buffer array
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertex2D.SizeInBytes * vertexarray.Length), vertexarray, BufferUsageHint.StreamDraw); // sets and creates buffer data
            }
        }
        private Vertex2D[] vertexarray;
        #endregion

        #region Rendering Fields
        // used in Render() to determine how this object renders
        protected PrimitiveType RenderingType = PrimitiveType.Triangles;
        protected PolygonMode PolygonMode = PolygonMode.Fill;
        protected MaterialFace MaterialFace = MaterialFace.FrontAndBack;

        #endregion

        protected static readonly Vertex2D[] SquareMesh = new Vertex2D[6] {
            new Vertex2D(-0.5f, 0.5f, 0, 0, 1, 1, 1, 1),
            new Vertex2D(-0.5f,-0.5f, 0, 1, 1, 1, 1, 1),
            new Vertex2D( 0.5f,-0.5f, 1, 1, 1, 1, 1, 1),

            new Vertex2D( 0.5f,-0.5f, 1, 1, 1, 1, 1, 1),
            new Vertex2D(-0.5f, 0.5f, 0, 0, 1, 1, 1, 1),
            new Vertex2D( 0.5f, 0.5f, 1, 0, 1, 1, 1, 1),
            }; // for convenience

        /// <summary>
        /// The Shader program manages the code compilation, uniform passing
        /// </summary>
        public ShaderProgram ShaderProgram;
        
        /// <summary>
        /// </summary>
        /// <param name="Vertices">The vertex array.</param>
        /// <param name="VertexShader">the file path to the vertex shader code</param>
        /// <param name="FragmentShader">the file path to the fragment shader code</param>
        public RenderObject2D(Vertex2D[] Vertices, string VertexShader, string FragmentShader) : base(0, 1, 1, 0, 0)
        {
            Set_Z_Index = value => z_index = value;
            Set_visible = (value) =>
            {
                visible = value;
                if (visible) RenderList.Add(this);
                else RenderList.Remove(this);
            };
            Visible = true;

            RenderList.Add(this);            

            // initiate the shader program with the file paths to the shaders
            ShaderProgram = new ShaderProgram(VertexShader, FragmentShader);

            // Buffer array is the buffer that stores the vertices. this requires shaderprogram to be initiated because it adds in the shader parameters of the vertices
            Init_BufferArray(out VertexArrayHandle, out VertexBufferHandle, Vertices);

            // shader uniforms added in derived object
        }
        /// <summary>
        /// initiates a square render object with default shaders
        /// </summary>
        public RenderObject2D() : base(0, 1, 1, 0, 0)
        {
            Set_Z_Index = value => z_index = value;
            Set_visible = (value) =>
            {
                visible = value;
                if (visible) RenderList.Add(this);
                else RenderList.Remove(this);
            };
            Visible = true;


            // initiate the shader program with the default shaders
            ShaderProgram = new ShaderProgram("Default", "Default");

            // create default square for vertices
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

        #region Vertex Setup
        /// <summary>
        /// recognises a new attribute in an array when passed in from buffer.
        /// </summary>
        /// <param name="Location">The parameter index when delivered to the shader. increments on return.</param>
        /// <param name="ByteLocation">The memory index when delivered to shader. Adds size in bytes of 'T' on return.</param>
        /// <param name="ArrayHandle">The OpenGL Handle ID of Vertex Array Attribute.</param>
        /// <param name="Name">The name of this parameter. Must match shader script.</param>
        /// <typeparam name="T">The type of the attribute. used for the shader program.</typeparam>
        private void LoadBufferAttribute<T>(ref int Location, ref int ByteLocation, int ArrayHandle, string Name) where T : unmanaged
        {
            int ByteSize; // the size of this type in bytes
            unsafe { ByteSize = sizeof(T); }

            GL.VertexArrayAttribBinding(ArrayHandle, Location, 0); // generates a new attribute binding to location vertex buffer array
            GL.EnableVertexArrayAttrib(ArrayHandle, Location); // enables the attribute binding
            GL.VertexArrayAttribFormat(ArrayHandle, Location, ByteSize / 4, VertexAttribType.Float, false, ByteLocation); // defines attribute location, ByteSize/4 = FloatSize

            Location++; // increments Location
            ByteLocation += ByteSize; // Adds ByteSize to ByteLocation

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
            GL.BindVertexArray(ArrayHandle); // uses this vertex array
            
            // add vertex attributes in openGl and shaderprogram
            int Location = 0;
            int ByteOffset = 0;
            LoadBufferAttribute<Vector2>(ref Location, ref ByteOffset, ArrayHandle, "VertUV");
            LoadBufferAttribute<Vector2>(ref Location, ref ByteOffset, ArrayHandle, "TextureUV");
            LoadBufferAttribute<Vector4>(ref Location, ref ByteOffset, ArrayHandle, "VertColour");
            
            // generates and binds vertex buffer object
            BufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, BufferHandle); // uses this buffer

            GL.VertexArrayVertexBuffer(ArrayHandle, 0, BufferHandle, IntPtr.Zero, Vertex2D.SizeInBytes); // assigns vertice data

            this.VertexArray = Vertices; // sets array attribute to use buffers after buffer has been set
        }
        #endregion


        /// <summary>
        /// Show this object on the screen.
        /// </summary>
        public void Render()
        {
            ShaderProgram.UseProgram(); // tell openGL to use this objects program
            GL.BindVertexArray(VertexArrayHandle);// use current vertex array
            GL.PolygonMode(MaterialFace, PolygonMode); // use this programs rendering modes
            GL.DrawArrays(RenderingType, 0, VertexArray.Length); // draw these vertices in triangles, 0 to the number of vertices
        }       
    }
}
