#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SpriteRendererBakerWindow : EditorWindow
{
    private const int TemporaryLayer = 31;

    [SerializeField] int m_PixelsPerUnit = 100;
    [SerializeField] int m_Padding = 2;
    [SerializeField] string m_OutputFolder = "Assets/BakedSprites";
    [SerializeField] bool m_CreateSpriteObject = true;

    private List<SpriteRenderer> m_SpriteRenderers = new();

    [MenuItem("Tools/Sprite Baker")]
    private static void OpenWindow()
    {
        GetWindow<SpriteRendererBakerWindow>("Sprite Baker");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Selected Sprite Renderers",
            EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh Selection"))
        {
            RefreshSelection();
        }

        EditorGUILayout.LabelField(
            $"Found: {m_SpriteRenderers.Count}");

        EditorGUILayout.Space();

        m_PixelsPerUnit = EditorGUILayout.IntField(
            "Pixels Per Unit",
            m_PixelsPerUnit);

        m_Padding = EditorGUILayout.IntField(
            "Padding (Pixels)",
            m_Padding);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Output",
            EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        m_OutputFolder = EditorGUILayout.TextField(
            "Folder",
            m_OutputFolder);

        if (GUILayout.Button("Select", GUILayout.Width(60)))
        {
            string folder = EditorUtility.OpenFolderPanel(
                "Select Output Folder",
                Application.dataPath,
                "");

            if (!string.IsNullOrEmpty(folder))
            {
                if (folder.StartsWith(Application.dataPath))
                {
                    m_OutputFolder =
                        "Assets" +
                        folder.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Invalid Folder",
                        "Please select a folder inside the Unity Assets folder.",
                        "OK");
                }
            }
        }

        EditorGUILayout.EndHorizontal();

        m_CreateSpriteObject = EditorGUILayout.Toggle(
            "Create Sprite Object",
            m_CreateSpriteObject);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(
                   m_SpriteRenderers.Count == 0))
        {
            if (GUILayout.Button(
                    "Bake Selected Sprites",
                    GUILayout.Height(40)))
            {
                Bake();
            }
        }

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Select the SpriteRenderer GameObjects you want to combine, " +
            "then click Refresh Selection and Bake Selected Sprites.",
            MessageType.Info);
    }

    private void RefreshSelection()
    {
        m_SpriteRenderers.Clear();

        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            SpriteRenderer spriteRenderer =
                selectedObject.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null &&
                spriteRenderer.sprite != null)
            {
                m_SpriteRenderers.Add(spriteRenderer);
            }

            SpriteRenderer[] children =
                selectedObject.GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer child in children)
            {
                if (child.sprite != null &&
                    !m_SpriteRenderers.Contains(child))
                {
                    m_SpriteRenderers.Add(child);
                }
            }
        }

        Repaint();
    }

    private void Bake()
    {
        if (m_SpriteRenderers.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Sprite Baker",
                "No SpriteRenderers found.",
                "OK");

            return;
        }

        if (m_PixelsPerUnit <= 0)
        {
            EditorUtility.DisplayDialog(
                "Sprite Baker",
                "Pixels Per Unit must be greater than zero.",
                "OK");

            return;
        }

        SceneView sceneView =
            SceneView.lastActiveSceneView;

        if (sceneView == null ||
            sceneView.camera == null)
        {
            EditorUtility.DisplayDialog(
                "Sprite Baker",
                "Could not access the Scene View camera.",
                "OK");

            return;
        }

        Camera sceneCamera =
            sceneView.camera;

        Bounds bounds =
            CalculateBounds();

        Vector3 cameraRight =
            sceneCamera.transform.right;

        Vector3 cameraUp =
            sceneCamera.transform.up;

        Vector3 cameraForward =
            sceneCamera.transform.forward;

        CalculateProjectedBounds(
            bounds,
            cameraRight,
            cameraUp,
            out float minX,
            out float maxX,
            out float minY,
            out float maxY);

        float widthWorld =
            maxX - minX;

        float heightWorld =
            maxY - minY;

        int width =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    widthWorld * m_PixelsPerUnit) +
                m_Padding * 2);

        int height =
            Mathf.Max(
                1,
                Mathf.CeilToInt(
                    heightWorld * m_PixelsPerUnit) +
                m_Padding * 2);

        Vector3 cameraPosition =
            bounds.center -
            cameraForward * 100f;

        GameObject cameraObject =
            new GameObject("Sprite Baker Camera");

        Camera bakeCamera =
            cameraObject.AddComponent<Camera>();

        List<(GameObject gameObject, int layer)> originalLayers =
            new();

        RenderTexture renderTexture = null;

        try
        {
            PrepareLayers(originalLayers);

            bakeCamera.transform.position =
                cameraPosition;

            bakeCamera.transform.rotation =
                sceneCamera.transform.rotation;

            bakeCamera.orthographic = true;

            bakeCamera.orthographicSize =
                height / (float)m_PixelsPerUnit * 0.5f;

            bakeCamera.aspect =
                (float)width / height;

            bakeCamera.nearClipPlane = 0.01f;
            bakeCamera.farClipPlane = 200f;

            bakeCamera.clearFlags =
                CameraClearFlags.SolidColor;

            bakeCamera.backgroundColor =
                new Color(0f, 0f, 0f, 0f);

            bakeCamera.cullingMask =
                1 << TemporaryLayer;

            bakeCamera.allowHDR = false;
            bakeCamera.allowMSAA = false;

            renderTexture =
                new RenderTexture(
                    width,
                    height,
                    24,
                    RenderTextureFormat.ARGB32);

            renderTexture.Create();

            bakeCamera.targetTexture =
                renderTexture;

            RenderTexture previousActive =
                RenderTexture.active;

            RenderTexture.active =
                renderTexture;

            bakeCamera.Render();

            Texture2D texture =
                new Texture2D(
                    width,
                    height,
                    TextureFormat.RGBA32,
                    false);

            texture.ReadPixels(
                new Rect(0, 0, width, height),
                0,
                0);

            texture.Apply();

            RenderTexture.active =
                previousActive;

            byte[] pngData =
                texture.EncodeToPNG();

            DestroyImmediate(texture);

            bakeCamera.targetTexture = null;

            string outputPath =
                EditorUtility.SaveFilePanelInProject(
                    "Save Baked Sprite",
                    "BakedSprite",
                    "png",
                    "Choose where to save the baked PNG.",
                    m_OutputFolder);

            if (string.IsNullOrEmpty(outputPath))
            {
                return;
            }

            File.WriteAllBytes(
                outputPath,
                pngData);

            AssetDatabase.Refresh();

            ConfigureImportedSprite(outputPath);

            AssetDatabase.Refresh();

            Sprite bakedSprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(
                    outputPath);

            if (bakedSprite != null &&
                m_CreateSpriteObject)
            {
                CreateSpriteObject(
                    bakedSprite,
                    bounds.center);
            }

            EditorUtility.DisplayDialog(
                "Sprite Baker",
                $"Successfully baked sprite.\n\n" +
                $"Resolution: {width} × {height}\n" +
                $"Path: {outputPath}",
                "OK");
        }
        finally
        {
            RestoreLayers(originalLayers);

            if (renderTexture != null)
            {
                if (renderTexture.IsCreated())
                {
                    renderTexture.Release();
                }

                DestroyImmediate(renderTexture);
            }

            if (cameraObject != null)
            {
                DestroyImmediate(cameraObject);
            }

            AssetDatabase.Refresh();
        }
    }

    private Bounds CalculateBounds()
    {
        Bounds bounds =
            m_SpriteRenderers[0].bounds;

        for (int i = 1;
             i < m_SpriteRenderers.Count;
             i++)
        {
            bounds.Encapsulate(
                m_SpriteRenderers[i].bounds);
        }

        return bounds;
    }

    private void CalculateProjectedBounds(
        Bounds bounds,
        Vector3 cameraRight,
        Vector3 cameraUp,
        out float minX,
        out float maxX,
        out float minY,
        out float maxY)
    {
        minX = float.MaxValue;
        maxX = float.MinValue;

        minY = float.MaxValue;
        maxY = float.MinValue;

        Vector3 center =
            bounds.center;

        Vector3[] corners =
        {
            new Vector3(
                bounds.min.x,
                bounds.min.y,
                bounds.min.z),

            new Vector3(
                bounds.min.x,
                bounds.min.y,
                bounds.max.z),

            new Vector3(
                bounds.min.x,
                bounds.max.y,
                bounds.min.z),

            new Vector3(
                bounds.min.x,
                bounds.max.y,
                bounds.max.z),

            new Vector3(
                bounds.max.x,
                bounds.min.y,
                bounds.min.z),

            new Vector3(
                bounds.max.x,
                bounds.min.y,
                bounds.max.z),

            new Vector3(
                bounds.max.x,
                bounds.max.y,
                bounds.min.z),

            new Vector3(
                bounds.max.x,
                bounds.max.y,
                bounds.max.z)
        };

        foreach (Vector3 corner in corners)
        {
            Vector3 relativePosition =
                corner - center;

            float x =
                Vector3.Dot(
                    relativePosition,
                    cameraRight);

            float y =
                Vector3.Dot(
                    relativePosition,
                    cameraUp);

            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);

            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);
        }
    }

    private void PrepareLayers(
        List<(GameObject gameObject, int layer)> originalLayers)
    {
        foreach (SpriteRenderer spriteRenderer
                 in m_SpriteRenderers)
        {
            GameObject gameObject =
                spriteRenderer.gameObject;

            originalLayers.Add(
                (gameObject, gameObject.layer));

            gameObject.layer =
                TemporaryLayer;
        }
    }

    private void RestoreLayers(
        List<(GameObject gameObject, int layer)> originalLayers)
    {
        foreach (var item in originalLayers)
        {
            if (item.gameObject != null)
            {
                item.gameObject.layer =
                    item.layer;
            }
        }
    }

    private void ConfigureImportedSprite(
        string assetPath)
    {
        TextureImporter importer =
            AssetImporter.GetAtPath(assetPath)
            as TextureImporter;

        if (importer == null)
        {
            return;
        }

        importer.textureType =
            TextureImporterType.Sprite;

        importer.spriteImportMode =
            SpriteImportMode.Single;

        importer.alphaIsTransparency =
            true;

        importer.mipmapEnabled =
            false;

        importer.filterMode =
            FilterMode.Bilinear;

        importer.wrapMode =
            TextureWrapMode.Clamp;

        importer.spritePixelsPerUnit =
            m_PixelsPerUnit;

        importer.SaveAndReimport();
    }

    private void CreateSpriteObject(
        Sprite sprite,
        Vector3 position)
    {
        GameObject spriteObject =
            new GameObject(
                sprite.name + "_Baked");

        spriteObject.transform.position =
            position;

        SpriteRenderer spriteRenderer =
            spriteObject.AddComponent<SpriteRenderer>();

        spriteRenderer.sprite =
            sprite;

        Selection.activeGameObject =
            spriteObject;
    }
}
#endif