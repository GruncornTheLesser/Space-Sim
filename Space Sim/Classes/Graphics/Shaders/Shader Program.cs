using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections;
using DeepCopy;

namespace Shaders
{

    class ShaderProgram
    {
        private int ProgramHandle;
        private string vertpath;
        private string fragpath;

        private Dictionary<string, UniformParameter> UniformParameters = new Dictionary<string, UniformParameter>();
        private List<IVertexParameter> VertexParameters = new List<IVertexParameter>();
        public UniformParameter this[string Name]
        {
            get => UniformParameters[Name];
        }


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

        public PolygonMode PolygonMode = PolygonMode.Fill;
        public MaterialFace MaterialFace = MaterialFace.FrontAndBack;

        public bool ready = false;

        public ShaderProgram(string VertexPath, string FragmentPath)
        {
            vertpath = VertexPath;
            fragpath = FragmentPath;
        }

        public void AddUniform(UniformParameter NewUniform)
        {
            if (UniformParameters.ContainsKey(NewUniform.name))
            {
                throw new Exception($"the name {NewUniform.name} is already taken on this shader program");
            }

            UniformParameters[NewUniform.name] = NewUniform;
        }
        public void AddVertexParameter(IVertexParameter NewVertexParameter)
        {
            VertexParameters.Add(NewVertexParameter);
        }

        public void UseProgram()
        {
            GL.PolygonMode(MaterialFace, PolygonMode);
            GL.UseProgram(ProgramHandle);
            foreach (string name in UniformParameters.Keys) UniformParameters[name].UpdateUniform();
        }
      
        public void CompileProgram()
        {
            GL.DeleteProgram(ProgramHandle);

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
            string code = $"#version 450 core{Environment.NewLine}";

            // writes initial variable definition at start of script
            int location = 0;

            // generate uniform definitions
            if (shadertype == ShaderType.VertexShader)
            {
                // generate vertex defintions
                foreach (IVertexParameter P in VertexParameters) code += P.VertDefinition(ref location);
                foreach (string name in UniformParameters.Keys) code += UniformParameters[name].VertDefinition(ref location);
            }
            else
            {
                foreach (string name in UniformParameters.Keys) code += UniformParameters[name].FragDefinition(ref location);
            }
            
            // read file and add text to code
            code += File.ReadAllText(path);

            // attaches shader and code
            GL.ShaderSource(NewShaderHandle, code);

            // compiles shader code
            GL.CompileShader(NewShaderHandle);

            // checks if compilation worked
            string info = GL.GetShaderInfoLog(NewShaderHandle);
            if (!string.IsNullOrWhiteSpace(info)) throw new Exception($"Failed to compile {shadertype}{Environment.NewLine}{code}{Environment.NewLine}{info}");

            return NewShaderHandle;
        }
    }
}

