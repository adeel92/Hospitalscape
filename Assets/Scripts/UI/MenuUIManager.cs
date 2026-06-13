using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Arc;
using Isometric.Data;
using Isometric.Cam;

namespace Isometric.UI
{
    public class MenuUIManager : UIPopupBase
    {
        [Serializable]
        private class PlayPlayDirectInfo
        {
            public List<Info> Infos;

            [Serializable]
            public class Info
            {
                public MapType MapType;
                public int LevelIndex;
            }
        }


        [SerializeField] Canvas m_Canvas;
        [SerializeField] GameObject m_Popup;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;
        [SerializeField] bool m_HideOnSetup;
        [SerializeField] bool m_HasDirectPlay;
        [NaughtyAttributes.ShowIf(nameof(m_HasDirectPlay))]
        [SerializeField] PlayPlayDirectInfo m_PlayDirectInfos;

        private bool m_IsOpen = false;
        

        [Header("---Uppper---")]
        [SerializeField] float m_UpperHideableUIValueY;
        [SerializeField] List<RectTransform> m_UpperHideableUI;

        [Header("---Right---")]
        [SerializeField] float m_RightHideableUIValueX;
        [SerializeField] List<RectTransform> m_RightHideableUI;

        [Header("---Lower---")]
        [SerializeField] float m_LowerHideableUIValueY;
        [SerializeField] List<RectTransform> m_LowerHideableUI;

        [Header("---Play Button---")]
        [SerializeField] TextMeshProUGUI m_PlayButtonText;

        [Header("---Achievement Notification---")]
        [SerializeField] NotificationParentUI m_AchievementNotificationParent;

        [Header("---Vending Machien Button---")]
        [SerializeField] GameObject m_VendingMachineButton;
        [SerializeField] TextMeshProUGUI m_VendingMachineButtonText;

        [Header("---Profile Button---")]
        [SerializeField] Image m_ProfileButtonBackgroundImage;
        [SerializeField] Image m_ProfileButtonAvtarImage;
        [SerializeField] Image m_ProfileButtonFrameImage;

        [Header("---No Ads Button---")]
        [SerializeField] bool m_DisableNoAdsButton;
        [SerializeField] GameObject m_NoAdsPurchaseButton;

        private void OnEnable()
        {
            if (!m_DisableNoAdsButton)
            {
                GlobalEventHolder.OnNoAdsPurchaseSuccessful += OnNoAdsPurchaseSuccessful;
            }
        }

        private void OnDisable()
        {
            if (!m_DisableNoAdsButton)
            {
                GlobalEventHolder.OnNoAdsPurchaseSuccessful -= OnNoAdsPurchaseSuccessful;
            }
        }

        public override void Setup()
        {
            if (m_HideOnSetup)
            {
                foreach (var upperHideableUI in m_UpperHideableUI)
                {
                    Vector2 position = upperHideableUI.anchoredPosition;
                    position.y = m_UpperHideableUIValueY;

                    upperHideableUI.anchoredPosition = position;
                }

                foreach (var rightHideableUI in m_RightHideableUI)
                {
                    Vector2 position = rightHideableUI.anchoredPosition;
                    position.x = m_RightHideableUIValueX;

                    rightHideableUI.anchoredPosition = position;
                }

                foreach (var lowerHideableUI in m_LowerHideableUI)
                {

                    Vector2 position = lowerHideableUI.anchoredPosition;
                    position.y = m_LowerHideableUIValueY;

                    lowerHideableUI.anchoredPosition = position;
                }

                if (DataManager.GetCurrentDataLevel() == null)
                {
                    m_PlayButtonText.text = (DataManager.CurrentMapLevelIndex).ToString();
                }
                else
                {
                    m_PlayButtonText.text = (DataManager.CurrentMapLevelIndex + 1).ToString();
                }

                m_IsOpen = false;
            }
            else
            {
                m_IsOpen = true;
            }

            SetupNoAdsPurchaseButton();
            SetupVendingMachineButton();
        }

        public override void OpenPopup(Action onCompete)
        {
            if (m_IsOpen == false)
            {
                m_IsOpen = true;
                m_Popup.SetActive(true);
                m_OpeningSequence.PlaySequence(onCompete);
            }
            else
            {
                onCompete?.Invoke();
            }
        }

        public override void ClosePopup(Action onComplete)
        {
            if (m_IsOpen == true)
            {
                m_IsOpen = false;
                m_ClosingSequence.PlaySequence(() => {
                    onComplete?.Invoke();
                    m_Popup.SetActive(false);
                });
            }
            else
            {
                onComplete?.Invoke();
            }

        }

        public void OnPlayButton()
        {
            if (m_HasDirectPlay)
            {
                MapType currentMapType = DataManager.CurrentMapType;
                int levelIndex = DataManager.CurrentMapLevelIndex;
                DataLevel currentLevel = DataManager.GetCurrentDataLevel();
                var directPlayInfo = m_PlayDirectInfos.Infos.Find((x) => x.MapType == currentMapType && x.LevelIndex == levelIndex);
                if (directPlayInfo != null && currentLevel != null)
                {
                    if (HeartTimeCurrencyCounter.HasHeartTimeCurrency() || DataManager.HeartCurrency > 0)
                    {

                        HeartCurrencyUIController.SetValueForShouldBeMinusOne(true);

                        UIManager.UIInteractionOff();
                        CameraController.Interactability(false);
                        CameraController.SetupForGameplay(() =>
                        {
                            ClosePopup(() =>
                            {

                                UIManager.UIInteractionOn();
                                UIManager.SetupForGameplay();
                            });
                        });

                        return;
                    }
                    else
                    {
                        GeneralPopupUIManager.OpenNoMoreHeartPopup();
                        return;
                    }
                }
            }

            UIManager.UIInteractionOff();
            ClosePopup(() =>
            {

                UIManager.UIInteractionOn();
                UIManager.OpenUpgradePopup();
            });
        }

        private void SetupNoAdsPurchaseButton()
        {
            if (!m_DisableNoAdsButton)
            {
                if (DataManager.NoAdsPurchase == false)
                {
                    m_NoAdsPurchaseButton.SetActive(true);
                }
                else
                {
                    m_NoAdsPurchaseButton.SetActive(false);
                }
            }
            else
            {
                m_NoAdsPurchaseButton.SetActive(false);
            }
        }

        private void SetupVendingMachineButton()
        {
            VendingMachineUIManager vendingMachine = UIManager.GetPopup<VendingMachineUIManager>();
            if (vendingMachine != null)
            {
                vendingMachine.InsertVendingMachineButton(m_VendingMachineButton, m_VendingMachineButtonText);
            }
            else
            {
                Debug.LogWarning(typeof(VendingMachineUIManager) + " is null");
            }
        }

        public void OnVendingMachineButton()
        {
            UIManager.OpenPopup<VendingMachineUIManager>(true, true);
        }


        private void OnNoAdsPurchaseSuccessful()
        {
            m_NoAdsPurchaseButton.SetActive(false);
        }

        public void OnSettingButton()
        {
            UIManager.OpenPopup<SettingUIManager>(false, true);
        }

        public void OnAchievementButton()
        {
            UIManager.OpenPopup<AchievementUIManager>(false, true);
        }

        public NotificationParentUI GetAchievementButtonNotificationParent()
        {
            return m_AchievementNotificationParent;
        }

        public void OnStarItemButton()
        {
            UIManager.OpenPopup<StarItemUnlockingUIManager>(true, true);
        }

        /// <summary>
        /// Returns BackgroundImage, AvtarImage, FrameImage
        /// </summary>
        public Tuple<Image, Image, Image> GetProfileButtonRelatedImages()
        {
            return new Tuple<Image, Image, Image>(m_ProfileButtonBackgroundImage, m_ProfileButtonAvtarImage, m_ProfileButtonFrameImage);
        }

        public void OnProfileButton()
        {
            UIManager.OpenPopup<ProfileUIManager>(true, true);
        }

        public void OnMapButton()
        {
            UIManager.OpenPopup<MapUIManager>(false, true);
        }
    }
}
