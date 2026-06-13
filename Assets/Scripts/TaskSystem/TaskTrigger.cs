using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Isometric.PathSystem;
using System;
using TMPro;

namespace Isometric.TaskSystem
{
    /// <summary>
    /// This will be working with the <see cref="TaskTarget"/> component
    /// This will trigger the tasks for the TaskTarget which will be
    /// carried out one by one in queue
    /// </summary>
    public class TaskTrigger : MonoBehaviour
    {
        /// <summary>
        /// On Task Starts Start
        /// </summary>
        public event Action<TaskTarget> OnTaskStart;

        private bool m_IsRunningTask = false;
        public Action<TaskResult> OnCompleteTask;

        [Serializable]
        private class TaskNumberData
        {
            public int TaskNumber;
            public TaskTargetType TaskTargetType;
            public GameObject TaskPrefab;
        }

        [Serializable]
        private class TaskNumberPrefabData
        {
            public TaskTargetType TaskTargetType;
            public Transform ContentHolder; 
            public Transform Prefab;
        }

        [Header("---Task Number---")]
        [SerializeField] List<TaskNumberPrefabData> m_TaskNumberPrefabs;
        List<TaskNumberData> m_TasksNumberData = new List<TaskNumberData>();

        [Header("---Node---")]
        [SerializeField] PathNode m_Node;
        public PathNode Node => m_Node;

        private void OnEnable(){}

        public void OnClicked()
        {
            if (!enabled) return;
            GlobalEventHolder.OnTaskAssigned?.Invoke();
            TaskTarget.InsertTask(this);
        }

        public void Interact(TaskTarget taskTarget, Action<TaskResult> onComplete)
        {
            if (!m_IsRunningTask)
            {
                if (OnTaskStart != null)
                {
                    m_IsRunningTask = true;
                    OnCompleteTask = onComplete;

                    OnTaskStart.Invoke(taskTarget);
                }
                else
                {
                    onComplete?.Invoke(TaskResult.Failed);
                }
            }
            else
            {
                onComplete?.Invoke(TaskResult.Failed);
            }
        }

        public void SendTaskResult(TaskResult taskResult)
        {
            if (m_IsRunningTask)
            {
                m_IsRunningTask = false;
                OnCompleteTask?.Invoke(taskResult);
                OnCompleteTask = null;
            }
        }

        public void ClearAllTaskNumber(TaskTargetType taskTargetType)
        {
            foreach (var taskNumberData in m_TasksNumberData)
            {
                if (taskNumberData.TaskTargetType == taskTargetType)
                {
                    Destroy(taskNumberData.TaskPrefab);
                }
            }

            m_TasksNumberData.RemoveAll((x ) => x.TaskTargetType == taskTargetType);
        }

        public void AddTaskNumber(TaskTargetType taskTargetType, int taskNumber)
        {
            TaskNumberPrefabData prefabData = m_TaskNumberPrefabs.Find((x) => x.TaskTargetType == taskTargetType);
            if (prefabData != null)
            {
                Transform taskNumberPrefab = Instantiate(prefabData.Prefab, prefabData.ContentHolder);
                taskNumberPrefab.SetAsFirstSibling();
                TextMeshProUGUI text = taskNumberPrefab.GetComponentInChildren<TextMeshProUGUI>();
                text.text = taskNumber.ToString();

                TaskNumberData taskNumberData = new TaskNumberData();
                taskNumberData.TaskTargetType = taskTargetType;
                taskNumberData.TaskNumber = taskNumber;
                taskNumberData.TaskPrefab = taskNumberPrefab.gameObject;

                m_TasksNumberData.Add(taskNumberData);
            }
            else
            {
                Debug.LogWarning("TaskNumber prefab not found!");
            }

        }
    }

    public enum TaskResult
    {
        Failed, Success
    }
}