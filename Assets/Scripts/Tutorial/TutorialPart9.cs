using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Isometric.Cam;
using Isometric.UI;
using Isometric.Customer;
using Isometric.Environment;

namespace Isometric.Tutorial
{
    public class TutorialPart9 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;

        [Header("---Step1---")]
        [SerializeField] float m_CustomerInteractionDelay;
        private CustomerSalonController m_TargetCustomer = null;

        [Header("---Step2---")]
        [SerializeField] List<Collider2D> m_OtherTableColliders;
        [SerializeField] GameObject m_GestureStep2;
        [SerializeField] List<SalonChairCustomerHandler> m_Tables;
        [SerializeField] Transform m_DragFocusPosition;
        [SerializeField] Vector2 m_DragFocusSize;
        [SerializeField] FocusShapeType m_DragFocusShape;

        [Header("---Step3---")]
        [SerializeField] GameObject m_GestureStep3;
        [SerializeField] SpriteButton m_TableCallWorker;
        [SerializeField] Transform m_TableFocus;
        [SerializeField] float m_TableFocusScale;
        [SerializeField] Vector2 m_TableFocusOffset;

        [Header("---Step4---")]
        [SerializeField] float m_Step4Delay;
        [SerializeField] EventTrigger m_Kitchen1Trigger;
        private EventTrigger.Entry m_Kitchen1TriggerEntery = null;
        [SerializeField] float m_Kitchen1Size;
        [SerializeField] Vector2 m_Kitchen1Offset;
        [SerializeField] Collider2D m_Kitchen1Collider;
        [SerializeField] List<Collider2D> m_Kitchen1OtherColliders;
        [SerializeField] GameObject m_DialogStep4;

        [Header("---Step5---")]
        [SerializeField] float m_Step5Delay;
        [SerializeField] EventTrigger m_FishBoxTrigger;
        private EventTrigger.Entry m_FishBoxTriggerEntry = null;
        [SerializeField] float m_FishBoxSize;
        [SerializeField] Vector2 m_FishBoxOffset;
        [SerializeField] Collider2D m_FishBoxCollider;
        [SerializeField] GameObject m_GestureStep5;
        [SerializeField] GameObject m_DialogStep5;

        [Header("---Step6---")]
        [SerializeField] float m_Step6Delay;
        [SerializeField] GameObject m_DialogStep6;
        [SerializeField] GameObject m_GestureStep6;

        [Header("---Step7---")]
        [SerializeField] float m_Step7Duration;
        [SerializeField] GameObject m_DialogStep7;


        [Header("---Step9---")]
        [SerializeField] float m_Step9Delay;
        [SerializeField] EventTrigger m_TabeTrigger;
        EventTrigger.Entry m_TableTriggerEntry = null;
        [SerializeField] GameObject m_DialogStep9;

        public override void Play()
        {
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
            
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);

            m_TargetCustomer.FreezeWait();

            CustomerManager.CustomerGenerationOff();
            yield return new WaitForSeconds(m_CustomerInteractionDelay);

            Step2();
        }

        public void Step2()
        {
            TutorialFocusManager.StopAllFocus();
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_DragFocusPosition.position, Vector2.zero, m_DragFocusSize, m_DragFocusShape);

            CameraController.SetEnvironemntInteractiblity(true);
            m_GestureStep2.SetActive(true);

            foreach (var collider in m_OtherTableColliders)
            {
                collider.enabled = false;
            }

            foreach (var table in m_Tables)
            {
                table.TutorialLock(true);
            }

            StartCoroutine(Steping2());
        }

        IEnumerator Steping2()
        {
            yield return new WaitWhile(() => m_TargetCustomer.IsCustomerOnSalonChair() == false);

            m_GestureStep2.SetActive(false);

            Step3();
        }

        public void Step3()
        {
            TutorialFocusManager.StopAllFocus();
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_TableFocus.position, m_TableFocusOffset, Vector2.one * m_TableFocusScale, FocusShapeType.Circle);
            m_GestureStep3.SetActive(true);
            m_TableCallWorker.OnClick.AddListener(Step3Next);
        }

        public void Step3Next()
        {
            m_GestureStep3.SetActive(false);
            m_TableCallWorker.OnClick.RemoveListener(Step3Next);

            foreach (var collider in m_OtherTableColliders)
            {
                collider.enabled = true;
            }

            foreach (var table in m_Tables)
            {
                table.TutorialLock(false);
            }

            Step4();
        }

        public void Step4()
        {
            CameraController.SetEnvironemntInteractiblity(false);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            CoroutineManager.LateAction(() =>
            {
                TutorialFocusManager.FocusAtPosition(m_Kitchen1Trigger.transform.position, m_Kitchen1Offset, Vector2.one * m_Kitchen1Size, FocusShapeType.Circle);
                TutorialFocusManager.SetBackgroundButtonCallback(Step5);

                m_DialogStep4.SetActive(true);
            }, m_Step4Delay);
        }

        public void Step5()
        {
            m_DialogStep4.SetActive(false);

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            m_Kitchen1Collider.enabled = false;

            foreach (var collider in m_Kitchen1OtherColliders)
            {
                collider.enabled = false;
            }

            CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
                TutorialFocusManager.FocusAtPosition(m_FishBoxTrigger.transform.position, m_FishBoxOffset, Vector2.one * m_FishBoxSize, FocusShapeType.Circle);

                m_GestureStep5.SetActive(true);
                m_DialogStep5.SetActive(true);

                m_FishBoxTriggerEntry = m_FishBoxTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
                if (m_FishBoxTriggerEntry != null)
                {
                    m_FishBoxTriggerEntry.callback.AddListener(Step6);
                }
            }, m_Step5Delay);
        }


        public void Step6(BaseEventData eventData)
        {
            if (m_FishBoxTriggerEntry != null)
            {
                m_FishBoxTriggerEntry.callback.RemoveListener(Step6);
                m_FishBoxTriggerEntry = null;
            }

            m_GestureStep5.SetActive(false);
            m_DialogStep5.SetActive(false);

            m_FishBoxCollider.enabled = false;

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            CameraController.SetEnvironemntInteractiblity(false);

            CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
                TutorialFocusManager.FocusAtPosition(m_Kitchen1Trigger.transform.position, m_Kitchen1Offset, Vector2.one * m_Kitchen1Size, FocusShapeType.Circle);

                m_DialogStep6.SetActive(true);
                m_GestureStep6.SetActive(true);

                m_Kitchen1Collider.enabled = true;

                m_Kitchen1TriggerEntery = m_Kitchen1Trigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
                if (m_Kitchen1TriggerEntery != null)
                {
                    m_Kitchen1TriggerEntery.callback.AddListener(Step7);
                }
            }, m_Step6Delay);
        }

        public void Step7(BaseEventData eventData)
        {
            if (m_Kitchen1TriggerEntery != null)
            {
                m_Kitchen1TriggerEntery.callback.RemoveListener(Step7);
                m_Kitchen1TriggerEntery = null;
            }

            m_DialogStep6.SetActive(false);
            m_GestureStep6.SetActive(false);

            CameraController.SetEnvironemntInteractiblity(false);

            m_DialogStep7.SetActive(true);

            CoroutineManager.LateAction(Step8, m_Step7Duration);

        }

        public void Step8()
        {
            m_DialogStep7.SetActive(false);
            CameraController.SetEnvironemntInteractiblity(true);

            m_DialogStep6.SetActive(true);
            m_GestureStep6.SetActive(true);

            m_Kitchen1TriggerEntery = m_Kitchen1Trigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
            if (m_Kitchen1TriggerEntery != null)
            {
                m_Kitchen1TriggerEntery.callback.AddListener(Step9);
            }
        }

        public void Step9(BaseEventData eventData)
        {

            if (m_Kitchen1TriggerEntery != null)
            {
                m_Kitchen1TriggerEntery.callback.RemoveListener(Step9);
                m_Kitchen1TriggerEntery = null;
            }

            m_DialogStep6.SetActive(false);
            m_GestureStep6.SetActive(false);

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            CameraController.SetEnvironemntInteractiblity(false);

            CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
                TutorialFocusManager.FocusAtPosition(m_TabeTrigger.transform.position, m_TableFocusOffset, Vector2.one * m_TableFocusScale, FocusShapeType.Circle);
                m_DialogStep9.SetActive(true);

                m_TableTriggerEntry = m_TabeTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
                if (m_TableTriggerEntry != null)
                {
                    m_TableTriggerEntry.callback.AddListener(Step10);
                }
            }, m_Step9Delay);
        }

        public void Step10(BaseEventData eventData)
        {
            if (m_TableTriggerEntry != null)
            {
                m_TableTriggerEntry.callback.RemoveListener(Step10);
                m_TableTriggerEntry = null;
            }

            m_DialogStep9.SetActive(false);
            m_Holder.SetActive(false);

            m_FishBoxCollider.enabled = true;

            foreach (var collider in m_Kitchen1OtherColliders)
            {
                collider.enabled = true;
            }

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            UIManager.UIInteractionOn();
            CustomerManager.CustomerGenerationOn();

            Stop();
        }
    }
}
