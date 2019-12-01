using System;
using System.Collections.Generic;
using RayTracingLearning.RayTracer.Materials;

namespace RayTracingLearning.RayTracer.Geometries
{
    public abstract class Geometry
    {
        #region fields
        private Material material;
        #endregion
        
        #region constructors
        public Geometry(Material material)
        {
            if (material == null)
            {
                throw new Exception("material of a geometry can not be null");
            }
            
            this.material = material;
        }
        #endregion
        
        #region methods
        public abstract bool IsHit(Ray ray);
        public abstract bool GetHitInfo(Ray ray, out HitInfo hitInfo, float tMin, float tMax);

        protected static bool IsValidT(float t, float tMin, float tMax)
        {
            return t > tMin && t < tMax;
        }

        public static bool GetHitInfo(Ray ray, List<Geometry> geometryList, out HitInfo hitInfo)
        {
            float tMax = float.MaxValue;
            bool isHitAnything = false;
            hitInfo = new HitInfo();
            
            foreach (Geometry geometry in geometryList)
            {
                if (geometry.GetHitInfo(ray, out hitInfo, 0.001f, tMax))
                {
                    tMax = hitInfo.DistanceFormRayOrigin;
                    isHitAnything = true;
                }
            }

            return isHitAnything;
        }
        #endregion
    }
}