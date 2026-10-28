using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Isometric.UI;
using Isometric.Cam;
using Isometric.Customer;
using Isometric.Environment;
using Isometric.Player;

namespace Isometric.Tutorial
{
    public class TutorialPart6 : TutorialContainer
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
        [SerializeField] GameObject m_DialogStep5;

        [Header("---Step6---")]
        [SerializeField] GameObject m_DialogStep6;
        [SerializeField] float m_PlayerFocusScale;
        [SerializeField] Vector2 m_PlayerFocusOffset;

        [Header("---Step7---")]
        // [SerializeField] float m_DelayStep7;
        [SerializeField] GameObject m_DialogStep7;
        [SerializeField] GameObject m_GestureStep7;
        [SerializeField] EventTrigger m_InjectionTrigger;
        [SerializeField] Collider2D m_InjectionCollider;
        EventTrigger.Entry m_InjectionTriggerEntry = null;
        [SerializeField] float m_InjectionFocusScale;
        [SerializeField] Vector2 m_InjectionFocusOffset;

        [Header("---Step8---")]
        [SerializeField] float m_DelayStep8;
        [SerializeField] GameObject m_DialogStep8;
        [SerializeField] GameObject m_GestureStep8;
        
        private PlayerController player = null;


        public override void Play()
        {
            player = PlayerManager.GetPlayerAtIndex(0);
            Step1();
        }

        public void Step1()
        {
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

            yield return new WaitForSeconds(m_CustomerFocusDelay);
            player.PauseGreetingReaction();
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
            m_BedCollider.enabled = true;
            m_TargetBed.TutorialLock(false);
            yield return new WaitWhile(() => m_TargetCustomer.IsCustomerOnSalonChair() == false);
            
            m_DragGestureStep2.SetActive(false);
            TutorialFocusManager.StopAllFocusInstantly();
            
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
            /* foreach (var table in m_Beds)
            {
                table.TutorialLock(false);
            } */
            m_BedCollider.enabled = false;
            m_GestureStep3.SetActive(false);
            TutorialFocusManager.StopAllFocus();

            Step4();
        }

        public void Step4()
        {
            CameraController.SetEnvironemntInteractiblity(false);
            CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
                TutorialFocusManager.RemoveBackgroundButtonCallback();
                TutorialFocusManager.FocusAtPosition(m_VitaminsTrigger.transform.position, m_VitaminsFocusOffset, Vector2.one * m_VitaminsFocusScale, FocusShapeType.Circle);

                m_Holder.SetActive(true);
                m_DialogStep4.SetActive(true);
                m_GestureStep4.SetActive(true);
                m_VitaminsCollider.enabled = true;

                m_VitaminsTriggerEntry = m_VitaminsTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
                if (m_VitaminsTriggerEntry != null)
                {
                    m_VitaminsTriggerEntry.callback.AddListener(Step4Next);
                }
            },m_DelayStep4);
        }
            

        public void Step4Next(BaseEventData eventData)
        {
            if (m_VitaminsTriggerEntry != null)
            {
                m_VitaminsTriggerEntry.callback.RemoveListener(Step4Next);
                m_VitaminsTriggerEntry = null;
            }

            CameraController.SetEnvironemntInteractiblity(false);
            TutorialFocusManager.StopAllFocusInstantly();
            m_DialogStep4.SetActive(false);
            m_GestureStep4.SetActive(false);
            m_VitaminsCollider.enabled = false;

            Step5();
        }

        public void Step5()
        {
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_BedFocus.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
            m_DialogStep5.SetActive(true);
            TutorialFocusManager.SetBackgroundButtonCallback(Step6);
            TutorialManager.ActivateTapToContinueText(TutorailTextPosition.Down);
        }

        public void Step6()
        {
            m_DialogStep5.SetActive(false);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtTransform(player.transform, m_PlayerFocusOffset, Vector2.one * m_PlayerFocusScale, FocusShapeType.Circle);
            m_DialogStep6.SetActive(true);
            TutorialFocusManager.SetBackgroundButtonCallback(Step7);
        }

        public void Step7()
        {
            TutorialFocusManager.StopAllFocusInstantly();
            TutorialManager.DeactivateTapToContinueText();
            m_DialogStep6.SetActive(false);
            CameraController.SetEnvironemntInteractiblity(true);
            TutorialFocusManager.FocusAtPosition(m_InjectionTrigger.transform.position, m_InjectionFocusOffset, Vector2.one * m_InjectionFocusScale, FocusShapeType.Circle);

            m_DialogStep7.SetActive(true);
            m_GestureStep7.SetActive(true);
            m_InjectionCollider.enabled = true;

            m_InjectionTriggerEntry = m_InjectionTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
            if (m_InjectionTriggerEntry != null)
            {
                m_InjectionTriggerEntry.callback.AddListener(Step7Next);
            }
            /* CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
            }, m_DelayStep7); */
        }

        public void Step7Next(BaseEventData eventData)
        {
            if (m_InjectionTriggerEntry != null)
            {
                m_InjectionTriggerEntry.callback.RemoveListener(Step7Next);
                m_InjectionTriggerEntry = null;
            }

            TutorialFocusManager.StopAllFocus();
            m_DialogStep7.SetActive(false);
            m_GestureStep7.SetActive(false);
            m_InjectionCollider.enabled = false;

            Step8();
        }

        public void Step8()
        {
            CameraController.SetEnvironemntInteractiblity(false);
            CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
                TutorialFocusManager.FocusAtPosition(m_BedTrigger.transform.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
                m_DialogStep8.SetActive(true);
                m_GestureStep8.SetActive(true);
                m_BedCollider.enabled = true;

                m_BedTriggerEntry = m_BedTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
                if (m_BedTriggerEntry != null)
                {
                    m_BedTriggerEntry.callback.AddListener(Step8Next);
                }
            }, m_DelayStep8);
        }

        public void Step8Next(BaseEventData eventData)
        {
            if (m_BedTriggerEntry != null)
            {
                m_BedTriggerEntry.callback.RemoveListener(Step8Next);
                m_BedTriggerEntry = null;
            }

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            m_DialogStep8.SetActive(false);
            m_GestureStep8.SetActive(false);
            m_Holder.SetActive(false);

            foreach (var collider in m_OtherColliders)
            {
                collider.enabled = true;
            }

            player.UnpauseGreetingReaction();
            UIManager.UIInteractionOn();
            CustomerManager.CustomerGenerationOn();
            Stop();
        }
    }
}
