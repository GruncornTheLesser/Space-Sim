using Graphics;
using OpenTK.Mathematics;
using System;

namespace GameObjects
{ 
    abstract class PointMass : RenderObject2D
    {
        protected const double G = 6.67e-11 * 1e-12 * 1e-6; // 1 pixel = 1 000 km so r^-2 means (1e6)^-2 to get to 1m for calculations, then to convert back down another 1e-6
        public double Mass;
        public Vector2d Velocity;

        public Trail Trail;
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

        public PointMass(Vector2 Scale, Vector2 Position, double Mass, Vector2d Velocity, string VertShader, string FragShader) : base(SquareMesh, VertShader, FragShader)
        {
            SpaceSimWindow.UpdatePosition += OnUpdatePosition;
            SpaceSimWindow.QuadTree.Add(this);
            Set_visible += (value) => Trail.Visible = value;

            this.Scale = Scale;
            this.Position = Position;
            this.Mass = Mass;
            this.Velocity = Velocity;
            this.Z_index = 3;
            this.Trail = new Trail(1000, 0, () => this.Position, Color4.White);
        }


        /// <summary>
        /// Calculates the acceleration that this point mass would experience from point mass P
        /// </summary>
        /// <param name="P"></param>
        /// <param name="delta"></param>
        public Vector2d CalcAccFrom(PointMass P, float delta) => CalcAccFrom(P.Mass, P.Position);
        /// <summary>
        /// Calculates the acceleration that this point mass would experience from mass M at position P
        /// </summary>
        /// <param name="Mass">the mass of the other point mass</param>
        /// <param name="Position">the position of the other point mass</param>
        /// <returns>the acceleration that Mass at Position exerts on this object</returns>
        public Vector2d CalcAccFrom(double M, Vector2 P)
        {
            Vector2d r = (P - this.Position);
            Vector2d Accel = Vector2.Zero;

            double dist = r.Length;
            if (dist > 500)
            {
                // this approach reduces the chances of an error
                // previously resolved forces on x and forces on y
                // that was prone to errors because as r^2 -> 0 Acc -> ∞. using the scalar magnitude i reduce the chance of it being a problem.
                // things still can get dicey if the distance between bodies is too small 
                double Mag = G * M / Math.Pow(dist, 2); // magnitude
                Accel = r.Normalized() * Mag; // direction * magnitude
            }

            return Accel;
        }

        protected virtual void Destroy()
        {
            Trail.Stop(); // trail will hide with visible but its better for processing time to also stop replacing vertices
            Visible = false;
        }
        private void OnUpdatePosition(float delta) => Position += (Vector2)Velocity * delta;
    }
}

