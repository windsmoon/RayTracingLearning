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
        public Ray RayIn;
        public Geometry Geometry;
        public Color Attenuation;
        public Ray RayReflected;
        public bool hasReflectedRay;
        #endregion
    }
}