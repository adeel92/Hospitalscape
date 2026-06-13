using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Isometric.UI;
using Isometric.Cam;
using Isometric.Customer;
using Isometric.Environment;

namespace Isometric.Tutorial
{
    public class TutorialPart14 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;

        [Header("---Step1---")]
        [SerializeField] float m_Step1Delay;

        [Header("---Step2---")]
        [SerializeField] Button m_InstantOrderBoosterFocus;
        [SerializeField] float m_InstantOrderBoosterSize;
        [SerializeField] Vector2 m_InstantOrderBoosterOffset;
        [SerializeField] TextMeshProUGUI m_InstantOrderBoosterText;
        [SerializeField] GameObject m_DialogStep2;
        [SerializeField] GameObject m_GestureStep2;

        public override void Play()
        {
            Step1();
        }

        public void Step1()
        {
            StartCoroutine(Steping1());
        }

        IEnumerator Steping1()
        {
            yield return new WaitForSeconds(m_Step1Delay);

            while (true)
            {
                var orders = CustomerManager.GetCurrentWaitressOrders();

                if (orders.Count > 0)
                {
                    break;
                }

                yield return null;
            }

            Step2();
        }

        public void Step2()
        {
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);

            CustomerManager.CustomerGenerationOff();
            TutorialFocusManager.FocusAtPosition(m_InstantOrderBoosterFocus.transform.position, m_InstantOrderBoosterOffset, Vector2.one * m_InstantOrderBoosterSize, FocusShapeType.Circle);

            m_Holder.SetActive(true);
            m_DialogStep2.SetActive(true);
            m_GestureStep2.SetActive(true);
            m_InstantOrderBoosterFocus.onClick.AddListener(Step2Next);

            LevelManager.SetTimerLock(true);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);
        }

        public void Step2Next()
        {
            m_InstantOrderBoosterFocus.onClick.RemoveListener(Step2Next);

            m_DialogStep2.SetActive(false);
            m_GestureStep2.SetActive(false);
            m_Holder.SetActive(false);

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            CameraController.SetEnvironemntInteractiblity(true);
            CustomerManager.CustomerGenerationOn();

            LevelManager.SetTimerLock(false);
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(false);

            Stop();
        }
    }
}
