using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using Shaders;
using DeepCopy;
namespace Graphics
{
    class Node2D
    {
        /// <summary>
        /// the matrix transform. setting this directly will not update rotation, position or scale
        /// </summary>
        public Matrix3 Transform_Matrix;

        private float rotation;
        private Vector2 scale;
        private Vector2 position;
        /// <summary>
        /// The rotation of the matrix
        /// </summary>
        public float Rotation
        {
            set
            {
                rotation = value;
                Transform_Matrix = new Matrix3(
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
        /// <summary>
        /// the scale of the matrix
        /// </summary>
        public Vector2 Scale
        {
            set
            {
                scale = value;
                Transform_Matrix = new Matrix3(
                    scale.X * MathF.Cos(rotation), scale.Y *-MathF.Sin(rotation), position.X,
                    scale.X * MathF.Sin(rotation), scale.Y * MathF.Cos(rotation), position.Y,
                    0, 0, 1
                );
            }
            get
            {
                return scale;
            }
        }
        /// <summary>
        /// the position of the matrix
        /// </summary>
        public Vector2 Position
        {
            set
            {
                position = value;
                Transform_Matrix = new Matrix3(
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
            Transform_Matrix = new Matrix3(
                    scale.X * MathF.Cos(rotation),-scale.Y * MathF.Sin(rotation), position.X,
                    scale.X * MathF.Sin(rotation), scale.Y * MathF.Cos(rotation), position.Y,
                    0, 0, 1
                    );
        }
        public Node2D(float rotation, float scaleX, float scaleY, float positionX, float positionY)
        {
            this.rotation = rotation;
            this.scale = new Vector2(scaleX, scaleY);
            this.position = new Vector2(positionX, positionY);
            Transform_Matrix = new Matrix3(
                    scaleX * MathF.Cos(rotation), -scaleY * MathF.Sin(rotation), positionX,
                    scaleX * MathF.Sin(rotation), scaleY * MathF.Cos(rotation), positionY,
                    0, 0, 1
                    );
        }
    }
}
