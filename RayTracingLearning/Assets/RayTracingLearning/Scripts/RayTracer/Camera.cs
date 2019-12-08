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
        private float verticlaFOV;
        #endregion
        
        #region constructors
        public Camera(Vector3 lookFrom, Vector3 lookAt, float verticlaFOV, float aspect)
        {
            this.origin = lookFrom;
            Vector3 positiveZ = (lookAt - lookFrom).GetNormalizedVector();
            Vector3 positiveX = Vector3.Cross(new Vector3(0f, 1f, 0f), positiveZ).GetNormalizedVector();
            Vector3 positiveY = Vector3.Cross(positiveZ, positiveX); // no need to normalize, because z and x are all normalized and the angle between them is 90
            this.verticlaFOV = (float)System.Math.PI * verticlaFOV / 180f;
            float halfHeight = (float)System.Math.Tan(this.verticlaFOV / 2);
            float halfWidth = halfHeight * aspect;
            this.lowLeftDepthCorner = this.origin - halfWidth * positiveX - halfHeight * positiveY + positiveZ;
            this.horizontalLength = 2f * halfWidth;
            this.verticalLength = 2f * halfHeight;
//            this.origin = new Vector3(0f, 0f, 0f);
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