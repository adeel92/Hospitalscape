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
    public class TutorialPart14 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;
        [SerializeField] List<Collider2D> m_OtherColliders;

        [Header("---Step1---")]
        [SerializeField] float m_CustomerFocusDelay;
        [SerializeField] float m_CustomerInteractionDelay;
        [SerializeField] float m_FocusOnCustomerSize;
        [SerializeField] Vector2 m_FocusOnCustomerOffset;
        private CustomerSalonController m_TargetCustomer = null;

        [Header("---Step2---")]
        [SerializeField] MainServiceCustomerHandler m_TargetBed;
        [SerializeField] List<MainServiceCustomerHandler> m_Beds;
        [SerializeField] GameObject m_DragGestureStep2;

        [Header("---Step3---")]
        [SerializeField] EventTrigger m_BedTrigger;
        [SerializeField] Collider2D m_BedCollider;
        EventTrigger.Entry m_BedTriggerEntry = null;
        [SerializeField] Transform m_BedFocus;
        [SerializeField] float m_BedFocusScale;
        [SerializeField] Vector2 m_BedFocusOffset;
        [SerializeField] SpriteButton m_SpriteButtonStep3;
        [SerializeField] GameObject m_GestureStep3;

        [Header("---Step4---")]
        [SerializeField] DataConsumable m_TargetOrderType;
        [SerializeField] GameObject m_DialogStep4;
        // [SerializeField] GameObject m_GestureStep4;

        [Header("---Step5---")]
        [SerializeField] GameObject m_DialogStep5;
        [SerializeField] GameObject m_GestureStep5;
        [SerializeField] StationEngageOrderOutOnCustomerDemand m_ReportGeneratorStation;
        [SerializeField] EventTrigger m_ReportGeneratorTrigger;
        [SerializeField] Collider2D m_ReportGeneratorCollider;
        EventTrigger.Entry m_ReportGeneratorTriggerEntry = null;
        [SerializeField] float m_ReportGeneratorFocusScale;
        [SerializeField] Vector2 m_ReportGeneratorFocusOffset;

        [Header("---Step6---")]
        [SerializeField] GameObject m_DialogStep6;
        [SerializeField] GameObject m_DialogStep6Hold;
        [SerializeField] GameObject m_GestureStep6;
        
        private PlayerController player = null;


        public override void Play()
        {
            player = PlayerManager.GetPlayerAtIndex(0);
            Step1();
        }

        public void Step1()
        {
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);

            m_Holder.SetActive(true);

            UIManager.UIInteractionOff();
            CameraController.SetEnvironemntInteractiblity(false);

            StartCoroutine(Steping1());
        }

        IEnumerator Steping1()
        {
            while (m_TargetCustomer == null)
            {
                yield return null;
                CustomerSalonController customer = CustomerManager.GetSalonCustomerInQueue(0);

                if (customer != null)
                {
                    m_TargetCustomer = customer;
                }
            }

            m_TargetCustomer.FreezeWait();
            CustomerManager.CustomerGenerationOff();

            yield return new WaitForSeconds(m_CustomerFocusDelay);
            player.PauseGreetingReaction();
            TutorialFocusManager.FocusAtTransform(m_TargetCustomer.transform, m_FocusOnCustomerOffset, Vector2.one * m_FocusOnCustomerSize, FocusShapeType.Circle);
            yield return new WaitForSeconds(m_CustomerInteractionDelay);

            Step2();
        }

        public void Step2()
        {
            foreach (var bed in m_Beds)
            {
                bed.TutorialLock(true);
            }

            foreach (var collider in m_OtherColliders)
            {
                collider.enabled = false;
            }

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_BedFocus.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
            m_DragGestureStep2.SetActive(true);

            CameraController.SetEnvironemntInteractiblity(true);

            StartCoroutine(Steping2());
        }

        IEnumerator Steping2()
        {
            m_BedCollider.enabled = true;
            m_TargetBed.TutorialLock(false);
            yield return new WaitWhile(() => m_TargetCustomer.IsCustomerOnSalonChair() == false);
            
            m_DragGestureStep2.SetActive(false);
            TutorialFocusManager.StopAllFocusInstantly();
            
            Step3();
        }

        public void Step3()
        {
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_BedFocus.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
            m_GestureStep3.SetActive(true);
            m_SpriteButtonStep3.OnClick.AddListener(Step3Next);
        }

        public void Step3Next()
        {
            m_SpriteButtonStep3.OnClick.RemoveListener(Step3Next);

            /* foreach (var collider in m_OtherCollider)
            {
                collider.enabled = true;
            } */
            /* foreach (var table in m_Beds)
            {
                table.TutorialLock(false);
            } */
            m_BedCollider.enabled = false;
            m_GestureStep3.SetActive(false);
            CameraController.SetEnvironemntInteractiblity(false);
            TutorialFocusManager.StopAllFocus();

            StartCoroutine(Step4());
        }

        public IEnumerator Step4()
        {
            while (true)
            {
                Tuple<DataConsumable, Vector3> target = null;
                CustomerSalonController targetCustomer = null;

                List<Tuple<DataConsumable, Vector3>> orders = m_TargetBed.MainServiceController.GetCurrentPlayerOrders();
                if (orders != null)
                {
                    foreach (var order in orders)
                    {
                        if (order.Item1 == m_TargetOrderType)
                        {
                            target = order;
                            targetCustomer = m_TargetBed.CurrentCustomer;
                            break;
                        }
                    }
                }

                yield return null;

                if (targetCustomer != null)
                {
                    break;
                }
            }

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_BedFocus.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
            m_DialogStep4.SetActive(true);
            TutorialFocusManager.SetBackgroundButtonCallback(Step5);
            TutorialManager.ActivateTapToContinueText(TutorailTextPosition.Down);
        }

        public void Step5()
        {
            TutorialManager.DeactivateTapToContinueText();
            m_DialogStep4.SetActive(false);
            CameraController.SetEnvironemntInteractiblity(true);
            TutorialFocusManager.FocusAtPosition(m_ReportGeneratorTrigger.transform.position, m_ReportGeneratorFocusOffset, Vector2.one * m_ReportGeneratorFocusScale, FocusShapeType.Circle);

            m_DialogStep5.SetActive(true);
            m_GestureStep5.SetActive(true);
            m_ReportGeneratorCollider.enabled = true;

            m_ReportGeneratorTriggerEntry = m_ReportGeneratorTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
            if (m_ReportGeneratorTriggerEntry != null)
            {
                m_ReportGeneratorTriggerEntry.callback.AddListener(Step5Next);
            }
            /* CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
            }, m_DelayStep7); */
        }

        public void Step5Next(BaseEventData eventData)
        {
            if (m_ReportGeneratorTriggerEntry != null)
            {
                m_ReportGeneratorTriggerEntry.callback.RemoveListener(Step5Next);
                m_ReportGeneratorTriggerEntry = null;
            }

            m_DialogStep5.SetActive(false);
            m_GestureStep5.SetActive(false);
            m_ReportGeneratorCollider.enabled = false;
            TutorialFocusManager.StopAllFocus();

            Step6();
        }

        public void Step6()
        {
            CameraController.SetEnvironemntInteractiblity(false);
            StartCoroutine(Stepping6());
        }
        IEnumerator Stepping6()
        {
            yield return new WaitWhile(() => m_ReportGeneratorStation.IsProcessing == false);

            m_DialogStep6Hold.SetActive(true);
            TutorialFocusManager.FocusAtPosition(m_ReportGeneratorTrigger.transform.position, m_ReportGeneratorFocusOffset, Vector2.one * m_ReportGeneratorFocusScale, FocusShapeType.Circle);

            CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
                TutorialFocusManager.StopAllFocusInstantly();
                TutorialFocusManager.FocusAtPosition(m_BedTrigger.transform.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
                m_DialogStep6Hold.SetActive(false);
                m_DialogStep6.SetActive(true);
                m_GestureStep6.SetActive(true);
                m_BedCollider.enabled = true;

                m_BedTriggerEntry = m_BedTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
                if (m_BedTriggerEntry != null)
                {
                    m_BedTriggerEntry.callback.AddListener(Step7);
                }
            }, m_ReportGeneratorStation.DurationProperty + 0.5f);
        }

        public void Step7(BaseEventData eventData)
        {
            if (m_BedTriggerEntry != null)
            {
                m_BedTriggerEntry.callback.RemoveListener(Step7);
                m_BedTriggerEntry = null;
            }

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            m_DialogStep6.SetActive(false);
            m_GestureStep6.SetActive(false);
            m_Holder.SetActive(false);

            foreach (var bed in m_Beds)
            {
                bed.TutorialLock(false);
            }

            foreach (var collider in m_OtherColliders)
            {
                collider.enabled = true;
            }

            player.UnpauseGreetingReaction();
            UIManager.UIInteractionOn();
            CustomerManager.CustomerGenerationOn();
            Stop();
        }
    }
}
