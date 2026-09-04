using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.TaskSystem;
using Isometric.PathSystem;
using Isometric.Cam;
using Isometric.UI;
using Unity.VisualScripting;
using UnityEngine.Profiling;
using Isometric.Customer;

namespace Isometric.Environment
{
    public class StationEngageOrderOutOnCustomerDemand : MonoBehaviour
    {
        //---Setup---
        const string MetaSetupFoldOut = "---Setup---";
        [SerializeField, Foldout(MetaSetupFoldOut), Expandable] DataStation m_Data;
        [SerializeField, Foldout(MetaSetupFoldOut)] TaskTrigger m_TaskTrigger;
        [SerializeField, Foldout(MetaSetupFoldOut)] List<DataConsumable> m_OrderTypes;
        [SerializeField, Foldout(MetaSetupFoldOut)] List<MainServiceController> m_MainServiceControllers = new();
        [SerializeField, Foldout(MetaSetupFoldOut)] PathDirection m_EngageDirection;

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
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] float m_DurationProperty = 3;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] int m_CostProperty = 3;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] UnityEvent<int> OnDurationSetup;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] UnityEvent<float> OnDurationStart;

        //---Engege---
        const string MetaEngegeFoldOut = "---Engege---";
        [SerializeField, Foldout(MetaEngegeFoldOut)] UnityEvent OnEngegeSuccesful;
        [SerializeField, Foldout(MetaEngegeFoldOut)] UnityEvent OnOrderOutSuccesful;

        private Queue<DemandedCustomerInfo> DemandedCustomersQueue = new();


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
                m_Data.StationData.HasJustUnlocked = false;
                m_Data.Save();
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

            bool hasJustUnlocked = false;
            if (m_Data.StationData.HasJustUnlocked)
            {
                hasJustUnlocked = true;
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

                        ApplyUpgradeProperties();
                    }, m_CameraFocusDuration);
                });
                m_Data.StationData.HasJustUnlocked = false;
                m_Data.Save();
            }

            if (m_Data.StationData.HasUpgraded)
            {
                OnHasUpgradedGameplay?.Invoke();
                m_Data.StationData.HasUpgraded = false;
                m_Data.Save();
            }

            if (m_Data.StationData.IsUnlocked && !hasJustUnlocked)
                ApplyUpgradeProperties();
        }

        private void ApplyUpgradeProperties()
        {
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
            foreach(MainServiceController mainServiceController in m_MainServiceControllers)
            {
                mainServiceController.OnCustomerNewOrderBunchAsked += CheckDemandedCustomer;
            }
        }

        private void OnDisable()
        {
            m_TaskTrigger.OnTaskStart -= OnTaskStart;
            foreach(MainServiceController mainServiceController in m_MainServiceControllers)
            {
                mainServiceController.OnCustomerNewOrderBunchAsked -= CheckDemandedCustomer;
            }
        }

        private void CheckDemandedCustomer(CustomerSalonController customer, CustomerFirstOrderInfo firstOrder, List<DataConsumable> orderItems)
        {
            foreach(DataConsumable order in orderItems)
            {
                if(m_OrderTypes.Contains(order))
                {
                    AddDemandedCustomer(customer, firstOrder, order);
                }
            }
        }

        private void AddDemandedCustomer(CustomerSalonController customer, CustomerFirstOrderInfo firstOrder, DataConsumable orderItem)
        {
            DemandedCustomersQueue.Enqueue(new DemandedCustomerInfo
            {
                CustomerController = customer,
                // CustomerFirstOrderInfo  = firstOrder,
                CustomerOrderItem = orderItem
            });
        }
        private void RemoveDemandedCustomer()
        {
            DemandedCustomersQueue.Dequeue();
        }

        private bool HasAnyDemandedCustomer()
        {
            return DemandedCustomersQueue.Count > 0;
        }

        private void OnTaskStart(TaskTarget taskTarget)
        {
            if (taskTarget.TryGetComponent(out IEnvironmentInteractable interactable) && HasAnyDemandedCustomer())
            {
                DemandedCustomerInfo currentDemandedCustomerInfo = DemandedCustomersQueue.Peek();
                if (interactable.SendDataConsumable(currentDemandedCustomerInfo.CustomerOrderItem, m_CostProperty))
                {
                    interactable.EngageInteractable(m_EngageDirection);
                    OnDurationStart?.Invoke(m_DurationProperty);
                    OnEngegeSuccesful?.Invoke();
                    CoroutineManager.LateAction(() =>
                    {
                        OnOrderOutSuccesful?.Invoke();
                        RemoveDemandedCustomer();
                        m_TaskTrigger.SendTaskResult(TaskResult.Success);
                    }, m_DurationProperty);
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

        private class DemandedCustomerInfo
        {
            public CustomerSalonController CustomerController;
            // public CustomerFirstOrderInfo CustomerFirstOrderInfo;
            public DataConsumable CustomerOrderItem;
        }
    }
}
