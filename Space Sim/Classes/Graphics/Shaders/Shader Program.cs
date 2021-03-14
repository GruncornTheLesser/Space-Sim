using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace Graphics.Shaders
{
    /// <summary>
    /// manages object's shader.
    /// </summary>
    class ShaderProgram
    {
        // the handle in openGl for this program
        private int ProgramHandle;

        #region Collections For Parameters
        private Dictionary<string, UniformParameter> UniformParameters = new Dictionary<string, UniformParameter>(); // only uniform parameters need to be accessible
        private Dictionary<string, TextureUniform> UniformTextures = new Dictionary<string, TextureUniform>();
        private List<IVertexParameter> VertexParameters = new List<IVertexParameter>();
        #endregion

        /// <summary>
        /// used to set uniform after program has been compiled.
        /// </summary>
        /// <param name="Name">The name of the parameter.</param>
        /// <returns></returns>
        public UniformParameter this[string Name]
        {
            get
            {
                if (UniformParameters.ContainsKey(Name)) return UniformParameters[Name];
                else if (UniformTextures.ContainsKey(Name)) return UniformTextures[Name];
                else throw new Exception($"{Name} uniform not found");
            }
            set
            {
                if (UniformParameters.ContainsKey(Name))
                {
                    RemoveUniform(Name);
                    AddUniform(value);
                }
            }
        }

        #region file paths
        // file paths

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
        private string vertpath;
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
        private string fragpath;
        #endregion

        public bool ready = false; // = "do the fields in this object match whats been compiled in openGL?"
        public Action UpdateUniforms = () => { };

        public ShaderProgram(string VertexPath, string FragmentPath)
        {
            vertpath = VertexPath;
            fragpath = FragmentPath;
        }

        #region Adding and Removing Parameters
        /// <summary>
        /// adds a uniform parameter
        /// </summary>
        /// <param name="NewUniform"></param>
        public void AddUniform(UniformParameter NewUniform)
        {
            // if parameter exists with this name already exists throw exception
            if (UniformParameters.ContainsKey(NewUniform.name)) throw new Exception($"the name {NewUniform.name} is already taken on this shader program");
            UpdateUniforms += NewUniform.OnUpdateUniform;

            if (NewUniform.GetType() == typeof(TextureUniform))
            {
                // add to textures 
                UniformTextures[NewUniform.name] = (TextureUniform)NewUniform;
            }
            else
            {
                // add to parameters
                UniformParameters[NewUniform.name] = NewUniform;
            }

            ready = false; // fields no longer match whats compiled 
        }
        
        /// <summary>
        /// removes a uniform parameter
        /// </summary>
        /// <param name="NewUniform"></param>
        public void RemoveUniform(string Name)
        {
            ready = false;
            UpdateUniforms -= UniformParameters[Name].OnUpdateUniform;
            UniformParameters.Remove(Name);
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
        #endregion

        #region Compilation and loading of shaders
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
            if (!string.IsNullOrWhiteSpace(info)) 
                throw new Exception($"Failed to link shaders to program: {info}" +
                $"{Environment.NewLine}+--------------------------------+{Environment.NewLine}" +
                $"{Load_Code(ShaderType.VertexShader, @"Shaders\" + vertpath + "Vert.shader")}" +
                $"{Environment.NewLine}+--------------------------------+{Environment.NewLine}" +
                $"{Load_Code(ShaderType.FragmentShader, @"Shaders\" + fragpath + "Frag.shader")}" +
                $"{Environment.NewLine}+--------------------------------+{Environment.NewLine}");

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
            if (!string.IsNullOrWhiteSpace(info)) throw new Exception($"Failed to compile {path}{Environment.NewLine}{code}{Environment.NewLine}{info}");

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
                foreach (string name in UniformTextures.Keys) code += UniformTextures[name].VertDefinition(ref location);
                foreach (string name in UniformParameters.Keys) code += UniformParameters[name].VertDefinition(ref location);
            }
            else
            {
                // generate uniform definitions
                foreach (string name in UniformTextures.Keys) code += UniformTextures[name].FragDefinition(ref location);
                foreach (string name in UniformParameters.Keys) code += UniformParameters[name].FragDefinition(ref location);
                
                // only uniform parameters can go into the fragments shader
            }

            // read file and add text to code
            code += File.ReadAllText(path);
            return code;
        }
        #endregion

        /// <summary>
        /// uses this program to render next object.
        /// </summary>
        public void UseProgram()
        {
            TextureManager.TexturesLoaded = 0; // each time a shader program is used the texture units are forgotten ie allows overwriting of textures
            GL.UseProgram(ProgramHandle); // tell openGL to use this object
            UpdateUniforms(); // update the uniforms in the shaders
        }
    }
}

