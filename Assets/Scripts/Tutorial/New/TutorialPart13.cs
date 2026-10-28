using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Isometric.Cam;
using Isometric.UI;
using Isometric.Customer;
using Isometric.Environment;
using Isometric.Player;
using System;
using Isometric.Data;

namespace Isometric.Tutorial
{
    public class TutorialPart13 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;

        [Header("---Step1---")]
        [SerializeField] float m_CustomerFocusDelay;
        [SerializeField] float m_CustomerInteractionDelay;
        [SerializeField] float m_FocusOnCustomerSize;
        [SerializeField] Vector2 m_FocusOnCustomerOffset;
        private CustomerSalonController m_TargetCustomer = null;

        [Header("---Step2---")]
        [SerializeField] List<Collider2D> m_OtherColliders;
        [SerializeField] List<Collider2D> m_PatientSampleColliders;
        [SerializeField] GameObject m_GestureStep2;
        [SerializeField] MainServiceCustomerHandler m_TargetBed;
        [SerializeField] List<MainServiceCustomerHandler> m_Beds;

        [Header("---Step3---")]
        [SerializeField] GameObject m_GestureStep3;
        [SerializeField] SpriteButton m_BedCallWorker;
        [SerializeField] Transform m_BedFocus;
        [SerializeField] float m_BedFocusScale;
        [SerializeField] Vector2 m_BedFocusOffset;

        [Header("---Step4---")]
        // [SerializeField] float m_Step4Delay;
        [SerializeField] DataConsumable m_TargetOrderType;
        [SerializeField] EventTrigger m_ResearchCenterTrigger;
        private EventTrigger.Entry m_ResearchCenterTriggerEntery = null;
        [SerializeField] float m_ResearchCenterSize;
        [SerializeField] Vector2 m_ResearchCenterOffset;
        [SerializeField] Collider2D m_ResearchCenterCollider;
        [SerializeField] GameObject m_DialogStep4;

        [Header("---Step5---")]
        // [SerializeField] float m_Step5Delay;
        [SerializeField] EventTrigger m_PatientSampleTrigger;
        private EventTrigger.Entry m_PatientSampleTriggerEntry = null;
        [SerializeField] float m_PatientSampleSize;
        [SerializeField] Vector2 m_PatientSampleOffset;
        [SerializeField] Collider2D m_PatientSampleCollider;
        [SerializeField] GameObject m_GestureStep5;
        [SerializeField] GameObject m_DialogStep5;

        [Header("---Step6---")]
        [SerializeField] float m_Step6Delay;
        [SerializeField] GameObject m_DialogStep6;
        [SerializeField] GameObject m_GestureStep6;

        [Header("---Step7---")]
        // [SerializeField] float m_Step7Duration;
        [SerializeField] StationSerialOrderInOutOnCustomerDemand m_ResearchStation;
        [SerializeField] GameObject m_DialogStep7;

        [Header("---Step8---")]
        [SerializeField] GameObject m_DialogStep8;
        [SerializeField] GameObject m_GestureStep8;


        [Header("---Step9---")]
        // [SerializeField] float m_Step9Delay;
        [SerializeField] EventTrigger m_BedTrigger;
        [SerializeField] Collider2D m_BedCollider;
        EventTrigger.Entry m_BedTriggerEntry = null;
        [SerializeField] GameObject m_DialogStep9;

        private PlayerController m_Player;

        public override void Play()
        {
            m_Player = PlayerManager.GetPlayerAtIndex(0);
            Step1();
        }

        public void Step1()
        {
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
            
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);

            m_TargetCustomer.FreezeWait();
            CustomerManager.CustomerGenerationOff();

            yield return new WaitForSeconds(m_CustomerFocusDelay);
            m_Player.PauseGreetingReaction();
            TutorialFocusManager.FocusAtTransform(m_TargetCustomer.transform, m_FocusOnCustomerOffset, Vector2.one * m_FocusOnCustomerSize, FocusShapeType.Circle);
            yield return new WaitForSeconds(m_CustomerInteractionDelay);

            Step2();
        }

        public void Step2()
        {
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_BedFocus.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);

            CameraController.SetEnvironemntInteractiblity(true);
            m_GestureStep2.SetActive(true);

            foreach (var collider in m_OtherColliders)
            {
                collider.enabled = false;
            }

            foreach (var collider in m_PatientSampleColliders)
            {
                collider.enabled = false;
            }

            foreach (var table in m_Beds)
            {
                table.TutorialLock(true);
            }

            StartCoroutine(Steping2());
        }

        IEnumerator Steping2()
        {
            m_TargetBed.TutorialLock(false);
            m_BedCollider.enabled = true;
            yield return new WaitWhile(() => m_TargetCustomer.IsCustomerOnSalonChair() == false);

            m_GestureStep2.SetActive(false);
            TutorialFocusManager.StopAllFocusInstantly();

            Step3();
        }

        public void Step3()
        {
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_BedFocus.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
            m_GestureStep3.SetActive(true);
            m_BedCallWorker.OnClick.AddListener(Step3Next);
        }

        public void Step3Next()
        {
            m_GestureStep3.SetActive(false);
            m_BedCollider.enabled = false;
            m_BedCallWorker.OnClick.RemoveListener(Step3Next);
            CameraController.SetEnvironemntInteractiblity(false);

            Step4();
        }

        public void Step4()
        {
            CameraController.SetEnvironemntInteractiblity(false);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            StartCoroutine(Stepping4());
        }
        IEnumerator Stepping4()
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

            TutorialFocusManager.FocusAtPosition(m_ResearchCenterTrigger.transform.position, m_ResearchCenterOffset, Vector2.one * m_ResearchCenterSize, FocusShapeType.Circle);
            TutorialFocusManager.SetBackgroundButtonCallback(Step5);
            TutorialManager.ActivateTapToContinueText(TutorailTextPosition.Down);

            m_DialogStep4.SetActive(true);
        }

        public void Step5()
        {
            m_DialogStep4.SetActive(false);

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocusInstantly();
            TutorialManager.DeactivateTapToContinueText();

            CameraController.SetEnvironemntInteractiblity(true);
            TutorialFocusManager.FocusAtPosition(m_PatientSampleTrigger.transform.position, m_PatientSampleOffset, Vector2.one * m_PatientSampleSize, FocusShapeType.Circle);

            m_GestureStep5.SetActive(true);
            m_DialogStep5.SetActive(true);
            m_PatientSampleCollider.enabled = true;

            m_PatientSampleTriggerEntry = m_PatientSampleTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
            if (m_PatientSampleTriggerEntry != null)
            {
                m_PatientSampleTriggerEntry.callback.AddListener(Step6);
            }
            /* CoroutineManager.LateAction(() =>
            {
            }, m_Step5Delay); */
        }


        public void Step6(BaseEventData eventData)
        {
            if (m_PatientSampleTriggerEntry != null)
            {
                m_PatientSampleTriggerEntry.callback.RemoveListener(Step6);
                m_PatientSampleTriggerEntry = null;
            }

            m_GestureStep5.SetActive(false);
            m_DialogStep5.SetActive(false);
            m_PatientSampleCollider.enabled = false;

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            CameraController.SetEnvironemntInteractiblity(false);

            CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
                TutorialFocusManager.FocusAtPosition(m_ResearchCenterTrigger.transform.position, m_ResearchCenterOffset, Vector2.one * m_ResearchCenterSize, FocusShapeType.Circle);

                m_DialogStep6.SetActive(true);
                m_GestureStep6.SetActive(true);
                m_ResearchCenterCollider.enabled = true;
                m_BedCollider.enabled = false;

                m_ResearchCenterTriggerEntery = m_ResearchCenterTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
                if (m_ResearchCenterTriggerEntery != null)
                {
                    m_ResearchCenterTriggerEntery.callback.AddListener(Step7);
                }
            }, m_Step6Delay);
        }

        public void Step7(BaseEventData eventData)
        {
            if (m_ResearchCenterTriggerEntery != null)
            {
                m_ResearchCenterTriggerEntery.callback.RemoveListener(Step7);
                m_ResearchCenterTriggerEntery = null;
            }

            m_DialogStep6.SetActive(false);
            m_GestureStep6.SetActive(false);

            CameraController.SetEnvironemntInteractiblity(false);

            CoroutineManager.LateAction(() =>
            {
                m_DialogStep7.SetActive(true);
                CoroutineManager.LateAction(Step8, m_ResearchStation.DurationProperty + 2f);
            }, 2.75f);

        }

        public void Step8()
        {
            m_DialogStep7.SetActive(false);
            CameraController.SetEnvironemntInteractiblity(true);

            m_DialogStep8.SetActive(true);
            m_GestureStep8.SetActive(true);

            m_ResearchCenterTriggerEntery = m_ResearchCenterTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
            if (m_ResearchCenterTriggerEntery != null)
            {
                m_ResearchCenterTriggerEntery.callback.AddListener(Step9);
            }
        }

        public void Step9(BaseEventData eventData)
        {

            if (m_ResearchCenterTriggerEntery != null)
            {
                m_ResearchCenterTriggerEntery.callback.RemoveListener(Step9);
                m_ResearchCenterTriggerEntery = null;
            }

            m_DialogStep8.SetActive(false);
            m_GestureStep8.SetActive(false);
            m_ResearchCenterCollider.enabled = false;

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocusInstantly();

            TutorialFocusManager.FocusAtPosition(m_BedTrigger.transform.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
            m_DialogStep9.SetActive(true);
            m_GestureStep3.SetActive(true);
            m_BedCollider.enabled = true;

            m_BedTriggerEntry = m_BedTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
            if (m_BedTriggerEntry != null)
            {
                m_BedTriggerEntry.callback.AddListener(Step10);
            }
            /* CoroutineManager.LateAction(() =>
            {
            }, m_Step9Delay); */
        }

        public void Step10(BaseEventData eventData)
        {
            if (m_BedTriggerEntry != null)
            {
                m_BedTriggerEntry.callback.RemoveListener(Step10);
                m_BedTriggerEntry = null;
            }

            m_DialogStep9.SetActive(false);
            m_GestureStep3.SetActive(false);
            m_Holder.SetActive(false);

            foreach (var collider in m_OtherColliders)
            {
                collider.enabled = true;
            }

            foreach (var collider in m_PatientSampleColliders)
            {
                collider.enabled = false;
            }

            foreach (var table in m_Beds)
            {
                table.TutorialLock(false);
            }

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            UIManager.UIInteractionOn();
            CustomerManager.CustomerGenerationOn();
            m_Player.UnpauseGreetingReaction();

            Stop();
        }
    }
}
