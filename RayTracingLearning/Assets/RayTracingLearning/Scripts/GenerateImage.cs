using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ray = RayTracingLearning.RayTracer.Ray;

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
                    Color color = GetColorForBackground(ray);
                    texture.SetPixel(i, j, color);
                }
            }

            texture.filterMode = FilterMode.Point;
            texture.Apply();
            GetComponent<RawImage>().texture = texture;
        }

        private Color GetColorForBackground(Ray ray)
        {
            float t = 0.5f * (ray.Direction.y + 1); // -1 ~ 1 to 0 ~ 1
            return Color.Lerp(new Color(1f, 1f, 1f), new Color(0.5f, 0.7f, 1.0f), t);
        }
        #endregion
    }
}