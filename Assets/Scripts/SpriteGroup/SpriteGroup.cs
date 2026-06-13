using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Arc
{
    [DisallowMultipleComponent]
    public class SpriteGroup : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField] float m_Alpha = 1f;
        private float m_PreAlpha = 1;

        [Header("-turn this on if you need more optimized performance")]
        [Space(-10)]
        [Header("enable this only if your sprites hirearchy or sprites alpha value do not change")]
        [SerializeField] bool m_DontRefershSpriteHirearchy = false;

        [Serializable]
        private class SpriteRendererInfo
        {
            public SpriteRenderer SpriteRenderer;
            public float BaseAlpha;
            public int LastAppliedAByte = -1;
        }

        [SerializeField, ReadOnly]
        private List<SpriteRendererInfo> m_SpriteRenderersInfo;

        private CanvasGroup m_CanvasGroup;
        private float m_LastCanvasAlpha = 1f;

        private void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_LastCanvasAlpha = m_CanvasGroup ? m_CanvasGroup.alpha : 1f;

            RefreshCache();
            ApplyAlpha();
        }

        private void Update()
        {
            if (m_CanvasGroup != null)
            {
                float canvasGroupAlpha = m_CanvasGroup.alpha;
                if (!Mathf.Approximately(canvasGroupAlpha, m_LastCanvasAlpha))
                {
                    m_LastCanvasAlpha = canvasGroupAlpha;
                    if (!m_DontRefershSpriteHirearchy)
                    {
                        RefreshCache();
                    }
                    ApplyAlpha();
                }
            }
        }

        [ContextMenu("RefreshCache")]
        private void RefreshCache()
        {
            if (m_SpriteRenderersInfo == null)
                m_SpriteRenderersInfo = new List<SpriteRendererInfo>();

            foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
            {
                var info = m_SpriteRenderersInfo.Find(x => x.SpriteRenderer == sr);

                if (info == null)
                {
                    m_SpriteRenderersInfo.Add(new SpriteRendererInfo
                    {
                        SpriteRenderer = sr,
                        BaseAlpha = sr.color.a,
                        LastAppliedAByte = Mathf.RoundToInt(sr.color.a * 255f)
                    });
                }
                else
                {
                    int currentByte = Mathf.RoundToInt(sr.color.a * 255f);
                    if (info.LastAppliedAByte >= 0 && currentByte != info.LastAppliedAByte)
                    {
                        // If sprite color was changed externally, compute BaseAlpha relative to effective alpha
                        float denom = Mathf.Max(0.0001f, GetEffectiveAlpha());
                        info.BaseAlpha = Mathf.Clamp01(sr.color.a / denom);
                    }
                }
            }

            m_SpriteRenderersInfo.RemoveAll(x => x == null || x.SpriteRenderer == null);
        }

        // Calculate the alpha that will actually be applied to sprites.
        // If a CanvasGroup exists on the same GameObject, it multiplies the user m_Alpha.
        private float GetEffectiveAlpha()
        {
            return m_CanvasGroup != null ? m_Alpha * m_CanvasGroup.alpha : m_Alpha;
        }

        [ContextMenu("ApplyAlpha")]
        private void ApplyAlpha()
        {
            if (m_SpriteRenderersInfo == null)
                return;

            float effectiveAlpha = GetEffectiveAlpha();

            foreach (var info in m_SpriteRenderersInfo)
            {
                var sr = info.SpriteRenderer;
                if (!sr)
                    continue;

                int desiredByte = Mathf.Clamp(
                    Mathf.RoundToInt(info.BaseAlpha * 255f * effectiveAlpha),
                    0,
                    255
                );

                // Skip if unchanged
                if (info.LastAppliedAByte == desiredByte)
                    continue;

                // Apply using Color32 to avoid float precision issues
                Color32 c32 = (Color32)sr.color;
                c32.a = (byte)desiredByte;
                sr.color = (Color)c32;

                info.LastAppliedAByte = desiredByte;
            }
        }

        public void SetAlpha(float value)
        {
            m_Alpha = Mathf.Clamp01(value);
            m_PreAlpha = m_Alpha;
            if (!m_DontRefershSpriteHirearchy)
            {
                RefreshCache();
            }
            ApplyAlpha();
        }

        public float GetAlpha()
        {
            return m_Alpha;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // keep CanvasGroup reference up to date in Editor when component added/removed
            if (m_CanvasGroup == null)
                m_CanvasGroup = GetComponent<CanvasGroup>();

            if (!Mathf.Approximately(m_PreAlpha, m_Alpha))
            {
                m_PreAlpha = m_Alpha;
                if (!m_DontRefershSpriteHirearchy)
                {
                    RefreshCache();
                }
                ApplyAlpha();
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
