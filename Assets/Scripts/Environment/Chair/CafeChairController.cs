using DG.Tweening;
using Isometric.Customer;
using Isometric.Data;
using Isometric.PathSystem;
using Isometric.TaskSystem;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Isometric.Environment
{
    public class CafeChairController : MonoBehaviour
    {
        [Serializable]
        private class CafeChairClothInfo
        {
            public DataConsumable OrderType;
            public Sprite CafeChairClothSprite;
            public Sprite OrderSymbolSprite;
        }

        private class CurrentOrderInfo
        {
            public DataConsumable Order;
            public bool HasBeenServed;
            public Vector3 UIOrderPosition;
        }

        [Header("---Setup---")]
        [SerializeField, Expandable] DataCafeChair m_Data;
        public PathNode ServeNode => m_ServeNode;
        [SerializeField] PathNode m_ServeNode;

        [SerializeField] TaskTrigger m_TaskTrigger;

        public UnityEvent OnIsLocked;
        [Header("-Unlocking for the first time")]
        public UnityEvent OnIsUnlocked;
        public UnityEvent OnHasJustUnlocked;

        public CafeChairCustomerHandler CustomerHandler => m_CustomerHandler;
        [SerializeField] CafeChairCustomerHandler m_CustomerHandler;
        [SerializeField] SpriteFillController m_SpriteFillController;
        [SerializeField] SpriteRenderer m_CafeChairCloth;
        [SerializeField] SpriteRenderer m_OrderSymbol;
        [SerializeField] List<CafeChairClothInfo> m_CafeChairClothsInfo;
        [SerializeField] PathDirection m_CleaningDirection;
        [SerializeField] List<DataConsumable> m_ChairLeaveOrders;
        private Transform m_CurrentHoldingOrder = null;

        [Header("---UI---")]
        [SerializeField] CafeChairOrderUIController m_OrderUIController;
        [SerializeField] EnvironmentRevenuUIController m_RevenuUIController;

        [Header("---Parameters---")]
        [SerializeField] float m_CleaningDuration = 3;

        [Header("---Per Order Delay---")]
        [SerializeField] float m_PerOrderDuration = 0;

        [Header("---Base Revenue---")]
        [SerializeField] int m_CafeBaseCost = 0;

        [Header("---Customer Sits---")]
        public UnityEvent OnCustomerSits;

        [Header("---Calls Worker---")]
        public UnityEvent OnCustomerCallsWorker;

        [Header("---Serve Start---")]
        public UnityEvent OnCustomerServeStart;

        [Header("---Customer Complete---")]
        public UnityEvent OnCustomerServeComplete;

        [Header("---Cafe Chair Clean Start---")]
        public UnityEvent OnCafeChairCleanStart;

        [Header("---Cafe Chair Clean Complete---")]
        public UnityEvent OnCafeChairCleanComplete;


        private int m_OrderIndexNumber = 0;
        private bool m_IsCustomerWaitingToBeServed = false;
        private bool m_IsPlayerOrdersLocked = false;
        private bool m_IsCustomerSitting = false;
        private bool m_IsCafeChairDirty = false;

        private bool m_IsCustomerVIP = false;
        private int m_PerOrderTipVIP = 0;
        private int m_LastTipVIP = 0;

        private int m_TotalCurrentOrders = 0;

        private int m_TotalRevenue = 0;

        private List<CurrentOrderInfo> m_CurrentOrdersInfo;
        [SerializeField, ReadOnly]
        private float m_WaitDuration = 0;

        public void SetupForMenu()
        {
            if (!m_Data.CafeChairData.IsUnlocked)
            {
                OnIsLocked?.Invoke();
            }
            else if (m_Data.CafeChairData.IsUnlocked && !m_Data.CafeChairData.HasJustUnlocked)
            {
                OnIsUnlocked?.Invoke();
            }

            if (m_Data.CafeChairData.HasJustUnlocked)
            {
                OnHasJustUnlocked?.Invoke();
                m_Data.CafeChairData.HasJustUnlocked = false;
                m_Data.Save();
            }

            m_SpriteFillController.SetFillAmount(0);
            m_IsPlayerOrdersLocked = false;

            m_TotalRevenue = 0;
        }

        public void SetupForGameplay()
        {
            if (!m_Data.CafeChairData.IsUnlocked)
            {
                OnIsLocked?.Invoke();
            }
            else if (m_Data.CafeChairData.IsUnlocked && !m_Data.CafeChairData.HasJustUnlocked)
            {
                OnIsUnlocked?.Invoke();
                m_CustomerHandler.SetupForGameplay();
            }

            if (m_Data.CafeChairData.HasJustUnlocked)
            {
                OnHasJustUnlocked?.Invoke();
                m_Data.CafeChairData.HasJustUnlocked = false;
                m_Data.Save();
                m_CustomerHandler.SetupForGameplay();
            }

            m_SpriteFillController.SetFillAmount(0);
            m_IsPlayerOrdersLocked = false;

            m_TotalRevenue = 0;
        }

        private void OnEnable()
        {
            m_TaskTrigger.OnTaskStart += OnTaskStart;
        }

        private void OnDisable()
        {
            m_TaskTrigger.OnTaskStart -= OnTaskStart;
        }

        //----Step 1---
        public void SitOnTheCafeChair()
        {
            if (CanSitOnTheCafeChair())
            {
                m_IsCustomerSitting = true;

                m_IsCustomerVIP = false;
                m_PerOrderTipVIP = 0;
                m_LastTipVIP = 0;

                m_IsPlayerOrdersLocked = false;

                m_OrderIndexNumber = 0;
                SetCafeChairCloth();

                CustomerWaitState customerWaitState = CustomerWaitState.Happy;
                if (m_CustomerHandler.GetCurrentCustomer() != null)
                {
                    customerWaitState = m_CustomerHandler.GetCurrentCustomer().GetWaitState();
                }

                CustomerCommonSetting commonSettings = m_CustomerHandler.GetCurrentCustomer().GetCommonSetting();
                int tipVaue = commonSettings.GetTip(customerWaitState);
                m_RevenuUIController.ShowTip(tipVaue, customerWaitState);

                if (customerWaitState == CustomerWaitState.Happy)
                {
                    GlobalEventHolder.OnCustomerSitHappy?.Invoke();
                }

                if (m_CustomerHandler.GetCurrentCustomer() != null)
                {
                    m_IsCustomerVIP = m_CustomerHandler.GetCurrentCustomer().IsCustomerVIP();
                }

                if (m_IsCustomerVIP)
                {
                    m_PerOrderTipVIP = commonSettings.GetPerOrderTipVIP();
                    m_LastTipVIP = commonSettings.GetLastOrderTipVIP();
                }

                GlobalEventHolder.OnCoinsCollected?.Invoke(tipVaue);


                OnCustomerSits?.Invoke();

                OnCustomerServeStart?.Invoke();

                List<CustomerOrderInfo> customerOrderInfo = m_CustomerHandler.GetCurrentCustomer().GetCafeOrders();
                if (customerOrderInfo != null)
                {
                    m_TotalCurrentOrders = customerOrderInfo.Count;
                }
                else
                {
                    m_TotalCurrentOrders = 0;
                }

                m_TotalRevenue += commonSettings.GetBaseRevenue();
                m_TotalRevenue += m_CafeBaseCost;

                List<CustomerOrderInfo> customerOrders = m_CustomerHandler.GetCurrentCustomer().GetCafeOrders();
                if(customerOrders != null && customerOrders.Count > 0)
                {
                    CoroutineManager.LateAction(() =>
                    {
                        ShowNextOrder();
                    }, 1);
                }
                else
                {
                    m_SpriteFillController.SetFillAmount(0);
                    m_SpriteFillController.StartFill(0, 1, 7);
                    CoroutineManager.LateAction(() =>
                    {
                        ShowNextOrder();
                    }, 7);
                }
            }
        }

        //----Step 4---
        public void ShowNextOrder()
        {
            if (m_CustomerHandler.GetCurrentCustomer() != null)
            {
                List<CustomerOrderInfo> customerOrders = m_CustomerHandler.GetCurrentCustomer().GetCafeOrders();
                if (customerOrders != null &&
                    m_OrderIndexNumber < customerOrders.Count)
                {
                    CustomerOrderInfo currentOrder = customerOrders[m_OrderIndexNumber];
                    m_CurrentOrdersInfo = new List<CurrentOrderInfo>();

                    foreach (var order in currentOrder.OrdersConsumable)
                    {
                        CurrentOrderInfo info = new CurrentOrderInfo();
                        info.Order = order;
                        info.HasBeenServed = false;
                        m_CurrentOrdersInfo.Add(info);
                    }

                    m_OrderUIController.CleanPreviousOrders();
                    List<Vector3> uiOrderPositions = m_OrderUIController.SetOrders(currentOrder.OrdersConsumable);

                    for (int i = 0; i < m_CurrentOrdersInfo.Count && i < uiOrderPositions.Count; i++)
                    {
                        m_CurrentOrdersInfo[i].UIOrderPosition = uiOrderPositions[i];
                    }


                    CheckChairLeaveOrder();

                    m_WaitDuration = m_PerOrderDuration;
                    m_OrderIndexNumber++;
                    m_IsCustomerWaitingToBeServed = true;
                }
                else
                {
                    ServeComplete();
                }
            }
        }

        //----Step 5---
        private void OnTaskStart(TaskTarget taskTarget)
        {
            if (taskTarget.TryGetComponent(out IEnvironmentInteractable interactable))
            {
                if (m_CustomerHandler.GetCurrentCustomer() != null
                    && m_IsCustomerWaitingToBeServed
                    && !m_IsPlayerOrdersLocked)
                {
                    List<DataConsumable> currentOrders = new List<DataConsumable>();
                    bool hasBeenServedSomething = false;
                    foreach (var currentOrder in m_CurrentOrdersInfo)
                    {
                        if (currentOrder.HasBeenServed == false)
                        {
                            var items = interactable.GetDataConsumable(currentOrder.Order);
                            if (items != null && items.Item1)
                            {
                                m_TotalRevenue += items.Item2;
                                currentOrder.HasBeenServed = true;
                                hasBeenServedSomething = true;
                            }
                            else
                            {
                                currentOrders.Add(currentOrder.Order);
                            }
                        }
                    }


                    if (m_CurrentOrdersInfo.TrueForAll((x) => x.HasBeenServed))
                    {
                        m_OrderUIController.CleanPreviousOrders();
                        m_IsCustomerWaitingToBeServed = false;

                        float fillFrom = m_TotalCurrentOrders == 0 ? 0 : (float)(m_OrderIndexNumber - 1) / (float)m_TotalCurrentOrders;
                        float fillTo = m_TotalCurrentOrders == 0 ? 0 : (float)(m_OrderIndexNumber) / (float)m_TotalCurrentOrders;

                        fillFrom = fillFrom < 0 ? 0 : fillFrom;

                        if (m_IsCustomerVIP)
                        {
                            m_RevenuUIController.ShowReveneu(m_PerOrderTipVIP);
                            GlobalEventHolder.OnCoinsCollected?.Invoke(m_PerOrderTipVIP);
                        }

                        m_SpriteFillController.StartFill(fillFrom, fillTo, m_WaitDuration);

                        GlobalEventHolder.OnCustomerOrderServed?.Invoke();
                        CoroutineManager.LateAction(ShowNextOrder, m_WaitDuration);

                        m_TaskTrigger.SendTaskResult(TaskResult.Success);
                    }
                    else if (hasBeenServedSomething == true)
                    {
                        m_OrderUIController.CleanPreviousOrders();
                        List<Vector3> uiOrderPositions = m_OrderUIController.SetOrders(currentOrders);

                        int uiOrderCount = 0;
                        for (int i = 0; i < m_CurrentOrdersInfo.Count && uiOrderCount < uiOrderPositions.Count; i++)
                        {
                            if (!m_CurrentOrdersInfo[i].HasBeenServed)
                            {
                                m_CurrentOrdersInfo[i].UIOrderPosition = uiOrderPositions[uiOrderCount];
                                uiOrderCount++;
                            }
                        }

                        CheckChairLeaveOrder();

                        m_TaskTrigger.SendTaskResult(TaskResult.Success);
                    }
                    else
                    {
                        foreach (var currentOrder in m_CurrentOrdersInfo)
                        {
                            if (currentOrder.HasBeenServed == false)
                            {
                                GlobalEventHolder.OnHintByConsumable?.Invoke(currentOrder.Order);
                            }
                        }
                        m_TaskTrigger.SendTaskResult(TaskResult.Failed);
                    }

                }
                else if (m_IsCafeChairDirty)
                {
                    OnCafeChairCleanStart?.Invoke();
                    interactable.EngageInteractable(m_CleaningDirection);
                    CoroutineManager.LateAction(() =>
                    {
                        m_SpriteFillController.DOKill();
                        m_SpriteFillController.SetFillAmount(0);
                        m_IsCafeChairDirty = false;
                        OnCafeChairCleanComplete?.Invoke();
                        m_TaskTrigger.SendTaskResult(TaskResult.Success);
                    }, m_CleaningDuration);
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


        //----Step 6---
        private void ServeComplete()
        {
            if (m_CurrentHoldingOrder != null)
            {
                m_CurrentHoldingOrder.DOKill();
                Destroy(m_CurrentHoldingOrder.gameObject);
            }

            if (m_IsCustomerVIP)
            {
                m_TotalRevenue += m_LastTipVIP;
            }

            m_RevenuUIController.ShowReveneu(m_TotalRevenue);

            GlobalEventHolder.OnCoinsCollected?.Invoke(m_TotalRevenue);

            GlobalEventHolder.OnCustomerServed?.Invoke();

            m_TotalRevenue = 0;
            m_IsCustomerSitting = false;

            OnCustomerServeComplete?.Invoke();
            m_CustomerHandler.CustomerLeaves();
            m_IsCafeChairDirty = true;
        }


        //----Step 7---
        private void SetCafeChairCloth()
        {

        }


        private void CheckChairLeaveOrder()
        {
            if (m_CurrentOrdersInfo != null)
            {
                DataConsumable currentLeaveOrder = null;
                foreach (var currentOrderInfo in m_CurrentOrdersInfo)
                {
                    if (currentOrderInfo.HasBeenServed == false &&
                        m_ChairLeaveOrders.Contains(currentOrderInfo.Order))
                    {
                        currentLeaveOrder = currentOrderInfo.Order;
                    }
                }

                if (currentLeaveOrder != null)
                {
                    if (m_CustomerHandler.GetCurrentCustomer() != null)
                    {
                        m_CustomerHandler.GetCurrentCustomer().SetCustomerForChairLeaveOrder(currentLeaveOrder);
                    }
                }
            }
        }

        public bool CanSitOnTheCafeChair()
        {
            return m_IsCafeChairDirty == false && m_IsCustomerSitting == false;
        }

        #region Helper
        public void LockPlayerOrders()
        {
            m_IsPlayerOrdersLocked = true;
        }

        public void UnlockPlayerOrders()
        {
            m_IsPlayerOrdersLocked = false;
        }

        public void CleanPreviousOrders()
        {
            m_OrderUIController.CleanPreviousOrders();
        }

        public void AddToRevenue(int revenue)
        {
            m_TotalRevenue += revenue;
        }

        //Send orders with costs
        public void FillCurrentPlayerOrders(List<Tuple<DataConsumable, int>> nonWorkerOrders)
        {
            if (m_CurrentOrdersInfo != null && nonWorkerOrders != null)
            {
                List<DataConsumable> currentOrders = new List<DataConsumable>();
                bool hasBeenServedSomething = false;
                foreach (var currentOrder in m_CurrentOrdersInfo)
                {
                    if (currentOrder.HasBeenServed == false)
                    {
                        var item = nonWorkerOrders.Find((x) => x.Item1 == currentOrder.Order);
                        if (item != null)
                        {
                            m_TotalRevenue += item.Item2;
                            currentOrder.HasBeenServed = true;
                        }
                        else
                        {
                            currentOrders.Add(currentOrder.Order);
                        }
                    }
                }

                if (m_CurrentOrdersInfo.TrueForAll((x) => x.HasBeenServed))
                {
                    m_OrderUIController.CleanPreviousOrders();
                    m_IsCustomerWaitingToBeServed = false;

                    float fillFrom = m_TotalCurrentOrders == 0 ? 0 : (float)(m_OrderIndexNumber - 1) / (float)m_TotalCurrentOrders;
                    float fillTo = m_TotalCurrentOrders == 0 ? 0 : (float)(m_OrderIndexNumber) / (float)m_TotalCurrentOrders;

                    fillFrom = fillFrom < 0 ? 0 : fillFrom;

                    if (m_IsCustomerVIP)
                    {
                        m_RevenuUIController.ShowReveneu(m_PerOrderTipVIP);
                        GlobalEventHolder.OnCoinsCollected?.Invoke(m_PerOrderTipVIP);
                    }

                    m_SpriteFillController.StartFill(fillFrom, fillTo, m_WaitDuration);
                    CoroutineManager.LateAction(ShowNextOrder, m_WaitDuration);
                }
                else if (hasBeenServedSomething == true)
                {
                    m_OrderUIController.CleanPreviousOrders();
                    List<Vector3> uiOrderPositions = m_OrderUIController.SetOrders(currentOrders);

                    int uiOrderCount = 0;
                    for (int i = 0; i < m_CurrentOrdersInfo.Count && uiOrderCount < uiOrderPositions.Count; i++)
                    {
                        if (!m_CurrentOrdersInfo[i].HasBeenServed)
                        {
                            m_CurrentOrdersInfo[i].UIOrderPosition = uiOrderPositions[uiOrderCount];
                            uiOrderCount++;
                        }
                    }
                }
            }
        }

        // Returns the Order Type and UI Order Position
        public List<Tuple<DataConsumable, Vector3>> GetCurrentPlayerOrders()
        {
            if (m_CurrentOrdersInfo != null)
            {
                List<Tuple<DataConsumable, Vector3>> ordersInfo = new List<Tuple<DataConsumable, Vector3>>();

                foreach (var orderInfo in m_CurrentOrdersInfo)
                {
                    if (!orderInfo.HasBeenServed)
                    {
                        Tuple<DataConsumable, Vector3> temOrderInfo = new Tuple<DataConsumable, Vector3>(
                            orderInfo.Order,
                            orderInfo.UIOrderPosition
                            );
                        ordersInfo.Add(temOrderInfo);
                    }
                }

                return ordersInfo;
            }

            return null;
        }

        #endregion
    }
}
