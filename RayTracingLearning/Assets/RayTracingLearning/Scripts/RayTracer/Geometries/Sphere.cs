using RayTracingLearning.RayTracer.Materials;
using RayTracingLearning.RayTracer.Math;

namespace RayTracingLearning.RayTracer.Geometries
{
    public class Sphere : Geometry
    {
        #region fields
        private Vector3 center;
        private float radius;
        #endregion

        #region constructors
        public Sphere(Material material, Vector3 center, float radius) : base(material)
        {
            this.center = center;
            this.radius = radius;
        }
        #endregion
        
        #region methods
        public override AABB GetAABB()
        {
            return new AABB(center - new Vector3(radius), center + new Vector3(radius));
        }

        public override bool IsHit(Ray rayIn)
        {
            Vector3 oc = rayIn.Origin - center;
            float a = Vector3.Dot(rayIn.Direction, rayIn.Direction);
            float b = 2f * Vector3.Dot(oc, rayIn.Direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            float discriminant = b * b - 4f * a * c;
            return discriminant > 0;
        }
        
        public override bool GetHitInfo(Ray rayIn, out HitInfo hitInfo, float tMin, float tMax)
        {
            Vector3 oc = rayIn.Origin - center;
            float a = Vector3.Dot(rayIn.Direction, rayIn.Direction);
            float b = Vector3.Dot(oc, rayIn.Direction); // eliminated 2
            float c = Vector3.Dot(oc, oc) - radius * radius;
            float discriminant = b * b - a * c; // eliminated 2
            bool isHit = discriminant > 0f;
            
            if (isHit)
            {
                float t = (-b - (float)System.Math.Sqrt(discriminant)) / a; // eliminated 2
                
                if (IsValidT(t, tMin, tMax))
                {
                    hitInfo = new HitInfo();
                    hitInfo.HitPoint = rayIn.Origin + t * rayIn.Direction;
                    hitInfo.DistanceFormRayOrigin = t;
                    hitInfo.Normal = (hitInfo.HitPoint - center) / radius; // do not have to normalized, just use this method
                    hitInfo.Geometry = this;
                    hitInfo.RayIn = rayIn;
                    
                    if (material.TryGetScatteredRay(rayIn, hitInfo, out Ray rayOut))
                    {
                        hitInfo.Attenuation = material.GetAttenuation(rayIn, hitInfo);
                        hitInfo.hasReflectedRay = true;
                        hitInfo.RayReflected = rayOut;
                    }

                    else
                    {
                        hitInfo.hasReflectedRay = true;
                    }
                    
                    return true;
                }
                
                t = (-b + (float)System.Math.Sqrt(discriminant)) / a; // eliminated 2

                if (IsValidT(t, tMin, tMax))
                {
                    hitInfo = new HitInfo();
                    hitInfo.HitPoint = rayIn.Origin + t * rayIn.Direction;
                    hitInfo.DistanceFormRayOrigin = t;
                    hitInfo.Normal = (hitInfo.HitPoint - center) / radius; // do not have to normalized, just use this method
                    hitInfo.Geometry = this;
                    hitInfo.RayIn = rayIn;
                    
                    if (material.TryGetScatteredRay(rayIn, hitInfo, out Ray rayOut))
                    {
                        hitInfo.Attenuation = material.GetAttenuation(rayIn, hitInfo);
                        hitInfo.hasReflectedRay = true;
                        hitInfo.RayReflected = rayOut;
                    }

                    else
                    {
                        hitInfo.hasReflectedRay = false;
                    }
                    
                    return true;
                }
            }

            hitInfo = default(HitInfo);
            return false;
        }
        #endregion
    }
}