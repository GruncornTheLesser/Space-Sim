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
     * 
     * idk how i want to deal with textures
     */

    /// <summary>
    /// parameter type qualifier
    /// </summary>
    public enum TypeQualifier
    {
        Const,
        Uniform,
        Vertex
    }

    class ShaderParameters<Vertex> : List<Parameter>
    {
        private readonly int VertexArrayHandle;
        private readonly int VertexBufferHandle;

        private readonly int VertexCount; // number of vertices
        private readonly int VertexSize; // size of vertex in bytes
        private readonly int VertexLength; // number of data points in vertex

        public ShaderParameters() { }
        //public void UseShader() { }
        //private int Load_Shader(ShaderType type, string path) { }
        //private int LoadBufferAttribute() { }
        private string GenerateShaderCode() 
        {
            string code = "#version 450 core";
            int indexlocation = 0;
            foreach (Parameter P in this) 
            {
                if (P.VertexParameter) code += P.GenerateDefinition(ref indexlocation);
            }
            return "";
        }

    }

    class Parameter
    {
        public TypeQualifier TypeQualifier; // parameter type qualifier ie const, uniform, in
        public string Name;
        public float[] Value;// maybe should be binary definition
        public bool VertexParameter; // which shader it belongs to
        public bool FragmentParameter;

        public Parameter(TypeQualifier typequalifier, float[] value, string name, bool fragmentparameter, bool vertexparameter) 
        {
            TypeQualifier = typequalifier;
            Name = name;
            Value = value;
            VertexParameter = vertexparameter;
            FragmentParameter = fragmentparameter;
            if (TypeQualifier == TypeQualifier.Vertex) VertexParameter = true;
        }
        public string GenerateDefinition(ref int location) 
        {
            string code;
            switch (TypeQualifier) 
            {
                case TypeQualifier.Const:
                    switch (Value.Length)
                    {
                    case 1:
                        code = $"const float {Name} = {Value[0]};";
                        break;
                    case 2:
                        code = $"const vec2 {Name} = vec2({Value[0]}, {Value[1]});";
                        break;
                    case 3:
                        code = $"const vec3 {Name} = vec3({Value[0]}, {Value[1]}, {Value[2]});";
                        break;
                    case 4:
                        code = $"const vec4 {Name} = vec4({Value[0]}, {Value[1]}, {Value[2]}, {Value[3]});";
                        break;
                    case 9:
                        code = $"const mat3 {Name} = mat3({Value[0]}, {Value[1]}, {Value[2]}, {Value[3]}, {Value[4]}, {Value[5]}, {Value[6]}, {Value[7]}, {Value[8]});";
                        break;
                    case 16:
                        code = $"const mat4 {Name} = mat4({Value[0]}, {Value[1]}, {Value[2]}, {Value[3]}, {Value[4]}, {Value[5]}, {Value[6]}, {Value[7]}, {Value[8]}, {Value[9]}, {Value[10]}, {Value[11]}, {Value[12]}, {Value[13]}, {Value[14]}, {Value[15]});";
                        break;
                    default:
                        throw new Exception($"failed to set in constant value of {Name}.");
                    }
                    break;

                case TypeQualifier.Uniform:
                    switch (Value.Length)
                    {
                        case 1:
                            code = $"uniform float {Name};";
                            break;
                        case 2:
                            code = $"layout(location = {location}) uniform layout(location = {location}) vec2 {Name};";
                            break;
                        case 3:
                            code = $"layout(location = {location}) uniform vec3 {Name};";
                            break;
                        case 4:
                            code = $"layout(location = {location}) uniform vec4 {Name};";
                            break;
                        case 9:
                            code = $"layout(location = {location}) uniform mat3 {Name};";
                            break;
                        case 16:
                            code = $"layout(location = {location}) uniform mat4 {Name};";
                            break;
                        default:
                            throw new Exception("Unsure how to handle a value of this type.");
                    }
                    break;

                case TypeQualifier.Vertex:
                    switch (Value.Length)
                    {
                        case 1:
                            code = $"uniform float {Name};";
                            break;
                        case 2:
                            code = $"layout(location = {location}) in layout(location = {location}) vec2 {Name};";
                            break;
                        case 3:
                            code = $"layout(location = {location}) in vec3 {Name};";
                            break;
                        case 4:
                            code = $"layout(location = {location}) in vec4 {Name};";
                            break;
                        case 9:
                            code = $"layout(location = {location}) in mat3 {Name};";
                            break;
                        case 16:
                            code = $"layout(location = {location}) in mat4 {Name};";
                            break;
                        default:
                            throw new Exception("Unsure how to handle a value of this type.");
                    }
                    break;

                default:
                    throw new Exception("Type qualifier unrecognised.");
            }

            return code;

        }

    }
}
