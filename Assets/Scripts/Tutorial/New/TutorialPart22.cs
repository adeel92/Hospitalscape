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
using DG.Tweening;

namespace Isometric.Tutorial
{
    public class TutorialPart22 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;
        [SerializeField] DataConsumable m_TargetOrderType;
        [SerializeField] StationCustomerInHandler m_TargetStationHandler;
        [SerializeField] List<MainServiceController> m_Beds;
        [SerializeField] GameObject m_GestureDrag;
        [SerializeField] TrailRenderer m_GestureDragTrailRenderer;
        [SerializeField] Vector2 m_GestureDragOffset;
        [SerializeField] float m_GestureDragMoveDuration;

        private MainServiceCustomerHandler m_TargetCustomerHandler;
        private Coroutine m_CheckForOrderDeliveryCoroutine = null;
        private bool m_HasCustomerDroppedOnStation = false;
        

        public override void Play()
        {
            GlobalEventHolder.OnInstanceOrderFillBooster += OnInstantOrderFillBoosterUsed;
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
            m_TargetStationHandler.OnCustomerDropped += SetCustomerDroppingStatus;
            m_Holder.SetActive(true);
            m_GestureDrag.transform.position = (Vector2)m_TargetCustomerHandler.transform.position + m_GestureDragOffset;
            m_GestureDrag.SetActive(true);
            m_GestureDrag.transform.DOMove(m_TargetStationHandler.transform.position, m_GestureDragMoveDuration).SetLoops(-1, LoopType.Restart).OnStepComplete(() =>
            {
                m_GestureDragTrailRenderer.Clear();
            });
            m_CheckForOrderDeliveryCoroutine = StartCoroutine(CheckForOrderDelivery());
        }

        private IEnumerator CheckForOrderDelivery()
        {
            yield return new WaitWhile(() => m_HasCustomerDroppedOnStation == false);
            End();
        }

        private void SetCustomerDroppingStatus(CustomerSalonController customer)
        {
            m_HasCustomerDroppedOnStation = true;
            m_TargetStationHandler.OnCustomerDropped -= SetCustomerDroppingStatus;
        }

        private void End()
        {
            GlobalEventHolder.OnInstanceOrderFillBooster -= OnInstantOrderFillBoosterUsed;
            m_GestureDrag.transform.DOKill();
            m_GestureDrag.SetActive(false);
            m_Holder.SetActive(false);
            m_HasCustomerDroppedOnStation = false;
            Stop();
        }

        private void OnInstantOrderFillBoosterUsed()
        {
            if(m_CheckForOrderDeliveryCoroutine != null)
            {
                StopCoroutine(m_CheckForOrderDeliveryCoroutine);
                m_CheckForOrderDeliveryCoroutine = null;
                End();
            }
        }
    }
}
