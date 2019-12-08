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

        public override bool TryGetScatteredRay(Ray rayIn, HitInfo hitInfo, out Ray rayOut)
        {
            Vector3 outwardNormal; // normal of rayIn side
            float refractiveIndexInOverOut;
            float cosIn = Vector3.Dot(rayIn.Direction, hitInfo.Normal);
            float cosine;
            
            if (cosIn > 0) // rayIn come from inside to outside
            {
                outwardNormal = -1 * hitInfo.Normal;
                refractiveIndexInOverOut = refractiveIndex;
                cosIn = -cosIn;
                
                cosine = refractiveIndex * cosIn / rayIn.Direction.GetLength();
            }

            else // rayIn come from outside to inside
            {
                outwardNormal = hitInfo.Normal;
                refractiveIndexInOverOut = 1 / refractiveIndex;
                
                cosine = -cosIn / rayIn.Direction.GetLength();
            } // can not be 0, because intersect only one point will be discarded in hit step

            if (GetRefraciveDirection(cosIn, refractiveIndexInOverOut, rayIn.Direction, outwardNormal, out Vector3 refractiveDirection))
            {
                rayOut = new Ray(hitInfo.HitPoint, refractiveDirection);
                return true;
            }
            
            if (TryGetReflectedVector(rayIn, hitInfo, out Vector3 reflectedVector))
            {   
                rayOut = new Ray(hitInfo.HitPoint, reflectedVector + fuzziness * RandomUtility.RandomInSphere(1f));
                return true;
            }

            rayOut = default(Ray);
            return false;
        }

        private bool GetRefraciveDirection(float cosIn, float refractiveIndexInOverOut, Vector3 inDirection, Vector3 outwardNormal, out Vector3 refractiveDirection)
        {
            float squaredSinOut = refractiveIndexInOverOut * refractiveIndexInOverOut * (1 - cosIn * cosIn);
            float discriminant = 1 - squaredSinOut;

            if (discriminant > 0) // means out angle exist
            {
                refractiveDirection = refractiveIndexInOverOut * (inDirection - outwardNormal * cosIn) - outwardNormal * (float)System.Math.Sqrt(discriminant);
                return true;
            }

            refractiveDirection = default(Vector3);
            return false;
        }
        
        private float Schlick(float cos, float refractiveIndex)
        {
            float r0 = (1 - refractiveIndex) / (1 + refractiveIndex);
            r0 = r0 * r0;
            return r0 + (1 - r0) * (float)System.Math.Pow((1 - cos), 5);
        }
        #endregion
    }
}