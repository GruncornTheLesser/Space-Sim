using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Graphics.Shaders
{
    /* There is probably a better way to do this
     * I tried for a while to get it into the same class
     * but although this is huge and gross it does what its supposed to relatively simply
     * 
     * The problems i was dealing with types at run time in c# is painful
     * it also needed to be iterable hence the abstract Iparameter class
     * 
     * I might try and trim it down becuase its might end up expensive in memory
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
    public enum ParameterType
    {
        Fragment,
        Vertex,
        Both
    }


    /// <summary>
    /// Iterable parameter abstract class.
    /// </summary>
    abstract unsafe class Iparameter
    {
        protected ParameterType _parametertype; // which shader this parameter belongs, fragment, vertex or both
        protected TypeQualifier _typequalifier; // ie uniform, in
        protected ValueType _valuetype; // float, vec2, mat3, Texture etc
        protected string _name; // the name in the script
        protected int location;


        /// <summary>
        /// Generate the definition for this parameter in the vertex shader script.
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        public virtual string GenVertDef(ref int Location)
        {

            if (_parametertype == ParameterType.Fragment) return "";

            string code;
            switch (_typequalifier)
            {
                case TypeQualifier.Vertex:
                    if (_valuetype == ValueType.Texture) throw new Exception("Cannot store texture on a vertex.");

                    location = Location;
                    code = $"layout(location = {location++}) in {_valuetype.ToString().ToLower()} {_name};\n";

                    if (_parametertype == ParameterType.Both) code += $"out {_valuetype.ToString().ToLower()} {_name}\n";

                    break;


                case TypeQualifier.Uniform:
                    if (_valuetype == ValueType.Texture)
                    {
                        code = $"uniform sampler2D {_name};\n";
                    }
                    else
                    {
                        Location = location;
                        code = $"layout(location = {location++}) uniform {_valuetype.ToString().ToLower()} {_name};\n";
                    }
                    break;
                default:
                    throw new Exception("unidentified type qualifier.");
            }
            return code;
        }
        /// <summary>
        /// Generate the definition for this parameter in the fragment shader script.
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        public virtual string GenFragDef(ref int Location)
        {
            if (_parametertype == ParameterType.Vertex) return "";

            string code;
            switch (_typequalifier)
            {
                case TypeQualifier.Vertex:
                    if (_valuetype == ValueType.Texture) throw new Exception("Cannot store texture on a vertex.");

                    code = $"in {_valuetype.ToString().ToLower()} {_name};\n";

                    break;

                case TypeQualifier.Uniform:
                    if (_valuetype == ValueType.Texture) code = $"uniform sampler2D {_name};\n";
                    else code = $"layout(location = {location++}) uniform {_valuetype.ToString().ToLower()} {_name};\n";

                    break;
                default:
                    throw new Exception("unidentified type qualifier.");
            }

            return code;
        }
        /// <summary>
        /// updates the parameter if its a uniform
        /// </summary>
        public abstract void UpdateUniform();
    }


    unsafe class FloatParameter : Iparameter
    {
        float* valuepointer;

        /// <summary>
        /// Generate new parameter.
        /// </summary>
        /// <param name="ParameterType">which shader this parameter belongs, fragment, vertex or both</param>
        /// <param name="TypeQualifier">const, uniform or in</param>
        /// <param name="ValueType">the type of this parameter's value</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this parameter's value.</param>
        public FloatParameter(ParameterType ParameterType, TypeQualifier TypeQualifier, ValueType ValueType, string Name, float* ValuePointer)
        {
            _parametertype = ParameterType;
            _typequalifier = TypeQualifier;
            _valuetype = ValueType;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in float uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            if (_typequalifier == TypeQualifier.Uniform) GL.Uniform1(location, *valuepointer); // doesnt need ref  for floats???
        }
    }
    unsafe class IntParameter : Iparameter
    {
        int* valuepointer;

        /// <summary>
        /// Generate new parameter.
        /// </summary>
        /// <param name="ParameterType">which shader this parameter belongs, fragment, vertex or both</param>
        /// <param name="TypeQualifier">const, uniform or in</param>
        /// <param name="ValueType">the type of this parameter's value</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this parameter's value.</param>
        public IntParameter(ParameterType ParameterType, TypeQualifier TypeQualifier, ValueType ValueType, string Name, int* ValuePointer)
        {
            _parametertype = ParameterType;
            _typequalifier = TypeQualifier;
            _valuetype = ValueType;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in int uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            if (_typequalifier == TypeQualifier.Uniform) GL.Uniform1(location, *valuepointer);
        }
    }
    unsafe class Vec2Parameter : Iparameter
    {
        Vector2* valuepointer;

        /// <summary>
        /// Generate new parameter.
        /// </summary>
        /// <param name="ParameterType">which shader this parameter belongs, fragment, vertex or both</param>
        /// <param name="TypeQualifier">const, uniform or in</param>
        /// <param name="ValueType">the type of this parameter's value</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this parameter's value.</param>
        public Vec2Parameter(ParameterType ParameterType, TypeQualifier TypeQualifier, ValueType ValueType, string Name, Vector2* ValuePointer)
        {
            _parametertype = ParameterType;
            _typequalifier = TypeQualifier;
            _valuetype = ValueType;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in vec2 uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            if (_typequalifier == TypeQualifier.Uniform) GL.Uniform2(location, ref *valuepointer);
        }
    }
    unsafe class Vec3Parameter : Iparameter
    {
        Vector3* valuepointer;

        /// <summary>
        /// Generate new parameter.
        /// </summary>
        /// <param name="ParameterType">which shader this parameter belongs, fragment, vertex or both</param>
        /// <param name="TypeQualifier">const, uniform or in</param>
        /// <param name="ValueType">the type of this parameter's value</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this parameter's value.</param>
        public Vec3Parameter(ParameterType ParameterType, TypeQualifier TypeQualifier, ValueType ValueType, string Name, Vector3* ValuePointer)
        {
            _parametertype = ParameterType;
            _typequalifier = TypeQualifier;
            _valuetype = ValueType;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in vec3 uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            if (_typequalifier == TypeQualifier.Uniform) GL.Uniform3(location, ref *valuepointer);
        }
    }
    unsafe class Vec4Parameter : Iparameter
    {
        Vector4* valuepointer;

        /// <summary>
        /// Generate new parameter.
        /// </summary>
        /// <param name="ParameterType">which shader this parameter belongs, fragment, vertex or both</param>
        /// <param name="TypeQualifier">const, uniform or in</param>
        /// <param name="ValueType">the type of this parameter's value</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this parameter's value.</param>
        public Vec4Parameter(ParameterType ParameterType, TypeQualifier TypeQualifier, ValueType ValueType, string Name, Vector4* ValuePointer)
        {
            _parametertype = ParameterType;
            _typequalifier = TypeQualifier;
            _valuetype = ValueType;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in vec4 uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            if (_typequalifier == TypeQualifier.Uniform) GL.Uniform4(location, ref *valuepointer);
        }
    }
    unsafe class Mat2Parameter : Iparameter
    {
        Matrix2* valuepointer;

        /// <summary>
        /// Generate new parameter.
        /// </summary>
        /// <param name="ParameterType">which shader this parameter belongs, fragment, vertex or both</param>
        /// <param name="TypeQualifier">const, uniform or in</param>
        /// <param name="ValueType">the type of this parameter's value</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this parameter's value.</param>
        public Mat2Parameter(ParameterType ParameterType, TypeQualifier TypeQualifier, ValueType ValueType, string Name, Matrix2* ValuePointer)
        {
            _parametertype = ParameterType;
            _typequalifier = TypeQualifier;
            _valuetype = ValueType;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in mat2 uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            if (_typequalifier == TypeQualifier.Uniform) GL.UniformMatrix2(location, true, ref *valuepointer);
        }
    }
    unsafe class Mat3Parameter : Iparameter
    {
        Matrix3* valuepointer;

        /// <summary>
        /// Generate new parameter.
        /// </summary>
        /// <param name="ParameterType">which shader this parameter belongs, fragment, vertex or both</param>
        /// <param name="TypeQualifier">const, uniform or in</param>
        /// <param name="ValueType">the type of this parameter's value</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this parameter's value.</param>
        public Mat3Parameter(ParameterType ParameterType, TypeQualifier TypeQualifier, ValueType ValueType, string Name, Matrix3* ValuePointer)
        {
            _parametertype = ParameterType;
            _typequalifier = TypeQualifier;
            _valuetype = ValueType;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in mat3 uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            if (_typequalifier == TypeQualifier.Uniform) GL.UniformMatrix3(location, true, ref *valuepointer);
        }
    }
    unsafe class Mat4Parameter : Iparameter
    {
        Matrix4* valuepointer;

        /// <summary>
        /// Generate new parameter.
        /// </summary>
        /// <param name="ParameterType">which shader this parameter belongs, fragment, vertex or both</param>
        /// <param name="TypeQualifier">const, uniform or in</param>
        /// <param name="ValueType">the type of this parameter's value</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this parameter's value.</param>
        public Mat4Parameter(ParameterType ParameterType, TypeQualifier TypeQualifier, ValueType ValueType, string Name, Matrix4* ValuePointer)
        {
            _parametertype = ParameterType;
            _typequalifier = TypeQualifier;
            _valuetype = ValueType;
            _name = Name;
            valuepointer = ValuePointer;
        }
        /// <summary>
        /// Pass in mat4 uniform from pointer
        /// </summary>
        public override void UpdateUniform()
        {
            if (_typequalifier == TypeQualifier.Uniform) GL.UniformMatrix4(location, true, ref *valuepointer);
        }
    }
    unsafe class TextureParameter : Iparameter
    {
        int valuepointer; // doesnt need to be a pointer as textures are stored in openGL which are stored as pointer

        /// <summary>
        /// Generate new parameter.
        /// </summary>
        /// <param name="ParameterType">which shader this parameter belongs, fragment, vertex or both</param>
        /// <param name="TypeQualifier">const, uniform or in</param>
        /// <param name="ValueType">the type of this parameter's value</param>
        /// <param name="Name">the name its given in the shader script</param>
        /// <param name="ValuePointer">a pointer to this parameter's value.</param>
        public TextureParameter(ParameterType ParameterType, TypeQualifier TypeQualifier, ValueType ValueType, string Name, int ValuePointer)
        {
            _parametertype = ParameterType;
            _typequalifier = TypeQualifier;
            _valuetype = ValueType;
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
            if (_typequalifier == TypeQualifier.Uniform) GL.BindTextures(location, 1, new int[1] { valuepointer });
        }
    }

}
