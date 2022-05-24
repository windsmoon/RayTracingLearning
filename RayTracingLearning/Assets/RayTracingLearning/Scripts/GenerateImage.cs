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
using UnityEngine.UIElements;
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
        [SerializeField]
        private bool useBVH = true;
        private Sphere sphere1;
        private Sphere sphere2;
        private Sphere sphere3;
        private Sphere sphere4;
        private Sphere sphere5;
        private List<Geometry> sphereList;
        private BVH<Geometry> bvh;
        private Texture2D texture;
        private new Camera camera;
        private bool isGenerating = false;
        private Stopwatch stopwatch;
        private List<Thread> threadList;
        private ConcurrentQueue<TextureColorData> tempTextureColorDataQueue;
        private int finishPixelCount;
        private Text text;
        private float timer = 0f;
        #endregion
        
        #region unity methods
        private void Awake()
        {
            #if !UNITY_EDITOR
            ParseConfig();
            Generate();
            #endif
            text = transform.Find("Text").gameObject.GetComponent<Text>();
        }

        private void Update()
        {
            if (isGenerating == false)
            {
                return;
            }

            timer += Time.deltaTime;
            text.text = timer.ToString();
            TextureColorData textureColorData;
            int count = tempTextureColorDataQueue.Count;
            int counter = 0;
            
            while (tempTextureColorDataQueue.TryPeek(out textureColorData) && counter <= count)
            {
                tempTextureColorDataQueue.TryDequeue(out textureColorData);
                Color color = textureColorData.color;
                texture.SetPixel(textureColorData.Col, textureColorData.row, new UnityEngine.Color(color.R, color.G, color.B, 1f));
                ++counter;
                ++finishPixelCount;
            }
            
            if (count > 0)
            {
                texture.Apply();
                
                if (finishPixelCount == resolution.x * resolution.y)
                {
                    Debug.Log(RayTracer.RayTracer.Counter);
                    isGenerating = false;
                    stopwatch.Stop();
                    Debug.Log("generate finish");
                    #if UNITY_EDITOR
                    EditorUtility.DisplayDialog("ray tracer", "ray tracer used time : " + stopwatch.Elapsed.TotalSeconds.ToString(), "confirm");
                    #endif
                    OnDestroy();
                    SaveToDisk();
                }
            }
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
            timer = 0f;
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
            RayTracer.RayTracer.Counter = 0;
            finishPixelCount = 0;
            tempTextureColorDataQueue = new ConcurrentQueue<TextureColorData>();
            int perThreadRowCount = resolution.y / threadCount;
            threadList = new List<Thread>(threadCount + 1);

            for (int i = 0; i < threadCount; ++i)
            {
                ThreadData threadData = new ThreadData();
                threadData.StartRow = i * perThreadRowCount;
                threadData.EndRow = threadData.StartRow + perThreadRowCount - 1;
                threadData.StartCol = 0;
                threadData.EndCol = resolution.x - 1;
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
                threadData.StartCol = 0;
                threadData.EndCol = resolution.x;
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
//            ThreadPool.SetMinThreads(1, 1);
//            ThreadPool.SetMaxThreads(1, 1);
//            
//            for (int row = 0; row < resolution.y; ++row)
//            {
//                for (int col = 0; col < resolution.x; ++col)
//                {
//                    ThreadData threadData = new ThreadData();
//                    threadData.StartRow = row;
//                    threadData.EndRow = row;
//                    threadData.StartCol = col;
//                    threadData.EndCol = col;
//                    threadData.ID = row * resolution.x + col;
//                    ThreadPool.QueueUserWorkItem(new WaitCallback(StartThread), threadData);
//                }
//            }
        }

        [ContextMenu("Generate Config File")]
        private void GenerateConfigFile()
        {
            string filePath = "Bin/config.txt";
            
            if (System.IO.File.Exists(filePath) == true)
            {
                System.IO.File.Delete(filePath);
            }
            
            FileStream fs = new FileStream(filePath, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine((isUseAA ? 1 : 0).ToString());
            sw.WriteLine(aaSampleCount.ToString());
            sw.WriteLine(maxReflectCount.ToString());
            sw.WriteLine(resolution.x.ToString());
            sw.WriteLine(resolution.y.ToString());
            sw.WriteLine(lookFrom.x.ToString());
            sw.WriteLine(lookFrom.y.ToString());
            sw.WriteLine(lookFrom.z.ToString());
            sw.WriteLine(lookAt.x.ToString());
            sw.WriteLine(lookAt.y.ToString());
            sw.WriteLine(lookAt.z.ToString());
            sw.WriteLine(fov.ToString());
            sw.WriteLine(apeture.ToString());
            sw.WriteLine(focusDistance.ToString());
            sw.WriteLine((useBVH ? 1 : 0).ToString());
//            sw.Write((isUseAA ? 1 : 0).ToString() + "\r\n");
//            sw.Write(aaSampleCount.ToString() + "\r\n");
//            sw.Write(maxReflectCount.ToString() + "\r\n");
//            sw.Write(resolution.x.ToString() + "\r\n");
//            sw.Write(resolution.y.ToString() + "\r\n");
//            sw.Write(lookFrom.x.ToString() + "\r\n");
//            sw.Write(lookFrom.y.ToString() + "\r\n");
//            sw.Write(lookFrom.z.ToString() + "\r\n");
//            sw.Write(lookAt.x.ToString() + "\r\n");
//            sw.Write(lookAt.y.ToString() + "\r\n");
//            sw.Write(lookAt.z.ToString() + "\r\n");
//            sw.Write(fov.ToString() + "\r\n");
//            sw.Write(apeture.ToString() + "\r\n");
//            sw.Write(focusDistance.ToString() + "\r\n");
            sw.Close();
            fs.Close();
            #if UNITY_EDITOR
            AssetDatabase.Refresh(ImportAssetOptions.Default);
            #endif
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

            if (useBVH)
            {
                bvh = new BVH<Geometry>(sphereList);
            }
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
//            Debug.Log("therad " + threadData.ID + " start");
//            int tempDebug = 0;

            for (int row = threadData.StartRow; row <= threadData.EndRow; ++row)
            {
                for (int col = threadData.StartCol; col <= threadData.EndCol; ++col)
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
//                    ++tempDebug;
                    //texture.SetPixel(currentCol, currentRow, new UnityEngine.Color(color.R, color.G, color.B, 1f));
                }
            }
            
//            Debug.Log("therad " + threadData.ID + " finish  " + tempDebug);
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
            
            for (int i = 0; i < aaSampleCount; ++i)
            {
                float randomU = (float) (x + RandomUtility.Random(0, 100) * 0.01f) / resolution.x;
                float randomV = (float) (y + RandomUtility.Random(0, 100) * 0.01f) / resolution.y;
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

        // private Color GetColorForSphere(Ray ray, Vector3 center, float radius)
        // {
        //     Sphere sphere = new Sphere(null, center, radius);
        //     
        //     if (sphere.IsHit(ray))
        //     {
        //         return new Color(1f, 0f, 0f, 1f);
        //     }
        //
        //     return GetColorForBackground(ray);
        // }

        // private Color GetNormalColorForSphere(Ray ray, Vector3 center, float radius)
        // {
        //     HitInfo hitInfo;
        //     Sphere sphere = new Sphere(null, center, radius);
        //     
        //     if (sphere.GetHitInfo(ray, out hitInfo, 0, float.MaxValue))
        //     {
        //         Vector3 normal = hitInfo.Normal;
        //         Vector3 colorVector = (normal + new Vector3(1f, 1f, 1f)) * 0.5f;
        //         return new Color(colorVector.X, colorVector.Y, colorVector.Z);
        //     }
        //
        //     return GetColorForBackground(ray);
        // }
        //
        // private Color GetTwoSphereNormalColorForSphere(Ray ray)
        // {
        //     if (Geometry.GetHitInfo(ray, sphereList, out HitInfo hitInfo))
        //     {
        //         Vector3 normal = hitInfo.Normal;
        //         Vector3 colorVector = (normal + new Vector3(1f, 1f, 1f)) * 0.5f;
        //         return new Color(colorVector.X, colorVector.Y, colorVector.Z);
        //     }
        //     
        //     return GetColorForBackground(ray);
        // }

        private Color GetTwoSphereDiffuse(Ray ray, int reflectCount)
        {
            if (useBVH)
            {
                if (RayTracer.RayTracer.GetHitInfo(ray, bvh, out HitInfo hitInfo))
                {
                    if (hitInfo.hasReflectedRay && reflectCount <= maxReflectCount)
                    {
                        return hitInfo.Attenuation * GetTwoSphereDiffuse(hitInfo.RayReflected, reflectCount + 1);
                    }

                    else
                    {
                        return new Color(0f, 0f, 0f);
                    }
                }
            }

            else
            {
                if (RayTracer.RayTracer.GetHitInfo(ray, sphereList, out HitInfo hitInfo))
                {
                    if (hitInfo.hasReflectedRay && reflectCount <= maxReflectCount)
                    {
                        return hitInfo.Attenuation * GetTwoSphereDiffuse(hitInfo.RayReflected, reflectCount + 1);
                    }

                    else
                    {
                        return new Color(0f, 0f, 0f);
                    }
                }
            }
            
            return GetColorForBackground(ray);
        }

        private void ParseConfig()
        {
            string filePath = "config.txt";
            
            if (System.IO.File.Exists(filePath) == false)
            {
                return;
            }
            
            FileStream fs = new FileStream(filePath, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            isUseAA = sr.ReadLine() == "1";
            aaSampleCount = Convert.ToInt32(sr.ReadLine());
            maxReflectCount = Convert.ToInt32(sr.ReadLine());    
            resolution = new Vector2Int(Convert.ToInt32(sr.ReadLine()), Convert.ToInt32(sr.ReadLine()));
            lookFrom = new UnityEngine.Vector3(Convert.ToSingle(sr.ReadLine()), Convert.ToSingle(sr.ReadLine()), Convert.ToSingle(sr.ReadLine()));
            lookAt = new UnityEngine.Vector3(Convert.ToSingle(sr.ReadLine()), Convert.ToSingle(sr.ReadLine()), Convert.ToSingle(sr.ReadLine()));
            fov = Convert.ToSingle(sr.ReadLine());
            apeture = Convert.ToSingle(sr.ReadLine());
            focusDistance = Convert.ToSingle(sr.ReadLine());
            useBVH = sr.ReadLine() == "1";
            
//            sr.Read((isUseAA ? 1 : 0).ToString() + "\r\n");
//            sr.Write(aaSampleCount.ToString() + "\r\n");
//            sr.Write(maxReflectCount.ToString() + "\r\n");
//            sr.Write(resolution.x.ToString() + "\r\n");
//            sr.Write(resolution.y.ToString() + "\r\n");
//            sr.Write(lookFrom.x.ToString() + "\r\n");
//            sr.Write(lookFrom.y.ToString() + "\r\n");
//            sr.Write(lookFrom.z.ToString() + "\r\n");
//            sr.Write(lookAt.x.ToString() + "\r\n");
//            sr.Write(lookAt.y.ToString() + "\r\n");
//            sr.Write(lookAt.z.ToString() + "\r\n");
//            sr.Write(fov.ToString() + "\r\n");
//            sr.Write(apeture.ToString() + "\r\n");
//            sr.Write(focusDistance.ToString() + "\r\n");
            sr.Close();
            fs.Close();
        }
        #endregion
        
        #region structs
        private struct ThreadData
        {
            public int ID;
            public int StartRow;
            public int EndRow;
            public int StartCol;
            public int EndCol;
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