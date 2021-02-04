﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using DeepCopy;
namespace Shaders
{


    /* THING TO DO:
     * write summaries
     * 
     * 
     * friendship ended with generic types. polymorphism is my new best friend.
     */


    public enum ShaderTarget
    {
        Fragment,
        Vertex,
        Both
    }

    interface IVertexParameter
    {
        public abstract string VertDefinition(ref int Location);

    }
    
    /// <summary>
    /// A parameter thats different per vertice such as position.
    /// </summary>
    /// <typeparam name="T">The type of parameter being saved</typeparam>
    class VertexParameter<T> : IVertexParameter
    {
        public readonly string name; // the name in the script
        protected ShaderTarget shadertarget; // which shader this parameter belongs, fragment, vertex or both
        protected int location; // the order its written in the shader script

        public VertexParameter(ShaderTarget ShaderTarget, string Name)
        {
            shadertarget = ShaderTarget;
            name = Name;
        }

        /// <summary>
        /// Generate the definition for a vertex parameter in the vertex shader script. 
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        public string VertDefinition(ref int Location)
        {
            location = Location;
            string code = $"layout(location = {Location++}) in {TypeToGLSL<T>()} {name};{Environment.NewLine}";
            return code;
        }

        /// <summary>
        /// converts type to its GL shading language(GLSL) equivalent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>string syntax in GLSL of this type</returns>
        private static string TypeToGLSL<T1>()
        {
            switch (typeof(T1).ToString())
            {
                case "System.Single":
                    return "float";
                case "System.Int32":
                    return "int";
                case "OpenTK.Mathematics.Vector2":
                    return "vec2";
                case "OpenTK.Mathematics.Vector3":
                    return "vec3";
                case "OpenTK.Mathematics.Vector4":
                    return "vec4";
                case "OpenTK.Mathematics.Matrix2":
                    return "mat2";
                case "OpenTK.Mathematics.Matrix3":
                    return "mat3";
                case "OpenTK.Mathematics.Matrix4":
                    return "mat4";

                default:
                    throw new NotImplementedException($"This will probably come up alot. type: {typeof(T1)}");
            }
        }
    }
    
    /// <summary>
    /// A parameter to pass to shader thats equal for all vertices. uses a deep copy so the value updates here.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    abstract class UniformParameter
    {
        public readonly string name; // the name in the script

        protected ShaderTarget shadertarget; // which shader this parameter belongs, fragment, vertex or both
        protected int location; // the order its written in the shader script
        protected IDeepCopy parameter;

        public UniformParameter(ShaderTarget ShaderTarget, string Name)
        {
            shadertarget = ShaderTarget;
            name = Name;
        }

        /// <summary>
        /// converts type to its GL shading language(GLSL) equivalent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected static string TypeToGLSL<T>()
        {
            switch (typeof(T).ToString())
            {
                case "System.Single":
                    return "float";
                case "System.Int32":
                    return "int";
                case "OpenTK.Mathematics.Vector2":
                    return "vec2";
                case "OpenTK.Mathematics.Vector3":
                    return "vec3";
                case "OpenTK.Mathematics.Vector4":
                    return "vec4";
                case "OpenTK.Mathematics.Matrix2":
                    return "mat2";
                case "OpenTK.Mathematics.Matrix3":
                    return "mat3";
                case "OpenTK.Mathematics.Matrix4":
                    return "mat4";

                default:
                    throw new NotImplementedException($"type: {typeof(T)}");
            }
        }

        /// <summary>
        /// Generate the definition for a uniform parameter in the vertex shader script. 
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        protected string GenericVertDef<T>(ref int Location)
        {
            if (shadertarget == ShaderTarget.Fragment) return "";
            // else if vertex or both
            location = Location;
            return $"layout(location = {Location++}) uniform {TypeToGLSL<T>()} {name};{Environment.NewLine}";
        }

        /// <summary>
        /// Generate the definition for a uniform parameter in the vertex shader script. 
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        protected string GenericFragDef<T>(ref int Location)
        {
            if (shadertarget == ShaderTarget.Vertex) return "";
            // both needs to read the location made in the vertex definition
            if (shadertarget == ShaderTarget.Both) return $"layout(location = {this.location}) uniform {TypeToGLSL<T>()} {name};{Environment.NewLine}";
            
            // else if only fragment 
            this.location = Location;
            return $"layout(location = {Location++}) uniform {TypeToGLSL<T>()} {name};{Environment.NewLine}";
        }

        /// <summary>
        /// Updates the value of the uniform parameter.
        /// </summary>
        public abstract void UpdateUniform();
        public abstract string VertDefinition(ref int Location);
        public abstract string FragDefinition(ref int Location);
        public abstract void SetUniform(IDeepCopy DeepCopy);
    }

    class FloatUniform : UniformParameter
    {
        new DeepCopy<float> parameter = new DeepCopy<float>(); // default local value inside Deepcopy
        /// <summary>
        /// define declared parameter
        /// </summary>
        /// <param name="ShaderTarget">the shader script this parameter is used in</param>
        /// <param name="Name">the name of this parameter</param>
        /// <param name="Parameter">the deepcopy of this parameter</param>
        public FloatUniform(ShaderTarget ShaderTarget, string Name, DeepCopy<float> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        /// <summary>
        /// define undeclared parameter.
        /// </summary>
        /// <param name="ShaderTarget">the shader script this parameter is used in</param>
        /// <param name="Name">the name of this parameter</param>
        public FloatUniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public override void UpdateUniform() => GL.Uniform1(location, parameter.Value);
        public override string VertDefinition(ref int Location) => GenericVertDef<float>(ref Location);
        public override string FragDefinition(ref int Location) => GenericFragDef<float>(ref Location);
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<float>)DeepCopy;

    }
    class IntUniform : UniformParameter
    {
        new DeepCopy<int> parameter = new DeepCopy<int>();
        public IntUniform(ShaderTarget ShaderTarget, string Name, DeepCopy<int> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public IntUniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public override void UpdateUniform() => GL.Uniform1(location, parameter.Value);
        public override string VertDefinition(ref int Location) => GenericVertDef<int>(ref Location);
        public override string FragDefinition(ref int Location) => GenericFragDef<int>(ref Location);
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<int>)DeepCopy;
    }
    class Vec2Uniform : UniformParameter
    {
        new DeepCopy<Vector2> parameter = new DeepCopy<Vector2>();
        public Vec2Uniform(ShaderTarget ShaderTarget, string Name, DeepCopy<Vector2> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public Vec2Uniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public override void UpdateUniform() => GL.Uniform2(location, parameter.Value);
        public override string VertDefinition(ref int Location) => GenericVertDef<Vector2>(ref Location);
        public override string FragDefinition(ref int Location) => GenericFragDef<Vector2>(ref Location);
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<Vector2>)DeepCopy;
    }
    class Vec3Uniform : UniformParameter 
    {
        new DeepCopy<Vector3> parameter = new DeepCopy<Vector3>();
        public Vec3Uniform(ShaderTarget ShaderTarget, string Name, DeepCopy<Vector3> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public Vec3Uniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public override void UpdateUniform() => GL.Uniform3(location, parameter.Value);
        public override string VertDefinition(ref int Location) => GenericVertDef<Vector3>(ref Location);
        public override string FragDefinition(ref int Location) => GenericFragDef<Vector3>(ref Location);
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<Vector3>)DeepCopy;
    }
    class Vec4Uniform : UniformParameter 
    {
        new DeepCopy<Vector4> parameter = new DeepCopy<Vector4>();
        public Vec4Uniform(ShaderTarget ShaderTarget, string Name, DeepCopy<Vector4> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public Vec4Uniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public override void UpdateUniform() => GL.Uniform4(location, parameter.Value);
        public override string VertDefinition(ref int Location) => GenericVertDef<Vector4>(ref Location);
        public override string FragDefinition(ref int Location) => GenericFragDef<Vector4>(ref Location);
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<Vector4>)DeepCopy;
    }
    class Mat2Uniform : UniformParameter 
    {
        new DeepCopy<Matrix2> parameter = new DeepCopy<Matrix2>();
        public Mat2Uniform(ShaderTarget ShaderTarget, string Name, DeepCopy<Matrix2> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public Mat2Uniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public override void UpdateUniform()
        {
            Matrix2 M = parameter.Value;
            GL.UniformMatrix2(location, true, ref M); // cant ref a property
            //parameter.Value = M; // update parameter
        }
        public override string VertDefinition(ref int Location) => GenericVertDef<Matrix2>(ref Location);
        public override string FragDefinition(ref int Location) => GenericFragDef<Matrix2>(ref Location);
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<Matrix2>)DeepCopy;
    }
    class Mat3Uniform : UniformParameter 
    {
        new DeepCopy<Matrix3> parameter = new DeepCopy<Matrix3>();
        public Mat3Uniform(ShaderTarget ShaderTarget, string Name, DeepCopy<Matrix3> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public Mat3Uniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public override void UpdateUniform()
        {
            Matrix3 M = parameter.Value;
            GL.UniformMatrix3(location, true, ref M); // cant ref a property
            //parameter.Value = M; // update parameter
        }
        public override string VertDefinition(ref int Location) => GenericVertDef<Matrix3>(ref Location);
        public override string FragDefinition(ref int Location) => GenericFragDef<Matrix3>(ref Location);
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<Matrix3>)DeepCopy;

    }
    class Mat4Uniform : UniformParameter 
    {
        new DeepCopy<Matrix4> parameter = new DeepCopy<Matrix4>();
        public Mat4Uniform(ShaderTarget ShaderTarget, string Name, DeepCopy<Matrix4> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public Mat4Uniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public override void UpdateUniform()
        {
            Matrix4 M = parameter.Value;
            GL.UniformMatrix4(location, true, ref M); // cant ref a property
            //parameter.Value = M; // update parameter
        }
        public override string VertDefinition(ref int Location) => GenericVertDef<Matrix4>(ref Location);
        public override string FragDefinition(ref int Location) => GenericFragDef<Matrix4>(ref Location);
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<Matrix4>)DeepCopy;
    }
    class TextureUniform : UniformParameter 
    {
        new int parameter;
        public TextureUniform(ShaderTarget ShaderTarget, string Name, int TextureHandle) : base(ShaderTarget, Name) => parameter = TextureHandle;
        public override void UpdateUniform() => GL.BindTextures(0, 1, new int[1] { parameter });
        public override string VertDefinition(ref int Location)
        {
            if (shadertarget == ShaderTarget.Fragment) return "";
            return $"uniform sampler2D {name};{Environment.NewLine}";
        }
        public override string FragDefinition(ref int Location)
        {
            if (shadertarget == ShaderTarget.Vertex) return "";
            return $"uniform sampler2D {name};{Environment.NewLine}";
        }
        public override void SetUniform(IDeepCopy DeepCopy) => throw new Exception("Currently cant set texture this way. try ");
    }
}
