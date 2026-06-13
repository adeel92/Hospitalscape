using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Isometric.Cam;
using Isometric.UI;
using Isometric.Customer;
using Isometric.Environment;

namespace Isometric.Tutorial
{
    public class TutorialPart15 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;

        [Header("---Step1---")]
        [SerializeField] float m_CustomerInteractionDelay;
        private CustomerSalonController m_TargetCustomer = null;

        [Header("---Step2---")]
        [SerializeField] List<Collider2D> m_OtherCollider;
        [SerializeField] List<SalonChairCustomerHandler> m_Tables;
        [SerializeField] Transform m_DragFocusPosition;
        [SerializeField] Vector2 m_DragFocusSize;
        [SerializeField] FocusShapeType m_DragFocusShape;
        [SerializeField] GameObject m_DragGestureStep2;

        [Header("---Step3---")]
        [SerializeField] EventTrigger m_TabeTrigger;
        EventTrigger.Entry m_TableTriggerEntry = null;
        [SerializeField] Transform m_TableFocus;
        [SerializeField] float m_TableFocusScale;
        [SerializeField] Vector2 m_TableFocusOffset;
        [SerializeField] SpriteButton m_SpriteButtonStep3;
        [SerializeField] GameObject m_GestureStep3;

        [Header("---Step4---")]
        [SerializeField] float m_DelayStep4;
        [SerializeField] GameObject m_DialogStep4;
        [SerializeField] GameObject m_GestureStep4;
        [SerializeField] EventTrigger m_SaladTrigger;
        EventTrigger.Entry m_SaladTriggerEntry = null;
        [SerializeField] float m_SaladFocusScale;
        [SerializeField] Vector2 m_SaladFocusOffset;

        [Header("---Step5---")]
        [SerializeField] Button m_TaskStopButton;
        [SerializeField] GameObject m_Step5Dialog;
        [SerializeField] GameObject m_TapGesture5;
        [SerializeField] float m_TaskStopButtonFocusScale;
        [SerializeField] Vector2 m_TaskStopButtonFocusOffset;

        public override void Play()
        {
            Step1();
        }

        public void Step1()
        {
            LevelManager.SetTimerLock(true);
            UIManager.UIInteractionOff();
            CameraController.SetEnvironemntInteractiblity(false);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);

            TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);
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
            yield return new WaitForSeconds(m_CustomerInteractionDelay);

            Step2();
        }

        public void Step2()
        {
            TutorialFocusManager.StopAllFocus();
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_DragFocusPosition.position, Vector2.zero, m_DragFocusSize, m_DragFocusShape);
            m_DragGestureStep2.SetActive(true);

            CameraController.SetEnvironemntInteractiblity(true);

            foreach (var collider in m_OtherCollider)
            {
                collider.enabled = false;
            }

            foreach (var table in m_Tables)
            {
                table.TutorialLock(true);
            }

            StartCoroutine(Steping2());
        }

        IEnumerator Steping2()
        {
            yield return new WaitWhile(() => m_TargetCustomer.IsCustomerOnSalonChair() == false);

            m_DragGestureStep2.SetActive(false);

            Step3();
        }

        public void Step3()
        {
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            TutorialFocusManager.FocusAtPosition(m_TableFocus.position, m_TableFocusOffset, Vector2.one * m_TableFocusScale, FocusShapeType.Circle);
            m_GestureStep3.SetActive(true);
            m_SpriteButtonStep3.OnClick.AddListener(Step3Next);
        }

        public void Step3Next()
        {
            m_SpriteButtonStep3.OnClick.RemoveListener(Step3Next);

            foreach (var collider in m_OtherCollider)
            {
                collider.enabled = true;
            }
            foreach (var table in m_Tables)
            {
                table.TutorialLock(false);
            }
            m_GestureStep3.SetActive(false);

            Step4();
        }

        public void Step4()
        {
            CameraController.SetEnvironemntInteractiblity(false);
            CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
                TutorialFocusManager.FocusAtPosition(m_SaladTrigger.transform.position, m_SaladFocusOffset, Vector2.one * m_SaladFocusScale, FocusShapeType.Circle);

                m_DialogStep4.SetActive(true);
                m_GestureStep4.SetActive(true);

                m_SaladTriggerEntry = m_SaladTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
                if (m_SaladTriggerEntry != null)
                {
                    m_SaladTriggerEntry.callback.AddListener(Step4Next);
                }
            }, m_DelayStep4);
        }

        public void Step4Next(BaseEventData eventData)
        {
            if (m_SaladTriggerEntry != null)
            {
                m_SaladTriggerEntry.callback.RemoveListener(Step4Next);
                m_SaladTriggerEntry = null;
            }

            m_DialogStep4.SetActive(false);
            m_GestureStep4.SetActive(false);

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

            TutorialFocusManager.FocusAtPosition(m_TableFocus.position, m_TableFocusOffset, Vector2.one * m_TableFocusScale, FocusShapeType.Circle);
            TutorialFocusManager.FocusAtPosition(m_TaskStopButton.transform.position, m_TaskStopButtonFocusOffset, Vector2.one * m_TaskStopButtonFocusScale, FocusShapeType.Circle);

            m_TaskStopButton.onClick.AddListener(Complete);
        }

        private void Complete()
        {
            m_TaskStopButton.onClick.RemoveListener(Complete);

            m_Step5Dialog.SetActive(false);
            m_TapGesture5.SetActive(false);
            m_Holder.SetActive(false);

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
