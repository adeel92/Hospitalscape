using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Isometric.UI;
using Isometric.Data;
using Isometric.Cam;

namespace Isometric.Tutorial
{
    public class TutorialPart1 : TutorialContainer
    {
        [SerializeField] GameObject m_Canvas;

        [Header("---Step 1---")]
        [SerializeField] string m_KeySubPart1;
        [SerializeField] GameObject m_DialogScreen1;
        [SerializeField] GameObject m_DialogScreen2;

        [Header("---Step 2---")]
        [SerializeField] GameObject m_Dialog3;
        [SerializeField] float m_DelayOnStarFocus;
        [SerializeField] Transform m_UnlockStarFocus;
        [SerializeField] Vector2 m_StarFocusOffset;
        [SerializeField] Vector2 m_StarFocusSize = new Vector2(0.8f, 0.8f);

        [Header("---Step 3---")]
        [SerializeField] GameObject m_Dialog4;
        [SerializeField] Button m_UnlockStarItemButton;
        [SerializeField] Vector2 m_StarItemButtonOffset;
        [SerializeField] Vector2 m_StarItemButtonSize = new Vector2(0.8f, 0.8f);
        [SerializeField] float m_DelayOnPlayButtonFocus;


        [Header("---Step 4---")]
        [SerializeField] string m_KeySubPart2;
        [SerializeField] float m_DirectStepDelay = 1;
        [SerializeField] GameObject m_Dialog5;
        [SerializeField] Button m_PlayButtonFocus;
        [SerializeField] Vector2 m_PlayButtonFocusSize = new Vector2(2.15f, 2.15f);
        [SerializeField] Vector2 m_PlayButtonFocusOffset;

        public override void Play()
        {
            if (DataManager.GetBool(m_KeySubPart1, false) == false)
            {
                TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);

                CameraController.InteractabilityForTutorial(false);
                m_Canvas.SetActive(true);
                PlayDialogScreen1();
            }
            else if (DataManager.GetBool(m_KeySubPart2, false) == false)
            {
                TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);

                CameraController.InteractabilityForTutorial(false);
                UIManager.CheckNextUpdatable();
                UIManager.FullUIInteractionOff();
                CoroutineManager.LateActionRealTime(() =>
                {
                    PlayButtonFocus();
                }, m_DirectStepDelay);
            }
            else
            {
                Stop();
            }
        }

        public void PlayDialogScreen1()
        {
            m_DialogScreen1.SetActive(true);
        }

        public void PlayDialogScreen2()
        {
            m_DialogScreen1.SetActive(false);
            m_DialogScreen2.SetActive(true);
        }

        public void PlayNextUpdatabeUnlock()
        {
            m_DialogScreen2.SetActive(false);
            UIManager.CheckNextUpdatable();
            UIManager.FullUIInteractionOff();
            CoroutineManager.LateActionRealTime(() =>
            {
                TutorialFocusManager.FocusAtPosition(m_UnlockStarFocus.position, m_StarFocusOffset, m_StarFocusSize, FocusShapeType.LongRectangle);
                TutorialFocusManager.RemoveBackgroundButtonCallback();
                TutorialFocusManager.SetBackgroundButtonCallback(PlayFocusOnUnlockStarItem);

                m_Dialog3.SetActive(true);
                UIManager.FullUIInteractionOn();
            }, m_DelayOnStarFocus);
        }

        public void PlayFocusOnUnlockStarItem()
        {
            m_Dialog3.SetActive(false);
            m_Dialog4.SetActive(true);
            m_UnlockStarItemButton.onClick.AddListener(() =>
            {
                UIManager.FullUIInteractionOff();
                TutorialFocusManager.RemoveBackgroundButtonCallback();
                TutorialFocusManager.StopAllFocus();
                m_Dialog4.SetActive(false);
                DataManager.SetBool(m_KeySubPart1, true);
                CoroutineManager.LateActionRealTime(PlayButtonFocus, m_DelayOnPlayButtonFocus);
            });
            TutorialFocusManager.FocusAtPosition(m_UnlockStarItemButton.transform.position, m_StarItemButtonOffset, m_StarItemButtonSize, FocusShapeType.LongRectangle);
        }

        public void PlayButtonFocus()
        {
            m_Canvas.SetActive(true);
            m_Dialog5.SetActive(true);
            UIManager.FullUIInteractionOn();
            m_PlayButtonFocus.onClick.AddListener(() =>
            {
                m_Canvas.SetActive(false);
                DataManager.SetBool(m_KeySubPart2, true);
                TutorialFocusManager.RemoveBackgroundButtonCallback();
                TutorialFocusManager.StopAllFocus();
                CameraController.InteractabilityForTutorial(true);
                Stop();
            });
            TutorialFocusManager.FocusAtTransform(m_PlayButtonFocus.transform, m_PlayButtonFocusOffset, m_PlayButtonFocusSize, FocusShapeType.Circle);
        }

        public override void Stop()
        {
            base.Stop();
        }
    }
}