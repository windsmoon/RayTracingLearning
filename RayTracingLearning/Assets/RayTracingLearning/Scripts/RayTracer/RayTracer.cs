using RayTracingLearning.RayTracer.Geometries;
using System.Collections.Generic;

namespace RayTracingLearning.RayTracer
{
    public class RayTracer
    {
        #region fieldss
        public static int Counter;
        #endregion
        
        #region methods
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

            Counter += geometryList.Count;
            
            hitInfo = tempInfo;
            return isHitAnything;
        }

        public static bool GetHitInfo(Ray rayIn, BVH<Geometry> bvh, out HitInfo hitInfo)
        {
            float tMax = float.MaxValue;
            bool isHitAnything = false;
            HitInfo tempInfo = default(HitInfo);
            List<BVHNode<Geometry>> nodeList = bvh.GetIntersectionNode(rayIn);
            
            foreach (BVHNode<Geometry> node in nodeList)
            {
                if (node.Value.GetHitInfo(rayIn, out hitInfo, 0.001f, tMax))
                {
                    tMax = hitInfo.DistanceFormRayOrigin;
                    isHitAnything = true;
                    tempInfo = hitInfo;
                }
            }
            
            Counter += nodeList.Count;
            hitInfo = tempInfo;
            return isHitAnything;
        }
        #endregion
    }
}