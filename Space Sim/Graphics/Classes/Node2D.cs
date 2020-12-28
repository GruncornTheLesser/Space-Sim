using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
namespace Graphics
{
    class Node2D
    {
        // this could be improved if i wasnt using OpenTK.mathematics
        public Matrix3 Transform_Matrix;
        private float rotation;
        private Vector2 scale;
        private Vector2 position;
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
        public Vector2 Scale
        {
            set
            {
                scale = value;
                Transform_Matrix = new Matrix3(
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
                    scale.X * MathF.Cos(rotation), -scale.Y * MathF.Sin(rotation), position.X,
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

    class Node3D
    {
        public Matrix4 TransformMatrix;
        private Vector3 rotation;
        private Vector3 scale;
        private Vector3 position;

        //public Vector3 Rotation { }
        //public Vector3 Scale { }
        //public Vector3 Position { }

        public Node3D(Vector3 rotation, Vector3 scale, Vector3 position) 
        { 
            
        }
        public Node3D(float rotationX, float rotationY, float rotationZ, float scaleX, float scaleY, float scaleZ, float positionX, float positionY, float positionZ) { }
    }
}
