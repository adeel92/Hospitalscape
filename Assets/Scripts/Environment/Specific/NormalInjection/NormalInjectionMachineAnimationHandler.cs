using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;


namespace Isometric.Environment
{
    public class NormalInjectionMachineAnimationHandler : MonoBehaviour
    {
        [SerializeField] Animator m_Animator;
        [SerializeField] List<AnimatorTriggerInfo> m_AnimatorTriggerInfo;
        [SerializeField] float m_DelayBeforeNextProductionAnimation = 0.75f;
        [SerializeField, ReadOnly] bool m_IsAlreadyProducing;

        
        // OnOrderOutSuccessful
        public void PlayGauzeProductionAnimation()
        {
            StartCoroutine(PlayState(NormalInjectionMachineAnimatorStates.Produce));
        }

        private IEnumerator PlayState(NormalInjectionMachineAnimatorStates state)
        {
            if (m_IsAlreadyProducing)
            {
                yield break;    
            }

            AnimatorTriggerInfo triggerInfo = GetTriggerInfo(state);
            if(triggerInfo == null)
            {
                Debug.LogWarning($"TriggerInfo for {state} state not found in AnimatorStateInfo of {nameof(NormalInjectionMachineAnimationHandler)}!");
                yield break;
            }
            if(string.IsNullOrEmpty(triggerInfo.ParameterName))
            {
                Debug.LogWarning($"ParameterName for {state} state not found in AnimatorStateInfo of {nameof(NormalInjectionMachineAnimationHandler)}!");
                yield break;
            }
            if(string.IsNullOrEmpty(triggerInfo.StateName))
            {
                Debug.LogWarning($"StateName for {state} state not found in AnimatorStateInfo of {nameof(NormalInjectionMachineAnimationHandler)}!");
                yield break;
            }

            m_IsAlreadyProducing = true;
            ResetAllTriggers();
            m_Animator.SetTrigger(triggerInfo.ParameterName);
            
            yield return new WaitForSeconds(m_DelayBeforeNextProductionAnimation);
            m_IsAlreadyProducing = false;
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

        private AnimatorTriggerInfo GetTriggerInfo(NormalInjectionMachineAnimatorStates state)
        {
            AnimatorTriggerInfo stateInfo = m_AnimatorTriggerInfo.Find(info => info.AnimatorState == state);
            return stateInfo;
        }
        private string GetStateName(NormalInjectionMachineAnimatorStates state)
        {
            AnimatorTriggerInfo stateInfo = m_AnimatorTriggerInfo.Find(info => info.AnimatorState == state);
            return stateInfo?.ParameterName;
        }

        [Serializable]
        private class AnimatorTriggerInfo
        {
            public NormalInjectionMachineAnimatorStates AnimatorState;
            public string ParameterName;
            public string StateName;
        }
        public enum NormalInjectionMachineAnimatorStates
        {
            Produce
        }
    }
}