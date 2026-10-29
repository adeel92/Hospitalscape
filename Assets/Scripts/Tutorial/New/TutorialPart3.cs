using UnityEngine;
using UnityEngine.UI;
using Isometric.UI;
using Isometric.Data;
using Isometric.Cam;
using System.Collections;
using NaughtyAttributes;

namespace Isometric.Tutorial
{
    public class TutorialPart3 : TutorialContainer
    {
        [SerializeField] GameObject m_Canvas;

        [Header("---Step 1---")]
        [SerializeField] GameObject m_DialogScreen1;

        [Header("---Step 2---")]
        [SerializeField] string m_KeySubPart1;
        [SerializeField] UIManager m_UIManager;
        [SerializeField] Button m_StarItemUnlockingButton;

        [Header("---Step 3---")]
        [SerializeField] GameObject m_DialogScreen2;
        [SerializeField] Button m_MenuPlayButton;
        [SerializeField] Vector2 m_MenuPlayButtonFocusSize = new Vector2(2.15f, 2.15f);
        [SerializeField] Vector2 m_MenuPlayButtonFocusOffset;
        [SerializeField] GameObject m_MenuPlayButtonGestureTap;

        [SerializeField, ReadOnly] private bool m_HasMenuShownAfterUpdatables = false;


        private void OnEnable()
        {
            m_UIManager.OnMenuShownAfterUpdatables += SetHasMenuShownAfterUpdatables;
        }
        private void OnDisable()
        {
            m_UIManager.OnMenuShownAfterUpdatables -= SetHasMenuShownAfterUpdatables;
        }

        public override void Play()
        {
            if(DataManager.GetBool(m_KeySubPart1, false) == false)
            {
                PlayStep1();
            }
            else
            {
                PlayStep3();
            }
        }

        private void SetHasMenuShownAfterUpdatables()
        {
            m_HasMenuShownAfterUpdatables = true;
        }

        public void PlayStep1()
        {
            CameraController.InteractabilityForTutorial(false);
            UIManager.UIInteractionOff();
            m_Canvas.SetActive(true);
            m_DialogScreen1.SetActive(true);
        }

        public void PlayStep2()
        {
            m_DialogScreen1.SetActive(false);
            UIManager.UIInteractionOn();
            
            // Order matters
            UIManager.CheckNextUpdatable();
            m_StarItemUnlockingButton.onClick.AddListener(SaveSubPart1);

            StartCoroutine(HoldingStep2());
        }
        private IEnumerator HoldingStep2()
        {
            yield return new WaitWhile(() => m_HasMenuShownAfterUpdatables == false);
            PlayStep3();
        }

        private void SaveSubPart1()
        {
            DataManager.SetBool(m_KeySubPart1, true);
            m_StarItemUnlockingButton.onClick.RemoveListener(SaveSubPart1);
        }

        public void PlayStep3()
        {
            UIManager.UIInteractionOn();
            m_Canvas.SetActive(true);
            m_DialogScreen2.SetActive(true);
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
            m_DialogScreen2.SetActive(false);
            m_Canvas.SetActive(false);
            TutorialFocusManager.StopAllFocus();
            m_MenuPlayButtonGestureTap.SetActive(false);
            m_MenuPlayButton.onClick.RemoveListener(HideMenuPlayButtonFocus);
            CameraController.InteractabilityForTutorial(true);
            Stop();
        }
    }
}