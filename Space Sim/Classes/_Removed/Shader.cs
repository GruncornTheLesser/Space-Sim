using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Shaders
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
        Uniform,
        Vertex
    }
    public enum ValueType
    {
        Float,
        Int,
        Vec2,
        Vec3,
        Vec4,
        Mat2,
        Mat3,
        Mat4,
        Texture
    }
    public enum ParameterType
    {
        Fragment,
        Vertex,
        Both
    }

    class ShaderProgram<Vertex> : Collection<Parameter>
    {
        private int ProgramHandle;

        private string vertpath;
        private string fragpath;

        public string VertexShaderPath
        {
            set
            {
                vertpath = value;
                ready = false;
            }
            get
            {
                return vertpath;
            }
        }
        public string FragmentShaderPath
        {
            set
            {
                fragpath = value;
                ready = false;
            }
            get
            {
                return vertpath;
            }
        }
        
        public bool ready;

        public ShaderProgram(string VertexPath, string FragPath) 
        {
            ready = false;

        }
        public void UseProgram() 
        {
            if (!ready) 
            {
            #warning Program is not ready to be used. Program shader path has been changed or parameter has been added. Program must be compiled again to see changed
            } 
            GL.UseProgram(ProgramHandle);


        }
        public void CompileProgram() 
        {
            // creates new program
            ProgramHandle = GL.CreateProgram();

            // compile new shaders
            int Vert = Load_Shader(ShaderType.VertexShader, @"Shaders\" + vertpath + "Vert.shader");
            int Frag = Load_Shader(ShaderType.FragmentShader, @"Shaders\" + fragpath + "Frag.shader");

            // attach new shaders
            GL.AttachShader(ProgramHandle, Vert);
            GL.AttachShader(ProgramHandle, Frag);

            // link new shaders
            GL.LinkProgram(ProgramHandle);

            // check for error linking shaders to program
            string info = GL.GetProgramInfoLog(ProgramHandle);
            if (!string.IsNullOrWhiteSpace(info)) throw new Exception($"Failed to link shaders to program: {info}");

            // detach and delete both shaders
            GL.DetachShader(ProgramHandle, Vert);
            GL.DetachShader(ProgramHandle, Frag);
            GL.DeleteShader(Vert);
            GL.DeleteShader(Frag);

            ready = true;
        }

        private int Load_Shader(ShaderType shadertype, string path) 
        {
            // create new shader object in OpenGL
            int NewShaderHandle = GL.CreateShader(shadertype);

            // get code from file
            string code = "#version 450 core\n";
            
            // writes initial script for passing in vairables from vertex
            int location = 0;
            if (shadertype == ShaderType.FragmentShader)
            {
                foreach (Parameter P in this) P.GenFragDef(ref location);
            }
            else
            {
                foreach (Parameter P in this) P.GenVertDef(ref location);
            }
            code += File.ReadAllText(path);
            
            // attaches shader and code
            GL.ShaderSource(NewShaderHandle, code);

            // compiles shader code
            GL.CompileShader(NewShaderHandle);

            // checks if compilation worked
            string info = GL.GetShaderInfoLog(NewShaderHandle);
            if (!string.IsNullOrWhiteSpace(info)) throw new Exception($"Failed to compile {shadertype} shader: {info}");

            return NewShaderHandle;
        }


        unsafe public void AddParameter(TypeQualifier typequalifier, ValueType valuetype, ParameterType parametertype, string name, int* valuepointer)
        {
            this.Add(new Parameter(parametertype, typequalifier, valuetype, name, valuepointer));
        }

    }

    unsafe class Parameter
    {
        public ParameterType ParameterType; // Frag, Vert
        public TypeQualifier TypeQualifier; // const, uniform, in
        public ValueType ValueType; // float, vec2, mat3, Texture
        public string Name;
        public int* ValuePointer;
        public int location;
        
        public Parameter(ParameterType parametertype, TypeQualifier typequalifier, ValueType valuetype, string name, int* valuepointer)
        {
            ParameterType = parametertype;
            TypeQualifier = typequalifier;
            ValueType = valuetype;
            Name = name;
            ValuePointer = valuepointer;
        }
        
        public string GenVertDef(ref int location) 
        {
            
            if (ParameterType == ParameterType.Fragment) return "";
            
            string code;
            switch (TypeQualifier) 
            {
                case TypeQualifier.Vertex:
                    if (ValueType == ValueType.Texture) throw new Exception("Cannot store texture on a vertex.");
                    
                    code = $"layout(location = {location++}) in {ValueType.ToString().ToLower()} {Name};\n";
                    if (ParameterType == ParameterType.Both) code += $"out {ValueType.ToString().ToLower()} {Name}\n";
                    
                    break;
                
                
                case TypeQualifier.Uniform:
                    if (ValueType == ValueType.Texture) code = $"uniform sampler2D {Name};\n";
                    else code = $"layout(location = {location++}) uniform {ValueType.ToString().ToLower()} {Name};\n";
                    
                    break;
                default:
                    throw new Exception("unidentified type qualifier.");
            }
            return code;
        }
        public string GenFragDef(ref int location) 
        {
            if (ParameterType == ParameterType.Vertex) return "";

            string code;
            switch (TypeQualifier)
            {
                case TypeQualifier.Vertex:
                    if (ValueType == ValueType.Texture) throw new Exception("Cannot store texture on a vertex.");

                    code = $"in {ValueType.ToString().ToLower()} {Name};\n";

                    break;

                case TypeQualifier.Uniform:
                    if (ValueType == ValueType.Texture) code = $"uniform sampler2D {Name};\n";
                    else code = $"layout(location = {location++}) uniform {ValueType.ToString().ToLower()} {Name};\n";

                    break;
                default:
                    throw new Exception("unidentified type qualifier.");
            }

            return code;
        }
    
        public void PassToShader() 
        {
        }
    }
}
