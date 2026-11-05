using System;
using Arc;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Isometric.Tutorial
{
    public class TutorialSoftFocus : MonoBehaviour
    {
        [SerializeField] PlayDoTweenSequence m_EnablingSequence;
        [SerializeField] PlayDoTweenSequence m_DisablingSequence;

        [Space, SerializeField] Image m_FocusImage;

        private Sequence m_EnablingSequenceCache;
        private Sequence m_DisablingSequenceCache;

        public Image FocusImage => m_FocusImage;


        public void PlayEnablingTween(Action onStart, Action onComplete)
        {
            if(m_EnablingSequence != null)
            {
                if(m_DisablingSequenceCache != null)
                {
                    m_DisablingSequence.Stop();
                    m_DisablingSequenceCache = null;
                }
                m_EnablingSequenceCache = m_EnablingSequence.PlaySequence(onStart, onComplete);
            }
            else
            {
                Debug.LogWarning($"Enabling Sequence is null inside {nameof(TutorialSoftFocus)}");
            }
        }

        public void PlayDisablingTween(Action onStart, Action onComplete)
        {
            if(m_DisablingSequence != null)
            {
                if(m_EnablingSequenceCache != null)
                {
                    m_EnablingSequence.Stop();
                    m_EnablingSequenceCache = null;
                }
                m_DisablingSequenceCache = m_DisablingSequence.PlaySequence(onStart, onComplete);
            }
            else
            {
                Debug.LogWarning($"Disabling Sequence is null inside {nameof(TutorialSoftFocus)}");
            }
        }
    }
}