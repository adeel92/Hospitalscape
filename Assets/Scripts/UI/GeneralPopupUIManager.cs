using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Arc;
using Isometric.Sound;
using Isometric.Data;

namespace Isometric.UI
{
    public class GeneralPopupUIManager : MonoBehaviour
    {
        private static GeneralPopupUIManager s_Instance;

        [Header("---Setup---")]
        [SerializeField] GameObject m_Popup;
        [SerializeField] GraphicRaycaster m_GraphicRaycaster;

        [Header("---Purchase Popup---")]
        [SerializeField] PlayDoTweenSequence m_PurchaseSuccessPopupOpeningSequence;
        [SerializeField] PlayDoTweenSequence m_PurchaseSuccessPopupClosingSequence;
        [SerializeField] GameObject m_PurchaseSuccessPopup;
        private Action onPurchaseSuccessPopupClose;

        [SerializeField] PlayDoTweenSequence m_PurchaseFailedPopupOpeningSequence;
        [SerializeField] PlayDoTweenSequence m_PurchaseFailedPopupClosingSequence;
        [SerializeField] GameObject m_PurchaseFailedPopup;
        private Action onPurchaseFailedPopupClose;

        [Header("---Not Enough Gem Popup---")]
        [SerializeField] PlayDoTweenSequence m_NotEnoughGemPopupOpeningSequence;
        [SerializeField] PlayDoTweenSequence m_NotEnoughGemPopupClosingSequence;
        [SerializeField] GameObject m_NotEnoughGemPopup;
        private Action onNotEnoughGemPopupClose;

        [Header("---Not Enough Gem Shop Popup---")]
        [SerializeField] PlayDoTweenSequence m_NotEnoughGemShopPopupOpeningSequence;
        [SerializeField] PlayDoTweenSequence m_NotEnoughGemShopPopupClosingSequence;
        [SerializeField] GameObject m_NotEnoughGemShopPopup;
        [SerializeField] GraphicRaycaster m_NotEnoughShopGemGraphicRaycaster;
        private Action onNotEnoughGemShopPopupClose;

        [Header("---Not Enough Coin Shop Popup---")]
        [SerializeField] PlayDoTweenSequence m_NotEnoughCoinShopPopupOpeningSequence;
        [SerializeField] PlayDoTweenSequence m_NotEnoughCoinShopPopupClosingSequence;
        [SerializeField] GameObject m_NotEnoughCoinShopPopup;
        [SerializeField] GraphicRaycaster m_NotEnoughShopCoinGraphicRaycaster;
        private Action onNotEnoughCoinShopPopupClose;

        [Header("---Heart Popup---")]
        [SerializeField] PlayDoTweenSequence m_NoMoreHeartsPopupOpeningSequence;
        [SerializeField] PlayDoTweenSequence m_NoMoreHeartsPopupClosingSequence;
        [SerializeField] GameObject m_NoMoreHeartsPopup;
        [SerializeField] int m_HeartPopupGemRefillCost;
        [SerializeField] TextMeshProUGUI m_HeartPopupGemCostText;
        private Action onNoMoreHeartsPopupClose;

        private List<bool> m_IsPopupOpened = new List<bool>();

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }

        public static void OpenPurchaseSuccessPopup(Action onClose = null)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.onPurchaseSuccessPopupClose = onClose;

            SoundManager.PlaySound(SoundType.PopupWhoosh);

            UIManager.UIInteractionOff();
            s_Instance.m_GraphicRaycaster.enabled = false;
            s_Instance.m_IsPopupOpened.Add(true);
            s_Instance.m_Popup.SetActive(true);
            s_Instance.m_PurchaseSuccessPopup.SetActive(true);
            s_Instance.m_PurchaseSuccessPopupOpeningSequence.PlaySequence(() =>
            {
                s_Instance.m_GraphicRaycaster.enabled = true;
                UIManager.UIInteractionOn();
            });
        }

        public static void ClosePurchaseSuccessPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            SoundManager.PlaySound(SoundType.PopupWhoosh);

            UIManager.UIInteractionOff();
            s_Instance.m_GraphicRaycaster.enabled = false;
            s_Instance.m_PurchaseSuccessPopupClosingSequence.PlaySequence(() =>
            {
                s_Instance.m_GraphicRaycaster.enabled = true;
                s_Instance.m_IsPopupOpened.Remove(true);
                if (s_Instance.m_IsPopupOpened.Count <= 0)
                {
                    s_Instance.m_Popup.SetActive(false);
                }
                s_Instance.m_PurchaseSuccessPopup.SetActive(false);
                UIManager.UIInteractionOn();

                s_Instance.onPurchaseSuccessPopupClose?.Invoke();
                s_Instance.onPurchaseSuccessPopupClose = null;
            });
        }

        public static void OpenPurchaseFailedPopup(Action onClose = null)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.onPurchaseFailedPopupClose = onClose;

            SoundManager.PlaySound(SoundType.PopupWhoosh);

            UIManager.UIInteractionOff();
            s_Instance.m_GraphicRaycaster.enabled = false;
            s_Instance.m_IsPopupOpened.Add(true);
            s_Instance.m_Popup.SetActive(true);
            s_Instance.m_PurchaseFailedPopup.SetActive(true);
            s_Instance.m_PurchaseFailedPopupOpeningSequence.PlaySequence(() =>
            {
                s_Instance.m_GraphicRaycaster.enabled = true;
                UIManager.UIInteractionOn();

            });
        }

        public static void ClosePurchaseFailedPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UIManager.UIInteractionOff();

            SoundManager.PlaySound(SoundType.PopupWhoosh);

            s_Instance.m_GraphicRaycaster.enabled = false;
            s_Instance.m_PurchaseFailedPopupClosingSequence.PlaySequence(() =>
            {
                s_Instance.m_GraphicRaycaster.enabled = true;
                s_Instance.m_IsPopupOpened.Remove(true);
                if (s_Instance.m_IsPopupOpened.Count <= 0)
                {
                    s_Instance.m_Popup.SetActive(false);
                }
                s_Instance.m_PurchaseFailedPopup.SetActive(false);
                UIManager.UIInteractionOn();

                s_Instance.onPurchaseFailedPopupClose?.Invoke();
                s_Instance.onPurchaseFailedPopupClose = null;
            });
        }

        public static void OpenNotEnoughGemPopup(Action onClose = null)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.onNotEnoughGemPopupClose = onClose;

            SoundManager.PlaySound(SoundType.PopupWhoosh);

            UIManager.UIInteractionOff();
            s_Instance.m_GraphicRaycaster.enabled = false;
            s_Instance.m_IsPopupOpened.Add(true);
            s_Instance.m_Popup.SetActive(true);
            s_Instance.m_NotEnoughGemPopup.SetActive(true);
            s_Instance.m_NotEnoughGemPopupOpeningSequence.PlaySequence(() =>
            {
                s_Instance.m_GraphicRaycaster.enabled = true;
                UIManager.UIInteractionOn();
            });
        }

        public static void CloseNotEnoughGemPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            SoundManager.PlaySound(SoundType.PopupWhoosh);

            UIManager.UIInteractionOff();
            s_Instance.m_GraphicRaycaster.enabled = false;
            s_Instance.m_NotEnoughGemPopupClosingSequence.PlaySequence(() =>
            {
                s_Instance.m_GraphicRaycaster.enabled = true;
                s_Instance.m_IsPopupOpened.Remove(true);
                if (s_Instance.m_IsPopupOpened.Count <= 0)
                {
                    s_Instance.m_Popup.SetActive(false);
                }
                s_Instance.m_NotEnoughGemPopup.SetActive(false);
                UIManager.UIInteractionOn();

                s_Instance.onNotEnoughGemPopupClose?.Invoke();
                s_Instance.onNotEnoughGemPopupClose = null;
            });
        }


        #region Heart
        public static void OpenNoMoreHeartPopup(Action onClose = null)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.onNoMoreHeartsPopupClose = onClose;

            SoundManager.PlaySound(SoundType.PopupWhoosh);

            UIManager.UIInteractionOff();
            s_Instance.m_GraphicRaycaster.enabled = false;
            s_Instance.m_HeartPopupGemCostText.text = s_Instance.m_HeartPopupGemRefillCost.ToString();
            s_Instance.m_IsPopupOpened.Add(true);
            s_Instance.m_Popup.SetActive(true);
            s_Instance.m_NoMoreHeartsPopup.SetActive(true);
            s_Instance.m_NoMoreHeartsPopupOpeningSequence.PlaySequence(() =>
            {
                s_Instance.m_GraphicRaycaster.enabled = true;
                UIManager.UIInteractionOn();
            });
        }

        public static void CloseNoMoreHeartPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            SoundManager.PlaySound(SoundType.PopupWhoosh);

            UIManager.UIInteractionOff();
            s_Instance.m_GraphicRaycaster.enabled = false;
            s_Instance.m_NoMoreHeartsPopupClosingSequence.PlaySequence(() =>
            {
                s_Instance.m_GraphicRaycaster.enabled = true;
                s_Instance.m_IsPopupOpened.Remove(true);
                if (s_Instance.m_IsPopupOpened.Count <= 0)
                {
                    s_Instance.m_Popup.SetActive(false);
                }
                s_Instance.m_NoMoreHeartsPopup.SetActive(false);
                UIManager.UIInteractionOn();


                s_Instance.onNoMoreHeartsPopupClose?.Invoke();
                s_Instance.onNoMoreHeartsPopupClose = null;
            });
        }

        public void OnGemHeartRefill()
        {
            if (DataManager.GemCurrency >= m_HeartPopupGemRefillCost)
            {
                DataManager.GemCurrency -= m_HeartPopupGemRefillCost;
                DataManager.HeartCurrency = Mathf.Min(DataManager.HeartCurrency + 1, DataManager.HeartCurrencyMaxValue);
                DataManager.SaveData();

                SoundManager.PlaySound(SoundType.Reward);

                CloseNoMoreHeartPopup();
            }
            else
            {
                OpenNotEnoughGemPopup();
            }
        }

        public void OnVideoHeart()
        {
            DataManager.HeartCurrency = Mathf.Min(DataManager.HeartCurrency + 1, DataManager.HeartCurrencyMaxValue);
            DataManager.SaveData();

            SoundManager.PlaySound(SoundType.Reward);

            CloseNoMoreHeartPopup();
        }
        #endregion


        public static void OpenNotEnoughGemShopPopup(Action onClose = null)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.onNotEnoughGemShopPopupClose = onClose;

            SoundManager.PlaySound(SoundType.PopupWhoosh);

            UIManager.UIInteractionOff();
            s_Instance.m_GraphicRaycaster.enabled = false;
            s_Instance.m_NotEnoughShopGemGraphicRaycaster.enabled = false;
            s_Instance.m_IsPopupOpened.Add(true);
            s_Instance.m_Popup.SetActive(true);
            s_Instance.m_NotEnoughGemShopPopup.SetActive(true);
            s_Instance.m_NotEnoughGemShopPopupOpeningSequence.PlaySequence(() =>
            {
                s_Instance.m_GraphicRaycaster.enabled = true;
                s_Instance.m_NotEnoughShopGemGraphicRaycaster.enabled = true;
                UIManager.UIInteractionOn();
            });
        }

        public static void CloseNotEnoughGemShopPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            SoundManager.PlaySound(SoundType.PopupWhoosh);

            UIManager.UIInteractionOff();
            s_Instance.m_GraphicRaycaster.enabled = false;
            s_Instance.m_NotEnoughShopGemGraphicRaycaster.enabled = false;
            s_Instance.m_NotEnoughGemShopPopupClosingSequence.PlaySequence(() =>
            {
                s_Instance.m_GraphicRaycaster.enabled = true;
                s_Instance.m_NotEnoughShopGemGraphicRaycaster.enabled = true;
                s_Instance.m_IsPopupOpened.Remove(true);
                if (s_Instance.m_IsPopupOpened.Count <= 0)
                {
                    s_Instance.m_Popup.SetActive(false);
                }
                s_Instance.m_NotEnoughGemShopPopup.SetActive(false);
                UIManager.UIInteractionOn();

                s_Instance.onNotEnoughGemShopPopupClose?.Invoke();
                s_Instance.onNotEnoughGemShopPopupClose = null;
            });
        }

        public static void OpenNotEnoughCoinShopPopup(Action onClose = null)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.onNotEnoughCoinShopPopupClose = onClose;

            SoundManager.PlaySound(SoundType.PopupWhoosh);

            UIManager.UIInteractionOff();
            s_Instance.m_GraphicRaycaster.enabled = false;
            s_Instance.m_NotEnoughShopCoinGraphicRaycaster.enabled = false;
            s_Instance.m_IsPopupOpened.Add(true);
            s_Instance.m_Popup.SetActive(true);
            s_Instance.m_NotEnoughCoinShopPopup.SetActive(true);
            s_Instance.m_NotEnoughCoinShopPopupOpeningSequence.PlaySequence(() =>
            {
                s_Instance.m_GraphicRaycaster.enabled = true;
                s_Instance.m_NotEnoughShopCoinGraphicRaycaster.enabled = true;
                UIManager.UIInteractionOn();
            });
        }

        public static void CloseNotEnoughCoinShopPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            SoundManager.PlaySound(SoundType.PopupWhoosh);

            UIManager.UIInteractionOff();
            s_Instance.m_GraphicRaycaster.enabled = false;
            s_Instance.m_NotEnoughShopCoinGraphicRaycaster.enabled = false;
            s_Instance.m_NotEnoughCoinShopPopupClosingSequence.PlaySequence(() =>
            {
                s_Instance.m_GraphicRaycaster.enabled = true;
                s_Instance.m_NotEnoughShopCoinGraphicRaycaster.enabled = true;
                s_Instance.m_IsPopupOpened.Remove(true);
                if (s_Instance.m_IsPopupOpened.Count <= 0)
                {
                    s_Instance.m_Popup.SetActive(false);
                }
                s_Instance.m_NotEnoughCoinShopPopup.SetActive(false);
                UIManager.UIInteractionOn();

                s_Instance.onNotEnoughCoinShopPopupClose?.Invoke();
                s_Instance.onNotEnoughCoinShopPopupClose = null;
            });
        }


        public void OnShopButton()
        {
            GlobalEventHolder.OnOpenShop?.Invoke(false);
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(GeneralPopupUIManager) + " is null");
        }
    }
}
