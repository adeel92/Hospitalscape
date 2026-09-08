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
    public class StationCustomerInHandler : MonoBehaviour
    {
        [Serializable]
        private class CustomerAnimationInfo
        {
            public float Delay;
            public CustomerAnimatorState AnimatorState;
            public UnityEvent Callback;
        }

        //---For different states during the whole treatment---
        public Action OnMenuSetupStart;
        public Action OnGameplaySetupStart;
        public Action OnCustomerDraggedIn;
        public Action OnCustomerDraggedOut;
        public Action<CustomerSalonController> OnCustomerDropped;
        public Action OnCustomerSettled;
        public Action<float> OnServiceStart;
        public Action<float> OnServiceTimerUpdate;
        public Action OnServiceEnd;
        public Action OnCustomerLeft;
        public Action OnInstantOrderComplete;

        [Header("---Setup---")]
        [SerializeField] DataConsumable m_InOrder;
        [SerializeField] bool m_ShowCapturingGizmos = true;
        [SerializeField] float m_CaptureDistance;
        [SerializeField] float m_ExitDistance;
        [SerializeField] float m_DetectionCooldownDuration = 0.2f;
        [SerializeField] CustomerAnimatorState m_InAnimatorState;
        [SerializeField] bool m_HasCustomerServiceStartAnimation = false;
        [ShowIf(nameof(m_HasCustomerServiceStartAnimation))]
        [SerializeField] CustomerAnimatorState m_CustomerServiceStartState;
        [SerializeField] bool m_HasCustomerServiceEndAnimation = false;
        [ShowIf(nameof(m_HasCustomerServiceEndAnimation))]
        [SerializeField] CustomerAnimatorState m_CustomerServiceEndState;

        [Space]
        [SerializeField] Transform m_CustomerHolder;
        [SerializeField] Vector3 m_CustomerSittingLocalScale = Vector3.one;
        [SerializeField, SortingLayer] string m_HoldingSortingLayer;
        [SerializeField] int m_HoldingSortingOrder;
        [SerializeField] float m_DurationProperty;
        [SerializeField] int m_CostProperty;
        [SerializeField] UnityEvent OnCustomerEnteres;
        [SerializeField] UnityEvent OnCustomerLeaves;
        [SerializeField] List<CustomerAnimationInfo> m_CustomerAnimationInfos;

        [Space, SerializeField, ReadOnly]
        private CustomerSalonController m_CurrentCustomer;
        public CustomerSalonController CurrentCustomer => m_CurrentCustomer;

        [SerializeField, ReadOnly] float m_BeforeCustomerSettleDelay = 0f;
        [SerializeField, ReadOnly] float m_BeforeServiceStartDelay = 0f; 
        [SerializeField, ReadOnly] float m_BeforeCustomerLeaveDelay = 0f;
        [SerializeField] float m_BeforeInstantServiceCompleteDelay = 0.5f;

        private Coroutine m_DectectionUpdate = null;
        private Coroutine m_StartingCustomer = null;

        private void OnEnable()
        {
            GlobalEventHolder.OnInstanceOrderFillBooster += OnInstantOrderFill;
        }
        private void OnDisable()
        {
            GlobalEventHolder.OnInstanceOrderFillBooster -= OnInstantOrderFill;
        }

        public void SetupForMenu(float durationProperty, int costProperty)
        {
            OnMenuSetupStart?.Invoke();
            m_DurationProperty = durationProperty;
            m_CostProperty = costProperty;
        }

        public void SetupForGameplay(float durationProperty, int costProperty)
        {
            OnGameplaySetupStart?.Invoke();
            m_DurationProperty = durationProperty;
            m_CostProperty = costProperty;

            if (m_DectectionUpdate != null)
            {
                StopCoroutine(m_DectectionUpdate);
                m_DectectionUpdate = null;
            }
            m_DectectionUpdate = StartCoroutine(DectectionUpdate());
        }

        public void SetServiceDelays(float beforeCustomerSettleDelay, float beforeServiceStartDelay, float beforeCustomerLeaveDelay)
        {
            m_BeforeCustomerSettleDelay = beforeCustomerSettleDelay;
            m_BeforeServiceStartDelay = beforeServiceStartDelay;
            m_BeforeCustomerLeaveDelay = beforeCustomerLeaveDelay;
        }

        private IEnumerator DectectionUpdate()
        {
            float sqrCaptureDistance = m_CaptureDistance * m_CaptureDistance;
            yield return new WaitForSeconds(m_DetectionCooldownDuration);

            while (true)
            {
                yield return null;

                if (m_CurrentCustomer == null)
                {
                    List<CustomerSalonController> customers = CustomerManager.GetAllSalonCustomer();
                    foreach (var customer in customers)
                    {
                        if (customer != null 
                            && customer.IsCustomerOnSalonChair()
                            && customer.GetCurrentChairLeaveOrder() == m_InOrder)
                        {
                            float sqrDist = (customer.transform.position - transform.position).sqrMagnitude;

                            if (sqrDist <= sqrCaptureDistance)
                            {
                                m_CurrentCustomer = customer;
                                m_CurrentCustomer.transform.SetParent(m_CustomerHolder);
                                m_CurrentCustomer.transform.localPosition = Vector3.zero;
                                m_CurrentCustomer.transform.localScale = m_CustomerSittingLocalScale;
                                m_CurrentCustomer.SetSortingLayer(m_HoldingSortingLayer, m_HoldingSortingOrder);
                                m_CurrentCustomer.PlayAnimationState(m_InAnimatorState);
                                m_CurrentCustomer.SetForCustomerInHandler(this);
                                OnCustomerDraggedIn?.Invoke();
                                yield break;
                            }
                        }
                    }
                }

            }
        }

        public void RemoveCustomer()
        {
            m_CurrentCustomer = null;
            OnCustomerDraggedOut?.Invoke();
            if (m_DectectionUpdate != null)
            {
                StopCoroutine(m_DectectionUpdate);
                m_DectectionUpdate = null;
            }
            m_DectectionUpdate = StartCoroutine(DectectionUpdate());
        }

        public void CustomerEnters()
        {
            if(m_CurrentCustomer != null)
            {
                OnCustomerEnteres?.Invoke();
                OnCustomerDropped?.Invoke(m_CurrentCustomer);
                m_StartingCustomer = StartCoroutine(StartingCustomer());
            }
        }

        private IEnumerator StartingCustomer()
        {
            float waitCounter = 0;
            int customerAnimationInfoIndex = 0;

            yield return new WaitForSeconds(m_BeforeCustomerSettleDelay);
            OnCustomerSettled?.Invoke();

            yield return new WaitForSeconds(m_BeforeServiceStartDelay);
            OnServiceStart?.Invoke(m_DurationProperty);
            if (m_HasCustomerServiceStartAnimation)
            {
                m_CurrentCustomer.PlayAnimationState(m_CustomerServiceStartState);
            }

            while (waitCounter < m_DurationProperty)
            {
                if (customerAnimationInfoIndex < m_CustomerAnimationInfos.Count)
                {
                    CustomerAnimationInfo currentCustomerAnimationInfo = m_CustomerAnimationInfos[customerAnimationInfoIndex];
                    yield return new WaitForSeconds(currentCustomerAnimationInfo.Delay);

                    m_CurrentCustomer.PlayAnimationState(currentCustomerAnimationInfo.AnimatorState);
                    currentCustomerAnimationInfo.Callback?.Invoke();
                    customerAnimationInfoIndex++;
                }

                yield return null;
                waitCounter += Time.deltaTime;
                OnServiceTimerUpdate?.Invoke(waitCounter);
            }

            OnServiceEnd?.Invoke();
            if (m_HasCustomerServiceEndAnimation)
            {
                m_CurrentCustomer.PlayAnimationState(m_CustomerServiceEndState);
            }
            yield return new WaitForSeconds(m_BeforeCustomerLeaveDelay);
            OnCustomerLeaves?.Invoke();
            //m_CurrentCustomer.PerformanceDone(m_CostProperty);
            OnCustomerLeft?.Invoke();
            CustomerLeaves();
        }

        public void CustomerLeaves()
        {
            m_CurrentCustomer.CustomerOutOnStationDone(m_CostProperty);
            m_CurrentCustomer = null;

            if (m_DectectionUpdate != null)
            {
                StopCoroutine(m_DectectionUpdate);
                m_DectectionUpdate = null;
            }
            m_DectectionUpdate = StartCoroutine(DectectionUpdate());
        }

        public float GetExitDistance()
        {
            return m_ExitDistance;
        }

        private void OnInstantOrderFill()
        {
            if (m_StartingCustomer != null)
            {
                StopCoroutine(m_StartingCustomer);
                m_StartingCustomer = null;
            }
            StartCoroutine(CompleteServiceInstantly());
        }
        private IEnumerator CompleteServiceInstantly()
        {
            yield return new WaitForSeconds(m_BeforeInstantServiceCompleteDelay);
            if(m_CurrentCustomer == null)
                yield break;

            m_CurrentCustomer.CustomerOutOnStationDone();
            m_CurrentCustomer = null;

            if (m_DectectionUpdate != null)
            {
                StopCoroutine(m_DectectionUpdate);
                m_DectectionUpdate = null;
            }
            m_DectectionUpdate = StartCoroutine(DectectionUpdate());
            OnInstantOrderComplete?.Invoke();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(!m_ShowCapturingGizmos)
                return;

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
                Vector3 currentPoint = transform.position + new Vector3(x, y, 0f);

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
    }
}
