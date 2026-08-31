using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isometric.Environment
{
    public class StationCustomerInItemAnimationHandler : MonoBehaviour
    {
        [SerializeField] List<AnimatorTriggerInfo> m_AnimatorTriggerInfo;
        [Space, SerializeField] Animator m_Animator;


        /* public void PlayState(StationCustomerInItemAnimatorStates state, Action onStartCallback)
        {
            string parameterName = GetStateName(state);
            if (!string.IsNullOrEmpty(parameterName))
            {
                onStartCallback?.Invoke();
                m_Animator.SetTrigger(parameterName);
            }
            else
            {
                Debug.LogWarning($"State {state} not found in AnimatorStateInfo of {nameof(StationCustomerInItemAnimationHandler)}!");
            }
        } */
        public void PlayState(StationCustomerInItemAnimatorStates state, Action onStartCallback, Action onCompleteCallback)
        {
            AnimatorTriggerInfo triggerInfo = GetTriggerInfo(state);
            if(triggerInfo == null)
            {
                Debug.LogWarning($"TriggerInfo for {state} state not found in AnimatorStateInfo of {nameof(StationCustomerInItemAnimationHandler)}!");
                return;
            }
            if(string.IsNullOrEmpty(triggerInfo.ParameterName))
            {
                Debug.LogWarning($"ParameterName for {state} state not found in AnimatorStateInfo of {nameof(StationCustomerInItemAnimationHandler)}!");
                return;
            }
            if(string.IsNullOrEmpty(triggerInfo.StateName))
            {
                Debug.LogWarning($"StateName for {state} state not found in AnimatorStateInfo of {nameof(StationCustomerInItemAnimationHandler)}!");
                return;
            }

            onStartCallback?.Invoke();
            ResetAllTriggers();
            if(onCompleteCallback == null)
            {
                m_Animator.SetTrigger(triggerInfo.ParameterName);
            }
            else
            {
                StartCoroutine(PlayStateWithCompleteCallback(triggerInfo, onCompleteCallback));
            }
        }
        private IEnumerator PlayStateWithCompleteCallback(AnimatorTriggerInfo triggerInfo, Action onCompleteCallback)
        {
            m_Animator.SetTrigger(triggerInfo.ParameterName);

            yield return null;
            AnimatorStateInfo currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);

            while (currentAnimatorStateInfo.IsName(triggerInfo.StateName) &&
                   currentAnimatorStateInfo.normalizedTime < 1.0f)
            {
                yield return null;
            }

            onCompleteCallback?.Invoke();
        }

        private void ResetAllTriggers()
        {
            foreach (var triggerInfo in m_AnimatorTriggerInfo)
            {
                if (!string.IsNullOrEmpty(triggerInfo.ParameterName))
                {
                    m_Animator.ResetTrigger(triggerInfo.ParameterName);
                }
            }
        }

        private AnimatorTriggerInfo GetTriggerInfo(StationCustomerInItemAnimatorStates state)
        {
            AnimatorTriggerInfo stateInfo = m_AnimatorTriggerInfo.Find(info => info.AnimatorState == state);
            return stateInfo;
        }
        private string GetStateName(StationCustomerInItemAnimatorStates state)
        {
            AnimatorTriggerInfo stateInfo = m_AnimatorTriggerInfo.Find(info => info.AnimatorState == state);
            return stateInfo?.ParameterName;
        }


        [Serializable]
        private class AnimatorTriggerInfo
        {
            public StationCustomerInItemAnimatorStates AnimatorState;
            public string ParameterName;
            public string StateName;
        }
    }

    public enum StationCustomerInItemAnimatorStates
    {
        Enable,
        Disable,
        Emit,
        MoveInside,
        MoveOutside,
        DisplayWave,
        ScanComplete,
        Hold,
        On,
        Off,
        Glow,
        Open,
        Close,
        ScalePingPong,
        StandBy
    }
}