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
        public Sphere(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }
        #endregion
        
        #region methods
        public override bool IsHit(Ray ray)
        {
            Vector3 oc = ray.Origin - center;
            float a = Vector3.Dot(ray.Direction, ray.Direction);
            float b = 2f * Vector3.Dot(oc, ray.Direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            float discriminant = b * b - 4f * a * c;
            return discriminant > 0;
            return false;
        }
        
        public override bool GetHitInfo(Ray ray, ref HitInfo hitInfo, float tMin, float tMax)
        {
            Vector3 oc = ray.Origin - center;
            float a = Vector3.Dot(ray.Direction, ray.Direction);
            float b = Vector3.Dot(oc, ray.Direction); // eliminated 2
            float c = Vector3.Dot(oc, oc) - radius * radius;
            float discriminant = b * b - a * c; // eliminated 2
            bool isHit = discriminant > 0f;
            
            if (isHit)
            {
                float t = (-b - (float)System.Math.Sqrt(discriminant)) / a; // eliminated 2
                
                if (IsValidT(t, tMin, tMax))
                {
                    hitInfo = new HitInfo();
                    hitInfo.HitPoint = ray.Origin + t * ray.Direction;
                    hitInfo.DistanceFormOriginal = t;
                    hitInfo.Normal = (hitInfo.HitPoint - center) / radius; // do not have to normalized, just use this method
                    return true;
                }
                
                t = (-b + (float)System.Math.Sqrt(discriminant)) / a; // eliminated 2

                if (IsValidT(t, tMin, tMax))
                {
                    hitInfo = new HitInfo();
                    hitInfo.HitPoint = ray.Origin + t * ray.Direction;
                    hitInfo.DistanceFormOriginal = t;
                    hitInfo.Normal = (hitInfo.HitPoint - center) / radius; // do not have to normalized, just use this method
                    return true;
                }
            }
            
            return false;
        }
        #endregion
    }
}