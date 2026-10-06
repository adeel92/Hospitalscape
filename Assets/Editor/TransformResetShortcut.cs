#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public static class TransformResetShortcut
{
    [Shortcut("Custom/Reset Transform", KeyCode.Alpha1)]
    private static void ResetTransform()
    {
        if (Selection.transforms == null || Selection.transforms.Length == 0)
            return;

        foreach (Transform t in Selection.transforms)
        {
            Undo.RecordObject(t, "Rotate Z");

            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            EditorUtility.SetDirty(t);
        }
    }

    [Shortcut("Custom/Reset Position")]
    private static void ResetPosition()
    {
        if (Selection.transforms == null || Selection.transforms.Length == 0)
            return;

        foreach (Transform t in Selection.transforms)
        {
            Undo.RecordObject(t, "Reset Pos");

            t.localPosition = Vector3.zero;
            EditorUtility.SetDirty(t);
        }
    }
}
#endif