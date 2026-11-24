using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Isometric.Data;
using Isometric.Sound;
using Arc;
using UnityEngine.Events;
using CustomVibrations;

namespace Isometric.UI
{
    public class SettingUIManager : UIPopupBase
    {
        [SerializeField] GameObject m_Popup;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;
        [SerializeField] SlideButtonUI m_SoundSideButton;
        [SerializeField] SlideButtonUI m_MusicSideButton;
        [SerializeField] SlideButtonUI m_VibrationSideButton;

        [Header("---Links---")]
        [SerializeField] string m_FeedbackEmail = "support@example.com"; // your email
        [SerializeField] string m_PlayStoreURL = "https://play.google.com/store/apps/details?id=com.fizzyplay.my.happy.hospital.doctor.clinic.games.f2p"; // your Play Store link

        private bool m_IsSoundOn;
        private bool m_IsMusicOn;
        private bool m_IsVibrationOn;
        public bool IsSoundOn => m_IsSoundOn;
        public bool IsMusicOn => m_IsMusicOn;
        public bool IsVibrationOn => m_IsVibrationOn;


        public override void Setup()
        {
            SetupToggleButtons();
        }
        private void SetupToggleButtons()
        {
            m_IsSoundOn = DataManager.GetBool(SoundCategroy.Sound.ToString(), true);   //mrcHefF
            m_IsMusicOn = DataManager.GetBool(SoundCategroy.Music.ToString(), true);   //mrcHefF
            m_IsVibrationOn = DataManager.GetBool(VibrationCategory.Vibration.ToString(), true);   //mrcHefF

            if(m_IsSoundOn)
            {
                m_SoundSideButton.SetOn(false);
            }
            else
            {
                m_SoundSideButton.SetOff(false);
            }

            if (m_IsMusicOn)
            {
                m_MusicSideButton.SetOn(false);
            }
            else
            {
                m_MusicSideButton.SetOff(false);
            }

            if (m_IsVibrationOn)
            {
                m_VibrationSideButton.SetOn(false);
            }
            else
            {
                m_VibrationSideButton.SetOff(false);
            }
        }

        public override void OpenPopup(Action onComplete)
        {
            // To update the current states if values are changed somewhere else during gameplay
            SetupToggleButtons();

            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_Popup.SetActive(true);
            m_OpeningSequence.PlaySequence(() =>
            {
                onComplete?.Invoke();
                OnPopupOpened?.Invoke();
            });
        }

        public override void ClosePopup(Action onCompete)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_ClosingSequence.PlaySequence(() =>
            {
                m_Popup.SetActive(false);
                onCompete?.Invoke();
                OnPopupClosed?.Invoke();
            });
        }

        public void SetSoundOn()
        {
            SoundManager.PlaySound(SoundType.ButtonSwitch);
            SoundManager.SetSound(true);
            m_IsSoundOn = true;
            DataManager.SetBool(SoundCategroy.Sound.ToString(), m_IsSoundOn);
        }

        public void SetSoundOff()
        {
            SoundManager.PlaySound(SoundType.ButtonSwitch);
            SoundManager.SetSound(false);
            m_IsSoundOn = false;
            DataManager.SetBool(SoundCategroy.Sound.ToString(), m_IsSoundOn);
        }

        public void SetMusicOn()
        {
            SoundManager.PlaySound(SoundType.ButtonSwitch);
            SoundManager.SetMusic(true);
            m_IsMusicOn = true;
            DataManager.SetBool(SoundCategroy.Music.ToString(), m_IsMusicOn);
        }

        public void SetMusicOff()
        {
            SoundManager.PlaySound(SoundType.ButtonSwitch);
            SoundManager.SetMusic(false);
            m_IsMusicOn = false;
            DataManager.SetBool(SoundCategroy.Music.ToString(), m_IsMusicOn);
        }

        public void SetVibrationOn()
        {
            SoundManager.PlaySound(SoundType.ButtonSwitch);
            VibrationManager.SetVibration(true);
            m_IsVibrationOn = true;
            DataManager.SetBool(VibrationCategory.Vibration.ToString(), m_IsVibrationOn);
        }

        public void SetVibrationOff()
        {
            SoundManager.PlaySound(SoundType.ButtonSwitch);
            VibrationManager.SetVibration(false);
            m_IsVibrationOn = false;
            DataManager.SetBool(VibrationCategory.Vibration.ToString(), m_IsVibrationOn);
        }

        public void OnResetGameButton()
        {
            SoundManager.StopSound(SoundType.GameMusic1);
            SoundManager.StopSound(SoundType.MenuMusic1);
            DataManager.DeleteData();
            DataManager.Setup();
            UIManager.RestartGame();
        }

        public void OnFeedbackButton()
        {
            string subject = Uri.EscapeDataString("Game Feedback");

            string deviceInfo = "\n\n___________________\n" +
                                "Please Do Not Modify This\n\n" +
                                $"Version: {Application.version}\n" +
                                $"Device Name: {SystemInfo.deviceName}\n" +
                                $"Model: {SystemInfo.deviceModel}\n" +
                                $"OS: {SystemInfo.operatingSystem}\n" +
                                $"Resolution: {Screen.width} x {Screen.height} @ {Screen.currentResolution.refreshRateRatio.value:F0}Hz\n" +
                                $"RAM: {SystemInfo.systemMemorySize}\n" +
                                $"Level: {DataManager.CurrentMapLevelIndex + 1}\n" +
                                "_________________________\n";

            string defaultUserMessage = "\n"; // Space left for user to type above the info
            string fullBody = defaultUserMessage + deviceInfo;

            string body = Uri.EscapeDataString(fullBody);
            string mailto = $"mailto:{m_FeedbackEmail}?subject={subject}&body={body}";

            Application.OpenURL(mailto);
        }

        public void OnRatingToGooglePlayButton()
        {
            Application.OpenURL(m_PlayStoreURL);
        }

        public void OnCloseButton()
        {
            UIManager.ClosePopup<SettingUIManager>();
        }
    }
}
