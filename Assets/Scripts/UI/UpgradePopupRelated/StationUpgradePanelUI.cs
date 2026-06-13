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
    public class StationUpgradePanelUI : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        public UpgradePopupUIManager UpgradePopupUIManager;

        [SerializeField, ReadOnly]
        private DataStation m_DataStation;
        [SerializeField, ReadOnly]
        private StationUpgradeType m_StationUpgradeType;

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

        public void Setup(DataStation dataStation, StationUpgradeType stationUpgradeType)
        {
            m_DataStation = dataStation;
            m_StationUpgradeType = stationUpgradeType;

            UpgradeButton.onClick.RemoveAllListeners();
            UpgradeButton.onClick.AddListener(UpgradeStation);
        }

        public void SetupNotification(NotificationParentUI notificationParentUI)
        {
            List<NotificationParentUI> notificationParent = new List<NotificationParentUI>();
            notificationParent.Add(notificationParentUI);

            m_NotificationMain.Setup(NotifiationSeenKey, notificationParent);
        }

        public void UpgradeStation()
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
                    UpgradePopupUIManager.UpgradeStation(m_DataStation, m_StationUpgradeType, this);
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
                    UpgradePopupUIManager.UpgradeStation(m_DataStation, m_StationUpgradeType, this);
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
