using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Isometric.Cam;
using Isometric.Customer;
using Isometric.Player;

namespace Isometric.Tutorial
{
    public class TutorialPart19 : TutorialContainer
    {
        [Header("---Step1---")]
        [SerializeField] GameObject m_Holder;
        [SerializeField] Transform m_DontLoseCustomerFocus;
        [SerializeField] Vector2 m_DontLoseCustomerFocusScale;
        [SerializeField] Vector2 m_DontLoseCustomerFocusOffset;
        [SerializeField] GameObject m_Step1Dialog;
        [SerializeField] GameObject m_FocusArrow;

        [Header("---Step2---")]
        [SerializeField] GameObject m_Step2Dialog;

        private PlayerController m_Player;

        public override void Play()
        {
            m_Player = PlayerManager.GetPlayerAtIndex(0);
            Step1();
        }

        public void Step1()
        {
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);

            CustomerManager.CustomerGenerationOff();
            CameraController.SetEnvironemntInteractiblity(false);
            m_Player.PauseGreetingReaction();

            TutorialFocusManager.RemoveBackgroundButtonCallback();

            TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);
            TutorialManager.ActivateTapToContinueText(TutorailTextPosition.Down);

            TutorialFocusManager.SetBackgroundButtonCallback(Step2);
            TutorialFocusManager.FocusAtPosition(m_DontLoseCustomerFocus.position, m_DontLoseCustomerFocusOffset, m_DontLoseCustomerFocusScale, FocusShapeType.Circle);
            

            m_Holder.SetActive(true);
            m_Step1Dialog.SetActive(true);
            m_FocusArrow.SetActive(true);
        }

        public void Step2()
        {
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            m_Step1Dialog.SetActive(false);
            m_FocusArrow.SetActive(false);

            m_Step2Dialog.SetActive(true);
        }

        public void Complete()
        {
            m_Step2Dialog.SetActive(false);
            m_Holder.SetActive(false);
            TutorialManager.DeactivateTapToContinueText();

            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(false);
            CameraController.SetEnvironemntInteractiblity(true);
            CustomerManager.CustomerGenerationOn();
            m_Player.UnpauseGreetingReaction();

            Stop();
        }
    }
}
