#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-time importer for the separate kerning CSV extracted from Babar-Slab-Demo.ttf.
/// Compatible with the TMP_FontAsset / TMP_FontFeatureTable APIs used by TMP 3.x.
/// Put this file in an Assets/Editor directory; select a TMP font asset before running.
/// This changes only the selected TMP font asset's glyph pair adjustment records.
/// It does not modify any original TTF, font atlas, or UI hierarchy.
/// </summary>
public static class ImportBabarSlabKerning
{
    [MenuItem("Tools/TextMeshPro/Import Babar Slab GPOS Kerning")]
    private static void ImportKerning()
    {
        TMP_FontAsset fontAsset = Selection.activeObject as TMP_FontAsset;
        if (fontAsset == null)
        {
            EditorUtility.DisplayDialog("Select a TMP font asset",
                "Select your Babar Slab Demo SDF font asset in the Project window first.", "OK");
            return;
        }

        string path = EditorUtility.OpenFilePanel("Open Babar Slab GPOS kerning CSV", Application.dataPath, "csv");
        if (string.IsNullOrEmpty(path)) return;

        if (!EditorUtility.DisplayDialog("Import kerning into selected asset?",
            "Selected TMP font asset:\n" + fontAsset.name +
            "\n\nThis adds missing glyph pair adjustments from the CSV. " +
            "Existing pair adjustments will be preserved.\n\n" +
            "Duplicate this font asset first if you want a safe backup.",
            "Import", "Cancel")) return;

        string[] lines;
        try { lines = File.ReadAllLines(path); }
        catch (Exception ex)
        {
            Debug.LogError("Could not open the kerning CSV: " + ex.Message);
            return;
        }

        // Resolving Unicode characters through the selected TMP asset means this works
        // even if atlas glyph order differs from the original source font's glyph order.
        Dictionary<uint, TMP_Character> chars = fontAsset.characterLookupTable;
        List<TMP_GlyphPairAdjustmentRecord> records = fontAsset.fontFeatureTable.glyphPairAdjustmentRecords;
        var existing = new HashSet<ulong>();
        foreach (TMP_GlyphPairAdjustmentRecord pair in records)
        {
            existing.Add(MakeKey(pair.firstAdjustmentRecord.glyphIndex,
                                 pair.secondAdjustmentRecord.glyphIndex));
        }

        int imported = 0, skipped = 0;
        float samplingSize = fontAsset.faceInfo.pointSize;
        Undo.RecordObject(fontAsset, "Import Babar Slab GPOS Kerning");
        for (int line = 1; line < lines.Length; line++) // Header is row 0.
        {
            string[] fields = lines[line].Trim().Split(',');
            if (fields.Length != 3 ||
                !uint.TryParse(fields[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out uint leftUnicode) ||
                !uint.TryParse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out uint rightUnicode) ||
                !float.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float xAdvanceEm) ||
                !chars.TryGetValue(leftUnicode, out TMP_Character leftChar) ||
                !chars.TryGetValue(rightUnicode, out TMP_Character rightChar))
            {
                skipped++;
                continue;
            }

            uint leftIndex = leftChar.glyphIndex;
            uint rightIndex = rightChar.glyphIndex;
            if (!existing.Add(MakeKey(leftIndex, rightIndex)))
            {
                skipped++;
                continue;
            }

            // CSV values are fractions of an em, e.g. P+A = -0.120 em.
            // TMP glyph metrics/adjustments are in the sampled face's point-size units.
            float xAdvance = xAdvanceEm * samplingSize;
            var left = new TMP_GlyphAdjustmentRecord(leftIndex,
                new TMP_GlyphValueRecord(0f, 0f, xAdvance, 0f));
            var right = new TMP_GlyphAdjustmentRecord(rightIndex,
                new TMP_GlyphValueRecord(0f, 0f, 0f, 0f));
            records.Add(new TMP_GlyphPairAdjustmentRecord(left, right));
            imported++;
        }

        fontAsset.fontFeatureTable.SortGlyphPairAdjustmentRecords();
        fontAsset.ReadFontAssetDefinition(); // Refresh TMP's internal lookup dictionaries.
        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        Debug.Log($"Kerning import into '{fontAsset.name}': {imported} pairs added, {skipped} skipped. " +
                  $"Glyph Adjustment Table total: {records.Count}.", fontAsset);
        EditorUtility.DisplayDialog("Kerning import finished",
            $"Added {imported} pairs; skipped {skipped}.\n" +
            "Use Kerning enabled on the TMP text component.\n" +
            "Missing characters like ! still require a fallback or a full font version.", "OK");
    }

    private static ulong MakeKey(uint left, uint right) => ((ulong)left << 32) | right;

    [MenuItem("Tools/TextMeshPro/Import Babar Slab GPOS Kerning", true)]
    private static bool ValidateImport() => Selection.activeObject is TMP_FontAsset;
}
#endif
