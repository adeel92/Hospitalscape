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
    public class CustomerSalonController : CustomerBaseController
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

        [Header("---Apron---")]
        [SerializeField] CustomerApronController m_ApronController;

        [Header("---PickedUp---")]
        [SerializeField] List<PickupCollider> PickupColliders = new();
        [SerializeField] GameObject m_PickUp;
        [SerializeField, ReadOnly] PickupCollider m_CurrentPickupCollider = null;
        private bool m_IsPickedUp = false;
        [SerializeField, SortingLayer] string m_PickedUpSortingLayer;
        [SerializeField] float m_PickedUpZAxis = 0;
        [SerializeField] float m_SittingDragMoveMinDisance = 1;
        private Vector3 m_PickedLastPosition;

        [Header("---Service---")]
        [SerializeField] CustomerAnimatorState m_MainServiceAnimationState = CustomerAnimatorState.LayingPain;

        private MainServiceCustomerHandler m_MainServiceCustomerHandler = null;
        private StationCustomerInHandler m_StationCusterInHandler = null;
        private CounterTableController m_CounterTableController = null;

        [Header("---Loaded Externally---")]
        [SerializeField, ReadOnly] bool m_IsCustomerVIP;
        [SerializeField, ReadOnly] CustomerData m_DataCustomerSalon;
        [SerializeField, ReadOnly, Expandable] CustomerCommonSetting m_CommonSettings;
        [SerializeField, ReadOnly] CustomerFirstOrderInfo m_CustomerFirstSalonOrder;
        [SerializeField, ReadOnly] List<CustomerOrderInfo> m_CustomerSalonOrdersInfo;
        [SerializeField, ReadOnly] PathNode m_CurrentNode;
        [SerializeField, ReadOnly] QueueInfo m_CurrentQueue;
        [SerializeField, ReadOnly] DataConsumable m_CurrentChairLeaveOrder = null;

        private float m_WalkSpeed;
        private float m_WaitDurationCounter = 0;
        private CustomerAnimatorState m_SalonWaitState = CustomerAnimatorState.PickedUpNeutral;
        private CustomerWaitState m_WaitState;
        private Coroutine m_SalonWaitCorotoine;
        private bool m_HasEntered = false;
        [SerializeField, ReadOnly]
        private bool m_IsOnSalonChair = false;
        [SerializeField, ReadOnly]
        private bool m_IsFirstOrderUndecided = false;
        private bool m_IsTimeFrozeBoosterActivated = false;
        private bool m_WaitFrozen = false;
        private bool m_IsAboutToLeaveUnserved = false;
        private bool m_IsOnPatienceChair = false;

        public CustomerFirstOrderInfo CustomerFirstSalonOrder => m_CustomerFirstSalonOrder;
        
        public void Setup(CustomerData dataCustomer, PathNode currentNode, QueueInfo queueInfo, float walkSpeed)
        {
            m_DataCustomerSalon = dataCustomer;
            m_CommonSettings = m_DataCustomerSalon.CustomerCommonSettings;
            m_CurrentNode = currentNode;
            m_CurrentQueue = queueInfo;
            m_CurrentQueue.CurrentCustomer = this;
            m_WalkSpeed = walkSpeed;
            m_IsFirstOrderUndecided = dataCustomer.IsFirstOrderUndecided;

            m_WaitState = CustomerWaitState.Happy;

            CustomerData salonData = (CustomerData)dataCustomer;
            m_IsCustomerVIP = salonData.IsCustomerVIP;

            int firstOrderIndex = UnityEngine.Random.Range(0, salonData.CustomerFirstOrdersInfoHolder.CustomerFirstOrdersInfo.Count);
            m_CustomerFirstSalonOrder = salonData.CustomerFirstOrdersInfoHolder.CustomerFirstOrdersInfo[firstOrderIndex];

            m_WaitingUIController.SetupForSalon(m_CustomerFirstSalonOrder.OrderConsumable, m_IsFirstOrderUndecided);

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

            if (salonData.CustomerOrderBundles.Count > 0)
            {
                int ordersIndex = UnityEngine.Random.Range(0, salonData.CustomerOrderBundles.Count);
                m_CustomerSalonOrdersInfo = salonData.CustomerOrderBundles[ordersIndex].CustomerOrdersInfo;
            }
            else
            {
                m_CustomerSalonOrdersInfo = null;
            }

            DisableAllPickupColliders();
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

        #region Pickup
        private void EnablePickupCollider(PickupColliderType colliderType)
        {
            PickupCollider pickupCollider = PickupColliders.Find(x => x.PickupColliderType == colliderType);
            if(pickupCollider != null)
            {
                pickupCollider.ColliderObj.SetActive(true);
                m_CurrentPickupCollider = pickupCollider;
            }
        }
        private void DisablePickupCollider()
        {
            if(m_CurrentPickupCollider != null)
            {
                m_CurrentPickupCollider.ColliderObj.SetActive(false);
                m_CurrentPickupCollider = null;
            }
        }
        private void DisableAllPickupColliders()
        {
            if(PickupColliders != null && PickupColliders.Count > 0)
            {
                foreach(PickupCollider pickupCollider in PickupColliders)
                {
                    pickupCollider.ColliderObj.SetActive(false);
                }
            }
        }
        #endregion

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
                PatienceChairController patienceChairController = handler.PatienceChairController;
                transform.SetParent(patienceChairController.CustomerSittingHolder);
                transform.localPosition = Vector3.zero;
                // m_PickUp.SetActive(true);
                EnablePickupCollider(PickupColliderType.Sitting);
                m_SalonWaitState = CustomerAnimatorState.SittingIdleNeurtalRight;
                m_AnimatorController.PlayState(CustomerAnimatorState.SititngRight, () =>
                {
                    m_AnimatorController.PlayState(CustomerAnimatorState.SittingIdleNeurtalRight);
                });
                m_SalonWaitCorotoine = StartCoroutine(SalonWaitCorotine());
            }
            else
            {
                m_AnimatorController.PlayState(CustomerAnimatorState.StandingIdleNeutral);
                m_IsOnPatienceChair = false;
                // m_PickUp.SetActive(true);
                EnablePickupCollider(PickupColliderType.Standing);
                m_SalonWaitCorotoine = StartCoroutine(SalonWaitCorotine());
            }

            m_HasEntered = true;
        }
        #endregion

        #region Waiting Customer Salon
        IEnumerator SalonWaitCorotine()
        {
            CustomerData customerSalon = (CustomerData)m_DataCustomerSalon;
            
            float waitDuration = m_CommonSettings.WaitingDuration + CustomerPatienceManager.GetExtraPatienace();
            m_WaitDurationCounter = waitDuration;

            m_WaitingUIController.SetActiveState(true);
            m_SalonWaitState = m_IsOnPatienceChair == false ? CustomerAnimatorState.StandingIdleNeutral : CustomerAnimatorState.SittingIdleNeurtalDown;
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
                    m_SalonWaitState = m_IsOnPatienceChair == false ? CustomerAnimatorState.StandingIdleAngry : CustomerAnimatorState.SittingIdleAngryDown;
                    PlaySalonAnimationState();
                    m_WaitState = CustomerWaitState.Angry;
                    InvokeAngryStateEvent();
                }

                if (m_WaitState == CustomerWaitState.Angry && m_CommonSettings.IsSecondWaitOver(m_WaitDurationCounter, waitDuration))
                {
                    m_WaitingUIController.SetWaitingColor(m_CommonSettings.SecondWaitColor());
                    m_WaitingUIController.StartVibrating();
                    m_SalonWaitState = m_IsOnPatienceChair == false ? CustomerAnimatorState.StandingIdleFurious : CustomerAnimatorState.SittingIdleFuriousDown;
                    PlaySalonAnimationState();
                    m_WaitState = CustomerWaitState.Furious;
                    InvokeFuriousStateEvent();
                }
            }

            yield return new WaitWhile(() => m_IsPickedUp == true);

            if (m_MainServiceCustomerHandler == null)
            {
                m_WaitingUIController.StopVibrating();
                m_WaitingUIController.SetActiveState(false);
                LeaveUnservedSalonCustomer();
            }
        }

        private void StopSalonWaitCorotoine()
        {
            m_WaitingUIController.StopVibrating();
            m_WaitingUIController.SetActiveState(false);
            if (m_SalonWaitCorotoine != null)
            {
                StopCoroutine(m_SalonWaitCorotoine);
                m_SalonWaitCorotoine = null;
            }
        }
        #endregion

        #region Picked
        public void OnDragBegin()
        {
            m_IsPickedUp = true;
            SetSortingLayer(m_PickedUpSortingLayer, 0);
            if (m_IsOnSalonChair)
            {
                transform.SetParent(CustomerManager.ParentTransfomr);
            }
            PlaySalonAnimationState();
            HideApron();
        }

        public void OnDrag()
        {
            Vector3 pickedPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pickedPosition.z = m_PickedUpZAxis;

            if (!m_IsOnSalonChair)
            {
                if(!m_IsFirstOrderUndecided)
                {
                    if (m_MainServiceCustomerHandler != null
                        && Vector3.Distance(pickedPosition, m_MainServiceCustomerHandler.transform.position) > m_MainServiceCustomerHandler.GetExitDistance())
                    {
                        m_MainServiceCustomerHandler.CustomerDragedRemoved();
                        m_MainServiceCustomerHandler = null;
                        transform.SetParent(CustomerManager.ParentTransfomr);
                        transform.position = pickedPosition;
                        OnDragBegin();
                    }
                    else if (m_MainServiceCustomerHandler == null)
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
            if (!m_IsOnSalonChair)
            {
                if (!m_IsFirstOrderUndecided)
                {
                    if (m_MainServiceCustomerHandler == null)
                    {
                        if(m_CounterTableController == null)
                        {
                            transform.position = m_CurrentNode.transform.position;
                            PlaySalonAnimationState();

                            if (m_CurrentNode.TryGetComponent(out SortingGroup group))
                            {
                                SetSortingLayer(group.sortingLayerName, group.sortingOrder);
                            }

                            CustomerManager.ResetQueue();
                        }
                        else
                        {
                            PlaySalonAnimationState();
                            m_CounterTableController.ReStandAtTheCounter();
                        }
                    }
                    else if (m_MainServiceCustomerHandler != null)
                    {
                        m_IsOnSalonChair = true;
                        // m_PickUp.SetActive(false);
                        DisablePickupCollider();
                        StopSalonWaitCorotoine();

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

                        m_MainServiceCustomerHandler.SitOnTheSalonChair();
                    }
                }
                else
                {
                    if (m_CounterTableController == null)
                    {
                        transform.position = m_CurrentNode.transform.position;
                        PlaySalonAnimationState();

                        if (m_CurrentNode.TryGetComponent(out SortingGroup group))
                        {
                            SetSortingLayer(group.sortingLayerName, group.sortingOrder);
                        }

                        CustomerManager.ResetQueue();
                    }
                    else if (m_CounterTableController != null)
                    {
                        // m_PickUp.SetActive(false);
                        DisablePickupCollider();
                        m_CurrentQueue.CurrentCustomer = null;
                        CustomerManager.ResetQueue();
                        m_CurrentNode = m_CounterTableController.GetStandingPathNode();
                        StopSalonWaitCorotoine();

                        m_CounterTableController.StandAtTheCounter(() =>
                        {
                            m_IsFirstOrderUndecided = false;
                            m_WaitingUIController.SetupForSalon(m_CustomerFirstSalonOrder.OrderConsumable, m_IsFirstOrderUndecided);
                            m_AnimatorController.PlayState(CustomerAnimatorState.StandingIdleNeutral);
                            // m_PickUp.SetActive(true);
                            EnablePickupCollider(PickupColliderType.Standing);
                            m_SalonWaitCorotoine = StartCoroutine(SalonWaitCorotine());
                        });
                    }
                }
            }
            else
            {
                if (m_StationCusterInHandler == null)
                {
                    m_MainServiceCustomerHandler.ResitOnSalonChair();
                    SetSalonAnimationStateSitting();
                    ShowApron(GetSalonFirstOrder().OrderConsumable);
                }
                else if (m_StationCusterInHandler != null)
                {
                    m_MainServiceCustomerHandler.GetSalonChair().CleanPreviousOrders();
                    m_StationCusterInHandler.CustomerEnters();
                    // m_PickUp.SetActive(false);
                    DisablePickupCollider();
                }
            }
        }

        #endregion

        #region Sitting
        // public void SitOnSalonChair(SalonChairCustomerHandler salonChairHandler)
        // {
        //     if (!m_IsOnSalonChair)
        //     {
        //         m_MainServiceCustomerHandler = salonChairHandler;
        //         m_PickedLastPosition = transform.position;
        //         m_AnimatorController.PlayState(CustomerAnimatorState.SittingDown, () =>
        //         {
        //             m_AnimatorController.PlayState(CustomerAnimatorState.SittingIdleNeurtalDown);
        //         });
        //     }
        // }
        public void SitOnServiceSeat(MainServiceCustomerHandler mainServiceCustomerHandler)
        {
            if (!m_IsOnSalonChair)
            {
                m_MainServiceCustomerHandler = mainServiceCustomerHandler;
                m_PickedLastPosition = transform.position;
                m_AnimatorController.PlayState(CustomerAnimatorState.SittingDown, () =>
                {
                    m_AnimatorController.PlayState(m_MainServiceAnimationState);
                });
            }
        }
        #endregion

        #region  Counter
        public void StandAtTheCounter(CounterTableController counterTableController)
        {
            if (!m_IsOnSalonChair && m_IsFirstOrderUndecided)
            {
                m_CounterTableController = counterTableController;
                m_PickedLastPosition = transform.position;
                m_AnimatorController.PlayState(CustomerAnimatorState.StandingIdleNeutral);
            }
        }
        #endregion

        #region Salon Chair Leave Order
        public void SetCustomerForChairLeaveOrder(DataConsumable chairLeaveOrder)
        {
            m_CurrentChairLeaveOrder = chairLeaveOrder;
            // m_PickUp.SetActive(true);
            EnablePickupCollider(PickupColliderType.Laying);
        }

        public void SetForCustomerInHandler(StationCustomerInHandler stationCustomerInHandler)
        {
            m_StationCusterInHandler = stationCustomerInHandler;
            m_PickedLastPosition = transform.position;
        }

        public void CustomerOutOnStationDone(int cost)
        {
            ShowApron(GetSalonFirstOrder().OrderConsumable);
            m_CurrentChairLeaveOrder = null;
            m_MainServiceCustomerHandler.ResitOnSalonChair();
            m_MainServiceCustomerHandler.GetSalonChair().AddToRevenue(cost);
            m_MainServiceCustomerHandler.GetSalonChair().ResitAfterLeaveOrder();
            m_AnimatorController.PlayState(CustomerAnimatorState.SittingDown, () =>
            {
                m_AnimatorController.PlayState(m_MainServiceAnimationState);
            });
            m_MainServiceCustomerHandler.ShowNextOrder();
        }
        #endregion

        #region Reset Queue
        public void ResetQueue(QueueInfo newQueue)
        {
            m_CurrentQueue = newQueue;
            // m_PickUp.SetActive(false);
            DisablePickupCollider();

            PathTraverserExtension.StopTargetImmediately(transform);
            PathTraverserExtension.MoveTarget(transform, m_CurrentNode, newQueue.Node, m_WalkSpeed, OnGoingToNodeResetQueue, OnReachedQueueSalonQueue);
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

        private void OnReachedQueueSalonQueue(PathNode node)
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
                    if (m_SalonWaitState == CustomerAnimatorState.StandingIdleNeutral)
                    {
                        m_SalonWaitState = CustomerAnimatorState.SittingIdleNeurtalDown;
                    }
                    else if (m_SalonWaitState == CustomerAnimatorState.StandingIdleAngry)
                    {
                        m_SalonWaitState = CustomerAnimatorState.SittingIdleNeurtalDown;
                    }
                    else if(m_SalonWaitState == CustomerAnimatorState.StandingIdleFurious)
                    {
                        m_SalonWaitState = CustomerAnimatorState.SittingIdleFuriousDown;
                    }

                    m_AnimatorController.PlayState(CustomerAnimatorState.SittingDown, () =>
                    {
                        PlaySalonAnimationState();
                    });
                    EnablePickupCollider(PickupColliderType.Sitting);
                }
                else
                {
                    PlaySalonAnimationState();
                }
                EnablePickupCollider(PickupColliderType.Standing);
                // m_PickUp.SetActive(true);
            }
        }
        #endregion

        #region Leave Unserved
        public void RemoveFromQueue()
        {
            m_CurrentQueue.CurrentCustomer = null;
        }

        public void LeaveUnservedSalonCustomer()
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

            if (m_IsOnPatienceChair)
            {
                Transform customerManagerTransform = CustomerManager.GetTransform();
                if(customerManagerTransform != null)
                {
                    transform.SetParent(customerManagerTransform);
                    transform.position = m_CurrentNode.transform.position;                    
                }
            }
            // m_PickUp.SetActive(false);
            DisablePickupCollider();
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

            if (m_SalonWaitCorotoine != null)
            {
                StopCoroutine(m_SalonWaitCorotoine);
                m_SalonWaitCorotoine = null;
            }

            // m_PickUp.SetActive(true);
            EnablePickupCollider(PickupColliderType.Standing);
            m_SalonWaitCorotoine = StartCoroutine(SalonWaitCorotine());
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

        private void PlaySalonAnimationState()
        {
            if (!m_IsPickedUp)
            {
                m_AnimatorController.PlayState(m_SalonWaitState);
            }
            else if(m_IsPickedUp && m_MainServiceCustomerHandler == null)
            {
                if (m_SalonWaitState == CustomerAnimatorState.StandingIdleNeutral 
                    || m_SalonWaitState == CustomerAnimatorState.SittingIdleNeurtalDown)
                {
                    m_AnimatorController.PlayState(CustomerAnimatorState.PickedUpNeutral);
                }
                else if (m_SalonWaitState == CustomerAnimatorState.StandingIdleAngry 
                    || m_SalonWaitState == CustomerAnimatorState.SittingIdleAngryDown)
                {
                    m_AnimatorController.PlayState(CustomerAnimatorState.PickedUpAngry);
                }
                else if (m_SalonWaitState == CustomerAnimatorState.StandingIdleFurious 
                    || m_SalonWaitState == CustomerAnimatorState.SittingIdleFuriousDown)
                {
                    m_AnimatorController.PlayState(CustomerAnimatorState.PickedUpFurious);
                }
            }
            else if (/*m_PickUp*/m_CurrentPickupCollider != null && m_IsOnSalonChair)
            {
                m_AnimatorController.PlayState(CustomerAnimatorState.PickedUpNeutral);
            }
        }

        public void PlayAnimationState(CustomerAnimatorState animationState)
        {
            m_AnimatorController.PlayState(animationState);
        }

        public void SetSalonAnimationStateSitting()
        {
            m_AnimatorController.PlayState(CustomerAnimatorState.SittingIdleNeurtalDown);
        }

        public void SetMainServiceAnimationState()
        {
            m_AnimatorController.PlayState(m_MainServiceAnimationState);
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

        public CustomerFirstOrderInfo GetSalonFirstOrder()
        {
            return m_CustomerFirstSalonOrder;
        }

        public List<CustomerOrderInfo> GetSalonOrders()
        {
            return m_CustomerSalonOrdersInfo;
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

        public bool IsCustomerOnSalonChair()
        {
            return m_IsOnSalonChair;
        }

        public bool IsCustomerFirstOrderUndecided()
        {
            return m_IsFirstOrderUndecided;
        }

        // Returns the Order Type and UI Order Position
        public List<Tuple<DataConsumable, Vector3>> GetCurrentWaitressOrders()
        {
            if (m_MainServiceCustomerHandler != null)
            {
                return m_MainServiceCustomerHandler.GetSalonChair().GetCurrentPlayerOrders();
            }

            return null;
        }

        public void LockWaitressOrders()
        {
            if (m_MainServiceCustomerHandler != null)
            {
                m_MainServiceCustomerHandler.GetSalonChair().LockPlayerOrders();
            }
        }

        public void UnlockWaitressOrders()
        {
            if (m_MainServiceCustomerHandler != null)
            {
                m_MainServiceCustomerHandler.GetSalonChair().UnlockPlayerOrders();
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
            if (m_MainServiceCustomerHandler != null)
            {
                m_MainServiceCustomerHandler.GetSalonChair().FillCurrentPlayerOrders(nonWorkerOrders);
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

        public void ShowApron(DataConsumable orderType)
        {
            m_ApronController.ShowApron(orderType);
        }

        public void HideApron()
        {
            m_ApronController.HideApron();
        }
        #endregion

        #region Leaving at Game's End
        public void LeaveImmeditalityForGamesEnd()
        {
            if (m_SalonWaitCorotoine != null)
            {
                StopCoroutine(m_SalonWaitCorotoine);
                m_SalonWaitCorotoine = null;
            }

            // m_PickUp.SetActive(false);
            DisablePickupCollider();
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

    public enum PickupColliderType
    {
        Standing, Sitting, Laying    
    }

    public enum CustomerWaitState
    {
        Happy, Angry, Furious
    }

    public enum CustomerType
    {
        Salon, Cafe
    }

    [Serializable]
    public class PickupCollider
    {
        public PickupColliderType PickupColliderType;
        public GameObject ColliderObj;
    }
}
