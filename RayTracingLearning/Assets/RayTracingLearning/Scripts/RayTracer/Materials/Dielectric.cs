using System;
using RayTracingLearning.RayTracer.Math;

namespace RayTracingLearning.RayTracer.Materials
{
    public class Dielectric : Material
    {
        #region fields
        private float refractiveIndex; // refractRatio means [out refractive index] / [in refractive index]
        #endregion
    
        #region constructors
        public Dielectric(Color albedo, float fuzziness, float refractiveIndex) : base(albedo, fuzziness)
        {
            if (refractiveIndex <= 0)
            {
                throw new Exception("refraceive Index must be greater than 0");
            }
            
            this.refractiveIndex = refractiveIndex;
        }
        #endregion

        #region methods
        public override Color GetAttenuation(Ray rayIn, HitInfo hitInfo)
        {
            return albedo;
        }

        public override bool GetScatteredRay(Ray rayIn, HitInfo hitInfo, out Ray rayOut)
        {
            Vector3 outwardNormal; // normal of rayIn side
            float refractiveIndexInOverOut;
            float cosIn = Vector3.Dot(rayIn.Direction, hitInfo.Normal);

            if (cosIn > 0) // rayIn come from inside to outside
            {
                outwardNormal = -1 * hitInfo.Normal;
                refractiveIndexInOverOut = refractiveIndex;
                cosIn = -cosIn;
            }

            else // rayIn come from outside to inside
            {
                outwardNormal = hitInfo.Normal;
                refractiveIndexInOverOut = 1 / refractiveIndex;
            } // can not be 0, because intersect only one point will be discarded in hit step

            float squaredSinOut = refractiveIndexInOverOut * refractiveIndexInOverOut * (1 - cosIn * cosIn);
            float discriminant = 1 - squaredSinOut;

            if (discriminant > 0) // means out angle exist
            {
                Vector3 refracted = refractiveIndexInOverOut * (rayIn.Direction - outwardNormal * cosIn) - outwardNormal * (float)System.Math.Sqrt(discriminant);
                rayOut = new Ray(hitInfo.HitPoint, refracted);
                return true;
            }

            rayOut = default(Ray);
            return false;
        }
        #endregion
    }
}