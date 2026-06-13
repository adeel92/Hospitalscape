using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Isometric.Data;
using Isometric.Sound;
using Arc;

namespace Isometric.UI
{
    public class SettingUIManager : UIPopupBase
    {
        [SerializeField] GameObject m_Popup;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;
        [SerializeField] SlideButtonUI m_SoundSideButton;
        [SerializeField] SlideButtonUI m_MusicSideButton;


        public override void Setup()
        {
            bool isSoundOn = DataManager.GetBool(SoundCategroy.Sound.ToString(), true);
            bool isMusicOn = DataManager.GetBool(SoundCategroy.Music.ToString(), true);

            if(isSoundOn)
            {
                m_SoundSideButton.SetOn(false);
            }
            else
            {
                m_SoundSideButton.SetOff(false);
            }

            if (isMusicOn)
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
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_Popup.SetActive(true);
            m_OpeningSequence.PlaySequence(() =>
            {
                onComplete?.Invoke();
            });
        }

        public override void ClosePopup(Action onCompete)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_ClosingSequence.PlaySequence(() =>
            {
                m_Popup.SetActive(false);
                onCompete?.Invoke();
            });
        }

        public void SetSoundOn()
        {
            SoundManager.PlaySound(SoundType.ButtonSwitch);
            SoundManager.SetSound(true);
            DataManager.SetBool(SoundCategroy.Sound.ToString(), true);
        }

        public void SetSoundOff()
        {
            SoundManager.PlaySound(SoundType.ButtonSwitch);
            SoundManager.SetSound(false);
            DataManager.SetBool(SoundCategroy.Sound.ToString(), false);
        }

        public void SetMusicOn()
        {
            SoundManager.PlaySound(SoundType.ButtonSwitch);
            SoundManager.SetMusic(true);
            DataManager.SetBool(SoundCategroy.Music.ToString(), true);
        }

        public void SetMusicOff()
        {
            SoundManager.PlaySound(SoundType.ButtonSwitch);
            SoundManager.SetMusic(false);
            DataManager.SetBool(SoundCategroy.Music.ToString(), false);
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
