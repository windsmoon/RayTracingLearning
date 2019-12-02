using System.Net.Http.Headers;

namespace RayTracingLearning.RayTracer.Materials
{
    public abstract class Material
    {
        #region fields
        protected Color albedo;
        #endregion
        
        #region constructors
        public Material(Color albedo)
        {
            this.albedo = albedo;
        }
        #endregion
        
        #region methods
        public abstract Ray GetReflectedRay(Ray rayIn, HitInfo hitInfo);
        public abstract Color GetAttenuation(Ray rayIn, HitInfo hitInfo);
        #endregion
    }
}