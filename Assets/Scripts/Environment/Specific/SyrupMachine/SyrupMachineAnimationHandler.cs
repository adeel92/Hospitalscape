using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;


namespace Isometric.Environment
{
    public class SyrupMachineAnimationHandler : MonoBehaviour
    {
        [SerializeField] Animator m_Animator;
        [SerializeField] List<AnimatorTriggerInfo> m_AnimatorTriggerInfo;
        [SerializeField, ReadOnly] string m_ProductionSpeedMultiplier = "ProductionSpeed";
        [Space, SerializeField] ParticlesEnabler m_ProductionParticlesEnabler;
        [SerializeField] ParticlesEnabler m_OutputParticlesEnabler;

        
        // OnDurationStart
        public void PlaySyrupProductionAnimation(float duration)
        {
            AnimatorTriggerInfo triggerInfo = GetTriggerInfo(SyrupMachineAnimatorStates.Produce);
            if(triggerInfo == null)
            {
                Debug.LogWarning($"TriggerInfo for {SyrupMachineAnimatorStates.Produce} state not found in AnimatorStateInfo of {nameof(SyrupMachineAnimationHandler)}!");
            }
            if(string.IsNullOrEmpty(triggerInfo.StateName))
            {
                Debug.LogWarning($"StateName for {SyrupMachineAnimatorStates.Produce} state not found in AnimatorStateInfo of {nameof(SyrupMachineAnimationHandler)}!");
            }
            
            if (duration <= 0f)
            {
                Debug.LogWarning("Animation duration must be greater than zero to avoid infinite clip speed!");
                return;
            }

            float speedMultiplier = triggerInfo.AnimationClip.length / duration;
            m_Animator.SetFloat(m_ProductionSpeedMultiplier, speedMultiplier);
            m_Animator.Play(triggerInfo.StateName, 0, 0f);
        }

        public void PlayProductionParticles()
        {
            m_ProductionParticlesEnabler.PlayParticles();
        }
        public void StopProductionParticles()
        {
            m_ProductionParticlesEnabler.StopParticles();
        }

        public void PlayOutputParticles()
        {
            m_OutputParticlesEnabler.PlayParticles();
        }
        public void StopOutputParticles()
        {
            m_OutputParticlesEnabler.StopParticles();
        }

        private AnimatorTriggerInfo GetTriggerInfo(SyrupMachineAnimatorStates state)
        {
            AnimatorTriggerInfo stateInfo = m_AnimatorTriggerInfo.Find(info => info.AnimatorState == state);
            return stateInfo;
        }

        [Serializable]
        private class AnimatorTriggerInfo
        {
            public SyrupMachineAnimatorStates AnimatorState;
            // public string ParameterName;
            public string StateName;
            public AnimationClip AnimationClip;
        }
        public enum SyrupMachineAnimatorStates
        {
            Produce
        }
    }
}