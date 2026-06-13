using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using Isometric.UI;
using CustomerPanelType = Isometric.UI.CustomerAchievementPanelType;
using Achievement = Isometric.UI.AchievementPanelType;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataMapAchievementUpdate", menuName = "GameData/DataMapAchievements")]
    public class DataMapAchievementUpdate : ScriptableObject
    {
        [Header("---Customer---")]
        [SerializeField] UIGeneralPanel m_CustomerAchievementPanelPrefab;
        [SerializeField] List<CustomerAchievementInfo> m_CustomersAchievementInfo;

        [Header("---Achievement---")]
        [SerializeField] UIGeneralPanel m_AchievementPanelPrefab;
        [SerializeField] List<AchievementInfo> m_AchievementsInfo;

        /// <summary>
        /// Is going to return panel list bundle of customer achievemts also if reward is collectable 
        /// onClaimCallback(Coins, CoinTransfrom, Gems, GemTransform)
        /// </summary>
        public List<Tuple<RectTransform, RectTransform, bool>> GetCustomerAchievementPanels(AchievementUIManager achievementUIManager, Transform holder, Transform characterHolder, Action<int, Transform, int, Transform> onClaimCallback)
        {
            List<Tuple<RectTransform, RectTransform, bool>> customersPanel = new List<Tuple<RectTransform, RectTransform, bool>>();
            foreach (var customerAchievementInfo in m_CustomersAchievementInfo)
            {
                bool isCollectable = false;
                bool isAvailable = false;

                UIGeneralPanel customerAchievementPanel = Instantiate(m_CustomerAchievementPanelPrefab, holder);

                customerAchievementPanel.GetPanelHolding<TextMeshProUGUI, CustomerPanelType>(CustomerPanelType.HeaderNameText).text = customerAchievementInfo.CustomerName;

                GameObject coinHolder = customerAchievementPanel.GetPanelHolding<GameObject, CustomerPanelType>(CustomerPanelType.CoinRewardHolder);
                GameObject gemHolder = customerAchievementPanel.GetPanelHolding<GameObject, CustomerPanelType>(CustomerPanelType.GemRewardHolder);

                Transform coinTransform = customerAchievementPanel.GetPanelHolding<Transform, CustomerPanelType>(CustomerPanelType.CoinTransform);
                Transform gemTransform = customerAchievementPanel.GetPanelHolding<Transform, CustomerPanelType>(CustomerPanelType.GemTransfrom);

                NotificationMainUI notificationMain = customerAchievementPanel.GetPanelHolding<NotificationMainUI, CustomerPanelType>(CustomerPanelType.NotificationMain);

                notificationMain.gameObject.SetActive(false);

                coinHolder.SetActive(false);
                gemHolder.SetActive(false);

                int coinReward = 0;
                int gamReward = 0;

                if (customerAchievementInfo.HasCoinReward)
                {
                    coinReward = customerAchievementInfo.CoinReward;
                    coinHolder.SetActive(true);
                    customerAchievementPanel.GetPanelHolding<TextMeshProUGUI, CustomerPanelType>(CustomerPanelType.CoinRewardText).text = customerAchievementInfo.CoinReward.ToString();
                }

                if (customerAchievementInfo.HasGemReward)
                {
                    gamReward = customerAchievementInfo.GemReward;
                    gemHolder.SetActive(true);
                    customerAchievementPanel.GetPanelHolding<TextMeshProUGUI, CustomerPanelType>(CustomerPanelType.GemRewardText).text = customerAchievementInfo.GemReward.ToString();
                }

                GameObject lockButton = customerAchievementPanel.GetPanelHolding<GameObject, CustomerPanelType>(CustomerPanelType.LockedButton);
                Button claimButton = customerAchievementPanel.GetPanelHolding<Button, CustomerPanelType>(CustomerPanelType.ClaimButton);
                GameObject claimedButton = customerAchievementPanel.GetPanelHolding<GameObject, CustomerPanelType>(CustomerPanelType.ClaimedButton);

                lockButton.SetActive(false);
                claimButton.gameObject.SetActive(false);
                claimedButton.SetActive(false);

                int level = DataManager.CurrentMapLevelIndex + 1;

                TextMeshProUGUI levelText = customerAchievementPanel.GetPanelHolding<TextMeshProUGUI, CustomerPanelType>(CustomerPanelType.AtLevelUnlockText);

                levelText.text = "Unlock at Level " + customerAchievementInfo.UnlockAtLevel;

                if (customerAchievementInfo.UnlockAtLevel <= level)
                {
                    isAvailable = true;

                    if (DataManager.GetBool(customerAchievementInfo.CustomerId + "RewardCollected", false) == false)
                    {
                        notificationMain.gameObject.SetActive(true);
                        List<NotificationParentUI> notificationParent = new List<NotificationParentUI>();
                        notificationParent.Add(achievementUIManager.GetNotificationCustmoerAchievement());
                        notificationMain.Setup(customerAchievementInfo.NotifationSeenKey, notificationParent);

                        levelText.text = "Unlocked";
                        isCollectable = true;
                        claimButton.gameObject.SetActive(true);
                        claimButton.onClick.AddListener(() =>
                        {
                            claimButton.gameObject.SetActive(false);
                            claimedButton.SetActive(true);
                            onClaimCallback?.Invoke(coinReward, coinTransform, gamReward, gemTransform);
                            levelText.text = "Collected";

                            DataManager.SetBool(customerAchievementInfo.CustomerId + "RewardCollected", true);

                            notificationMain.OnNotficationSeen();
                        });
                    }
                    else
                    {
                        levelText.text = "Collected";
                        claimedButton.SetActive(true);
                    }
                }
                else
                {
                    lockButton.SetActive(true);
                }

                GameObject character = null;
                if(isAvailable)
                {
                    character = Instantiate(customerAchievementInfo.CharacterAvailablePrefab, characterHolder);
                }
                else
                {
                    character = Instantiate(customerAchievementInfo.CharacterNotAvailablePrefab, characterHolder);
                }
                RectTransform panelRectTransform = customerAchievementPanel.GetComponent<RectTransform>();
                RectTransform characterRectTransform = character.GetComponent<RectTransform>();
                
                Tuple<RectTransform, RectTransform, bool> customerPanel = new Tuple<RectTransform, RectTransform, bool>(panelRectTransform, characterRectTransform, isCollectable);

                customersPanel.Add(customerPanel);

            }
            return customersPanel;
        }

        /// <summary>
        /// Returns true if the achievement record is set on the first time
        /// </summary>
        public bool SetAchievementRecord(AchievementType achievementType, int addToRecord)
        {
            AchievementInfo achievementInfo = m_AchievementsInfo.Find((x) => x.Achievement == achievementType);
            if(achievementInfo != null)
            {
                int record = DataManager.GetInt(achievementInfo.CollectionRecordKey, 0);
                if (record < achievementInfo.Target)
                {
                    record += addToRecord;
                    record = Mathf.Clamp(record, 0, achievementInfo.Target);
                    DataManager.SetInt(achievementInfo.CollectionRecordKey, record);

                    if (record >= achievementInfo.Target)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsAchievementComplete(AchievementType achievementType)
        {
            AchievementInfo achievementInfo = m_AchievementsInfo.Find((x) => x.Achievement == achievementType);
            if (achievementInfo != null)
            {
                int record = DataManager.GetInt(achievementInfo.CollectionRecordKey, 0);
                if (record >= achievementInfo.Target)
                {
                    return true;
                }
            }

            return false;
        }

        public string GetAchievementNotificationMessage(AchievementType achievementType)
        {
            AchievementInfo achievementInfo = m_AchievementsInfo.Find((x) => x.Achievement == achievementType);
            if (achievementInfo != null)
            {
                return achievementInfo.NotificationText;
            }

            return "";
        }

        public int GetAchievementTarget(AchievementType achievementType)
        {
            AchievementInfo achievementInfo = m_AchievementsInfo.Find((x) => x.Achievement == achievementType);
            if (achievementInfo != null)
            {
                return achievementInfo.Target;
            }

            return 0;
        }

        public int GetAchievementRecord(AchievementType achievementType)
        {
            AchievementInfo achievementInfo = m_AchievementsInfo.Find((x) => x.Achievement == achievementType);
            if (achievementInfo != null)
            {
                return DataManager.GetInt(achievementInfo.CollectionRecordKey, 0);
            }

            return 0;
        }

        /// <summary>
        /// Is going to return Achievement Panels
        /// onClaimCallback(Coins, CoinTransfrom, Gems, GemTransform)
        /// </summary>
        public List<Transform> GetAchievementPanels(AchievementUIManager achievementUIManager, Transform holder, Action<int, Transform, int, Transform> onClaimCallback)
        {
            List<Transform> panels = new List<Transform>();


            List<AchievementInfo> frontList = new List<AchievementInfo>();   // record >= target && !isCollected
            List<AchievementInfo> middleList = new List<AchievementInfo>();  // !isCollected
            List<AchievementInfo> endList = new List<AchievementInfo>();     // isCollected

            foreach (var item in m_AchievementsInfo)
            {
                int record = DataManager.GetInt(item.CollectionRecordKey, 0);
                bool isCollected = DataManager.GetBool(item.CollectionKey, false);

                if (record >= item.Target && isCollected == false)
                {
                    frontList.Add(item); // First: ready to collect
                }
                else if (isCollected == false)
                {
                    middleList.Add(item); // Second: not yet complete
                }
                else
                {
                    endList.Add(item); // Third: already collected
                }
            }

            // Final ordered list
            List<AchievementInfo> achievementsInfo = new List<AchievementInfo>();
            achievementsInfo.AddRange(frontList);
            achievementsInfo.AddRange(middleList);
            achievementsInfo.AddRange(endList);


            foreach (AchievementInfo achievementInfo in achievementsInfo)
            {
                UIGeneralPanel achievementPanel = Instantiate(m_AchievementPanelPrefab, holder);

                achievementPanel.GetPanelHolding<Image, Achievement>(Achievement.PreviewImage).sprite = achievementInfo.PreviewSprite;
                achievementPanel.GetPanelHolding<TextMeshProUGUI, Achievement>(Achievement.DiscriptionText).text = achievementInfo.DiscriptionText;

                Image barFill = achievementPanel.GetPanelHolding<Image, Achievement>(Achievement.BarFillImage);
                TextMeshProUGUI barText = achievementPanel.GetPanelHolding<TextMeshProUGUI, Achievement>(Achievement.BarText);

                GameObject coinHolder = achievementPanel.GetPanelHolding<GameObject, Achievement>(Achievement.CoinRewardGameObject);
                coinHolder.SetActive(false);
                TextMeshProUGUI coinText = achievementPanel.GetPanelHolding<TextMeshProUGUI, Achievement>(Achievement.CoinRewardText);

                GameObject gemHolder = achievementPanel.GetPanelHolding<GameObject, Achievement>(Achievement.GemRewardGameObject);
                gemHolder.SetActive(false);
                TextMeshProUGUI gemText = achievementPanel.GetPanelHolding<TextMeshProUGUI, Achievement>(Achievement.GemRewardText);

                Button claimButton = achievementPanel.GetPanelHolding<Button, Achievement>(Achievement.ClaimButton);
                claimButton.gameObject.SetActive(false);
                
                GameObject lockedGameobject = achievementPanel.GetPanelHolding<GameObject, Achievement>(Achievement.LockedGameObject);
                lockedGameobject.SetActive(false);

                GameObject claimedGameobject = achievementPanel.GetPanelHolding<GameObject, Achievement>(Achievement.ClaimedGameObject);
                claimedGameobject.SetActive(false);

                Transform coinTransfrom = achievementPanel.GetPanelHolding<Transform, Achievement>(Achievement.CoinTransfrom);
                Transform gemTransfrom = achievementPanel.GetPanelHolding<Transform, Achievement>(Achievement.GemTransfrom);

                NotificationMainUI notificationMain = achievementPanel.GetPanelHolding<NotificationMainUI, Achievement>(Achievement.NotificationMain);
                notificationMain.gameObject.SetActive(false);


                int coinReward = achievementInfo.HasCoinReward ? achievementInfo.CoinReward : 0;
                int gemReawrd = achievementInfo.HasGemReward ? achievementInfo.GemReward : 0;

                if (achievementInfo.HasCoinReward)
                {
                    coinHolder.SetActive(true);
                    coinText.text = achievementInfo.CoinReward.ToString();
                }

                if (achievementInfo.HasGemReward)
                {
                    gemHolder.SetActive(true);
                    gemText.text = achievementInfo.GemReward.ToString();
                }

                achievementPanel.GetPanelHolding<Image, Achievement>(Achievement.PreviewImage).sprite = achievementInfo.PreviewSprite;
                int record = DataManager.GetInt(achievementInfo.CollectionRecordKey, 0);
                bool isCollected = DataManager.GetBool(achievementInfo.CollectionKey, false);

                barFill.fillAmount = (float)record / (float)achievementInfo.Target;
                barText.text = record + "/" + achievementInfo.Target;

                string collectionKey = achievementInfo.CollectionKey;

                if (record >= achievementInfo.Target) // can be collected
                {
                    if (isCollected == false)
                    {
                        notificationMain.gameObject.SetActive(true);
                        List<NotificationParentUI> notificationParent = new List<NotificationParentUI>();
                        notificationParent.Add(achievementUIManager.GetNotificationAchievement());
                        notificationMain.Setup(achievementInfo.NotifationSeenKey, notificationParent);

                        claimButton.gameObject.SetActive(true);

                        claimButton.onClick.AddListener(() =>
                        {
                            claimButton.gameObject.SetActive(false);
                            lockedGameobject.SetActive(true);
                            onClaimCallback?.Invoke(coinReward, coinTransfrom, gemReawrd, gemTransfrom);

                            DataManager.SetBool(collectionKey, true);

                            notificationMain.OnNotficationSeen();

                            achievementUIManager.FullResetAchievementPanels();
                        });
                    }
                    else
                    {
                        claimedGameobject.SetActive(true);
                    }
                }
                else
                {
                    lockedGameobject.SetActive(true);
                }

            }

            return panels;
        }
    }

    [Serializable]
    public class CustomerAchievementInfo
    {
        public CustomerId CustomerId;
        
        [Space]
        public int UnlockAtLevel;

        [Header("---Reward---")]
        public bool HasCoinReward;
        [ShowIf(nameof(HasCoinReward))]
        public int CoinReward;
        public bool HasGemReward;
        [ShowIf(nameof(HasGemReward))]
        public int GemReward;


        [Header("---Panel---")]
        public string CustomerName;

        [Header("---Character---")]
        public GameObject CharacterNotAvailablePrefab;
        public GameObject CharacterAvailablePrefab;

        [Header("---Notification---")]
        public string NotifationSeenKey;

    }

    [Serializable]
    public class AchievementInfo
    {
        public AchievementType Achievement;
        public string CollectionRecordKey;
        public string CollectionKey;

        [Header("---Target---")]
        public int Target;

        [Header("---Reward---")]
        public bool HasCoinReward;
        [ShowIf(nameof(HasCoinReward))]
        public int CoinReward;
        public bool HasGemReward;
        [ShowIf(nameof(HasGemReward))]
        public int GemReward;

        [Header("---Info---")]
        public Sprite PreviewSprite;
        [TextArea]
        public string DiscriptionText;
        [TextArea]
        public string NotificationText;

        [Header("---Notification---")]
        public string NotifationSeenKey;
    }


    public enum AchievementType
    {
        AchievementCollectCustomers,
        AchievementCompleteAllLevels,
        Achievement25KeyCollected,
        AchievementPlayerWalksDistance,
        AchievementNotLostACustomer,
        AchievementCustomersServed,
        AchievementServedHappy,
        AchievementCustomerOrders,
        AchievementCoinCurrencyEarned,
        AchievementCompleteLevels,
        AchievementCoinCurrencySpendOnUpgrades,
        AchievementPlayForHour,
        AchievementPlayerMovingForSeconds,
        AchievementCompleteOrdersOnFastBooster,
        AchievementWorkerServesOrder,
        AchievementFreezeTheCustomersWait,
    }
}