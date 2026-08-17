using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class PillowAnimationHandler : MonoBehaviour
{
    [SerializeField] List<AnimatorTriggerInfo> m_AnimatorStateInfo;
    [Space, SerializeField] Animator m_Animator;
    [SerializeField] SpriteRenderer m_PillowRenderer;
    [SerializeField] float m_FadeDuration = 0.15f;


    public void FadeIn()
    {
        m_PillowRenderer.gameObject.SetActive(false);
        Color color = m_PillowRenderer.color;
        color.a = 0f;
        m_PillowRenderer.color = color;
        m_PillowRenderer.gameObject.SetActive(true);
        m_PillowRenderer.DOFade(1f, m_FadeDuration);
    }
    public void FadeOut(bool disableOnFade)
    {
        m_PillowRenderer.DOFade(0f, m_FadeDuration).OnComplete(() =>
        {
            if(disableOnFade)
                m_PillowRenderer.gameObject.SetActive(false);
        });
    }

    public void PlayDirty(Action onStartCallback, bool doFade)
    {
        string parameterName = GetStateName(PillowAnimatorState.Dirty);
        if (!string.IsNullOrEmpty(parameterName))
        {
            onStartCallback?.Invoke();
            if (doFade)
                FadeIn();
            m_Animator.SetTrigger(parameterName);
        }
        else
        {
            Debug.LogWarning($"State {PillowAnimatorState.Dirty} not found in AnimatorStateInfo!");
        }
    }
    public void PlayCleaning(Action onStartCallback)
    {
        string parameterName = GetStateName(PillowAnimatorState.Cleaning);
        if (!string.IsNullOrEmpty(parameterName))
        {
            onStartCallback?.Invoke();
            m_Animator.SetTrigger(parameterName);
        }
        else
        {
            Debug.LogWarning($"State {PillowAnimatorState.Cleaning} not found in AnimatorStateInfo!");
        }
    }
    public void PlayCleaned(Action onStartCallback)
    {
        string parameterName = GetStateName(PillowAnimatorState.Cleaned);
        if (!string.IsNullOrEmpty(parameterName))
        {
            onStartCallback?.Invoke();
            m_Animator.SetTrigger(parameterName);
        }
        else
        {
            Debug.LogWarning($"State {PillowAnimatorState.Cleaned} not found in AnimatorStateInfo!");
        }
    }

    private string GetStateName(PillowAnimatorState state)
    {
        AnimatorTriggerInfo stateInfo = m_AnimatorStateInfo.Find(info => info.AnimatorState == state);
        return stateInfo?.ParameterName;
    }


    [Serializable]
    private class AnimatorTriggerInfo
    {
        public PillowAnimatorState AnimatorState;
        public string ParameterName;
    }
}

public enum PillowAnimatorState
{
    Dirty,
    Cleaning,
    Cleaned
}
