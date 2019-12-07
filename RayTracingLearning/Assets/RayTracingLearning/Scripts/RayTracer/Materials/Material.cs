using RayTracingLearning.RayTracer.Math;

namespace RayTracingLearning.RayTracer.Materials
{
    public abstract class Material
    {
        #region fields
        protected Color albedo;
        protected float fuzziness;
        #endregion
        
        #region constructors
        public Material(Color albedo, float fuzziness)
        {
            this.albedo = albedo;
            this.fuzziness = fuzziness < 1f ? fuzziness : 1f;
        }
        #endregion
        
        #region methods
        public abstract bool GetScatteredRay(Ray rayIn, HitInfo hitInfo, out Ray rayOut);
        public abstract Color GetAttenuation(Ray rayIn, HitInfo hitInfo);

        public bool GetReflectedVector(Ray rayIn, HitInfo hitInfo, out Vector3 rayOutVector)
        {
            Vector3 rayInDirection = rayIn.Direction;
            Vector3 hitNormal = hitInfo.Normal;

            if (Vector3.Dot(rayInDirection, hitNormal) > 0)
            {
                rayOutVector = default(Vector3);
                return false;
            }

            rayOutVector = rayInDirection - 2 * Vector3.Dot(rayInDirection, hitNormal) * hitNormal;
            return true;
        }
        #endregion
    }
}