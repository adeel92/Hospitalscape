using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Isometric.Data;

namespace Isometric.UI
{
    public class ProfileFrameHolderUI : MonoBehaviour
    {
        ProfileUIManager m_ProfileUIManager;
        DataProfile m_DataProfile;
        ProfileFrameInfo m_ProfileFrameInfo;

        [SerializeField] RectTransform m_RectTransform;
        [SerializeField] Color m_UnlockColor;
        public Image PreviewImage;
        public GameObject Selected;
        public GameObject Tick;

        public void Setup(ProfileUIManager profileUIManager, DataProfile dataProfile, ProfileFrameInfo profileFrameInfo)
        {
            m_ProfileUIManager = profileUIManager;
            m_DataProfile = dataProfile;
            m_ProfileFrameInfo = profileFrameInfo;
        }

        public void SelectProfileFrame()
        {
            Tuple<Image, Image, Image> profileButton = m_ProfileUIManager.GetProfileButtonRelatedImages();

            m_ProfileUIManager.ProfileFrameImage.sprite = m_ProfileFrameInfo.PreviewSprite;
            m_ProfileUIManager.ProfileFrameImage.rectTransform.anchoredPosition = m_ProfileFrameInfo.SelectedPreviewImageAnchorPosition;
            m_ProfileUIManager.ProfileFrameImage.rectTransform.sizeDelta = m_ProfileFrameInfo.SelectedPreviewImageDeltaSize;

            profileButton.Item3.sprite = m_ProfileFrameInfo.PreviewSprite;
            profileButton.Item3.rectTransform.anchoredPosition = m_ProfileFrameInfo.SelectedPreviewImageAnchorPosition;
            profileButton.Item3.rectTransform.sizeDelta = m_ProfileFrameInfo.SelectedPreviewImageDeltaSize;
        }

        public void OnClick()
        {
            if (m_ProfileFrameInfo.IsUnlocked)
            {
                m_ProfileUIManager.ProfileFrameImage.sprite = m_ProfileFrameInfo.PreviewSprite;
                m_ProfileUIManager.ProfileFrameImage.rectTransform.anchoredPosition = m_ProfileFrameInfo.SelectedPreviewImageAnchorPosition;
                m_ProfileUIManager.ProfileFrameImage.rectTransform.sizeDelta = m_ProfileFrameInfo.SelectedPreviewImageDeltaSize;

                m_ProfileUIManager.SelectProfileFrame(this);
            }
            else
            {
                int cost = 0;
                if (m_ProfileFrameInfo.UpdateType == CurrencyType.Coin)
                {
                    cost = m_ProfileFrameInfo.RequiredCoins;

                }
                else if (m_ProfileFrameInfo.UpdateType == CurrencyType.Gem)
                {
                    cost = m_ProfileFrameInfo.RequiredGems;
                }

                m_ProfileUIManager.OpenPurchasePopup(m_RectTransform, cost, m_ProfileFrameInfo.UpdateType,
                () =>
                {
                    if (m_ProfileFrameInfo.UpdateType == CurrencyType.Coin)
                    {
                        if (cost <= DataManager.CoinCurrency)
                        {
                            DataManager.CoinCurrency -= cost;
                            m_ProfileFrameInfo.IsUnlocked = true;

                            PreviewImage.color = m_UnlockColor;

                            m_DataProfile.Save();
                            DataManager.SaveData();
                        }
                        else
                        {
                            GeneralPopupUIManager.OpenNotEnoughCoinShopPopup();
                        }

                    }
                    else if (m_ProfileFrameInfo.UpdateType == CurrencyType.Gem)
                    {
                        if (cost <= DataManager.GemCurrency)
                        {
                            DataManager.GemCurrency -= cost;
                            m_ProfileFrameInfo.IsUnlocked = true;

                            PreviewImage.color = m_UnlockColor;

                            m_DataProfile.Save();
                            DataManager.SaveData();
                        }
                        else
                        {
                            GeneralPopupUIManager.OpenNotEnoughGemShopPopup();
                        }
                    }
                });
            }
        }
    }
}
