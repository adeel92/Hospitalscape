using Arc;
using Isometric.UI;
using UnityEngine;

public class SettingBarAnimationHandler : MonoBehaviour
{
    [SerializeField] SettingBarType m_SettingBarType;

    [Header("---Core References---")]
    [SerializeField] SettingUIManager m_SettingsUIManager;
    [SerializeField] SlideButtonUI m_SlideButtonUI;

    [Header("---Toggle Bacteria---")]
    [SerializeField] PlayDoTween m_BacteriaEnableTween;
    [SerializeField] PlayDoTween m_BacteriaDisableTween;
    [SerializeField] ParticleSystem m_SleepParticle;

    private void OnEnable()
    {
        m_SlideButtonUI.OnToggleOn.AddListener(OnToggleOn);
        m_SlideButtonUI.OnToggleOff.AddListener(OnToggleOff);
        m_SettingsUIManager.OnPopupOpened.AddListener(OnPopupOpened);
    }
    private void OnDisable()
    {
        m_SlideButtonUI.OnToggleOn.RemoveListener(OnToggleOn);
        m_SlideButtonUI.OnToggleOff.RemoveListener(OnToggleOff);
        m_SettingsUIManager.OnPopupOpened.RemoveListener(OnPopupOpened);
    }

    private void OnPopupOpened()
    {
        if (!IsToggleOn())
        {
            m_SleepParticle.Play();
        }
    }

    private bool IsToggleOn()
    {
        switch (m_SettingBarType)
        {
            case SettingBarType.Music:
            return m_SettingsUIManager.IsMusicOn;

            case SettingBarType.Sound:
            return m_SettingsUIManager.IsSoundOn;

            default:
            return true;
        }
    }
    private void OnToggleOn()
    {
        m_BacteriaEnableTween.Stop();
        m_BacteriaDisableTween.Play();
    }
    private void OnToggleOff()
    {
        m_BacteriaDisableTween.Stop();
        m_BacteriaEnableTween.Play();
    }


    public enum SettingBarType
    {
        Music, Sound
    }
}
