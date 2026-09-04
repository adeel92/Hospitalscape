using UnityEngine;
using UnityEngine.Events;
using Isometric.TaskSystem;
using Isometric.PathSystem;

namespace Isometric.Environment
{
    public class StationRemoveTrayHolding : MonoBehaviour
    {
        [Header("---Setup---")]
        [SerializeField] TaskTrigger m_TaskTrigger;
        [SerializeField] PathDirection m_EngageDirection;
        [SerializeField] float m_RemoveDuration = 0.5f;
        [SerializeField] float m_HoldingItemHideDuration = 0.85f;
        [SerializeField] UnityEvent OnEngageSuccessful;
        [SerializeField] UnityEvent OnTaskSuccesful;

        private void OnEnable()
        {
            m_TaskTrigger.OnTaskStart += OnTaskStart;
        }

        private void OnDisable()
        {
            m_TaskTrigger.OnTaskStart -= OnTaskStart;
        }

        private void OnTaskStart(TaskTarget taskTarget)
        {
            if (taskTarget.TryGetComponent(out IEnvironmentInteractable interactable))
            {
                if (interactable.IsCarryingAnyThrowableDataConsumable())
                {
                    interactable.EngageInteractable(m_EngageDirection);
                    OnEngageSuccessful?.Invoke();
                    interactable.HideAllVisibleDataConsumables(m_HoldingItemHideDuration);
                    CoroutineManager.LateAction(() =>
                    {
                        interactable.RemoveAllThrowableDataConsumables();
                        OnTaskSuccesful?.Invoke();
                        m_TaskTrigger.SendTaskResult(TaskResult.Success);
                    }, m_RemoveDuration);
                }
                else
                {
                    m_TaskTrigger.SendTaskResult(TaskResult.Failed);
                }
            }
            else
            {
                m_TaskTrigger.SendTaskResult(TaskResult.Failed);
            }
        }
    }

}
