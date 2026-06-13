using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.PathSystem;
using Isometric.Environment;

namespace Isometric.Customer
{
    public class CustomerCafeController : CustomerBaseController
    {
        [Header("---Setup---")]
        [SerializeField] CustomerAnimatorController m_AnimatorController;
        [SerializeField] List<SortingGroup> m_SortingGroups;

        [Header("---UI---")]
        [SerializeField] CustomerWaitingUIController m_WaitingUIController;

        [Header("---Events---")]
        public UnityEvent OnAngryState;
        public UnityEvent OnFuriousState;
        public UnityEvent OnHappySurvedState;

        [Header("---PickedUp---")]
        [SerializeField] GameObject m_PickUp;
        private bool m_IsPickedUp = false;
        [SerializeField, SortingLayer] string m_PickedUpSortingLayer;
        [SerializeField] float m_PickedUpZAxis = 0;
        [SerializeField] float m_SittingDragMoveMinDisance = 1;
        private Vector3 m_PickedLastPosition;

        private CafeChairCustomerHandler m_CafeChairHandler = null;
        private StationCustomerInHandler m_StationCusterInHandler = null;
        private CounterTableController m_CounterTableController = null;


        [Header("---Loaded Externally---")]
        [SerializeField, ReadOnly] bool m_IsCustomerVIP;
        [SerializeField, ReadOnly] CustomerData m_CustomerData;
        [SerializeField, ReadOnly, Expandable] CustomerCommonSetting m_CommonSettings;
        [SerializeField, ReadOnly] List<CustomerOrderInfo> m_CustomerCafeOrdersInfo;
        [SerializeField, ReadOnly] PathNode m_CurrentNode;
        [SerializeField, ReadOnly] QueueInfo m_CurrentQueue;
        [SerializeField, ReadOnly] DataConsumable m_CurrentChairLeaveOrder = null;

        private float m_WalkSpeed;
        private float m_WaitDurationCounter = 0;
        private CustomerAnimatorState m_CafeWaitState = CustomerAnimatorState.PickedUpNeutral;
        private CustomerWaitState m_WaitState;
        private Coroutine m_CafeWaitCorotoine;
        private bool m_HasEntered = false;
        [SerializeField, ReadOnly]
        private bool m_IsOnCafeChair = false;
        [SerializeField, ReadOnly]
        private bool m_IsFirstOrderUndecided = false;
        private bool m_IsTimeFrozeBoosterActivated = false;
        private bool m_WaitFrozen = false;
        private bool m_IsAboutToLeaveUnserved = false;
        private bool m_IsOnPatienceChair = false;

        public void Setup(CustomerData customerData, PathNode currentNode, QueueInfo queueInfo, float walkSpeed)
        {
            m_CustomerData = customerData;
            m_CommonSettings = m_CustomerData.CustomerCommonSettings;
            m_CurrentNode = currentNode;
            m_CurrentQueue = queueInfo;
            m_CurrentQueue.CurrentCustomer = this;
            m_WalkSpeed = walkSpeed;
            m_IsFirstOrderUndecided = customerData.IsFirstOrderUndecided;

            m_WaitState = CustomerWaitState.Happy;

            CustomerData data = customerData;
            m_IsCustomerVIP = data.IsCustomerVIP;

            m_WaitingUIController.SetupForCafe(m_IsFirstOrderUndecided);

            if (CustomerManager.IsTimeFrozeBoosterActivated)
            {
                m_IsTimeFrozeBoosterActivated = true;
                m_WaitingUIController.PlayTimeFrozeEffect();

            }
            else
            {
                m_IsTimeFrozeBoosterActivated = false;
                m_WaitingUIController.StopTimeFrozeEffect();
            }

            m_WaitFrozen = CustomerManager.IsCustomerWaitFrozen;

            if (CustomerPatienceManager.IsSunRaysActivated())
            {
                m_WaitingUIController.PlayHeatEffect();

            }
            else
            {
                m_WaitingUIController.StopHeatEffect();
            }

            if (data.CustomerOrderBundles.Count > 0)
            {
                int ordersIndex = UnityEngine.Random.Range(0, data.CustomerOrderBundles.Count);
                m_CustomerCafeOrdersInfo = data.CustomerOrderBundles[ordersIndex].CustomerOrdersInfo;
            }
            else
            {
                m_CustomerCafeOrdersInfo = null;
            }

            Enter();
        }

        private void OnEnable()
        {
            GlobalEventHolder.OnTimeFrozeBooster += OnTimeFrozeBooster;
            GlobalEventHolder.OnPatienceSunRays += OnPatienceSunRays;
            GlobalEventHolder.OnCustomerWaitFreeze += OnCustomerWaitFreeze;

        }

        private void OnDisable()
        {
            GlobalEventHolder.OnTimeFrozeBooster -= OnTimeFrozeBooster;
            GlobalEventHolder.OnPatienceSunRays -= OnPatienceSunRays;
            GlobalEventHolder.OnCustomerWaitFreeze -= OnCustomerWaitFreeze;
        }

        #region Enter
        private void Enter()
        {
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, m_CurrentQueue.Node, m_WalkSpeed, OnGoingToNodeEntering, OnReachedQueueNodeEntered);
        }

        private void OnGoingToNodeEntering(PathNode node1, PathNode node2)
        {
            m_CurrentNode = node2;

            if (node1.TryGetComponent(out PathNodeDirection nodeDirection))
            {
                PathDirection direction = nodeDirection.GetDirection(node2);
                m_AnimatorController.SetWalkSpeed(m_WalkSpeed);
                PlayWalkAnimation(direction);
            }

            if (node2.TryGetComponent(out SortingGroup sortingGroup))
            {
                SetSortingLayer(sortingGroup.sortingLayerName, sortingGroup.sortingOrder);
            }
        }

        private void OnReachedQueueNodeEntered(PathNode node)
        {
            GlobalEventHolder.OnCustomerEntered?.Invoke();

            m_CurrentNode = node;

            if (node.TryGetComponent(out PatienceChairCustomerHandler handler))
            {
                m_IsOnPatienceChair = true;
                m_PickUp.SetActive(true);
                m_CafeWaitState = CustomerAnimatorState.SittingIdleNeurtalDown;
                m_AnimatorController.PlayState(CustomerAnimatorState.SittingDown, () =>
                {
                    m_AnimatorController.PlayState(CustomerAnimatorState.SittingIdleNeurtalDown);
                });
                m_CafeWaitCorotoine = StartCoroutine(CafeWaitCorotine());
            }
            else
            {
                m_AnimatorController.PlayState(CustomerAnimatorState.StandingIdleNeutral);
                m_IsOnPatienceChair = false;
                m_PickUp.SetActive(true);
                m_CafeWaitCorotoine = StartCoroutine(CafeWaitCorotine());
            }

            m_HasEntered = true;
        }
        #endregion

        #region Waiting Customer Cafe
        IEnumerator CafeWaitCorotine()
        {
            CustomerData customerData = m_CustomerData;

            float waitDuration = m_CommonSettings.WaitingDuration + CustomerPatienceManager.GetExtraPatienace();
            m_WaitDurationCounter = waitDuration;

            m_WaitingUIController.SetActiveState(true);
            m_CafeWaitState = m_IsOnPatienceChair == false ? CustomerAnimatorState.StandingIdleNeutral : CustomerAnimatorState.SittingIdleNeurtalDown;
            m_WaitingUIController.SetWaitingColor(m_CommonSettings.DefaultColor());

            while (m_WaitDurationCounter > 0)
            {
                m_WaitingUIController.SetWaitingFill(m_WaitDurationCounter / waitDuration);

                if (!m_IsTimeFrozeBoosterActivated && !m_WaitFrozen)
                {
                    if (m_IsOnPatienceChair)
                    {
                        m_WaitDurationCounter -= (Time.deltaTime * CustomerPatienceManager.GetSofaPatienaceCoolDown() * CustomerPatienceManager.GetSunRaysPatienaceCoolDown());
                    }
                    else
                    {
                        m_WaitDurationCounter -= (Time.deltaTime * CustomerPatienceManager.GetSunRaysPatienaceCoolDown());
                    }
                }
                yield return null;

                if (m_WaitState == CustomerWaitState.Happy && m_CommonSettings.IsFirstWaitOver(m_WaitDurationCounter, waitDuration))
                {
                    m_WaitingUIController.SetWaitingColor(m_CommonSettings.FirstWaitColor());
                    m_CafeWaitState = m_IsOnPatienceChair == false ? CustomerAnimatorState.StandingIdleAngry : CustomerAnimatorState.SittingIdleAngryDown;
                    PlayCafeAnimationState();
                    m_WaitState = CustomerWaitState.Angry;
                    InvokeAngryStateEvent();
                }

                if (m_WaitState == CustomerWaitState.Angry && m_CommonSettings.IsSecondWaitOver(m_WaitDurationCounter, waitDuration))
                {
                    m_WaitingUIController.SetWaitingColor(m_CommonSettings.SecondWaitColor());
                    m_WaitingUIController.StartVibrating();
                    m_CafeWaitState = m_IsOnPatienceChair == false ? CustomerAnimatorState.StandingIdleFurious : CustomerAnimatorState.SittingIdleFuriousDown;
                    PlayCafeAnimationState();
                    m_WaitState = CustomerWaitState.Furious;
                    InvokeFuriousStateEvent();
                }
            }

            yield return new WaitWhile(() => m_IsPickedUp == true);

            if (m_CafeChairHandler == null)
            {
                m_WaitingUIController.StopVibrating();
                m_WaitingUIController.SetActiveState(false);
                LeaveUnservedCafeCustomer();
            }
        }

        private void StopCafeWaitCorotoine()
        {
            m_WaitingUIController.StopVibrating();
            m_WaitingUIController.SetActiveState(false);
            if (m_CafeWaitCorotoine != null)
            {
                StopCoroutine(m_CafeWaitCorotoine);
                m_CafeWaitCorotoine = null;
            }
        }
        #endregion

        #region Picked
        public void OnDragBegin()
        {
            m_IsPickedUp = true;
            SetSortingLayer(m_PickedUpSortingLayer, 0);
            if (m_IsOnCafeChair)
            {
                transform.SetParent(CustomerManager.ParentTransfomr);
            }
            PlayCafeAnimationState();
        }

        public void OnDrag()
        {
            Vector3 pickedPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pickedPosition.z = m_PickedUpZAxis;

            if (!m_IsOnCafeChair)
            {
                if(!m_IsFirstOrderUndecided)
                {
                    if (m_CafeChairHandler != null
                        && Vector3.Distance(pickedPosition, m_CafeChairHandler.transform.position) > m_CafeChairHandler.GetExitDistance())
                    {
                        m_CafeChairHandler.CustomerDragedRemoved();
                        m_CafeChairHandler = null;
                        transform.SetParent(CustomerManager.ParentTransfomr);
                        transform.position = pickedPosition;
                        OnDragBegin();
                    }
                    else if (m_CafeChairHandler == null)
                    {
                        transform.position = pickedPosition;
                    }
                }
                else
                {
                    if (m_CounterTableController != null
                        && Vector3.Distance(pickedPosition, m_CounterTableController.GetStandingPathNode().transform.position) > m_CounterTableController.GetExitDistance())
                    {
                        m_CounterTableController.CustomerRemoved();
                        m_CounterTableController = null;
                        transform.SetParent(CustomerManager.ParentTransfomr);
                        transform.position = pickedPosition;
                        OnDragBegin();
                    }
                    else if (m_CounterTableController == null)
                    {
                        transform.position = pickedPosition;
                    }
                }
            }
            else
            {
                if (m_StationCusterInHandler != null
                    && Vector3.Distance(pickedPosition, m_StationCusterInHandler.transform.position) > m_StationCusterInHandler.GetExitDistance())
                {
                    m_StationCusterInHandler.RemoveCustomer();
                    m_StationCusterInHandler = null;
                    transform.SetParent(CustomerManager.ParentTransfomr);
                    transform.position = pickedPosition;
                    OnDragBegin();
                }
                else if (m_StationCusterInHandler == null)
                {
                    transform.position = pickedPosition;
                }
            }
        }

        public void OnDragEnd()
        {
            m_IsPickedUp = false;
            if (!m_IsOnCafeChair)
            {
                if (!m_IsFirstOrderUndecided)
                {
                    if (m_CafeChairHandler == null)
                    {
                        if(m_CounterTableController == null)
                        {
                            transform.position = m_CurrentNode.transform.position;
                            PlayCafeAnimationState();

                            if (m_CurrentNode.TryGetComponent(out SortingGroup group))
                            {
                                SetSortingLayer(group.sortingLayerName, group.sortingOrder);
                            }
                            CustomerManager.ResetQueue();
                        }
                        else
                        {
                            PlayCafeAnimationState();
                            m_CounterTableController.ReStandAtTheCounter();
                        }
                    }
                    else if (m_CafeChairHandler != null)
                    {
                        m_IsOnCafeChair = true;
                        m_PickUp.SetActive(false);
                        StopCafeWaitCorotoine();

                        if (m_CounterTableController == null)
                        {
                            m_CurrentQueue.CurrentCustomer = null;
                            CustomerManager.ResetQueue();
                            m_CurrentNode = null;
                        }
                        else
                        {
                            m_CounterTableController.CustomerRemoved();
                        }

                        m_CafeChairHandler.SitOnTheCafeChair();
                    }
                }
                else
                {
                    if (m_CounterTableController == null)
                    {
                        transform.position = m_CurrentNode.transform.position;
                        PlayCafeAnimationState();

                        if (m_CurrentNode.TryGetComponent(out SortingGroup group))
                        {
                            SetSortingLayer(group.sortingLayerName, group.sortingOrder);
                        }

                        CustomerManager.ResetQueue();
                    }
                    else if (m_CounterTableController != null)
                    {
                        m_PickUp.SetActive(false);
                        m_CurrentQueue.CurrentCustomer = null;
                        CustomerManager.ResetQueue();
                        m_CurrentNode = m_CounterTableController.GetStandingPathNode();
                        StopCafeWaitCorotoine();

                        m_CounterTableController.StandAtTheCounter(() =>
                        {
                            m_IsFirstOrderUndecided = false;
                            m_WaitingUIController.SetupForCafe(m_IsFirstOrderUndecided);
                            m_AnimatorController.PlayState(CustomerAnimatorState.StandingIdleNeutral);
                            m_PickUp.SetActive(true);
                            m_CafeWaitCorotoine = StartCoroutine(CafeWaitCorotine());
                        });
                    }
                }
            }
            else
            {
                if (m_StationCusterInHandler == null)
                {
                    m_CafeChairHandler.ResitOnCafeChair();
                    SetCafeAnimationStateSitting();
                }
                else if (m_StationCusterInHandler != null)
                {
                    m_CafeChairHandler.GetCafeChair().CleanPreviousOrders();
                    m_StationCusterInHandler.CustomerEnters();
                    m_PickUp.SetActive(false);
                }
            }
        }

        #endregion

        #region Sitting
        public void SitOnCafeChair(CafeChairCustomerHandler cafeChairHandler)
        {
            if (!m_IsOnCafeChair)
            {
                m_CafeChairHandler = cafeChairHandler;
                m_PickedLastPosition = transform.position;
                m_AnimatorController.PlayState(CustomerAnimatorState.SititngRight, () =>
                {
                    m_AnimatorController.PlayState(CustomerAnimatorState.SittingDrinkingRight);
                });
            }
        }
        #endregion

        #region  Counter
        public void StandAtTheCounter(CounterTableController counterTableController)
        {
            if (!m_IsOnCafeChair && m_IsFirstOrderUndecided)
            {
                m_CounterTableController = counterTableController;
                m_PickedLastPosition = transform.position;
                m_AnimatorController.PlayState(CustomerAnimatorState.StandingIdleNeutral);
            }
        }
        #endregion

        #region Leave Order
        public void SetCustomerForChairLeaveOrder(DataConsumable chairLeaveOrder)
        {
            m_CurrentChairLeaveOrder = chairLeaveOrder;
            m_PickUp.SetActive(true);
        }

        public void SetForCustomerInHandler(StationCustomerInHandler stationCustomerInHandler)
        {
            m_StationCusterInHandler = stationCustomerInHandler;
            m_PickedLastPosition = transform.position;
        }

        public void CustomerOutOnStationDone(int cost)
        {
            m_CurrentChairLeaveOrder = null;
            m_CafeChairHandler.ResitOnCafeChair();
            m_CafeChairHandler.GetCafeChair().AddToRevenue(cost);
            SetCafeAnimationStateSitting();
            m_CafeChairHandler.ShowNextOrder();
        }
        #endregion

        #region Reset Queue
        public void ResetQueue(QueueInfo newQueue)
        {
            m_CurrentQueue = newQueue;
            m_PickUp.SetActive(false);

            PathTraverserExtension.StopTargetImmediately(transform);
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, newQueue.Node, m_WalkSpeed, OnGoingToNodeResetQueue, OnReachedQueueResetQueue);
        }

        private void OnGoingToNodeResetQueue(PathNode node1, PathNode node2)
        {
            m_CurrentNode = node2;

            if (node1.TryGetComponent(out PathNodeDirection nodeDirection))
            {
                PathDirection direction = nodeDirection.GetDirection(node2);
                m_AnimatorController.SetWalkSpeed(m_WalkSpeed);
                PlayWalkAnimation(direction);
            }

            if (node2.TryGetComponent(out SortingGroup sortingGroup))
            {
                SetSortingLayer(sortingGroup.sortingLayerName, sortingGroup.sortingOrder);
            }
        }

        private void OnReachedQueueResetQueue(PathNode node)
        {
            if (m_HasEntered == false)
            {
                OnReachedQueueNodeEntered(node);
            }
            else
            {
                m_CurrentNode = node;
                if (node.TryGetComponent(out PatienceChairCustomerHandler handler))
                {
                    m_IsOnPatienceChair = true;
                    if (m_CafeWaitState == CustomerAnimatorState.StandingIdleNeutral)
                    {
                        m_CafeWaitState = CustomerAnimatorState.SittingIdleNeurtalDown;
                    }
                    else if (m_CafeWaitState == CustomerAnimatorState.StandingIdleAngry)
                    {
                        m_CafeWaitState = CustomerAnimatorState.SittingIdleNeurtalDown;
                    }
                    else if (m_CafeWaitState == CustomerAnimatorState.StandingIdleFurious)
                    {
                        m_CafeWaitState = CustomerAnimatorState.SittingIdleFuriousDown;
                    }

                    m_AnimatorController.PlayState(CustomerAnimatorState.SittingDown, () =>
                    {
                        PlayCafeAnimationState();
                    });
                }
                else
                {
                    PlayCafeAnimationState();
                }
                m_PickUp.SetActive(true);
            }
        }
        #endregion

        #region Leave Unserved

        public void RemoveFromQueue()
        {
            m_CurrentQueue.CurrentCustomer = null;
        }

        public void LeaveUnservedCafeCustomer()
        {
            GlobalEventHolder.OnCustomerLost?.Invoke();

            if (CustomerManager.CanAcquireDoNotLoseCustomerLevelChance) // If can acquire chance the customer not gonna leave
            {
                m_IsAboutToLeaveUnserved = true;
                return;
            }

            PathNode startNode = CustomerManager.GetExitNode();
            if(m_CounterTableController == null)
            {
                m_CurrentQueue.CurrentCustomer = null;
                CustomerManager.ResetQueue();
            }
            else
            {
                m_CounterTableController.CustomerRemoved();
            }

            CustomerManager.RemoveFromQueue(this);

            m_PickUp.SetActive(false);
            PathTraverserExtension.StopTargetImmediately(transform);
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, startNode, m_WalkSpeed, OnGoingToNodeLeavingUnserved, OnReachedNodeLeft);
        }

        private void OnGoingToNodeLeavingUnserved(PathNode node1, PathNode node2)
        {
            m_CurrentNode = node2;

            if (node1.TryGetComponent(out PathNodeDirection nodeDirection))
            {
                PathDirection direction = nodeDirection.GetDirection(node2);
                m_AnimatorController.SetWalkSpeed(m_WalkSpeed);
                PlayWalkAnimation(direction);
            }

            if (node2.TryGetComponent(out SortingGroup sortingGroup))
            {
                SetSortingLayer(sortingGroup.sortingLayerName, sortingGroup.sortingOrder);
            }
        }

        private void OnReachedNodeLeft(PathNode pathNode)
        {
            Destroy(gameObject);
        }

        #endregion

        #region Reset On Level Chance
        public void ResetCustomerWaitOnLevelChance()
        {
            m_AnimatorController.PlayState(CustomerAnimatorState.StandingIdleNeutral);

            if (m_CafeWaitCorotoine != null)
            {
                StopCoroutine(m_CafeWaitCorotoine);
                m_CafeWaitCorotoine = null;
            }

            m_PickUp.SetActive(true);
            m_CafeWaitCorotoine = StartCoroutine(CafeWaitCorotine());
        }
        #endregion

        #region Helper
        private void PlayWalkAnimation(PathDirection direction)
        {
            switch (direction)
            {
                case PathDirection.Up:
                    m_AnimatorController.PlayState(CustomerAnimatorState.WalkUp);
                    break;
                case PathDirection.Down:
                    m_AnimatorController.PlayState(CustomerAnimatorState.WalkDown);
                    break;
                case PathDirection.Left:
                    m_AnimatorController.PlayState(CustomerAnimatorState.WalkLeft);
                    break;
                case PathDirection.Right:
                    m_AnimatorController.PlayState(CustomerAnimatorState.WalkRight);
                    break;
            }
        }

        private void PlayCafeAnimationState()
        {
            if (!m_IsPickedUp)
            {
                m_AnimatorController.PlayState(m_CafeWaitState);
            }
            else if (m_IsPickedUp && m_CafeChairHandler == null)
            {
                if (m_CafeWaitState == CustomerAnimatorState.StandingIdleNeutral
                    || m_CafeWaitState == CustomerAnimatorState.SittingIdleNeurtalDown)
                {
                    m_AnimatorController.PlayState(CustomerAnimatorState.PickedUpNeutral);
                }
                else if (m_CafeWaitState == CustomerAnimatorState.StandingIdleAngry
                    || m_CafeWaitState == CustomerAnimatorState.SittingIdleAngryDown)
                {
                    m_AnimatorController.PlayState(CustomerAnimatorState.PickedUpAngry);
                }
                else if (m_CafeWaitState == CustomerAnimatorState.StandingIdleFurious
                    || m_CafeWaitState == CustomerAnimatorState.SittingIdleFuriousDown)
                {
                    m_AnimatorController.PlayState(CustomerAnimatorState.PickedUpFurious);
                }
            }
            else if (m_PickUp && m_IsOnCafeChair)
            {
                m_AnimatorController.PlayState(CustomerAnimatorState.PickedUpNeutral);
            }
        }

        public void PlayAnimationState(CustomerAnimatorState animationState)
        {
            m_AnimatorController.PlayState(animationState);
        }

        public void SetCafeAnimationStateSitting()
        {
            m_AnimatorController.PlayState(CustomerAnimatorState.SittingDrinkingRight);
        }

        public void SetAnimationStateHappy(Action onCompete)
        {
            InvokeHappyStateEvent();
            m_AnimatorController.PlayState(CustomerAnimatorState.StandingIdleHappyDown, onCompete);
        }

        public void SetSortingLayer(string layer, int order)
        {
            foreach (var sortingGroup in m_SortingGroups)
            {
                sortingGroup.sortingLayerName = layer;
                sortingGroup.sortingOrder = order;
            }
        }

        public List<CustomerOrderInfo> GetCafeOrders()
        {
            return m_CustomerCafeOrdersInfo;
        }

        public CustomerWaitState GetWaitState()
        {
            return m_WaitState;
        }

        public CustomerCommonSetting GetCommonSetting()
        {
            return m_CommonSettings;
        }

        public bool IsCustomerVIP()
        {
            return m_IsCustomerVIP;
        }

        public bool IsCustomerOnCafeChair()
        {
            return m_IsOnCafeChair;
        }

        // Returns the Order Type and UI Order Position
        public List<Tuple<DataConsumable, Vector3>> GetCurrentWaitressOrders()
        {
            if (m_CafeChairHandler != null)
            {
                return m_CafeChairHandler.GetCafeChair().GetCurrentPlayerOrders();
            }

            return null;
        }

        public void LockWaitressOrders()
        {
            if (m_CafeChairHandler != null)
            {
                m_CafeChairHandler.GetCafeChair().LockPlayerOrders();
            }
        }

        public void UnlockWaitressOrders()
        {
            if (m_CafeChairHandler != null)
            {
                m_CafeChairHandler.GetCafeChair().UnlockPlayerOrders();
            }
        }

        public void FreezeWait()
        {
            m_WaitFrozen = true;
        }

        public void UnFreezeWait()
        {
            m_WaitFrozen = false;
        }

        //Send orders with costs
        public void FillCurrentWaitressOrders(List<Tuple<DataConsumable, int>> nonWorkerOrders)
        {
            if (m_CafeChairHandler != null)
            {
                m_CafeChairHandler.GetCafeChair().FillCurrentPlayerOrders(nonWorkerOrders);
            }
        }

        public void SetWaitDurationCounter(float value)
        {
            m_WaitDurationCounter = value;
        }

        public bool IsAboutToLeaveUnserved()
        {
            return m_IsAboutToLeaveUnserved;
        }

        public void InvokeAngryStateEvent()
        {
            OnAngryState?.Invoke();
        }

        public void InvokeFuriousStateEvent()
        {
            OnFuriousState?.Invoke();
        }

        public void InvokeHappyStateEvent()
        {
            OnHappySurvedState?.Invoke();
        }

        public bool IsPicked()
        {
            return m_IsPickedUp;
        }

        public DataConsumable GetCurrentChairLeaveOrder()
        {
            return m_CurrentChairLeaveOrder;
        }
        #endregion

        #region Leaving at Game's End
        public void LeaveImmeditalityForGamesEnd()
        {
            if (m_CafeWaitCorotoine != null)
            {
                StopCoroutine(m_CafeWaitCorotoine);
                m_CafeWaitCorotoine = null;
            }

            m_PickUp.SetActive(false);
            m_WaitingUIController.StopVibrating();
            m_WaitingUIController.SetActiveState(false);

            PathNode startNode = CustomerManager.GetExitNode();
            PathTraverserExtension.StopTargetImmediately(transform);
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, startNode, m_WalkSpeed, OnGoingToNodeLeavingImmediately, OnReachedNodeLeftImmediately);
        }

        private void OnGoingToNodeLeavingImmediately(PathNode node1, PathNode node2)
        {
            m_CurrentNode = node2;

            if (node1.TryGetComponent(out PathNodeDirection nodeDirection))
            {
                PathDirection direction = nodeDirection.GetDirection(node2);
                m_AnimatorController.SetWalkSpeed(m_WalkSpeed);
                PlayWalkAnimation(direction);
            }

            if (node2.TryGetComponent(out SortingGroup sortingGroup))
            {
                SetSortingLayer(sortingGroup.sortingLayerName, sortingGroup.sortingOrder);
            }
        }

        private void OnReachedNodeLeftImmediately(PathNode pathNode)
        {
            Destroy(gameObject);
        }
        #endregion

        #region Global Events
        private void OnTimeFrozeBooster(bool activation)
        {
            if (activation)
            {
                m_IsTimeFrozeBoosterActivated = true;
                m_WaitingUIController.PlayTimeFrozeEffect();
                GlobalEventHolder.OnACustomersWaitFrozen?.Invoke();
            }
            else
            {
                m_IsTimeFrozeBoosterActivated = false;
                m_WaitingUIController.StopTimeFrozeEffect();
            }
        }

        private void OnPatienceSunRays(bool activation)
        {
            if (activation)
            {
                m_WaitingUIController.PlayHeatEffect();
            }
            else
            {
                m_WaitingUIController.StopHeatEffect();
            }
        }

        private void OnCustomerWaitFreeze(bool value)
        {
            m_WaitFrozen = value;
        }
        #endregion
    }
}
