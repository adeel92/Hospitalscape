using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NaughtyAttributes;
using Isometric.Environment;
using Isometric.Cam;
using Isometric.UI;
using Isometric.Customer;

namespace Isometric.Tutorial
{
    public class TutorialPart10 : TutorialContainer
    {
        [Header("---Step1---")]
        //[SerializeField] TableCounterCustomerHandler m_CustomerHandler;
        [SerializeField] Vector2 m_FocusOnCustomerOffset;
        [SerializeField] Vector2 m_FocusOnCustomerSize;
        [SerializeField] GameObject m_DialogCanvas;
        [SerializeField] GameObject m_Dialog1;
        [SerializeField, ReadOnly] Transform m_ParcelCustomer;

        [Header("---Step2---")]
        [SerializeField] EventTrigger m_SaladEventTriggerStep2;
        [SerializeField] float m_SaladStationFocusSize;
        [SerializeField] Vector2 m_SaladStationFocusOffset;
        private EventTrigger.Entry m_EntryStep2;
        [SerializeField] GameObject m_DialogStep2;
        [SerializeField] GameObject m_GestureStep2;

        [Header("---Step3---")]
        [SerializeField] EventTrigger m_CounterTableEventTrigger;
        [SerializeField] float m_CounterTableFocusSize;
        [SerializeField] Vector2 m_CounterTableFocusOffset;
        [SerializeField] GameObject m_Dialog3;
        [SerializeField] GameObject m_Gesture3;
        private EventTrigger.Entry m_CounterTableTriggerEntery = null;

        public override void Play()
        {
            LevelManager.SetTimerLock(true);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);

            TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);

            UIManager.UIInteractionOff();
            CameraController.SetEnvironemntInteractiblity(false);

            CustomerManager.CustomerGenerationOff();

            StartCoroutine(CheckIfCustomerIsAtTheCounter());
        }

        IEnumerator CheckIfCustomerIsAtTheCounter()
        {
            yield return null;
            /*yield return new WaitWhile(() => m_CustomerHandler.CustomerController == null);

            m_ParcelCustomer = m_CustomerHandler.CustomerController.transform;

            Step1();*/
        }

        private void Step1()
        {

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            TutorialFocusManager.SetBackgroundButtonCallback(Step2);
            TutorialFocusManager.FocusAtPosition(m_ParcelCustomer.position, m_FocusOnCustomerOffset, m_FocusOnCustomerSize, FocusShapeType.Circle);

            m_DialogCanvas.SetActive(true);
            m_Dialog1.SetActive(true);
        }

        private void Step2()
        {
            CameraController.SetEnvironemntInteractiblity(true);

            TutorialFocusManager.StopAllFocus();
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_SaladEventTriggerStep2.transform.position, m_SaladStationFocusOffset, Vector2.one * m_SaladStationFocusSize, FocusShapeType.Circle);

            m_Dialog1.SetActive(false);

            m_DialogStep2.SetActive(true);
            m_GestureStep2.SetActive(true);

            m_EntryStep2 = m_SaladEventTriggerStep2.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);

            if (m_EntryStep2 != null)
            {
                m_EntryStep2.callback.AddListener(Steping2);
            }
        }

        private void Steping2(BaseEventData eventData)
        {
            if (m_EntryStep2 != null)
            {
                m_EntryStep2.callback.RemoveListener(Steping2);
                m_EntryStep2 = null;
            }

            m_DialogStep2.SetActive(false);
            m_GestureStep2.SetActive(false);

            Step3();
        }

        public void Step3()
        {
            m_CounterTableTriggerEntery = m_CounterTableEventTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
            if (m_CounterTableTriggerEntery != null)
            {
                m_CounterTableTriggerEntery.callback.AddListener(Complete);
            }

            TutorialFocusManager.StopAllFocus();
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_CounterTableEventTrigger.transform.position, m_CounterTableFocusOffset, Vector2.one * m_CounterTableFocusSize, FocusShapeType.Circle);

            m_Dialog3.SetActive(true);
            m_Gesture3.SetActive(true);
        }

        public void Complete(BaseEventData eventData)
        {
            if (m_CounterTableTriggerEntery != null)
            {
                m_CounterTableTriggerEntery.callback.RemoveListener(Complete);
                m_CounterTableTriggerEntery = null;
            }

            m_Dialog3.SetActive(false);
            m_Gesture3.SetActive(false);
            m_DialogCanvas.SetActive(false);

            TutorialFocusManager.StopAllFocus();
            TutorialFocusManager.RemoveBackgroundButtonCallback();

            UIManager.UIInteractionOn();

            CustomerManager.CustomerGenerationOn();
            LevelManager.SetTimerLock(false);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(false);

            Stop();
        }
    }
}
