using System.Collections.Generic;
using RayTracingLearning.RayTracer.Math;

namespace RayTracingLearning.RayTracer.Materials
{
    public class Lambertian : Material
    {
        #region constructors
        public Lambertian(Color albedo) : base(albedo)
        {
        }
        #endregion
        
        #region methods
        public override bool GetReflectedRay(Ray rayIn, HitInfo hitInfo, out Ray rayOut)
        {
            /*vec3 target = rec.p + rec.normal + random_in_unit_sphere();
            scattered = ray(rec.p, target-rec.p);
            attenuation = albedo;
            return true;*/

            //Vector3 target = hitInfo.HitPoint + hitInfo.Normal + RandomUtility.RandomInSphere(1f);
            //Ray scatteredRay = new Ray(hitInfo.HitPoint, target - hitInfo.HitPoint);
            rayOut = new Ray(hitInfo.HitPoint, hitInfo.Normal + RandomUtility.RandomInSphere(1f)); // combine above two line code
            return true;
        }

        public override Color GetAttenuation(Ray rayIn, HitInfo hitInfo)
        {
            return albedo;
        }

        #endregion
    }
}