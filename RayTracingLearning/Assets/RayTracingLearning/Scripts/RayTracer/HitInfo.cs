using RayTracingLearning.RayTracer.Geometries;
using RayTracingLearning.RayTracer.Math;

namespace RayTracingLearning.RayTracer
{
    public struct HitInfo
    {
        #region fields
        public float DistanceFormRayOrigin;
        public Vector3 HitPoint;
        public Vector3 Normal;
        public Geometry Geometry;
        #endregion

        #region constructors
        public HitInfo(float distanceFormRayOrigin, Vector3 hitPoint, Vector3 normal, Geometry geometry)
        {
            this.DistanceFormRayOrigin = distanceFormRayOrigin;
            this.HitPoint = hitPoint;
            this.Normal = normal;
            this.Geometry = geometry;
        }
        #endregion
    }
}