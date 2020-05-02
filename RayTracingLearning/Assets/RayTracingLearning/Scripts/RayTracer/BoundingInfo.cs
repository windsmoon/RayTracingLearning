using RayTracingLearning.RayTracer.Geometries;

namespace RayTracingLearning.RayTracer
{
    public struct BoundingInfo
    {
        #region fields
        public AABB AABB;
        public Geometry Geometry;
        #endregion
    }
}