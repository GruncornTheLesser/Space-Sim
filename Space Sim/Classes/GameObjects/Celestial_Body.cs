using System;
using System.Collections.Generic;
using System.Text;
using Graphics;
using OpenTK.Mathematics;
using Shaders;
using DeepCopy;

namespace GameObjects
{
    enum PlanetType
    {
        Sun,
        Mercury,
        Venus,
        Earth,
        Mars,
        Jupiter,
        Saturn,
        Uranus,
        Neptune,
        Random
    }

    class Celestial_Body : RenderObject2D
    {
        public readonly float Mass;
        private int TextureHandle;

        /// <summary>
        /// X, Z represent the fixed axis tilt. Y speed of rotation -> 1.0f = 1 rotation per second
        /// </summary>
        public Vector3 PlanetRotation
        {
            get => Rot;
            set
            {
                Rot = value;
                RotMat = Matrix3.CreateRotationX(Rot.X) * Matrix3.CreateRotationZ(Rot.Z) * Matrix3.CreateRotationY(Rot.Y); // want to rotate Y first
            }
        }
        public bool Adaptive_lighting = false; // used for sun

        private Vector3 Rot;
        private Matrix3 RotMat;
        

        public Celestial_Body(Vector2 Scale, Vector2 Position, string Texture) : base(Window.SquareMesh, "Default", "Planet")
        {
            this.Scale = Scale;
            this.Position = Position;
            Z_index = 2;

            TextureHandle = Init_Textures(Texture);
            // default 2d shader parameters
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", TransformCopy));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", Window.CameraCopy));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Both, "Time", EventManager.TimeCopy));

            PlanetRotation = new Vector3(0, 1, 0.3926875f);
            // planet shader parameters
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "Texture", new DeepCopy<int>(() => TextureHandle)));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Both, "XZrotmat3D", new DeepCopy<Matrix3>(() => RotMat)));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Fragment, "Yrotrate", new DeepCopy<float>(() => PlanetRotation.Y)));
            ShaderProgram.AddUniform(new Vec3Uniform(ShaderTarget.Fragment, "light_dir", new DeepCopy<Vector3>(() => GetLighting())));

            ShaderProgram.CompileProgram();
        }

        public override void OnProcess(float delta)
        {
            //Position = new Vector2(MathF.Sin(EventManager.Time), -MathF.Cos(EventManager.Time)) * 200000;
            PlanetRotation = new Vector3(0.0f, 1.0f * EventManager.Time, 0.3926875f);
        }
    
        protected virtual Vector3 GetLighting()
        {
            /*Position of sun minus Position*/
            // for testing used screen position while planets arent moving

            if (Adaptive_lighting)
            {
                Vector2 V = Window.WorldToScreen(Position);
                return new Vector3(V.X, -V.Y, V.Y);
            }
            else
            {
                return new Vector3(0, 0, 1);
            }
        }
    
    }
    
}
