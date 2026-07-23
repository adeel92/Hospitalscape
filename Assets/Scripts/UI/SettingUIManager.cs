using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Isometric.Data;
using Isometric.Sound;
using Arc;
using UnityEngine.Events;

namespace Isometric.UI
{
    public class SettingUIManager : UIPopupBase
    {
        [SerializeField] GameObject m_Popup;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;
        [SerializeField] SlideButtonUI m_SoundSideButton;
        [SerializeField] SlideButtonUI m_MusicSideButton;

        private bool m_IsSoundOn;
        private bool m_IsMusicOn;
        public bool IsSoundOn => m_IsSoundOn;
        public bool IsMusicOn => m_IsMusicOn;


        public override void Setup()
        {
            SetupToggleButtons();
        }
        private void SetupToggleButtons()
        {
            m_IsSoundOn = DataManager.GetBool(SoundCategroy.Sound.ToString(), true);   //mrcHefF
            m_IsMusicOn = DataManager.GetBool(SoundCategroy.Music.ToString(), true);   //mrcHefF

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

        public void OnResetGameButton()
        {
            SoundManager.StopSound(SoundType.GameMusic1);
            SoundManager.StopSound(SoundType.MenuMusic1);
            DataManager.DeleteData();
            DataManager.Setup();
            UIManager.RestartGame();
        }

        public void OnCloseButton()
        {
            UIManager.ClosePopup<SettingUIManager>();
        }
    }
}
