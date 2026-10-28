using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Isometric.Cam;
using Isometric.UI;
using Isometric.Customer;
using Isometric.Environment;
using Isometric.Player;

namespace Isometric.Tutorial
{
    public class TutorialPart9 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;

        [Header("---Step1---")]
        [SerializeField] float m_CustomerFocusDelay;
        [SerializeField] float m_CustomerInteractionDelay;
        [SerializeField] float m_FocusOnCustomerSize;
        [SerializeField] Vector2 m_FocusOnCustomerOffset;
        private CustomerSalonController m_TargetCustomer = null;

        [Header("---Step2---")]
        [SerializeField] List<Collider2D> m_OtherColliders;
        [SerializeField] MainServiceCustomerHandler m_TargetBed;
        [SerializeField] List<MainServiceCustomerHandler> m_Beds;
        [SerializeField] GameObject m_GestureStep2;

        [Header("---Step3---")]
        [SerializeField] GameObject m_GestureStep3;
        [SerializeField] SpriteButton m_BedCallWorker;
        [SerializeField] Transform m_BedFocus;
        [SerializeField] float m_BedFocusScale;
        [SerializeField] Vector2 m_BedFocusOffset;

        [Header("---Step4---")]
        [SerializeField] float m_Step4Delay;
        [SerializeField] EventTrigger m_SpecializedInjectionTrigger;
        [SerializeField] Collider2D m_SpecializedInjectionCollider;
        private EventTrigger.Entry m_SpecializedInjectionTriggerEntery = null;
        [SerializeField] float m_SpecializedInjectionSize;
        [SerializeField] Vector2 m_SpecializedInjectionOffset;
        [SerializeField] GameObject m_DialogStep4;

        [Header("---Step5---")]
        // [SerializeField] float m_Step5Delay;
        [SerializeField] EventTrigger m_InjectionTrigger;
        [SerializeField] Collider2D m_InjectionCollider;
        private EventTrigger.Entry m_InjectionTriggerEntry = null;
        [SerializeField] float m_InjectionSize;
        [SerializeField] Vector2 m_InjectionOffset;
        [SerializeField] GameObject m_GestureStep5;
        [SerializeField] GameObject m_DialogStep5;

        [Header("---Step6---")]
        [SerializeField] float m_Step6Delay;
        [SerializeField] GameObject m_DialogStep6;
        [SerializeField] GameObject m_GestureStep6;

        [Header("---Step7---")]
        //[SerializeField] float m_Step7Duration;
        [SerializeField] StationSerialOrderInOut m_SpecializedInjectionStation;
        [SerializeField] GameObject m_DialogStep7;


        [Header("---Step9---")]
        // [SerializeField] float m_Step9Delay;
        [SerializeField] EventTrigger m_BedTrigger;
        [SerializeField] Collider2D m_BedCollider;
        EventTrigger.Entry m_BedTriggerEntry = null;
        [SerializeField] GameObject m_DialogStep9;

        private PlayerController m_Player;

        public override void Play()
        {
            m_Player = PlayerManager.GetPlayerAtIndex(0);
            Step1();
        }

        public void Step1()
        {
            m_Holder.SetActive(true);

            UIManager.UIInteractionOff();
            CameraController.SetEnvironemntInteractiblity(false);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);

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

            TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);
            CustomerManager.CustomerGenerationOff();

            yield return new WaitForSeconds(m_CustomerFocusDelay);
            m_Player.PauseGreetingReaction();
            TutorialFocusManager.FocusAtTransform(m_TargetCustomer.transform, m_FocusOnCustomerOffset, Vector2.one * m_FocusOnCustomerSize, FocusShapeType.Circle);
            yield return new WaitForSeconds(m_CustomerInteractionDelay);

            Step2();
        }

        public void Step2()
        {
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_BedFocus.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);

            CameraController.SetEnvironemntInteractiblity(true);
            m_GestureStep2.SetActive(true);

            foreach (var collider in m_OtherColliders)
            {
                collider.enabled = false;
            }

            foreach (var bed in m_Beds)
            {
                bed.TutorialLock(true);
            }

            StartCoroutine(Steping2());
        }

        IEnumerator Steping2()
        {
            m_TargetBed.TutorialLock(false);
            m_BedCollider.enabled = true;
            yield return new WaitWhile(() => m_TargetCustomer.IsCustomerOnSalonChair() == false);

            m_GestureStep2.SetActive(false);
            TutorialFocusManager.StopAllFocusInstantly();

            Step3();
        }

        public void Step3()
        {
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_BedFocus.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
            m_GestureStep3.SetActive(true);
            m_BedCallWorker.OnClick.AddListener(Step3Next);
        }

        public void Step3Next()
        {
            m_GestureStep3.SetActive(false);
            m_BedCollider.enabled = false;
            m_BedCallWorker.OnClick.RemoveListener(Step3Next);

            /* foreach (var collider in m_OtherColliders)
            {
                collider.enabled = true;
            } */


            Step4();
        }

        public void Step4()
        {
            CameraController.SetEnvironemntInteractiblity(false);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            CoroutineManager.LateAction(() =>
            {
                TutorialFocusManager.FocusAtPosition(m_SpecializedInjectionTrigger.transform.position, m_SpecializedInjectionOffset, Vector2.one * m_SpecializedInjectionSize, FocusShapeType.Circle);
                TutorialFocusManager.SetBackgroundButtonCallback(Step5);
                TutorialManager.ActivateTapToContinueText(TutorailTextPosition.Down);

                m_DialogStep4.SetActive(true);
            }, m_Step4Delay);
        }

        public void Step5()
        {
            m_DialogStep4.SetActive(false);
            TutorialFocusManager.StopAllFocusInstantly();
            TutorialManager.DeactivateTapToContinueText();

            TutorialFocusManager.RemoveBackgroundButtonCallback();


            CameraController.SetEnvironemntInteractiblity(true);
            TutorialFocusManager.FocusAtPosition(m_InjectionTrigger.transform.position, m_InjectionOffset, Vector2.one * m_InjectionSize, FocusShapeType.Circle);

            m_GestureStep5.SetActive(true);
            m_DialogStep5.SetActive(true);
            m_InjectionCollider.enabled = true;

            m_InjectionTriggerEntry = m_InjectionTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
            if (m_InjectionTriggerEntry != null)
            {
                m_InjectionTriggerEntry.callback.AddListener(Step6);
            }
            /* CoroutineManager.LateAction(() =>
            {
            }, m_Step5Delay); */
        }


        public void Step6(BaseEventData eventData)
        {
            if (m_InjectionTriggerEntry != null)
            {
                m_InjectionTriggerEntry.callback.RemoveListener(Step6);
                m_InjectionTriggerEntry = null;
            }

            m_GestureStep5.SetActive(false);
            m_DialogStep5.SetActive(false);
            m_InjectionCollider.enabled = false;


            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            CameraController.SetEnvironemntInteractiblity(false);

            CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
                TutorialFocusManager.FocusAtPosition(m_SpecializedInjectionTrigger.transform.position, m_SpecializedInjectionOffset, Vector2.one * m_SpecializedInjectionSize, FocusShapeType.Circle);

                m_DialogStep6.SetActive(true);
                m_GestureStep6.SetActive(true);
                m_SpecializedInjectionCollider.enabled = true;

                m_SpecializedInjectionTriggerEntery = m_SpecializedInjectionTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
                if (m_SpecializedInjectionTriggerEntery != null)
                {
                    m_SpecializedInjectionTriggerEntery.callback.AddListener(Step7);
                }
            }, m_Step6Delay);
        }

        public void Step7(BaseEventData eventData)
        {
            if (m_SpecializedInjectionTriggerEntery != null)
            {
                m_SpecializedInjectionTriggerEntery.callback.RemoveListener(Step7);
                m_SpecializedInjectionTriggerEntery = null;
            }

            m_DialogStep6.SetActive(false);
            m_GestureStep6.SetActive(false);
            m_SpecializedInjectionCollider.enabled = false;

            CameraController.SetEnvironemntInteractiblity(false);

            m_DialogStep7.SetActive(true);

            CoroutineManager.LateAction(Step8, m_SpecializedInjectionStation.DurationProperty + 1f);
            // CoroutineManager.LateAction(Step8, m_GuazeMachineStation.DurationProperty + 0.5f);
        }

        public void Step8()
        {
            m_DialogStep7.SetActive(false);
            CameraController.SetEnvironemntInteractiblity(true);

            m_DialogStep6.SetActive(true);
            m_GestureStep6.SetActive(true);
            m_SpecializedInjectionCollider.enabled = true;

            m_SpecializedInjectionTriggerEntery = m_SpecializedInjectionTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
            if (m_SpecializedInjectionTriggerEntery != null)
            {
                m_SpecializedInjectionTriggerEntery.callback.AddListener(Step9);
            }
        }

        public void Step9(BaseEventData eventData)
        {

            if (m_SpecializedInjectionTriggerEntery != null)
            {
                m_SpecializedInjectionTriggerEntery.callback.RemoveListener(Step9);
                m_SpecializedInjectionTriggerEntery = null;
            }

            m_DialogStep6.SetActive(false);
            m_GestureStep6.SetActive(false);
            m_SpecializedInjectionCollider.enabled = false;

            TutorialFocusManager.StopAllFocusInstantly();
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            // CameraController.SetEnvironemntInteractiblity(false);

            // CameraController.SetEnvironemntInteractiblity(true);
            TutorialFocusManager.FocusAtPosition(m_BedTrigger.transform.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
            m_DialogStep9.SetActive(true);
            m_GestureStep3.SetActive(true);
            m_BedCollider.enabled = true;

            m_BedTriggerEntry = m_BedTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
            if (m_BedTriggerEntry != null)
            {
                m_BedTriggerEntry.callback.AddListener(Step10);
            }
            /* CoroutineManager.LateAction(() =>
            {
            }, m_Step9Delay); */
        }

        public void Step10(BaseEventData eventData)
        {
            if (m_BedTriggerEntry != null)
            {
                m_BedTriggerEntry.callback.RemoveListener(Step10);
                m_BedTriggerEntry = null;
            }

            m_DialogStep9.SetActive(false);
            m_GestureStep3.SetActive(false);
            m_Holder.SetActive(false);

            m_InjectionCollider.enabled = true;
            foreach (var bed in m_Beds)
            {
                bed.TutorialLock(false);
            }
            foreach (var collider in m_OtherColliders)
            {
                collider.enabled = true;
            }

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            UIManager.UIInteractionOn();
            CustomerManager.CustomerGenerationOn();
            m_Player.UnpauseGreetingReaction();
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(false);

            Stop();
        }
    }
}
