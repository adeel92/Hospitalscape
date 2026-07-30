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
using Isometric.Tutorial;
using Unity.VisualScripting;

namespace Isometric.Environment
{
    public class StationSerialOrderInOut : MonoBehaviour
    {
        [Serializable]
        private class StationOutInfo
        {
            [AllowNesting, ReadOnly]
            public bool IsHolding = false;
            public DataConsumable OrderOutType;
            public UnityEvent OnOrderInSuccesful;
            public UnityEvent OnOrderOutSuccesful;
        }

        //---Setup---
        const string MetaSetupFoldOut = "---Setup---";
        [SerializeField, Foldout(MetaSetupFoldOut), Expandable] 
        private DataStation m_Data;
        [SerializeField, Foldout(MetaSetupFoldOut)] TaskTrigger m_TaskTrigger;
        [SerializeField, Foldout(MetaSetupFoldOut)] DataConsumable m_OrderTypeIn;

        //---Menu Calls---
        const string MetaMenuCallsFoldOut = "---Menu Calls---";
        [Header("-Station is locked"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsLockedMenu;
        [Header("-Station is unlocked (Not CALLED FIRST TIME)"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsUnlockdMenu;
        /* [Header("-Unlocking for the first time")]
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] Vector2 m_CameraFocusPosition;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraZoom;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraFocusDuration;
        [Foldout(MetaMenuCallsFoldOut)] public UnityEvent OnHasUnlockedMenu; */
        [Header("-Upgraded any of the properties"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnHasUpgradedMenu;

        //---Gameplay Calls---
        const string MetaGameplayCallsFoldOut = "---Gameplay Calls---";
        [Header("-Station is locked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsLockedGameplay;
        [Header("-Station is unlocked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsUnlockdGameplay;
        [Header("-Unlocking for the first time")]
        [SerializeField, Foldout(MetaGameplayCallsFoldOut)] bool m_UseCameraFocus = false;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut), ShowIf(nameof(m_UseCameraFocus))] Vector2 m_CameraFocusPosition;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut), ShowIf(nameof(m_UseCameraFocus))] float m_CameraZoom;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut)] bool m_UseSoftMaskFocus = true;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut), ShowIf(nameof(m_UseSoftMaskFocus))] Transform m_FocusTarget;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut), ShowIf(nameof(m_UseSoftMaskFocus))] Vector2 m_FocusPositionOffset;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut), ShowIf(nameof(m_UseSoftMaskFocus))] Vector2 m_FocusScale;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut), ShowIf(nameof(m_UseSoftMaskFocus))] FocusShapeType m_FocusShapeType;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut)] float m_FocusDuration;
        [Foldout(MetaGameplayCallsFoldOut)] public UnityEvent OnHasUnlockedGameplay;
        [Header("-Upgraded any of the properties"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnHasUpgradedGameplay;

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
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] List<StationOutInfo> m_StationOutInfo;

        [ContextMenu("SetupForMenu")]
        public void SetupForMenu()
        {
            if (!m_Data.StationData.IsUnlocked)
            {
                OnIsLockedMenu?.Invoke();
            }
            else if (m_Data.StationData.IsUnlocked /* && !m_Data.StationData.HasJustUnlocked */)
            {
                OnIsUnlockdMenu?.Invoke();
            }

            if (m_Data.StationData.HasUpgraded)
            {
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

            if (m_Data.StationData.HasJustUnlocked)
            {
                if (m_UseCameraFocus)
                {
                    CameraController.RegisterFocusCamera(m_CameraFocusPosition, m_CameraZoom, 1.4f, 
                    () =>
                    {
                        UIManager.UIInteractionOff();
                        // GameManager.PauseGame();
                    }, 
                    () =>
                    {
                        OnHasUnlockedGameplay?.Invoke();
                        CoroutineManager.LateAction(() =>
                        {
                            if (CameraController.NextFocusCamera() == false)
                            {
                                CameraController.SetupForGameplay(() =>
                                {
                                    UIManager.CheckNextGameplayUpdatable();
                                });
                            }

                        }, m_FocusDuration);
                    });
                }
                else if (m_UseSoftMaskFocus)
                {
                    /* NOT 100% COMPLETE! BELOW FOCUS WILL WORK BUT ADDITIONAL DESCRIPTION MECHANISM IS PENDING */

                    TutorialFocusManager.FocusAtTransform(m_FocusTarget, m_FocusPositionOffset, m_FocusScale, m_FocusShapeType,
                    () =>
                    {
                        UIManager.UIInteractionOff();
                    },
                    () =>
                    {
                        OnHasUnlockedGameplay?.Invoke();
                        CoroutineManager.LateAction(() =>
                        {
                            TutorialFocusManager.StopAllFocus();
                            CameraController.SetupForGameplay(() =>
                            {
                                UIManager.CheckNextGameplayUpdatable();
                            });

                        }, m_FocusDuration);
                    });
                }

                m_Data.StationData.HasJustUnlocked = false;
                m_Data.Save();
            }

            if (m_Data.StationData.HasUpgraded)
            {
                OnHasUpgradedGameplay?.Invoke();
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
                OnDurationSetup?.Invoke(upgradeDuration.CurrentUpgradeIndex);
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

                List<StationOutInfo> stationOutInfos = m_StationOutInfo.FindAll((x) => x.IsHolding == true);

                if (stationOutInfos.Count <= 1 && m_IsProcessing == false)
                {
                    if (interactable.GetDataConsumable(m_OrderTypeIn).Item1)
                    {
                        gotNextFoodType = true;
                    }
                }

                foreach (var item in m_StationOutInfo)
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
                    OnDurationStart?.Invoke(m_DurationProperty);
                    CoroutineManager.LateAction(() =>
                    {
                        OnDurationComplete?.Invoke();
                        int count = 0;
                        foreach (var item in m_StationOutInfo)
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
    }
}
