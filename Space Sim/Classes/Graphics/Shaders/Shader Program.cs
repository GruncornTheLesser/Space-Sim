using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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


    class ShaderProgram<Vertex> : Collection<Iparameter>
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

        public PolygonMode PolygonMode = PolygonMode.Fill;
        public MaterialFace MaterialFace = MaterialFace.Front;



        public bool ready = false;

        public ShaderProgram(string VertexPath, string FragmentPath) 
        {
            vertpath = VertexPath;
            fragpath = FragmentPath;
        }
        public void UseProgram() 
        {
            #if ready
            #warning Program is not ready to be used. Program shader path has been changed or parameter has been added. Program must be compiled to see changes.
            #endif

            
            GL.PolygonMode(MaterialFace, PolygonMode);
            GL.UseProgram(ProgramHandle);
            foreach (Iparameter P in this) P.UpdateUniform();

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
            string code = "#version 450 core\n";
            
            // writes initial variable definition at start of script
            int location = 0;
            if (shadertype == ShaderType.VertexShader)
            {
                foreach (Iparameter P in this) P.GenVertDef(ref location);
            }
            else
            {
                foreach (Iparameter P in this) P.GenFragDef(ref location);
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
    
    }

   
}

