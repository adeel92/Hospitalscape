using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Isometric.Environment;

namespace Isometric.TaskSystem
{
    /// <summary>
    /// This class is use for to arange empty tasks 
    /// </summary>
    [RequireComponent(typeof(TaskTrigger))]
    public class TaskEmptyTrigger : MonoBehaviour
    {
        [SerializeField, ReadOnly] TaskTrigger m_TaskTrigger;

        private void Awake()
        {
            m_TaskTrigger = GetComponent<TaskTrigger>();
        }

        private void OnEnable()
        {
            m_TaskTrigger.OnTaskStart += OnTaskStart;
        }

        private void OnDisable()
        {
            m_TaskTrigger.OnTaskStart -= OnTaskStart;
        }

        public void OnClicked()
        {
            m_TaskTrigger.OnClickedWithOutGlobalEvent();
        }

        private void OnTaskStart(TaskTarget taskTarget)
        {
            m_TaskTrigger.SendTaskResult(TaskResult.Success);
        }
    }
}
