using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Isometric.UI;
using NaughtyAttributes;
using UnityEngine.UI;
using Isometric.Data;

namespace Isometric.Tutorial
{
    public class TutorialPart5 : TutorialContainer
    {
        [SerializeField] UpgradePopupUIManager m_UpgradePopupUIManager;
        [SerializeField] GameObject m_Popup;

        [Header("---Step 1")]
        [SerializeField] GameObject m_DialogStep1;

        [Header("---Step 2---")]
        [SerializeField] string m_KeySubPart1;
        // [SerializeField] float m_UpgradeButtonFocusDelay;
        [SerializeField] Vector2 m_UpgradeButtonFocusScale;
        [SerializeField] Vector2 m_UpgradeButtonFocusOffset;
        [SerializeField] GameObject m_DialogStep2;
        [SerializeField] GameObject m_GestureTapStep2;

        [Header("---Step 3---")]
        [SerializeField] Button m_PlayButton;
        // [SerializeField] float m_PlayButtonFocusDelay;
        [SerializeField] Vector2 m_PlayButtonFocusScale;
        [SerializeField] Vector2 m_PlayButtonFocusOffset;
        [SerializeField] GameObject m_DialogStep3;
        [SerializeField] GameObject m_GestureTapStep3;

        [SerializeField, ReadOnly] private PlayerUpgradePanelUI m_PlayerUpgradePanelUI;

        public override void Play()
        {
            if (DataManager.GetBool(m_KeySubPart1, false) == false)
            {
                TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);

                UIManager.FullUIInteractionOff();
                m_UpgradePopupUIManager.ScrollRectOff();

                Transform playerPanel = m_UpgradePopupUIManager.GetPanelAtSiblingIndex(0);
                if (playerPanel.TryGetComponent(out m_PlayerUpgradePanelUI))
                {
                    Step1();
                }
                else
                {
                    Stop();
                }
            }
            else
            {
                Stop();
            }
        }

        public void Step1()
        {
            UIManager.FullUIInteractionOn();

            m_Popup.SetActive(true);
            m_DialogStep1.SetActive(true);
        }

        public void Step2()
        {
            UIManager.FullUIInteractionOn();

            m_DialogStep1.SetActive(false);
            m_DialogStep2.SetActive(true);
            Transform upgradeButton = m_PlayerUpgradePanelUI.UpgradeButton.transform;
            TutorialFocusManager.FocusAtPosition(upgradeButton.position, m_UpgradeButtonFocusOffset, Vector2.one * m_UpgradeButtonFocusScale, FocusShapeType.ShortRectangle);
            m_GestureTapStep2.SetActive(true);
            m_PlayerUpgradePanelUI.UpgradeButton.onClick.AddListener(Step3);
        }

        public void Step3()
        {
            m_PlayerUpgradePanelUI.UpgradeButton.onClick.RemoveListener(Step3);
            TutorialFocusManager.StopAllFocusInstantly();
            m_DialogStep2.SetActive(false);
            m_DialogStep3.SetActive(true);
            TutorialFocusManager.FocusAtTransform(m_PlayButton.transform, m_PlayButtonFocusOffset, Vector2.one * m_PlayButtonFocusScale, FocusShapeType.LongRectangle);
            m_GestureTapStep2.SetActive(false);
            m_GestureTapStep3.SetActive(true);
            m_PlayButton.onClick.AddListener(Step4);
            DataManager.SetBool(m_KeySubPart1, true);
        }

        public void Step4()
        {
            m_PlayButton.onClick.RemoveListener(Step4);
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            m_Popup.SetActive(false);
            m_DialogStep3.SetActive(false);
            m_GestureTapStep3.SetActive(false);

            m_UpgradePopupUIManager.ScrollRectOn();
            Stop();
        }
    }
}
