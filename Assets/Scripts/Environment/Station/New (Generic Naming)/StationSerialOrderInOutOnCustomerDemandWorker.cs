using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Isometric.PathSystem;
using NaughtyAttributes;
using System;

namespace Isometric.Environment
{
    public class StationSerialOrderInOutOnCustomerDemandWorker : MonoBehaviour
    {
        /* [Serializable]
        private class StationInfo
        {
            public int IDIndex;
            public PathNode OutputTrayNode;
            public PathNode MachineNode;
            public UnityEvent OnMachineToStart;
            public UnityEvent OnMachinePickUp;
            public UnityEvent OnOrderPutDown;
        } */

        [SerializeField] PathNode m_DefaultNode;
        [SerializeField] PathNode m_CurrentNode;
        [SerializeField] PathNode OutputTrayNode;
        [SerializeField] PathNode QueuedTrayNode;
        [SerializeField] PathNode MachineNode;
        [Space, SerializeField] UnityEvent OnQueuedTrayPickUp;
        [SerializeField] UnityEvent OnMachineToStart;
        [SerializeField] UnityEvent OnMachinePickUp;
        [SerializeField] UnityEvent OnOrderPutDown;
        [Space, SerializeField, ReadOnly] float m_WalkSpeed = 1;
        [SerializeField] List<float> m_WalkSpeeds;
        [Space, SerializeField] StationSerialOrderInOutOnCustomerDemandWorkerAnimatorController m_AnimatorController;
        [SerializeField] StationSerialOrderInOutOnCustomerDemand m_Station;
        [SerializeField] List<PickedOrderHolderInfo> m_PickedOrderHolderInfos = new();
        // [SerializeField] List<StationInfo> m_StationsInfo;

        private bool m_IsRunning = false;
        private bool m_IsGoingToDefaultNode = false;
        private List<StationSerialOrderInOutOnCustomerDemand.MachineOutputInfo> m_CurrentMachineOutputInfos = null;
        private PickedOrderHolderInfo m_CurrentPickedOrderHolderInfo = null;
        // private StationInfo m_CurrentStationInfo = null;
        // private Queue<int> m_TaskQueue = new Queue<int>();


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

        public void ToQueuedTray()
        {
            ToQueuedTrayProcess();
        }
        private void ToQueuedTrayProcess()
        {
            m_IsRunning = true;
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, QueuedTrayNode, m_WalkSpeed, OnGoingToNode, OnReachedQueuedTrayForOrderPickUp);
        }

        public void ToMachine(/* int idIndex */)
        {
            // if (m_IsRunning /* && !m_IsGoingToDefaultNode */)
            // {
            //     m_TaskQueue.Enqueue(idIndex);
            // }
            // else
            // {
            //     /* if (m_IsGoingToDefaultNode)
            //     {
            //         PathTraverserExtension.StopTarget(transform);
            //     } */
            //     ToMachineProcess(idIndex);
            // }
            ToMachineProcess(/* idIndex */);
        }
        private void ToMachineProcess(/* int idIndex */)
        {
            m_IsRunning = true;
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, MachineNode, m_WalkSpeed, OnGoingToNode, OnReachedMachineForProcessing);
            /* StationInfo stationInfo = m_StationsInfo.Find((x) => x.IDIndex == idIndex);
            if (stationInfo != null)
            {
                m_CurrentStationInfo = stationInfo;
                PathTraverserExtension.MoveTarget(transform, m_CurrentNode, stationInfo.MachineNode, m_WalkSpeed, OnGoingToNode, OnToMachineToStart);
            }
            else
            {
                Debug.LogWarning(typeof(StationInfo) + " is null");
                m_IsRunning = false;
            } */
        }

        public void FromMachineToOrder(List<StationSerialOrderInOutOnCustomerDemand.MachineOutputInfo> machineOutputInfos/* int idIndex */)
        {
            m_CurrentMachineOutputInfos = machineOutputInfos;
            FromMachineToOrderProcess(/* idIndex */);
        }
        private void FromMachineToOrderProcess(/* int idIndex */)
        {
            m_IsRunning = true;
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, MachineNode, m_WalkSpeed, OnGoingToNode, OnReachedMachineForOrderPickUp);
            /* StationInfo stationInfo = m_StationsInfo.Find((x) => x.IDIndex == idIndex);
            if (stationInfo != null)
            {
                m_CurrentStationInfo = stationInfo;
                PathTraverserExtension.MoveTarget(transform, m_CurrentNode, stationInfo.MachineNode, m_WalkSpeed, OnGoingToNode, OnMachineToPickUp);
            }
            else
            {
                Debug.LogWarning(typeof(StationInfo) + " is null");
                m_IsRunning = false;
            } */
        }

        // private void CheckQueue(/* out bool hasAnyTaskQueued */)
        // {
        //     // hasAnyTaskQueued = false;
        //     if (m_TaskQueue.Count > 0)
        //     {
        //         // hasAnyTaskQueued = true;
        //         int queued = m_TaskQueue.Dequeue();
        //         ToMachineProcess(queued);
        //     }
        // }

        private void OnReachedMachineForProcessing(PathNode node)
        {
            m_CurrentNode = node;
            m_Station.OnOrderToMachine();
            OnMachineToStart?.Invoke();
            /* StationInfo stationInfo = m_StationsInfo.Find((x) => x.IDIndex == m_CurrentStationInfo.IDIndex);
            if(stationInfo != null)
            {
                stationInfo.OnMachineToStart?.Invoke();
            }
            m_CurrentStationInfo = null; */
            m_AnimatorController.PlayWorkingUp();
        }

        private void OnReachedQueuedTrayForOrderPickUp(PathNode node)
        {
            m_CurrentNode = node;
            m_Station.OnPickFromQueuedInput();
            OnQueuedTrayPickUp?.Invoke();
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, MachineNode, m_WalkSpeed, OnGoingToNode, OnReachedMachineForProcessing);
        }

        private void OnReachedMachineForOrderPickUp(PathNode node)
        {
            m_CurrentNode = node;
            m_Station.OnPickFromMachine();
            PickFromMachine();
            /* StationInfo stationInfo = m_StationsInfo.Find((x) => x.IDIndex == m_CurrentStationInfo.IDIndex);
            if (stationInfo != null)
            {
                stationInfo.OnMachinePickUp?.Invoke();
            } */
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, OutputTrayNode, m_WalkSpeed, OnGoingToNode, OnReachedOutputTrayForOrderDrop);
        }
        private void PickFromMachine()
        {
            int totalOrderItems = m_CurrentMachineOutputInfos.Count;
            m_CurrentPickedOrderHolderInfo = m_PickedOrderHolderInfos.Find(info => info.HolderCapacity == totalOrderItems);
            if(m_CurrentPickedOrderHolderInfo != null)
            {
                m_CurrentPickedOrderHolderInfo.HolderRoot.SetActive(false);
                for(int i = 0; i < m_CurrentPickedOrderHolderInfo.Holders.Count; i++)
                {
                    StationSerialOrderInOutOnCustomerDemand.MachineOutputInfo machineOutputInfo = m_CurrentMachineOutputInfos[i];
                    Transform orderHolder = m_CurrentPickedOrderHolderInfo.Holders[i];
                    GameObject machineOutputObj = Instantiate(machineOutputInfo.OutputOrderDisplayObjPrefab, orderHolder);
                    machineOutputObj.transform.localPosition = Vector3.zero;
                    machineOutputObj.transform.localRotation = Quaternion.identity;
                    machineOutputObj.transform.localScale = Vector3.one;
                }
                m_CurrentPickedOrderHolderInfo.HolderRoot.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"{nameof(StationSerialOrderInOutOnCustomerDemandWorker)} -- No picked order holder info is available with capacity {totalOrderItems}");
            }
            OnMachinePickUp?.Invoke();
        }

        private void OnReachedOutputTrayForOrderDrop(PathNode node)
        {
            m_CurrentNode = node;
            m_IsRunning = false;
            m_AnimatorController.PlayIdle();
            m_Station.OnOrderToOutputTray();
            OnOrderPutDown?.Invoke();
            /* StationInfo stationInfo = m_StationsInfo.Find((x) => x.IDIndex == m_CurrentStationInfo.IDIndex);
            if (stationInfo != null)
            {
                stationInfo.OnOrderPutDown?.Invoke();
            }
            m_CurrentStationInfo = null; */
            // CheckQueue(/* out bool hasAnyTaskQueued */);
            /* if (!hasAnyTaskQueued)
            {
                GoToDefaultNode();
            } */
        }

        private void OnGoingToNode(PathNode node1, PathNode node2)
        {
            if (node1.TryGetComponent(out PathNodeDirection nodeDirection))
            {
                PathDirection direction = nodeDirection.GetDirection(node2);
                m_AnimatorController.PlayWalkAnimation(direction, m_WalkSpeed);
            }

        }

        public void ResetHolder()
        {
            m_CurrentPickedOrderHolderInfo.HolderRoot.SetActive(false);
            foreach(Transform holder in m_CurrentPickedOrderHolderInfo.Holders)
            {
                holder.DeleteAllChildren();
            }
            m_CurrentPickedOrderHolderInfo = null;
        }

        public void ResetToDefaultState()
        {
            PathTraverserExtension.StopTargetImmediately(transform);
            transform.position = m_DefaultNode.transform.position;
            m_CurrentNode = m_DefaultNode;
            m_AnimatorController.PlayIdle();
        }

        [Serializable]
        private class PickedOrderHolderInfo
        {
            public int HolderCapacity;
            public GameObject HolderRoot;
            public List<Transform> Holders = new();
        }
    }
}
