using UnityEngine;
using UnityEngine.UI;
using Isometric.UI;
using Isometric.Data;
using Isometric.Cam;
using System.Collections;

namespace Isometric.Tutorial
{
    public class TutorialPart3 : TutorialContainer
    {
        [SerializeField] GameObject m_Canvas;

        [Header("---Step 1---")]
        [SerializeField] GameObject m_DialogScreen1;
        [SerializeField] Button m_MenuPlayButton;
        [SerializeField] Vector2 m_MenuPlayButtonFocusSize = new Vector2(2.15f, 2.15f);
        [SerializeField] Vector2 m_MenuPlayButtonFocusOffset;
        [SerializeField] GameObject m_MenuPlayButtonGestureTap;
        

        public override void Play()
        {
            PlayStep1();
        }

        public void PlayStep1()
        {
            UIManager.UIInteractionOn();
            m_DialogScreen1.SetActive(true);
            ShowMenuPlayButtonFocus();
        }
        private void ShowMenuPlayButtonFocus()
        {
            m_MenuPlayButton.onClick.AddListener(HideMenuPlayButtonFocus);
            TutorialFocusManager.FocusAtTransform(m_MenuPlayButton.transform, m_MenuPlayButtonFocusOffset, m_MenuPlayButtonFocusSize, FocusShapeType.Circle);
            m_MenuPlayButtonGestureTap.SetActive(true);
        }
        private void HideMenuPlayButtonFocus()
        {
            TutorialManager.DeactivateTutorailText();
            m_DialogScreen1.SetActive(false);
            m_Canvas.SetActive(false);
            TutorialFocusManager.StopAllFocus();
            m_MenuPlayButtonGestureTap.SetActive(false);
            m_MenuPlayButton.onClick.RemoveListener(HideMenuPlayButtonFocus);
            CameraController.InteractabilityForTutorial(true);
            Stop();
        }
        

        public override void Stop()
        {
            base.Stop();
        }
    }
}