using System;
using OpenTK.Mathematics;

namespace Graphics
{
    class Transform
    {
        /// <summary>
        /// the matrix transform. setting this directly will not update rotation, position or scale
        /// </summary>
        public Matrix3 Transform_Matrix;
        private Matrix2 Rotation_Matrix; // top left corner, unscaled
        private float rotation;
        private Vector2 scale;
        private Vector2 position;
        /// <summary>
        /// The rotation of this transform
        /// </summary>
        public float Rotation
        {
            set
            {
                rotation = value;
                Rotation_Matrix = Matrix2.CreateRotation(rotation);
                Transform_Matrix.M11 = Scale.X * Rotation_Matrix.M11; Transform_Matrix.M12 = Scale.Y * Rotation_Matrix.M12;
                Transform_Matrix.M21 = Scale.X * Rotation_Matrix.M21; Transform_Matrix.M22 = Scale.Y * Rotation_Matrix.M22;
            }
            get
            {
                return rotation;
            }
        }
        /// <summary>
        /// the scale of this transform
        /// </summary>
        public Vector2 Scale
        {
            set
            {
                scale = value;
                Transform_Matrix.M11 = Scale.X * Rotation_Matrix.M11; Transform_Matrix.M12 = Scale.Y * Rotation_Matrix.M12;
                Transform_Matrix.M21 = Scale.X * Rotation_Matrix.M21; Transform_Matrix.M22 = Scale.Y * Rotation_Matrix.M22;
            }
            get
            {
                return scale;
            }
        }
        /// <summary>
        /// the position of this transform
        /// </summary>
        public Vector2 Position
        {
            set
            {
                position = value;
                Transform_Matrix.M13 = Position.X;
                Transform_Matrix.M23 = Position.Y;
            }
            get
            {
                return position;
            }
        }

        public Transform(float rotation, Vector2 scale, Vector2 position)
        {
            this.rotation = rotation;
            this.scale = scale;
            this.position = position;
            Set(rotation, this.scale, this.position);
        }
        public Transform(float rotation, float scaleX, float scaleY, float positionX, float positionY)
        {
            this.rotation = rotation;
            this.scale = new Vector2(scaleX, scaleY);
            this.position = new Vector2(positionX, positionY);
            Set(rotation, this.scale, this.position);
        }
        
        public void Set(float Rotation, Vector2 Scale, Vector2 Position)
        {
            Rotation_Matrix = Matrix2.CreateRotation(rotation);
            Transform_Matrix = new Matrix3(
                    scale.X * Rotation_Matrix.M11, -scale.Y * Rotation_Matrix.M12, position.X,
                    scale.X * Rotation_Matrix.M21, scale.Y * Rotation_Matrix.M22, position.Y,
                    0, 0, 1 // this row doesnt change
                    );
        }
    
    }
}
