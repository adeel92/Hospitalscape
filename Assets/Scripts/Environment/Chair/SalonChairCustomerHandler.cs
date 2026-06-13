using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using NaughtyAttributes;
using Isometric.Customer;
using Isometric.Data;

namespace Isometric.Environment
{
    public class SalonChairCustomerHandler : MonoBehaviour
    {
        [Header("---Setup---")]
        [SerializeField] SalonChairController m_SalonChairController;

        [SerializeField] float m_CaptureDistance;
        [SerializeField] float m_ExitDistance;

        [Space]
        [SerializeField] Transform m_CustomerSittingHolder;
        [SerializeField, SortingLayer] string m_SittingSortingLayer;
        [SerializeField] int m_SittingSortingOrder;
        [SerializeField] int m_SittingSortingOrderLeave;
        [SerializeField] Transform m_CustomerLeaveHolder;


        [Space, SerializeField, ReadOnly]
        private CustomerSalonController m_CurrentCustomer;
        public CustomerSalonController CurrentCustomer => m_CurrentCustomer;

        private bool m_LockForTutorail = false;
        Coroutine m_DectectionUpdate = null;

        public void SetupForGameplay()
        {
            if(m_DectectionUpdate != null)
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

                if (m_LockForTutorail == false
                    && m_CurrentCustomer == null 
                    && m_SalonChairController.CanSitOnTheSalonChair())
                {
                    List<CustomerSalonController> salonCustomers = CustomerManager.GetAllSalonCustomer();

                    foreach (var salonCustomer in salonCustomers)
                    {
                        if (salonCustomer != null 
                            && !salonCustomer.IsCustomerOnSalonChair()
                            && !salonCustomer.IsCustomerFirstOrderUndecided())
                        {
                            float sqrDist = (salonCustomer.transform.position - transform.position).sqrMagnitude;

                            if (sqrDist <= sqrCaptureDistance)
                            {
                                m_CurrentCustomer = salonCustomer;
                                m_CurrentCustomer.transform.SetParent(m_CustomerSittingHolder);
                                m_CurrentCustomer.transform.localPosition = Vector3.zero;
                                m_CurrentCustomer.SetSortingLayer(m_SittingSortingLayer, m_SittingSortingOrder);
                                // m_CurrentCustomer.SitOnSalonChair(this); //mrcHefF
                                yield break;
                            }
                        }
                    }
                }

            }
        }


        public void ResitOnSalonChair()
        {
            if (m_CurrentCustomer != null)
            {
                m_CurrentCustomer.transform.SetParent(m_CustomerSittingHolder);
                m_CurrentCustomer.transform.localPosition = Vector3.zero;
                m_CurrentCustomer.SetSortingLayer(m_SittingSortingLayer, m_SittingSortingOrder);
            }
        }

        public void CustomerDragedRemoved()
        {
            m_CurrentCustomer = null;

            if (m_DectectionUpdate != null)
            {
                StopCoroutine(m_DectectionUpdate);
                m_DectectionUpdate = null;
            }
            m_DectectionUpdate = StartCoroutine(DectectionUpdate());
        }

        public void SitOnTheSalonChair()
        {
            m_SalonChairController.SitOnTheSalonChair();
        }

        public void CustomerLeaves()
        {
            m_CurrentCustomer.transform.SetParent(m_CustomerLeaveHolder);
            m_CurrentCustomer.transform.localPosition = Vector3.zero;
            m_CurrentCustomer.transform.SetParent(null);
            m_CurrentCustomer.SetSortingLayer(m_SittingSortingLayer, m_SittingSortingOrderLeave);
            GameObject customer = m_CurrentCustomer.gameObject;

            CustomerManager.RemoveFromQueue(m_CurrentCustomer);
            m_CurrentCustomer.SetAnimationStateHappy(() =>
            {
                Destroy(customer);
            });


            if (m_DectectionUpdate != null)
            {
                StopCoroutine(m_DectectionUpdate);
                m_DectectionUpdate = null;
            }
            m_DectectionUpdate = StartCoroutine(DectectionUpdate());

            m_CurrentCustomer = null;
        }

        public void ShowNextOrder()
        {
            m_SalonChairController.ShowNextOrder();
        }

        #region Helper
        public CustomerSalonController GetCurrentCustomer()
        {
            return m_CurrentCustomer;
        }

        public SalonChairController GetSalonChair()
        {
            return m_SalonChairController;
        }

        public float GetExitDistance()
        {
            return m_ExitDistance;
        }
        #endregion

        #region Tutorail
        public void TutorialLock(bool value)
        {
            m_LockForTutorail = value;
        }
        #endregion

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
