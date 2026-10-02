using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.ShortcutManagement;

public static class DeselectSelectionShortcut
{
    private const string ShortcutId = "Custom/Deselect All";

    [Shortcut(ShortcutId)]
    private static void DeselectAll()
    {
        if (Selection.objects.Length == 0)
            return;

        UnityEngine.Object[] previousSelection = Selection.objects;

        Undo.RegisterCompleteObjectUndo(
            new SelectionUndoObject(previousSelection),
            "Deselect All");

        Selection.objects = Array.Empty<UnityEngine.Object>();
    }

    private class SelectionUndoObject : ScriptableObject
    {
        private UnityEngine.Object[] previousSelection;

        public SelectionUndoObject(UnityEngine.Object[] previousSelection)
        {
            this.previousSelection = previousSelection;
        }
    }
}