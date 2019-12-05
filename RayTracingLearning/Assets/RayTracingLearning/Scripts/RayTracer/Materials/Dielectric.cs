using RayTracingLearning.RayTracer.Math;

namespace RayTracingLearning.RayTracer.Materials
{
    public class Dielectric : Material
    {
        #region fields
        private float refractRatio;
        #endregion
    
        #region constructors
        public Dielectric(Color albedo, float refractRatio) : base(albedo)
        {
            this.refractRatio = refractRatio;
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
        
        private bool Refract(Ray rayIn, HitInfo hitInfo, out Ray rayOut)
        {
            float cosInAngle = Vector3.Dot(rayIn.Direction, -1 * hitInfo.Normal);
            float squaredSinInAngle = 1 - cosInAngle * cosInAngle;
            float squaredCosIOutAngle = 1 - refractRatio * refractRatio * squaredSinInAngle;

            if (squaredCosIOutAngle > 0) // todo how can be less than 0 ?
            {
                
            }
            
            return false;
        }
        #endregion
    }
}