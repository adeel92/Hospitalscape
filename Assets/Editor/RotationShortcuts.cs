using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public static class RotationShortcuts
{
    [Shortcut("Custom/Rotate Z 0", KeyCode.Alpha5)]
    private static void RotateZ0()
    {
        ApplyZRotation(0f);
    }

    [Shortcut("Custom/Rotate Z 30", KeyCode.Alpha3)]
    private static void RotateZ30()
    {
        ApplyZRotation(30f);
    }

    [Shortcut("Custom/Rotate Z 60", KeyCode.Alpha4)]
    private static void RotateZ60()
    {
        ApplyZRotation(60f);
    }

    private static void ApplyZRotation(float z)
    {
        if (Selection.transforms == null || Selection.transforms.Length == 0)
            return;

        foreach (Transform t in Selection.transforms)
        {
            Undo.RecordObject(t, "Rotate Z");

            Vector3 euler = t.eulerAngles;
            euler.z = z;
            t.eulerAngles = euler;

            EditorUtility.SetDirty(t);
        }
    }
}