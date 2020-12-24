using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
namespace Graphics
{
    class Node2D
    {
        // this could be improved if i wasnt using OpenTK.mathematics
        public Matrix3 Matrix;
        private float rotation;
        private Vector2 scale;
        private Vector2 position;
        public float Rotation
        {
            set
            {
                rotation = value;
                Matrix = new Matrix3(
                    scale.X * MathF.Cos(rotation), -scale.Y * MathF.Sin(rotation), position.X,
                    scale.X * MathF.Sin(rotation), scale.Y * MathF.Cos(rotation), position.Y,
                    0, 0, 1
                );
            }
            get
            {
                return rotation;
            }
        }
        public Vector2 Scale
        {
            set
            {
                scale = value;
                Matrix = new Matrix3(
                    scale.X * MathF.Cos(rotation), -scale.Y * MathF.Sin(rotation), position.X,
                    scale.X * MathF.Sin(rotation), scale.Y * MathF.Cos(rotation), position.Y,
                    0, 0, 1
                );
            }
            get
            {
                return scale;
            }
        }
        public Vector2 Position
        {
            set
            {
                position = value;
                Matrix = new Matrix3(
                    scale.X * MathF.Cos(rotation), -scale.Y * MathF.Sin(rotation), position.X,
                    scale.X * MathF.Sin(rotation), scale.Y * MathF.Cos(rotation), position.Y,
                    0, 0, 1
                    );
            }
            get
            {
                return position;
            }
        }

        public Node2D(float rotation, Vector2 scale, Vector2 position)
        {
            this.rotation = rotation;
            this.scale = scale;
            this.position = position;
            Matrix = new Matrix3(
                    scale.X * MathF.Cos(rotation), -scale.Y * MathF.Sin(rotation), position.X,
                    scale.X * MathF.Sin(rotation), scale.Y * MathF.Cos(rotation), position.Y,
                    0, 0, 1
                    );
        }

    }
}
