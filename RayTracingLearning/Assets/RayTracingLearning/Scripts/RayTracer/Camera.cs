using RayTracingLearning.RayTracer.Math;

namespace RayTracingLearning.RayTracer
{
    public class Camera
    {
        #region fields
        private Vector3 origin;
//        private float horizontalLength;
//        private float verticalLength;
        private float depthLength;
        private Vector3 lowLeftDepthCorner;
        private float verticlaFOV;
        private Vector3 positiveXLengthVector;
        private Vector3 positiveYLengthVector;
        #endregion
        
        #region constructors
        public Camera(Vector3 lookFrom, Vector3 lookAt, float verticlaFOV, float aspect)
        {
            this.origin = lookFrom;
            Vector3 positiveZ = (lookAt - lookFrom).GetNormalizedVector();
            Vector3 positiveX;
            
            if (positiveZ == new Vector3(0f, -1f, -0f))
            {
                positiveX = Vector3.Cross(new Vector3(0f, 0f, 1f), positiveZ).GetNormalizedVector();
            }

            else
            {
                positiveX = Vector3.Cross(new Vector3(0f, 1f, 0f), positiveZ).GetNormalizedVector();
            }
            
            Vector3 positiveY = Vector3.Cross(positiveZ, positiveX); // no need to normalize, because z and x are all normalized and the angle between them is 90
            this.verticlaFOV = (float)System.Math.PI * verticlaFOV / 180f;
            float halfHeight = (float)System.Math.Tan(this.verticlaFOV / 2);
            float halfWidth = halfHeight * aspect;
            this.lowLeftDepthCorner = this.origin - halfWidth * positiveX - halfHeight * positiveY + positiveZ;
            this.positiveXLengthVector = 2f * halfWidth * positiveX;
            this.positiveYLengthVector = 2f * halfHeight * positiveY;
//            this.horizontalLength = 2f * halfWidth;
//            this.verticalLength = 2f * halfHeight;
//            this.origin = new Vector3(0f, 0f, 0f);
        }
        #endregion

        #region methods
        public Ray GetRay(float u, float v)
        {
            Vector3 targetPoint = lowLeftDepthCorner + u * positiveXLengthVector + v * positiveYLengthVector;
            Ray ray = new Ray(origin, targetPoint - origin);
            return ray;
        }
        #endregion
    }
}