using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public static class TransformCopyShortcut
{
    [Shortcut("Custom/Copy Local Position")]
    private static void CopyLocalPosition()
    {
        Debug.Log("Copy Local Position Triggered 1");

        if (Selection.transforms.Length != 1)
            return;

        Debug.Log("Copy Local Position Triggered 2");
        Transform selectedTransform = Selection.activeTransform;
        EditorGUIUtility.systemCopyBuffer = $"{selectedTransform.localPosition.x}, {selectedTransform.localPosition.y}, {selectedTransform.localPosition.z}";
    }
}