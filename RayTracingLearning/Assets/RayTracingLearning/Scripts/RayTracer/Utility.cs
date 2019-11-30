using RayTracingLearning.RayTracer.Math;

namespace RayTracingLearning.RayTracer
{
    public static class Utility
    {
        #region methods
        public static bool IsRayHitSphere(Ray ray, Vector3 center, float radius)
        {
            /*
            vec3 oc = r.origin() - center;
            float a = dot(r.direction(), r.direction());
            float b = 2.0 * dot(oc, r.direction());
            float c = dot(oc, oc) - radius*radius;
            float discriminant = b*b - 4*a*c;
            return (discriminant > 0);0
            */
            Vector3 oc = ray.Origin - center;
            float a = Vector3.Dot(ray.Direction, ray.Direction);
            float b = 2f * Vector3.Dot(oc, ray.Direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            float discriminant = b * b - 4f * a * c;
            return discriminant > 0;
        }

        public static bool GetRaySphereHitPoint(Ray ray, Vector3 center, float radius, out Vector3 hitPoint)
        {
            Vector3 oc = ray.Origin - center;
            float a = Vector3.Dot(ray.Direction, ray.Direction);
            float b = 2f * Vector3.Dot(oc, ray.Direction);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            float discriminant = b * b - 4f * a * c;
            bool isHit = discriminant > 0f;

            if (isHit)
            {
                float t = (-b - (float)System.Math.Sqrt(discriminant)) / (2f * a);
                hitPoint = ray.Origin + t * ray.Direction;
            }

            else
            {
                hitPoint = new Vector3();
            }

            return isHit;
        }
        #endregion
    }
}