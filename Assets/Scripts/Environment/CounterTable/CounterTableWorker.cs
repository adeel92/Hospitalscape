using UnityEngine;
using UnityEngine.Events;

namespace Isometric.Environment
{
    public class CounterTableWorker : MonoBehaviour
    {
        [SerializeField] UnityEvent m_OnIsLockedGameplay;
        [SerializeField] UnityEvent m_OnIsUnlockedGameplay;
        [SerializeField] UnityEvent m_OnHasJustUnlockedGameplay;

        [Space, SerializeField] CounterTableWorkerAnimatorController m_AnimatorController;
        [SerializeField] CounterTableController m_CounterController;


        public void OnLocked()
        {
            gameObject.SetActive(false);
            m_OnIsLockedGameplay?.Invoke();
        }
        public void OnUnlocked()
        {
            gameObject.SetActive(true);
            m_OnIsUnlockedGameplay?.Invoke();
        }
        public void OnHasJustUnlocked()
        {
            gameObject.SetActive(true);
            m_OnHasJustUnlockedGameplay?.Invoke();
        }

        public void Setup()
        {
        }

        public void ResetToDefaultState()
        {
            m_AnimatorController.PlayIdleLeft();
        }
    }
}
