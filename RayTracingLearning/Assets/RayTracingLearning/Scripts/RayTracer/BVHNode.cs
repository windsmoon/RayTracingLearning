using RayTracingLearning.RayTracer.Geometries;
using UnityEngine;

namespace RayTracingLearning.RayTracer
{
    public class BVHNode<T> where T : IGetAABB
    {
        #region fields
        private BVHNode<T> left;
        private BVHNode<T> right;
        private AABB aabb;
        private T value;
        #endregion

        #region constructors
        public BVHNode(AABB aabb, T value, BVHNode<T> left, BVHNode<T> right)
        {
            this.value = value;
            this.left = left;
            this.right = right;
            this.aabb = aabb;
        }

        public BVHNode()
        {
        }
        #endregion
        
        #region Properties
        public BVHNode<T> Left
        {
            get => left;
            set => left = value;
        }

        public BVHNode<T> Right
        {
            get => right;
            set => right = value;
        }

        public AABB AABB
        {
            get => aabb;
            set => aabb = value;
        }
        
        public T Value
        {
            get => value;
            set => this.value = value;
        }
        #endregion
    }
}