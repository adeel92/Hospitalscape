using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Isometric.Customer
{
    public class CustomerAnimatorController : MonoBehaviour
    {
        private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");


        [Serializable]
        private class AnimatorStateInfo
        {
            public CustomerAnimatorState AnimatorState;
            public string StateName;
        }

        [SerializeField] Animator m_Animator;
        [SerializeField] float m_WalkSpeedMultiplier = 1;
        [SerializeField] List<AnimatorStateInfo> m_AnimatorStateInfo;
        private Coroutine m_CallbackCall = null;

        public void PlayState(CustomerAnimatorState state)
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
        public void PlayState(CustomerAnimatorState state, Action callback)
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

        public void SetWalkSpeed(float speed)
        {
            m_Animator.SetFloat(WalkSpeed, speed * m_WalkSpeedMultiplier);
        }

        private string GetStateName(CustomerAnimatorState state)
        {
            AnimatorStateInfo stateInfo = m_AnimatorStateInfo.Find(info => info.AnimatorState == state);
            return stateInfo?.StateName;
        }
    }

    //Down or right represents directions
    public enum CustomerAnimatorState
    {
        WalkRight,
        WalkLeft,
        WalkUp,
        WalkDown,
        StandingIdleNeutral,
        StandingIdleAngry,
        StandingIdleFurious,
        StandingIdleHappyDown,
        StandingIdleHappyRight,
        StandingIdleSinging,
        SittingDown,
        SititngRight,
        SittingIdleNeurtalDown,
        SittingIdleNeurtalRight,
        SittingIdleAngryDown,
        SittingIdleAngryRight,
        SittingIdleFuriousDown,
        SittingIdleFuriousRight,
        SittingDrinkingRight,
        PickedUpNeutral,
        PickedUpAngry,
        PickedUpFurious,
        LayingPain,
        LayingHappy,
    }
}