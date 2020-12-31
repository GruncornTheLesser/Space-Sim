using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Graphics.Shaders
{
    public enum ParameterTypeQualifier
    {
        Const,
        Uniform,
        Vertex
    }

    class Parameters<Vertex> : Dictionary<string, Parameter>
    {
        private readonly int VertexArrayHandle;
        private readonly int VertexBufferHandle;

        private readonly int VertexCount; // number of vertices
        private readonly int VertexSize; // size of vertex in bytes
        private int VertexLength; // number of data points in vertex

        public Parameters() { }
        //private int Load_Shader(ShaderType type, string path) { }
        //private int LoadBufferAttribute() { }
    }

    struct Parameter
    {
        public string Name;
        public ParameterTypeQualifier Type;
        public Byte[] Value;

        public Parameter(ParameterTypeQualifier type, string name, Byte[] value)
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }
}
