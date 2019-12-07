using RayTracingLearning.RayTracer.Math;

namespace RayTracingLearning.RayTracer.Materials
{
    public class Metal : Material
    {
        #region fields
        #endregion
        
        #region constructors
        public Metal(Color albedo, float fuzziness) : base(albedo, fuzziness)
        {
        }
        #endregion
        
        #region methods
        public override Color GetAttenuation(Ray rayIn, HitInfo hitInfo)
        {
            return albedo;
        }

        public override bool GetScatteredRay(Ray rayIn, HitInfo hitInfo, out Ray rayOut)
        {
            if (GetReflectedVector(rayIn, hitInfo, out Vector3 reflectedOutVector) == false)
            {
                rayOut = default(Ray);
                return false; 
            }
            
            rayOut = new Ray(hitInfo.HitPoint, reflectedOutVector + fuzziness * RandomUtility.RandomInSphere(1f));
            return true;
        }
        #endregion
    }
}