using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using DG.Tweening;
using Isometric.Data;
using Isometric.Environment;
using Isometric.Customer;
using Isometric.UI;

namespace Isometric.Tutorial
{
    public class TutorialPart12 : TutorialContainer
    {
        [SerializeField] List<SalonChairController> m_Tables;
        [SerializeField] DataConsumable m_TargetFoodType;
        [SerializeField] Vector2 m_CustomerFocusSize;
        [SerializeField] Vector2 m_CustomerFocusOffset;
        [SerializeField] StationPerformance m_StationPerformance;
        [SerializeField] Vector2 m_StationPerformanceFocusSize;
        [SerializeField] Vector2 m_StationPerformanceFocusOffset;
        [SerializeField] GameObject m_Dialog;
        [SerializeField] Transform m_DragGesture;
        [SerializeField] float m_DragGestureMoveDuration;
        [SerializeField] Vector2 m_DragGestuerStartOffset;

        public override void Play()
        {
            StartCoroutine(CheckPerformanceOrder());
        }

        IEnumerator CheckPerformanceOrder()
        {
            while (true)
            {
                Tuple<DataConsumable, Vector3> target = null;
                CustomerSalonController targetCustomer = null;
                foreach (var table in m_Tables)
                {
                    List<Tuple<DataConsumable, Vector3>> orders = table.GetCurrentPlayerOrders();
                    if (orders != null)
                    {
                        foreach (var order in orders)
                        {
                            if (order.Item1 == m_TargetFoodType)
                            {
                                target = order;
                                targetCustomer = table.CustomerHandler.CurrentCustomer;
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

                if (targetCustomer != null)
                {
                    DragGesture(targetCustomer.transform);
                    break;
                }
            }
        }

        private void DragGesture(Transform startPosition)
        {
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);

            LevelManager.SetTimerLock(true);
            UIManager.UIInteractionOff();
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);

            TutorialFocusManager.StopAllFocus();
            TutorialFocusManager.RemoveBackgroundButtonCallback();

            TutorialFocusManager.FocusAtTransform(startPosition, m_CustomerFocusOffset, m_CustomerFocusSize, FocusShapeType.Circle);
            TutorialFocusManager.FocusAtPosition(m_StationPerformance.transform.position, m_StationPerformanceFocusOffset, m_StationPerformanceFocusSize, FocusShapeType.Circle);

            m_DragGesture.position = (Vector2)startPosition.position + m_DragGestuerStartOffset;
            m_DragGesture.DOMove(m_StationPerformance.transform.position, m_DragGestureMoveDuration).SetLoops(-1, LoopType.Restart);

            m_DragGesture.gameObject.SetActive(true);
            m_Dialog.SetActive(true);

            StartCoroutine(DragComplete());
        }

        IEnumerator DragComplete()
        {
            //yield return new WaitWhile(() => m_StationPerformance.CurrentCustomer == null);
            yield return null;
            Complete();
        }

        private void Complete()
        {
            TutorialFocusManager.StopAllFocus();

            m_DragGesture.DOKill();
            m_DragGesture.gameObject.SetActive(false);
            m_Dialog.SetActive(false);

            LevelManager.SetTimerLock(false);
            UIManager.UIInteractionOn();
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(false);

            Stop();
        }
    }
}
