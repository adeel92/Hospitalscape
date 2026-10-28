using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Isometric.UI;
using Isometric.Cam;
using Isometric.Customer;
using Isometric.Environment;
using Isometric.Player;

namespace Isometric.Tutorial
{
    public class TutorialPart7 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;

        [Header("---Step1---")]
        [SerializeField] float m_CustomerFocusDelay;
        [SerializeField] float m_FocusOnCustomerSize;
        [SerializeField] Vector2 m_FocusOnCustomerOffset;
        private CustomerSalonController m_TargetCustomer = null;

        [Header("---Step2---")]
        [SerializeField] float m_Step2Delay;
        [SerializeField] Button m_FreezeBoosterFocus;
        [SerializeField] float m_FreezeBoosterSize;
        [SerializeField] Vector2 m_FreezeBoosterOffset;
        [SerializeField] GameObject m_DialogStep2;
        [SerializeField] GameObject m_GestureStep2;

        private PlayerController m_Player;

        public override void Play()
        {
            m_Player = PlayerManager.GetPlayerAtIndex(0);
            Step1();
        }

        public void Step1()
        {
            LevelManager.SetTimerLock(true);

            TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);

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
            TutorialFocusManager.RemoveBackgroundButtonCallback();

            yield return new WaitForSeconds(m_CustomerFocusDelay);
            m_Player.PauseGreetingReaction();
            TutorialFocusManager.FocusAtTransform(m_TargetCustomer.transform, m_FocusOnCustomerOffset, Vector2.one * m_FocusOnCustomerSize, FocusShapeType.Circle);
            yield return new WaitForSeconds(m_Step2Delay);

            Step2();
        }

        public void Step2()
        {
            UIManager.UIInteractionOn();
            TutorialFocusManager.FocusAtPosition(m_FreezeBoosterFocus.transform.position, m_FreezeBoosterOffset, Vector2.one * m_FreezeBoosterSize, FocusShapeType.Circle);

            m_TargetCustomer.SetWaitDurationCounter(5);
            m_TargetCustomer.PlayAnimationState(CustomerAnimatorState.StandingIdleFurious);

            m_Holder.SetActive(true);
            m_DialogStep2.SetActive(true);
            m_GestureStep2.SetActive(true);
            m_FreezeBoosterFocus.onClick.AddListener(Step2Next);
        }

        public void Step2Next()
        {
            m_FreezeBoosterFocus.onClick.RemoveListener(Step2Next);

            m_DialogStep2.SetActive(false);
            m_GestureStep2.SetActive(false);
            m_Holder.SetActive(false);

            m_TargetCustomer.UnFreezeWait();

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            CameraController.SetEnvironemntInteractiblity(true);
            CustomerManager.CustomerGenerationOn();
            m_Player.UnpauseGreetingReaction();

            LevelManager.SetTimerLock(false);

            Stop();
        }
    }
}
