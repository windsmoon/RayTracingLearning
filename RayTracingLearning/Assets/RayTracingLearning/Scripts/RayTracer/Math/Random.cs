using System;

namespace RayTracingLearning.RayTracer.Math
{
    public static class RandomUtility
    {
        #region fields
        private static System.Random random;
        #endregion

        #region constructors
        static RandomUtility()
        {
            random = new Random();
        }
        #endregion
        
        #region methods
        public static Vector3 RandomInSphere(float radius)
        {
            Vector3 direction = new Vector3(Random01(), Random01(), Random01());
            direction = direction * 2 - new Vector3(-1f, -1f, -1f);
            direction.Normalize();
            return direction * Random(0, radius);
        }

        public static float Random01()
        {
            return (float)random.NextDouble();
        }

        public static float Random(float min, float max)
        {
            float randomValue = (float)random.NextDouble();
            return (1 - randomValue) * min + max * randomValue;
        }
        #endregion
    }
}