using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Arc;
using Isometric.Data;
using Isometric.Sound;
using TMPro;

namespace Isometric.UI
{
    public class PauseUIManager : UIPopupBase
    {
        [SerializeField] GameObject m_Popup;

        [Header("---Pause---")]
        [SerializeField] GameObject m_PausePopup;
        [SerializeField] PlayDoTweenSequence m_PauseOpeningSequence; 
        [SerializeField] PlayDoTweenSequence m_PauseClosingSequence;
        [Space, SerializeField] TextMeshProUGUI m_MapNameText;
        [SerializeField] TextMeshProUGUI m_LevelText;
        [SerializeField] GameObject m_SoundOffObj;
        [SerializeField] GameObject m_MusicOffObj;
        private bool m_IsSoundOn;
        private bool m_IsMusicOn;

        [Header("---Warning---")]
        [SerializeField] GameObject m_WarningPopup;
        [SerializeField] PlayDoTweenSequence m_WarningOpeningSequence;
        [SerializeField] PlayDoTweenSequence m_WarningClosingSequence;

        public override void Setup() {}

        public void SetupForGameplay()
        {
            m_MapNameText.text = DataManager.GetCurrentMapName();
            m_LevelText.text = "Level " + (DataManager.CurrentMapLevelIndex + 1).ToString();
            m_IsSoundOn = DataManager.GetBool(SoundCategroy.Sound.ToString(), true);
            m_IsMusicOn = DataManager.GetBool(SoundCategroy.Music.ToString(), true);
            m_SoundOffObj.SetActive(!m_IsSoundOn);
            m_MusicOffObj.SetActive(!m_IsMusicOn);
            m_Popup.SetActive(true);
        }

        #region  Pause Popup
        public override void OpenPopup(Action onComplete)
        {
            UIManager.CameraEnvironmentInteractionOff();
            UIManager.UIInteractionOff();
            UIManager.PauseGame();
            m_PausePopup.SetActive(true);
            m_PauseOpeningSequence.PlaySequence(() =>
            {
                UIManager.UIInteractionOn();
                onComplete?.Invoke();
            });
        }

        public override void ClosePopup(Action onComplete)
        {
            UIManager.UIInteractionOff();
            m_PauseClosingSequence.PlaySequence(() =>
            {
                m_PausePopup.SetActive(false);
                UIManager.UIInteractionOn();
                UIManager.CameraEnvironmentInteractionOn();
                UIManager.UnPauseGame();
                onComplete?.Invoke();
            });
        }

        public void OnPauseButton()
        {
            GlobalEventHolder.OnPlayerStopMoving?.Invoke();
            OpenPopup(null);
        }

        public void OnGoToMenuButton()
        {
            OpenWarningPopup();
        }

        public void OnUnPauseButton()
        {
            ClosePopup(null);
        }

        public void ToggleSound()
        {
            SoundManager.PlaySound(SoundType.ButtonSwitch);
            m_IsSoundOn = !m_IsSoundOn;
            m_SoundOffObj.SetActive(!m_IsSoundOn);
            SoundManager.SetSound(m_IsSoundOn);
            DataManager.SetBool(SoundCategroy.Sound.ToString(), m_IsSoundOn);
        }
        public void ToggleMusic()
        {
            SoundManager.PlaySound(SoundType.ButtonSwitch);
            m_IsMusicOn = !m_IsMusicOn;
            m_MusicOffObj.SetActive(!m_IsMusicOn);
            SoundManager.SetMusic(m_IsMusicOn);
            DataManager.SetBool(SoundCategroy.Music.ToString(), m_IsMusicOn);
        }
        #endregion

        #region Warning Popup
        public void OnWarningQuitButton()
        {
            int levelNumber = DataManager.CurrentMapLevelIndex + 1;
            FirebaseManager.LogEvent("Level_" + levelNumber + "_", FirebaseLogType.GameLeft);

            if (HeartTimeCurrencyCounter.HasHeartTimeCurrency() == false)
            {
                if (DataManager.HeartCurrency > 0)
                {
                    HeartCurrencyUIController.CheckLastTimeSaved();
                    DataManager.HeartCurrency--;
                }
            }

            HeartCurrencyUIController.SetValueForShouldBeMinusOne(false);

            DataManager.SaveData();
            UIManager.RestartGame();
        }

        public void OnCancleButton()
        {
            CloseWarningPopup();
        }

        public void OpenWarningPopup()
        {
            UIManager.CameraEnvironmentInteractionOff();
            UIManager.UIInteractionOff();
            m_WarningPopup.SetActive(true);
            m_WarningOpeningSequence.PlaySequence(() =>
            {
                UIManager.UIInteractionOn();
            });
        }

        public void CloseWarningPopup()
        {
            UIManager.UIInteractionOff();
            m_WarningClosingSequence.PlaySequence(() =>
            {
                m_WarningPopup.SetActive(false);
                UIManager.UIInteractionOn();
            });
        }
        #endregion
    }
}
