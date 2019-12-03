namespace RayTracingLearning.RayTracer.Math
{
    public struct Vector3
    {
        #region fields
        public float X;
        public float Y;
        public float Z;
        #endregion
        
        #region constructors
        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        #endregion

        #region methods
        public static float Dot(Vector3 value1, Vector3 value2)
        {
            return value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
        }

        public static Vector3 Cross(Vector3 value1, Vector3 value2)
        {
               /*eturn vec3(v1.e[1] * v2.e[2] - v1.e[2] * v2.e[1],
                            v1.e[2] * v2.e[0] - v1.e[0] * v2.e[2],
                            v1.e[0] * v2.e[1] - v1.e[1] * v2.e[0]);*/
            return new Vector3(value1.Y * value2.Z - value1.Z * value2.Y, value1.Z * value2.X - value1.X * value2.Z, value1.X * value2.Y - value1.Y * value2.X);
        }

        public static Vector3 operator +(Vector3 value1, Vector3 value2)
        {
            return new Vector3(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z);
        }

        public static Vector3 operator -(Vector3 value1, Vector3 value2)
        {
            return new Vector3(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z); 
        }

        public static Vector3 operator *(Vector3 value1, Vector3 value2)
        {
            return new Vector3(value1.X * value2.X, value1.Y * value2.Y, value1.Z * value2.Z); 
        }
        
        public static Vector3 operator /(Vector3 value1, Vector3 value2)
        {
            return new Vector3(value1.X / value2.X, value1.Y / value2.Y, value1.Z / value2.Z); 
        }
        
        public static Vector3 operator *(Vector3 value1, float scale)
        {
            return new Vector3(value1.X * scale, value1.Y * scale, value1.Z * scale); 
        }
        
        public static Vector3 operator *(float scale, Vector3 value1)
        {
            return new Vector3(value1.X * scale, value1.Y * scale, value1.Z * scale); 
        }
        
        public static Vector3 operator /(Vector3 value1, float scale)
        {
            return new Vector3(value1.X / scale, value1.Y / scale, value1.Z / scale); 
        }
        
        public static Vector3 operator /(float scale, Vector3 value1)
        {
            return new Vector3(value1.X / scale, value1.Y / scale, value1.Z / scale); 
        }

        public void Normalize()
        {
            float lenght = (float) System.Math.Sqrt(X * X + Y * Y + Z * Z);
            X /= lenght;
            Y /= lenght;
            Z /= lenght;
        }

        public Vector3 GetNormalizedVector()
        {
            Vector3 temp = new Vector3(X, Y, Z);
            temp.Normalize();
            return temp;
        }
        
        public float GetSquaredLength()
        {
            return Vector3.Dot(this, this);
        }
        #endregion
    }
}