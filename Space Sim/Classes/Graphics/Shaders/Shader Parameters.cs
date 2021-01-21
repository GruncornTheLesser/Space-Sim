using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Shaders
{
    /* There is probably a better way to do this
     * I tried for a while to get it into the same class
     * but although this is huge and gross it does what its supposed to relatively simply
     * 
     * The problems i was dealing with types at run time in c# is painful
     * it also needed to be iterable hence the abstract IUniform class
     * 
     * This heavily uses pointers. the idea was you pass in the pointer from where ever and then updating uniforms becomes really easy
     */



    /* THING TO DO:
     * 
     */
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
    public enum ShaderTarget
    {
        Fragment,
        Vertex,
        Both
    }


    /// <summary>
    /// Iterable Uniform abstract class.
    /// </summary>
    abstract unsafe class IParameter
    {
        protected ShaderTarget _shadertarget; // which shader this Uniform belongs, fragment, vertex or both
        protected ValueType _valuetype; // float, vec2, mat3, Texture etc
        protected string _name; // the name in the script
        protected int _location;

        /// <summary>
        /// Generate the definition for a uniform parameter in the vertex shader script. 
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        public virtual string GenVertDef(ref int Location)
        {

            if (_shadertarget == ShaderTarget.Fragment) return "";

            string code;
            
            // textures are passed in differently to other values
            if (_valuetype == ValueType.Texture)
            {
                code = $"uniform sampler2D {_name};{Environment.NewLine}";
            }
            else
            {
                _location = Location;
                code = $"layout(location = {Location++}) uniform {_valuetype.ToString().ToLower()} {_name};{Environment.NewLine}";
            }

            return code;
        }
        /// <summary>
        /// Generate the definition for a uniform parameter in the vertex shader script. 
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        public virtual string GenFragDef(ref int Location)
        {
            if (_shadertarget == ShaderTarget.Vertex) return "";

            string code;

            if (_valuetype == ValueType.Texture)
            {
                code = $"uniform sampler2D {_name};{Environment.NewLine}";
            }
            else
            {
                _location = Location;
                code = $"layout(location = {Location++}) uniform {_valuetype.ToString().ToLower()} {_name};{Environment.NewLine}";
            }

            return code;
        }
        /// <summary>
        /// updates the Uniform if its a uniform
        /// </summary>
        public abstract void UpdateUniform();
    }


    unsafe class FloatUniform : IParameter
    {
        float* valuepointer;

        /// <summary>
        /// Generate new Uniform.
        /// </summary>
        /// <param name="ShaderTarget">which shader this Uniform belongs, fragment, vertex or both</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this Uniform's value.</param>
        public FloatUniform(ShaderTarget ShaderTarget, string Name, float* ValuePointer)
        {
            _shadertarget = ShaderTarget;
            _valuetype = ValueType.Float;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in float uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            GL.Uniform1(_location, *valuepointer); // doesnt need ref  for floats???
        }
    }
    unsafe class IntUniform : IParameter
    {
        int* valuepointer;

        /// <summary>
        /// Generate new Uniform.
        /// </summary>
        /// <param name="ShaderTarget">which shader this Uniform belongs, fragment, vertex or both</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this Uniform's value.</param>
        public IntUniform(ShaderTarget ShaderTarget, string Name, int* ValuePointer)
        {
            _shadertarget = ShaderTarget;
            _valuetype = ValueType.Int;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in int uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            GL.Uniform1(_location, *valuepointer);
        }
    }
    unsafe class Vec2Uniform : IParameter
    {
        Vector2* valuepointer;

        /// <summary>
        /// Generate new Uniform.
        /// </summary>
        /// <param name="ShaderTarget">which shader this Uniform belongs, fragment, vertex or both</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this Uniform's value.</param>
        public Vec2Uniform(ShaderTarget ShaderTarget, string Name, Vector2* ValuePointer)
        {
            _shadertarget = ShaderTarget;
            _valuetype = ValueType.Vec2;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in vec2 uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            GL.Uniform2(_location, ref *valuepointer);
        }
    }
    unsafe class Vec3Uniform : IParameter
    {
        Vector3* valuepointer;

        /// <summary>
        /// Generate new Uniform.
        /// </summary>
        /// <param name="ShaderTarget">which shader this Uniform belongs, fragment, vertex or both</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this Uniform's value.</param>
        public Vec3Uniform(ShaderTarget ShaderTarget, string Name, Vector3* ValuePointer)
        {
            _shadertarget = ShaderTarget;
            _valuetype = ValueType.Vec3;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in vec3 uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            GL.Uniform3(_location, ref *valuepointer);
        }
    }
    unsafe class Vec4Uniform : IParameter
    {
        Vector4* valuepointer;

        /// <summary>
        /// Generate new Uniform.
        /// </summary>
        /// <param name="ShaderTarget">which shader this Uniform belongs, fragment, vertex or both</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this Uniform's value.</param>
        public Vec4Uniform(ShaderTarget ShaderTarget, string Name, Vector4* ValuePointer)
        {
            _shadertarget = ShaderTarget;
            _valuetype = ValueType.Vec4;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in vec4 uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            GL.Uniform4(_location, ref *valuepointer);
        }
    }
    unsafe class Mat2Uniform : IParameter
    {
        Matrix2* valuepointer;

        /// <summary>
        /// Generate new Uniform.
        /// </summary>
        /// <param name="ShaderTarget">which shader this Uniform belongs, fragment, vertex or both</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this Uniform's value.</param>
        public Mat2Uniform(ShaderTarget ShaderTarget, string Name, Matrix2* ValuePointer)
        {
            _shadertarget = ShaderTarget;
            _valuetype = ValueType.Mat2;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in mat2 uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            GL.UniformMatrix2(_location, true, ref *valuepointer);
        }
    }
    unsafe class Mat3Uniform : IParameter
    {
        Matrix3* valuepointer;

        /// <summary>
        /// Generate new Uniform.
        /// </summary>
        /// <param name="ShaderTarget">which shader this Uniform belongs, fragment, vertex or both</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this Uniform's value.</param>
        public Mat3Uniform(ShaderTarget ShaderTarget, string Name, Matrix3* ValuePointer)
        {
            _shadertarget = ShaderTarget;
            _valuetype = ValueType.Mat3;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in mat3 uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            GL.UniformMatrix3(_location, true, ref *valuepointer);
        }
    }
    unsafe class Mat4Uniform : IParameter
    {
        Matrix4* valuepointer;

        /// <summary>
        /// Generate new Uniform.
        /// </summary>
        /// <param name="ShaderTarget">which shader this Uniform belongs, fragment, vertex or both</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this Uniform's value.</param>
        public Mat4Uniform(ShaderTarget ShaderTarget, string Name, Matrix4* ValuePointer)
        {
            _shadertarget = ShaderTarget;
            _valuetype = ValueType.Mat4;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in mat4 uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            GL.UniformMatrix4(_location, true, ref *valuepointer);
        }
    }
    unsafe class TextureUniform : IParameter
    {
        int valuepointer; // doesnt need to be a pointer as textures are stored in openGL which are stored as pointer

        /// <summary>
        /// Generate new Uniform.
        /// </summary>
        /// <param name="ShaderTarget">which shader this Uniform belongs, fragment, vertex or both</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this Uniform's value.</param>
        public TextureUniform(ShaderTarget ShaderTarget, string Name, int ValuePointer)
        {
            _shadertarget = ShaderTarget;
            _valuetype = ValueType.Texture;
            _name = Name;
            valuepointer = ValuePointer;
        }

        /// <summary>
        /// pass in texture pointer
        /// </summary>
        public override void UpdateUniform()
        {
            /* https://opentk.net/learn/chapter1/6-multiple-textures.html
             * Sampler2D variable is uniform but isn't assigned the same way
             * this is assigned a location value with a texture sampler 
             * known as a texture unit.
             * 
             * allows use of more than 1 texture to be passed.
             * 
             * This is then bound with BindTextures() to the current active 
             * texture unit.
             * 
             * idk its weird openGL stuff. 
             */

            GL.BindTextures(_location, 1, new int[1] { valuepointer });
        }
    }



    unsafe class VertexParameter : IParameter
    {
        public VertexParameter(ShaderTarget ShaderTarget, ValueType ValueType, string Name)
        {
            _shadertarget = ShaderTarget;
            _valuetype = ValueType;
            _name = Name;
        }

        /// <summary>
        /// Generate the definition for a vertex parameter in the vertex shader script. 
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        public override string GenFragDef(ref int Location)
        {
            _location = Location;
            string code = $"in {_valuetype.ToString().ToLower()} {_name.Replace("Vert", "Frag")};{Environment.NewLine}";
            if (_shadertarget == ShaderTarget.Fragment) code += $"out {_valuetype.ToString().ToLower()} {_name};{Environment.NewLine}";
            return code;
        }
        /// <summary>
        /// Generate the definition for a vertex parameter in the vertex shader script. 
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        public override string GenVertDef(ref int Location)
        {
            return $"layout(location = {Location++})  in {_valuetype.ToString().ToLower()} {_name};{Environment.NewLine}";
        }
        public override void UpdateUniform() { }

    }
}
