using UnityEngine;
using UnityEngine.Rendering;
using NaughtyAttributes;
using Isometric.PathSystem;
using Isometric.Worker;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

namespace Isometric.Environment.Worker
{
    public class EnvironmentWorkerController : MonoBehaviour
    {
        [Header("---Setup---")]
        [SerializeField] bool m_AlwaysUnlocked;
        [SerializeField, ReadOnly, HideIf(nameof(m_AlwaysUnlocked))] bool m_IsUnlocked;
        [Space, SerializeField] PathNode m_StartNode;
        [SerializeField] EnvironmentWorkerAnimatorController m_AnimatorController;
        [SerializeField] SortingGroup m_SortingGroup;
        [SerializeField] float m_WalkSpeed;

        [Header("---Action Details---")]
        [SerializeField] List<ActionInfo> m_ActionInfos = new();
        [SerializeField] bool m_RandomizeActions = false;
        [HideIf(nameof(m_RandomizeActions))]
        [SerializeField, ReadOnly] int m_CurrentActionIndex = 0;
        [SerializeField, ReadOnly] ActionInfo m_CurrentActionInfo = null;

        private string m_DefaultSortingLayer;
        private int m_DefaultSortingOrder;
        private PathNode m_CurrentNode = null;
        private Coroutine m_EndActionCoroutine = null;


        public void Setup()
        {
            m_DefaultSortingLayer = m_SortingGroup.sortingLayerName;
            m_DefaultSortingOrder = m_SortingGroup.sortingOrder;
            m_CurrentNode = m_StartNode;
            m_CurrentActionIndex = 0;
            m_CurrentActionInfo = m_ActionInfos[m_CurrentActionIndex];
            StartNodeAction();
        }

        private void StartNodeAction()
        {
            if(m_CurrentNode == m_CurrentActionInfo.ActionNode)
            {
                switch (m_CurrentActionInfo.ActionType)
                {
                    case ActionType.Idle:
                    m_AnimatorController.PlayIdleAnimation(m_CurrentActionInfo.ActionDirection);
                    break;

                    case ActionType.IdleTalk:
                    m_AnimatorController.PlayTalkAnimation(m_CurrentActionInfo.ActionDirection);
                    break;

                    case ActionType.IdleDrink:
                    m_AnimatorController.PlayDrinkAnimation(m_CurrentActionInfo.ActionDirection);
                    break;

                    case ActionType.IdleWork:
                    m_AnimatorController.PlayWorkAnimation(m_CurrentActionInfo.ActionDirection);
                    break;
                }
                
                m_CurrentActionInfo.ActionStartEvent?.Invoke();

                if (!m_CurrentActionInfo.doLoop)
                {
                    if(m_EndActionCoroutine != null)
                    {
                        StopCoroutine(m_EndActionCoroutine);
                        m_EndActionCoroutine = null;
                    }
                    m_EndActionCoroutine = StartCoroutine(EndNodeAction());
                }
            }
            else
            {
                StartTraverse(m_CurrentActionInfo.ActionNode);
            }
        }

        private void StartTraverse(PathNode endNode)
        {
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, endNode, m_WalkSpeed, OnGoingToNodeEntering, OnReachedTraverseEndNode);
        }

        private void OnGoingToNodeEntering(PathNode node1, PathNode node2)
        {
            if (node1.TryGetComponent(out PathNodeDirection nodeDirection))
            {
                PathDirection direction = nodeDirection.GetDirection(node2);
                m_AnimatorController.PlayWalkAnimation(direction, m_WalkSpeed);
            }

            if (node2.TryGetComponent(out SortingGroup sortingGroup))
            {
                m_SortingGroup.sortingLayerName = sortingGroup.sortingLayerName;
                m_SortingGroup.sortingOrder = sortingGroup.sortingOrder;
            }
            else
            {
                m_SortingGroup.sortingLayerName = m_DefaultSortingLayer;
                m_SortingGroup.sortingOrder = m_DefaultSortingOrder;
            }
        }
        private void OnReachedTraverseEndNode(PathNode endNode)
        {
            m_CurrentNode = endNode;
            StartNodeAction();
        }

        private IEnumerator EndNodeAction()
        {
            yield return new WaitForSeconds(m_CurrentActionInfo.ActionDuration);
            m_CurrentActionInfo.ActionEndEvent?.Invoke();
            
            if (m_RandomizeActions)
            {
                if(m_ActionInfos.Count > 1)
                {
                    while (true)
                    {
                        yield return null;
                        ActionInfo newActionInfo = GetRandomActionInfo();
                        if(m_CurrentActionInfo != newActionInfo)
                        {
                            m_CurrentActionInfo = newActionInfo;
                            break;
                        }
                    }
                    StartNodeAction();
                }
                else
                {
                    Debug.LogWarning($"{nameof(EnvironmentWorkerController)} -- Same action info is retrieved after ending the node action so, not starting the new action!");
                }
            }
            else
            {
                int newIndex = m_CurrentActionIndex + 1;
                if(newIndex >= m_ActionInfos.Count)
                {
                    newIndex = 0;
                }
                ActionInfo newActionInfo = m_ActionInfos[newIndex];
                if(m_CurrentActionInfo != newActionInfo)
                {
                    m_CurrentActionIndex = newIndex;
                    m_CurrentActionInfo = newActionInfo;
                    StartNodeAction();
                }
                else
                {
                    Debug.LogWarning($"{nameof(EnvironmentWorkerController)} -- Same action info is retrieved after ending the node action so, not starting the new action!");
                }
            }
        }

        private ActionInfo GetRandomActionInfo()
        {
            ActionInfo actionInfo = m_ActionInfos[UnityEngine.Random.Range(0, m_ActionInfos.Count)];
            return actionInfo;
        }
        
        /* private void CheckForNextTraverseEndNode()
        {
            PathNode pathNode = GetRandomTraverseEndNode();
            if(pathNode == null)
            {
                Debug.LogWarning($"{nameof(EnvironmentWorkerController)} -- Retrieved random traverse end node does not exist so, stopping the traverse!");
                return;
            }
            if(m_CurrentNode == pathNode)
            {
                CheckForNextTraverseEndNode();
                return;
            }
            StartTraverse(pathNode);
        } */

        /* private void PerformNodeAction(PathNode pathNode)
        {
            if(pathNode.TryGetComponent(out EnvironmentWorkerPathActionNode actionNode))
            {
                ActionInfo currentActionInfo = m_ActionInfos.Find(x => x.ActionType == actionNode.ActionType);
                if(currentActionInfo == null)
                {
                    Debug.LogWarning($"{nameof(EnvironmentWorkerController)} -- No action info is present with ActionType {actionNode.ActionType} so, performing nothing!");
                    return;
                }

                switch (currentActionInfo.ActionType)
                {
                    case ActionType.Idle:
                    m_AnimatorController.PlayIdleAnimation(actionNode.PathDirection);
                    break;
                }

                if(!currentActionInfo.doLoop && m_WorkerType == WorkerType.Walk)
                {
                    
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(EnvironmentWorkerController)} -- No action node is present so, performing nothing!");
            }
        } */

        /* private PathNode GetRandomTraverseEndNode()
        {
            PathNode pathNode = null;
            if(m_TraverseEndNodes.Count == 0)
            {
                return pathNode;
            }
            pathNode = m_TraverseEndNodes[UnityEngine.Random.Range(0, m_TraverseEndNodes.Count)];
            return pathNode;
        } */


        public enum ActionType
        {
            Idle,
            IdleTalk,
            IdleDrink,
            IdleWork,
        }

        [Serializable]
        public class ActionInfo
        {
            public ActionType ActionType;
            public PathNode ActionNode;
            public PathDirection ActionDirection;
            public UnityEvent ActionStartEvent;
            public UnityEvent ActionEndEvent;
            public bool doLoop = false;
            [AllowNesting, HideIf(nameof(doLoop))]
            public float ActionDuration = 5f;
        }

        /* [Serializable]
        public class ActionInfo
        {
            public ActionType ActionType;
            public bool doLoop = false;
            [HideIf(nameof(doLoop))]
            public float ActionDuration = 5f;
        } */
    }
}
