using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Isometric.UI;
using Isometric.Cam;

namespace Isometric.Tutorial
{
    public class TutorialPart3 : TutorialContainer
    {
        [SerializeField] GameObject m_Holder;
        [SerializeField] GameObject m_Dialog;
        [SerializeField] Button m_PlayButtonFocus;
        [SerializeField] float m_PlayButtonFocusScale;
        [SerializeField] Vector2 m_PlayButtonFocusOffset;
        [SerializeField] float m_Delay;

        public override void Play()
        {
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);

            CameraController.InteractabilityForTutorial(false);
            UIManager.CheckNextUpdatable();
            UIManager.FullUIInteractionOff();
            CoroutineManager.LateAction(Step1, m_Delay);
        }

        public void Step1()
        {
            UIManager.FullUIInteractionOn();
            m_Holder.SetActive(true);
            m_Dialog.SetActive(true);
            TutorialFocusManager.FocusAtTransform(m_PlayButtonFocus.transform, m_PlayButtonFocusOffset, Vector2.one * m_PlayButtonFocusScale, FocusShapeType.Circle);

            m_PlayButtonFocus.onClick.AddListener(Step2);
        }

        public void Step2()
        {
            m_PlayButtonFocus.onClick.RemoveListener(Step2);
            m_Holder.SetActive(false);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            CameraController.InteractabilityForTutorial(true);

            Stop();
        }
    }
}