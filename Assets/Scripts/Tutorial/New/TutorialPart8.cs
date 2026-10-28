using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Isometric.Cam;
using Isometric.UI;
using Isometric.Customer;
using Isometric.Environment;
using Isometric.Player;

namespace Isometric.Tutorial
{
    public class TutorialPart8 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;
        [SerializeField] List<Collider2D> m_OtherColliders;

        [Header("---Step1---")]
        [SerializeField] float m_CustomerFocusDelay;
        [SerializeField] float m_CustomerInteractionDelay;
        [SerializeField] float m_FocusOnCustomerSize;
        [SerializeField] Vector2 m_FocusOnCustomerOffset;
        private CustomerSalonController m_TargetCustomer = null;

        [Header("---Step2---")]
        [SerializeField] MainServiceCustomerHandler m_TargetBed;
        [SerializeField] List<MainServiceCustomerHandler> m_Beds;
        [SerializeField] GameObject m_DragGestureStep2;

        [Header("---Step3---")]
        [SerializeField] EventTrigger m_BedTrigger;
        [SerializeField] Collider2D m_BedCollider;
        EventTrigger.Entry m_BedTriggerEntry = null;
        [SerializeField] Transform m_BedFocus;
        [SerializeField] float m_BedFocusScale;
        [SerializeField] Vector2 m_BedFocusOffset;
        [SerializeField] SpriteButton m_SpriteButtonStep3;
        [SerializeField] GameObject m_GestureStep3;

        [Header("---Step4---")]
        [SerializeField] float m_DelayStep4;
        [SerializeField] GameObject m_DialogStep4;
        [SerializeField] GameObject m_GestureStep4;
        [SerializeField] EventTrigger m_VitaminsTrigger;
        [SerializeField] Collider2D m_VitaminsCollider;
        EventTrigger.Entry m_VitaminsTriggerEntry = null;
        [SerializeField] float m_VitaminsFocusScale;
        [SerializeField] Vector2 m_VitaminsFocusOffset;

        [Header("---Step5---")]
        [SerializeField] Button m_TaskStopButton;
        [SerializeField] GameObject m_Step5Dialog;
        [SerializeField] GameObject m_TapGesture5;
        [SerializeField] float m_TaskStopButtonFocusScale;
        [SerializeField] Vector2 m_TaskStopButtonFocusOffset;

        private PlayerController m_Player;

        public override void Play()
        {
            m_Player = PlayerManager.GetPlayerAtIndex(0);
            Step1();
        }

        public void Step1()
        {
            LevelManager.SetTimerLock(true);
            UIManager.UIInteractionOff();
            CameraController.SetEnvironemntInteractiblity(false);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);

            TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);
            m_Holder.SetActive(true);

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
            yield return new WaitForSeconds(m_CustomerFocusDelay);
            m_Player.PauseGreetingReaction();
            TutorialFocusManager.FocusAtTransform(m_TargetCustomer.transform, m_FocusOnCustomerOffset, Vector2.one * m_FocusOnCustomerSize, FocusShapeType.Circle);
            yield return new WaitForSeconds(m_CustomerInteractionDelay);

            Step2();
        }

        public void Step2()
        {
            foreach (var bed in m_Beds)
            {
                bed.TutorialLock(true);
            }
            
            foreach (var collider in m_OtherColliders)
            {
                collider.enabled = false;
            }

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_BedFocus.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
            m_DragGestureStep2.SetActive(true);

            CameraController.SetEnvironemntInteractiblity(true);

            StartCoroutine(Steping2());
        }

        IEnumerator Steping2()
        {
            m_TargetBed.TutorialLock(false);
            m_BedCollider.enabled = true;
            yield return new WaitWhile(() => m_TargetCustomer.IsCustomerOnSalonChair() == false);

            m_DragGestureStep2.SetActive(false);
            TutorialFocusManager.StopAllFocusInstantly();

            // CoroutineManager.LateAction(Step3, TutorialFocusManager.GetFocusDisableDuration());
            Step3();
        }

        public void Step3()
        {
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_BedFocus.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
            m_GestureStep3.SetActive(true);
            m_SpriteButtonStep3.OnClick.AddListener(Step3Next);
        }

        public void Step3Next()
        {
            m_SpriteButtonStep3.OnClick.RemoveListener(Step3Next);

            /* foreach (var collider in m_OtherCollider)
            {
                collider.enabled = true;
            } */
            
            m_GestureStep3.SetActive(false);
            TutorialFocusManager.StopAllFocus();

            Step4();
        }

        public void Step4()
        {
            CameraController.SetEnvironemntInteractiblity(false);
            m_BedCollider.enabled = false;
            CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
                TutorialFocusManager.FocusAtPosition(m_VitaminsTrigger.transform.position, m_VitaminsFocusOffset, Vector2.one * m_VitaminsFocusScale, FocusShapeType.Circle);

                m_DialogStep4.SetActive(true);
                m_GestureStep4.SetActive(true);

                m_VitaminsCollider.enabled = true;
                m_VitaminsTriggerEntry = m_VitaminsTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
                if (m_VitaminsTriggerEntry != null)
                {
                    m_VitaminsTriggerEntry.callback.AddListener(Step4Next);
                }
            }, m_DelayStep4);
        }

        public void Step4Next(BaseEventData eventData)
        {
            if (m_VitaminsTriggerEntry != null)
            {
                m_VitaminsTriggerEntry.callback.RemoveListener(Step4Next);
                m_VitaminsTriggerEntry = null;
            }

            TutorialFocusManager.StopAllFocusInstantly();
            m_DialogStep4.SetActive(false);
            m_GestureStep4.SetActive(false);
            m_VitaminsCollider.enabled = false;

            // CoroutineManager.LateAction(Step6, TutorialFocusManager.GetFocusDisableDuration());
            Step6();
        }

        private void Step6()
        {
            m_Step5Dialog.SetActive(true);
            m_TapGesture5.SetActive(true);

            GameManager.PauseGame();

            CameraController.SetEnvironemntInteractiblity(false);
            UIManager.UIInteractionOn();

            TutorialFocusManager.RemoveBackgroundButtonCallback();

            TutorialFocusManager.FocusAtPosition(m_BedFocus.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
            TutorialFocusManager.FocusAtPosition(m_TaskStopButton.transform.position, m_TaskStopButtonFocusOffset, Vector2.one * m_TaskStopButtonFocusScale, FocusShapeType.Circle);

            m_TaskStopButton.onClick.AddListener(Complete);
        }

        private void Complete()
        {
            m_TaskStopButton.onClick.RemoveListener(Complete);

            m_Step5Dialog.SetActive(false);
            m_TapGesture5.SetActive(false);
            m_Holder.SetActive(false);

            foreach (var bed in m_Beds)
            {
                bed.TutorialLock(false);
            }

            foreach (var collider in m_OtherColliders)
            {
                collider.enabled = true;
            }

            TutorialFocusManager.StopAllFocus();

            CameraController.SetEnvironemntInteractiblity(true);
            LevelManager.SetTimerLock(false);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(false);

            CustomerManager.CustomerGenerationOn();

            GameManager.UnPauseGame();

            Stop();
        }
    }
}
