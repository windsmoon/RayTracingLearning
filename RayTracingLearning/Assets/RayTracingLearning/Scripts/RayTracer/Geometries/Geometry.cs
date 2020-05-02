using System;
using System.Collections.Generic;
using RayTracingLearning.RayTracer.Materials;

namespace RayTracingLearning.RayTracer.Geometries
{
    public abstract class Geometry : IGetAABB
    {
        #region fields
        protected Material material;
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

        #region properties
        public Material Material
        {
            get { return material; }
        }
        #endregion

        #region interfaces
        public abstract AABB GetAABB();
        #endregion
        
        #region methods
        public abstract bool IsHit(Ray rayIn);
        public abstract bool GetHitInfo(Ray rayIn, out HitInfo hitInfo, float tMin, float tMax);

        //public Ray GetReflectedRay(Ray rayIn, HitInfo hitInfo)
        //{
        //    return material.GetReflectedRay(rayIn, hitInfo);
        //}
        
        protected static bool IsValidT(float t, float tMin, float tMax)
        {
            return t > tMin && t < tMax;
        }

        public static bool GetHitInfo(Ray rayIn, List<Geometry> geometryList, out HitInfo hitInfo)
        {
            float tMax = float.MaxValue;
            bool isHitAnything = false;
            HitInfo tempInfo = default(HitInfo);
            
            foreach (Geometry geometry in geometryList)
            {
                if (geometry.GetHitInfo(rayIn, out hitInfo, 0.001f, tMax))
                {
                    tMax = hitInfo.DistanceFormRayOrigin;
                    isHitAnything = true;
                    tempInfo = hitInfo;
                }
            }
            
            hitInfo = tempInfo;
            return isHitAnything;
        }
        #endregion
    }
}