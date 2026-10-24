using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;


namespace Isometric.Environment
{
    public class VitaminMachineAnimationHandler : MonoBehaviour
    {
        [SerializeField] Animator m_Animator;
        [SerializeField] List<AnimatorTriggerInfo> m_AnimatorTriggerInfo;
        [SerializeField, ReadOnly] string m_ProductionSpeedMultiplier = "ProductionSpeed";
        [Space, SerializeField] ParticlesEnabler m_MachineParticlesEnabler;

        
        // OnDurationStart
        public void PlayVitaminProductionAnimation(float duration)
        {
            AnimatorTriggerInfo triggerInfo = GetTriggerInfo(VitaminMachineAnimatorStates.Produce);
            if(triggerInfo == null)
            {
                Debug.LogWarning($"TriggerInfo for {VitaminMachineAnimatorStates.Produce} state not found in AnimatorStateInfo of {nameof(VitaminMachineAnimationHandler)}!");
            }
            if(string.IsNullOrEmpty(triggerInfo.StateName))
            {
                Debug.LogWarning($"StateName for {VitaminMachineAnimatorStates.Produce} state not found in AnimatorStateInfo of {nameof(VitaminMachineAnimationHandler)}!");
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
            m_MachineParticlesEnabler.PlayParticles();
        }
        public void StopProductionParticles()
        {
            m_MachineParticlesEnabler.StopParticles();
        }

        private AnimatorTriggerInfo GetTriggerInfo(VitaminMachineAnimatorStates state)
        {
            AnimatorTriggerInfo stateInfo = m_AnimatorTriggerInfo.Find(info => info.AnimatorState == state);
            return stateInfo;
        }

        [Serializable]
        private class AnimatorTriggerInfo
        {
            public VitaminMachineAnimatorStates AnimatorState;
            // public string ParameterName;
            public string StateName;
            public AnimationClip AnimationClip;
        }
        public enum VitaminMachineAnimatorStates
        {
            Produce
        }
    }
}