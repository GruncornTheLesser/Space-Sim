using Graphics;
using OpenTK.Mathematics;
using System;

namespace GameObjects
{ 
    abstract class PointMass : RenderObject2D
    {
        protected const double G = 6.67e-11 * 1e-12 * 1e-6; // m^3⋅kg^−1⋅s^−2 1 pixel = 1 000 km so r^-2 means (1e6)^-2 to get to 1m for calculations, then to convert back down another 1e-6
        protected SpaceSimWindow SimWin;
        public double Mass;
        public Vector2d Velocity;

        public Trail Trail;
        
        

        public PointMass(SpaceSimWindow Sim, RenderWindow Window, Vector2 Scale, Vector2 Position, double Mass, Vector2d Velocity, string VertShader, string FragShader) : base(Window, SquareMesh, VertShader, FragShader)
        {
            SimWin = Sim;
            SimWin.UpdatePosition += OnUpdatePosition;
            SimWin.QuadTree.Add(this);
            Set_visible += (value) => Trail.Visible = value;

            this.Scale = Scale;
            this.Position = Position;
            this.Mass = Mass;
            this.Velocity = Velocity;
            this.Z_index = 3;
            this.Trail = new Trail(RenderWindow, 1000, 0, () => this.Position, Color4.White);
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
        public Vector2d CalcAccFrom(double Mass, Vector2 Position)
        {
            // this approach reduces the chances of an error
            // previously resolved forces on x and forces on y
            // that was prone to errors because as r^2 -> 0 Acc -> ∞. using the scalar magnitude i reduce the chance of it being a problem.
            // things still can get dicey if the distance between bodies is too small 
            Vector2d r = (Position - this.Position);
            double dist = r.Length;
            double Mag = G * Mass / Math.Pow(dist, 2); // magnitude
            return r.Normalized() * Mag; // direction * magnitude
        }

        protected virtual void Destroy()
        {
            Trail.Stop(); // trail will hide with visible but its better for processing time to also stop replacing vertices
            Visible = false;
        }
        protected void OnUpdatePosition(float delta) => Position = Mass == 0 ? Vector2.Zero : Position + (Vector2)Velocity * delta;
    }
}

