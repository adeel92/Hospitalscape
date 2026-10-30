using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NaughtyAttributes;
using Isometric.Environment;
using Isometric.Cam;
using Isometric.UI;
using Isometric.Customer;
using Isometric.Player;

namespace Isometric.Tutorial
{
    public class TutorialPart15 : TutorialContainer
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
        [SerializeField] GameObject m_DialogStep2;

        [Header("---Step3---")]
        [SerializeField] GameObject m_DialogStep3;
        [SerializeField] GameObject m_DragGestureStep3;
        [SerializeField] CounterTableController m_CounterTable;

        [Header("---Step4---")]
        [SerializeField] GameObject m_DialogStep4;
        [SerializeField] GameObject m_GestureStep4;
        [SerializeField] EventTrigger m_VitaminsTrigger;
        [SerializeField] Collider2D m_VitaminsCollider;
        EventTrigger.Entry m_VitaminsTriggerEntry = null;
        [SerializeField] float m_VitaminsFocusScale;
        [SerializeField] Vector2 m_VitaminsFocusOffset;
        
        [Header("---Step5---")]
        [SerializeField] float m_DelayStep5;
        [SerializeField] GameObject m_DialogStep5;
        [SerializeField] GameObject m_GestureStep5;
        [SerializeField] EventTrigger m_CounterTableEventTrigger;
        [SerializeField] Collider2D m_CounterCollider;
        [SerializeField] float m_CounterTableFocusSize;
        [SerializeField] Vector2 m_CounterTableFocusOffset;
        private EventTrigger.Entry m_CounterTableTriggerEntery = null;

        [Header("---Step6---")]
        [SerializeField] float m_DelayStep6;
        [SerializeField] GameObject m_DialogStep6;

        [Header("---Step7---")]
        [SerializeField] MainServiceCustomerHandler m_TargetBed;
        [SerializeField] List<MainServiceCustomerHandler> m_Beds;
        [SerializeField] GameObject m_DialogStep7;
        [SerializeField] GameObject m_DragGestureStep7;
        [SerializeField] EventTrigger m_BedTrigger;
        [SerializeField] Collider2D m_BedCollider;
        [SerializeField] float m_BedFocusScale;
        [SerializeField] Vector2 m_BedFocusOffset;

        private PlayerController m_Player = null;
        private bool m_HasCustomerDroppedOnCounter = false;


        public override void Play()
        {
            m_CounterTable.OnCustomerDropped.AddListener(CustomerDroppedOnCounter);
            m_Player = PlayerManager.GetPlayerAtIndex(0);
            Step1();
        }

        private void CustomerDroppedOnCounter()
        {
            m_HasCustomerDroppedOnCounter = true;   
        }

        public void Step1()
        {
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);

            m_Holder.SetActive(true);

            LevelManager.SetTimerLock(true);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);
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
            
            m_Holder.SetActive(true);
            m_DialogStep2.SetActive(true);
            TutorialFocusManager.SetBackgroundButtonCallback(Step3);
            TutorialManager.ActivateTapToContinueText(TutorailTextPosition.Down);
        }

        public void Step3()
        {
            CameraController.SetEnvironemntInteractiblity(true);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            m_DialogStep2.SetActive(false);
            TutorialManager.DeactivateTapToContinueText();
            TutorialFocusManager.FocusAtPosition(m_CounterTableEventTrigger.transform.position, m_CounterTableFocusOffset, Vector2.one * m_CounterTableFocusSize, FocusShapeType.Circle);
            m_DialogStep3.SetActive(true);
            m_DragGestureStep3.SetActive(true);
            StartCoroutine(Step3Next());
        }

        public IEnumerator Step3Next()
        {
            yield return new WaitWhile(() => m_HasCustomerDroppedOnCounter == false);
            m_DialogStep3.SetActive(false);
            m_DragGestureStep3.SetActive(false);
            TutorialFocusManager.StopAllFocusInstantly();
            Step4();
        }

        public void Step4()
        {
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_CounterTableEventTrigger.transform.position, m_CounterTableFocusOffset, Vector2.one * m_CounterTableFocusSize, FocusShapeType.Circle);
            TutorialFocusManager.FocusAtPosition(m_VitaminsTrigger.transform.position, m_VitaminsFocusOffset, Vector2.one * m_VitaminsFocusScale, FocusShapeType.Circle);

            m_DialogStep4.SetActive(true);
            m_GestureStep4.SetActive(true);
            m_VitaminsCollider.enabled = true;

            m_VitaminsTriggerEntry = m_VitaminsTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
            if (m_VitaminsTriggerEntry != null)
            {
                m_VitaminsTriggerEntry.callback.AddListener(Step4Next);
            }
        }
            

        public void Step4Next(BaseEventData eventData)
        {
            if (m_VitaminsTriggerEntry != null)
            {
                m_VitaminsTriggerEntry.callback.RemoveListener(Step4Next);
                m_VitaminsTriggerEntry = null;
            }

            TutorialFocusManager.StopAllFocus();
            CameraController.SetEnvironemntInteractiblity(false);
            m_DialogStep4.SetActive(false);
            m_GestureStep4.SetActive(false);
            m_VitaminsCollider.enabled = false;

            Step5();
        }

        public void Step5()
        {
            CoroutineManager.LateAction(() =>
            {
                CameraController.SetEnvironemntInteractiblity(true);
                TutorialFocusManager.RemoveBackgroundButtonCallback();
                TutorialFocusManager.FocusAtPosition(m_CounterTableEventTrigger.transform.position, m_CounterTableFocusOffset, Vector2.one * m_CounterTableFocusSize, FocusShapeType.Circle);
                m_DialogStep5.SetActive(true);
                m_GestureStep5.SetActive(true);
                m_CounterCollider.enabled = true;

                m_CounterTableTriggerEntery = m_CounterTableEventTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
                if (m_CounterTableTriggerEntery != null)
                {
                    m_CounterTableTriggerEntery.callback.AddListener(Step5Next);
                }
            }, m_DelayStep5);
        }

        public void Step5Next(BaseEventData eventData)
        {
            if (m_CounterTableTriggerEntery != null)
            {
                m_CounterTableTriggerEntery.callback.RemoveListener(Step5Next);
                m_CounterTableTriggerEntery = null;
            }

            m_DialogStep5.SetActive(false);
            m_GestureStep5.SetActive(false);
            m_CounterCollider.enabled = false;
            TutorialFocusManager.StopAllFocus();
            CameraController.SetEnvironemntInteractiblity(false);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            
            Step6();
        }

        public void Step6()
        {
            CoroutineManager.LateAction(() =>
            {
                TutorialFocusManager.FocusAtPosition(m_CounterTableEventTrigger.transform.position, m_CounterTableFocusOffset, Vector2.one * m_CounterTableFocusSize, FocusShapeType.Circle);
                m_DialogStep6.SetActive(true);

                CoroutineManager.LateAction(Step7, m_CounterTable.DurationProperty + 0.5f);
            }, m_DelayStep6);
        }

        public void Step7()
        {
            TutorialFocusManager.StopAllFocusInstantly();
            m_DialogStep6.SetActive(false);
            m_DialogStep7.SetActive(true);
            m_DragGestureStep7.SetActive(true);

            CameraController.SetEnvironemntInteractiblity(true);
            TutorialFocusManager.FocusAtTransform(m_TargetCustomer.transform, m_FocusOnCustomerOffset, Vector2.one * m_FocusOnCustomerSize, FocusShapeType.Circle);
            TutorialFocusManager.FocusAtPosition(m_BedTrigger.transform.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);

            StartCoroutine(Step7Next());
        }

        private IEnumerator Step7Next()
        {
            m_BedCollider.enabled = true;
            m_TargetBed.TutorialLock(false);
            yield return new WaitWhile(() => m_TargetCustomer.IsCustomerOnSalonChair() == false);
            
            m_DialogStep7.SetActive(false);
            m_DragGestureStep7.SetActive(false);
            
            Step8();
        }

        public void Step8()
        {
            m_CounterTable.OnCustomerDropped.RemoveListener(CustomerDroppedOnCounter);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            m_Holder.SetActive(false);

            foreach (var bed in m_Beds)
            {
                bed.TutorialLock(false);
            }

            foreach (var collider in m_OtherColliders)
            {
                collider.enabled = true;
            }

            m_BedCollider.enabled = false;

            m_Player.UnpauseGreetingReaction();
            UIManager.UIInteractionOn();
            CustomerManager.CustomerGenerationOn();

            LevelManager.SetTimerLock(false);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(false);

            Stop();
        }
    }
}
