using Graphics;
using Graphics.Shaders;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;
using System;

namespace GameObjects
{
    /* THING TO DO:
     * ring shader function and all that stuff
     * rocket point in direction
     * 
     * 
     */
    /// <summary>
    /// A spherical point mass with light, rotating and follow properties
    /// </summary>
    class Sphereoid : PointMass
    {
        protected string TexturePath;
        private ClickBox clickbox;

        #region Lighting Fields and Properties
        protected Vector3 LightingDirection
        {
            get => GetLighting();
        }
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
        private bool dir_lighting = true;

        /* Directional Lighting Switches toggles get lighting between topdownlighting and sunlighting
         * 
         */
        private Func<Vector3> GetLighting;
        private Func<Vector3> TopDownLighting = () => new Vector3(0, 0, 1);
        private Func<Vector3> SunLighting;

        #endregion

        #region Rotation Fields and Properties
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
        protected Vector3 Rot;
        protected Matrix3 RotMat;

        #endregion

        #region Enlarging Fields and Properties
        public bool Enlarged
        {
            get => enlarged;
            set
            {
                enlarged = value;
                if (enlarged) Scale *= enlargedSF;
                else Scale /= enlargedSF;
            }
        }
        private bool enlarged = false;
        public float enlargedSF = 800;
        #endregion

        #region Fixed Position Fields and Properties
        
        /// <summary>
        /// fixes the position such that velocity does not change position. it still calculates new velocities every frame.
        /// </summary>
        public bool FixedPosition
        {
            get => fixedposition;
            set
            {
                fixedposition = value;
                if (fixedposition) SpaceSimWindow.UpdatePosition -= OnUpdatePosition;
                else SpaceSimWindow.UpdatePosition += OnUpdatePosition;
            }
        }
        private bool fixedposition = false;
        #endregion




        #region Constructors

        /// <summary>
        /// half initiated. shaders havent been set.
        /// </summary>
        /// <param name="Scale"></param>
        /// <param name="Position"></param>
        /// <param name="Mass"></param>
        /// <param name="Velocity"></param>
        /// <param name="VertShader"></param>
        /// <param name="FragShader"></param>
        public Sphereoid(Vector2 Scale, Vector2 Position, double Mass, Vector2d Velocity, string VertShader, string FragShader) : base(Scale, Position, Mass, Velocity, VertShader, FragShader)
        {
            EventManager.Program_Process += OnProgramProcess;

            SunLighting = () =>
            {
                Vector2 V = (SpaceSimWindow.sun.Position - this.Position).Normalized();
                return new Vector3(-V.X, V.Y, 0);
            };
            DirectionalLighting = true;

            clickbox = new ClickBox(() => Transform_Matrix, this.FixToScreen);
            clickbox.Click += () =>
            {
                if (clickbox.Time_Since_Last_Call < 0.5f) // if double click less than 0.5 seconds
                {
                    StartFollow(); // follow this planet
                }
            };
        }

        /// <summary>
        /// initated with planet shader.
        /// </summary>
        /// <param name="Scale"></param>
        /// <param name="Position"></param>
        /// <param name="Mass"></param>
        /// <param name="Velocity"></param>
        /// <param name="TexturePath"></param>
        public Sphereoid(Vector2 Scale, Vector2 Position, double Mass, Vector2d Velocity, string TexturePath) : base(Scale, Position, Mass, Velocity, "Default", "Planet")
        {
            EventManager.Program_Process += OnProgramProcess;
            this.TexturePath = TexturePath;

            SunLighting = () =>
            {
                Vector2 V = (SpaceSimWindow.sun.Position - this.Position).Normalized();
                return new Vector3(-V.X, V.Y, 0);
            };
            DirectionalLighting = true;

            // default 2d shader parameters
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", new DeepCopy<Matrix3>(() => Transform_Matrix)));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", () => RenderWindow.Camera.Transform_Matrix));

            SphereRotation = new Vector3(0, 0, 0.3926875f);

            // planet shader parameters
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "Texture", () => TextureManager.Get(this.TexturePath)));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Fragment, "rotmat3D", new DeepCopy<Matrix3>(() => RotMat)));
            ShaderProgram.AddUniform(new Vec3Uniform(ShaderTarget.Fragment, "lightDir", () => LightingDirection));
            ShaderProgram.AddUniform(new BoolUniform(ShaderTarget.Fragment, "DoLighting", () => true));
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

        #endregion

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

        #region Add Planet Stuff
        /// <summary>
        /// Adds Satellite to this sphereoid. uses v = sqrt(GM/r)
        /// </summary>
        /// <param name="SatelliteDiameter"></param>
        /// <param name="Mass"></param>
        /// <param name="OrbitRadius"></param>
        /// <param name="Texture"></param>
        /// <param name="TextureRes"></param>
        /// <returns></returns>
        public Sphereoid AddSatellite(float SatelliteDiameter, double Mass, float OrbitRadius, string TextureRes, string Texture) => new Sphereoid(new Vector2(SatelliteDiameter), this.Position + new Vector2(OrbitRadius, 0), Mass, this.Velocity + new Vector2d(0, Math.Sqrt(G * this.Mass / OrbitRadius)), $"Textures/{TextureRes}/{Texture}");
        /// <summary>
        /// Adds ring to this sphereoid.
        /// </summary>        
        public void AddRing() => new Ring(() => Position, () => LightingDirection);
        #endregion

        protected void OnProgramProcess(float delta) => SphereRotation = new Vector3(SphereRotation.X, SphereRotation.Y + delta / 24 / 3600, SphereRotation.Z);
    }

    #region Planet derived classes

    // 1 pixel = 1 000 000m = 6.68459e-6 AU
    class Sun : Sphereoid
    {
        public Sun(string TextureRes) : base(new Vector2(1.39e3f), Vector2.Zero, 1.99e30, Vector2.Zero, "Textures/" + TextureRes + "sun.png")
        {
            this.FixedPosition = true;
            this.DirectionalLighting = false;
            this.enlargedSF = 75;
            this.ShaderProgram["DoLighting"].SetUniform(new DeepCopy<bool>(() => false));
        }
    }
    class Earth : Sphereoid
    {
        public Earth(string TextureRes) :   base(new Vector2(1.28e1f), new Vector2(1.47e5f, 0), 5.97e24, new Vector2(0, 0.03f), "Default", "Earth") 
        {
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", new DeepCopy<Matrix3>(() => Transform_Matrix)));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", () => RenderWindow.Camera.Transform_Matrix));
            ShaderProgram.AddUniform(new FloatUniform(ShaderTarget.Both, "Time", () => EventManager.Program_Time));

            SphereRotation = new Vector3(-3.14f / 2, 0, 0.3926875f);

            // planet shader parameters
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "Day", () => TextureManager.Get("Textures/2K planet textures/earth.png")));
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "Night", () => TextureManager.Get("Textures/2K planet textures/earth_night.png")));
            //ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "Cloud", () => TextureManager.Get("Textures/2K planet textures/earth_cloud.png"))); // I have the textures I just dont know what to do with them
            //ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "Norm", () => TextureManager.Get("Textures/2K planet textures/earth_norm.png")));
            //ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "Spec", () => TextureManager.Get("Textures/2K planet textures/earth_spec.png")));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Fragment, "rotmat3D", () => RotMat));
            ShaderProgram.AddUniform(new Vec3Uniform(ShaderTarget.Fragment, "lightDir", () => this.LightingDirection));
            ShaderProgram.CompileProgram();

            //StartFollow();
        }
    }
    #endregion

    class Ring : RenderObject2D
    {
        Func<Vector3> GetLightingDir;
        Func<Vector2> GetPosition;
        public Ring(Func<Vector2> GetPosition, Func<Vector3> GetLightingDir) : base(SquareMesh, "Default", "PlanetRing")
        {
            this.GetLightingDir = GetLightingDir;
            this.GetPosition = GetPosition;

            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", () => this.Transform_Matrix));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", () => RenderWindow.Camera.Transform_Matrix));
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "Texture", () => 0));
            ShaderProgram.CompileProgram();
        }
    }

    class Rocket : PointMass
    {
        
        private ClickBox clickbox;
        public Rocket(Vector2 Position, Vector2d Velocity) : base(new Vector2(250), Position, 1.42e12, Velocity, "Default", "Rocket") 
        {
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "transform", new DeepCopy<Matrix3>(() => Transform_Matrix)));
            ShaderProgram.AddUniform(new Mat3Uniform(ShaderTarget.Vertex, "camera", () => RenderWindow.Camera.Transform_Matrix));
            ShaderProgram.AddUniform(new TextureUniform(ShaderTarget.Fragment, "Texture", () => TextureManager.Get("Textures/rocket.png")));
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
    }

}
