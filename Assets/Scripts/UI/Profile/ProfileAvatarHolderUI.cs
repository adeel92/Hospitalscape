using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Isometric.Data;

namespace Isometric.UI
{
    public class ProfileAvatarHolderUI : MonoBehaviour
    {
        ProfileUIManager m_ProfileUIManager;
        DataProfile m_DataProfile;
        ProfileAvatarInfo m_ProfileAvatarInfo;

        [SerializeField] RectTransform m_RectTransform;
        [SerializeField] Color m_UnlockColor;
        public Image BackgroundImage;
        public Image PreviewImage;
        public GameObject Selected;
        public GameObject Tick;

        public void Setup(ProfileUIManager profileUIManager, DataProfile dataProfile, ProfileAvatarInfo profileAvatarInfo)
        {
            m_ProfileUIManager = profileUIManager;
            m_DataProfile = dataProfile;
            m_ProfileAvatarInfo = profileAvatarInfo;
        }

        public void SelectProfileAvtar()
        {
            Tuple<Image, Image, Image> profileButton = m_ProfileUIManager.GetProfileButtonRelatedImages();

            m_ProfileUIManager.ProfileBackgroundImage.sprite = m_ProfileAvatarInfo.BackgroundSprite;
            profileButton.Item1.sprite = m_ProfileAvatarInfo.BackgroundSprite;

            m_ProfileUIManager.ProfileAvtarImage.sprite = m_ProfileAvatarInfo.PreviewSprite;
            m_ProfileUIManager.ProfileAvtarImage.rectTransform.anchoredPosition = m_ProfileAvatarInfo.SelectedPreviewImageAnchorPosition;
            m_ProfileUIManager.ProfileAvtarImage.rectTransform.sizeDelta = m_ProfileAvatarInfo.SelectedPreviewImageDeltaSize;

            profileButton.Item2.sprite = m_ProfileAvatarInfo.PreviewSprite;
            profileButton.Item2.rectTransform.anchoredPosition = m_ProfileAvatarInfo.SelectedPreviewImageAnchorPosition;
            profileButton.Item2.rectTransform.sizeDelta = m_ProfileAvatarInfo.SelectedPreviewImageDeltaSize;
        }

        public void OnClick()
        {
            if (m_ProfileAvatarInfo.IsUnlocked)
            {
                m_ProfileUIManager.ProfileBackgroundImage.sprite = m_ProfileAvatarInfo.BackgroundSprite;

                m_ProfileUIManager.ProfileAvtarImage.sprite = m_ProfileAvatarInfo.PreviewSprite;
                m_ProfileUIManager.ProfileAvtarImage.rectTransform.anchoredPosition = m_ProfileAvatarInfo.SelectedPreviewImageAnchorPosition;
                m_ProfileUIManager.ProfileAvtarImage.rectTransform.sizeDelta = m_ProfileAvatarInfo.SelectedPreviewImageDeltaSize;

                m_ProfileUIManager.SelectProfileAvtar(this);
            }
            else
            {
                int cost = 0;
                if (m_ProfileAvatarInfo.UpdateType == CurrencyType.Coin)
                {
                    cost = m_ProfileAvatarInfo.RequiredCoins;

                }
                else if (m_ProfileAvatarInfo.UpdateType == CurrencyType.Gem)
                {
                    cost = m_ProfileAvatarInfo.RequiredGems;
                }

                m_ProfileUIManager.OpenPurchasePopup(m_RectTransform, cost, m_ProfileAvatarInfo.UpdateType,
                () => 
                {
                    if (m_ProfileAvatarInfo.UpdateType == CurrencyType.Coin)
                    {
                        if (cost <= DataManager.CoinCurrency)
                        {
                            DataManager.CoinCurrency -= cost;
                            m_ProfileAvatarInfo.IsUnlocked = true;

                            BackgroundImage.color = m_UnlockColor;
                            PreviewImage.color = m_UnlockColor;

                            m_DataProfile.Save();
                            DataManager.SaveData();
                        }
                        else
                        {
                            GeneralPopupUIManager.OpenNotEnoughCoinShopPopup();
                        }

                    }
                    else if (m_ProfileAvatarInfo.UpdateType == CurrencyType.Gem)
                    {
                        if (cost <= DataManager.GemCurrency)
                        {
                            DataManager.GemCurrency -= cost;
                            m_ProfileAvatarInfo.IsUnlocked = true;

                            BackgroundImage.color = m_UnlockColor;
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
