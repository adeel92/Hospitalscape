using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Isometric.PathSystem;
using NaughtyAttributes;
using System;

namespace Isometric.TaskSystem
{
    /// <summary>
    /// This system is created to carry out queue tasks by the player or multiple players
    /// This will be component on the object by which the tasks are being carried out
    /// The will be trigger by the <see cref="TaskTrigger"/> componenet
    /// </summary>
    public class TaskTarget : MonoBehaviour
    {
        private static TaskTarget s_CurrentTaskTarget = null;

        /// <summary>
        /// Starting first task
        /// </summary>
        public event Action OnStartingFirstTask;

        /// <summary>
        /// On going from node1 to node2 during path traversing 
        /// </summary>
        public event Action<PathNode, PathNode> OnGoingToNode;

        /// <summary>
        /// On going from node1 to node2 distance
        /// </summary>
        public event Action<float> OnGoingToNodeDistance;

        /// <summary>
        /// On the target reaches the target node
        /// and sends the bool if the next task is going to execuated 
        /// </summary>
        public event Action<PathNode, bool> OnReachedTargetNode;

        /// <summary>
        /// On task complete and sends the task result and 
        /// returns the delay to execuate the next task
        /// </summary>
        public event Func<TaskResult, float> OnTaskComplete;

        /// <summary>
        /// On no more taks left
        /// </summary>
        public event Action OnNoMoreTasks;


        [Header("---TaskType---")]
        [SerializeField] TaskTargetType m_Type;

        [Header("---Path---")]
        [SerializeField] PathNode m_CurrentNode;
        [SerializeField] float m_PathTraverseSpeed = 5;

        [Header("---Tasks---")]
        [SerializeField, ReadOnly] List<Task> m_Tasks = new List<Task>();
        private bool m_IsRunningTask = false;
        private bool m_HasStartedFirstTask;

        private void Awake()
        {
            s_CurrentTaskTarget = this;
        }

        private void OnEnable()
        {
            GlobalEventHolder.OnCurrentTaskTargetCancle += OnDeleteCurrentAllTasks;
            GlobalEventHolder.OnCurrentTaskTargetCancleImmediately += OnDeleteCurrentAllTasksImmediately;
        }

        private void OnDisable()
        {
            GlobalEventHolder.OnCurrentTaskTargetCancle -= OnDeleteCurrentAllTasks;
            GlobalEventHolder.OnCurrentTaskTargetCancleImmediately -= OnDeleteCurrentAllTasksImmediately;
        }

        public void SelectCurrentTaskTarget()
        {
            s_CurrentTaskTarget = this;
        }

        public void PlayTask()
        {
            if (m_Tasks.Count > 0)
            {
                if (m_HasStartedFirstTask == false)
                {
                    OnStartingFirstTask?.Invoke();
                    m_HasStartedFirstTask = true;
                }

                if (m_IsRunningTask == false)
                {
                    m_IsRunningTask = true;

                    Task task = m_Tasks[0];
                    TaskTrigger taskTrigger = task.TaskTrigger;
                    PathNode bNode = taskTrigger.Node;

                    PathTraverserExtension.MoveTarget(transform, m_CurrentNode, bNode, m_PathTraverseSpeed,
                    // On node reached    
                    (node1, node2) =>
                    {
                        m_CurrentNode = node2;
                        OnGoingToNode?.Invoke(node1, node2);
                    }, 
                    (distance) =>
                    {
                        OnGoingToNodeDistance?.Invoke(distance);
                    },
                    // On complete
                    (lastReachedNode) => 
                    {
                        OnReachedTargetNode?.Invoke(lastReachedNode, m_IsRunningTask);

                        if (m_IsRunningTask)
                        {
                            taskTrigger.Interact(this, (taskResut) =>
                            {
                                CompleteTask();
                                if(OnTaskComplete != null)
                                {
                                    float delay = OnTaskComplete.Invoke(taskResut);
                                    CoroutineManager.LateAction(PlayTask, delay);
                                }
                                else
                                {
                                    PlayTask();
                                }
                            });
                        }
                    });
                }
            }
            else if (m_Tasks.Count == 0)
            {
                m_IsRunningTask = false;
                m_HasStartedFirstTask = false;
                OnNoMoreTasks?.Invoke();
            }
        }

        public static void InsertTask(TaskTrigger taskTrigger)
        {
            if (s_CurrentTaskTarget != null)
            {
                TaskTarget taskTarget = s_CurrentTaskTarget;
                Task task = new Task();
                task.TaskTrigger = taskTrigger;
                task.TaskOrder = 0;
                taskTarget.m_Tasks.Add(task);
                s_CurrentTaskTarget.ClearTasksNumber();
                s_CurrentTaskTarget.UpdateTasksNumber();

                s_CurrentTaskTarget.PlayTask();
            }
            else
            {
                Debug.LogWarning("No task target selected");
            }
        }

        public void CompleteTask()
        {
            List<Task> tasks = m_Tasks;
            if (tasks.Count > 0)
            {
                Task task = tasks[0];
                ClearTasksNumber();
                tasks.RemoveAt(0);
                UpdateTasksNumber();
                m_IsRunningTask = false;
            }
            else
            {
                m_IsRunningTask = false;
            }
        }

        public void ClearTasksNumber()
        {
            foreach (var task in m_Tasks)
            {
                task.TaskTrigger.ClearAllTaskNumber(m_Type);
            }
        }

        public void UpdateTasksNumber()
        {
            int i = 1;
            foreach (var task in m_Tasks)
            {
                task.TaskOrder = i;
                task.TaskTrigger.AddTaskNumber(m_Type, task.TaskOrder);
                i++;
            }
        }

        public void DeleteAllTasks()
        {
            m_IsRunningTask = false;
            m_HasStartedFirstTask = false;
            ClearTasksNumber();
            m_Tasks.Clear();
            UpdateTasksNumber();
            PathTraverserExtension.StopTarget(transform);
        }

        public void DeleteAllTasksImmediately()
        {
            m_IsRunningTask = false;
            m_HasStartedFirstTask = false;
            ClearTasksNumber();
            m_Tasks.Clear();
            UpdateTasksNumber();
            PathTraverserExtension.StopTargetImmediately(transform);
        }

        private void OnDeleteCurrentAllTasks()
        {
            if (s_CurrentTaskTarget != null)
            {
                s_CurrentTaskTarget.DeleteAllTasks();
            }
        }

        private void OnDeleteCurrentAllTasksImmediately()
        {
            if (s_CurrentTaskTarget != null)
            {
                s_CurrentTaskTarget.DeleteAllTasksImmediately();
            }
        }

        public void SetPathTraverseSpeed(float speed)
        {
            m_PathTraverseSpeed = speed;
        }

    }

    [Serializable]
    public class Task
    {
        public int TaskOrder;
        public TaskTrigger TaskTrigger;
    }

    public enum TaskTargetType
    {
        Player1, Player2
    }
}
