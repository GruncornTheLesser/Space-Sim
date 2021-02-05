using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using Shaders;
namespace Space_Sim.Classes._Removed
{
    // Not doing 3d isnt neccessary + not what user asked for.
    // yuck.
    //https://www.symbolab.com/solver/simplify-calculator/simplify%20%5Cbegin%7Bpmatrix%7D1%260%260%26M%5C%5C%20%20%20%20%200%261%260%26N%5C%5C%20%20%20%20%200%260%261%26O%5C%5C%20%20%20%20%200%260%260%261%5Cend%7Bpmatrix%7D%5Cbegin%7Bpmatrix%7DS%260%260%260%5C%5C%20%20%20%200%26T%260%260%5C%5C%20%20%20%200%260%26U%260%5C%5C%20%20%20%200%260%260%261%5Cend%7Bpmatrix%7D%5Cbegin%7Bpmatrix%7DcosZ%26-sinZ%260%260%5C%5C%20%20%20%20sin%5Cleft(Z%5Cright)%26cosZ%260%260%5C%5C%20%20%20%20%20%20%200%260%261%260%5C%5C%20%20%20%20%20%20%200%260%260%261%5Cend%7Bpmatrix%7D%5Cbegin%7Bpmatrix%7DcosY%260%26sinY%260%5C%5C%20%20%20%20%20%20%20%20%200%261%260%260%5C%5C%20%20%20%20%20%20%20%20-sinY%260%26cosY%260%5C%5C%20%20%20%20%20%20%20%200%20%260%260%261%5Cend%7Bpmatrix%7D%5Cbegin%7Bpmatrix%7D1%260%260%260%5C%5C%20%20%20%20%20%20%20%200%26cosX%26-sinX%260%5C%5C%20%20%20%20%20%20%20%200%26sinX%26cosX%260%5C%5C%20%20%20%20%20%20%20%200%260%260%261%5Cend%7Bpmatrix%7D
    // the individual matrix transformation multiplied together into 1 matrix
    // theyve been split into properties so i can better optimise when changing rotation, position and scale
    class Node3D
    {
        public Matrix4 Transform_Matrix;
        private Vector3 rotation;
        private Vector3 scale;
        private Vector3 position;


        public Vector3 Rotation3D
        {
            set
            {
                SetTransform(value, scale, position);
            }
            get
            {
                return rotation;
            }
        }
        public Vector3 Scale3D
        {
            set
            {
                Transform_Matrix = new Matrix4(
                    new Vector4(
                        value.X * Transform_Matrix.Row0.X / scale.X,
                        value.X * Transform_Matrix.Row0.Y / scale.X,
                        value.X * Transform_Matrix.Row0.Z / scale.X,
                        position.X
                        ),
                    new Vector4(
                        value.Y * Transform_Matrix.Row1.X / scale.Y,
                        value.Y * Transform_Matrix.Row1.Y / scale.Y,
                        value.Y * Transform_Matrix.Row1.Z / scale.Y,
                        position.Y
                        ),
                    new Vector4(
                        value.Z * Transform_Matrix.Row2.X / scale.Z,
                        value.Z * Transform_Matrix.Row2.Y / scale.Z,
                        value.Z * Transform_Matrix.Row2.Z / scale.Z,
                        position.Z
                        ),
                    new Vector4(0, 0, 0, 1)
                );
                scale = value;

            }
            get
            {
                return scale;
            }
        }
        public Vector3 Position3D
        {
            set
            {
                position = value;
                Transform_Matrix = new Matrix4(
                    new Vector4(
                        Transform_Matrix.Row0.X,
                        Transform_Matrix.Row0.Y,
                        Transform_Matrix.Row0.Z,
                        position.X
                        ),
                    new Vector4(
                        Transform_Matrix.Row1.X,
                        Transform_Matrix.Row1.Y,
                        Transform_Matrix.Row1.Z,
                        position.Y
                        ),
                    new Vector4(
                        Transform_Matrix.Row2.X,
                        Transform_Matrix.Row2.Y,
                        Transform_Matrix.Row2.Z,
                        position.Z
                    ),
                    new Vector4(0, 0, 0, 1));
            }
            get
            {
                return position;
            }
        }

        public float Rotation2D
        {
            set
            {
                Rotation3D = new Vector3(0, 0, value);
            }
            get
            {
                return rotation.Z;
            }

        }
        public Vector2 Scale2D
        {
            set
            {
                Scale3D = new Vector3(value.X, value.Y, 1);
            }
            get
            {
                return new Vector2(scale.X, scale.Y);
            }

        }
        public Vector2 Position2D
        {
            set
            {
                Position3D = new Vector3(value.X, value.Y, 0);
            }
            get
            {
                return new Vector2(position.X, position.Y);
            }
        }


        public Node3D(Vector3 rotation, Vector3 scale, Vector3 position)
        {
            this.rotation = rotation;
            this.scale = scale;
            this.position = position;
        }
        public Node3D(float rotationX, float rotationY, float rotationZ, float scaleX, float scaleY, float scaleZ, float positionX, float positionY, float positionZ)
        {
            rotation = new Vector3(rotationX, rotationY, rotationZ);
            position = new Vector3(positionX, positionY, positionZ);
            scale = new Vector3(scaleX, scaleY, scaleZ);

            SetTransform(rotation, scale, position);
        }

        public Node3D(float rotation, Vector2 scale, Vector2 position)
        {
            this.rotation = new Vector3(0, 0, rotation);
            this.scale = new Vector3(scale.X, scale.Y, 1);
            this.position = new Vector3(position.X, position.Y, 0);

            SetTransform(this.rotation, this.scale, this.position);
        }
        public Node3D(float rotation, float scaleX, float scaleY, float positionX, float positionY)
        {
            this.rotation = new Vector3(0, 0, rotation);
            this.scale = new Vector3(scaleX, scaleY, 0);
            this.position = new Vector3(positionX, positionY, 0);

            SetTransform(this.rotation, this.scale, this.position);
        }

        public void SetTransform(Vector3 Rotation, Vector3 Scale, Vector3 Position)
        {
            this.rotation = Rotation;
            this.scale = Scale;
            this.position = Position;

            Vector3 SinR = new Vector3(MathF.Sin(rotation.X), MathF.Sin(rotation.Y), MathF.Sin(rotation.Z));
            Vector3 CosR = new Vector3(MathF.Cos(rotation.X), MathF.Cos(rotation.Y), MathF.Cos(rotation.Z));

            Transform_Matrix = new Matrix4(
                new Vector4(
                    scale.X * CosR.Y * CosR.Z,
                    scale.X * (SinR.X * SinR.Y * CosR.Z - CosR.X * SinR.Z),
                    scale.X * (CosR.X * SinR.Y * CosR.Z + SinR.X * SinR.Z),
                    position.X
                    ),
                new Vector4(
                    scale.Y * CosR.Y * SinR.Z,
                    scale.Y * (CosR.X * CosR.Z + SinR.X * SinR.Y * SinR.Z),
                    scale.Y * (SinR.X * CosR.Z - CosR.X * SinR.Y * SinR.Z),
                    position.Y
                    ),
                new Vector4(
                    scale.Z * SinR.Y,
                    scale.Z * SinR.Z * CosR.Y,
                    scale.Z * CosR.X * CosR.Y,
                    position.Z
                ),
                new Vector4(0, 0, 0, 1));

        }
    }
}
