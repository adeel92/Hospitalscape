using System.Collections.Generic;
using UnityEngine;
using System;

namespace Isometric.Environment
{
    public class EnvironmentPropAnimationHandler : MonoBehaviour
    {
        [Serializable]
        private class AnimatorStateInfo
        {
            public EnvironmentPropAnimatorState AnimatorState;
            public string StateName;
        }

        [SerializeField] Animator m_Animator;
        [SerializeField] EnvironmentPropAnimatorState m_DefaultState;
        [SerializeField] List<AnimatorStateInfo> m_AnimatorStateInfo;


        private void Awake()
        {
            GlobalEventHolder.OnMenuEnvironmentShown += PlayDefaultState;
        }
        private void OnDestroy()
        {
            GlobalEventHolder.OnMenuEnvironmentShown -= PlayDefaultState;
        }

        public void PlayState(EnvironmentPropAnimatorState state)
        {
            string stateName = GetStateName(state);
            
            if (!string.IsNullOrEmpty(stateName))
            {
                m_Animator.Play(stateName);
            }
            else
            {
                Debug.LogWarning($"State {state} not found in AnimatorStateInfo!");
            }
        }

        public void PlayState(string stateName)
        {
            if (!string.IsNullOrEmpty(stateName))
            {
                m_Animator.Play(stateName);
            }
            else
            {
                Debug.LogWarning($"State {stateName} not found in AnimatorStateInfo!");
            }
        }

        private void PlayDefaultState()
        {
            string stateName = GetStateName(m_DefaultState);
            
            if (!string.IsNullOrEmpty(stateName))
            {
                m_Animator.Play(stateName);
            }
            else
            {
                Debug.LogWarning($"State {m_DefaultState} not found in AnimatorStateInfo!");
            }
        }

        private string GetStateName(EnvironmentPropAnimatorState state)
        {
            AnimatorStateInfo stateInfo = m_AnimatorStateInfo.Find(info => info.AnimatorState == state);
            return stateInfo?.StateName;
        }
    }

    //Down or right represents directions
    public enum EnvironmentPropAnimatorState
    {
        Init,
        Enable,
        Display,
        Sway,
        Unlock,
    }
}