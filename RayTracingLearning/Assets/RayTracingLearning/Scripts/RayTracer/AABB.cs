using RayTracingLearning.RayTracer.Geometries;
using RayTracingLearning.RayTracer.Math;

namespace RayTracingLearning.RayTracer
{
    public struct AABB
    {
        #region enums
        public enum Axis
        {
            X,
            Y,
            Z,
        }
        #endregion
        
        #region fields
        public Vector3 Min;
        public Vector3 Max;
        #endregion
        
        #region constructors
        public AABB(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;
        }
        #endregion
        
        #region methods
        public bool GetIntersection(Ray ray)
        {
            Vector3 origin = ray.Origin;
            Vector3 direction = ray.Direction;
            float xEnter, xExit, yEnter, yExit, zEnter, zExit;

            if (direction.X > 0)
            {
                xEnter = (Min.X - origin.X) / direction.X;
                xExit = (Max.X - origin.X) / direction.X;
            }

            else
            {
                xExit = (Min.X - origin.X) / direction.X;
                xEnter = (Max.X - origin.X) / direction.X;
            }

            if (direction.Y > 0)
            {
                yEnter = (Min.Y - origin.Y) / direction.Y;
                yExit = (Max.Y - origin.Y) / direction.Y;
            }

            else
            {
                yExit = (Min.Y - origin.Y) / direction.Y;
                yEnter = (Max.Y - origin.Y) / direction.Y;
            }

            if (direction.Z > 0)
            {
                zEnter = (Min.Z - origin.Z) / direction.Z;
                zExit = (Max.Z - origin.Z) / direction.Z;
            }

            else
            {
                zExit = (Min.Z - origin.Z) / direction.Z;
                zEnter = (Max.Z - origin.Z) / direction.Z;
            }

            float tEnter = xEnter > yEnter ? (xEnter > zEnter ? xEnter : zEnter) : (yEnter > zEnter ? yEnter : zEnter);
            float tExit = xExit < yExit ? (xExit< zExit ? xExit : zExit) : (yExit < zExit ? yExit : zExit);
            return tEnter < tExit && tExit > 0;
        }

        public Axis GetMaxAxis()
        {
            Vector3 distance = Max - Min;
            return distance.X > distance.Y ? (distance.X > distance.Z ? Axis.X : Axis.Z) : (distance.Y > distance.Z ? Axis.Y : Axis.Z);
        }

        public Vector3 GetCenter()
        {
            return (Min + Max) / 2;
        }

        public static AABB Union(AABB aabb1, AABB aabb2)
        {
            AABB aabb = new AABB();
            aabb.Min = Vector3.Min(aabb1.Min, aabb2.Min);
            aabb.Max = Vector3.Max(aabb1.Max, aabb2.Max);
            return aabb;
        }
        #endregion
    }
}