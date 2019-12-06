using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
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
using Debug = UnityEngine.Debug;
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
        [SerializeField]
        private Vector2Int resolution = new Vector2Int(200, 100);
        [SerializeField]
        private int threadCount = 8;
        [SerializeField]
        private float updateInterval = 0f;
        private Sphere sphere1;
        private Sphere sphere2;
        private Sphere sphere3;
        private Sphere sphere4;
        private List<Geometry> sphereList;
        private Texture2D texture;
        private Camera camera;
        private int currentRow;
        private int currentCol;
        private bool isGenerating = false;
        private Stopwatch stopwatch;
        private float timer;
        private List<Thread> threadList;
        private ConcurrentQueue<TextureColorData> tempTextureColorDataQueue;
        private int finishPixelCount;
        #endregion
        
        #region unity methods
        private void Update()
        {
            if (isGenerating == false)
            {
                return;
            }
            
            timer += Time.deltaTime;
//            Color color = GetColor(camera, texture, currentCol, currentRow);
//            color.R = (float)Math.Sqrt(color.R);
//            color.G = (float)Math.Sqrt(color.G);
//            color.B = (float)Math.Sqrt(color.B);
//            texture.SetPixel(currentCol, currentRow, new UnityEngine.Color(color.R, color.G, color.B, 1f));
//            GetNextRowCol();

            if (timer >= updateInterval)
            {
                TextureColorData textureColorData;
                int count = tempTextureColorDataQueue.Count;
                int counter = 0;
                
                while (tempTextureColorDataQueue.TryPeek(out textureColorData) && counter <= count)
                {
                    tempTextureColorDataQueue.TryDequeue(out textureColorData);
                    Color color = textureColorData.color;
                    texture.SetPixel(textureColorData.Col, textureColorData.row, new UnityEngine.Color(color.R, color.G, color.B, 1f));
                    ++count;
                    ++finishPixelCount;
                }
                
                timer = 0f;

                if (count > 0)
                {
                    texture.Apply();
                    
                    if (finishPixelCount == resolution.x * resolution.y)
                    {
                        isGenerating = false;
                        stopwatch.Stop();
                        Debug.Log("generate finish");
                        EditorUtility.DisplayDialog("ray tracer", "ray tracer used time : " + stopwatch.Elapsed.TotalSeconds.ToString(), "confirm");
                        OnDestroy();
                        //texture.EncodeToPNG();
                    }
                }
            }
            
            texture.Apply();
        }

        private void OnDestroy()
        {
            if (threadList == null)
            {
                return;
            }
            
            foreach (Thread thread in threadList)
            {
                thread.Abort();
            }
        }

        #endregion

        #region methods
        [ContextMenu("Generate Image")]
        private void Generate()
        {
            OnDestroy();
            Debug.Log("generate start");
            // build the world, width : 200, height : 100, depth : 100
            texture = new Texture2D(resolution.x, resolution.y);
            //Vector3 horizontalLength = new Vector3(4, 0, 0); // width : 200
            //Vector3 verticalLength = new Vector3(0, 2, 0); // height : 100
            //Vector3 center = new Vector3(0, 0, 0);
            //Vector3 lowLeftCorner = new Vector3(-2, -1, 1); // depth : 50, z : 1
            camera = new Camera(new Vector3(0f, 0f, 0f), 4f, 2f, 1f);
            Material lambertian1 = new Lambertian(new Color(0.8f, 0.3f, 0.3f));
            Material lambertian2 = new Lambertian(new Color(0.8f, 0.8f, 0f));
            Material metal1 = new Metal(new Color(0.8f, 0.6f, 0.2f), 1f * globalMetalFuzziness);
            Material metal2 = new Metal(new Color(0.8f, 0.8f, 0.8f), 0.3f * globalMetalFuzziness);
            sphere1 = new Sphere(lambertian1, new Vector3(0f, 0f, 1f), 0.5f);
            sphere2 = new Sphere(lambertian2, new Vector3(0f,-100.5f,1f), 100f);
            sphere3 = new Sphere(metal1, new Vector3(1f,0f,1f), 0.5f);
            sphere4 = new Sphere(metal2, new Vector3(-1,0,1f), 0.5f);
            sphereList = new List<Geometry>() {sphere1, sphere2, sphere3, sphere4};
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            GetComponent<RawImage>().texture = texture;
            currentRow = resolution.y - 1;
            currentCol = 0;
            timer = 0f;
            finishPixelCount = 0;
            tempTextureColorDataQueue = new ConcurrentQueue<TextureColorData>();
            int perThreadRowCount = resolution.y / threadCount;
            threadList = new List<Thread>(threadCount + 1);

            for (int i = 0; i < threadCount; ++i)
            {
                ThreadData threadData = new ThreadData();
                threadData.StartRow = i * perThreadRowCount;
                threadData.EndRow = threadData.StartRow + perThreadRowCount - 1;
                threadData.ID = i;
                Thread thread = new Thread(StartThread);
                threadList.Add(thread);
                thread.IsBackground = true;
                thread.Start(threadData);
            }

            int leftRowCount = resolution.y % threadCount;

            if (leftRowCount > 0)
            {
                ThreadData threadData = new ThreadData();
                threadData.StartRow = resolution.y - leftRowCount;
                threadData.EndRow = resolution.y - 1;
                threadData.ID = threadCount;
                Thread thread = new Thread(StartThread);
                threadList.Add(thread);
                thread.IsBackground = true;
                thread.Start(threadData);
            }
            
            isGenerating = true;
            stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
            
            
        //     for (int i = 0; i < texture.width; ++i)
        //     {
        //         for (int j = texture.height - 1; j >= 0; --j)
        //         {
        //             // Color color = GetColor(camera, texture, i, j);
        //             // color.R = (float)Math.Sqrt(color.R);
        //             // color.G = (float)Math.Sqrt(color.G);
        //             // color.B = (float)Math.Sqrt(color.B);
        //             // texture.SetPixel(i, j, new UnityEngine.Color(color.R, color.G, color.B, 1f));
        //         }
        //     }
         }

        private void GetNextRowCol()
        {
            ++currentCol;

            if (currentCol == resolution.x)
            {
                currentCol = 0;
                --currentRow;

                if (currentRow == -1)
                {
                    isGenerating = false;
                    stopwatch.Stop();
                    texture.Apply();
                    Debug.Log("generate finish");
                    EditorUtility.DisplayDialog("ray tracer", "ray tracer used time : " + stopwatch.Elapsed.TotalSeconds.ToString(), "confirm");
                }
            }
        }

        private void StartThread(object param)
        {
            ThreadData threadData = (ThreadData)param;
            Debug.Log("therad " + threadData.ID + " start");
            int tempDebug = 0;

            for (int row = threadData.StartRow; row <= threadData.EndRow; ++row)
            {
                for (int col = 0; col < resolution.x; ++col)
                {
                    Color color = GetColor(camera, texture, col, row);
                    color.R = (float)Math.Sqrt(color.R);
                    color.G = (float)Math.Sqrt(color.G);
                    color.B = (float)Math.Sqrt(color.B);
                    TextureColorData textureColorData = new TextureColorData();
                    textureColorData.color = color;
                    textureColorData.row = row;
                    textureColorData.Col = col;
                    tempTextureColorDataQueue.Enqueue(textureColorData);
                    ++tempDebug;
                    //texture.SetPixel(currentCol, currentRow, new UnityEngine.Color(color.R, color.G, color.B, 1f));
                }
            }
            
            Debug.Log("therad " + threadData.ID + " finish  " + tempDebug);
        }
        
        private Color GetColor(Camera camera, Texture texture, int x, int y)
        {
            if (isUseAA == false)
            {
                float u = (float) x / resolution.x;
                float v = (float) y / resolution.y;
                Ray ray = camera.GetRay(u, v);
                return GetColorImpl(ray);
            }

            Color color = new Color(0f, 0f, 0f);
            Random random = new Random();
            
            for (int i = 0; i < aaSampleCount; ++i)
            {
                float randomU = (float) (x + random.Next(0, 100) * 0.01f) / resolution.x;
                float randomV = (float) (y + random.Next(0, 100) * 0.01f) / resolution.y;
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
        
        #region structs
        private struct ThreadData
        {
            public int ID;
            public int StartRow;
            public int EndRow;
        }
        
        private struct TextureColorData
        {
            public int row;
            public int Col;
            public Color color;
        }
        #endregion
    }
}