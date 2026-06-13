using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Isometric.UI;
using Isometric.Cam;
using Isometric.Customer;
using Isometric.Environment;

namespace Isometric.Tutorial
{
    public class TutorialPart5 : TutorialContainer
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
        [SerializeField] EventTrigger m_CurlyFriesSnackTrigger;
        EventTrigger.Entry m_CurlyFriesSnakTriggerEntry = null;
        [SerializeField] float m_CurlyFriesSnackFocusScale;
        [SerializeField] Vector2 m_CurlyFriesSnackFocusOffset;

        [Header("---Step5---")]
        [SerializeField] float m_DelayStep5;
        [SerializeField] GameObject m_DialogStep5;
        [SerializeField] GameObject m_GestureStep5;
        [SerializeField] EventTrigger m_SaladTrigger;
        EventTrigger.Entry m_SaladTriggerEntry = null;
        [SerializeField] float m_SaladFocusScale;
        [SerializeField] Vector2 m_SaladFocusOffset;

        [Header("---Step6---")]
        [SerializeField] float m_DelayStep6;
        [SerializeField] GameObject m_DialogStep6;
        [SerializeField] GameObject m_GestureStep6;
        

        public override void Play()
        {
            Step1();
        }

        public void Step1()
        {
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);

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
                TutorialFocusManager.RemoveBackgroundButtonCallback();
                TutorialFocusManager.StopAllFocus();
                TutorialFocusManager.FocusAtPosition(m_CurlyFriesSnackTrigger.transform.position, m_CurlyFriesSnackFocusOffset, Vector2.one * m_CurlyFriesSnackFocusScale, FocusShapeType.Circle);

                m_Holder.SetActive(true);
                m_DialogStep4.SetActive(true);
                m_GestureStep4.SetActive(true);

                m_CurlyFriesSnakTriggerEntry = m_CurlyFriesSnackTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
                if (m_CurlyFriesSnakTriggerEntry != null)
                {
                    m_CurlyFriesSnakTriggerEntry.callback.AddListener(Step4Next);
                }
            },m_DelayStep4);
        }
            

        public void Step4Next(BaseEventData eventData)
        {
            if (m_CurlyFriesSnakTriggerEntry != null)
            {
                m_CurlyFriesSnakTriggerEntry.callback.RemoveListener(Step4Next);
                m_CurlyFriesSnakTriggerEntry = null;
            }

            TutorialFocusManager.StopAllFocus();
            m_DialogStep4.SetActive(false);
            m_GestureStep4.SetActive(false);

            Step5();
        }

        public void Step5()
        {
            CameraController.SetEnvironemntInteractiblity(false);
            CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
                TutorialFocusManager.FocusAtPosition(m_SaladTrigger.transform.position, m_SaladFocusOffset, Vector2.one * m_SaladFocusScale, FocusShapeType.Circle);

                m_DialogStep5.SetActive(true);
                m_GestureStep5.SetActive(true);

                m_SaladTriggerEntry = m_SaladTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
                if (m_SaladTriggerEntry != null)
                {
                    m_SaladTriggerEntry.callback.AddListener(Step5Next);
                }
            }, m_DelayStep5);
        }

        public void Step5Next(BaseEventData eventData)
        {
            if (m_SaladTriggerEntry != null)
            {
                m_SaladTriggerEntry.callback.RemoveListener(Step5Next);
                m_SaladTriggerEntry = null;
            }

            TutorialFocusManager.StopAllFocus();
            m_DialogStep5.SetActive(false);
            m_GestureStep5.SetActive(false);

            Step6();
        }

        public void Step6()
        {
            CameraController.SetEnvironemntInteractiblity(false);
            CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
                TutorialFocusManager.FocusAtPosition(m_TabeTrigger.transform.position, m_TableFocusOffset, Vector2.one * m_TableFocusScale, FocusShapeType.Circle);
                m_DialogStep6.SetActive(true);
                m_GestureStep6.SetActive(true);

                m_TableTriggerEntry = m_TabeTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
                if (m_TableTriggerEntry != null)
                {
                    m_TableTriggerEntry.callback.AddListener(Step6Next);
                }
            }, m_DelayStep6);
        }

        public void Step6Next(BaseEventData eventData)
        {
            if (m_TableTriggerEntry != null)
            {
                m_TableTriggerEntry.callback.RemoveListener(Step6Next);
                m_TableTriggerEntry = null;
            }

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            m_DialogStep6.SetActive(false);
            m_GestureStep6.SetActive(false);
            m_Holder.SetActive(false);

            UIManager.UIInteractionOn();
            CustomerManager.CustomerGenerationOn();
            Stop();
        }
    }
}
