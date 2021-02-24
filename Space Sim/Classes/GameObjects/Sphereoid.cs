using Graphics;
using Shaders;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;
using System;

namespace GameObjects
{
    class Sphereoid : PointMass
    {
        private ClickBox clickbox;

        public bool DirectionalLighting 
        {
            get => dir_lighting;
            set
            {
                dir_lighting = value;
                if (dir_lighting) GetLighting = SunLighting;
                else GetLighting = TopDownLighting;
            }
        }
        private Vector3 LightingDirection 
        { 
            get => GetLighting();
        }

        private bool dir_lighting = true;
        private Func<Vector3> GetLighting;
        private Func<Vector3> TopDownLighting = () => new Vector3(0, 0, 1);
        private Func<Vector3> SunLighting;

        /// <summary>
        /// X, Y, Z rotation of planet. Y first then Z, X
        /// </summary>
        public Vector3 SphereRotation
        {
            get => Rot;
            set
            {
                Rot = value;
                RotMat = Matrix3.CreateRotationX(Rot.X) * Matrix3.CreateRotationZ(Rot.Z) * Matrix3.CreateRotationY(Rot.Y); // y first to spin planet
            }
        }
        private Vector3 Rot;
        private Matrix3 RotMat;

        private string TexturePath;

        public Sphereoid(Vector2 Scale, Vector2 Position, double Mass, Vector2d Velocity, string TexturePath) : base(Scale, Position, Mass, Velocity, "Default", "Planet")
        {
            EventManager.Program_Process += OnProcess;
            this.TexturePath = TexturePath;

            SunLighting = () =>
            {
                Vector2 V = Position.Normalized();
                return new Vector3(V.X, -V.Y, V.Y);
            };
            DirectionalLighting = true;

            // default 2d shader parameters
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", new DeepCopy<Matrix3>(() => Transform_Matrix)));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", () => RenderWindow.Camera.Transform_Matrix));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Both, "Time", () => EventManager.Program_Time));

            SphereRotation = new Vector3(0, 1, 0.3926875f);

            // planet shader parameters
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "Texture", new DeepCopy<int>(() => TextureManager.Get(this.TexturePath))));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Fragment, "rotmat3D", new DeepCopy<Matrix3>(() => RotMat)));
            ShaderProgram.AddUniform(new Vec3Uniform(ShaderTarget.Fragment, "lightDir", () => LightingDirection));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Fragment, "dark", () => 0));
            ShaderProgram.CompileProgram();

            clickbox = new ClickBox(() => Transform_Matrix, this.FixToScreen);
            clickbox.Click += () =>
            {
                if (clickbox.Time_Since_Last_Call < 0.5f) // if double click less than 0.5 seconds
                {
                    StartFollow(); // follow this planet
                }
            };

        }

        #region Camera Follow Functions
        private void CameraFollow(float delta) => RenderWindow.Camera.WorldPosition = Position; // set camera world position
        private void StopWithButtonFollow(MouseState M, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Button3)
            {
                StopFollow();
            }
        }
        private void StopFollow()
        {
            EventManager.Program_Process -= CameraFollow; // remove follow functions -> it wont follow on process
            EventManager.MouseDown -= StopWithButtonFollow; // remove stop follow function -> it wont stop following when click
            SpaceSimWindow.HomeButton.Release -= StopFollow;
        }
        protected void StartFollow()
        {
            EventManager.Program_Process += CameraFollow; // every frame set camera world position to this planets position
            EventManager.MouseDown += StopWithButtonFollow; // if click again stop following
            SpaceSimWindow.HomeButton.Release += StopFollow;
        }
        #endregion

        protected void OnProcess(float delta)
        {
            //Position += new Vector2(MathF.Sin(EventManager.Program_Time), MathF.Cos(EventManager.Program_Time)) * 100;
            SphereRotation = new Vector3(SphereRotation.X, 1.0f * EventManager.Program_Time, SphereRotation.Z);
        }
    
    }


    #region Planet derived classes

    // 1 pixel = 1 000 000m = 6.68459e-6 AU
    // 1 unit mass = 1 000 000 000 000 000 000 000 000 kg = 10e24 kg
    class Sun : Sphereoid
    {
        public Sun(Vector2 Position, Vector2 Velocity, string TextureRes) : base(new Vector2(0.695f * 2), Position, 1.99e24f, Velocity, "Textures/" + TextureRes + "sun.png")
        {
            this.DirectionalLighting = false;
            this.enlargedSF = 80;
            this.ShaderProgram["dark"].SetUniform(new DeepCopy<float>(() => 1.0f));
        }
    }
    class Mercury : Sphereoid
    {
        public Mercury(Vector2 Position, Vector2 Velocity, string TextureRes) : base(new Vector2(0.00244f * 2), Position, 1, Velocity, "Textures/" + TextureRes + "mercury.png") { }
    }
    class Venus : Sphereoid
    {
        public Venus(Vector2 Position, Vector2 Velocity, string TextureRes) : base(new Vector2(0.00605f * 2), Position, 1, Velocity, "Textures/" + TextureRes + "venus.png") { }
    }
    class Earth : Sphereoid
    {
        public Earth(Vector2 Position, Vector2 Velocity, string TextureRes) : base(new Vector2(0.00637f * 2), Position, 1, Velocity, "Textures/" + TextureRes + "earth.png") 
        {
            StartFollow();
        }
    }
    class Mars : Sphereoid
    {
        public Mars(Vector2 Position, Vector2 Velocity, string TextureRes) : base(new Vector2(0.00338f * 2), Position, 1, Velocity, "Textures/" + TextureRes + "mars.png") { }
    }
    class Jupiter : Sphereoid
    {
        public Jupiter(Vector2 Position, Vector2 Velocity, string TextureRes) : base(new Vector2(0.0699f * 2), Position, 1, Velocity, "Textures/" + TextureRes + "jupiter.png") { }
    }
    class Saturn : Sphereoid
    {
        public Saturn(Vector2 Position, Vector2 Velocity, string TextureRes) : base(new Vector2(0.0582f * 2), Position, 1, Velocity, "Textures/" + TextureRes + "saturn.png") { }
    }
    class Uranus : Sphereoid
    {
        public Uranus(Vector2 Position, Vector2 Velocity, string TextureRes) : base(new Vector2(0.0254f * 2), Position, 1, Velocity, "Textures/" + TextureRes + "uranus.png") { }
    }
    class Neptune : Sphereoid
    {
        public Neptune(Vector2 Position, Vector2 Velocity, string TextureRes) : base(new Vector2(0.0246f * 2), Position, 1, Velocity, "Textures/" + TextureRes + "neptune.png") { }
    }
    class Moon : Sphereoid
    {
        public Moon(Vector2 Position, Vector2 Velocity, string TextureRes) : base(new Vector2(0.00347f * 2), Position, 1, Velocity, "Textures/" + TextureRes + "moon.png") { }
    }
        #endregion


}
