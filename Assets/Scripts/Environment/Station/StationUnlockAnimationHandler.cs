using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Isometric.Environment
{
    public class StationUnlockAnimationHandler : MonoBehaviour
{
    [SerializeField] Animator m_Animator;
    [SerializeField] UnlockAnimatorState m_UnlockState;

    [Space] public UnityEvent OnUnlockAnimationComplete;


    public void PlayUnlockAnimation()
    {
        string state = m_UnlockState.ToString();
        m_Animator.Play(state, 0, 0f);
        StartCoroutine(StartUnlockAnimation(state));
    }

    private IEnumerator StartUnlockAnimation(string state)
    {
        yield return null;
        while (true)
        {
            AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(state) && stateInfo.normalizedTime >= 1f)
                break;
            yield return null;
        }

        OnUnlockAnimationComplete?.Invoke();
    }


    public enum UnlockAnimatorState
    {
        Unlock
    }
}   
}