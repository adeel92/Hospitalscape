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
using Isometric.Customer;

namespace Isometric.Environment
{
    public class StationCustomerIn : MonoBehaviour
    {
        [Serializable]
        public class StationCapacityInfo
        {
            [AllowNesting, ReadOnly] 
            public bool IsUnlocked;
            public StationCustomerInHandler Handler;
            [Header("-Station is unlocked (Not CALLED FIRST TIME)")]
            public UnityEvent OnIsLocked;
            public UnityEvent OnIsUnlockd;
            [Header("-Unlocking for the first time")]
            public UnityEvent OnHasUnlocked;
        }


        //---Setup---
        const string MetaSetupFoldOut = "---Setup---";
        [SerializeField, Foldout(MetaSetupFoldOut), Expandable]
        private DataStation m_Data;

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

        //---Gameplay Calls---
        const string MetaGameplayCallsFoldOut = "---Gameplay Calls---";
        [Header("-Station is locked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsLockedGameplay;
        [Header("-Station is unlocked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsUnlockdGameplay;
        [Header("-Upgraded any of the properties"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnHasUpgradedGameplay;

        //---Upgrade Properties---
        const string MetaUpgradePropertiesFoldOut = "---Upgrade Properties---";
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] float m_DurationProperty;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] int m_CapacityProperty;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] int m_CostProperty;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] List<StationCapacityInfo> m_StationCapacityInfos;
        private int m_CurrentCpacityIndex = -1;//Default is -1 to check if any capacity upgrade happend already

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


            StationUpgrade upgradeCapacity = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
            if (m_CurrentCpacityIndex != -1)
            {
                m_CurrentCpacityIndex = upgradeCapacity.CurrentUpgradeIndex;
            }

            StationUpgrade upgradeDuration = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
            if (upgradeDuration != null)
            {
                m_DurationProperty = upgradeDuration.Upgrade[upgradeDuration.CurrentUpgradeIndex];
            }

            StationUpgrade upgradeCost = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
            if (upgradeCost != null)
            {
                m_CostProperty = Mathf.RoundToInt(upgradeCost.Upgrade[upgradeCost.CurrentUpgradeIndex]);
            }


            SetupStationHandlers(upgradeCapacity, true, false);

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
                    SetupStationHandlers(upgradeCapacity, true, true);

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
                if (!hasJustUnlocked)
                {
                    bool hasCapacityUpgrade = false;
                    if (m_CurrentCpacityIndex != upgradeCapacity.CurrentUpgradeIndex)
                    {
                        hasCapacityUpgrade = true;
                    }
                    SetupStationHandlers(upgradeCapacity, true, hasCapacityUpgrade);
                }

                OnHasUpgradedMenu?.Invoke();
                m_Data.StationData.HasUpgraded = false;

                m_Data.Save();
            }
        }



        [ContextMenu("SetupForGameplay")]
        public void SetupForGameplay()
        {
            if (!m_Data.StationData.IsUnlocked)
            {
                OnIsLockedGameplay?.Invoke();
            }
            else if (m_Data.StationData.IsUnlocked && !m_Data.StationData.HasJustUnlocked)
            {
                OnIsUnlockdGameplay?.Invoke();
            }


            StationUpgrade upgradeCapacity = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
            if (m_CurrentCpacityIndex != -1)
            {
                m_CurrentCpacityIndex = upgradeCapacity.CurrentUpgradeIndex;
            }

            StationUpgrade upgradeDuration = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
            if (upgradeDuration != null)
            {
                m_DurationProperty = upgradeDuration.Upgrade[upgradeDuration.CurrentUpgradeIndex];
            }

            StationUpgrade upgradeCost = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
            if (upgradeCost != null)
            {
                m_CostProperty = Mathf.RoundToInt(upgradeCost.Upgrade[upgradeCost.CurrentUpgradeIndex]);
            }


            if (m_Data.StationData.HasUpgraded)
            {
                bool hasCapacityUpgrade = false;
                if (m_CurrentCpacityIndex != upgradeCapacity.CurrentUpgradeIndex)
                {
                    hasCapacityUpgrade = true;
                }
                SetupStationHandlers(upgradeCapacity, false, hasCapacityUpgrade);

                OnHasUpgradedGameplay?.Invoke();
                m_Data.StationData.HasUpgraded = false;

                m_Data.Save();
            }
            else if(m_Data.StationData.IsUnlocked)
            {
                SetupStationHandlers(upgradeCapacity, false, false);
            }
        }

        private void SetupStationHandlers(StationUpgrade upgradeCapacity, bool isForMenu, bool hasCapacityUpgrade)
        {
            for (int i = 0; i < m_StationCapacityInfos.Count; i++)
            {
                if (i <= upgradeCapacity.CurrentUpgradeIndex)
                {
                    if (hasCapacityUpgrade && i == upgradeCapacity.CurrentUpgradeIndex)
                    {
                        m_StationCapacityInfos[i].IsUnlocked = true;
                        m_StationCapacityInfos[i].OnHasUnlocked?.Invoke();
                    }
                    else
                    {
                        m_StationCapacityInfos[i].IsUnlocked = true;
                        m_StationCapacityInfos[i].OnIsUnlockd?.Invoke();
                    }


                    if (isForMenu) m_StationCapacityInfos[i].Handler.SetupForMenu(m_DurationProperty, m_CostProperty);
                    else m_StationCapacityInfos[i].Handler.SetupForGameplay(m_DurationProperty, m_CostProperty);
                }
                else
                {
                    m_StationCapacityInfos[i].IsUnlocked = false;
                    m_StationCapacityInfos[i].OnIsLocked?.Invoke();
                }
            }
        }
    }
}
