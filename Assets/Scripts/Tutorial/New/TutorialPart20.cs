using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Isometric.Cam;
using Isometric.Customer;
using Isometric.UI;
using Isometric.Player;

namespace Isometric.Tutorial
{
    public class TutorialPart20 : TutorialContainer
    {
        [Header("---Step1---")]
        [SerializeField] GameObject m_Holder;
        [SerializeField] GameObject m_Step1Dialog;
        [SerializeField] Transform m_WorkerStatsFocusPosition;
        [SerializeField] Vector2 m_WorkerStatsFocusScale;
        [SerializeField] Vector2 m_WorkerStatsFocusOffset;
        [SerializeField] GameObject m_GestureArrow;

        [Header("---Step2---")]
        [SerializeField] GameObject m_Step2Dialog;

        [Header("---Step3---")]
        [SerializeField] GameObject m_Step3Dialog;

        [Header("---Step4---")]
        [SerializeField] GameObject m_Step4Dialog;

        private PlayerController m_Player;

        public override void Play()
        {
            m_Player = PlayerManager.GetPlayerAtIndex(0);
            Step1();
        }

        public void Step1()
        {
            m_Player.PauseGreetingReaction();
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);

            CustomerManager.CustomerGenerationOff();
            CameraController.SetEnvironemntInteractiblity(false);

            UIManager.UIInteractionOff();

            TutorialFocusManager.RemoveBackgroundButtonCallback();

            TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);
            TutorialManager.ActivateTapToContinueText(TutorailTextPosition.Down);

            TutorialFocusManager.SetBackgroundButtonCallback(Step2);
            TutorialFocusManager.FocusAtPosition(m_WorkerStatsFocusPosition.position, m_WorkerStatsFocusOffset, m_WorkerStatsFocusScale, FocusShapeType.Circle);

            m_Holder.SetActive(true);
            m_Step1Dialog.SetActive(true);
            m_GestureArrow.SetActive(true);
        }

        public void Step2()
        {
            m_Step1Dialog.SetActive(false);
            m_Step2Dialog.SetActive(true);

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.SetBackgroundButtonCallback(Step3);
        }

        public void Step3()
        {
            m_Step2Dialog.SetActive(false);
            m_Step3Dialog.SetActive(true);

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.SetBackgroundButtonCallback(Step4);
        }

        public void Step4()
        {
            m_Step3Dialog.SetActive(false);
            m_Step4Dialog.SetActive(true);
            m_GestureArrow.SetActive(false);

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
        }

        public void Complete()
        {
            m_Step4Dialog.SetActive(false);
            m_Holder.SetActive(false);

            UIManager.UIInteractionOn();

            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(false);

            CustomerManager.CustomerGenerationOn();
            CameraController.SetEnvironemntInteractiblity(true);
            m_Player.UnpauseGreetingReaction();

            Stop();
        }
    }
}
