using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPCurvedText : MonoBehaviour
{
    [Header("Curve Settings")]
    [SerializeField]
    private AnimationCurve m_Curve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f)
    );

    [SerializeField]
    private float m_CurveStrength = 10f;

    [SerializeField]
    private bool m_RotateCharacters = true;

    private TextMeshProUGUI m_Text;

    private void OnEnable()
    {
        m_Text = GetComponent<TextMeshProUGUI>();

        m_Text.OnPreRenderText += ApplyCurvature;

        RefreshText();
    }

    private void OnDisable()
    {
        if (m_Text != null)
            m_Text.OnPreRenderText -= ApplyCurvature;

#if UNITY_EDITOR
        EditorApplication.delayCall -= RefreshInEditor;
#endif
    }

    private void ApplyCurvature(TMP_TextInfo textInfo)
    {
        if (textInfo.characterCount == 0)
            return;

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        // Determine visible text boundaries
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo character = textInfo.characterInfo[i];

            if (!character.isVisible)
                continue;

            minX = Mathf.Min(minX, character.origin);
            maxX = Mathf.Max(maxX, character.xAdvance);
        }

        float width = maxX - minX;

        if (width <= 0.0001f)
            return;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo character = textInfo.characterInfo[i];

            if (!character.isVisible)
                continue;

            int materialIndex = character.materialReferenceIndex;
            int vertexIndex = character.vertexIndex;

            Vector3[] vertices =
                textInfo.meshInfo[materialIndex].vertices;

            // Character horizontal center
            float centerX =
                (character.origin + character.xAdvance) * 0.5f;

            float normalizedX =
                Mathf.Clamp01((centerX - minX) / width);

            // Calculate vertical displacement
            float offsetY =
                m_Curve.Evaluate(normalizedX) * m_CurveStrength;

            // Calculate character rotation
            float angle = 0f;

            if (m_RotateCharacters)
            {
                const float delta = 0.001f;

                float left = m_Curve.Evaluate(normalizedX - delta);
                float right = m_Curve.Evaluate(normalizedX + delta);

                float slope =
                    ((right - left) / (2f * delta))
                    * m_CurveStrength / width;

                angle = Mathf.Atan(slope) * Mathf.Rad2Deg;
            }

            // Rotate around character baseline center
            Vector3 pivot = new Vector3(
                centerX,
                character.baseLine,
                0f
            );

            Quaternion rotation =
                Quaternion.Euler(0f, 0f, angle);

            Vector3 displacement =
                new Vector3(0f, offsetY, 0f);

            for (int j = 0; j < 4; j++)
            {
                int index = vertexIndex + j;

                vertices[index] =
                    rotation * (vertices[index] - pivot)
                    + pivot
                    + displacement;
            }
        }
    }

    private void RefreshText()
    {
        if (m_Text == null || !m_Text.isActiveAndEnabled)
            return;

        // TMP generates fresh vertices before invoking
        // OnPreRenderText, where our curvature is applied.
        m_Text.ForceMeshUpdate();
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        EditorApplication.delayCall -= RefreshInEditor;
        EditorApplication.delayCall += RefreshInEditor;
    }

    private void RefreshInEditor()
    {
        EditorApplication.delayCall -= RefreshInEditor;

        if (this == null || !isActiveAndEnabled)
            return;

        if (m_Text == null)
            m_Text = GetComponent<TextMeshProUGUI>();

        RefreshText();
    }

#endif
}