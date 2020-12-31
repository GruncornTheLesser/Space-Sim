using System;
using System.Drawing;

namespace Maths
{
    /*
     * The purpose of this namespace was to deal with matrix calculations. The reason I didnt want to use OpenGL.Mathematics was in certain scenarios it 
     * doesnt work how expected. for example it doesnt always allow matrix multiplication and theyre all sealed classes meaning I cant inherit from them.
     * 
     * Because of the way openTK is set up it needs reference variables so I decided to scrap this namespace.
     * Ive replaced the Node2D transform matrix with a new transform matrix which holds all the same variables and data but also a matrix variable.
     * 
     * It is quite satisfying setting something like this up but its stupid if there is something already available.
     */
    struct Vector2
    {
        public float X, Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
        public Vector2(Vector3 V)
        {
            X = V.X;
            Y = V.Y;
        }

        public static Vector2 operator +(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X + right.X, left.Y + right.Y);
        }
        public static Vector2 operator -(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X - right.X, left.Y - right.Y);
        }
        public static Vector2 operator *(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X * right.X, left.Y * right.Y);
        }
        public static Vector2 operator *(Vector2 left, float right)
        {
            return new Vector2(left.X * right, left.Y * right);
        }
        public static Vector2 operator *(float left, Vector2 right)
        {
            return new Vector2(right.X * left, right.Y * left);
        }
        public static Vector2 operator /(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X / right.X, left.Y / right.Y);
        }
        


        public static explicit operator Size(Vector2 V)
        {
            return new Size((int)V.X, (int)V.Y);
        }
        public static explicit operator OpenTK.Mathematics.Vector4(Vector2 V) 
        {
            return new OpenTK.Mathematics.Vector4((float)V.X, (float)V.Y, 0, 1);
        }
        public static explicit operator OpenTK.Mathematics.Vector2(Vector2 V)
        {
            return new OpenTK.Mathematics.Vector2((float)V.X, (float)V.Y);
        }
        
        public float dist()
        {
            return (float)MathF.Sqrt(X * X + Y * Y);
        }
        public float dot(Vector2 right)
        {
            return X * right.X + Y * right.Y;
        }
        public float cross(Vector2 right)
        {
            return X * right.Y + Y * right.X;
        }
    }
    struct Vector3
    {
        public float X, Y, Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector3(Vector2 V)
        {
            X = V.X;
            Y = V.Y;
            Z = 1;
        }
        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }
        public static Vector3 operator *(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }
        public static Vector3 operator *(Vector3 left, float right)
        {
            return new Vector3(left.X * right, left.Y * right, left.Z * right);
        }
        public static Vector3 operator *(float left, Vector3 right)
        {
            return new Vector3(left * right.X, left * right.Y, left * right.Z);
        }
        public static Vector3 operator /(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
        }

        public static explicit operator Vector2(Vector3 V)
        {
            return new Vector2(V.X, V.Y);
        }
        public float dist()
        {
            return MathF.Sqrt(X * X + Y * Y + Z * Z);
        }
        public float dot(Vector3 V)
        {
            return X * V.X + Y * V.Y + Z * V.Z;
        }
        public float cross(Vector3 V)
        {
            return X * (V.Y + V.Z) + Y * (V.X + V.Z) + Z * (V.X + V.Y);
        }
    }
    /// <summary>
    /// 3x3 matrix.
    /// </summary>
    class Matrix3
    {
        private protected float M00, M01, M02, M10, M11, M12, M20, M21, M22;
        protected internal Matrix3 inverse
        {
            get
            {
                return (1 / Determinant) * new Matrix3(
                +(M11 * M22 - M12 * M21), -(M10 * M22 - M20 * M12), +(M10 * M21 - M20 * M11),
                -(M01 * M22 - M02 * M21), +(M00 * M22 - M20 * M02), -(M00 * M21 - M20 * M01),
                +(M01 * M12 - M11 * M02), -(M00 * M12 - M10 * M02), +(M00 * M11 - M10 * M01)
                );
            }
        }
        protected internal float Determinant
        {
            get
            {
                return M00 * (M11 * M22 - M21 * M12) - M01 * (M01 * M22 - M21 * M02) + M02 * (M01 * M12 - M11 * M02);
            }
        }

        // constructors
        public Matrix3(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
        {
            M00 = m00; M01 = m01; M02 = m02;
            M10 = m10; M11 = m11; M12 = m12;
            M20 = m20; M21 = m21; M22 = m22;
        }
        public Matrix3(Vector3 Colummn0, Vector3 Column1, Vector3 Column2)
        {
            M00 = Colummn0.X; M01 = Column1.X; M02 = Column2.X;
            M10 = Colummn0.Y; M11 = Column1.Y; M12 = Column2.Y;
            M20 = Colummn0.Z; M21 = Column1.Z; M22 = Column2.Z;
        }
        
        // explicit operators
        public static explicit operator OpenTK.Mathematics.Matrix3(Matrix3 M)
        {
            return new OpenTK.Mathematics.Matrix3(M.M00, M.M02, M.M02, M.M10, M.M11, M.M12, M.M20, M.M21, M.M22);
        }

        // operators
        public static Matrix3 operator *(Matrix3 left, Matrix3 right)
        {
            return new Matrix3(
                left.M00 * right.M00 + left.M10 * right.M01 + left.M20 * right.M02,
                left.M00 * right.M10 + left.M10 * right.M11 + left.M20 * right.M12,
                left.M00 * right.M20 + left.M10 * right.M21 + left.M20 * right.M22,

                left.M01 * right.M00 + left.M11 * right.M01 + left.M21 * right.M02,
                left.M01 * right.M10 + left.M11 * right.M11 + left.M21 * right.M12,
                left.M01 * right.M20 + left.M11 * right.M21 + left.M21 * right.M22,

                left.M02 * right.M00 + left.M12 * right.M01 + left.M22 * right.M02,
                left.M02 * right.M10 + left.M12 * right.M11 + left.M22 * right.M12,
                left.M02 * right.M20 + left.M12 * right.M21 + left.M22 * right.M22
                );
        }
        public static Vector3 operator *(Matrix3 left, Vector3 right)
        {
            return new Vector3(
                left.M00 * right.X + left.M10 * right.Y + left.M20 * right.Z,
                left.M01 * right.X + left.M11 * right.Y + left.M21 * right.Z,
                left.M02 * right.X + left.M12 * right.Y + left.M22 * right.Z
                );
        }
        public static Vector2 operator *(Matrix3 left, Vector2 right)
        {
            return (Vector2)(left * new Vector3(right));
        }
        public static Matrix3 operator *(float D, Matrix3 M)
        {
            return new Matrix3(
                D * M.M00, D * M.M10, D * M.M20,
                D * M.M01, D * M.M11, D * M.M21,
                D * M.M02, D * M.M12, D * M.M22
                );
        }   
    }
    
    /// <summary>
    /// 2d transformation represented with a 3x3 matrix. Includes variables for scale translation and rotation.
    /// </summary>
    class Node2D : Matrix3
    {
        // stores variables
        private Vector2 scale;
        private Vector2 translation;
        private float rotation;

        public float Rotation
        { 
            set
            {
                rotation = value;
                M00 = scale.X * MathF.Cos(rotation);
                M10 =-scale.Y * MathF.Sin(rotation);
                //M20 = translation.X;
                M01 = scale.X * MathF.Sin(rotation);
                M11 = scale.Y * MathF.Cos(rotation);
                //M21 = translation.Y;
                //M02 = 0;
                //M12 = 0;
                //M22 = 1;
            }
            get
            {
                return rotation;
            }
        }
        public Vector2 Position
        {
            set
            {
                translation = value;

                //M00 = scale.X * MathF.Cos(rotation);
                //M10 = -scale.Y * MathF.Sin(rotation);
                M20 = translation.X;
                //M01 = scale.X * MathF.Sin(rotation);
                //M11 = scale.Y * MathF.Cos(rotation);
                M21 = translation.Y;
                //M02 = 0;
                //M12 = 0;
                //M22 = 1;
            }
            get
            {
                return translation;
            }
        }
        public Vector2 Scale
        {
            set
            {
                scale = value;

                M00 = scale.X * MathF.Cos(rotation); M10 =-scale.Y * MathF.Sin(rotation);//M20 = translation.X;
                M01 = scale.X * MathF.Sin(rotation); M11 = scale.Y * MathF.Cos(rotation);//M21 = translation.Y;
                //M02 = 0;//M12 = 0;//M22 = 1;
            }
            get
            {
                return scale;
            }
        }
        // constructors
        public Node2D(float Rotation, Vector2 Scale, Vector2 Translation) : base(
                Scale.X * MathF.Cos(Rotation),-Scale.Y * MathF.Sin(Rotation), Translation.X,
                Scale.X * MathF.Sin(Rotation), Scale.Y * MathF.Cos(Rotation), Translation.Y,
                0, 0, 1)
        {
            // has to extract variables to create matrix from scale, rotation and translation
            scale = Scale;
            rotation = Rotation;
            translation = Translation;
        }
        public Node2D(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
            : base(m00, m01, m02, m10, m11, m12, m20, m21, m22)
        {
            translation = new Vector2(m20, m21);
            rotation = MathF.Atan(m01 / m00);
            scale = new Vector2(m00 / MathF.Cos(rotation), m11 / MathF.Cos(rotation));
        }    
    }
}
