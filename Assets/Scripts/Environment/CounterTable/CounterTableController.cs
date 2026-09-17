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
using Isometric.PathSystem;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace Isometric.Environment
{
    public class CounterTableController : MonoBehaviour
    {
        //---Setup---
        private const string MetaSetupFoldOut = "---Setup---";
        [SerializeField, Foldout(MetaSetupFoldOut), Expandable] DataStation m_Data;
        [SerializeField, Foldout(MetaSetupFoldOut)] TaskTrigger m_TaskTrigger;
        [SerializeField, Foldout(MetaSetupFoldOut)] MainServiceOrderUIController m_OrderUIController;


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


        private const string MetaInteractiveFoldOut = "---Interactive---";
        [SerializeField, Foldout(MetaInteractiveFoldOut)] float m_OrderDecidingDelay;
        [SerializeField, Foldout(MetaInteractiveFoldOut)] float m_InstantOrderCompleteDelay = 2f;
        [SerializeField, Foldout(MetaInteractiveFoldOut)] PathNode m_StandingNode;
        [SerializeField, Foldout(MetaInteractiveFoldOut)] float m_CaptureDistance;
        [SerializeField, Foldout(MetaInteractiveFoldOut)] float m_ExitDistance;
        [SerializeField, SortingLayer, Foldout(MetaInteractiveFoldOut)] string m_StandingSortingLayer;
        [SerializeField, Foldout(MetaInteractiveFoldOut)] int m_StandingSortingOrder;
        [SerializeField, Foldout(MetaInteractiveFoldOut), ReadOnly] CustomerSalonController m_CurrentSalonCustomer;
        // [SerializeField, Foldout(MetaInteractiveFoldOut), ReadOnly] CustomerCafeController m_CurrentCafeCustomer;

        private Coroutine m_DectectionUpdate = null;
        private List<CurrentOrderInfo> m_CurrentOrdersInfo;
        private bool m_HasFirstOrderDecisionCallSent = false;
        private bool m_IsPlayerOrdersLocked = false;
        private bool m_IsCustomerWaitingToBeServed = false;
        private int m_TotalRevenue = 0;
        private Action m_OnFirstOrderDecided = null;


        public Action<int> OnTotalRevenueGenerated;


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
            else if (m_Data.StationData.IsUnlocked)
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

                        StartDetection();
                    }, m_CameraFocusDuration);
                });

                m_Data.StationData.HasJustUnlocked = false;
                m_Data.Save();
            }

            if (m_Data.StationData.HasUpgraded)
            {
                OnHasUpgradedMenu?.Invoke();
                m_Data.StationData.HasUpgraded = false;
                m_Data.Save();
            }

            if(m_Data.StationData.IsUnlocked && !hasJustUnlocked)
                StartDetection();
        }

        private void OnEnable()
        {
            m_TaskTrigger.OnTaskStart += OnTaskStart;
            GlobalEventHolder.OnInstanceOrderFillBooster += OnInstantOrderComplete;
        }

        private void OnDisable()
        {
            m_TaskTrigger.OnTaskStart -= OnTaskStart;
            GlobalEventHolder.OnInstanceOrderFillBooster -= OnInstantOrderComplete;
        }

        private void StartDetection()
        {
            if (m_DectectionUpdate != null)
            {
                StopCoroutine(m_DectectionUpdate);
                m_DectectionUpdate = null;
            }
            m_DectectionUpdate = StartCoroutine(DectectionUpdate());
        }
        private IEnumerator DectectionUpdate()
        {
            float sqrCaptureDistance = m_CaptureDistance * m_CaptureDistance;

            while (true)
            {
                yield return null;

                if (m_CurrentSalonCustomer == null /* && m_CurrentCafeCustomer == null */)
                {
                    List<CustomerSalonController> salonCustomers = CustomerManager.GetAllSalonCustomer();

                    foreach (var salonCustomer in salonCustomers)
                    {
                        if (salonCustomer != null 
                            && salonCustomer.IsCustomerFirstOrderUndecided()
                            && !salonCustomer.IsCustomerOnSalonChair())
                        {
                            float sqrDist = (salonCustomer.transform.position - m_StandingNode.transform.position).sqrMagnitude;
                            
                            if (sqrDist <= sqrCaptureDistance)
                            {
                                m_CurrentSalonCustomer = salonCustomer;
                                m_CurrentSalonCustomer.transform.SetParent(m_StandingNode.transform);
                                m_CurrentSalonCustomer.transform.localPosition = Vector3.zero;
                                m_CurrentSalonCustomer.SetSortingLayer(m_StandingSortingLayer, m_StandingSortingOrder);
                                m_CurrentSalonCustomer.StandAtTheCounter(this);
                                yield break;
                            }
                        }
                    }

                    /* List<CustomerCafeController> cafeCustomers = CustomerManager.GetAllCafeCustomer();

                    foreach (var cafeCustomer in cafeCustomers)
                    {
                        if (cafeCustomer != null && !cafeCustomer.IsCustomerOnCafeChair())
                        {
                            float sqrDist = (cafeCustomer.transform.position - m_StandingNode.transform.position).sqrMagnitude;

                            if (sqrDist <= sqrCaptureDistance)
                            {
                                m_CurrentCafeCustomer = cafeCustomer;
                                m_CurrentCafeCustomer.transform.SetParent(m_StandingNode.transform);
                                m_CurrentCafeCustomer.transform.localPosition = Vector3.zero;
                                m_CurrentCafeCustomer.SetSortingLayer(m_StandingSortingLayer, m_StandingSortingOrder);
                                m_CurrentCafeCustomer.StandAtTheCounter(this);
                                yield break;
                            }
                        }
                    } */
                }

            }
        }

        public void ReStandAtTheCounter()
        {
            if(m_CurrentSalonCustomer != null)
            {
                m_CurrentSalonCustomer.transform.SetParent(m_StandingNode.transform);
                m_CurrentSalonCustomer.transform.localPosition = Vector3.zero;
                m_CurrentSalonCustomer.SetSortingLayer(m_StandingSortingLayer, m_StandingSortingOrder);
            }

            /* if(m_CurrentCafeCustomer != null)
            {
                m_CurrentCafeCustomer.transform.SetParent(m_StandingNode.transform);
                m_CurrentCafeCustomer.transform.localPosition = Vector3.zero;
                m_CurrentCafeCustomer.SetSortingLayer(m_StandingSortingLayer, m_StandingSortingOrder);
            } */
        }

        public void StartDecidingFirstOrder(Action onWaitComplete)
        {
            if(m_HasFirstOrderDecisionCallSent)
                return;

            m_HasFirstOrderDecisionCallSent = true;
            CoroutineManager.LateAction(() =>
            {
                OnTotalRevenueGenerated?.Invoke(m_TotalRevenue);
                onWaitComplete?.Invoke();
            }, m_OrderDecidingDelay);
        }

        public void StartShowingOrders(Action onFirstOrderDecided)
        {
            m_OnFirstOrderDecided = onFirstOrderDecided;
            ShowNextOrder();
        }
        private void ShowNextOrder()
        {
            if (m_CurrentSalonCustomer != null)
            {
                CustomerOrderInfo customerOrders = m_CurrentSalonCustomer.GetCounterOrders();
                if (customerOrders != null && customerOrders.OrdersConsumable.Count > 0)
                {
                    m_CurrentOrdersInfo = new List<CurrentOrderInfo>();

                    foreach (var order in customerOrders.OrdersConsumable)
                    {
                        CurrentOrderInfo info = new CurrentOrderInfo();
                        info.Order = order;
                        info.HasBeenServed = false;
                        m_CurrentOrdersInfo.Add(info);
                    }

                    m_OrderUIController.CleanPreviousOrders();
                    List<Vector3> uiOrderPositions = m_OrderUIController.SetOrders(customerOrders.OrdersConsumable, m_CurrentSalonCustomer.GetSalonFirstOrder());

                    for (int i = 0; i < m_CurrentOrdersInfo.Count && i < uiOrderPositions.Count; i++)
                    {
                        m_CurrentOrdersInfo[i].UIOrderPosition = uiOrderPositions[i];
                    }

                    m_IsCustomerWaitingToBeServed = true;
                }
                else
                {
                    StartDecidingFirstOrder(m_OnFirstOrderDecided);
                }
            }
        }

        public void CustomerRemoved()
        {
            m_CurrentSalonCustomer = null;
            m_CurrentOrdersInfo = null;
            m_OnFirstOrderDecided = null;
            m_IsCustomerWaitingToBeServed = false;
            m_TotalRevenue = 0;
            m_HasFirstOrderDecisionCallSent = false;
            // m_CurrentCafeCustomer = null;

            if (m_DectectionUpdate != null)
            {
                StopCoroutine(m_DectectionUpdate);
                m_DectectionUpdate = null;
            }
            m_DectectionUpdate = StartCoroutine(DectectionUpdate());
        }

        public PathNode GetStandingPathNode()
        {
            return m_StandingNode;
        }

        public float GetExitDistance()
        {
            return m_ExitDistance;
        }

        // Returns the Order Type and UI Order Position
        public List<Tuple<DataConsumable, Vector3>> GetCurrentPlayerOrders()
        {
            if (m_CurrentOrdersInfo != null && m_CurrentOrdersInfo.Count > 0)
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

        public void LockPlayerOrders()
        {
            m_IsPlayerOrdersLocked = true;
        }

        public void UnlockPlayerOrders()
        {
            m_IsPlayerOrdersLocked = false;
        }

        private void OnInstantOrderComplete()
        {
            if(m_CurrentSalonCustomer != null)
            {
                CoroutineManager.LateAction(() =>
                {
                    m_OrderUIController.CleanPreviousOrders();
                    m_IsCustomerWaitingToBeServed = false;
                    StartDecidingFirstOrder(m_OnFirstOrderDecided);
                }, m_InstantOrderCompleteDelay);
            }
        }

        private void OnTaskStart(TaskTarget taskTarget)
        {
            if (taskTarget.TryGetComponent(out IEnvironmentInteractable interactable))
            {
                if (m_CurrentSalonCustomer != null && m_IsCustomerWaitingToBeServed && !m_IsPlayerOrdersLocked)
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
                        StartDecidingFirstOrder(m_OnFirstOrderDecided);
                        m_TaskTrigger.SendTaskResult(TaskResult.Success);
                    }
                    else if (hasBeenServedSomething == true)
                    {
                        m_OrderUIController.CleanPreviousOrders();
                        List<Vector3> uiOrderPositions = m_OrderUIController.SetOrders(currentOrders, m_CurrentSalonCustomer.GetSalonFirstOrder());

                        int uiOrderCount = 0;
                        for (int i = 0; i < m_CurrentOrdersInfo.Count && uiOrderCount < uiOrderPositions.Count; i++)
                        {
                            if (!m_CurrentOrdersInfo[i].HasBeenServed)
                            {
                                m_CurrentOrdersInfo[i].UIOrderPosition = uiOrderPositions[uiOrderCount];
                                uiOrderCount++;
                            }
                        }
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
            else
            {
                m_TaskTrigger.SendTaskResult(TaskResult.Failed);
            }
        }


        private class CurrentOrderInfo
        {
            public DataConsumable Order;
            public bool HasBeenServed;
            public Vector3 UIOrderPosition;
        }

        #region Editor
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(m_StandingNode == null) return;
            
            DrawCircle(Color.blue, m_CaptureDistance);
            DrawCircle(Color.red, m_ExitDistance);
        }

        private void DrawCircle(Color color, float radius)
        {
            Gizmos.color = color;

            Vector3 prevPoint = Vector3.zero;
            Vector3 firstPoint = Vector3.zero;

            float angleStep = 360f / 64;

            for (int i = 0; i <= 64; i++)
            {
                float angle = Mathf.Deg2Rad * (i * angleStep);
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                //Vector3 currentPoint = transform.position + new Vector3(x, y, 0f);
                Vector3 currentPoint = m_StandingNode.transform.position + new Vector3(x, y, 0f);

                if (i > 0)
                {
                    Gizmos.DrawLine(prevPoint, currentPoint);
                }
                else
                {
                    firstPoint = currentPoint;
                }

                prevPoint = currentPoint;
            }

            Gizmos.DrawLine(prevPoint, firstPoint);
        }
#endif
#endregion
    }
}
