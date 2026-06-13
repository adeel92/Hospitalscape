using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using Arc;
using Isometric.Data;
using Isometric.Sound;

namespace Isometric.UI
{
    public class VendingMachineUIManager : UIPopupBase
    {
        private const string c_VendingMachineActivationDurationKey = "VendingMachineActivationDurationKey";

        [Header("---Setup---")]
        [SerializeField] GameObject m_Popup;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;
        [SerializeField] int m_ActivateAfterLevel;
        [SerializeField] float m_ActivationDurationInSeconds;
        [SerializeField] TextMeshProUGUI m_TimerText;
        [SerializeField, ReadOnly] GameObject m_VendingMachineButton;
        [SerializeField, ReadOnly] TextMeshProUGUI m_VendingMachineButtonText;


        private DateTime m_AcitationDateTime;

        [Header("---Offer(1)---")]
        [SerializeField] string m_Offer1CollectionKey;
        [SerializeField] int m_Offer1TotalColleciton;
        [SerializeField] int m_Offer1TotalCoinValue;
        [SerializeField] TextMeshProUGUI m_Offer1TotalCoinText;
        [SerializeField] TextMeshProUGUI m_Offer1TotalCoinCollections;
        [SerializeField] int m_Offer1TotalGemValue;
        [SerializeField] TextMeshProUGUI m_Offer1TotalGemText;
        [SerializeField] TextMeshProUGUI m_Offer1TotalGemCollections;
        [SerializeField] GameObject m_Offer1CollectionButton;
        [SerializeField] GameObject m_Offer1CollectedButton;

        [Header("---Offer(2)---")]
        [SerializeField] string m_Offer2CollectionKey;
        [SerializeField] int m_Offer2TotalColleciton;
        [SerializeField] int m_Offer2TotalCoinValue;
        [SerializeField] TextMeshProUGUI m_Offer2TotalCoinText;
        [SerializeField] TextMeshProUGUI m_Offer2TotalCoinCollections;
        [SerializeField] GameObject m_Offer2CollectionButton;
        [SerializeField] GameObject m_Offer2CollectedButton;

        [Header("---Offer(3)---")]
        [SerializeField] string m_Offer3CollectionKey;
        [SerializeField] int m_Offer3TotalColleciton;
        [SerializeField] int m_Offer3TotalFrozenWaitBoosterValue;
        [SerializeField] TextMeshProUGUI m_Offer3TotalFrozenWaitBoosterText;
        [SerializeField] TextMeshProUGUI m_Offer3TotalFrozenWaitBoosterCollections;
        [SerializeField] GameObject m_Offer3CollectionButton;
        [SerializeField] GameObject m_Offer3CollectedButton;

        [Header("---Offer(4)---")]
        [SerializeField] string m_Offer4CollectionKey;
        [SerializeField] int m_Offer4TotalColleciton;
        [SerializeField] int m_Offer4TotalSpeedBoosterValue;
        [SerializeField] TextMeshProUGUI m_Offer4TotalSpeedBoosterText;
        [SerializeField] TextMeshProUGUI m_Offer4TotalSpeedBoosterCollections;
        [SerializeField] GameObject m_Offer4CollectionButton;
        [SerializeField] GameObject m_Offer4CollectedButton;

        [Header("---Offer(5)---")]
        [SerializeField] string m_Offer5CollectionKey;
        [SerializeField] int m_Offer5TotalColleciton;
        [SerializeField] int m_Offer5TotalOrderFillBoosterValue;
        [SerializeField] TextMeshProUGUI m_Offer5TotalOrderFillBoosterText;
        [SerializeField] TextMeshProUGUI m_Offer5TotalOrderFillBoosterCollections;
        [SerializeField] GameObject m_Offer5CollectionButton;
        [SerializeField] GameObject m_Offer5CollectedButton;


        public override void Setup()
        {
            if (DataManager.CurrentMapLevelIndex >= m_ActivateAfterLevel)
            {
                DateTime now = DateTime.UtcNow;
                if (DateTime.TryParse(DataManager.GetString(c_VendingMachineActivationDurationKey, now.ToString()), out DateTime savedTime))
                {
                    m_AcitationDateTime = savedTime;
                    TimeSpan timePassed = now - m_AcitationDateTime;

                    if (timePassed.TotalSeconds < m_ActivationDurationInSeconds)
                    {
                        m_VendingMachineButton.SetActive(true);
                        StartCoroutine(Timer());
                    }
                    else
                    {
                        m_VendingMachineButton.SetActive(false);
                    }
                }
                else
                {
                    m_VendingMachineButton.SetActive(false);
                }

                m_Offer1TotalCoinText.text = "x" + m_Offer1TotalCoinValue;
                m_Offer1TotalGemText.text = "x" + m_Offer1TotalGemValue;
                if (DataManager.GetInt(m_Offer1CollectionKey, 0) >= m_Offer1TotalColleciton)
                {
                    m_Offer1TotalCoinCollections.text = DataManager.GetInt(m_Offer1CollectionKey, 0) + "/" + m_Offer1TotalColleciton;
                    m_Offer1TotalGemCollections.text = DataManager.GetInt(m_Offer1CollectionKey, 0) + "/" + m_Offer1TotalColleciton;
                    m_Offer1CollectionButton.SetActive(false);
                    m_Offer1CollectedButton.SetActive(true);
                }
                else
                {
                    m_Offer1TotalCoinCollections.text = DataManager.GetInt(m_Offer1CollectionKey, 0) + "/" + m_Offer1TotalColleciton;
                    m_Offer1TotalGemCollections.text = DataManager.GetInt(m_Offer1CollectionKey, 0) + "/" + m_Offer1TotalColleciton;
                    m_Offer1CollectionButton.SetActive(true);
                    m_Offer1CollectedButton.SetActive(false);
                }


                m_Offer2TotalCoinText.text = "x" + m_Offer2TotalCoinValue;
                if (DataManager.GetInt(m_Offer2CollectionKey, 0) >= m_Offer2TotalColleciton)
                {
                    m_Offer2TotalCoinCollections.text = DataManager.GetInt(m_Offer2CollectionKey, 0) + "/" + m_Offer2TotalColleciton;
                    m_Offer2CollectionButton.SetActive(false);
                    m_Offer2CollectedButton.SetActive(true);
                }
                else
                {
                    m_Offer2TotalCoinCollections.text = DataManager.GetInt(m_Offer2CollectionKey, 0) + "/" + m_Offer2TotalColleciton;
                    m_Offer2CollectionButton.SetActive(true);
                    m_Offer2CollectedButton.SetActive(false);
                }

                m_Offer3TotalFrozenWaitBoosterText.text = "x" + m_Offer3TotalFrozenWaitBoosterValue;
                if (DataManager.GetInt(m_Offer3CollectionKey, 0) >= m_Offer3TotalColleciton)
                {
                    m_Offer3TotalFrozenWaitBoosterCollections.text = DataManager.GetInt(m_Offer3CollectionKey, 0) + "/" + m_Offer3TotalColleciton;
                    m_Offer3CollectionButton.SetActive(false);
                    m_Offer3CollectedButton.SetActive(true);
                }
                else
                {
                    m_Offer3TotalFrozenWaitBoosterCollections.text = DataManager.GetInt(m_Offer3CollectionKey, 0) + "/" + m_Offer3TotalColleciton;
                    m_Offer3CollectionButton.SetActive(true);
                    m_Offer3CollectedButton.SetActive(false);
                }

                m_Offer4TotalSpeedBoosterText.text = "x" + m_Offer4TotalSpeedBoosterValue;
                if (DataManager.GetInt(m_Offer4CollectionKey, 0) >= m_Offer4TotalColleciton)
                {
                    m_Offer4TotalSpeedBoosterCollections.text = DataManager.GetInt(m_Offer4CollectionKey, 0) + "/" + m_Offer4TotalColleciton;
                    m_Offer4CollectionButton.SetActive(false);
                    m_Offer4CollectedButton.SetActive(true);
                }
                else
                {
                    m_Offer4TotalSpeedBoosterCollections.text = DataManager.GetInt(m_Offer4CollectionKey, 0) + "/" + m_Offer4TotalColleciton;
                    m_Offer4CollectionButton.SetActive(true);
                    m_Offer4CollectedButton.SetActive(false);
                }

                m_Offer5TotalOrderFillBoosterText.text = "x" + m_Offer5TotalOrderFillBoosterValue;
                if (DataManager.GetInt(m_Offer5CollectionKey, 0) >= m_Offer5TotalColleciton)
                {
                    m_Offer5TotalOrderFillBoosterCollections.text = DataManager.GetInt(m_Offer5CollectionKey, 0) + "/" + m_Offer5TotalColleciton;
                    m_Offer5CollectionButton.SetActive(false);
                    m_Offer5CollectedButton.SetActive(true);
                }
                else
                {
                    m_Offer5TotalOrderFillBoosterCollections.text = DataManager.GetInt(m_Offer5CollectionKey, 0) + "/" + m_Offer5TotalColleciton;
                    m_Offer5CollectionButton.SetActive(true);
                    m_Offer5CollectedButton.SetActive(false);
                }
            }
            else
            {
                m_VendingMachineButton.SetActive(false);
            }
        }

        public override void OpenPopup(Action onCompete)
        {
            m_Popup.SetActive(true);
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_OpeningSequence.PlaySequence(() =>
            {
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

        IEnumerator Timer()
        {
            while (true)
            {
                TimeSpan timePassed = DateTime.UtcNow - m_AcitationDateTime;
                if (timePassed.TotalSeconds <= m_ActivationDurationInSeconds)
                {
                    TimeSpan remainingTime = TimeSpan.FromSeconds(m_ActivationDurationInSeconds - timePassed.TotalSeconds);
                    m_TimerText.text = $"{remainingTime.Days:D2}D:{remainingTime.Hours:D2}H:{remainingTime.Minutes:D2}M:{remainingTime.Seconds:D2}S";
                    m_VendingMachineButtonText.text = $"{remainingTime.Days:D2}D:{remainingTime.Hours:D2}H:{remainingTime.Minutes:D2}M";
                }
                else
                {
                    m_TimerText.text = "00:00:00";
                    m_VendingMachineButton.SetActive(false);
                    break;
                }
                yield return null;
            }
        }

        public void InsertVendingMachineButton(GameObject vendingMachineButton, TextMeshProUGUI vendingMachineButtonText)
        {
            m_VendingMachineButton = vendingMachineButton;
            m_VendingMachineButtonText = vendingMachineButtonText;
        }

        public void CollectOffer1()
        {
            DataManager.CoinCurrency += m_Offer1TotalCoinValue;
            DataManager.GemCurrency += m_Offer1TotalGemValue;
            DataManager.SetInt(m_Offer1CollectionKey, DataManager.GetInt(m_Offer1CollectionKey, 0) + 1);
            DataManager.SaveData();

            UIManager.SetRewardCollectionPopupMessage("Tap to Collect!", RewardCollectionTextPoistion.FarDownMiddle);
            UIManager.OpenRewardCollectionPopup(m_Offer1TotalCoinValue, m_Offer1TotalGemValue, null, null, null, null, null, () =>
            {
                SoundManager.PlaySound(SoundType.MediumReward);
            }, null);

                m_Offer1TotalCoinCollections.text = DataManager.GetInt(m_Offer1CollectionKey, 0) + "/" + m_Offer1TotalColleciton;
                m_Offer1TotalGemCollections.text = DataManager.GetInt(m_Offer1CollectionKey, 0) + "/" + m_Offer1TotalColleciton;

            if (DataManager.GetInt(m_Offer1CollectionKey, 0) >= m_Offer1TotalColleciton)
            {
                m_Offer1CollectionButton.SetActive(false);
                m_Offer1CollectedButton.SetActive(true);
            }
        }

        public void CollectOffer2()
        {
            DataManager.CoinCurrency += m_Offer2TotalCoinValue;
            DataManager.SetInt(m_Offer2CollectionKey, DataManager.GetInt(m_Offer2CollectionKey, 0) + 1);
            DataManager.SaveData();

            UIManager.SetRewardCollectionPopupMessage("Tap to Collect!", RewardCollectionTextPoistion.FarDownMiddle);
            UIManager.OpenRewardCollectionPopup(m_Offer1TotalCoinValue, null, null, null, null, null, null, () =>
            {
                SoundManager.PlaySound(SoundType.MediumReward);
            }, null);

            m_Offer2TotalCoinCollections.text = DataManager.GetInt(m_Offer2CollectionKey, 0) + "/" + m_Offer2TotalColleciton;
            if (DataManager.GetInt(m_Offer2CollectionKey, 0) >= m_Offer2TotalColleciton)
            {
                m_Offer2CollectionButton.SetActive(false);
                m_Offer2CollectedButton.SetActive(true);
            }
        }

        public void CollectOffer3()
        {
            DataManager.TimeFrozeBoosterCount += m_Offer3TotalFrozenWaitBoosterValue;
            DataManager.SetInt(m_Offer3CollectionKey, DataManager.GetInt(m_Offer3CollectionKey, 0) + 1);
            DataManager.SaveData();

            UIManager.SetRewardCollectionPopupMessage("Tap to Collect!", RewardCollectionTextPoistion.FarDownMiddle);
            UIManager.OpenRewardCollectionPopup(null, null, null, m_Offer3TotalFrozenWaitBoosterValue, null, null, null, () =>
            {
                SoundManager.PlaySound(SoundType.MediumReward);
            }, null);

            m_Offer3TotalFrozenWaitBoosterCollections.text = DataManager.GetInt(m_Offer3CollectionKey, 0) + "/" + m_Offer3TotalColleciton;
            if (DataManager.GetInt(m_Offer3CollectionKey, 0) >= m_Offer3TotalColleciton)
            {
                m_Offer3CollectionButton.SetActive(false);
                m_Offer3CollectedButton.SetActive(true);
            }
        }

        public void CollectOffer4()
        {
            DataManager.WaitressSpeedBoosterCount += m_Offer4TotalSpeedBoosterValue;
            DataManager.SetInt(m_Offer4CollectionKey, DataManager.GetInt(m_Offer4CollectionKey, 0) + 1);
            DataManager.SaveData();

            UIManager.SetRewardCollectionPopupMessage("Tap to Collect!", RewardCollectionTextPoistion.FarDownMiddle);
            UIManager.OpenRewardCollectionPopup(null, null, null, null, null, m_Offer4TotalSpeedBoosterValue, null, () =>
            {
                SoundManager.PlaySound(SoundType.MediumReward);
            }, null);

            m_Offer4TotalSpeedBoosterCollections.text = DataManager.GetInt(m_Offer4CollectionKey, 0) + "/" + m_Offer4TotalColleciton;
            if (DataManager.GetInt(m_Offer4CollectionKey, 0) >= m_Offer4TotalColleciton)
            {
                m_Offer4CollectionButton.SetActive(false);
                m_Offer4CollectedButton.SetActive(true);
            }
        }

        public void CollectOffer5()
        {
            DataManager.InstanceOrderFillBoosterCount += m_Offer5TotalOrderFillBoosterValue;
            DataManager.SetInt(m_Offer5CollectionKey, DataManager.GetInt(m_Offer5CollectionKey, 0) + 1);
            DataManager.SaveData();

            UIManager.SetRewardCollectionPopupMessage("Tap to Collect!", RewardCollectionTextPoistion.FarDownMiddle);
            UIManager.OpenRewardCollectionPopup(null, null, null, null, m_Offer5TotalOrderFillBoosterValue, null, null, () =>
            {
                SoundManager.PlaySound(SoundType.MediumReward);
            }, null);

            m_Offer5TotalOrderFillBoosterCollections.text = DataManager.GetInt(m_Offer5CollectionKey, 0) + "/" + m_Offer5TotalColleciton;
            if (DataManager.GetInt(m_Offer5CollectionKey, 0) >= m_Offer5TotalColleciton)
            {
                m_Offer5CollectionButton.SetActive(false);
                m_Offer5CollectedButton.SetActive(true);
            }
        }

        public void OnCloseButton()
        {
            UIManager.ClosePopup<VendingMachineUIManager>();
        }
    }
}