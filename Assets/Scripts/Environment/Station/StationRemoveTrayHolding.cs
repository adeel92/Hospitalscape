using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Isometric.TaskSystem;

namespace Isometric.Environment
{
    public class StationRemoveTrayHolding : MonoBehaviour
    {
        [Header("---Setup---")]
        [SerializeField] TaskTrigger m_TaskTrigger;
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
                interactable.RemoveAllDataConsumables();
                OnTaskSuccesful?.Invoke();
                m_TaskTrigger.SendTaskResult(TaskResult.Success);
            }
            else
            {
                m_TaskTrigger.SendTaskResult(TaskResult.Failed);
            }
        }
    }

}
