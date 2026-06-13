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
    public class StationAutoOrderOutVisualUpgrade : MonoBehaviour
    {

        [Serializable]
        private class ProperityCallbackInfo
        {
            public int CapacityPropertyMatch;

            [Header("-This is also called at the menu")]
            public UnityEvent<int> OnDurationSetup;
            public UnityEvent<float> OnDurationStart;
            public UnityEvent OnDurationComplete;

            public List<FoodOutSuccesfulInfo> FoodOutSuccesfulInfos;

            [Serializable]
            public class FoodOutSuccesfulInfo
            {
                public int CurrentCapacity;
                public UnityEvent OnFoodOutSuccesful;
            }
        }

        [InfoBox("Specialized AutoOrderOut station with complete dependency of its visuals and outputs on any of its upgrade properties. It behaves on the basis of CurrentUpgradeIndex of a predefined UpgradeType inside its Setup.")]
        [SerializeField, ReadOnly, BoxGroup("Class Description")] string m_Note = "See the Description Above!";

        //---Setup---
        const string MetaSetupFoldOut = "---Setup---";
        [SerializeField, Foldout(MetaSetupFoldOut), Expandable]DataStation m_Data;
        [SerializeField, Foldout(MetaSetupFoldOut)] PropertyUpgradeType m_UpgradeType;
        [SerializeField, Foldout(MetaSetupFoldOut)] TaskTrigger m_TaskTrigger;
        [SerializeField, Foldout(MetaSetupFoldOut)] DataConsumable m_FoodType;

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
        [Header("-Upgraded any of the properties"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnHasUpgradedMenu;
        [Foldout(MetaMenuCallsFoldOut)] public List<MenuUpgradeCallbackInfo> MenuUpgradeCallbackInfos = new();

        //---Gameplay Calls---
        const string MetaGameplayCallsFoldOut = "---Gameplay Calls---";
        [Header("-Station is locked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsLockedGameplay;
        [Header("-Station is unlocked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsUnlockdGameplay;
        [Header("-Upgraded any of the properties"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnHasUpgradedGameplay;
        [Foldout(MetaGameplayCallsFoldOut)] public List<GameplayUpgradeCallbackInfo> GameplayUpgradeCallbackInfos = new();

        //---Upgrade Properties---
        const string MetaUpgradePropertiesFoldOut = "---Upgrade Properties---";
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] float m_DurationProperty;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] int m_CapacityProperty;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] int m_CostProperty;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] List<ProperityCallbackInfo> m_ProperityCallbackInfos;
        private ProperityCallbackInfo m_CurrentProperityCallbackInfo = null;
        private bool m_IsProcessing = false;
        private int m_HoldingCapacity = 0;


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

            if (m_Data.StationData.IsUnlocked && !hasJustUnlocked)
            {
                StationUpgrade upgradeCapacity = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
                if (upgradeCapacity != null)
                {
                    m_CapacityProperty = (int)upgradeCapacity.Upgrade[upgradeCapacity.CurrentUpgradeIndex];

                    foreach (var propertyCallbackInfo in m_ProperityCallbackInfos)
                    {
                        if (propertyCallbackInfo.CapacityPropertyMatch == m_CapacityProperty)
                        {
                            m_CurrentProperityCallbackInfo = propertyCallbackInfo;
                            break;
                        }
                    }
                }

                StationUpgrade upgradeDuration = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
                if (upgradeDuration != null)
                {
                    m_DurationProperty = upgradeDuration.Upgrade[upgradeDuration.CurrentUpgradeIndex];
                    m_CurrentProperityCallbackInfo.OnDurationSetup?.Invoke(upgradeDuration.CurrentUpgradeIndex);
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


            if (m_Data.StationData.IsUnlocked)
            {
                StationUpgrade upgradeCapacity = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
                if (upgradeCapacity != null)
                {
                    m_CapacityProperty = (int)upgradeCapacity.Upgrade[upgradeCapacity.CurrentUpgradeIndex];

                    foreach (var propertyCallbackInfo in m_ProperityCallbackInfos)
                    {
                        if (propertyCallbackInfo.CapacityPropertyMatch == m_CapacityProperty)
                        {
                            m_CurrentProperityCallbackInfo = propertyCallbackInfo;
                            break;
                        }
                    }
                }

                StationUpgrade upgradeDuration = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
                if (upgradeDuration != null)
                {
                    m_DurationProperty = upgradeDuration.Upgrade[upgradeDuration.CurrentUpgradeIndex];
                    m_CurrentProperityCallbackInfo.OnDurationSetup?.Invoke(upgradeDuration.CurrentUpgradeIndex);
                }

                StationUpgrade upgradeCost = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
                if (upgradeCost != null)
                {
                    m_CostProperty = Mathf.RoundToInt(upgradeCost.Upgrade[upgradeCost.CurrentUpgradeIndex]);
                }

                AutoMakeFood();
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
                if (m_IsProcessing == false && m_HoldingCapacity > 0)
                {
                    if (interactable.SendDataConsumable(m_FoodType, m_CostProperty))
                    {
                        m_HoldingCapacity--;
                        var foodOutSuccesfulInfos = m_CurrentProperityCallbackInfo.FoodOutSuccesfulInfos.Find((x) => x.CurrentCapacity == m_HoldingCapacity);
                        if (foodOutSuccesfulInfos != null)
                        {
                            foodOutSuccesfulInfos.OnFoodOutSuccesful?.Invoke();
                        }
                        AutoMakeFood();
                        m_TaskTrigger.SendTaskResult(TaskResult.Success);
                        return;
                    }
                }
            }
            
            m_TaskTrigger.SendTaskResult(TaskResult.Failed);
        }

        private void AutoMakeFood()
        {
            if (m_HoldingCapacity <= 0)
            {
                m_IsProcessing = true;
                m_CurrentProperityCallbackInfo.OnDurationStart?.Invoke(m_DurationProperty);
                CoroutineManager.LateAction(() => 
                {
                    m_IsProcessing = false;
                    m_HoldingCapacity = m_CapacityProperty;
                    m_CurrentProperityCallbackInfo.OnDurationComplete?.Invoke();
                }, m_DurationProperty);
            }
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
        #endregion
    }
}
