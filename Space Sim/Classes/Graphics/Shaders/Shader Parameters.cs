using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

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
     * 
     * pointer are making things difficult. C# moves memory around alot so requires you to fix all the variables that need pointers. this makes sense. its just frustrating.
     * 
     * an alternative idea is using delegates???
     * 
     * using delegates is much better :)
     * 
     * 
     * 
     * The reason I have used a bunch of inheritance rather than a dictionary of functions(or something else) is it needs to know the type to use in the openGL functions.
     
     */

    public enum ShaderTarget
    {
        Fragment,
        Vertex,
        Both
    }
    /// <summary>
    /// A deep copy updates the original value and vice versa. This is opposed to a shallow copy that doesnt update the original.
    /// </summary>
    /// <typeparam name="T">The type of the value being copied.</typeparam>
    class DeepCopy<T>
    {
        // 2 delegate values
        private readonly Func<T> GET;
        private readonly Action<T> SET;

        // create a property from the delegate functions
        public T Value
        {
            get { return GET(); }
            set { SET(value); }
        }

        // pass in a get and set value
        public DeepCopy(Func<T> getter, Action<T> setter)
        {
            GET = getter;
            SET = setter;
        }
    }

    /// <summary>
    /// Abstract iterable Uniform or Vertex parameter.
    /// </summary>
    abstract class Iparameter
    {
        protected ShaderTarget shadertarget; // which shader this parameter belongs, fragment, vertex or both
        protected string name; // the name in the script
        protected int location; // the order its written in the shader script
        
        
        
        /// <summary>
        /// Generate the definition for a vertex parameter in the vertex shader script. 
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        public abstract string GenVertDef(ref int location);
        
        /// <summary>
        /// Generate the definition for a vertex parameter in the fragment shader script. 
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        public abstract string GenFragDef(ref int location);



        /// <summary>
        /// if parameter Uniform updates the uniform else does nothing.
        /// </summary>
        public abstract void UpdateUniform();

        /// <summary>
        /// converts type to its GLSL equivalent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected string TypeToGLSL<T>()
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
                    throw new NotImplementedException($"This will probably come up alot. type: {typeof(T)}");
            }
        }
    }
    class VertexParameter<T> : Iparameter
    {
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
        public override string GenVertDef(ref int Location)
        {
            location = Location;
            string code = $"layout(location = {Location++}) in {TypeToGLSL<T>()} {name};{Environment.NewLine}";
            return code;
        }
        /// <summary>
        /// Generate nothing. 
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        public override string GenFragDef(ref int Location) => "";
        public override void UpdateUniform() { }
    }
    class UniformParameter<T> : Iparameter
    {
        protected DeepCopy<T> parameter;

        public UniformParameter(ShaderTarget ShaderTarget, string Name, Func<T> Get, Action<T> Set)
        {
            shadertarget = ShaderTarget;
            name = Name;
            parameter = new DeepCopy<T>(Get, Set);
        }

        /// <summary>
        /// Generate the definition for a uniform parameter in the vertex shader script. 
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        public override string GenVertDef(ref int Location)
        {
            if (shadertarget == ShaderTarget.Fragment) return "";
            location = Location;
            return $"layout(location = {Location++}) uniform {TypeToGLSL<T>()} {name};{Environment.NewLine}";
        }
        /// <summary>
        /// Generate the definition for a uniform parameter in the vertex shader script. 
        /// </summary>
        /// <param name="Location">The index in which its saved in memory.</param>
        /// <returns></returns>
        public override string GenFragDef(ref int Location)
        {
            if (shadertarget == ShaderTarget.Vertex) return "";
            if (shadertarget == ShaderTarget.Both) return $"layout(location = {this.location}) uniform {TypeToGLSL<T>()} {name};{Environment.NewLine}";
            
            this.location = Location;
            return $"layout(location = {Location++}) uniform {TypeToGLSL<T>()} {name};{Environment.NewLine}";
        }
        /// <summary>
        /// Updates the value of the uniform parameter.
        /// </summary>
        public override void UpdateUniform()
        {
            // if not implimented in inherited classes a not implemented error will be thrown
            throw new NotImplementedException();
        }

    }

    class FloatUniform : UniformParameter<float>
    {
        public FloatUniform(ShaderTarget ShaderTarget, string Name, Func<float> Get, Action<float> Set) : base(ShaderTarget, Name, Get, Set) { }
        public override void UpdateUniform() => GL.Uniform1(location, parameter.Value);
    }
    class IntUniform : UniformParameter<int> 
    {
        public IntUniform(ShaderTarget ShaderTarget, string Name, Func<int> Get, Action<int> Set) : base(ShaderTarget, Name, Get, Set) { }
        public override void UpdateUniform() => GL.Uniform1(location, parameter.Value);
    }
    class Vec2Uniform : UniformParameter<Vector2> 
    {
        public Vec2Uniform(ShaderTarget ShaderTarget, string Name, Func<Vector2> Get, Action<Vector2> Set) : base(ShaderTarget, Name, Get, Set) { }
        public override void UpdateUniform() => GL.Uniform2(location, parameter.Value);

    }
    class Vec3Uniform : UniformParameter<Vector3> 
    {
        public Vec3Uniform(ShaderTarget ShaderTarget, string Name, Func<Vector3> Get, Action<Vector3> Set) : base(ShaderTarget, Name, Get, Set) { }
        public override void UpdateUniform() => GL.Uniform3(location, parameter.Value);
    }
    class Vec4Uniform : UniformParameter<Vector4> 
    {
        public Vec4Uniform(ShaderTarget ShaderTarget, string Name, Func<Vector4> Get, Action<Vector4> Set) : base(ShaderTarget, Name, Get, Set) { }
        public override void UpdateUniform() => GL.Uniform4(location, parameter.Value);
    }
    class Mat2Uniform : UniformParameter<Matrix2> 
    {
        public Mat2Uniform(ShaderTarget ShaderTarget, string Name, Func<Matrix2> Get, Action<Matrix2> Set) : base(ShaderTarget, Name, Get, Set) { }
        public override void UpdateUniform()
        {
            Matrix2 M = parameter.Value;
            GL.UniformMatrix2(location, true, ref M); // cant ref a property
            //parameter.Value = M; // update parameter
        }
    }
    class Mat3Uniform : UniformParameter<Matrix3> 
    {
        public Mat3Uniform(ShaderTarget ShaderTarget, string Name, Func<Matrix3> Get, Action<Matrix3> Set) : base(ShaderTarget, Name, Get, Set) { }
        public override void UpdateUniform()
        {
            Matrix3 M = parameter.Value;
            GL.UniformMatrix3(location, true, ref M); // cant ref a property
            //parameter.Value = M; // update parameter
        }
    }
    class Mat4Uniform : UniformParameter<Matrix4> 
    {
        public Mat4Uniform(ShaderTarget ShaderTarget, string Name, Func<Matrix4> Get, Action<Matrix4> Set) : base(ShaderTarget, Name, Get, Set) { }
        public override void UpdateUniform()
        {
            Matrix4 M = parameter.Value;
            GL.UniformMatrix4(location, true, ref M); // cant ref a property
            //parameter.Value = M; // update parameter
        }
    }
    class TextureUniform : UniformParameter<int> 
    {
        public TextureUniform(ShaderTarget ShaderTarget, string Name, int TextureHandle) : base(ShaderTarget, Name, ()=> TextureHandle, value => { TextureHandle = value; }) { }
        public override void UpdateUniform() => GL.BindTextures(0, 1, new int[1] { parameter.Value });
        public override string GenVertDef(ref int Location)
        {
            if (shadertarget == ShaderTarget.Fragment) return "";
            return $"uniform sampler2D {name};{Environment.NewLine}";
        }
        public override string GenFragDef(ref int Location)
        {
            if (shadertarget == ShaderTarget.Vertex) return "";
            return $"uniform sampler2D {name};{Environment.NewLine}";
        }
    }
}
