using System.Collections.Generic;
using UnityEngine;
using Vector3 = RayTracingLearning.RayTracer.Math.Vector3;

namespace RayTracingLearning.RayTracer
{
    public class BVH<T> where  T : IGetAABB
    {
        #region fields
        private BVHNode<T> root;
        #endregion

        #region constructors
        public BVH(List<T> objects)
        {
            root = Build(objects);
        }
        #endregion

        #region methods
        public List<BVHNode<T>> GetIntersectionNode(Ray ray)
        {
            List<BVHNode<T>> resultNodeList = new List<BVHNode<T>>();
            GetIntersectionNode(ray, root, resultNodeList);
            return resultNodeList;
        }
        
        private void GetIntersectionNode(Ray ray, BVHNode<T> node, List<BVHNode<T>> resultNodeList)
        {
            if (node.Left == null && node.Right == null)
            {
                resultNodeList.Add(node);
                return;
            }

            if (node.Left != null && node.Left.AABB.GetIntersection(ray))
            {
                 GetIntersectionNode(ray, node.Left, resultNodeList);
            }

            if (node.Right != null && node.Right.AABB.GetIntersection(ray))
            {
                GetIntersectionNode(ray, node.Right, resultNodeList);
            }
        }

        private BVHNode<T> Build(List<T> objects)
        {
            BVHNode<T> node;
            if (objects.Count == 1)
            {
                T obj = objects[0];
                node = new BVHNode<T>(obj.GetAABB(), obj, null, null);;
                return node;
            }

            if (objects.Count == 2)
            {
                node = new BVHNode<T>();
                node.Left = Build(objects.GetRange(0, 1));
                node.Right = Build(objects.GetRange(1, 1));
                node.AABB = AABB.Union(node.Left.AABB, node.Right.AABB);
                return node;
            }

            AABB aabb = new AABB(new Vector3(0), new Vector3(0));

            foreach (T obj in objects)
            {
                aabb = AABB.Union(aabb, obj.GetAABB());
            }

            AABB.Axis maxAixs = aabb.GetMaxAxis();

            switch (maxAixs)
            {
                case AABB.Axis.X:
                    objects.Sort((aabb1, aabb2) =>
                        {
                            return aabb1.GetAABB().GetCenter().X < aabb2.GetAABB().GetCenter().X ? -1 : 1;
                        });
                    break;
                case AABB.Axis.Y:
                    objects.Sort((aabb1, aabb2) =>
                    {
                        return aabb1.GetAABB().GetCenter().Y < aabb2.GetAABB().GetCenter().Y ? -1 : 1;
                    });
                    break;
                case AABB.Axis.Z:
                    objects.Sort((aabb1, aabb2) =>
                    {
                        return aabb1.GetAABB().GetCenter().Z < aabb2.GetAABB().GetCenter().Z ? -1 : 1;
                    });
                    break;
            }

            List<T> leftObjects = objects.GetRange(0, objects.Count / 2);
            List<T> rightObjects = objects.GetRange(leftObjects.Count, objects.Count - leftObjects.Count);
            node = new BVHNode<T>();
            node.Left = Build(leftObjects);
            node.Right = Build(rightObjects);
            node.AABB = aabb;
            return node;
        }
        #endregion
    }
}