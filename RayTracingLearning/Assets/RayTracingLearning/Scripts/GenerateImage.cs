using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RayTracingLearning.RayTracer;
using RayTracingLearning.RayTracer.Geometries;
using RayTracingLearning.RayTracer.Materials;
using RayTracingLearning.RayTracer.Math;
using UnityEditor;
using Ray = RayTracingLearning.RayTracer.Ray;
using Vector3 = RayTracingLearning.RayTracer.Math.Vector3;
using Camera = RayTracingLearning.RayTracer.Camera;
using Random = System.Random;
using Color = RayTracingLearning.RayTracer.Color;
using Material = RayTracingLearning.RayTracer.Materials.Material;

namespace RayTracingLearning
{
    public class GenerateImage : MonoBehaviour
    {
        #region fields
        [SerializeField] 
        private bool isUseAA = false;
        [SerializeField]
        private int aaSampleCount = 100;
        [SerializeField]
        private int maxReflectCount = 50;
        [SerializeField, Range(0f, 1f)]
        private float globalMetalFuzziness = 1f;
        private Sphere sphere1;
        private Sphere sphere2;
        private Sphere sphere3;
        private Sphere sphere4;
        private List<Geometry> sphereList;
        #endregion
        
        #region methods
        [ContextMenu("Generate Image")]
        private void Generate()
        {
            // build the world, width : 200, height : 100, depth : 100
            Texture2D texture = new Texture2D(1920, 1080);
            //Vector3 horizontalLength = new Vector3(4, 0, 0); // width : 200
            //Vector3 verticalLength = new Vector3(0, 2, 0); // height : 100
            //Vector3 center = new Vector3(0, 0, 0);
            //Vector3 lowLeftCorner = new Vector3(-2, -1, 1); // depth : 50, z : 1
            Camera camera = new Camera(new Vector3(0f, 0f, 0f), 4f, 2f, 1f);
            Material lambertian1 = new Lambertian(new Color(0.8f, 0.3f, 0.3f));
            Material lambertian2 = new Lambertian(new Color(0.8f, 0.8f, 0f));
            Material metal1 = new Metal(new Color(0.8f, 0.6f, 0.2f), 1f * globalMetalFuzziness);
            Material metal2 = new Metal(new Color(0.8f, 0.8f, 0.8f), 0.3f * globalMetalFuzziness);
            sphere1 = new Sphere(lambertian1, new Vector3(0f, 0f, 1f), 0.5f);
            sphere2 = new Sphere(lambertian2, new Vector3(0f,-100.5f,1f), 100f);
            sphere3 = new Sphere(metal1, new Vector3(1f,0f,1f), 0.5f);
            sphere4 = new Sphere(metal2, new Vector3(-1,0,1f), 0.5f);
            sphereList = new List<Geometry>() {sphere1, sphere2, sphere3, sphere4};
            
            for (int i = 0; i < texture.width; ++i)
            {
                for (int j = texture.height - 1; j >= 0; --j)
                {
                    Color color = GetColor(camera, texture, i, j);
                    color.R = (float)Math.Sqrt(color.R);
                    color.G = (float)Math.Sqrt(color.G);
                    color.B = (float)Math.Sqrt(color.B);
                    texture.SetPixel(i, j, new UnityEngine.Color(color.R, color.G, color.B, 1f));
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
            //return GetTwoSphereNormalColorForSphere(ray);
            return GetTwoSphereDiffuse(ray, 0);
        }

        private Color GetColorForBackground(Ray ray)
        {
            float t = 0.5f * (ray.Direction.Y + 1); // -1 ~ 1 to 0 ~ 1
            return Color.Lerp(new Color(1f, 1f, 1f), new Color(0.5f, 0.7f, 1.0f), t);
        }

        private Color GetColorForSphere(Ray ray, Vector3 center, float radius)
        {
            Sphere sphere = new Sphere(null, center, radius);
            
            if (sphere.IsHit(ray))
            {
                return new Color(1f, 0f, 0f, 1f);
            }

            return GetColorForBackground(ray);
        }

        private Color GetNormalColorForSphere(Ray ray, Vector3 center, float radius)
        {
            HitInfo hitInfo;
            Sphere sphere = new Sphere(null, center, radius);
            
            if (sphere.GetHitInfo(ray, out hitInfo, 0, float.MaxValue))
            {
                Vector3 normal = hitInfo.Normal;
                Vector3 colorVector = (normal + new Vector3(1f, 1f, 1f)) * 0.5f;
                return new Color(colorVector.X, colorVector.Y, colorVector.Z);
            }

            return GetColorForBackground(ray);
        }

        private Color GetTwoSphereNormalColorForSphere(Ray ray)
        {
            if (Geometry.GetHitInfo(ray, sphereList, out HitInfo hitInfo))
            {
                Vector3 normal = hitInfo.Normal;
                Vector3 colorVector = (normal + new Vector3(1f, 1f, 1f)) * 0.5f;
                return new Color(colorVector.X, colorVector.Y, colorVector.Z);
            }
            
            return GetColorForBackground(ray);
        }

        private Color GetTwoSphereDiffuse(Ray ray, int reflectCount)
        {
            if (Geometry.GetHitInfo(ray, sphereList, out HitInfo hitInfo))
            {
                if (hitInfo.hasReflectedRay && reflectCount <= maxReflectCount)
                {
                    return hitInfo.Attenuation * GetTwoSphereDiffuse(hitInfo.RayReflected, reflectCount + 1);
                }

                else
                {
                    return new Color(0f, 0f, 0f);
                }
                //Ray scatteredRay = hitInfo.Geometry.ma
                //Ray reflectedRay = hitInfo.Geometry.GetReflectedRay(hitInfo.RayIn, hitInfo);
                //Vector3 point = hitInfo.HitPoint + hitInfo.Normal + RandomUtility.RandomInSphere(1);
            }
            
            return GetColorForBackground(ray);
        }
        #endregion
    }
}