using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.TaskSystem;
using Isometric.Cam;
using Isometric.UI;

namespace Isometric.Environment
{
    public class StationParallelFoodInOut : MonoBehaviour
    {
        [Serializable]
        private class StationInfo
        {
            public TaskTrigger TaskTrigger;
            public DataConsumable FoodInType;
            public DataConsumable FoodOutType;
            public float DurationProperty;
            public int CostProperty;
            private bool IsProcessing = false;
            private bool IsHoldingFoodType = false;
            [SerializeField] UnityEvent OnFoodInSuccesful;
            [SerializeField] UnityEvent<float> OnDurationStart;
            [SerializeField] UnityEvent OnDurationComplete;
            [SerializeField] UnityEvent OnFoodOutSuccesful;

            public void OnTaskStart(TaskTarget taskTarget)
            {
                if (IsProcessing == false)
                {
                    if (taskTarget.TryGetComponent(out IEnvironmentInteractable interactable))
                    {
                        bool isTaskComplete = false;
                        bool gotNextFoodType = false;


                        if (interactable.GetDataConsumable(FoodInType).Item1)
                        {
                            gotNextFoodType = true;
                        }

                        if (IsHoldingFoodType)
                        {
                            if(interactable.SendDataConsumable(FoodOutType, CostProperty))
                            {
                                IsHoldingFoodType = false;
                                OnFoodOutSuccesful?.Invoke();
                                isTaskComplete = true;
                            }
                        }
                        
                        if (gotNextFoodType)
                        {
                            OnFoodInSuccesful?.Invoke();
                            IsProcessing = true;
                            OnDurationStart?.Invoke(DurationProperty);
                            CoroutineManager.LateAction(() =>
                            {
                                IsHoldingFoodType = true;
                                IsProcessing = false;
                                OnDurationComplete?.Invoke();
                            }, DurationProperty);
                            isTaskComplete = true;
                        }
                        

                        if (isTaskComplete)
                        {
                            TaskTrigger.SendTaskResult(TaskResult.Success);
                            return;
                        }
                        else
                        {
                            GlobalEventHolder.OnHintByConsumable?.Invoke(FoodInType);
                        }
                    }
                }
                
                TaskTrigger.SendTaskResult(TaskResult.Failed);
            }
        }

        [Serializable]
        private class UpgradePropertyCallbackInfo
        {
            public PropertyUpgradeType UpgradeProperty;
            public int MatchIndex;

            public UnityEvent OnUpgradePropertySetup;
        }

        //---Setup---
        private const string MetaSetupFoldOut = "---Setup---";
        [SerializeField, Foldout(MetaSetupFoldOut), Expandable] DataStation m_Data;

        //---Menu Calls---
        private const string MetaMenuCallsFoldOut = "---Menu Calls---";
        [Header("-Station is locked"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsLockedMenu;
        [Header("-Station is unlocked (Not CALLED FIRST TIME UNLOCKING)"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsUnlockdMenu;
        [Header("-Unlocking for the first time")]
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] Vector2 m_CameraFocusPosition;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraZoom;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraFocusDuration;
        [Foldout(MetaMenuCallsFoldOut)] public UnityEvent OnHasUnlockedMenu;
        [Header("-Upgraded any of the properties"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnHasUpgradedMenu;
        [Header("-Upgraded property setup calls (NOT CALLED WHEN LOCKED OR UNLOCKING FIRSTIME"), Foldout(MetaMenuCallsFoldOut)]
        [SerializeField] List<UpgradePropertyCallbackInfo> m_UpgradePropertyMenuCallbackInfos;

        //---Gameplay Calls---
        private const string MetaGameplayCallsFoldOut = "---Gameplay Calls---";
        [Header("-Station is locked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsLockedGameplay;
        [Header("-Station is unlocked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsUnlockdGameplay;
        [Header("-Upgraded any of the properties"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnHasUpgradedGameplay;
        [Header("-Upgraded property setupcalls (NOT CALLED WHEN LOCKED OR ON UNLOCKED FIRST TIME)"), Foldout(MetaGameplayCallsFoldOut)]
        [SerializeField] List<UpgradePropertyCallbackInfo> m_UpgradePropertyGameplayCallbackInfos;

        //---Parallel Stations---
        private const string MetaParallelStationsFoldOut = "---Parallel Stations---";
        [SerializeField, Foldout(MetaParallelStationsFoldOut)] List<StationInfo> m_StationInfo;

        [ContextMenu("SetupForMenu")]
        public void SetupForMenu()
        {
            if (!m_Data.StationData.IsUnlocked)
            {
                OnIsLockedMenu?.Invoke();
            }
            else if (m_Data.StationData.IsUnlocked && !m_Data.StationData.HasJustUnlocked)
            {
                OnIsUnlockdMenu?.Invoke();
            }

            bool hasJustUnlocked = false;
            if (m_Data.StationData.HasJustUnlocked)
            {
                hasJustUnlocked = true;
                CameraController.RegisterFocusCamera(m_CameraFocusPosition, m_CameraZoom, 1.4f, 
                () =>
                {
                    UIManager.UIInteractionOff();
                    UIManager.HideMenu(null);
                    CameraController.Interactability(false);
                },
                () =>
                {
                    OnHasUnlockedMenu?.Invoke();
                    CoroutineManager.LateAction(() =>
                    {
                        if (CameraController.NextFocusCamera() == false)
                        {
                            CameraController.SetupForMenu(() =>
                            {
                                UIManager.CheckNextUpdatable();
                            });
                        }
                    }, m_CameraFocusDuration);
                });
                m_Data.StationData.HasJustUnlocked = false;
                m_Data.Save();
            }

            if (m_Data.StationData.HasUpgraded)
            {
                OnHasUpgradedMenu?.Invoke();
                m_Data.StationData.HasUpgraded = false;
                m_Data.Save();
            }

            if (m_Data.StationData.IsUnlocked == true
                && hasJustUnlocked == false)
            {
                StationUpgrade capacityUpgrade = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
                if (capacityUpgrade != null)
                {
                    foreach (var upgradeCallbackInfo in m_UpgradePropertyMenuCallbackInfos)
                    {
                        if (upgradeCallbackInfo.UpgradeProperty == PropertyUpgradeType.Capacity
                            && upgradeCallbackInfo.MatchIndex == capacityUpgrade.CurrentUpgradeIndex)
                        {
                            upgradeCallbackInfo.OnUpgradePropertySetup?.Invoke();
                            break;
                        }
                    }
                }
            }
        }

        [ContextMenu("SetupForGameplay")]
        public void SetupForGameplay()
        {
            if (!m_Data.StationData.IsUnlocked)
            {
                OnIsLockedGameplay?.Invoke();
            }
            else if (m_Data.StationData.IsUnlocked)
            {
                OnIsUnlockdGameplay?.Invoke();
            }

            if (m_Data.StationData.HasUpgraded)
            {
                OnHasUpgradedMenu?.Invoke();
                m_Data.StationData.HasUpgraded = false;
                m_Data.Save();
            }

            if (m_Data.StationData.IsUnlocked == true)
            {
                StationUpgrade capacityUpgrade = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
                if (capacityUpgrade != null)
                {
                    foreach (var upgradeCallbackInfo in m_UpgradePropertyGameplayCallbackInfos)
                    {
                        if (upgradeCallbackInfo.UpgradeProperty == PropertyUpgradeType.Capacity
                            && upgradeCallbackInfo.MatchIndex == capacityUpgrade.CurrentUpgradeIndex)
                        {
                            upgradeCallbackInfo.OnUpgradePropertySetup?.Invoke();
                            break;
                        }
                    }
                }
            }

            StationUpgrade durationUpgrade = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
            if (durationUpgrade != null)
            {
                float duration = durationUpgrade.Upgrade[durationUpgrade.CurrentUpgradeIndex];

                foreach (var stataionInfo in m_StationInfo)
                {
                    stataionInfo.DurationProperty = duration;
                }
            }

            StationUpgrade costUpgrade = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
            if (costUpgrade != null)
            {
                float cost = costUpgrade.Upgrade[costUpgrade.CurrentUpgradeIndex];

                foreach (var stataionInfo in m_StationInfo)
                {
                    stataionInfo.CostProperty = Mathf.RoundToInt(cost);
                }
            }

        }

        private void OnEnable()
        {
            foreach (var item in m_StationInfo)
            {
                item.TaskTrigger.OnTaskStart += item.OnTaskStart;
            }
        }

        private void OnDisable()
        {
            foreach (var item in m_StationInfo)
            {
                item.TaskTrigger.OnTaskStart -= item.OnTaskStart;
            }
        }
        
    }
}
