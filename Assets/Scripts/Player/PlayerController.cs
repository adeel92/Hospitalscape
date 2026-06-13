using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.TaskSystem;
using Isometric.PathSystem;
using Isometric.Environment;
using Isometric.Data;
using Isometric.Sound;

namespace Isometric.Player
{
    public class PlayerController : MonoBehaviour, IEnvironmentInteractable
    {
        [Header("---Setup---")]
        [SerializeField, Expandable] DataPlayer m_Data;
        [SerializeField] TaskTarget m_TaskTarget;
        [SerializeField] PlayerAnimatorController m_AnimatorController;
        [SerializeField] PlayerTrayController m_TrayController;
        [SerializeField] SortingGroup m_SortingGroup;
        [SerializeField] float m_TaskFailureDelay = 1.5f;
        public UnityEvent OnPropertyUpgraded;
        public UnityEvent OnSpeedBoosterActivated;
        public UnityEvent OnSpeedBoosterDectivated;
        private string m_DefaultSortingLayer;
        private int m_DefaultSortingOrder;

        [Header("---Paramaters---")]
        [SerializeField] float m_WalkSpeed;
        private float m_DefaultWalkSpeed;
        [SerializeField] int m_HoldingCapacity;
        [SerializeField] float m_WalkBoosterSpeed;

        public void SetupForMenu()
        {
            if (m_Data.PlayerData.IsUpgraded)
            {
                OnPropertyUpgraded?.Invoke();
                m_Data.PlayerData.IsUpgraded = false;
                m_Data.Save();
            }


            m_HoldingCapacity = m_Data.PlayerData.Capacity[m_Data.PlayerData.CurrentCapacityIndex];
            m_WalkSpeed = m_Data.PlayerData.WalkSpeed[m_Data.PlayerData.CurrentWalkSpeedIndex];

            m_DefaultWalkSpeed = m_WalkSpeed;

            m_TaskTarget.SetPathTraverseSpeed(m_WalkSpeed);
            m_DefaultSortingLayer = m_SortingGroup.sortingLayerName;
            m_DefaultSortingOrder = m_SortingGroup.sortingOrder;
            m_TrayController.Setup(m_HoldingCapacity);
        }

        public void SetupForGameplay()
        {
            if (m_Data.PlayerData.IsUpgraded)
            {
                OnPropertyUpgraded?.Invoke();
                m_Data.PlayerData.IsUpgraded = false;
                m_Data.Save();
            }

            m_HoldingCapacity = m_Data.PlayerData.Capacity[m_Data.PlayerData.CurrentCapacityIndex];
            m_WalkSpeed = m_Data.PlayerData.WalkSpeed[m_Data.PlayerData.CurrentWalkSpeedIndex];

            m_DefaultWalkSpeed = m_WalkSpeed;

            m_TaskTarget.SetPathTraverseSpeed(m_WalkSpeed);
            m_DefaultSortingLayer = m_SortingGroup.sortingLayerName;
            m_DefaultSortingOrder = m_SortingGroup.sortingOrder;
            m_TrayController.Setup(m_HoldingCapacity);

            //m_AnimatorController.PlayHappyWave();
        }

        private void OnEnable()
        {
            m_TaskTarget.OnStartingFirstTask += OnStartingFirstTask;
            m_TaskTarget.OnGoingToNode += OnGoingToNode;
            m_TaskTarget.OnReachedTargetNode += OnReachedTargetNode;
            m_TaskTarget.OnGoingToNodeDistance += OnGoingToNodeDistance;
            m_TaskTarget.OnTaskComplete += OnTaskComplete;
            m_TaskTarget.OnNoMoreTasks += OnNoMoreTasks;

            GlobalEventHolder.OnWaitressSpeedBooster += OnManagerSpeedBooster;
            GlobalEventHolder.OnGameWon += OnGameWon;
            GlobalEventHolder.OnGameLost += OnGameLost;
            GlobalEventHolder.OnTaskAssigned += OnTaskAssigned;
        }

        private void OnDisable()
        {
            m_TaskTarget.OnStartingFirstTask -= OnStartingFirstTask;
            m_TaskTarget.OnGoingToNode -= OnGoingToNode;
            m_TaskTarget.OnReachedTargetNode -= OnReachedTargetNode;
            m_TaskTarget.OnGoingToNodeDistance -= OnGoingToNodeDistance;
            m_TaskTarget.OnTaskComplete -= OnTaskComplete;
            m_TaskTarget.OnNoMoreTasks -= OnNoMoreTasks;

            GlobalEventHolder.OnWaitressSpeedBooster -= OnManagerSpeedBooster;
            GlobalEventHolder.OnGameWon -= OnGameWon;
            GlobalEventHolder.OnGameLost -= OnGameLost;
            GlobalEventHolder.OnTaskAssigned -= OnTaskAssigned;
        }

        private void OnStartingFirstTask()
        {
            GlobalEventHolder.OnPlayerStartedMoving?.Invoke();
        }

        private void OnGoingToNode(PathNode node1, PathNode node2)
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

        private void OnGoingToNodeDistance(float distance)
        {
            GlobalEventHolder.OnPlayerWalkDistance?.Invoke(Mathf.RoundToInt(distance));
        }

        private void OnReachedTargetNode(PathNode node, bool isNextTaskRunning)
        {
            if (isNextTaskRunning == false)
            {
                m_AnimatorController.PlayIdle();
            }
        }


        private float OnTaskComplete(TaskResult taskResult)
        {
            if (taskResult == TaskResult.Success)
            {
                return 0;
            }
            else
            {
                m_AnimatorController.PlayWhatCanIdo();
                return m_TaskFailureDelay;
            }
        }


        private void OnNoMoreTasks()
        {
            GlobalEventHolder.OnPlayerStopMoving?.Invoke();
            m_AnimatorController.PlayIdle();
        }


        private void OnManagerSpeedBooster(bool on)
        {
            if (on == true)
            {
                OnSpeedBoosterActivated?.Invoke();
                m_WalkSpeed = m_WalkBoosterSpeed;
                m_TaskTarget.SetPathTraverseSpeed(m_WalkSpeed);

            }
            else
            {
                OnSpeedBoosterDectivated?.Invoke();
                m_WalkSpeed = m_DefaultWalkSpeed;
                m_TaskTarget.SetPathTraverseSpeed(m_WalkSpeed);
            }
        }

        private void OnTaskAssigned()
        {
            SoundManager.PlaySound(SoundType.TaskAssigned);
        }

        private void OnGameWon()
        {
            GlobalEventHolder.OnCurrentTaskTargetCancleImmediately?.Invoke();
            SoundManager.PlaySound(SoundType.CrowdCheering);
            m_AnimatorController.PlayHappyWon();
        }

        private void OnGameLost()
        {
            GlobalEventHolder.OnCurrentTaskTargetCancleImmediately?.Invoke();
            m_AnimatorController.PlaySadLost();
        }

        #region Station Interactable
        public Tuple<bool, int> GetDataConsumable(DataConsumable dataConsumable)
        {
            if (m_TrayController.HasItem(dataConsumable.Key))
            {
                SoundManager.PlaySound(SoundType.TaskInteractions);
                return m_TrayController.RemoveItem(dataConsumable.Key);
            }
            else
            {
                return new Tuple<bool, int>(false, 0);
            }
        }

        public bool SendDataConsumable(DataConsumable dataConsumable, int itemCost)
        {
            if (m_TrayController.HasCapacity())
            {
                SoundManager.PlaySound(SoundType.TaskInteractions);
                m_TrayController.AddItem(dataConsumable.Key, dataConsumable.ConsumableTrayPrefab, itemCost);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RemoveAllDataConsumables()
        {
            SoundManager.PlaySound(SoundType.TaskInteractions);
            m_TrayController.RemoveAllItem();
        }

        public void EngageInteractable(PathDirection direction)
        {
            SoundManager.PlaySound(SoundType.TaskInteractions);
            m_AnimatorController.PlayWorkAnimation(direction);
        }
        #endregion
    }
}
