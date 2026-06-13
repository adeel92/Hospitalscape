using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Isometric.UI;

namespace Isometric.Tutorial
{
    public class TutorialPart4 : TutorialContainer
    {
        [SerializeField] UpgradePopupUIManager m_UpgradePopupUIManager;
        [SerializeField] float m_FocusDelay;
        [SerializeField] Vector2 m_FocusScale;
        [SerializeField] Vector2 m_FocusOffset;
        [SerializeField] GameObject m_Popup;
        [SerializeField] GameObject m_Dialog;

        public override void Play()
        {
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Down);

            UIManager.FullUIInteractionOff();
            m_UpgradePopupUIManager.ScrollRectOff();

            Transform playerPanel = m_UpgradePopupUIManager.GetPanelAtSiblingIndex(0);
            if (playerPanel.TryGetComponent(out PlayerUpgradePanelUI playerUpgradePanelUI))
            {
                Step1(playerUpgradePanelUI);
            }
            else
            {
                Stop();
            }
        }

        public void Step1(PlayerUpgradePanelUI playerUpgradePanelUI)
        {
            UIManager.FullUIInteractionOn();

            m_Popup.SetActive(true);
            m_Dialog.SetActive(true);
            Transform upgradeButton = playerUpgradePanelUI.UpgradeButton.transform;
            TutorialFocusManager.FocusAtPosition(upgradeButton.position, m_FocusOffset, Vector2.one * m_FocusScale, FocusShapeType.LongRectangle);
            playerUpgradePanelUI.UpgradeButton.onClick.AddListener(Step2);
        }

        public void Step2()
        {
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            m_Popup.SetActive(false);
            m_Dialog.SetActive(false);

            m_UpgradePopupUIManager.ScrollRectOn();
            Stop();
        }
    }
}
