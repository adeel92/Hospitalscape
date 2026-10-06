using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Isometric.Environment
{
    public class GameplayItemUnlockAnimationHandler : MonoBehaviour
    {
        [SerializeField] Animator m_Animator;
        [SerializeField] List<AnimatorStateInfo> m_AnimatorStatesInfo;
        [Space, SerializeField] UnityEvent m_OnUnlockAnimationComplete;


        public void PlayUnlockAnimation()
        {
            string stateName = GetStateName(GameplayItemUnlockAnimatorState.Unlock);
            m_Animator.Play(stateName);
            StartCoroutine(WaitForUnlockAnimationComplete(stateName));
        }

        private IEnumerator WaitForUnlockAnimationComplete(string stateName)
        {
            int stateHash = Animator.StringToHash(stateName);
            yield return new WaitUntil(() => m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash);
            yield return new WaitUntil(() => m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
            m_OnUnlockAnimationComplete?.Invoke();
        }

        private string GetStateName(GameplayItemUnlockAnimatorState state)
        {
            AnimatorStateInfo stateInfo = m_AnimatorStatesInfo.Find(info => info.AnimatorState == state);
            return stateInfo?.StateName;
        }

        public enum GameplayItemUnlockAnimatorState
        {
            Unlock
        }

        [Serializable]
        private class AnimatorStateInfo
        {
            public GameplayItemUnlockAnimatorState AnimatorState;
            public string StateName;
        }
    }
}