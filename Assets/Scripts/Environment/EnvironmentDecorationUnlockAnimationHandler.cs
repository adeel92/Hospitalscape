using Arc;
using DG.Tweening;
using UnityEngine;

namespace Isometric.Environment
{
    public class EnvironmentDecorationUnlockAnimationHandler : MonoBehaviour
    {
        [Header("---Unlock Sequences---")]
        [SerializeField] PlayDoTweenSequence m_LockDisabledSequence;
        [SerializeField] PlayDoTweenSequence m_UnlockEnabledSequence;
        [SerializeField] PlayDoTweenSequence m_DesignSelectedSequence;

        [Header("---Design Preview Tween---")]
        [SerializeField] Transform m_UnlockedParent;
        [SerializeField] Vector3 m_PunchStrength = Vector3.one * 0.15f;
        [SerializeField] float m_PunchDuration = 0.25f;
        [SerializeField] int m_PunchVibration = 4;
        [SerializeField] float m_PunchElasticity = 0.5f;

        private Tweener m_PunchTween;


        public void PlayUnlockedSequence()
        {
            if(m_LockDisabledSequence != null)
            {
                m_LockDisabledSequence.PlaySequence();
            }
            else
            {
                m_UnlockEnabledSequence.PlaySequence();
            }
        }
        
        public void PlayDesignSelectedSequence()
        {
            if(m_DesignSelectedSequence != null)
            {
                m_DesignSelectedSequence.PlaySequence();
            }
        }

        public void PlayDesignPreviewTween()
        {
            if(m_PunchTween != null)
            {
                m_PunchTween.Kill();
            }
            m_UnlockedParent.localScale = Vector3.one;
            m_PunchTween = m_UnlockedParent.DOPunchScale(m_PunchStrength, m_PunchDuration, m_PunchVibration, m_PunchElasticity);
        }
    }
}
