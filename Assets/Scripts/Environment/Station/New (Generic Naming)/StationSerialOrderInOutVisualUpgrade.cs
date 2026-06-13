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
    public class StationSerialOrderInOutVisualUpgrade : MonoBehaviour
    {
        [Serializable]
        public class StationOutInfo
        {
            [AllowNesting, ReadOnly]
            public bool IsHolding = false;
            public DataConsumable OrderOutType;
            public UnityEvent OnOrderInSuccesful;
            public UnityEvent OnOrderOutSuccesful;
        }

        [InfoBox("Specialized SerialOrderInOut station with complete dependency of its visuals and outputs on any of its upgrade properties. It behaves on the basis of CurrentUpgradeIndex of a predefined UpgradeType inside its Setup.")]
        [SerializeField, ReadOnly, BoxGroup("Class Description")] string m_Note = "See the Description Above!";

        //---Setup---
        const string MetaSetupFoldOut = "---Setup---";
        [SerializeField, Foldout(MetaSetupFoldOut), Expandable] DataStation m_Data;
        [SerializeField, Foldout(MetaSetupFoldOut)] PropertyUpgradeType m_UpgradeType;
        [SerializeField, Foldout(MetaSetupFoldOut)] TaskTrigger m_TaskTrigger;
        [SerializeField, Foldout(MetaSetupFoldOut)] DataConsumable m_OrderTypeIn;
        private StationUpgradedOutInfo m_CurrentStationUpgradedOutInfo;
        private List<StationOutInfo> m_CurrentStationOutInfos;

        //---Menu Calls---
        const string MetaMenuCallsFoldOut = "---Menu Calls---";
        [Header("-Station is locked"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsLockedMenu;
        [Header("-Station is unlocked (Not CALLED FIRST TIME)"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsUnlockdMenu;
        [Header("-Unlocking for the first time")]
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] Vector2 m_CameraFocusPosition;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraZoom;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraFocusDuration;
        [Foldout(MetaMenuCallsFoldOut)] public UnityEvent OnHasUnlockedMenu;
        [Foldout(MetaMenuCallsFoldOut)] public List<MenuUpgradeCallbackInfo> MenuUpgradeCallbackInfos = new();

        //---Gameplay Calls---
        const string MetaGameplayCallsFoldOut = "---Gameplay Calls---";
        [Header("-Station is locked"), Foldout(MetaGameplayCallsFoldOut)] public UnityEvent OnIsLockedGameplay;
        [Header("-Station is unlocked"), Foldout(MetaGameplayCallsFoldOut)] public UnityEvent OnIsUnlockdGameplay;
        [Foldout(MetaGameplayCallsFoldOut)] public List<GameplayUpgradeCallbackInfo> GameplayUpgradeCallbackInfos = new();

        //---Upgrade Properties---
        const string MetaUpgradePropertiesFoldOut = "---Upgrade Properties---";
        public float DurationProperty => m_DurationProperty;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] float m_DurationProperty;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] int m_CapacityProperty = 3;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] int m_CostProperty = 3;
        private bool m_IsProcessing = false;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] UnityEvent<int> OnDurationSetup;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] UnityEvent<float> OnDurationStart;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] UnityEvent OnDurationComplete;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] List<StationUpgradedOutInfo> m_StationUpgradedOutInfos = new();

        [ContextMenu("SetupForMenu")]
        public void SetupForMenu()
        {
            foreach(StationUpgradedOutInfo stationUpgradedOutInfo in m_StationUpgradedOutInfos)
            {
                StationUpgrade stationUpgrade = m_Data.GetStationUpgrade(m_UpgradeType);
                if(stationUpgrade != null && stationUpgrade.CurrentUpgradeIndex == stationUpgradedOutInfo.UpgradeLevelIndex)
                {
                    m_CurrentStationUpgradedOutInfo = stationUpgradedOutInfo;
                    m_CurrentStationOutInfos = stationUpgradedOutInfo.m_StationOutInfo;
                }
            }

            if (!m_Data.StationData.IsUnlocked)
            {
                OnIsLockedMenu?.Invoke();
            }
            else if (m_Data.StationData.IsUnlocked && !m_Data.StationData.HasJustUnlocked)
            {
                OnIsUnlockdMenu?.Invoke();
            }

            if (m_Data.StationData.HasJustUnlocked)
            {
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
                foreach(MenuUpgradeCallbackInfo menuUpgradeCallbackInfo in MenuUpgradeCallbackInfos)
                {
                    StationUpgrade stationUpgrade = m_Data.GetStationUpgrade(m_UpgradeType);
                    if(stationUpgrade != null && stationUpgrade.CurrentUpgradeIndex == menuUpgradeCallbackInfo.UpgradeLevelIndex)
                    {
                        menuUpgradeCallbackInfo.OnHasUpgradedMenu?.Invoke();
                        break;
                    }
                }
                // OnHasUpgradedMenu?.Invoke();
                m_Data.StationData.HasUpgraded = false;
                m_Data.Save();
            }
        }

        [ContextMenu("SetupForGameplay")]
        public void SetupForGameplay()
        {
            foreach(StationUpgradedOutInfo stationUpgradedOutInfo in m_StationUpgradedOutInfos)
            {
                StationUpgrade stationUpgrade = m_Data.GetStationUpgrade(m_UpgradeType);
                if(stationUpgrade != null && stationUpgrade.CurrentUpgradeIndex == stationUpgradedOutInfo.UpgradeLevelIndex)
                {
                    m_CurrentStationUpgradedOutInfo = stationUpgradedOutInfo;
                    m_CurrentStationOutInfos = stationUpgradedOutInfo.m_StationOutInfo;
                }
            }

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
                foreach(GameplayUpgradeCallbackInfo gameplayUpgradeCallbackInfo in GameplayUpgradeCallbackInfos)
                {
                    StationUpgrade stationUpgrade = m_Data.GetStationUpgrade(m_UpgradeType);
                    if(stationUpgrade != null && stationUpgrade.CurrentUpgradeIndex == gameplayUpgradeCallbackInfo.UpgradeLevelIndex)
                    {
                        gameplayUpgradeCallbackInfo.OnHasUpgradedGameplay?.Invoke();
                        break;
                    }
                }
                // OnHasUpgradedGameplay?.Invoke();
                m_Data.StationData.HasUpgraded = false;
                m_Data.Save();
            }

            StationUpgrade upgradeCapacity = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
            if (upgradeCapacity != null)
            {
                m_CapacityProperty = (int)upgradeCapacity.Upgrade[upgradeCapacity.CurrentUpgradeIndex];
            }

            StationUpgrade upgradeDuration = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
            if (upgradeDuration != null)
            {
                m_DurationProperty = upgradeDuration.Upgrade[upgradeDuration.CurrentUpgradeIndex];
                m_CurrentStationUpgradedOutInfo.OnDurationSetup?.Invoke(upgradeDuration.CurrentUpgradeIndex);
                // OnDurationSetup?.Invoke(upgradeDuration.CurrentUpgradeIndex);
            }

            StationUpgrade upgradeCost = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
            if (upgradeCost != null)
            {
                m_CostProperty = Mathf.RoundToInt(upgradeCost.Upgrade[upgradeCost.CurrentUpgradeIndex]);
            }
        }

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
                bool isTaskSuccessful = false;
                bool gotNextFoodType = false;

                List<StationOutInfo> stationOutInfos = m_CurrentStationOutInfos.FindAll((x) => x.IsHolding == true);
                if (stationOutInfos.Count <= 1 && m_IsProcessing == false)
                {
                    if (interactable.GetDataConsumable(m_OrderTypeIn).Item1)
                    {
                        gotNextFoodType = true;
                    }
                }

                foreach (var item in m_CurrentStationOutInfos)
                {
                    if (item.IsHolding)
                    {
                        if (interactable.SendDataConsumable(item.OrderOutType, m_CostProperty))
                        {
                            item.IsHolding = false;
                            item.OnOrderOutSuccesful?.Invoke();
                            isTaskSuccessful = true;
                            break;
                        }
                    }
                }

                if (gotNextFoodType)
                {
                    m_IsProcessing = true;
                    m_CurrentStationUpgradedOutInfo.OnDurationStart?.Invoke(m_DurationProperty);
                    CoroutineManager.LateAction(() =>
                    {
                        m_CurrentStationUpgradedOutInfo.OnDurationComplete?.Invoke();
                        int count = 0;
                        foreach (var item in m_CurrentStationOutInfos)
                        {
                            if (count < m_CapacityProperty)
                            {
                                item.IsHolding = true;
                                item.OnOrderInSuccesful?.Invoke();
                            }
                            count++;
                        }
                        m_IsProcessing = false;
                    }, m_DurationProperty);
                    isTaskSuccessful = true;
                }
               

                if (isTaskSuccessful)
                {
                    m_TaskTrigger.SendTaskResult(TaskResult.Success);
                    return;
                }
                else
                {
                    GlobalEventHolder.OnHintByConsumable?.Invoke(m_OrderTypeIn);
                }
            }
            
            m_TaskTrigger.SendTaskResult(TaskResult.Failed);
        }

        #region Nested Classes
        [Serializable]
        public class MenuUpgradeCallbackInfo
        {
            public int UpgradeLevelIndex;
            public UnityEvent OnHasUpgradedMenu;
        }
        [Serializable]
        public class GameplayUpgradeCallbackInfo
        {
            public int UpgradeLevelIndex;
            public UnityEvent OnHasUpgradedGameplay;
        }
        [Serializable]
        public class StationUpgradedOutInfo
        {
            public int UpgradeLevelIndex;
            public UnityEvent<int> OnDurationSetup;
            public UnityEvent<float> OnDurationStart;
            public UnityEvent OnDurationComplete;
            public List<StationOutInfo> m_StationOutInfo = new();
        }
        #endregion
    }
}
