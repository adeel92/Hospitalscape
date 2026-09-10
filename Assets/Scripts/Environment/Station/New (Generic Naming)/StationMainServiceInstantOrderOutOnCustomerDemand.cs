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

        //---Menu Calls---
        private const string MetaMenuCallsFoldOut = "---Menu Calls---";
        [Header("-Station is locked"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsLockedMenu;
        [Header("-Station is unlocked (Not CALLED FIRST TIME)"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsUnlockdMenu;
        /* [Header("-Unlocking for the first time")]
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] Vector2 m_CameraFocusPosition;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraZoom;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraFocusDuration;*/
        [Foldout(MetaMenuCallsFoldOut)] public UnityEvent OnHasUnlockedMenu; 
        [Header("-Upgraded any of the properties"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnHasUpgradedMenu;

        //---Gameplay Calls---
        private const string MetaGameplayCallsFoldOut = "---Gameplay Calls---";
        [Header("-Station is locked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsLockedGameplay;
        [Header("-Station is unlocked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsUnlockdGameplay;
        [Header("-Unlocking for the first time")]
        [Foldout(MetaGameplayCallsFoldOut)] public UnityEvent OnHasUnlockedGameplay;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut)] float m_DelayAfterHasUnlocked = 1f;
        [Header("-Upgraded any of the properties"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnHasUpgradedGameplay;

        //---Upgrade Properties---
        const string MetaUpgradePropertiesFoldOut = "---Upgrade Properties---";
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] int m_CostProperty;

        //---Interaction
        private const string MetaInteractionFoldOut = "---Interaction---";
        [Foldout(MetaInteractionFoldOut)] public UnityEvent OnStationOpen;
        [Foldout(MetaInteractionFoldOut)] public UnityEvent OnStationClose;
        [Foldout(MetaInteractionFoldOut)] public UnityEvent OnFoodOutSuccesful;

        private CustomerSalonController m_CurrentCustomer = null;
        private CustomerFirstOrderInfo m_CurrentCustomerFirstOrderInfo = null;
        [SerializeField, ReadOnly] List<OutputOrderInfo> m_CurrentOutputOrderInfos = null;
        private List<GameObject> m_CurrentDisplayedOutputOrders = new();


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

            if (m_Data.StationData.HasJustUnlocked)
            {
                OnHasUnlockedMenu?.Invoke();
                m_Data.StationData.HasJustUnlocked = false;
                m_Data.Save();
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
                OnHasUnlockedGameplay?.Invoke();
                m_Data.StationData.HasJustUnlocked = false;
                m_Data.Save();
                CoroutineManager.LateAction(() =>
                {
                    UIManager.CheckNextGameplayUpdatable();
                }, m_DelayAfterHasUnlocked);
            }

            if (m_Data.StationData.HasUpgraded)
            {
                OnHasUpgradedGameplay?.Invoke();
                m_Data.StationData.HasUpgraded = false;
                m_Data.Save();
            }

            StationUpgrade upgradeCost = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
            if (upgradeCost != null)
            {
                m_CostProperty = Mathf.RoundToInt(upgradeCost.Upgrade[upgradeCost.CurrentUpgradeIndex]);
            }
            else
            {
                m_CostProperty = 0;
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
            // Debug.Log($"{nameof(StationMainServiceInstantOrderOutOnCustomerDemand)} -- Adeel 1!");
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
            // m_TaskTrigger.enabled = true;
            ShowCustomerDemandedOrdersUI();
            DisplayOutputOrders();
            OnStationOpen?.Invoke();
        }
        private void CloseStation()
        {
            m_IsStationOpen = false;
            // m_TaskTrigger.enabled = false;
            HideCustomerDemandedOrdersUI();
            RemoveDisplayedOutputOrders();
            OnStationClose?.Invoke();
        }

        private void ShowCustomerDemandedOrdersUI()
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
        private void HideCustomerDemandedOrdersUI()
        {
            m_OrderUIController.CleanPreviousOrders();
        }

        private void DisplayOutputOrders()
        {
            RemoveDisplayedOutputOrders();

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
                            m_CurrentDisplayedOutputOrders.Add(displayObj);
                            break;
                        }
                    }
                }
            }
        }
        private void RemoveDisplayedOutputOrders()
        {
            if(m_CurrentDisplayedOutputOrders.Count > 0)
            {
                foreach(GameObject displayObj in m_CurrentDisplayedOutputOrders)
                {
                    displayObj.SetActive(false);
                }
                m_CurrentDisplayedOutputOrders.Clear();
            }
        }

        private bool HasAnyDemandedCustomer()
        {
            return m_CurrentOutputOrderInfos.Count > 0;
        }

        private void OnTaskStart(TaskTarget taskTarget)
        {
            if (taskTarget.TryGetComponent(out IEnvironmentInteractable interactable) && HasAnyDemandedCustomer())
            {
                bool isOutputSuccessfullySent = false;
                for(int i = m_CurrentOutputOrderInfos.Count - 1; i >= 0; i--)
                {
                    OutputOrderInfo outputOrderInfo = m_CurrentOutputOrderInfos[i];
                    if (interactable.SendDataConsumable(outputOrderInfo.OutputOrderType, m_CostProperty))
                    {
                        OnFoodOutSuccesful?.Invoke();
                        isOutputSuccessfullySent = true;
                        m_CurrentOutputOrderInfos.RemoveAt(i);
                    }
                }

                if (HasAnyDemandedCustomer())
                {
                    DisplayOutputOrders();
                    ShowCustomerDemandedOrdersUI();
                }
                else
                {
                    CloseStation();
                }


                if (isOutputSuccessfullySent)
                {
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
