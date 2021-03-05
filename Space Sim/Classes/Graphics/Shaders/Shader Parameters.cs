using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace Graphics.Shaders
{
    /// <summary>
    /// which shader is this parameter going to
    /// </summary>
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
    /// A parameter thats different per vertice eg vertex position.
    /// </summary>
    /// <typeparam name="T">The type of parameter being saved.</typeparam>
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


    #region Uniform Parameters
    abstract class UniformParameter
    {
        public readonly string name; // the name in the script

        protected ShaderTarget shadertarget; // which shader this parameter belongs, fragment, vertex or both
        protected int location; // the order its written in the shader script

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
                case "System.Boolean":
                    return "bool";
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
        public abstract void OnUpdateUniform();
        public abstract string VertDefinition(ref int Location);
        public abstract string FragDefinition(ref int Location);
        public abstract void SetUniform(IDeepCopy DeepCopy);
    }
    class FloatUniform : UniformParameter
    {
        DeepCopy<float> parameter = new DeepCopy<float>(); // default local value inside Deepcopy
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
        /// <summary>
        /// defines declared parameter with get func
        /// </summary>
        /// <param name="ShaderTarget">the shader script this parameter is used in</param>
        /// <param name="Name">the name of this parameter</param>
        /// <param name="get_parameter">a function that returns the parameter</param>
        public FloatUniform(ShaderTarget ShaderTarget, string Name, Func<float> get_parameter) : base(ShaderTarget, Name) => parameter = new DeepCopy<float>(get_parameter);
        /// <summary>
        /// updates the value in openGl
        /// </summary>
        public override void OnUpdateUniform() => GL.Uniform1(location, (float)parameter.Value);
        /// <summary>
        /// Generates vertex definition for this object
        /// </summary>
        /// <param name="Location">the location in memory for openGl</param>
        /// <returns>the parameter definition in GLSL.</returns>
        public override string VertDefinition(ref int Location) => GenericVertDef<float>(ref Location);
        /// <summary>
        /// Generates fragment definition for this object
        /// </summary>
        /// <param name="Location">the location in memory for openGl</param>
        /// <returns>the parameter definition in GLSL.</returns>
        public override string FragDefinition(ref int Location) => GenericFragDef<float>(ref Location);
        /// <summary>
        /// Set the value which this Uniform is using.
        /// </summary>
        /// <param name="DeepCopy"></param>
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<float>)DeepCopy;

    }
    class BoolUniform : UniformParameter
    {
        DeepCopy<bool> parameter = new DeepCopy<bool>(); // default local value inside Deepcopy
        public BoolUniform(ShaderTarget ShaderTarget, string Name, DeepCopy<bool> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public BoolUniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public BoolUniform(ShaderTarget ShaderTarget, string Name, Func<bool> get_parameter) : base(ShaderTarget, Name) => parameter = new DeepCopy<bool>(get_parameter);
        public override void OnUpdateUniform() => GL.Uniform1(location, parameter.Value ? 1 : 0);
        public override string VertDefinition(ref int Location) => GenericVertDef<bool>(ref Location);
        public override string FragDefinition(ref int Location) => GenericFragDef<bool>(ref Location);
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<bool>)DeepCopy;

    }
    class IntUniform : UniformParameter
    {
        DeepCopy<int> parameter = new DeepCopy<int>();

        public IntUniform(ShaderTarget ShaderTarget, string Name, DeepCopy<int> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public IntUniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public IntUniform(ShaderTarget ShaderTarget, string Name, Func<int> get_parameter) : base(ShaderTarget, Name) => parameter = new DeepCopy<int>(get_parameter);
        public override void OnUpdateUniform() => GL.Uniform1(location, parameter.Value);
        public override string VertDefinition(ref int Location) => GenericVertDef<int>(ref Location);
        public override string FragDefinition(ref int Location) => GenericFragDef<int>(ref Location);
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<int>)DeepCopy;
    }
    class Vec2Uniform : UniformParameter
    {
        DeepCopy<Vector2> parameter = new DeepCopy<Vector2>();
        public Vec2Uniform(ShaderTarget ShaderTarget, string Name, DeepCopy<Vector2> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public Vec2Uniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public Vec2Uniform(ShaderTarget ShaderTarget, string Name, Func<Vector2> get_parameter) : base(ShaderTarget, Name) => parameter = new DeepCopy<Vector2>(get_parameter);
        public override void OnUpdateUniform() => GL.Uniform2(location, parameter.Value);
        public override string VertDefinition(ref int Location) => GenericVertDef<Vector2>(ref Location);
        public override string FragDefinition(ref int Location) => GenericFragDef<Vector2>(ref Location);
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<Vector2>)DeepCopy;
    }
    class Vec3Uniform : UniformParameter 
    {
        DeepCopy<Vector3> parameter = new DeepCopy<Vector3>();
        public Vec3Uniform(ShaderTarget ShaderTarget, string Name, DeepCopy<Vector3> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public Vec3Uniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public Vec3Uniform(ShaderTarget ShaderTarget, string Name, Func<Vector3> get_parameter) : base(ShaderTarget, Name) => parameter = new DeepCopy<Vector3>(get_parameter);
        public override void OnUpdateUniform() => GL.Uniform3(location, parameter.Value);
        public override string VertDefinition(ref int Location) => GenericVertDef<Vector3>(ref Location);
        public override string FragDefinition(ref int Location) => GenericFragDef<Vector3>(ref Location);
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<Vector3>)DeepCopy;
    }
    class Vec4Uniform : UniformParameter 
    {
        DeepCopy<Vector4> parameter = new DeepCopy<Vector4>();
        public Vec4Uniform(ShaderTarget ShaderTarget, string Name, DeepCopy<Vector4> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public Vec4Uniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public Vec4Uniform(ShaderTarget ShaderTarget, string Name, Func<Vector4> get_parameter) : base(ShaderTarget, Name) => parameter = new DeepCopy<Vector4>(get_parameter);
        public override void OnUpdateUniform() => GL.Uniform4(location, parameter.Value);
        public override string VertDefinition(ref int Location) => GenericVertDef<Vector4>(ref Location);
        public override string FragDefinition(ref int Location) => GenericFragDef<Vector4>(ref Location);
        public override void SetUniform(IDeepCopy DeepCopy) => parameter = (DeepCopy<Vector4>)DeepCopy;
    }
    class Mat2Uniform : UniformParameter 
    {
        DeepCopy<Matrix2> parameter = new DeepCopy<Matrix2>();
        public Mat2Uniform(ShaderTarget ShaderTarget, string Name, DeepCopy<Matrix2> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public Mat2Uniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public Mat2Uniform(ShaderTarget ShaderTarget, string Name, Func<Matrix2> get_parameter) : base(ShaderTarget, Name) => parameter = new DeepCopy<Matrix2>(get_parameter);
        public override void OnUpdateUniform()
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
        DeepCopy<Matrix3> parameter = new DeepCopy<Matrix3>();
        public Mat3Uniform(ShaderTarget ShaderTarget, string Name, DeepCopy<Matrix3> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public Mat3Uniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public Mat3Uniform(ShaderTarget ShaderTarget, string Name, Func<Matrix3> get_parameter) : base(ShaderTarget, Name) => parameter = new DeepCopy<Matrix3>(get_parameter);
        public override void OnUpdateUniform()
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
        DeepCopy<Matrix4> parameter = new DeepCopy<Matrix4>();
        public Mat4Uniform(ShaderTarget ShaderTarget, string Name, DeepCopy<Matrix4> Parameter) : base(ShaderTarget, Name) => parameter = Parameter;
        public Mat4Uniform(ShaderTarget ShaderTarget, string Name) : base(ShaderTarget, Name) { }
        public Mat4Uniform(ShaderTarget ShaderTarget, string Name, Func<Matrix4> get_parameter) : base(ShaderTarget, Name) => parameter = new DeepCopy<Matrix4>(get_parameter);
        public override void OnUpdateUniform()
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
        private Func<int> getter;

        public TextureUniform(ShaderTarget ShaderTarget, string Name, Func<int> Texture) : base(ShaderTarget, Name) => getter = Texture;
        public override void OnUpdateUniform()
        {
            int unit = TextureManager.TexturesLoaded++;
            GL.BindTextureUnit(unit, getter());

            GL.ActiveTexture((TextureUnit)unit);
            GL.Uniform1(location, (int)unit);
        }
        public override string VertDefinition(ref int Location)
        {
            location = Location++;
            if (shadertarget == ShaderTarget.Fragment) return "";
            return $"layout(location = {location}) uniform sampler2D {name};{Environment.NewLine}";
        }
        public override string FragDefinition(ref int Location)
        {
            if (shadertarget == ShaderTarget.Vertex) return "";
            //if (shadertarget == ShaderTarget.Both) return $"layout(location = {this.location}) uniform sampler2D {name};{Environment.NewLine}";
            location = Location;
            return $"layout(location = {Location++}) uniform sampler2D {name};{Environment.NewLine}";
        }
        public override void SetUniform(IDeepCopy DeepCopy) => throw new Exception("Currently can't set texture this way.");
    }
    #endregion
}
