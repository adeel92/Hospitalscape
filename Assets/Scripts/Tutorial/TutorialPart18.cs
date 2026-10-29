using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Isometric.Cam;
using Isometric.Customer;
using Isometric.UI;

namespace Isometric.Tutorial.Old
{
    public class TutorialPart18 : TutorialContainer
    {
        [Header("---Step1---")]
        [SerializeField] GameObject m_Holder;
        [SerializeField] GameObject m_Step1Dialog;
        [SerializeField] Transform m_ChefStatsFocusPosition;
        [SerializeField] Vector2 m_ChefStatsFocusScale;
        [SerializeField] Vector2 m_ChefStatsFocusOffset;

        [Header("---Step2---")]
        [SerializeField] GameObject m_Step2Dialog;

        [Header("---Step3---")]
        [SerializeField] GameObject m_Step3Dialog;

        [Header("---Step4---")]
        [SerializeField] GameObject m_Step4Dialog;

        public override void Play()
        {
            Step1();
        }

        public void Step1()
        {
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);

            CustomerManager.CustomerGenerationOff();
            CameraController.SetEnvironemntInteractiblity(false);

            UIManager.UIInteractionOff();

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);
            TutorialManager.ActivateTapToContinueText(TutorailTextPosition.Down);

            TutorialFocusManager.SetBackgroundButtonCallback(Step2);
            TutorialFocusManager.FocusAtPosition(m_ChefStatsFocusPosition.position, m_ChefStatsFocusOffset, m_ChefStatsFocusScale, FocusShapeType.Circle);

            m_Holder.SetActive(true);
            m_Step1Dialog.SetActive(true);
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

            Stop();
        }
    }
}
