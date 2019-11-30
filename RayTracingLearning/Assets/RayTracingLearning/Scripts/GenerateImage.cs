using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RayTracingLearning.RayTracer;
using RayTracingLearning.RayTracer.Geometries;
using Ray = RayTracingLearning.RayTracer.Ray;
using Utility = RayTracingLearning.RayTracer.Utility;
using Vector3 = RayTracingLearning.RayTracer.Math.Vector3;
using Camera = RayTracingLearning.RayTracer.Camera;
using Random = System.Random;

namespace RayTracingLearning
{
    public class GenerateImage : MonoBehaviour
    {
        #region fields
        [SerializeField] 
        private bool isUseAA = false;
        [SerializeField]
        private int aaSampleCount = 100;
        #endregion
        
        #region methods
        [ContextMenu("Generate Image")]
        private void Generate()
        {
            // build the world, width : 200, height : 100, depth : 100
            Texture2D texture = new Texture2D(200, 100);
            //Vector3 horizontalLength = new Vector3(4, 0, 0); // width : 200
            //Vector3 verticalLength = new Vector3(0, 2, 0); // height : 100
            //Vector3 center = new Vector3(0, 0, 0);
            //Vector3 lowLeftCorner = new Vector3(-2, -1, 1); // depth : 50, z : 1
            Camera camera = new Camera(new Vector3(0f, 0f, 0f), 4f, 2f, 1f);

            for (int i = 0; i < texture.width; ++i)
            {
                for (int j = texture.height - 1; j >= 0; --j)
                {
                    Color color = GetColor(camera, texture, i, j);
                    texture.SetPixel(i, j, color);
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply();
            GetComponent<RawImage>().texture = texture;
        }

        private Color GetColor(Camera camera, Texture texture, int x, int y)
        {
            if (isUseAA == false)
            {
                float u = (float) x / texture.width;
                float v = (float) y / texture.height;
                Ray ray = camera.GetRay(u, v);
                return GetColorImpl(ray);
            }

            Color color = new Color(0f, 0f, 0f);
            Random random = new Random();
            
            for (int i = 0; i < aaSampleCount; ++i)
            {
                float randomU = (float) (x + random.Next(0, 100) * 0.01f) / texture.width;
                float randomV = (float) (y + random.Next(0, 100) * 0.01f) / texture.height;
                Ray offsetRay = camera.GetRay(randomU, randomV);
                Color offsetColor = GetColorImpl(offsetRay);
                color += offsetColor;
            }

            return color / aaSampleCount;
        }

        private Color GetColorImpl(Ray ray)
        {
            //return GetColorForBackground(ray);
            //return GetColorForSphere(ray, new Vector3(0f, 0f, 1f), 0.5f);
            //return GetNormalColorForSphere(ray, new Vector3(0f, 0f, 1f), 0.5f);
            return GetTwoSphereNormalColorForSphere(ray);
        }

        private Color GetColorForBackground(Ray ray)
        {
            float t = 0.5f * (ray.Direction.Y + 1); // -1 ~ 1 to 0 ~ 1
            return Color.Lerp(new Color(1f, 1f, 1f), new Color(0.5f, 0.7f, 1.0f), t);
        }

        private Color GetColorForSphere(Ray ray, Vector3 center, float radius)
        {
            if (Utility.IsRayHitSphere(ray, center, radius))
            {
                return Color.red;
            }

            return GetColorForBackground(ray);
        }

        private Color GetNormalColorForSphere(Ray ray, Vector3 center, float radius)
        {
            HitInfo hitInfo = new HitInfo();
            Sphere sphere = new Sphere(center, radius);
            
            if (sphere.GetHitInfo(ray, ref hitInfo, 0, float.MaxValue))
            {
                Vector3 normal = hitInfo.Normal;
                Vector3 colorVector = (normal + new Vector3(1f, 1f, 1f)) * 0.5f;
                return new Color(colorVector.X, colorVector.Y, colorVector.Z);
            }

            return GetColorForBackground(ray);
        }

        private Color GetTwoSphereNormalColorForSphere(Ray ray)
        {
            HitInfo hitInfo = new HitInfo();
            Sphere sphere1 = new Sphere(new Vector3(0, 0, 1f), 0.5f);
            Sphere sphere2 = new Sphere(new Vector3(0,-100.5f,1f), 100f);
            
            if (Geometry.GetHitInfo(ray, new List<Geometry>() {sphere1, sphere2}, ref hitInfo))
            {
                Vector3 normal = hitInfo.Normal;
                Vector3 colorVector = (normal + new Vector3(1f, 1f, 1f)) * 0.5f;
                return new Color(colorVector.X, colorVector.Y, colorVector.Z);
            }
            
            return GetColorForBackground(ray);
        }
        #endregion
    }
}