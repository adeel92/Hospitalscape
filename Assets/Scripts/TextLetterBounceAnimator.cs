using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextLetterBounceAnimator : MonoBehaviour
{
    [Header("---Reference---")]
    [SerializeField] TMP_Text m_Text;

    [Header("---Animation Settings---")]
    [SerializeField] float m_MaxScale = 1.2f;
    [SerializeField] float m_MinScale = 0.9f;
    [SerializeField, Min(0.01f)] float m_PhaseDuration = 0.15f;

    [Header("---Playback Settings---")]
    [SerializeField] bool m_PlayOnEnable = true;
    [SerializeField] bool m_Loop = true;
    [SerializeField, Min(0f)] float m_LoopDelay = 1.5f;
    [SerializeField] bool m_IgnoreTimeScale = true;

    private Sequence m_Sequence;
    private TMP_MeshInfo[] m_OriginalMesh;

    private void OnEnable()
    {
        if (m_PlayOnEnable)
            PlayAnimation();
    }

    public void PlayAnimation()
    {
        if(!isActiveAndEnabled)
        {
            return;
        }

        StopAnimation();

        if (m_Text == null)
            return;

        m_Text.ForceMeshUpdate();

        TMP_TextInfo textInfo = m_Text.textInfo;

        int visibleCount = 0;

        // Count visible characters, excluding spaces.
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (textInfo.characterInfo[i].isVisible)
                visibleCount++;
        }

        if (visibleCount == 0)
            return;

        // Cache the original mesh data.
        m_OriginalMesh = textInfo.CopyMeshInfoVertexData();

        float totalDuration =
            (visibleCount + 2) * m_PhaseDuration;

        m_Sequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, m_IgnoreTimeScale);

        // Single tween controlling all characters.
        m_Sequence.Append(
            DOVirtual.Float(
                0f,
                totalDuration,
                totalDuration,
                AnimateLetters)
            .SetEase(Ease.Linear));

        if (m_Loop)
        {
            m_Sequence.AppendInterval(m_LoopDelay);
            m_Sequence.SetLoops(-1, LoopType.Restart);
        }

        m_Sequence.Play();
    }

    private void AnimateLetters(float elapsed)
    {
        TMP_TextInfo textInfo = m_Text.textInfo;
        int letterIndex = 0;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo character = textInfo.characterInfo[i];

            if (!character.isVisible)
                continue;

            float time = elapsed - letterIndex * m_PhaseDuration;
            letterIndex++;

            float scale = GetLetterScale(time);

            int materialIndex = character.materialReferenceIndex;
            int vertexIndex = character.vertexIndex;

            Vector3[] original =
                m_OriginalMesh[materialIndex].vertices;

            Vector3[] vertices =
                textInfo.meshInfo[materialIndex].vertices;

            // Calculate the center of the character.
            Vector3 center =
                (original[vertexIndex] +
                 original[vertexIndex + 2]) * 0.5f;

            // Uniformly scale the character around its center.
            for (int j = 0; j < 4; j++)
            {
                int index = vertexIndex + j;

                vertices[index] = center +
                    (original[index] - center) * scale;
            }
        }

        // Submit updated vertices once per animation callback.
        m_Text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }

    private float GetLetterScale(float time)
    {
        float duration = m_PhaseDuration;

        // Character is not currently animating.
        if (time <= 0f || time >= duration * 3f)
            return 1f;

        // Phase 1: Scale from 1 to 1.2.
        if (time < duration)
            return Mathf.Lerp(
                1f, m_MaxScale, time / duration);

        // Phase 2: Scale from 1.2 to 0.9.
        if (time < duration * 2f)
            return Mathf.Lerp(
                m_MaxScale, m_MinScale,
                (time - duration) / duration);

        // Phase 3: Scale from 0.9 to 1.
        return Mathf.Lerp(
            m_MinScale, 1f,
            (time - duration * 2f) / duration);
    }

    public void StopAnimation()
    {
        m_Sequence?.Kill();
        m_Sequence = null;

        // Restore the original text appearance.
        if (m_OriginalMesh != null && m_Text != null)
            m_Text.ForceMeshUpdate();

        m_OriginalMesh = null;
    }

    private void OnDisable()
    {
        StopAnimation();
    }
}