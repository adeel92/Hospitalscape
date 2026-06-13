using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Isometric.Environment;
using Isometric.Customer;
using Isometric.UI;

namespace Isometric.Tutorial
{
    public class TutorialPart11 : TutorialContainer
    {
        [SerializeField] GameObject m_Dialog;
        [SerializeField] GameObject m_Gesture;
        [SerializeField] EventTrigger m_ACEventTrigger;
        [SerializeField] float m_ACFocusSize;
        [SerializeField] Vector2 m_ACFocusOffset;
        private EventTrigger.Entry m_ACTriggerEntery = null;

        public override void Play()
        {
            StartCoroutine(CheckIfSunLightIsActivated());
        }

        IEnumerator CheckIfSunLightIsActivated()
        {
            yield return new WaitWhile(() =>  CustomerPatienceManager.IsSunRaysActivated() == false);

            UseAC();
        }

        public void UseAC()
        {
            LevelManager.SetTimerLock(true);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);

            UIManager.UIInteractionOff();
            CustomerManager.CustomerGenerationOff();

            TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);

            TutorialFocusManager.StopAllFocus();
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_ACEventTrigger.transform.position, m_ACFocusOffset, Vector2.one * m_ACFocusSize, FocusShapeType.Circle);


            m_ACTriggerEntery = m_ACEventTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
            if (m_ACTriggerEntery != null)
            {
                m_ACTriggerEntery.callback.AddListener(Complete);
            }

            m_Dialog.SetActive(true);
            m_Gesture.SetActive(true);
        }

        public void Complete(BaseEventData eventData)
        {
            if (m_ACTriggerEntery != null)
            {
                m_ACTriggerEntery.callback.RemoveListener(Complete);
                m_ACTriggerEntery = null;
            }

            TutorialFocusManager.StopAllFocus();

            m_Dialog.SetActive(false);
            m_Gesture.SetActive(false);

            CustomerManager.CustomerGenerationOn();
            UIManager.UIInteractionOn();

            LevelManager.SetTimerLock(false);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(false);

            Stop();
        }
    }
}
