using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;
using RayTracingLearning.RayTracer;
using Ray = RayTracingLearning.RayTracer.Ray;
using Utility = RayTracingLearning.RayTracer.Utility;

namespace RayTracingLearning
{
    public class GenerateImage : MonoBehaviour
    {
        #region methods

        [ContextMenu("Generate Image")]
        private void Generate()
        {
            Texture2D texture = new Texture2D(200, 100);
            float b = 0.2f;
            Vector3 horizontalLength = new Vector3(4, 0, 0); // width : 200
            Vector3 verticalLength = new Vector3(0, 2, 0); // height : 100
            Vector3 center = new Vector3(0, 0, 0);
            Vector3 lowLeftCorner = new Vector3(-2, -1, 1); // z : 1

            for (int i = 0; i < texture.width; ++i)
            {
                for (int j = texture.height - 1; j >= 0; --j)
                {
                    float u = (float) i / texture.width;
                    float v = (float) j / texture.height;
                    Ray ray = new Ray(center, lowLeftCorner + u * horizontalLength + v * verticalLength);
                    Color color = GetColor(ray);
                    texture.SetPixel(i, j, color);
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply();
            GetComponent<RawImage>().texture = texture;
        }

        private Color GetColor(Ray ray)
        {
            //return GetColorForBackground(ray);
            //return GetColorForSphere(ray, new Vector3(0f, 0f, 1f), 0.5f);
            return GetNormalColorForSphere(ray, new Vector3(0f, 0f, 1f), 0.5f);
        }

        private Color GetColorForBackground(Ray ray)
        {
            float t = 0.5f * (ray.Direction.y + 1); // -1 ~ 1 to 0 ~ 1
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
            Vector3 hitPoint;
            
            if (Utility.GetRaySphereHitPoint(ray, center, radius, out hitPoint))
            {
                Vector3 normal = hitPoint - center;
                normal = normal.normalized;
                Vector3 colorVector = (normal + new Vector3(1f, 1f, 1f)) * 0.5f;
                return new Color(colorVector.x, colorVector.y, colorVector.z);
            }

            return GetColorForBackground(ray);
        }
        #endregion
    }
}