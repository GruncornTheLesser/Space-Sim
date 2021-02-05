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
        // the handle in openGl for this program
        private int ProgramHandle;
        // file paths to shaders
        private string vertpath;
        private string fragpath;

        // collections for parameters
        private Dictionary<string, UniformParameter> UniformParameters = new Dictionary<string, UniformParameter>();
        private List<IVertexParameter> VertexParameters = new List<IVertexParameter>();

        /// <summary>
        /// Get uniform parameter. used for setting uniform after program has been compliled.
        /// </summary>
        /// <param name="Name">The name of the parameter.</param>
        /// <returns></returns>
        public UniformParameter this[string Name]
        {
            get => UniformParameters[Name];
            // cannot set like this.
        }

        /// <summary>
        /// Sets vertex shader path. This means the shader can be changed at run time. requires program to recompile.
        /// </summary>
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
        /// <summary>
        /// Sets fragment shader path. This means the shader can be changed at run time. requires program to recompile.
        /// </summary>
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

        // the rendering fill modes
        public PolygonMode PolygonMode = PolygonMode.Fill;
        public MaterialFace MaterialFace = MaterialFace.FrontAndBack;
        
        
        public bool ready = false; // = "do the fields in this object match whats been compiled in openGL?"

        public ShaderProgram(string VertexPath, string FragmentPath)
        {
            vertpath = VertexPath;
            fragpath = FragmentPath;
        }
        
        /// <summary>
        /// adds a uniform parameter
        /// </summary>
        /// <param name="NewUniform"></param>
        public void AddUniform(UniformParameter NewUniform)
        {
            // if parameter exists with this name already exists throw exception
            if (UniformParameters.ContainsKey(NewUniform.name))
            {
                throw new Exception($"the name {NewUniform.name} is already taken on this shader program");
            }
             
            ready = false; // fields no longer match whats compiled 
            UniformParameters[NewUniform.name] = NewUniform;
        }
        
        /// <summary>
        /// adds the vertex parameter
        /// </summary>
        /// <param name="NewVertexParameter"></param>
        public void AddVertexParameter(IVertexParameter NewVertexParameter)
        {
            ready = false; // fields no longer match whats compiled 
            VertexParameters.Add(NewVertexParameter);
        }

        /// <summary>
        /// uses current program to render next object.
        /// </summary>
        public void UseProgram()
        {
            GL.PolygonMode(MaterialFace, PolygonMode); // use this programs rendering modes
            GL.UseProgram(ProgramHandle); // tell openGL to use this object
            foreach (string name in UniformParameters.Keys) UniformParameters[name].UpdateUniform(); // update the uniforms in the shader
        }
        
        /// <summary>
        /// Compiles together the shaders to make the program. if any fields are changed after being compiled(not including updating uniforms) the program will need to be recompiled to see the changes.
        /// </summary>
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

        /// <summary>
        /// Loads the shader in openGL
        /// </summary>
        /// <param name="shadertype">Fragment or Vertex.</param>
        /// <param name="path">filepath to the shader.</param>
        /// <returns>the shader handle in openGL</returns>
        private int Load_Shader(ShaderType shadertype, string path)
        {
            // create new shader object in OpenGL
            int NewShaderHandle = GL.CreateShader(shadertype);

            string code = Load_Code(shadertype, path);

            // attaches shader and code
            GL.ShaderSource(NewShaderHandle, code);

            // compiles shader code
            GL.CompileShader(NewShaderHandle);

            // checks if compilation worked
            string info = GL.GetShaderInfoLog(NewShaderHandle);
            if (!string.IsNullOrWhiteSpace(info)) throw new Exception($"Failed to compile {shadertype}{Environment.NewLine}{code}{Environment.NewLine}{info}");

            return NewShaderHandle;
        }

        /// <summary>
        /// Loads the code from the file path and adds the parameters.
        /// </summary>
        /// <param name="shadertype">Fragment or Vertex.</param>
        /// <param name="path">filepath to the shader.</param>
        /// <returns>the code as a string.</returns>
        private string Load_Code(ShaderType shadertype, string path)
        {
            // all GLSL starts with this
            string code = $"#version 450 core{Environment.NewLine}";

            // the order in which the varaibles are packed and unpacked from the parameter buffer
            int location = 0;

            if (shadertype == ShaderType.VertexShader)
            {
                // generate vertex defintions
                foreach (IVertexParameter P in VertexParameters) code += P.VertDefinition(ref location);
                // generate uniform definitions
                foreach (string name in UniformParameters.Keys) code += UniformParameters[name].VertDefinition(ref location);
            }
            else
            {
                // generate uniform definitions
                foreach (string name in UniformParameters.Keys) code += UniformParameters[name].FragDefinition(ref location);
                // only uniform parameters can go into the fragments shader
            }

            // read file and add text to code
            code += File.ReadAllText(path);
            return code;
        }
    }
}

