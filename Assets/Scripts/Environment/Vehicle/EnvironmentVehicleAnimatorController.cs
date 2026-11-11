using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Isometric.Environment
{
    public class EnvironmentVehicleAnimatorController : MonoBehaviour
    {
        [Serializable]
        private class AnimatorStateInfo
        {
            public VehicleAnimatorState AnimatorState;
            public string StateName;
        }

        [SerializeField] Animator m_Animator;
        [SerializeField] List<AnimatorStateInfo> m_AnimatorStateInfo;
        private Coroutine m_CallbackCall = null;

        public void PlayState(VehicleAnimatorState state)
        {
            string stateName = GetStateName(state);
            
            if (!string.IsNullOrEmpty(stateName))
            {
                StopCallback();
                m_Animator.Play(stateName);
            }
            else
            {
                Debug.LogWarning($"State {state} not found in AnimatorStateInfo!");
            }
        }

        /// <summary>
        /// Play state with a callback 
        /// (callback will be canceled if the another state called before completeing this)
        /// </summary>
        public void PlayState(VehicleAnimatorState state, Action callback)
        {
            string stateName = GetStateName(state);
            if (!string.IsNullOrEmpty(stateName))
            {
                StopCallback();
                m_CallbackCall = StartCoroutine(PlayStateWithCallback(stateName, callback));
            }
            else
            {
                Debug.LogWarning($"State {state} not found in AnimatorStateInfo!");
            }
        }

        public void StopCallback()
        {
            if (m_CallbackCall != null)
            {
                StopCoroutine(m_CallbackCall);
                m_CallbackCall = null;
            }
        }

        private IEnumerator PlayStateWithCallback(string stateName, Action callback)
        {
            m_Animator.Play(stateName);

            yield return null;

            UnityEngine.AnimatorStateInfo currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);

            while (currentAnimatorStateInfo.IsName(stateName) &&
                   currentAnimatorStateInfo.normalizedTime < 1.0f)
            {
                yield return null;
                currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            }

            callback?.Invoke();
        }

        private string GetStateName(VehicleAnimatorState state)
        {
            AnimatorStateInfo stateInfo = m_AnimatorStateInfo.Find(info => info.AnimatorState == state);
            return stateInfo?.StateName;
        }
    }

    public enum VehicleAnimatorState
    {
        None,
        DriveUp,
        DriveDown,
        DriveLeft,
        DriveRight,
        ParkUp,
        ParkDown,
        ParkLeft,
        ParkRight,
    }
}