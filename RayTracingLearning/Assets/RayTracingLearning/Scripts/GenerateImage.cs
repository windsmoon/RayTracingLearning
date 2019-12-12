using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
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
using ThreadPriority = System.Threading.ThreadPriority;

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
        [SerializeField, UnityEngine.Range(0f, 1f)]
        private float globalMetalFuzziness = 1f;
        [SerializeField]
        private Vector2Int resolution = new Vector2Int(200, 100);
        [SerializeField]
        private UnityEngine.Vector3 lookFrom = new UnityEngine.Vector3(0f, 0f, -1f);
        [SerializeField]
        private UnityEngine.Vector3 lookAt = new UnityEngine.Vector3(0f, 0f, 1f);
        [SerializeField]
        private float fov = 90f;
        [SerializeField]
        private float apeture = 2f;
        [SerializeField]
        private float focusDistance = 10f;
        [SerializeField]
        private int threadCount = 8;
        private Sphere sphere1;
        private Sphere sphere2;
        private Sphere sphere3;
        private Sphere sphere4;
        private Sphere sphere5;
        private List<Geometry> sphereList;
        private Texture2D texture;
        private new Camera camera;
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
                    SaveToDisk();
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
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            GetComponent<RawImage>().texture = texture;
            Vector3 tempLookFrom = new Vector3(lookFrom.x, lookFrom.y, lookFrom.z);
            Vector3 tempLookAt = new Vector3(lookAt.x, lookAt.y, lookAt.z);
            camera = new Camera(tempLookFrom , tempLookAt, fov, (float)resolution.x / (float)resolution.y, apeture, focusDistance);
//            camera = new Camera(new Vector3(0f,2f,1f), new Vector3(0f,0f,1f), 90f, (float)resolution.x / (float)resolution.y);
            InitSpheres();
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
                thread.Priority = ThreadPriority.Highest;
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
                thread.Priority = ThreadPriority.Highest;
                thread.Start(threadData);
            }
            
            isGenerating = true;
            stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
        }

        private void InitSpheres()
        {
//            Material lambertian1 = new Lambertian(new Color(0.1f, 0.2f, 0.5f));
//            Material lambertian2 = new Lambertian(new Color(0.8f, 0.8f, 0f));
//            Material metal1 = new Metal(new Color(0.8f, 0.6f, 0.2f), 0f * globalMetalFuzziness);
////            Material metal2 = new Metal(new Color(0.8f, 0.8f, 0.8f), 0.3f * globalMetalFuzziness);
//            Material dielectric1 = new Dielectric(new Color(1f, 1f, 1f), 0f, 1.5f);
//            sphere1 = new Sphere(lambertian1, new Vector3(0f, 0f, 1f), 0.5f);
//            sphere2 = new Sphere(lambertian2, new Vector3(0f,-100.5f,1f), 100f);
//            sphere3 = new Sphere(metal1, new Vector3(1f,0f,1f), 0.5f);
//            sphere4 = new Sphere(dielectric1, new Vector3(-1f,0f,1f), 0.5f);
//            sphere5 = new Sphere(dielectric1, new Vector3(-1f, 0f, 1f), -0.45f);
//            sphereList = new List<Geometry>() {sphere1, sphere2, sphere3, sphere4};
            
//            float raidus = (float)System.Math.Cos(System.Math.PI / 4f);
//            Material lambertianCamera1 = new Lambertian(new Color(0f, 0f, 1f));
//            Material lambertianCamera2 = new Lambertian(new Color(1f, 0f, 0f));
//            Sphere sphereCamera1 = new Sphere(lambertianCamera1, new Vector3(-raidus, 0f, 1f), raidus);
//            Sphere sphereCamera2 = new Sphere(lambertianCamera2, new Vector3(raidus, 0f, 1f), raidus);
//            sphereList = new List<Geometry>() {sphereCamera1, sphereCamera2};

            RandomSpheres();
        }

        private void RandomSpheres()
        {
            sphereList = new List<Geometry>() {};
            sphereList.Add(new Sphere(new Lambertian(new Color(0.5f, 0.5f, 0.5f)), new Vector3(0f, -1000f, 0f), 1000f));

            for (int i = -11; i < 11; ++i)
            {
                for (int j = -11; j < 11; ++j)
                {
                    AddRandomMaterialSphere(i, j);
                }
            }
            
            sphereList.Add(new Sphere(new Dielectric(new Color(1f, 1f, 1f), 0f, 1.5f), new Vector3(0f, 1f, 0f), 1.0f));
            sphereList.Add(new Sphere(new Lambertian(new Color(0.4f, 0.2f, 0.1f)), new Vector3(-4f, 1f, 0f), 1.0f));
            sphereList.Add(new Sphere(new Metal(new Color(0.7f, 0.6f, 0.5f), 0f), new Vector3(4f, 1f, 0f), 1.0f));
        }

        private void AddRandomMaterialSphere(int a, int b)
        {
            float randomMaterialValue = RandomUtility.Random01();
            Vector3 center = new Vector3(a + 0.9f * RandomUtility.Random01(), 0.2f, -b - 0.9f * RandomUtility.Random01());

            if ((center - new Vector3(4.0f, 0.2f, 0f)).GetLength() > 0.9f)
            {
                if (randomMaterialValue < 0.8f)
                {
                    Lambertian lambertian = new Lambertian(
                        new Color(RandomUtility.Random01() * RandomUtility.Random01(), RandomUtility.Random01() * RandomUtility.Random01(), RandomUtility.Random01() * RandomUtility.Random01()));
                    Sphere sphere = new Sphere(lambertian, center, 0.2f);
                    sphereList.Add(sphere);
                }
                
                else if (randomMaterialValue < 0.95f)
                {
                    Metal metal = new Metal(
                        new Color(0.5f + (1f + RandomUtility.Random01()), 0.5f + (1f + RandomUtility.Random01()), 0.5f + (1f + RandomUtility.Random01())), 0.5f * RandomUtility.Random01());
                    Sphere sphere = new Sphere(metal, center, 0.2f);
                    sphereList.Add(sphere);
                }

                else
                {
                    Dielectric dielectric = new Dielectric(new Color(1f, 1f, 1f), 0f, 1.5f);
                    Sphere sphere = new Sphere(dielectric, center, 0.2f);
                    sphereList.Add(sphere);                        
                }
            }
        }
        
        private void SaveToDisk()
        {
            string resolutionName = resolution.x.ToString() + "x" + resolution.y.ToString();
            string folder = "Assets/Pictures/" + resolutionName + "/";
            
            if (System.IO.Directory.Exists(folder) == false)
            {
                Directory.CreateDirectory(folder);
            }
            
            byte[] bytes = texture.EncodeToPNG();
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            FileStream fs = new FileStream(folder + ts.TotalMilliseconds.ToString() + ".png", FileMode.Create);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
            #if UNITY_EDITOR
            AssetDatabase.Refresh(ImportAssetOptions.Default);
            #endif
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