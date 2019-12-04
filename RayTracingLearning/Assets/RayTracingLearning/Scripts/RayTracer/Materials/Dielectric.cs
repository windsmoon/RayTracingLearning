namespace RayTracingLearning.RayTracer.Materials
{
    public class Dielectric : Material
    {
        #region constructors
        public Dielectric(Color albedo) : base(albedo)
        {
        }
        #endregion

        #region methods
        public override Color GetAttenuation(Ray rayIn, HitInfo hitInfo)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetReflectedRay(Ray rayIn, HitInfo hitInfo, out Ray rayOut)
        {
            throw new System.NotImplementedException();
        }
        
        private bool Refract(Ray rayIn)
        {
            return false;
        }
        #endregion
    }
}