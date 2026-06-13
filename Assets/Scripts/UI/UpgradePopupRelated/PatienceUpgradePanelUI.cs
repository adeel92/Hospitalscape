using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.Sound;

namespace Isometric.UI
{
    public class PatienceUpgradePanelUI : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        public UpgradePopupUIManager UpgradePopupUIManager;

        [SerializeField, ReadOnly]
        private DataPatience m_DataPatience;

        [Space, ReadOnly]
        public CurrencyType UpgradeCurrency;
        [ReadOnly]
        public int UpgradeCost;

        [Space]
        public TextMeshProUGUI HeaderText;
        public Image PreviewImage;
        public TextMeshProUGUI UpgradeNameText;
        public TextMeshProUGUI UpgradeValueText;
        public StepFillBarUI StepFillBar;
        public Button UpgradeButton;
        public TextMeshProUGUI UpgradeButtonText;
        public GameObject UpgradeButtonCoin;
        public GameObject UpgradeButtonGem;
        public Button MaxButton;
        public GameObject Tick;
        public ParticleSystem UpgradeEffect;
        [ReadOnly]
        public Sprite InfoPreviewSprite;
        [TextArea, ReadOnly]
        public string InfoDiscriptionText;

        [Space, ReadOnly]
        public string NotifiationSeenKey = "";
        public NotificationMainUI m_NotificationMain;

        public void Setup(DataPatience dataPatience)
        {
            m_DataPatience = dataPatience;

            UpgradeButton.onClick.RemoveAllListeners();
            UpgradeButton.onClick.AddListener(UpdatePatience);
        }

        public void SetupNotification(NotificationParentUI notificationParentUI)
        {
            List<NotificationParentUI> notificationParent = new List<NotificationParentUI>();
            notificationParent.Add(notificationParentUI);

            m_NotificationMain.Setup(NotifiationSeenKey, notificationParent);
        }

        public void UpdatePatience()
        {
            OnNotficationSeen();
            if (UpgradeCurrency == CurrencyType.Coin)
            {
                if (DataManager.CoinCurrency >= UpgradeCost)
                {
                    SoundManager.PlaySound(SoundType.Upgrade);
                    UpgradeEffect.transform.parent = null;
                    GlobalFunctions.PlayParticleWithCallback(CoroutineManager.Instance, UpgradeEffect, () => Destroy(UpgradeEffect.gameObject));
                    GlobalEventHolder.OnCoinCurrencySpendOnUpgrade?.Invoke(UpgradeCost);
                    DataManager.CoinCurrency -= UpgradeCost;
                    DataManager.SaveData();
                    UpgradePopupUIManager.UpgradePatience(m_DataPatience, this);
                }
                else
                {
                    GeneralPopupUIManager.OpenNotEnoughCoinShopPopup();
                }
            }
            else if (UpgradeCurrency == CurrencyType.Gem)
            {
                if (DataManager.GemCurrency >= UpgradeCost)
                {
                    SoundManager.PlaySound(SoundType.Upgrade);
                    UpgradeEffect.transform.parent = null;
                    GlobalFunctions.PlayParticleWithCallback(CoroutineManager.Instance, UpgradeEffect, () => Destroy(UpgradeEffect.gameObject));
                    DataManager.GemCurrency -= UpgradeCost;
                    DataManager.SaveData();
                    UpgradePopupUIManager.UpgradePatience(m_DataPatience, this);
                }
                else
                {
                    GeneralPopupUIManager.OpenNotEnoughGemShopPopup();
                }
            }
        }

        public void OnInfoButton()
        {
            OnNotficationSeen();
            UpgradePopupUIManager.OpenInfoPopup(HeaderText.text, InfoDiscriptionText, InfoPreviewSprite);
        }

        public void OnNotficationSeen()
        {
            m_NotificationMain.OnNotficationSeen();
        }
    }
}