using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Arc;
using Isometric.Data;

namespace Isometric.UI
{
    public class InAppPurchaser : MonoBehaviour
    {

        [SerializeField] bool m_SetupOnStart;

        [Header("---NoAds Purchase---")]
        [SerializeField] PurchasingPackageType AdsPackage;
        [SerializeField] TextMeshProUGUI m_PriceText;
        public UnityEvent OnPurchaseSuccesful;

        private void Start()
        {
            if (m_SetupOnStart)
            {
                Setup();
            }
        }

        public void Setup()
        {
            m_PriceText.text = InAppManager.GetLocalizedPrice(AdsPackage);
        }

        public void OnPurchaseButton()
        {
            InAppManager.Purchase(AdsPackage, 
            () => 
            {
                DataManager.NoAdsPurchase = true;
                DataManager.SaveData();
                PlayerPrefs.SetInt("RemoveAds", 1);
                GlobalEventHolder.OnNoAdsPurchaseSuccessful?.Invoke();
                OnPurchaseSuccesful?.Invoke();
                GeneralPopupUIManager.OpenPurchaseSuccessPopup();
            },
            () => 
            {
                GeneralPopupUIManager.OpenPurchaseFailedPopup();
            });
        }
    }
}
