using UnityEngine;

namespace RayTracingLearning
{
    public struct HitInfo
    {
        #region fields
        public float DistanceFormOriginal;
        public Vector3 HitPoint;
        public Vector3 Normal;
        #endregion
    }
}