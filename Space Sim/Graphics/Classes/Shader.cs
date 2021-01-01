using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Graphics.Shaders
{
    /* Unfinished
     * very much a thing to do.
     * 
     * The Idea is all the shader parameters are stored in a dictionary
     * then the "parameters" class will generate the initial code for 
     * the vertex and fragment shader.
     */
    public enum PTQ
    {
        Const,
        Uniform,
        Vertex
    }

    class Parameters<Vertex> : List<Parameter>
    {
        private readonly int VertexArrayHandle;
        private readonly int VertexBufferHandle;

        private readonly int VertexCount; // number of vertices
        private readonly int VertexSize; // size of vertex in bytes
        private readonly int VertexLength; // number of data points in vertex

        public Parameters() { }
        //private int Load_Shader(ShaderType type, string path) { }
        //private int LoadBufferAttribute() { }
    }

    struct Parameter
    {
        public string Name;
        public PTQ Type;
        public Byte[] Value;

        public Parameter(PTQ type, string name, Byte[] value)
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }
}
