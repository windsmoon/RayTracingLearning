using RayTracingLearning.RayTracer.Math;

namespace RayTracingLearning.RayTracer
{
    public class Camera
    {
        #region fields
        private Vector3 origin;
        private float horizontalLength;
        private float verticalLength;
        private float depthLength;
        private Vector3 lowLeftDepthCorner;
        #endregion
        
        #region constructors
        public Camera(Vector3 origin, float horizontalLength, float verticalLength, float depthLength)
        {
            this.origin = origin;
            this.horizontalLength = horizontalLength;
            this.verticalLength = verticalLength;
            this.depthLength = depthLength;
            lowLeftDepthCorner = origin + new Vector3(-horizontalLength * 0.5f, -verticalLength * 0.5f, depthLength);
        }
        #endregion

        #region methods
        public Ray GetRay(float u, float v)
        {
            Vector3 targetPoint = lowLeftDepthCorner + new Vector3(u * horizontalLength, v * verticalLength, 0);
            Ray ray = new Ray(origin, targetPoint - origin);
            return ray;
        }
        #endregion
    }
}