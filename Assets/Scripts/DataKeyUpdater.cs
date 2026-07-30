using System.Collections.Generic;
using System.IO;
using Isometric.Data;
using NaughtyAttributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DataKeyUpdater : MonoBehaviour
{
    [SerializeField]
    private List<DataKeyed> assets = new();

#if UNITY_EDITOR

    [Button("Update Keys From Asset Names")]
    private void UpdateKeysFromAssetNames()
    {
        int updatedCount = 0;

        foreach (DataKeyed asset in assets)
        {
            if (asset == null)
                continue;

            string assetPath = AssetDatabase.GetAssetPath(asset);

            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning($"Could not find asset path for {asset.name}.", asset);
                continue;
            }

            // Gets filename without the extension.
            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            SerializedObject serializedAsset = new SerializedObject(asset);
            SerializedProperty keyProperty =
                serializedAsset.FindProperty("m_Key");

            if (keyProperty == null)
            {
                Debug.LogWarning(
                    $"Could not find m_Key on {asset.name}.",
                    asset
                );

                continue;
            }

            if (keyProperty.stringValue == fileName)
                continue;

            Undo.RecordObject(asset, "Update Data Key");

            keyProperty.stringValue = fileName;
            serializedAsset.ApplyModifiedProperties();

            EditorUtility.SetDirty(asset);

            updatedCount++;

            Debug.Log(
                $"Updated key of '{asset.name}' to '{fileName}'.",
                asset
            );
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Updated {updatedCount} DataKeyed asset keys.");
    }

#endif
}