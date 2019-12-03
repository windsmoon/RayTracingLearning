using RayTracingLearning.RayTracer.Math;

namespace RayTracingLearning.RayTracer.Materials
{
    public class Metal : Material
    {
        #region fields
        private float fuzziness;
        #endregion
        
        #region constructors
        public Metal(Color albedo, float fuzziness ) : base(albedo)
        {
            this.fuzziness = fuzziness < 1f ? fuzziness : 1f;
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

            Vector3 reflectedDirection = rayInDirection - 2 * Vector3.Dot(rayInDirection, hitNormal) * hitNormal;
            rayOut = new Ray(hitInfo.HitPoint, reflectedDirection + fuzziness * RandomUtility.RandomInSphere(1f));
            return true;
        }
        #endregion
    }
}