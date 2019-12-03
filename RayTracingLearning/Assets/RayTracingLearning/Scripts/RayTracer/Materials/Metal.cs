using RayTracingLearning.RayTracer.Math;

namespace RayTracingLearning.RayTracer.Materials
{
    public class Metal : Material
    {
        #region constructors
        public Metal(Color albedo) : base(albedo)
        {
        }
        #endregion
        
        #region methods
        public override Color GetAttenuation(Ray rayIn, HitInfo hitInfo)
        {
            return albedo;
        }

        public override bool GetReflectedRay(Ray rayIn, HitInfo hitInfo, out Ray rayOut)
        {
            Vector3 rayInDirection = rayIn.Direction;
            Vector3 hitNormal = hitInfo.Normal;

            if (Vector3.Dot(rayInDirection, hitNormal) > 0)
            {
                rayOut = default(Ray);
                return false;
            }
            
            rayOut = new Ray(hitInfo.HitPoint, rayInDirection - 2 * Vector3.Dot(rayInDirection, hitNormal) * hitNormal);
            return true;
        }
        #endregion
    }
}