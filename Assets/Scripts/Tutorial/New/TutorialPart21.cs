using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Isometric.UI;
using Isometric.Cam;
using Isometric.Customer;
using Isometric.Environment;
using Isometric.Player;
using Isometric.Data;
using System;

namespace Isometric.Tutorial
{
    public class TutorialPart21 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;
        [SerializeField] DataConsumable m_TargetOrderType;
        [SerializeField] List<MainServiceController> m_Beds;
        [SerializeField] GameObject m_GestureTap;
        [SerializeField] GameObject m_GestureArrow;
        [SerializeField] Vector2 m_ArrowOffset;

        private MainServiceCustomerHandler m_TargetCustomerHandler;
        private bool m_HasTargetOrderDelivered = false;
        

        public override void Play()
        {
            StartCoroutine(CheckForTargetOrderQuery());
        }
        
        private IEnumerator CheckForTargetOrderQuery()
        {
            while (true)
            {
                Tuple<DataConsumable, Vector3> target = null;
                CustomerSalonController targetCustomer = null;

                foreach (var bed in m_Beds)
                {
                    List<Tuple<DataConsumable, Vector3>> orders = bed.GetCurrentPlayerOrders();
                    if (orders != null)
                    {
                        foreach (var order in orders)
                        {
                            if (order.Item1 == m_TargetOrderType)
                            {
                                target = order;
                                targetCustomer = bed.CustomerHandler.CurrentCustomer;
                                m_TargetCustomerHandler = bed.CustomerHandler;
                                break;
                            }
                        }
                    }

                    if (targetCustomer != null)
                    {
                        break;
                    }
                }

                yield return null;

                if (targetCustomer != null && m_TargetCustomerHandler != null)
                {
                    Step1();
                    break;
                }
            }
        }

        private void Step1()
        {
            m_TargetCustomerHandler.MainServiceController.OnOrderItemsDelivered += SetTargetOrderDeliveredStatus;
            m_Holder.SetActive(true);
            m_GestureTap.SetActive(true);
            m_GestureArrow.transform.position = (Vector2)m_TargetCustomerHandler.transform.position + m_ArrowOffset;
            m_GestureArrow.SetActive(true);
            StartCoroutine(CheckForOrderDelivery());
        }

        private IEnumerator CheckForOrderDelivery()
        {
            yield return new WaitWhile(() => m_HasTargetOrderDelivered == false);
            End();
        }

        private void SetTargetOrderDeliveredStatus(List<DataConsumable> deliveredOrders)
        {
            if (deliveredOrders.Contains(m_TargetOrderType))
            {
                m_HasTargetOrderDelivered = true;
                m_TargetCustomerHandler.MainServiceController.OnOrderItemsDelivered -= SetTargetOrderDeliveredStatus;
            }
        }

        private void End()
        {
            m_GestureTap.SetActive(false);
            m_GestureArrow.SetActive(false);
            m_Holder.SetActive(false);
            m_HasTargetOrderDelivered = false;
            Stop();
        }
    }
}
