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
    public class TutorialPart13 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;

        [Header("---Step1---")]
        [SerializeField] float m_Step1Delay;

        [Header("---Step2---")]
        [SerializeField] Button m_SpeedBoosterFocus;
        [SerializeField] float m_SpeedBoosterSize;
        [SerializeField] Vector2 m_SpeedBoosterOffset;
        [SerializeField] TextMeshProUGUI m_SpeedBoosterText;
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
            Step2();
        }

        public void Step2()
        {
            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(true);

            TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);

            CustomerManager.CustomerGenerationOff();
            TutorialFocusManager.FocusAtPosition(m_SpeedBoosterFocus.transform.position, m_SpeedBoosterOffset, Vector2.one * m_SpeedBoosterSize, FocusShapeType.Circle);

            m_Holder.SetActive(true);
            m_DialogStep2.SetActive(true);
            m_GestureStep2.SetActive(true);
            m_SpeedBoosterFocus.onClick.AddListener(Step2Next);
        }

        public void Step2Next()
        {
            m_SpeedBoosterFocus.onClick.RemoveListener(Step2Next);

            GameManager.UnPauseGame();

            m_DialogStep2.SetActive(false);
            m_GestureStep2.SetActive(false);
            m_Holder.SetActive(false);

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            CameraController.SetEnvironemntInteractiblity(true);
            CustomerManager.CustomerGenerationOn();

            GlobalEventHolder.OnCustomerWaitFreeze?.Invoke(false);

            Stop();
        }
    }
}
