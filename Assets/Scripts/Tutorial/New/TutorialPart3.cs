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
        [SerializeField] GameObject m_DialogScreenStep1;

        [Header("---Step 2---")]
        [SerializeField] string m_KeySubPart1;
        [SerializeField] UIManager m_UIManager;
        [SerializeField] Button m_StarItemUnlockingButton;
        [SerializeField] Vector2 m_StarItemUnlockingButtonFocusSize = new Vector2(2.15f, 2.15f);
        [SerializeField] Vector2 m_StarItemUnlockingButtonFocusOffset;

        [Header("---Step 3---")]
        [SerializeField] GameObject m_DialogScreenStep3;
        [SerializeField] GameObject m_GestureArrowStep3;
        [SerializeField] Transform m_StarCurrencyHolder;
        [SerializeField] Vector2 m_StarCurrencyHolderFocusSize = new Vector2(2.15f, 2.15f);
        [SerializeField] Vector2 m_StarCurrencyHolderFocusOffset;

        [Header("---Step 4---")]
        [SerializeField] GameObject m_DialogScreenStep4;
        [SerializeField] GameObject m_GestureTapStep4;

        [Header("---Step 6---")]
        [SerializeField] GameObject m_DialogScreenStep6;
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
                PlayStep6();
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
            m_DialogScreenStep1.SetActive(true);
        }

        public void PlayStep2()
        {
            m_DialogScreenStep1.SetActive(false);
            
            // Order matters
            UIManager.GetPopup<StarItemUnlockingUIManager>().OnStarItemUnlockingPopupOpened += PlayStep3;
            UIManager.CheckNextUpdatable();
            m_StarItemUnlockingButton.onClick.AddListener(PlayStep5);
        }

        public void PlayStep3()
        {
            UIManager.GetPopup<StarItemUnlockingUIManager>().OnStarItemUnlockingPopupOpened -= PlayStep3;
            m_DialogScreenStep3.SetActive(true);
            m_GestureArrowStep3.SetActive(true);
            TutorialManager.ActivateTapToContinueText(TutorailTextPosition.Down);
            TutorialFocusManager.FocusAtTransform(m_StarCurrencyHolder, m_StarCurrencyHolderFocusOffset, m_StarCurrencyHolderFocusSize, FocusShapeType.LongRectangle);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.SetBackgroundButtonCallback(PlayStep4);
        }

        public void PlayStep4()
        {
            UIManager.UIInteractionOn();
            m_DialogScreenStep3.SetActive(false);
            m_GestureArrowStep3.SetActive(false);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialManager.DeactivateTapToContinueText();
            TutorialFocusManager.FocusAtTransform(m_StarItemUnlockingButton.transform, m_StarItemUnlockingButtonFocusOffset, m_StarItemUnlockingButtonFocusSize, FocusShapeType.LongRectangle);
            m_DialogScreenStep4.SetActive(true);
            m_GestureTapStep4.SetActive(true);
        }

        public void PlayStep5()
        {
            m_StarItemUnlockingButton.onClick.RemoveListener(PlayStep5);
            SaveSubPart1();
            m_DialogScreenStep4.SetActive(false);
            m_GestureTapStep4.SetActive(false);
            TutorialFocusManager.StopAllFocus();
            StartCoroutine(HoldingStep5());
        }
        private IEnumerator HoldingStep5()
        {
            yield return new WaitWhile(() => m_HasMenuShownAfterUpdatables == false);
            PlayStep6();
        }

        private void SaveSubPart1()
        {
            DataManager.SetBool(m_KeySubPart1, true);
        }

        public void PlayStep6()
        {
            UIManager.UIInteractionOn();
            m_Canvas.SetActive(true);
            m_DialogScreenStep6.SetActive(true);
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
            m_DialogScreenStep6.SetActive(false);
            m_Canvas.SetActive(false);
            TutorialFocusManager.StopAllFocus();
            m_MenuPlayButtonGestureTap.SetActive(false);
            m_MenuPlayButton.onClick.RemoveListener(HideMenuPlayButtonFocus);
            CameraController.InteractabilityForTutorial(true);
            Stop();
        }
    }
}