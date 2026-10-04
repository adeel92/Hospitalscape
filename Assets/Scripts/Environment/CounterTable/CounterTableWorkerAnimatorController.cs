using System.Collections.Generic;
using System;
using UnityEngine;

namespace Isometric.Environment
{

    public class CounterTableWorkerAnimatorController : MonoBehaviour
    {
        [Serializable]
        private class AnimatorStateInfo
        {
            public CounterTableWorkerAnimatorStates AnimatorState;
            public string StateName;
        }

        [SerializeField] Animator m_Animator;
        [SerializeField] List<AnimatorStateInfo> m_AnimatorStatesInfo;

        public void PlayIdleLeft()
        {
            m_Animator.Play(GetStateName(CounterTableWorkerAnimatorStates.IdleLeft));
        }
        public void PlayIdleDown()
        {
            m_Animator.Play(GetStateName(CounterTableWorkerAnimatorStates.IdleDown));
        }

        public void PlayWorkingDown()
        {
            m_Animator.Play(GetStateName(CounterTableWorkerAnimatorStates.WorkingDown));
        }

        public void PlayIdleToWorkDown()
        {
            m_Animator.Play(GetStateName(CounterTableWorkerAnimatorStates.IdleToWorkDown));
        }
        public void PlayWorkToIdleDown()
        {
            m_Animator.Play(GetStateName(CounterTableWorkerAnimatorStates.WorkToIdleDown));
        }

        private string GetStateName(CounterTableWorkerAnimatorStates state)
        {
            AnimatorStateInfo stateInfo = m_AnimatorStatesInfo.Find(info => info.AnimatorState == state);
            return stateInfo?.StateName;
        }
    }

    public enum CounterTableWorkerAnimatorStates
    {
        IdleLeft,
        IdleDown,
        WorkingDown,
        IdleToWorkDown,
        WorkToIdleDown
    }
}
