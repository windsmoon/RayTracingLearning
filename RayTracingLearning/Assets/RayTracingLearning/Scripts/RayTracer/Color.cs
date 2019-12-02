// ReSharper disable All
namespace RayTracingLearning.RayTracer
{
    public struct Color
    {
        #region fields
        public float R;
        public float G;
        public float B;
        public float A;
        #endregion
        
        #region constructors
        public Color(float r, float g, float b, float a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public Color(float r, float g, float b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = 0f;
        }
        #endregion

        #region methods
        public static Color operator +(Color value1, Color value2)
        {
            return new Color(value1.R + value2.R, value1.G + value2.G, value1.B + value2.B);
        }

        public static Color operator -(Color value1, Color value2)
        {
            return new Color(value1.R - value2.R, value1.G - value2.G, value1.B - value2.B); 
        }

        public static Color operator *(Color value1, Color value2)
        {
            return new Color(value1.R * value2.R, value1.G * value2.G, value1.B * value2.B); 
        }
        
        public static Color operator /(Color value1, Color value2)
        {
            return new Color(value1.R / value2.R, value1.G / value2.G, value1.B / value2.B); 
        }
        
        public static Color operator *(Color value1, float scale)
        {
            return new Color(value1.R* scale, value1.G * scale, value1.B * scale); 
        }
        
        public static Color operator *(float scale, Color value1)
        {
            return new Color(value1.R * scale, value1.G * scale, value1.B * scale); 
        }
        
        public static Color operator /(Color value1, float scale)
        {
            return new Color(value1.R / scale, value1.G / scale, value1.B / scale); 
        }
        
        public static Color operator /(float scale, Color value1)
        {
            return new Color(value1.R / scale, value1.G / scale, value1.B / scale); 
        }

        public static Color Lerp(Color start, Color end, float t)
        {
            return start * (1 - t) + end * t;
        }
        #endregion
    }
}