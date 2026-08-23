using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class BlanketAnimationHandler : MonoBehaviour
{
    [SerializeField] List<AnimatorTriggerInfo> m_AnimatorStateInfo;
    [Space, SerializeField] Animator m_Animator;
    [SerializeField] SpriteRenderer m_BlanketRenderer;
    [SerializeField] float m_FadeDuration = 0.15f;


    public void FadeIn()
    {
        m_BlanketRenderer.DOKill();
        m_BlanketRenderer.gameObject.SetActive(false);
        Color color = m_BlanketRenderer.color;
        color.a = 0f;
        m_BlanketRenderer.color = color;
        m_BlanketRenderer.gameObject.SetActive(true);
        m_BlanketRenderer.DOFade(1f, m_FadeDuration);
    }
    public void FadeOut(bool disableOnFade)
    {
        m_BlanketRenderer.DOKill();
        m_BlanketRenderer.DOFade(0f, m_FadeDuration).OnComplete(() =>
        {
            if(disableOnFade)
                m_BlanketRenderer.gameObject.SetActive(false);
        });
    }

    public void PlayWrap(Action onStartCallback, bool doFade)
    {
        AnimatorTriggerInfo triggerInfo = GetTriggerInfo(BlanketAnimatorState.Wrap);
        string parameterName = triggerInfo.ParameterName;
        string stateName = triggerInfo.NextStateName;
        if (!string.IsNullOrEmpty(parameterName))
        {
            /* AnimatorStateInfo currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            if(currentAnimatorStateInfo.IsName(stateName))  // State already running
                return; */

            onStartCallback?.Invoke();
            if (doFade)
                FadeIn();
            m_Animator.SetTrigger(parameterName);
        }
        else
        {
            Debug.LogWarning($"State {BlanketAnimatorState.Wrap} not found in AnimatorStateInfo!");
        }
    }
    public void PlayUnmade(Action onStartCallback)
    {
        // Debug.Log($"ADEEL... {gameObject.name} : PlayUnmade()");
        AnimatorTriggerInfo triggerInfo = GetTriggerInfo(BlanketAnimatorState.Unmade);
        string parameterName = triggerInfo.ParameterName;
        string stateName = triggerInfo.NextStateName;
        if (!string.IsNullOrEmpty(parameterName))
        {
            /* AnimatorStateInfo currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            if(currentAnimatorStateInfo.IsName(stateName))  // State already running
                return; */

            onStartCallback?.Invoke();
            m_Animator.SetTrigger(parameterName);
        }
        else
        {
            Debug.LogWarning($"State {BlanketAnimatorState.Unmade} not found in AnimatorStateInfo!");
        }
    }
    public void PlayDirty(Action onStartCallback, bool doFade)
    {
        AnimatorTriggerInfo triggerInfo = GetTriggerInfo(BlanketAnimatorState.Dirty);
        string parameterName = triggerInfo.ParameterName;
        string stateName = triggerInfo.NextStateName;
        if (!string.IsNullOrEmpty(parameterName))
        {
            /* AnimatorStateInfo currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            if(currentAnimatorStateInfo.IsName(stateName))  // State already running
                return; */

            onStartCallback?.Invoke();
            if (doFade)
                FadeIn();
            m_Animator.SetTrigger(parameterName);
        }
        else
        {
            Debug.LogWarning($"State {BlanketAnimatorState.Dirty} not found in AnimatorStateInfo!");
        }
    }
    public void PlayCleaning(Action onStartCallback)
    {
        AnimatorTriggerInfo triggerInfo = GetTriggerInfo(BlanketAnimatorState.Cleaning);
        string parameterName = triggerInfo.ParameterName;
        string stateName = triggerInfo.NextStateName;
        if (!string.IsNullOrEmpty(parameterName))
        {
            /* AnimatorStateInfo currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            if(currentAnimatorStateInfo.IsName(stateName))  // State already running
                return; */

            onStartCallback?.Invoke();
            m_Animator.SetTrigger(parameterName);
        }
        else
        {
            Debug.LogWarning($"State {BlanketAnimatorState.Cleaning} not found in AnimatorStateInfo!");
        }
    }
    public void PlayCleaned(Action onStartCallback, bool doFade)
    {
        AnimatorTriggerInfo triggerInfo = GetTriggerInfo(BlanketAnimatorState.Cleaned);
        string parameterName = triggerInfo.ParameterName;
        string stateName = triggerInfo.NextStateName;
        if (!string.IsNullOrEmpty(parameterName))
        {
            /* AnimatorStateInfo currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            if(currentAnimatorStateInfo.IsName(stateName))  // State already running
                return; */

            onStartCallback?.Invoke();
            if (doFade)
                FadeIn();
            m_Animator.SetTrigger(parameterName);
        }
        else
        {
            Debug.LogWarning($"State {BlanketAnimatorState.Cleaned} not found in AnimatorStateInfo!");
        }
    }

    private AnimatorTriggerInfo GetTriggerInfo(BlanketAnimatorState state)
    {
        AnimatorTriggerInfo stateInfo = m_AnimatorStateInfo.Find(info => info.AnimatorState == state);
        return stateInfo;
    }
    private string GetStateName(BlanketAnimatorState state)
    {
        AnimatorTriggerInfo stateInfo = m_AnimatorStateInfo.Find(info => info.AnimatorState == state);
        return stateInfo?.ParameterName;
    }


    [Serializable]
    private class AnimatorTriggerInfo
    {
        public BlanketAnimatorState AnimatorState;
        public string ParameterName;
        public string NextStateName;
    }
}

public enum BlanketAnimatorState
{
    Wrap,
    Unmade,
    Dirty,
    Cleaning,
    Cleaned
}
