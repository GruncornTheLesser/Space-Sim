using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
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
 */
namespace Graphics
{
    class RenderObject2D : IDisposable
    {
        private Node2D Transform;

        private readonly int VertexArrayHandle;
        private readonly int VertexBufferHandle;
        private readonly int ProgramHandle;
        private readonly int VertexCount;
        
        private bool Initialized = false;
        private Vertex[] Vertices;

        public RenderObject2D(float Rotation, Vector2 Scale, Vector2 Position, Vertex[] Vertices)
        {
            Transform = new Node2D(Rotation, Scale, Position);            
            
            this.Vertices = Vertices;
            this.VertexCount = Vertices.Length;

            // generate new handles for vertex array and buffer
            VertexArrayHandle = GL.GenVertexArray();
            VertexBufferHandle = GL.GenBuffer();
            
            // bind to current vertex array and vertex buffer
            GL.BindVertexArray(VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);

            

            // create buffer storage of vertice data
            GL.NamedBufferStorage<Vertex>(
                VertexBufferHandle,
                Vertex.Size * Vertices.Length,
                Vertices,
                BufferStorageFlags.MapWriteBit
                );

            // how to read buffer storage
            
            // Position
            GL.VertexArrayAttribBinding(VertexArrayHandle, 0, 0);
            GL.EnableVertexArrayAttrib(VertexArrayHandle, 0);
            
            GL.VertexArrayAttribFormat(VertexArrayHandle,
                0,                      // attribute index, from the shader location = 0
                2,                      // size of attribute, vec4
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                0                       // first item
                );                     
            
            // colour
            GL.VertexArrayAttribBinding(VertexArrayHandle, 1, 0);
            GL.EnableVertexArrayAttrib(VertexArrayHandle, 1);
            GL.VertexArrayAttribFormat(VertexArrayHandle,
                1,                      // attribute index, from the shader location = 1
                4,                      // size of attribute, color4
                VertexAttribType.Float, // contains floats
                false,                  // does not need to be normalized as it is already, floats ignore this flag anyway
                8                      // relative offset after a vec2
                );
            
            // link the vertex array and buffer and provide the stride as size of Vertex
            GL.VertexArrayVertexBuffer(VertexArrayHandle, 0, VertexBufferHandle, IntPtr.Zero, Vertex.Size);
            
            // pretty bad way of handling shaders and programs.
            // creates a new program in memory for each instance of renderobject. shaders will likely be reused so this is dumb.
            // should probably extract to Window and then store textures and shaders in a dictionary or something.
            ProgramHandle = Create_Program(); // this needs to have a input to decide which shaders to load to create the program

            Initialized = true;
        }
        
        
        
        private int Create_Program()
        {
            // creates new program
            int NewProgramHandle = GL.CreateProgram();

            // compile new shaders
            int Vert = Compile_Shader(ShaderType.VertexShader, @"Graphics\Shaders\DefaultVert.shader");
            int Frag = Compile_Shader(ShaderType.FragmentShader, @"Graphics\Shaders\DefaultFrag.shader");

            // attach new shaders
            GL.AttachShader(NewProgramHandle, Vert);
            GL.AttachShader(NewProgramHandle, Frag);

            // link new shaders
            GL.LinkProgram(NewProgramHandle);

            // check for error linking shaders to program
            string info = GL.GetProgramInfoLog(NewProgramHandle);
            if (!string.IsNullOrWhiteSpace(info)) throw new Exception($"Failed to link shaders to program: {info}");

            // detach and delete both shaders
            GL.DetachShader(NewProgramHandle, Vert);
            GL.DetachShader(NewProgramHandle, Frag);
            GL.DeleteShader(Vert);
            GL.DeleteShader(Frag);

            return NewProgramHandle;
        }
        private int Compile_Shader(ShaderType type, string path)
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
        
        
        
        public void Dispose()
        {
            // needed because it isnt handled very well without this? i dont fully understand
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Initialized)
                {
                     // deletes buffers in OpenGL
                    GL.DeleteVertexArray(VertexArrayHandle);
                    GL.DeleteBuffer(VertexBufferHandle);
                    Initialized = false;
                }
            }
        }


        /// <summary>
        /// where object logic goes
        /// </summary>
        /// <param name="delta">The time since process was last called.</param>
        private float Time;
        public virtual void Process(float delta)
        {
            Time += delta;
            //Transform.Rotation += delta;
            Transform.Position = new Vector2(MathF.Sin(Time), -MathF.Cos(Time));

        }
        /// <summary>
        /// Renders this object.
        /// </summary>
        public void Render() 
        {
            GL.UseProgram(ProgramHandle);
            // pass in renderobject transform matrix
            GL.UniformMatrix3(
                2,
                false,
                ref Transform.Matrix
                );


            GL.BindVertexArray(VertexArrayHandle);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, VertexCount);
        }
    }
}
