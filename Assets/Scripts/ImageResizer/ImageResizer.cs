using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

namespace Arc
{
#if UNITY_EDITOR
    public class ImageResizer : MonoBehaviour
    {
        [Range(1, 500)] [SerializeField] int resizePercentage = 100;
        [SerializeField] Texture2D[] textures; // Array of Texture2D references

        [ContextMenu("Resize to 4x4")]
        public void Resize()
        {
            foreach (Texture2D texture in textures)
            {
                ResizeAndSaveTexture(texture);
            }
        }

        [ContextMenu("Resize by Percentage")]
        public void ResizeByPercentage()
        {
            foreach (Texture2D texture in textures)
            {
                ResizeAndSaveByPercentage(texture, resizePercentage);
            }
        }

        void ResizeAndSaveByPercentage(Texture2D texture, int percentage)
        {
            if (texture == null || percentage <= 0)
            {
                Debug.LogWarning("Invalid texture or percentage value.");
                return;
            }

            int newWidth = Mathf.Max(1, Mathf.RoundToInt(texture.width * (percentage / 100f)));
            int newHeight = Mathf.Max(1, Mathf.RoundToInt(texture.height * (percentage / 100f)));

            Texture2D resizedTexture = ResizeTexture(texture, newWidth, newHeight);
            SaveTexture(texture, resizedTexture);

            Debug.Log($"Resized {texture.name} by {percentage}% -> {newWidth}x{newHeight}");
        }

        //[ContextMenu("Convert to POT")]
        public void ConvertToPOT()
        {
            foreach (Texture2D texture in textures)
            {
                ConvertAndSaveToPOT(texture);
            }
        }

        void ResizeAndSaveTexture(Texture2D texture)
        {
            int newWidth = Mathf.CeilToInt(texture.width / 4.0f) * 4;
            int newHeight = Mathf.CeilToInt(texture.height / 4.0f) * 4;

            Texture2D resizedTexture = ResizeTexture(texture, newWidth, newHeight);

            SaveTexture(texture, resizedTexture);

            Debug.Log($"Resized and saved texture: {texture.name} to {newWidth}x{newHeight}");
        }

        void ConvertAndSaveToPOT(Texture2D texture)
        {
            int newWidth = Mathf.NextPowerOfTwo(texture.width);
            int newHeight = Mathf.NextPowerOfTwo(texture.height);

            if (newWidth == texture.width && newHeight == texture.height)
            {
                Debug.Log($"Texture {texture.name} is already POT.");
                return;
            }

            Texture2D potTexture = ResizeTexture(texture, newWidth, newHeight);
            SaveTexture(texture, potTexture);

            Debug.Log($"Converted and saved texture: {texture.name} to {newWidth}x{newHeight}");
        }

        Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);

            Texture2D result = new Texture2D(newWidth, newHeight);
            result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            result.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }

        void SaveTexture(Texture2D original, Texture2D modifiedTexture)
        {
            string assetPath = AssetDatabase.GetAssetPath(original);
            byte[] imageData = modifiedTexture.EncodeToPNG();
            File.WriteAllBytes(assetPath, imageData);
            AssetDatabase.ImportAsset(assetPath);
            DestroyImmediate(modifiedTexture);
        }
    }
#endif
}
