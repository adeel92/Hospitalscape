using UnityEngine;
using UnityEngine.Rendering;
using NaughtyAttributes;
using Isometric.PathSystem;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

namespace Isometric.Environment
{
    public class EnvironmentVehicleController : MonoBehaviour
    {
        [Header("---Setup---")]
        [SerializeField] bool m_AlwaysUnlocked;
        [SerializeField, ReadOnly, HideIf(nameof(m_AlwaysUnlocked))] bool m_IsUnlocked;
        [SerializeField] bool m_StartActionOnSetup = true;
        [Space, SerializeField] PathNode m_StartNode;
        [SerializeField] EnvironmentVehicleAnimatorController m_AnimatorController;
        [SerializeField] SortingGroup m_SortingGroup;
        [SerializeField] float m_DriveSpeed;

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
            GlobalEventHolder.OnMenuEnvironmentHidden += ResetBehaviourOnMenuHidden;
            GlobalEventHolder.OnMenuEnvironmentShown += RestartBehaviourOnMenuShown;

            SetupController();
        }

        private void OnDestroy()
        {
            GlobalEventHolder.OnMenuEnvironmentHidden -= ResetBehaviourOnMenuHidden;
            GlobalEventHolder.OnMenuEnvironmentShown -= RestartBehaviourOnMenuShown;
        }

        private void SetupController()
        {
            m_DefaultSortingLayer = m_SortingGroup.sortingLayerName;
            m_DefaultSortingOrder = m_SortingGroup.sortingOrder;
            m_CurrentNode = m_StartNode;
            m_CurrentActionIndex = 0;
            m_CurrentActionInfo = m_ActionInfos[m_CurrentActionIndex];
            transform.position = m_CurrentNode.transform.position;
            
            if (m_AlwaysUnlocked || m_IsUnlocked)
            {
                gameObject.SetActive(true);
                if (m_StartActionOnSetup)
                {
                    StartNodeAction();
                }
            }
            else
            {
                m_IsUnlocked = false;
                gameObject.SetActive(false);
            }
        }

        private void StartNodeAction()
        {
            if(m_CurrentNode == m_CurrentActionInfo.ActionNode)
            {
                switch (m_CurrentActionInfo.ActionType)
                {
                    case ActionType.StopAndWaitUp:
                    m_AnimatorController.PlayState(VehicleAnimatorState.ParkUp);
                    break;

                    case ActionType.StopAndWaitDown:
                    m_AnimatorController.PlayState(VehicleAnimatorState.ParkDown);
                    break;

                    case ActionType.StopAndWaitLeft:
                    m_AnimatorController.PlayState(VehicleAnimatorState.ParkLeft);
                    break;

                    case ActionType.StopAndWaitRight:
                    m_AnimatorController.PlayState(VehicleAnimatorState.ParkRight);
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
                    if (isActiveAndEnabled)
                    {
                        m_EndActionCoroutine = StartCoroutine(EndNodeAction());
                    }
                }
            }
            else
            {
                StartTraverse(m_CurrentActionInfo.ActionNode);
            }
        }

        private void StartTraverse(PathNode endNode)
        {
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, endNode, m_DriveSpeed, OnGoingToNodeEntering, OnReachedTraverseEndNode);
        }

        private void OnGoingToNodeEntering(PathNode node1, PathNode node2)
        {
            if (node1.TryGetComponent(out PathNodeDirection nodeDirection))
            {
                PathDirection direction = nodeDirection.GetDirection(node2);
                switch (direction)
                {
                    case PathDirection.Up:
                        m_AnimatorController.PlayState(VehicleAnimatorState.DriveUp);
                        break;
                    case PathDirection.Down:
                        m_AnimatorController.PlayState(VehicleAnimatorState.DriveDown);
                        break;
                    case PathDirection.Left:
                        m_AnimatorController.PlayState(VehicleAnimatorState.DriveLeft);
                        break;
                    case PathDirection.Right:
                        m_AnimatorController.PlayState(VehicleAnimatorState.DriveRight);
                        break;
                }
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
                    Debug.LogWarning($"{nameof(EnvironmentVehicleController)} -- Same action info is retrieved after ending the node action so, not starting the new action!");
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
                    Debug.LogWarning($"{nameof(EnvironmentVehicleController)} -- Same action info is retrieved after ending the node action so, not starting the new action!");
                }
            }
        }

        private ActionInfo GetRandomActionInfo()
        {
            ActionInfo actionInfo = m_ActionInfos[UnityEngine.Random.Range(0, m_ActionInfos.Count)];
            return actionInfo;
        }

        // Can be utilized if it is locked initially and unlocked at runtime
        // Execute with doStartNodeAction = true only if Setup has been called before!
        public void SetStatusToUnlock(bool doStartNodeAction)
        {
            m_IsUnlocked = true;
            if (doStartNodeAction)
            {
                gameObject.SetActive(true);
                StartNodeAction();
            }
        }

        private void ResetBehaviourOnMenuHidden()
        {
            if (m_AlwaysUnlocked || m_IsUnlocked)
            {
                PathTraverserExtension.StopTargetImmediately(transform);
                if(m_EndActionCoroutine != null)
                {
                    StopCoroutine(m_EndActionCoroutine);
                    m_EndActionCoroutine = null;
                }
            }
        }
        private void RestartBehaviourOnMenuShown()
        {
            if (m_AlwaysUnlocked || m_IsUnlocked)
            {
                SetupController();
            }
        }


        public enum ActionType
        {
            None,
            StopAndWaitUp, StopAndWaitDown, StopAndWaitLeft, StopAndWaitRight,
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
    }
}
