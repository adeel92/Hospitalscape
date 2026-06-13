using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Isometric.Environment
{
    public class StationLockedAnimatorController : MonoBehaviour
    {
        [Serializable]
        private class AnimatorCallbackInfo
        {
            public string AnimationStateName;
            public UnityEvent OnUnlockAnimationStart;
            public UnityEvent OnUnlockAnimationComplete;
        }

        [SerializeField] Animator m_Animator;
        [SerializeField] List<AnimatorCallbackInfo> m_AnimatorCallbacksInfo;

        public void PlayState(string stateName)
        {
            m_Animator.Play(stateName);
            StartCoroutine(ExecuteCallback());
        }

        private IEnumerator ExecuteCallback()
        {
            yield return null;


            yield return new WaitForEndOfFrame(); 
            AnimatorStateInfo currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            foreach (var animatorCallbackInfo in m_AnimatorCallbacksInfo)
            {
                if (currentAnimatorStateInfo.IsName(animatorCallbackInfo.AnimationStateName))
                {

                    string currentStateName = animatorCallbackInfo.AnimationStateName;
                    animatorCallbackInfo.OnUnlockAnimationStart?.Invoke();

                    while (currentAnimatorStateInfo.IsName(currentStateName) 
                        && currentAnimatorStateInfo.normalizedTime < 1.0f)
                    {
                        yield return null;
                        currentAnimatorStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
                    }

                    animatorCallbackInfo.OnUnlockAnimationComplete?.Invoke();

                    break;
                }
            }
        } 
    }
}
