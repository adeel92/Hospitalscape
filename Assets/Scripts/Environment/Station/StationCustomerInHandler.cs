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

        [Header("---Setup---")]
        [SerializeField] DataConsumable m_InOrder;
        [SerializeField] float m_CaptureDistance;
        [SerializeField] float m_ExitDistance;
        [SerializeField] CustomerAnimatorState m_InAnimatorState;

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


        Coroutine m_DectectionUpdate = null;

        public void SetupForMenu(float durationProperty, int costProperty)
        {
            m_DurationProperty = durationProperty;
            m_CostProperty = costProperty;
        }

        public void SetupForGameplay(float durationProperty, int costProperty)
        {
            m_DurationProperty = durationProperty;
            m_CostProperty = costProperty;

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
                StartCoroutine(StartingCustomer());
            }
        }

        private IEnumerator StartingCustomer()
        {
            float waitCounter = 0;
            int customerAnimationInfoIndex = 0;
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
            }

            OnCustomerLeaves?.Invoke();
            //m_CurrentCustomer.PerformanceDone(m_CostProperty);
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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
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
