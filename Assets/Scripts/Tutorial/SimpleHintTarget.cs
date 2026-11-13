using System;
using System.Collections.Generic;
using Isometric.Data;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Isometric.Tutorial
{
    public class SimpleHintTarget : MonoBehaviour
    {
        [Serializable]
        protected class SimpleHintInfo
        {
            public HintKey HintKey;
            public DataConsumable DataConsumable;
            public bool HasAnimator;
            [ShowIf(nameof(HasAnimator)), AllowNesting]
            public Animator Animator;
            [ShowIf(nameof(HasAnimator)), AllowNesting]
            public string AnimatorStateName;
            [ShowIf(nameof(HasAnimator)), AllowNesting]
            public bool DisableAnimatorOnHide = true;
            public float ShowDelay = 0.5f;
            public float HideDelay = 2f;
            public UnityEvent OnHintShow;
            public UnityEvent OnHintHide;
        }

        // [SerializeField] TaskTrigger m_TargetTaskTrigger;
        [SerializeField] protected List<SimpleHintInfo> m_HintInfos = new();

        protected SimpleHintInfo m_CurrentHintInfo = null;
        protected bool m_IsShowing;
        protected bool m_IsHiding;


        /* private void OnEnable()
        {
            m_TargetTaskTrigger.OnTriggerClicked += DisableTargetHint;
        }
        private void OnDisable()
        {
            m_TargetTaskTrigger.OnTriggerClicked -= DisableTargetHint;
        } */

        public virtual void ShowHint(DataConsumable dataConsumable)
        {
            SimpleHintInfo simpleHintInfo = m_HintInfos.Find(x => x.DataConsumable == dataConsumable);
            
            if(simpleHintInfo == null)
            {
                Debug.LogWarning($"SimpleHintInfo with key '{dataConsumable}' does not exist in '{gameObject.name}'");
                return;
            }

            if (m_IsHiding)
            {
                return;
            }

            if (m_IsShowing)
            {
                CancelInvoke(nameof(HideHint));
                HideHint();
            }

            m_IsShowing = true;
            m_CurrentHintInfo =  simpleHintInfo;
            Invoke(nameof(EnableTargetHint), simpleHintInfo.ShowDelay);
        }
        public virtual void ShowHint(HintKey hintKey)
        {
            SimpleHintInfo simpleHintInfo = m_HintInfos.Find(x => x.HintKey == hintKey);
            
            if(simpleHintInfo == null)
            {
                Debug.LogWarning($"SimpleHintInfo with key '{hintKey}' does not exist in '{gameObject.name}'");
                return;
            }

            if (m_IsHiding)
            {
                return;
            }

            if (m_IsShowing)
            {
                CancelInvoke(nameof(HideHint));
                HideHint();
            }

            m_IsShowing = true;
            m_CurrentHintInfo =  simpleHintInfo;
            Invoke(nameof(EnableTargetHint), simpleHintInfo.ShowDelay);
        }
        protected void EnableTargetHint()
        {
            if (m_CurrentHintInfo.HasAnimator)
            {
                Animator animator = m_CurrentHintInfo.Animator;
                animator.enabled = true;
                animator.Play(m_CurrentHintInfo.AnimatorStateName);
            }

            m_CurrentHintInfo.OnHintShow?.Invoke();
            Invoke(nameof(HideHint), m_CurrentHintInfo.HideDelay);
        }

        public virtual void HideHint()
        {
            if(!m_IsShowing || m_IsHiding)
            {
                return;
            }

            m_IsHiding = true;
            if (m_CurrentHintInfo.HasAnimator)
            {
                Animator animator = m_CurrentHintInfo.Animator;
                animator.Rebind();
                animator.WriteDefaultValues();
                animator.Update(0);
                if (m_CurrentHintInfo.DisableAnimatorOnHide)
                {
                    animator.enabled = false;
                }
            }

            m_CurrentHintInfo.OnHintHide?.Invoke();
            ResetCurrentHintStatus();
        }
        /* private void DisableTargetHint()
        {
            CancelInvoke(nameof(HideHint));
            HideHint();
        } */

        private void ResetCurrentHintStatus()
        {
            m_CurrentHintInfo = null;
            m_IsShowing = false;
            m_IsHiding = false;
        }
    }    
}