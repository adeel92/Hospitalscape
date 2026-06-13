using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Isometric.UI;
using Isometric.Cam;
using Isometric.Customer;
using Isometric.Environment;

namespace Isometric.Tutorial
{
    public class TutorialPart2 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;

        [Header("---Step1---")]
        [SerializeField] float m_CustomerInteractionDelay;
        [SerializeField] float m_FocusOnCustomerSize;
        [SerializeField] Vector2 m_FocusOnCustomerOffset;
        private CustomerSalonController m_TargetCustomer = null;

        [Header("---Step2---")]
        [SerializeField] Transform m_TableFocus;
        [SerializeField] float m_TableFocusSize;
        [SerializeField] Vector2 m_TableOffset;
        [SerializeField] GameObject m_DialogStep2;
        [SerializeField] List<SalonChairCustomerHandler> m_Tables;
        [SerializeField] GameObject m_GestureStep2;

        [Header("---Step3---")]
        [SerializeField] SpriteButton m_SpriteButtonStep3;
        [SerializeField] GameObject m_GestureStep3;
        [SerializeField] GameObject m_DialogStep3;

        [Header("---Step4---")]
        [SerializeField] float m_Step4Delay;

        [Header("---Step5---")]
        [SerializeField] EventTrigger m_SaladEventTriggerStep4;
        [SerializeField] float m_SaladStationFocusSize;
        [SerializeField] Vector2 m_SaladStationFocusOffset;
        private EventTrigger.Entry m_EntryStep4;
        [SerializeField] GameObject m_DialogStep4;
        [SerializeField] GameObject m_GestureStep4;

        [Header("---Step6---")]
        [SerializeField] float m_Step6Delay;
        [SerializeField] GameObject m_DialogStep6;
        [SerializeField] EventTrigger m_EventTriggerStep6;
        private EventTrigger.Entry m_EntryStep6;

        [Header("---Step8---")]
        [SerializeField] GameObject m_DialogStep8;
        [SerializeField] float m_Step8Delay;

        [Header("---Step9---")]
        [SerializeField] float m_Step9Delay;
        [SerializeField] GameObject m_DialogStep9;
        [SerializeField] Transform m_GoalFocus;
        [SerializeField] Vector2 m_GoalFocusScale;
        [SerializeField] Vector2 m_GoalFocusOffset;

        public override void Play()
        {
            Step1();
        }

        public void Step1()
        {
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);

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
            TutorialFocusManager.FocusAtTransform(m_TargetCustomer.transform, m_FocusOnCustomerOffset, Vector2.one * m_FocusOnCustomerSize, FocusShapeType.Circle);
            yield return new WaitForSeconds(m_CustomerInteractionDelay);

            Step2();
        }

        public void Step2()
        {
            foreach (var table in m_Tables)
            {
                table.TutorialLock(true);
            }

            m_DialogStep2.SetActive(true);
            m_GestureStep2.SetActive(true);
            TutorialFocusManager.FocusAtPosition(m_TableFocus.position, m_TableOffset, Vector2.one * m_TableFocusSize, FocusShapeType.Circle);
            CameraController.SetEnvironemntInteractiblity(true);

            StartCoroutine(Steping2());
        }

        IEnumerator Steping2()
        {
            yield return new WaitWhile(() => m_TargetCustomer.IsCustomerOnSalonChair() == false);

            m_DialogStep2.SetActive(false);
            m_GestureStep2.SetActive(false);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            foreach (var table in m_Tables)
            {
                table.TutorialLock(false);
            }

            //yield return new WaitForSeconds(m_Step5Delay);

            Step3();
        }

        public void Step3()
        {
            TutorialFocusManager.FocusAtPosition(m_TableFocus.position, m_TableOffset, Vector2.one * m_TableFocusSize, FocusShapeType.Circle);
            m_GestureStep3.SetActive(true);
            m_DialogStep3.SetActive(true);
            m_SpriteButtonStep3.OnClick.AddListener(Step4);
        }

        public void Step4()
        {
            CameraController.SetEnvironemntInteractiblity(false);
            m_SpriteButtonStep3.OnClick.RemoveListener(Step4);

            m_DialogStep3.SetActive(false);
            m_GestureStep3.SetActive(false);

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            CoroutineManager.LateAction(Step4Delayed, m_Step4Delay);
        }

        public void Step4Delayed()
        {
            CameraController.SetEnvironemntInteractiblity(true);
            
            TutorialFocusManager.FocusAtPosition(m_SaladEventTriggerStep4.transform.position, m_SaladStationFocusOffset, Vector2.one * m_SaladStationFocusSize, FocusShapeType.Circle);
            m_DialogStep4.SetActive(true);
            m_GestureStep4.SetActive(true);

            m_EntryStep4 = m_SaladEventTriggerStep4.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);

            if (m_EntryStep4 != null)
            {
                m_EntryStep4.callback.AddListener(Step5);
            }
        }

        public void Step5(BaseEventData eventData)
        {
            if (m_EntryStep4 != null)
            {
                m_EntryStep4.callback.RemoveListener(Step5);
                m_EntryStep4 = null;
            }

            m_DialogStep4.SetActive(false);
            m_GestureStep4.SetActive(false);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            CameraController.SetEnvironemntInteractiblity(false);
            CoroutineManager.LateAction(Step6, m_Step6Delay);
        }

        public void Step6()
        {
            TutorialFocusManager.FocusAtTransform(m_TableFocus, m_TableOffset, Vector2.one * m_TableFocusSize, FocusShapeType.Circle);
            m_DialogStep6.SetActive(true);
            m_GestureStep3.SetActive(true);
            CameraController.SetEnvironemntInteractiblity(true);

            m_EntryStep6 = m_EventTriggerStep6.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
            if (m_EntryStep6 != null)
            {
                m_EntryStep6.callback.AddListener(Step7);
            }
        }

        public void Step7(BaseEventData eventData)
        {
            if (m_EntryStep6 != null)
            {
                m_EntryStep6.callback.RemoveListener(Step7);
                m_EntryStep6 = null;
            }

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            m_DialogStep6.SetActive(false);
            m_GestureStep3.SetActive(false);

            CameraController.SetEnvironemntInteractiblity(false);
            CoroutineManager.LateAction(Step8, m_Step8Delay);
        }

        public void Step8()
        {
            TutorialFocusManager.FocusAtTransform(m_TableFocus, m_TableOffset, Vector2.one * m_TableFocusSize, FocusShapeType.Circle);
            m_DialogStep8.SetActive(true);
            m_GestureStep3.SetActive(true);
            CameraController.SetEnvironemntInteractiblity(true);

            m_EntryStep6 = m_EventTriggerStep6.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
            if (m_EntryStep6 != null)
            {
                m_EntryStep6.callback.AddListener(Step9);
            }
        }

        public void Step9(BaseEventData eventData)
        {
            m_EntryStep6.callback.RemoveListener(Step9);
            m_EntryStep6 = null;

            m_DialogStep8.SetActive(false);
            m_GestureStep3.SetActive(false);

            CameraController.SetEnvironemntInteractiblity(false);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            CoroutineManager.LateAction(() =>
            {
                m_DialogStep9.SetActive(true);
                TutorialFocusManager.FocusAtTransform(m_GoalFocus, m_GoalFocusOffset, m_GoalFocusScale, FocusShapeType.LongRectangle);
                TutorialFocusManager.SetBackgroundButtonCallback(Step10);
            }, m_Step9Delay);

        }

        public void Step10()
        {
            m_Holder.SetActive(false);

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            CustomerManager.CustomerGenerationOn();
            UIManager.UIInteractionOn();
            CameraController.SetEnvironemntInteractiblity(true);

            Stop();
        }
    }
}
