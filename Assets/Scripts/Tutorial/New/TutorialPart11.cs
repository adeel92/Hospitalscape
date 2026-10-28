using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using DG.Tweening;
using Isometric.Data;
using Isometric.Environment;
using Isometric.Customer;
using Isometric.UI;
using Isometric.Cam;
using Isometric.Player;

namespace Isometric.Tutorial
{
    public class TutorialPart11 : TutorialContainer
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
        [SerializeField] Collider2D m_BedCollider;
        [SerializeField] Transform m_BedFocus;
        [SerializeField] float m_BedFocusScale;
        [SerializeField] Vector2 m_BedFocusOffset;
        [SerializeField] GameObject m_DragGestureStep2;

        [Header("---Step3---")]
        [SerializeField] SpriteButton m_SpriteButtonStep3;
        [SerializeField] GameObject m_GestureStep3;

        [Header("---Step4---")]
        [SerializeField] DataConsumable m_TargetFoodType;
        [SerializeField] StationCustomerInHandler m_StationCustomerInHandler;
        [SerializeField] Vector2 m_StationFocusSize;
        [SerializeField] Vector2 m_StationFocusOffset;
        [SerializeField] GameObject m_DialogStep4;
        [SerializeField] GameObject m_GestureStep4;

        [Header("---Step5---")]
        [SerializeField] GameObject m_DialogStep5;

        private PlayerController m_Player = null;


        public override void Play()
        {
            m_Player = PlayerManager.GetPlayerAtIndex(0);
            Step1();
        }

        public void Step1()
        {
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);

            LevelManager.SetTimerLock(true);
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
            CameraController.SetEnvironemntInteractiblity(false);
            TutorialFocusManager.StopAllFocus();

            Step4();
        }

        public void Step4()
        {
            StartCoroutine(CheckMachineOrder());
        }

        IEnumerator CheckMachineOrder()
        {
            while (true)
            {
                Tuple<DataConsumable, Vector3> target = null;
                CustomerSalonController targetCustomer = null;

                List<Tuple<DataConsumable, Vector3>> orders = m_TargetBed.MainServiceController.GetCurrentPlayerOrders();
                if (orders != null)
                {
                    foreach (var order in orders)
                    {
                        if (order.Item1 == m_TargetFoodType)
                        {
                            target = order;
                            targetCustomer = m_TargetBed.CurrentCustomer;
                            break;
                        }
                    }
                }

                yield return null;

                if (targetCustomer != null)
                {
                    break;
                }
            }

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(m_StationCustomerInHandler.transform.position, m_StationFocusOffset, m_StationFocusSize, FocusShapeType.Circle);
            TutorialFocusManager.FocusAtPosition(m_BedFocus.position, m_BedFocusOffset, Vector2.one * m_BedFocusScale, FocusShapeType.Circle);
            CameraController.SetEnvironemntInteractiblity(true);
            m_Holder.SetActive(true);
            m_DialogStep4.SetActive(true);
            m_GestureStep4.SetActive(true);

            yield return new WaitWhile(() => m_StationCustomerInHandler.HasCustomerTreatmentStarted == false);
            Step5();
        }

        public void Step5()
        {
            TutorialFocusManager.StopAllFocusInstantly();
            CameraController.SetEnvironemntInteractiblity(false);
            m_DialogStep4.SetActive(false);
            m_GestureStep4.SetActive(false);

            m_DialogStep5.SetActive(true);

            TutorialFocusManager.FocusAtPosition(m_StationCustomerInHandler.transform.position, m_StationFocusOffset, m_StationFocusSize, FocusShapeType.Circle);
            CoroutineManager.LateAction(Step6, m_StationCustomerInHandler.DurationProperty);
        }

        public void Step6()
        {
            foreach (var bed in m_Beds)
            {
                bed.TutorialLock(false);
            }

            foreach (var collider in m_OtherColliders)
            {
                collider.enabled = true;
            }

            CameraController.SetEnvironemntInteractiblity(true);
            m_Holder.SetActive(false);
            m_DialogStep5.SetActive(false);

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();

            LevelManager.SetTimerLock(false);
            m_Player.UnpauseGreetingReaction();
            UIManager.UIInteractionOn();
            CustomerManager.CustomerGenerationOn();

            Stop();
        }
    }
}
