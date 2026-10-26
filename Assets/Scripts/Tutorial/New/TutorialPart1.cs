using UnityEngine;
using UnityEngine.UI;
using Isometric.UI;
using Isometric.Data;
using Isometric.Cam;
using System.Collections;

namespace Isometric.Tutorial
{
    public class TutorialPart1 : TutorialContainer
    {
        [SerializeField] GameObject m_Canvas;

        [Header("---Step 1---")]
        [SerializeField] string m_KeySubPart1;
        [SerializeField] GameObject m_DialogScreen1;

        [Header("---Step 2---")]
        [SerializeField] string m_KeySubPart2;
        [SerializeField] Button m_MenuPlayButton;
        [SerializeField] Vector2 m_MenuPlayButtonFocusSize = new Vector2(2.15f, 2.15f);
        [SerializeField] Vector2 m_MenuPlayButtonFocusOffset;
        [SerializeField] GameObject m_MenuPlayButtonGestureTap;
        

        public override void Play()
        {
            if (DataManager.GetBool(m_KeySubPart1, false) == false)
            {
                TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);
                CameraController.InteractabilityForTutorial(false);
                m_Canvas.SetActive(true);
                PlaySubPart1();
            }
            else if (DataManager.GetBool(m_KeySubPart2, false) == false)
            {
                TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);
                CameraController.InteractabilityForTutorial(false);
                m_Canvas.SetActive(true);
                PlaySubPart2();
            }
            else
            {
                Stop();
            }
        }

        #region Sub-Part 1
        public void PlaySubPart1()
        {
            UIManager.UIInteractionOff();
            m_DialogScreen1.SetActive(true);
        }
        public void StopSubPart1()
        {
            m_DialogScreen1.SetActive(false);
            DataManager.SetBool(m_KeySubPart1, true);
            PlaySubPart2();
        }
        #endregion

        #region Sub-Part 2
        public void PlaySubPart2()
        {
            UIManager.UIInteractionOn();
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
            m_Canvas.SetActive(false);
            TutorialFocusManager.StopAllFocus();
            m_MenuPlayButtonGestureTap.SetActive(false);
            m_MenuPlayButton.onClick.RemoveListener(HideMenuPlayButtonFocus);
            CameraController.InteractabilityForTutorial(true);
            DataManager.SetBool(m_KeySubPart2, true);
            Stop();
        }
        #endregion

        public override void Stop()
        {
            base.Stop();
        }
    }
}