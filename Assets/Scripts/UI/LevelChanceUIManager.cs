using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Arc;
using Isometric.Customer;
using Isometric.Data;
using Isometric.Sound;

namespace Isometric.UI
{
    public class LevelChanceUIManager : UIPopupBase
    {

        [Serializable]
        private class TimeChancePopupInfo
        {
            public GameObject Popup;
            public PlayDoTweenSequence OpeningSequence;
            public PlayDoTweenSequence ClosingSequence;
            public GameObject CoinTargetHolder;
            public TextMeshProUGUI CoinRemaingTarget;
            public GameObject CustomerTargetHolder;
            public TextMeshProUGUI CustomerRemaingTarget;
            [Header("---Video---")]
            public int VideoAdditionalSeconds;
            public TextMeshProUGUI VideoTimerText;
            public TextMeshProUGUI VideoChanceDetailText;
            public TextMeshProUGUI VideoButtonText;
            [Header("---Currency---")]
            public int AdditionalSeconds;
            public TextMeshProUGUI TimerText;
            public TextMeshProUGUI ChanceDetailText;
            public int GemCost;
            public TextMeshProUGUI GemCostText;
        }

        [Serializable]
        private class CustomerPatienceChancePopupInfo
        {
            public GameObject Popup;
            public PlayDoTweenSequence OpeningSequence;
            public PlayDoTweenSequence ClosingSequence;
            public int GemCost;
            public TextMeshProUGUI GemCostText;
        }

        [Serializable]
        private class MoreCustomerChancePopupInfo
        {
            public GameObject Popup;
            public PlayDoTweenSequence OpeningSequence;
            public PlayDoTweenSequence ClosingSequence;
            public GameObject CoinTargetHolder;
            public TextMeshProUGUI CoinRemaingTarget;
            public GameObject CustomerTargetHolder;
            public TextMeshProUGUI CustomerRemaingTarget;
            [Header("---Video---")]
            public int VideoAdditionalCustomers;
            public TextMeshProUGUI VideoChanceDetailText;
            public TextMeshProUGUI VideoButtonText;
            [Header("---Currency---")]
            public int AdditionalCustomers;
            public TextMeshProUGUI ChanceDetailText;
            public int GemCost;
            public TextMeshProUGUI GemCostText;
        }

        [Serializable]
        private class OfferInfo
        {
            public GameObject Panel;
            public PlayDoTweenSequence OpeningSequence;
            public PlayDoTweenSequence ClosingSequence;

            [Header("---Time Chance---")]
            public GameObject TimeChanceSymbolHolder;
            public int AdditionalSeconds;
            public TextMeshProUGUI TimerText;
            [Header("---Patience Chance---")]
            public GameObject PatienceChanceSymbolHolder;
            [Header("---More Customer Chance---")]
            public GameObject MoreCustomerChanceSymbolHolder;
            public int AdditionalCustomers;
            public TextMeshProUGUI AddionalCustomersText;

            public int TotalCoins;
            public TextMeshProUGUI TotalCoinText;
            public int TotalGems;
            public TextMeshProUGUI TotalGemText;
            public int TotalTimeFroze;
            public TextMeshProUGUI TotalTimeFrozeText;
            public int TotalWaitressSpeed;
            public TextMeshProUGUI TotalWaitressSpeedText;
            public int TotalInstanceOrderFill;
            public TextMeshProUGUI TotalInstanceOrderFillText;

            public TextMeshProUGUI PurchaseButtonText;
        }

        [SerializeField] GameObject m_Popup;

        [Header("---Time Chance Popup---")]
        [SerializeField] TimeChancePopupInfo m_TimeChancePopupInfo;

        [Header("---Customer Patience Chance Popup---")]
        [SerializeField] CustomerPatienceChancePopupInfo m_CustomerPatienceChancePopupInfo;

        [Header("---More Customer Chance Popup---")]
        [SerializeField] MoreCustomerChancePopupInfo m_MoreCustomerChancePopupInfo;

        [Header("---Offer Info---")]
        [SerializeField] OfferInfo m_OfferInfo;

        private bool m_CanAcquireDoNotLoseCustomerLevelChance;


        private LevelLostReason m_LevelLostReason = LevelLostReason.None;


        public override void Setup()
        {
            m_CanAcquireDoNotLoseCustomerLevelChance = false;

            DataLevel data =  DataManager.GetCurrentDataLevel();
            if (data != null)
            {
                foreach (var constraint in data.LevelConstraintInfos)
                {
                    if (constraint.ConstraintType == LevelConstraintType.DoNotLoseCustomer)
                    {
                        m_CanAcquireDoNotLoseCustomerLevelChance = true;
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// This function does not do anything use OpenChancePopup
        /// </summary>
        public override void OpenPopup(Action onComplete)
        {
            onComplete?.Invoke();   
        }

        /// <summary>
        /// This function does not do anything use CloseChancePopup
        /// </summary>
        public override void ClosePopup(Action onComplete)
        {
            onComplete?.Invoke();   
        }

        public void OpenChancePopup(LevelGoalType levelGoalType, int goalTargetValue, int goalCurrentValue, LevelLostReason levelLostReason, Action onComplete)
        {
            m_Popup.SetActive(true);
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_LevelLostReason = levelLostReason;

            if (levelLostReason == LevelLostReason.NoMoreTime)
            {
                m_TimeChancePopupInfo.CoinTargetHolder.SetActive(false);
                m_TimeChancePopupInfo.CustomerTargetHolder.SetActive(false);
                if (levelGoalType == LevelGoalType.CollectCoins)
                {
                    m_TimeChancePopupInfo.CoinTargetHolder.SetActive(true);
                    m_TimeChancePopupInfo.CoinRemaingTarget.text = (goalTargetValue - goalCurrentValue).ToString();

                }
                else if (levelGoalType == LevelGoalType.ServeCustomers)
                {
                    m_TimeChancePopupInfo.CustomerTargetHolder.SetActive(true);
                    m_TimeChancePopupInfo.CustomerRemaingTarget.text = (goalTargetValue - goalCurrentValue).ToString();
                }

                m_TimeChancePopupInfo.VideoTimerText.text = "+" + m_TimeChancePopupInfo.VideoAdditionalSeconds + "s";
                m_TimeChancePopupInfo.ChanceDetailText.text = "Add " + m_TimeChancePopupInfo.VideoAdditionalSeconds + " Seconds to keep playing!";
                m_TimeChancePopupInfo.VideoButtonText.text = "+" + m_TimeChancePopupInfo.VideoAdditionalSeconds.ToString() + "s";


                m_TimeChancePopupInfo.TimerText.text = "+" + m_TimeChancePopupInfo.AdditionalSeconds + "s";
                m_TimeChancePopupInfo.ChanceDetailText.text = "Add " + m_TimeChancePopupInfo.AdditionalSeconds + " Seconds to keep playing!";
                m_TimeChancePopupInfo.GemCostText.text = m_TimeChancePopupInfo.GemCost.ToString();

                m_TimeChancePopupInfo.Popup.SetActive(true);
                m_TimeChancePopupInfo.OpeningSequence.PlaySequence(() =>
                {
                    onComplete?.Invoke();
                });
            }
            else if (levelLostReason == LevelLostReason.LostACustomer)
            {
                m_CustomerPatienceChancePopupInfo.GemCostText.text = m_CustomerPatienceChancePopupInfo.GemCost.ToString();

                m_CustomerPatienceChancePopupInfo.Popup.SetActive(true);
                m_CustomerPatienceChancePopupInfo.OpeningSequence.PlaySequence(() =>
                {
                    onComplete?.Invoke();
                });
            }
            else if(levelLostReason == LevelLostReason.NoMoreCustomers)
            {
                m_MoreCustomerChancePopupInfo.CoinTargetHolder.SetActive(false);
                m_MoreCustomerChancePopupInfo.CustomerTargetHolder.SetActive(false);
                if (levelGoalType == LevelGoalType.CollectCoins)
                {
                    m_MoreCustomerChancePopupInfo.CoinTargetHolder.SetActive(true);
                    m_MoreCustomerChancePopupInfo.CoinRemaingTarget.text = (goalTargetValue - goalCurrentValue).ToString();

                }
                else if (levelGoalType == LevelGoalType.ServeCustomers)
                {
                    m_MoreCustomerChancePopupInfo.CustomerTargetHolder.SetActive(true);
                    m_MoreCustomerChancePopupInfo.CustomerRemaingTarget.text = (goalTargetValue - goalCurrentValue).ToString();
                }

                m_MoreCustomerChancePopupInfo.VideoChanceDetailText.text = "Add " + m_MoreCustomerChancePopupInfo.VideoAdditionalCustomers + " more Customers to keep playing!";
                m_MoreCustomerChancePopupInfo.VideoButtonText.text = "+" + m_MoreCustomerChancePopupInfo.VideoAdditionalCustomers.ToString();

                m_MoreCustomerChancePopupInfo.ChanceDetailText.text = "Add " + m_MoreCustomerChancePopupInfo.AdditionalCustomers + " more Customers to keep playing!";
                m_MoreCustomerChancePopupInfo.GemCostText.text = m_MoreCustomerChancePopupInfo.GemCost.ToString();

                m_MoreCustomerChancePopupInfo.Popup.SetActive(true);
                m_MoreCustomerChancePopupInfo.OpeningSequence.PlaySequence(() =>
                {
                    onComplete?.Invoke();
                });
            }

            m_OfferInfo.Panel.SetActive(true);

            m_OfferInfo.TimeChanceSymbolHolder.SetActive(false);
            m_OfferInfo.PatienceChanceSymbolHolder.SetActive(false);
            m_OfferInfo.MoreCustomerChanceSymbolHolder.SetActive(false);

            if (levelLostReason == LevelLostReason.NoMoreTime)
            {
                m_OfferInfo.TimeChanceSymbolHolder.SetActive(true);
                m_OfferInfo.TimerText.text = "+ " + m_OfferInfo.AdditionalSeconds;
            }
            else if(levelLostReason == LevelLostReason.LostACustomer)
            {
                m_OfferInfo.PatienceChanceSymbolHolder.SetActive(true);
            }
            else if (levelLostReason == LevelLostReason.NoMoreCustomers)
            {
                m_OfferInfo.MoreCustomerChanceSymbolHolder.SetActive(true);
                m_OfferInfo.AddionalCustomersText.text = m_OfferInfo.AdditionalCustomers.ToString();
            }

            m_OfferInfo.TotalCoinText.text = m_OfferInfo.TotalCoins.ToString();
            m_OfferInfo.TotalGemText.text = m_OfferInfo.TotalGems.ToString();
            m_OfferInfo.TotalTimeFrozeText.text = m_OfferInfo.TotalTimeFroze.ToString();
            m_OfferInfo.TotalWaitressSpeedText.text = m_OfferInfo.TotalWaitressSpeed.ToString();
            m_OfferInfo.TotalInstanceOrderFillText.text = m_OfferInfo.TotalInstanceOrderFill.ToString();

            m_OfferInfo.PurchaseButtonText.text = InAppManager.GetLocalizedPrice(PurchasingPackageType.RevivePack);

            m_OfferInfo.OpeningSequence.PlaySequence();
        }

        public void CloseChancePopup(Action onComplete)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);

            m_OfferInfo.ClosingSequence.PlaySequence(() => 
            {
                m_OfferInfo.Panel.SetActive(false);
            });

            if (m_LevelLostReason == LevelLostReason.NoMoreTime)
            {
                m_TimeChancePopupInfo.ClosingSequence.PlaySequence(() =>
                {
                    m_Popup.SetActive(false);
                    m_TimeChancePopupInfo.Popup.SetActive(false);
                    onComplete?.Invoke();
                });
            }
            else if (m_LevelLostReason == LevelLostReason.LostACustomer)
            {
                m_CustomerPatienceChancePopupInfo.ClosingSequence.PlaySequence(() =>
                {
                    m_Popup.SetActive(false);
                    m_CustomerPatienceChancePopupInfo.Popup.SetActive(false);
                    onComplete?.Invoke();
                });
            }
            else if (m_LevelLostReason == LevelLostReason.NoMoreCustomers)
            {
                m_MoreCustomerChancePopupInfo.ClosingSequence.PlaySequence(() =>
                {
                    m_Popup.SetActive(false);
                    m_MoreCustomerChancePopupInfo.Popup.SetActive(false);
                    onComplete?.Invoke();
                });
            }
            else
            {
                m_Popup.SetActive(false);
                onComplete?.Invoke();
            }
        }

        public void OnSecondChancedAvailedButton(bool videoUsed)
        {
            if (m_LevelLostReason == LevelLostReason.NoMoreTime)
            {
                if (videoUsed)
                {
                    LevelManager.SetHasLevelChanceAvailed(true);
                    LevelManager.SetTimeConstraint(m_TimeChancePopupInfo.VideoAdditionalSeconds);
                    UIManager.CloseLevelChancePopupAfterAvailed();
                }
                else
                {
                    if (m_TimeChancePopupInfo.GemCost <= DataManager.GemCurrency)
                    {
                        LevelManager.SetHasLevelChanceAvailed(true);
                        LevelManager.SetTimeConstraint(m_TimeChancePopupInfo.AdditionalSeconds);
                        UIManager.CloseLevelChancePopupAfterAvailed();

                        DataManager.GemCurrency -= m_TimeChancePopupInfo.GemCost;
                        DataManager.SaveData();
                    }
                    else
                    {
                        GeneralPopupUIManager.OpenNotEnoughGemShopPopup();
                    }
                }
            }
            else if (m_LevelLostReason == LevelLostReason.NoMoreCustomers)
            {
                if (videoUsed)
                {
                    LevelManager.SetHasLevelChanceAvailed(true);
                    LevelManager.IncreaseCustomerConstraintBy(m_MoreCustomerChancePopupInfo.VideoAdditionalCustomers);
                    CustomerManager.AddExtraCustomers(m_MoreCustomerChancePopupInfo.VideoAdditionalCustomers);
                    UIManager.CloseLevelChancePopupAfterAvailed();
                }
                else
                {
                    if (m_MoreCustomerChancePopupInfo.GemCost <= DataManager.GemCurrency)
                    {
                        LevelManager.SetHasLevelChanceAvailed(true);
                        LevelManager.IncreaseCustomerConstraintBy(m_MoreCustomerChancePopupInfo.AdditionalCustomers);
                        CustomerManager.AddExtraCustomers(m_MoreCustomerChancePopupInfo.AdditionalCustomers);
                        UIManager.CloseLevelChancePopupAfterAvailed();

                        DataManager.GemCurrency -= m_MoreCustomerChancePopupInfo.GemCost;
                        DataManager.SaveData();
                    }
                    else
                    {
                        GeneralPopupUIManager.OpenNotEnoughGemShopPopup();
                    }
                }
            }
            else if (m_LevelLostReason == LevelLostReason.LostACustomer)
            {
                if (m_CustomerPatienceChancePopupInfo.GemCost <= DataManager.GemCurrency)
                {
                    LevelManager.SetHasLevelChanceAvailed(true);
                    CustomerManager.ResetCustomersForLevelChance();
                    UIManager.CloseLevelChancePopupAfterAvailed();

                    DataManager.GemCurrency -= m_CustomerPatienceChancePopupInfo.GemCost;
                    DataManager.SaveData();
                }
                else
                {
                    GeneralPopupUIManager.OpenNotEnoughGemShopPopup();
                }
            }
            else
            {
                UIManager.CloseLevelChancePopupAfterNotAvailed();
            }

        }

        public void OnSecondChancedAvailedByOfferButton()
        {
            LevelLostReason temLevelLostReason = m_LevelLostReason;
            InAppManager.Purchase(PurchasingPackageType.RevivePack, () =>
            {
                GeneralPopupUIManager.OpenPurchaseSuccessPopup();

                DataManager.GemCurrency += m_OfferInfo.TotalCoins;
                DataManager.GemCurrency += m_OfferInfo.TotalGems;
                DataManager.InstanceOrderFillBoosterCount += m_OfferInfo.TotalInstanceOrderFill;
                DataManager.TimeFrozeBoosterCount += m_OfferInfo.TotalTimeFroze;
                DataManager.WaitressSpeedBoosterCount += m_OfferInfo.TotalWaitressSpeed;
                DataManager.SaveData();

                int? coin = m_OfferInfo.TotalCoins > 0 ? m_OfferInfo.TotalCoins : null;
                int? gem = m_OfferInfo.TotalGems > 0 ? m_OfferInfo.TotalGems : null;
                int? timeFrozeBoosterCount = m_OfferInfo.TotalTimeFroze > 0 ? m_OfferInfo.TotalTimeFroze : null;
                int? instanceOrderFillBoosterCount = m_OfferInfo.TotalInstanceOrderFill > 0 ? m_OfferInfo.TotalInstanceOrderFill : null;
                int? speedBoosterCount = m_OfferInfo.TotalWaitressSpeed > 0 ? m_OfferInfo.TotalWaitressSpeed : null;

                UIManager.SetRewardCollectionPopupMessage("Tap to Collect!", RewardCollectionTextPoistion.FarDownMiddle);
                UIManager.OpenRewardCollectionPopup(coin, gem, null, timeFrozeBoosterCount, instanceOrderFillBoosterCount, speedBoosterCount, null, () =>
                {
                    SoundManager.PlaySound(SoundType.BigReward);
                }, () => 
                {
                    if (temLevelLostReason == LevelLostReason.NoMoreTime)
                    {
                        LevelManager.SetHasLevelChanceAvailed(true);
                        LevelManager.SetTimeConstraint(m_OfferInfo.AdditionalSeconds);
                        UIManager.CloseLevelChancePopupAfterAvailed();
                    }
                    else if (temLevelLostReason == LevelLostReason.NoMoreCustomers)
                    {
                        LevelManager.SetHasLevelChanceAvailed(true);
                        LevelManager.IncreaseCustomerConstraintBy(m_OfferInfo.AdditionalCustomers);
                        CustomerManager.AddExtraCustomers(m_OfferInfo.AdditionalCustomers);
                        UIManager.CloseLevelChancePopupAfterAvailed();
                    }
                    else
                    {
                        LevelManager.SetHasLevelChanceAvailed(true);
                        CustomerManager.ResetCustomersForLevelChance();
                        UIManager.CloseLevelChancePopupAfterAvailed();
                    }
                });

                int level = DataManager.CurrentMapLevelIndex + 1;

                GeneralPopupUIManager.OpenPurchaseSuccessPopup();
            },
            () =>
            {
                GeneralPopupUIManager.OpenPurchaseFailedPopup();
            });
        }

        public void OnCloseButton()
        {
            UIManager.CloseLevelChancePopupAfterNotAvailed();
        }

        #region Helper
        public bool CanAcquireDoNotLoseCustomerLevelChance()
        {
            return m_CanAcquireDoNotLoseCustomerLevelChance;
        }
        #endregion
    }
}
