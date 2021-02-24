using GameObjects;
using Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameObjects
{ 
    abstract class PointMass : RenderObject2D
    {
        public Trail Trail;

        public readonly double Mass;
        public Vector2d Velocity;

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
            EventManager.Program_Process += OnProcess;
            SpaceSimWindow.QuadTree.Add(this);
            Set_visible += (value) => Trail.Visible = value;

            this.Scale = Scale;
            this.Position = Position;
            this.Mass = Mass;
            this.Velocity = Velocity;
            this.Z_index = 3;
            this.Trail = new Trail(1000, 0, () => this.Position, Color4.White);
        }


       private void OnProcess(float delta)
        {
            Position -= (Vector2)Velocity * delta;
        }
    }
}

