using System.Collections.Generic;

namespace RayTracingLearning.RayTracer.Geometries
{
    public abstract class Geometry
    {
        #region methods
        public abstract bool IsHit(Ray ray);
        public abstract bool GetHitInfo(Ray ray, ref HitInfo hitInfo, float tMin, float tMax);

        protected static bool IsValidT(float t, float tMin, float tMax)
        {
            return t > tMin && t < tMax;
        }

        public static bool GetHitInfo(Ray ray, List<Geometry> geometryList, ref HitInfo hitInfo)
        {
            float tMax = float.MaxValue;
            bool isHitAnything = false;
            
            foreach (Geometry geometry in geometryList)
            {
                if (geometry.GetHitInfo(ray, ref hitInfo, 0, tMax))
                {
                    tMax = hitInfo.DistanceFormOriginal;
                    isHitAnything = true;
                }
            }

            return isHitAnything;
        }
        #endregion
    }
}