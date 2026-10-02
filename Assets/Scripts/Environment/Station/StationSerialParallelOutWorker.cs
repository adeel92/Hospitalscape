using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Isometric.PathSystem;
using NaughtyAttributes;

namespace Isometric.Environment
{
    public class StationSerialParallelOutWorker : MonoBehaviour
    {
        [Serializable]
        private class StationInfo
        {
            public int IDIndex;
            public PathNode OutputTrayNode;
            public PathNode MachineNode;
            public UnityEvent OnMachineToStart;
            public UnityEvent OnMachinePickUp;
            public UnityEvent OnOrderPutDown;
        }

        [SerializeField] PathNode m_DefaultNode;
        [SerializeField] PathNode m_CurrentNode;
        [SerializeField, ReadOnly] float m_WalkSpeed = 1;
        [SerializeField] List<float> m_WalkSpeeds;
        [SerializeField] StationSerialParallelOutWorkerAnimatorController m_AnimatorController;
        [SerializeField] StationSerialParallelOut m_Station;
        [SerializeField] List<StationInfo> m_StationsInfo;

        private bool m_IsRunning = false;
        private bool m_IsGoingToDefaultNode = false;
        private StationInfo m_CurrentStationInfo = null;
        private Queue<int> m_TaskQueue = new Queue<int>();


        public void Setup(int durationIndex)
        {
            if (durationIndex >= 0 && durationIndex < m_WalkSpeeds.Count)
            {
                m_WalkSpeed = m_WalkSpeeds[durationIndex];
            }
            else
            {
                m_WalkSpeed = m_WalkSpeeds[0];
            }

            m_AnimatorController.PlayIdle();
        }

        private void GoToDefaultNode()
        {
            if(m_CurrentNode == m_DefaultNode)
            {
                m_AnimatorController.PlayIdle();
                return;
            }
            m_IsRunning = true;
            m_IsGoingToDefaultNode = true;
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, m_DefaultNode, m_WalkSpeed, OnGoingToNode, OnReachDefaultNode);
        }
        private void OnReachDefaultNode(PathNode node)
        {
            m_CurrentNode = node;
            m_IsRunning = false;
            m_IsGoingToDefaultNode = false;
            m_AnimatorController.PlayIdle();
        }

        public void ToMachine(int idIndex)
        {
            if (m_IsRunning /* && !m_IsGoingToDefaultNode */)
            {
                m_TaskQueue.Enqueue(idIndex);
            }
            else
            {
                /* if (m_IsGoingToDefaultNode)
                {
                    PathTraverserExtension.StopTarget(transform);
                } */
                ToMachineProcess(idIndex);
            }
        }

        private void ToMachineProcess(int idIndex)
        {
            m_IsRunning = true;
            StationInfo stationInfo = m_StationsInfo.Find((x) => x.IDIndex == idIndex);
            if (stationInfo != null)
            {
                m_CurrentStationInfo = stationInfo;
                PathTraverserExtension.MoveTarget(transform, m_CurrentNode, stationInfo.MachineNode, m_WalkSpeed, OnGoingToNode, OnToMachineToStart);
            }
            else
            {
                Debug.LogWarning(typeof(StationInfo) + " is null");
                m_IsRunning = false;
            }
        }

        public void FromMachineToOrder(int idIndex)
        {
            FromMachineToOrderProcess(idIndex);
        }

        private void FromMachineToOrderProcess(int idIndex)
        {
            m_IsRunning = true;
            StationInfo stationInfo = m_StationsInfo.Find((x) => x.IDIndex == idIndex);
            if (stationInfo != null)
            {
                m_CurrentStationInfo = stationInfo;
                PathTraverserExtension.MoveTarget(transform, m_CurrentNode, stationInfo.MachineNode, m_WalkSpeed, OnGoingToNode, OnMachineToPickUp);
            }
            else
            {
                Debug.LogWarning(typeof(StationInfo) + " is null");
                m_IsRunning = false;
            }
        }

        private void CheckQueue(/* out bool hasAnyTaskQueued */)
        {
            // hasAnyTaskQueued = false;
            if (m_TaskQueue.Count > 0)
            {
                // hasAnyTaskQueued = true;
                int queued = m_TaskQueue.Dequeue();
                ToMachineProcess(queued);
            }
        }

        private void OnToMachineToStart(PathNode node)
        {
            m_CurrentNode = node;
            m_Station.OrderToMachine(m_CurrentStationInfo.IDIndex);
            StationInfo stationInfo = m_StationsInfo.Find((x) => x.IDIndex == m_CurrentStationInfo.IDIndex);
            if(stationInfo != null)
            {
                stationInfo.OnMachineToStart?.Invoke();
            }
            m_CurrentStationInfo = null;
            m_AnimatorController.PlayWorkingUp();
        }


        private void OnMachineToPickUp(PathNode node)
        {
            m_CurrentNode = node;
            m_Station.PickFromMachine(m_CurrentStationInfo.IDIndex);
            StationInfo stationInfo = m_StationsInfo.Find((x) => x.IDIndex == m_CurrentStationInfo.IDIndex);
            if (stationInfo != null)
            {
                stationInfo.OnMachinePickUp?.Invoke();
            }
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, m_CurrentStationInfo.OutputTrayNode, m_WalkSpeed, OnGoingToNode, OnToOrderToPutDown);
        }

        private void OnToOrderToPutDown(PathNode node)
        {
            m_CurrentNode = node;
            m_Station.OrderToOutputTray(m_CurrentStationInfo.IDIndex);
            m_IsRunning = false;
            StationInfo stationInfo = m_StationsInfo.Find((x) => x.IDIndex == m_CurrentStationInfo.IDIndex);
            if (stationInfo != null)
            {
                stationInfo.OnOrderPutDown?.Invoke();
            }
            m_CurrentStationInfo = null;
            CheckQueue(/* out bool hasAnyTaskQueued */);
            /* if (!hasAnyTaskQueued)
            {
                GoToDefaultNode();
            } */
            m_AnimatorController.PlayIdle();
        }

        private void OnGoingToNode(PathNode node1, PathNode node2)
        {
            if (node1.TryGetComponent(out PathNodeDirection nodeDirection))
            {
                PathDirection direction = nodeDirection.GetDirection(node2);
                m_AnimatorController.PlayWalkAnimation(direction, m_WalkSpeed);
            }

        }
    }
}
