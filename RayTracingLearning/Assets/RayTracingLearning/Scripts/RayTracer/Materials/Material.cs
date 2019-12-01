using System.Net.Http.Headers;

namespace RayTracingLearning.RayTracer.Materials
{
    public abstract class Material
    {
        #region fields
        private Color albedo;
        #endregion
        
        #region methods
        public abstract Ray GetScatteredRay(HitInfo hitInfo);
        #endregion
    }
}