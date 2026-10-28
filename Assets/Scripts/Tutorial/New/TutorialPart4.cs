using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Isometric.Data;
using Isometric.Player;
using Isometric.Cam;
using Isometric.UI;
using Isometric.Customer;

namespace Isometric.Tutorial
{
    public class TutorialPart4 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;

        [Header("---Step1---")]
        [SerializeField] PlayerController m_PlayerController;
        [SerializeField] PlayerAnimatorController m_PlayerAnimatorController;
        [SerializeField] DataConsumable m_OrderType;
        [SerializeField] float m_Step1FocusDelay;
        [SerializeField] float m_PlayerFocusScale;
        [SerializeField] Vector2 m_PlayerFocusOffset;
        [SerializeField] GameObject m_Step1Dialog;
        private PlayerController player = null;

        [Header("---Step2---")]
        [SerializeField] EventTrigger m_TrashEventTrigger;
        private EventTrigger.Entry m_TrashEventEntry = null;
        [SerializeField] float m_TrashFocusScale;
        [SerializeField] Vector2 m_TrashFocusOffset;
        [SerializeField] GameObject m_Step2Dialog;
        [SerializeField] GameObject m_GestureTap;

        public override void Play()
        {
            m_PlayerController.PauseGreetingReaction();
            m_PlayerAnimatorController.PlayIdle();
            LevelManager.SetTimerLock(true);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);

            CustomerManager.CustomerGenerationOff();
            UIManager.UIInteractionOff();
            CameraController.SetEnvironemntInteractiblity(false);
            player = PlayerManager.GetPlayerAtIndex(0);
            player.SendDataConsumable(m_OrderType, 0);
            CoroutineManager.LateAction(Step1, m_Step1FocusDelay);
        }

        public void Step1()
        {
            m_Holder.SetActive(true);
            m_Step1Dialog.SetActive(true);

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtTransform(player.transform, m_PlayerFocusOffset, Vector2.one * m_PlayerFocusScale, FocusShapeType.Circle);
            TutorialManager.ActivateTapToContinueText(TutorailTextPosition.Down);
            TutorialFocusManager.SetBackgroundButtonCallback(Step2);
        }

        public void Step2()
        {
            m_Step1Dialog.SetActive(false);
            m_Step2Dialog.SetActive(true);
            TutorialFocusManager.StopAllFocusInstantly();
            TutorialManager.DeactivateTapToContinueText();
            TutorialFocusManager.FocusAtTransform(m_TrashEventTrigger.transform, m_TrashFocusOffset, Vector2.one * m_TrashFocusScale, FocusShapeType.Circle);
            m_GestureTap.SetActive(true);
            CameraController.SetEnvironemntInteractiblity(true);

            m_TrashEventEntry = m_TrashEventTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
            if (m_TrashEventEntry != null)
            {
                m_TrashEventEntry.callback.AddListener(Step3);
            }
        }

        private void Step3(BaseEventData baseEventData)
        {
            if (m_TrashEventEntry != null)
            {
                m_TrashEventEntry.callback.RemoveListener(Step3);
                m_TrashEventEntry = null;
            }

            m_Step2Dialog.SetActive(false);
            m_Holder.SetActive(false);
            m_GestureTap.SetActive(false);

            TutorialFocusManager.StopAllFocus();
            UIManager.UIInteractionOn();
            CustomerManager.CustomerGenerationOn();
            m_PlayerController.UnpauseGreetingReaction();

            LevelManager.SetTimerLock(false);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(false);

            Stop();
        }

    }
}