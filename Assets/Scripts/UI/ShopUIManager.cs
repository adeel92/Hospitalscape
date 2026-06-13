using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Arc;
using Isometric.Data;
using Isometric.Sound;

namespace Isometric.UI
{
    public class ShopUIManager : UIPopupBase
    {
        [Serializable]
        public class SpecialOfferInfo
        {
            public PurchasingPackageType PackageType;
            public int CoinCount;
            public TextMeshProUGUI CoinText;
            public int GemCount;
            public TextMeshProUGUI GemText;
            public int HeartHourCount;
            public TextMeshProUGUI HeartHourText;
            public int SpeedBoosterCount;
            public TextMeshProUGUI SpeedBoosterText;
            public int FrozenBoosterCount;
            public TextMeshProUGUI FrozenBoosterText;
            public int OrderBoosterCount;
            public TextMeshProUGUI OrderBoosterCountText;
            public Button TargetButton;
            public TextMeshProUGUI TargetButtonText;
        }

        [Serializable]
        public class ShopGemOfferInfo
        {
            public PurchasingPackageType PackageType;
            public int GemPuchased;
            public TextMeshProUGUI GemPuchasedText;
            public Button TargetButton;
            public TextMeshProUGUI TargetButtonText;
        }

        [Serializable]
        public class ShopCoinOfferInfo
        {
            public int CoinPuchased;
            public TextMeshProUGUI CoinPuchasedText;
            public int GemCost;
            public Button TargetButton;
            public TextMeshProUGUI TargetButtonText;
        }

        [Serializable]
        public class ShopHeartOfferInfo
        {
            public int HeartTimeHourPuchased;
            public TextMeshProUGUI HeartTimePuchasedText;
            public int GemCost;
            public Button TargetButton;
            public TextMeshProUGUI TargetButtonText;
        }

        [Serializable]
        public class ShopBoosterOfferInfo
        {
            public BoosterType BoosterType;
            public int GemCost;
            public Button TargetButton;
            public TextMeshProUGUI TargetButtonText;
        }

        [SerializeField] GameObject m_Popup;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;
        [SerializeField] ScrollRect m_ScrollRect;
        [SerializeField] float m_ScrollRectMoveDuration;
        [SerializeField] Ease m_ScrollRectMoveEase;
        [SerializeField] List<SpecialOfferInfo> m_SpecialOffersInfo;
        [SerializeField] List<ShopGemOfferInfo> m_ShopGemOffersInfo;
        [SerializeField] List<ShopCoinOfferInfo> m_ShopCoinOffersInfo;
        [SerializeField] ShopHeartOfferInfo m_ShopHeartOfferInfo;
        [SerializeField] List<ShopBoosterOfferInfo> m_ShopBoosterOfferInfo;

        public override void Setup()
        {
            m_ScrollRect.horizontalNormalizedPosition = 0f;

            foreach (var specialOfferInfo in m_SpecialOffersInfo)
            {
                specialOfferInfo.CoinText.text = specialOfferInfo.CoinCount.ToString();
                specialOfferInfo.GemText.text = specialOfferInfo.GemCount.ToString();
                specialOfferInfo.HeartHourText.text = specialOfferInfo.HeartHourCount + "H";

                specialOfferInfo.SpeedBoosterText.text = "x" + specialOfferInfo.SpeedBoosterCount;
                specialOfferInfo.FrozenBoosterText.text = "x" + specialOfferInfo.FrozenBoosterCount.ToString();
                specialOfferInfo.OrderBoosterCountText.text = "x" + specialOfferInfo.OrderBoosterCount.ToString();

                specialOfferInfo.TargetButton.onClick.RemoveAllListeners();
                specialOfferInfo.TargetButton.onClick.AddListener(() => OnSpecialOfferButton(specialOfferInfo));

                specialOfferInfo.TargetButtonText.text = InAppManager.GetLocalizedPrice(specialOfferInfo.PackageType);

            }

            foreach (var shopGemOfferInfo in m_ShopGemOffersInfo)
            {
                shopGemOfferInfo.GemPuchasedText.text = shopGemOfferInfo.GemPuchased.ToString();
                shopGemOfferInfo.TargetButtonText.text = InAppManager.GetLocalizedPrice(shopGemOfferInfo.PackageType);
                shopGemOfferInfo.TargetButton.onClick.RemoveAllListeners();
                shopGemOfferInfo.TargetButton.onClick.AddListener(() => OnGemOfferButton(shopGemOfferInfo));
            }

            foreach (var shopCoinOfferInfo in m_ShopCoinOffersInfo)
            {
                shopCoinOfferInfo.CoinPuchasedText.text = shopCoinOfferInfo.CoinPuchased.ToString();
                shopCoinOfferInfo.TargetButtonText.text = shopCoinOfferInfo.GemCost.ToString();
                shopCoinOfferInfo.TargetButton.onClick.RemoveAllListeners();
                shopCoinOfferInfo.TargetButton.onClick.AddListener(() => OnCoinOfferButton(shopCoinOfferInfo));
            }

            if (m_ShopHeartOfferInfo != null)
            {
                m_ShopHeartOfferInfo.HeartTimePuchasedText.text = m_ShopHeartOfferInfo.HeartTimeHourPuchased + "H";
                m_ShopHeartOfferInfo.TargetButtonText.text = m_ShopHeartOfferInfo.GemCost.ToString();
                m_ShopHeartOfferInfo.TargetButton.onClick.RemoveAllListeners();
                m_ShopHeartOfferInfo.TargetButton.onClick.AddListener(() => OnHeartLifeOfferButton(m_ShopHeartOfferInfo));
            }

            foreach (var shopBoosterOfferInfo in m_ShopBoosterOfferInfo)
            {
                shopBoosterOfferInfo.TargetButtonText.text = shopBoosterOfferInfo.GemCost.ToString();
                shopBoosterOfferInfo.TargetButton.onClick.RemoveAllListeners();
                shopBoosterOfferInfo.TargetButton.onClick.AddListener(() => OnBoosterOfferButton(shopBoosterOfferInfo));
            }
        }

        public override void OpenPopup(Action onCompete)
        {
            Setup();

            DOVirtual.DelayedCall(0f, () =>
            {
                m_ScrollRect.horizontalNormalizedPosition = 1f;
            });

            m_Popup.SetActive(true);
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_OpeningSequence.PlaySequence(() =>
            {
                int level = DataManager.CurrentMapLevelIndex + 1;
                //GameFirebaseManager.LogEvent("Level_" + level + "_", FirebaseLogType.ShopOpen);

                m_ScrollRect.DOHorizontalNormalizedPos(0f, m_ScrollRectMoveDuration).SetEase(m_ScrollRectMoveEase).SetUpdate(true);

                onCompete?.Invoke();
            });
        }

        public override void ClosePopup(Action onCompete)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_ClosingSequence.PlaySequence(() =>
            {
                onCompete?.Invoke();
                m_Popup.SetActive(false);
            });
        }
        

        private void OnSpecialOfferButton(SpecialOfferInfo specialOfferInfo)
        {

            InAppManager.Purchase(specialOfferInfo.PackageType, () =>
            {
                DataManager.CoinCurrency += specialOfferInfo.CoinCount;
                DataManager.GemCurrency += specialOfferInfo.GemCount;
                DataManager.HeartTimeCurrency += (specialOfferInfo.HeartHourCount * 60 * 60);
                DataManager.TimeFrozeBoosterCount += specialOfferInfo.FrozenBoosterCount;
                DataManager.WaitressSpeedBoosterCount += specialOfferInfo.SpeedBoosterCount;
                DataManager.InstanceOrderFillBoosterCount += specialOfferInfo.OrderBoosterCount;
                DataManager.SaveData();

                int? coin = specialOfferInfo.CoinCount > 0 ? specialOfferInfo.CoinCount : null;
                int? gem = specialOfferInfo.GemCount > 0 ? specialOfferInfo.GemCount : null;
                double? heartTimeCurrency = specialOfferInfo.HeartHourCount > 0 ? (specialOfferInfo.HeartHourCount * 60 * 60) : null;
                int? timeFrozeBoosterCount = specialOfferInfo.FrozenBoosterCount > 0 ? specialOfferInfo.FrozenBoosterCount : null;
                int? instanceOrderFillBoosterCount = specialOfferInfo.OrderBoosterCount > 0 ? specialOfferInfo.OrderBoosterCount : null;
                int? speedBoosterCount = specialOfferInfo.SpeedBoosterCount > 0 ? specialOfferInfo.SpeedBoosterCount : null;

                UIManager.SetRewardCollectionPopupMessage("Tap to Collect!", RewardCollectionTextPoistion.FarDownMiddle);
                UIManager.OpenRewardCollectionPopup(coin, gem, heartTimeCurrency, timeFrozeBoosterCount, instanceOrderFillBoosterCount, speedBoosterCount, null, () => 
                { 
                    SoundManager.PlaySound(SoundType.BigReward);
                }, null);

                GeneralPopupUIManager.OpenPurchaseSuccessPopup();
            }, 
            () =>
            {
                GeneralPopupUIManager.OpenPurchaseFailedPopup();
            });
        }

        private void OnGemOfferButton(ShopGemOfferInfo shopGemOfferInfo)
        {
            InAppManager.Purchase(shopGemOfferInfo.PackageType, () =>
            {
                GeneralPopupUIManager.OpenPurchaseSuccessPopup();

                DataManager.GemCurrency += shopGemOfferInfo.GemPuchased;
                DataManager.SaveData();

                int? gem = shopGemOfferInfo.GemPuchased > 0 ? shopGemOfferInfo.GemPuchased : null;

                UIManager.SetRewardCollectionPopupMessage("Tap to Collect!", RewardCollectionTextPoistion.FarDownMiddle);
                UIManager.OpenRewardCollectionPopup(null, gem, null, null, null, null, null, () => 
                {
                    SoundManager.PlaySound(SoundType.MediumReward);
                }, 
                null);

            },
            () =>
            {
                GeneralPopupUIManager.OpenPurchaseFailedPopup();
            });
        }

        private void OnCoinOfferButton(ShopCoinOfferInfo shopCoinOfferInfo)
        {
            if (shopCoinOfferInfo.GemCost <= DataManager.GemCurrency)
            {
                DataManager.GemCurrency -= shopCoinOfferInfo.GemCost;
                DataManager.CoinCurrency += shopCoinOfferInfo.CoinPuchased;

                DataManager.SaveData();

                int? coin = shopCoinOfferInfo.CoinPuchased > 0 ? shopCoinOfferInfo.CoinPuchased : null;

                UIManager.SetRewardCollectionPopupMessage("Tap to Collect!", RewardCollectionTextPoistion.FarDownMiddle);
                UIManager.OpenRewardCollectionPopup(coin, null, null, null, null, null, null, () => 
                {
                    SoundManager.PlaySound(SoundType.MediumReward);
                }, 
                null);
            }
            else
            {
                GeneralPopupUIManager.OpenNotEnoughGemPopup();
            }
        }

        private void OnHeartLifeOfferButton(ShopHeartOfferInfo shopHeartOfferInfo)
        {
            if (shopHeartOfferInfo.GemCost <= DataManager.GemCurrency)
            {
                DataManager.GemCurrency -= shopHeartOfferInfo.GemCost;
                DataManager.HeartTimeCurrency += (shopHeartOfferInfo.HeartTimeHourPuchased * 60 * 60);

                DataManager.SaveData();

                double? heartTime = shopHeartOfferInfo.HeartTimeHourPuchased > 0 ? (shopHeartOfferInfo.HeartTimeHourPuchased * 60 * 60) : null;

                UIManager.SetRewardCollectionPopupMessage("Tap to Collect!", RewardCollectionTextPoistion.FarDownMiddle);
                UIManager.OpenRewardCollectionPopup(null, null, heartTime, null, null, null, null, () => 
                {
                    SoundManager.PlaySound(SoundType.MediumReward);
                }, 
                null);
            }
            else
            {
                GeneralPopupUIManager.OpenNotEnoughGemPopup();
            }
        }

        private void OnBoosterOfferButton(ShopBoosterOfferInfo shopBoosterOfferInfo)
        {
            if (shopBoosterOfferInfo.GemCost <= DataManager.GemCurrency)
            {
                DataManager.GemCurrency -= shopBoosterOfferInfo.GemCost;

                int? timeFrozeBoosterCount = null;
                int? instanceOrderFillBoosterCount = null;
                int? speedBoosterCount = null;

                if (shopBoosterOfferInfo.BoosterType == BoosterType.TimeFroze)
                {
                    DataManager.TimeFrozeBoosterCount += 1;
                    timeFrozeBoosterCount = 1;
                }
                else if (shopBoosterOfferInfo.BoosterType == BoosterType.InstanceOrderFill)
                {
                    DataManager.InstanceOrderFillBoosterCount += 1;
                    instanceOrderFillBoosterCount = 1;
                }
                else if (shopBoosterOfferInfo.BoosterType == BoosterType.WaitressSpeed)
                {
                    DataManager.WaitressSpeedBoosterCount += 1;
                    speedBoosterCount = 1;
                }

                DataManager.SaveData();

                UIManager.SetRewardCollectionPopupMessage("Tap to Collect!", RewardCollectionTextPoistion.FarDownMiddle);
                UIManager.OpenRewardCollectionPopup(null, null, null, timeFrozeBoosterCount, instanceOrderFillBoosterCount, speedBoosterCount, null, () => 
                {
                    SoundManager.PlaySound(SoundType.MediumReward);
                }, null);
            }
            else
            {
                GeneralPopupUIManager.OpenNotEnoughGemPopup();
            }
        }


        public void OnCloseButton()
        {
            UIManager.ClosePopup<ShopUIManager>();
        }
    }
}
