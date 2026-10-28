using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Isometric.Environment;
using Isometric.Customer;
using Isometric.UI;
using Isometric.Player;
using Isometric.Cam;

namespace Isometric.Tutorial
{
    public class TutorialPart17 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;
        [SerializeField] List<Collider2D> m_OtherColliders;

        [Header("---Step1---")]
        [SerializeField] GameObject m_DialogStep1;
        [SerializeField] GameObject m_GestureArrowStep1;
        [SerializeField] Vector2 m_HeatFocusSize;
        [SerializeField] Vector2 m_HeatFocusOffset;

        [Header("---Step2---")]
        [SerializeField] GameObject m_DialogStep2;
        [SerializeField] GameObject m_GestureStep2;
        [SerializeField] EventTrigger m_ACEventTrigger;
        [SerializeField] Collider2D m_ACCollider;
        [SerializeField] float m_ACFocusSize;
        [SerializeField] Vector2 m_ACFocusOffset;
        private EventTrigger.Entry m_ACTriggerEntery = null;

        private PlayerController m_Player = null;

        public override void Play()
        {
            m_Player = PlayerManager.GetPlayerAtIndex(0);
            StartCoroutine(CheckIfSunLightIsActivated());
        }

        IEnumerator CheckIfSunLightIsActivated()
        {
            yield return new WaitWhile(() =>  CustomerPatienceManager.IsSunRaysActivated() == false);
            foreach (var collider in m_OtherColliders)
            {
                collider.enabled = false;
            }
            CameraController.SetEnvironemntInteractiblity(false);

            Step1();
        }

        public void Step1()
        {
            m_Player.PauseGreetingReaction();
            LevelManager.SetTimerLock(true);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);

            UIManager.UIInteractionOff();
            CustomerManager.CustomerGenerationOff();

            TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_ACEventTrigger.transform.position, m_HeatFocusOffset, m_HeatFocusSize, FocusShapeType.Square);

            TutorialFocusManager.SetBackgroundButtonCallback(UseAC);
            TutorialManager.ActivateTapToContinueText(TutorailTextPosition.Down);

            m_Holder.SetActive(true);
            m_DialogStep1.SetActive(true);
            m_GestureArrowStep1.SetActive(true);
        }

        public void UseAC()
        {
            TutorialFocusManager.StopAllFocusInstantly();
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            m_DialogStep1.SetActive(false);
            m_GestureArrowStep1.SetActive(false);
            TutorialManager.DeactivateTapToContinueText();

            m_ACTriggerEntery = m_ACEventTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
            if (m_ACTriggerEntery != null)
            {
                m_ACTriggerEntery.callback.AddListener(Complete);
            }

            CameraController.SetEnvironemntInteractiblity(true);
            m_ACCollider.enabled = true;
            m_DialogStep2.SetActive(true);
            m_GestureStep2.SetActive(true);
            TutorialFocusManager.FocusAtPosition(m_ACEventTrigger.transform.position, m_ACFocusOffset, Vector2.one * m_ACFocusSize, FocusShapeType.Circle);
        }

        public void Complete(BaseEventData eventData)
        {
            if (m_ACTriggerEntery != null)
            {
                m_ACTriggerEntery.callback.RemoveListener(Complete);
                m_ACTriggerEntery = null;
            }

            m_ACCollider.enabled = false;
            m_DialogStep2.SetActive(false);
            m_GestureStep2.SetActive(false);
            TutorialFocusManager.StopAllFocus();
            
            StartCoroutine(CompleteNext());
        }

        private IEnumerator CompleteNext()
        {
            yield return new WaitWhile(() =>  CustomerPatienceManager.IsSunRaysActivated() == true);
            m_Holder.SetActive(false);

            foreach (var collider in m_OtherColliders)
            {
                collider.enabled = true;
            }

            CameraController.SetEnvironemntInteractiblity(true);
            TutorialFocusManager.StopAllFocus();
            CustomerManager.CustomerGenerationOn();
            m_Player.UnpauseGreetingReaction();
            UIManager.UIInteractionOn();

            LevelManager.SetTimerLock(false);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(false);

            Stop();
        }
    }
}
