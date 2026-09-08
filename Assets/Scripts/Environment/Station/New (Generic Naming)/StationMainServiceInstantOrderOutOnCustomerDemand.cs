using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.TaskSystem;
using Isometric.Cam;
using Isometric.UI;
using System;
using Isometric.Customer;

namespace Isometric.Environment
{
    public class StationMainServiceInstantOrderOutOnCustomerDemand : MonoBehaviour
    {
        //---Setup---
        private const string MetaSetupFoldOut = "---Setup---";
        [SerializeField, Foldout(MetaSetupFoldOut), Expandable] DataStation m_Data;
        [SerializeField, Foldout(MetaSetupFoldOut)] MainServiceController m_MainServiceController;
        [SerializeField, Foldout(MetaSetupFoldOut)] MainServiceOrderUIController m_OrderUIController;
        [SerializeField, Foldout(MetaSetupFoldOut)] TaskTrigger m_TaskTrigger;
        [SerializeField, Foldout(MetaSetupFoldOut)] List<OutputOrderInfo> m_OutputOrderInfos = new();
        [SerializeField, Foldout(MetaSetupFoldOut)] List<OutputOrderDisplayInfo> m_OutputOrderDisplayInfos = new();
        [SerializeField, Foldout(MetaSetupFoldOut), ReadOnly] bool m_IsStationOpen = false;
        [SerializeField, Foldout(MetaSetupFoldOut)] DataConsumable m_FoodType;
        [SerializeField, Foldout(MetaSetupFoldOut)] bool m_SetHasJustUnlockedValue;

        //---Menu Calls---
        private const string MetaMenuCallsFoldOut = "---Menu Calls---";
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
        private const string MetaGameplayCallsFoldOut = "---Gameplay Calls---";
        [Header("-Station is locked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsLockedGameplay;
        [Header("-Station is unlocked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsUnlockdGameplay;
        [Header("-Unlocking for the first time")]
        [SerializeField, Foldout(MetaGameplayCallsFoldOut)] Vector2 m_CameraFocusPosition;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut)] float m_CameraZoom;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut)] float m_CameraFocusDuration;
        [Foldout(MetaGameplayCallsFoldOut)] public UnityEvent OnHasUnlockedGameplay;
        [Header("-Upgraded any of the properties"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnHasUpgradedGameplay;

        //---Upgrade Properties---
        const string MetaUpgradePropertiesFoldOut = "---Upgrade Properties---";
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] int m_CostProperty;

        //---Interaction
        private const string MetaInteractionFoldOut = "---Interaction---";
        [Foldout(MetaInteractionFoldOut)] 
        public UnityEvent OnFoodOutSuccesful;

        private CustomerSalonController m_CurrentCustomer = null;
        private CustomerFirstOrderInfo m_CurrentCustomerFirstOrderInfo = null;
        private List<OutputOrderInfo> m_CurrentOutputOrderInfos = null;

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

            /* if (m_Data.StationData.HasJustUnlocked)
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

                if (m_SetHasJustUnlockedValue)
                {
                    m_Data.StationData.HasJustUnlocked = false;
                    m_Data.Save();
                }
            } */

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

                    }, m_CameraFocusDuration);
                });

                if (m_SetHasJustUnlockedValue)
                {
                    m_Data.StationData.HasJustUnlocked = false;
                    m_Data.Save();
                }
            }

            if (m_Data.StationData.HasUpgraded)
            {
                OnHasUpgradedMenu?.Invoke();
                m_Data.StationData.HasUpgraded = false;
                m_Data.Save();
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
            m_MainServiceController.OnCustomerNewOrderBunchAsked += CheckDemandedCustomer;
        }

        private void OnDisable()
        {
            m_TaskTrigger.OnTaskStart -= OnTaskStart;
            m_MainServiceController.OnCustomerNewOrderBunchAsked -= CheckDemandedCustomer;
        }

        private void CheckDemandedCustomer(CustomerSalonController customer, CustomerFirstOrderInfo firstOrder, List<DataConsumable> orderItems)
        {
            m_CurrentCustomer = customer;
            m_CurrentCustomerFirstOrderInfo = firstOrder;
            
            foreach(DataConsumable order in orderItems)
            {
                OutputOrderInfo outputOrderInfo = m_OutputOrderInfos.Find(x => x.CustomerDemandedOrderType == order);
                if(outputOrderInfo != null)
                {
                    m_CurrentOutputOrderInfos.Add(outputOrderInfo);
                }
            }

            if(m_CurrentOutputOrderInfos.Count > 0)
            {
                OpenStation();
            }
        }

        private void OpenStation()
        {
            m_IsStationOpen = true;
            m_TaskTrigger.enabled = true;
            ShowCustomerDemandedOrders();
            DisplayOutputOrders();
        }
        private void CloseStation()
        {
            m_IsStationOpen = false;
            m_TaskTrigger.enabled = false;
        }

        private void ShowCustomerDemandedOrders()
        {
            if (m_CurrentCustomer != null)
            {
                List<DataConsumable> currentOrders = new();
                foreach(OutputOrderInfo outputOrderInfo in m_CurrentOutputOrderInfos)
                {
                    currentOrders.Add(outputOrderInfo.CustomerDemandedOrderType);
                }

                m_OrderUIController.CleanPreviousOrders();
                m_OrderUIController.SetOrders(currentOrders, m_CurrentCustomerFirstOrderInfo);
            }
        }

        private void DisplayOutputOrders()
        {
            foreach(OutputOrderInfo outputOrderInfo in m_CurrentOutputOrderInfos)
            {
                OutputOrderDisplayInfo outputOrderDisplayInfo = m_OutputOrderDisplayInfos.Find(x => x.OutputOrderType == outputOrderInfo.OutputOrderType);
                if(outputOrderDisplayInfo != null)
                {
                    foreach(GameObject displayObj in outputOrderDisplayInfo.OutputOrderDisplayObjs)
                    {
                        if (!displayObj.activeSelf)
                        {
                            displayObj.SetActive(true);
                            break;
                        }
                    }
                }
            }
        }

        private void OnTaskStart(TaskTarget taskTarget)
        {
            if (taskTarget.TryGetComponent(out IEnvironmentInteractable interactable))
            {
                if (interactable.SendDataConsumable(m_FoodType, m_CostProperty))
                {
                    OnFoodOutSuccesful?.Invoke();
                    m_TaskTrigger.SendTaskResult(TaskResult.Success);
                }
                else
                {
                    m_TaskTrigger.SendTaskResult(TaskResult.Failed);
                }
            }
            else
            {
                m_TaskTrigger.SendTaskResult(TaskResult.Failed);
            }
        }


        [Serializable]
        public class OutputOrderInfo
        {
            public DataConsumable CustomerDemandedOrderType;
            public DataConsumable OutputOrderType;
        }

        [Serializable]
        public class OutputOrderDisplayInfo
        {
            public DataConsumable OutputOrderType;
            public List<GameObject> OutputOrderDisplayObjs = new();
        }
    }
}
