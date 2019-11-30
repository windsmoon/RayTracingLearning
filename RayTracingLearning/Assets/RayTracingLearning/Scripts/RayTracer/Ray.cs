using UnityEngine;

namespace RayTracingLearning.RayTracer
{
    public struct Ray
    {
        #region fields
        private Vector3 origin;
        private Vector3 direction;
        #endregion
        
        #region properties

        public Vector3 Origin
        {
            get { return origin; }
        }

        public Vector3 Direction
        {
            get { return direction; }
        }
        #endregion
        
        #region constructors
        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction.normalized;
        }
        #endregion
        
        #region methods
        public Vector3 GetPointInRay(float t)
        {
            return origin + t * direction;
        }
        #endregion
    }
}

