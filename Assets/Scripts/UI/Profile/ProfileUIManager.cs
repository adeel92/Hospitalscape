using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Arc;
using Isometric.Data;
using Isometric.Sound;

namespace Isometric.UI
{
    public class ProfileUIManager : UIPopupBase
    {
        [Header("---Setup---")]
        [SerializeField] GameObject m_Popup;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;
        [SerializeField] DataMapUpdate m_DataMapUpdate;
        [SerializeField] Transform m_PrfileAvtarHolder;
        [SerializeField] Transform m_PrfileFrameHolder;
        [SerializeField] GameObject m_OkButton;
        [SerializeField] GameObject m_OkOffButton;

        [Header("---Player Name---")]
        [SerializeField] TMP_InputField m_NameInput;

        [Header("---Tabs---")]
        [SerializeField] GameObject m_AvtarTab;
        [SerializeField] Button m_AvtarButton;
        [SerializeField] GameObject m_AvtarSelectedButton;
        [SerializeField] GameObject m_FrameTab;
        [SerializeField] Button m_FrameButton;
        [SerializeField] GameObject m_FrameSelectedButton;

        [Header("---Profile---")]
        public Image ProfileBackgroundImage;
        public Image ProfileAvtarImage;
        public Image ProfileFrameImage;

        [Header("---Purchase Popup---")]
        [SerializeField] GameObject m_PurchasePopup;
        [SerializeField] RectTransform m_PurchaseSmallPopup;
        [SerializeField] Vector2 m_PurchaseSmallPopupOffset;
        [SerializeField] TextMeshProUGUI m_PriceButtonText;
        [SerializeField] GameObject m_PriceButtonCoin;
        [SerializeField] GameObject m_PriceButtonGem;

        private Action OnPurchase;


        DataProfile m_DataProfile;

        private List<ProfileAvatarHolderUI> m_ProfileAvatars;
        private List<ProfileFrameHolderUI> m_ProfileFrames;

        [HideInInspector]
        public ProfileAvatarHolderUI SelectedProfilAvtarHolder = null;
        ProfileAvatarHolderUI m_SelectedToBeProfilAvtarHolder = null;

        [HideInInspector]
        public ProfileFrameHolderUI SelectedProfilFrameHolder = null;
        ProfileFrameHolderUI m_SelectedToBeProfilFrameHolder = null;

        public override void Setup()
        {
            m_SelectedToBeProfilAvtarHolder = null;
            m_SelectedToBeProfilFrameHolder = null;

            m_OkButton.SetActive(false);
            m_OkOffButton.SetActive(true);

            m_DataProfile = m_DataMapUpdate.GetDataProfile();

            m_NameInput.text = m_DataProfile.ProfileData.PlayerName;

            m_PrfileAvtarHolder.DeleteAllChildren();
            m_PrfileFrameHolder.DeleteAllChildren();

            m_ProfileAvatars = m_DataProfile.GetPrfileAvatars(m_PrfileAvtarHolder, this);
            m_ProfileFrames = m_DataProfile.GetPrfileFrames(m_PrfileFrameHolder, this);

            OpenProfileAvtarTab();
        }

        public override void OpenPopup(Action onComplete)
        {
            Setup();

            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_Popup.SetActive(true);
            m_OpeningSequence.PlaySequence(() =>
            {
                onComplete?.Invoke();
            });
        }

        public override void ClosePopup(Action onComplete)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);

            m_ClosingSequence.PlaySequence(() =>
            {
                m_Popup.SetActive(false);
                onComplete?.Invoke();
            });
        }

        public void OpenProfileAvtarTab()
        {
            m_FrameButton.gameObject.SetActive(true);
            m_FrameSelectedButton.SetActive(false);
            m_FrameTab.SetActive(false);

            m_AvtarButton.gameObject.SetActive(false);
            m_AvtarSelectedButton.SetActive(true);
            m_AvtarTab.SetActive(true);
        }

        public void OpenProfileFrameTab()
        {
            m_AvtarButton.gameObject.SetActive(true);
            m_AvtarSelectedButton.SetActive(false);
            m_AvtarTab.SetActive(false);

            m_FrameButton.gameObject.SetActive(false);
            m_FrameSelectedButton.SetActive(true);
            m_FrameTab.SetActive(true);
        }

        public void OnCloseButton()
        {
            UIManager.ClosePopup<ProfileUIManager>();
        }


        #region Profile Related
        /// <summary>
        /// Returns BackgroundImage, AvtarImage, FrameImage
        /// </summary>
        public Tuple<Image, Image, Image> GetProfileButtonRelatedImages()
        {
            MenuUIManager menuUIManager = UIManager.GetPopup<MenuUIManager>();
            if (menuUIManager != null)
            {
                return menuUIManager.GetProfileButtonRelatedImages();
            }
            else
            {
                Debug.LogWarning(nameof(MenuUIManager) + " is null");
                return null;
            }
        }

        

        public void SelectProfileAvtar(ProfileAvatarHolderUI profilAvtarHolder)
        {
            if (m_SelectedToBeProfilAvtarHolder != null)
            {
                m_SelectedToBeProfilAvtarHolder.Selected.SetActive(false);
            }

            m_SelectedToBeProfilAvtarHolder = profilAvtarHolder;
            m_SelectedToBeProfilAvtarHolder.Selected.SetActive(true);

            if (SelectedProfilAvtarHolder != m_SelectedToBeProfilAvtarHolder)
            {
                m_OkButton.SetActive(true);
                m_OkOffButton.SetActive(false);
            }
            else
            {
                m_OkButton.SetActive(false);
                m_OkOffButton.SetActive(true);
            }
        }

        public void SelectProfileFrame(ProfileFrameHolderUI profilFrameHolder)
        {
            if (m_SelectedToBeProfilFrameHolder != null)
            {
                m_SelectedToBeProfilFrameHolder.Selected.SetActive(false);
            }

            m_SelectedToBeProfilFrameHolder = profilFrameHolder;
            m_SelectedToBeProfilFrameHolder.Selected.SetActive(true);

            if (SelectedProfilFrameHolder != m_SelectedToBeProfilFrameHolder)
            {
                m_OkButton.SetActive(true);
                m_OkOffButton.SetActive(false);
            }
            else
            {
                m_OkButton.SetActive(false);
                m_OkOffButton.SetActive(true);
            }
        }

        public void OnOkButton()
        {
            if (m_SelectedToBeProfilAvtarHolder != null)
            {
                SelectedProfilAvtarHolder.Tick.SetActive(false);
                SelectedProfilAvtarHolder = m_SelectedToBeProfilAvtarHolder;
                SelectedProfilAvtarHolder.SelectProfileAvtar();
                SelectedProfilAvtarHolder.Tick.SetActive(true);
                for (int i = 0; i < m_ProfileAvatars.Count; i++)
                {
                    if (m_ProfileAvatars[i] == SelectedProfilAvtarHolder)
                    {
                        m_DataProfile.ProfileData.ProfileAvatarSelectedIndex = i;
                        m_DataProfile.Save();
                        break;
                    }
                }

                m_OkButton.SetActive(false);
                m_OkOffButton.SetActive(true);
            }

            if (m_SelectedToBeProfilFrameHolder != null)
            {
                SelectedProfilFrameHolder.Tick.SetActive(false);
                SelectedProfilFrameHolder = m_SelectedToBeProfilFrameHolder;
                SelectedProfilFrameHolder.SelectProfileFrame();
                SelectedProfilFrameHolder.Tick.SetActive(true);
                for (int i = 0; i < m_ProfileFrames.Count; i++)
                {
                    if (m_ProfileFrames[i] == SelectedProfilFrameHolder)
                    {
                        m_DataProfile.ProfileData.ProfileFramesSelectedIndex = i;
                        m_DataProfile.Save();
                        break;
                    }
                }

                m_OkButton.SetActive(false);
                m_OkOffButton.SetActive(true);
            }
        }
        #endregion

        #region PlayerName
        public void OnSavePlayerName()
        {
            m_DataProfile.ProfileData.PlayerName = m_NameInput.text;
            m_DataProfile.Save();
        }
        #endregion

        #region Purchase Popup

        public void OpenPurchasePopup(RectTransform target, int cost, CurrencyType currencyType, Action onPurchase)
        {
            Vector3 worldPos = target.position;

            // Convert world position to local position relative to popup's parent
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                m_PurchaseSmallPopup.parent as RectTransform,
                RectTransformUtility.WorldToScreenPoint(null, worldPos),
                null,
                out localPoint
            );

            m_PurchaseSmallPopup.anchoredPosition = localPoint + m_PurchaseSmallPopupOffset;

            OnPurchase = onPurchase;
            m_PriceButtonText.text = cost.ToString();

            m_PriceButtonCoin.SetActive(false);
            m_PriceButtonGem.SetActive(false);

            if (currencyType == CurrencyType.Coin)
            {
                m_PriceButtonCoin.SetActive(true);
            }
            else if (currencyType == CurrencyType.Gem)
            {
                m_PriceButtonGem.SetActive(true);
            }

            m_PurchasePopup.SetActive(true);
        }

        public void OnPurchaseButton()
        {
            OnPurchase?.Invoke();
            ClosePurchasePopup();
        }

        public void ClosePurchasePopup()
        {
            OnPurchase = null;
            m_PurchasePopup.SetActive(false);
        }
        #endregion
    }
}
