namespace RayTracingLearning.RayTracer.Geometries
{
    public abstract class Geometry
    {
        #region methods
        public abstract bool IsHit(Ray ray);
        public abstract bool GetHitInfo(Ray ray, out HitInfo hitInfo, float tMin, float tMax);

        protected static bool IsValidT(float t, float tMin, float tMax)
        {
            return t > tMin && t < tMax;
        }
        #endregion
    }
}