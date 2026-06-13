using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyAttributes;
using Arc.Attribute;
using Isometric.UI;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataMapUpdate", menuName = "GameData/DataMapUpdate")]
    public class DataMapUpdate : ScriptableObject
    {
        [Space, Header("---Player---")]
        [SerializeField] PlayerUpdateInfo m_PlayerUpdateInfo;

        [Space, Header("---Chair---")]
        [SerializeField] List<ChairUpdateInfo> m_ChairsUpdateInfo;

        [Space, Header("---Workers---")]
        [SerializeField] WorkerUpdateInfo m_WorkerUpdateInfo;

        [Space, Header("---Stations---")]
        [SerializeField] List<StationUpdateInfo> m_StationsUpdateInfo;

        [Space, Header("---Decoration---")]
        [SerializeField] List<DecorationUpdateInfo> m_DecorationsUpdateInfo;

        [Space, Header("---Patience---")]
        [SerializeField] DataMapPatienceUpdate m_DataMapPatienceUpdate;

        [Space, Header("---Booster---")]
        [SerializeField] List<BoosterInfo> m_BoostersInfo;

        [Space, Header("---Achievement---")]
        [SerializeField] DataMapAchievementUpdate m_MapAchievementUpdate;

        [Space, Header("---Profie---")]
        [SerializeField] DataProfile m_DataProfile;

        #region Player Related
        public List<PlayerUpgradePanelUI> GetAndSetPlayerUpgradePanels(Transform holder)
        {
            List<PlayerUpgradePanelUI> PlayerUpgradePanelsUI = new List<PlayerUpgradePanelUI>();

            PlayerData playerData = m_PlayerUpdateInfo.Data.PlayerData;
            foreach (var playerUpgrade in m_PlayerUpdateInfo.PlayerUpgradesInfo)
            {
                PlayerUpgradePanelUI playerUpgradePanelUI = Instantiate(playerUpgrade.PlayerUpgradePanelUIPrefab, holder);
                playerUpgradePanelUI.Setup(playerUpgrade.UpgradeType);
                PlayerUpgradePanelsUI.Add(playerUpgradePanelUI);
                playerUpgradePanelUI.HeaderText.text = playerUpgrade.HeaderText;
                playerUpgradePanelUI.PreviewImage.sprite = playerUpgrade.BasePreviewSprite;

                playerUpgradePanelUI.InfoDiscriptionText = playerUpgrade.InfoDiscriptionText;
                playerUpgradePanelUI.InfoPreviewSprite = playerUpgrade.BasePreviewSprite;

                playerUpgradePanelUI.NotifiationSeenKey = playerUpgrade.NotifationSeenKey;

                int currentIndex = 0;
                if (playerUpgrade.UpgradeType == PlayerUpgradeType.Capacity)
                {
                    currentIndex = playerData.CurrentCapacityIndex + 1;
                }
                else
                {
                    currentIndex = playerData.CurrentWalkSpeedIndex + 1;
                }

                playerUpgradePanelUI.StepFillBar.SetupBars(playerUpgrade.PropertyUpgradesInfo.Count - 1);
                playerUpgradePanelUI.StepFillBar.SetFillAmount(currentIndex - 1);

                if (currentIndex < playerUpgrade.PropertyUpgradesInfo.Count)
                {
                    playerUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                    playerUpgradePanelUI.MaxButton.gameObject.SetActive(false);
                    playerUpgradePanelUI.UpgradeNameText.gameObject.SetActive(true);
                    playerUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);
                    playerUpgradePanelUI.Tick.SetActive(false);

                    playerUpgradePanelUI.UpgradeNameText.text = playerUpgrade.UpgradeNameText;

                    if (playerUpgrade.UpgradeType == PlayerUpgradeType.Capacity
                        && playerData.Capacity.Count > currentIndex)
                    {
                        string valueText = "";
                        if (playerData.CurrentCapacityIndex == 0)
                        {
                            valueText = "-> " + playerData.Capacity[currentIndex].ToString();
                        }
                        else
                        {
                            valueText = playerData.Capacity[playerData.CurrentCapacityIndex] + " -> " + playerData.Capacity[currentIndex];
                        }
                        playerUpgradePanelUI.UpgradeValueText.text = valueText;
                    }
                    else if (playerUpgrade.UpgradeType == PlayerUpgradeType.WalkSpeed
                        && playerData.WalkSpeed.Count > currentIndex)
                    {
                        float baseSpeed = playerData.WalkSpeed[0];
                        float currentSpeed = playerData.WalkSpeed[playerData.CurrentWalkSpeedIndex];
                        float upgradedSpeed = playerData.WalkSpeed[currentIndex];

                        int upgradedDifference = (int)(((upgradedSpeed - currentSpeed) / currentSpeed) * 100f);

                        playerUpgradePanelUI.UpgradeValueText.text = "+" + upgradedDifference + "%";

                    }
                    else
                    {
                        Debug.LogWarning("Player Capacity upgrade does not match with expected upgrades in the DataMapUpdates");
                    }

                    var propertyUpgradeInfo = playerUpgrade.PropertyUpgradesInfo[currentIndex];
                    if (propertyUpgradeInfo.UpdateType == CurrencyType.Coin)
                    {
                        playerUpgradePanelUI.UpgradeButtonText.text = propertyUpgradeInfo.RequiredCoins.ToString();
                        playerUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                        playerUpgradePanelUI.UpgradeCost = propertyUpgradeInfo.RequiredCoins;
                        playerUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                        playerUpgradePanelUI.UpgradeButtonGem.SetActive(false);

                        if (propertyUpgradeInfo.RequiredCoins == 0)
                        {
                            playerUpgradePanelUI.UpgradeButtonText.text = "Free";
                            Vector2 position = playerUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition;
                            position.x = 0;
                            playerUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition = position;
                            playerUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                            playerUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                        }
                    }
                    else if (propertyUpgradeInfo.UpdateType == CurrencyType.Gem)
                    {
                        playerUpgradePanelUI.UpgradeButtonText.text = propertyUpgradeInfo.RequiredGems.ToString();
                        playerUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                        playerUpgradePanelUI.UpgradeCost = propertyUpgradeInfo.RequiredGems;
                        playerUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                        playerUpgradePanelUI.UpgradeButtonGem.SetActive(true);

                        if (propertyUpgradeInfo.RequiredGems == 0)
                        {
                            playerUpgradePanelUI.UpgradeButtonText.text = "Free";
                            Vector2 position = playerUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition;
                            position.x = 0;
                            playerUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition = position;
                            playerUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                            playerUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                        }
                    }


                    if (propertyUpgradeInfo.UsePreviewSprite)
                    {
                        playerUpgradePanelUI.PreviewImage.sprite = propertyUpgradeInfo.PreviewSprite;
                        playerUpgradePanelUI.InfoPreviewSprite = propertyUpgradeInfo.PreviewSprite;
                    }
                }
                else
                {
                    playerUpgradePanelUI.UpgradeButton.gameObject.SetActive(false);
                    playerUpgradePanelUI.MaxButton.gameObject.SetActive(true);
                    playerUpgradePanelUI.UpgradeNameText.gameObject.SetActive(false);
                    playerUpgradePanelUI.UpgradeValueText.gameObject.SetActive(false);
                    playerUpgradePanelUI.Tick.SetActive(true);

                    var propertyUpgradeInfo = playerUpgrade.PropertyUpgradesInfo[^1];
                    if (propertyUpgradeInfo.UsePreviewSprite)
                    {
                        playerUpgradePanelUI.PreviewImage.sprite = propertyUpgradeInfo.PreviewSprite;
                        playerUpgradePanelUI.InfoPreviewSprite = propertyUpgradeInfo.PreviewSprite;
                    }
                }
            }

            return PlayerUpgradePanelsUI;
        }

        public PlayerUpgradePanelUI GetAndSetPlayerUpgradePanel(Transform holder, PlayerUpgradeType playerUpgradeType)
        {

            PlayerData playerData = m_PlayerUpdateInfo.Data.PlayerData;
            foreach (var playerUpgrade in m_PlayerUpdateInfo.PlayerUpgradesInfo)
            {
                if (playerUpgrade.UpgradeType == playerUpgradeType)
                {
                    PlayerUpgradePanelUI playerUpgradePanelUI = Instantiate(playerUpgrade.PlayerUpgradePanelUIPrefab, holder);
                    playerUpgradePanelUI.Setup(playerUpgrade.UpgradeType);
                    playerUpgradePanelUI.HeaderText.text = playerUpgrade.HeaderText;
                    playerUpgradePanelUI.PreviewImage.sprite = playerUpgrade.BasePreviewSprite;

                    playerUpgradePanelUI.InfoDiscriptionText = playerUpgrade.InfoDiscriptionText;
                    playerUpgradePanelUI.InfoPreviewSprite = playerUpgrade.BasePreviewSprite;

                    playerUpgradePanelUI.NotifiationSeenKey = playerUpgrade.NotifationSeenKey;

                    int currentIndex = 0;
                    if (playerUpgrade.UpgradeType == PlayerUpgradeType.Capacity)
                    {
                        currentIndex = playerData.CurrentCapacityIndex + 1;
                    }
                    else
                    {
                        currentIndex = playerData.CurrentWalkSpeedIndex + 1;
                    }

                    playerUpgradePanelUI.StepFillBar.SetupBars(playerUpgrade.PropertyUpgradesInfo.Count - 1);
                    playerUpgradePanelUI.StepFillBar.SetFillAmount(currentIndex - 1);

                    if (currentIndex < playerUpgrade.PropertyUpgradesInfo.Count)
                    {
                        playerUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                        playerUpgradePanelUI.MaxButton.gameObject.SetActive(false);
                        playerUpgradePanelUI.UpgradeNameText.gameObject.SetActive(true);
                        playerUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);
                        playerUpgradePanelUI.Tick.SetActive(false);

                        playerUpgradePanelUI.UpgradeNameText.text = playerUpgrade.UpgradeNameText;

                        if (playerUpgrade.UpgradeType == PlayerUpgradeType.Capacity
                            && playerData.Capacity.Count > currentIndex)
                        {
                            string valueText = "";
                            if (playerData.CurrentCapacityIndex == 0)
                            {
                                valueText = "-> " + playerData.Capacity[currentIndex].ToString();
                            }
                            else
                            {
                                valueText = playerData.Capacity[playerData.CurrentCapacityIndex] + " -> " + playerData.Capacity[currentIndex];
                            }
                            playerUpgradePanelUI.UpgradeValueText.text = valueText;
                        }
                        else if (playerUpgrade.UpgradeType == PlayerUpgradeType.WalkSpeed
                            && playerData.WalkSpeed.Count > currentIndex)
                        {
                            float baseSpeed = playerData.WalkSpeed[0];
                            float currentSpeed = playerData.WalkSpeed[playerData.CurrentWalkSpeedIndex];
                            float upgradedSpeed = playerData.WalkSpeed[currentIndex];


                            int upgradedDifference = (int)(((upgradedSpeed - currentSpeed) / currentSpeed) * 100f);

                            playerUpgradePanelUI.UpgradeValueText.text = "+" + upgradedDifference + "%";

                        }
                        else
                        {
                            Debug.LogWarning("Player Capacity upgrade does not match with expected upgrades in the DataMapUpdates");
                        }

                        var propertyUpgradeInfo = playerUpgrade.PropertyUpgradesInfo[currentIndex];
                        if (propertyUpgradeInfo.UpdateType == CurrencyType.Coin)
                        {
                            playerUpgradePanelUI.UpgradeButtonText.text = propertyUpgradeInfo.RequiredCoins.ToString();
                            playerUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                            playerUpgradePanelUI.UpgradeCost = propertyUpgradeInfo.RequiredCoins;
                            playerUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                            playerUpgradePanelUI.UpgradeButtonGem.SetActive(false);

                            if (propertyUpgradeInfo.RequiredCoins == 0)
                            {
                                playerUpgradePanelUI.UpgradeButtonText.text = "Free";
                                Vector2 position = playerUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition;
                                position.x = 0;
                                playerUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition = position;
                                playerUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                                playerUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                            }
                        }
                        else if (propertyUpgradeInfo.UpdateType == CurrencyType.Gem)
                        {
                            playerUpgradePanelUI.UpgradeButtonText.text = propertyUpgradeInfo.RequiredGems.ToString();
                            playerUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                            playerUpgradePanelUI.UpgradeCost = propertyUpgradeInfo.RequiredGems;
                            playerUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                            playerUpgradePanelUI.UpgradeButtonGem.SetActive(true);

                            if (propertyUpgradeInfo.RequiredGems == 0)
                            {
                                playerUpgradePanelUI.UpgradeButtonText.text = "Free";
                                Vector2 position = playerUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition;
                                position.x = 0;
                                playerUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition = position;
                                playerUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                                playerUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                            }
                        }


                        if (propertyUpgradeInfo.UsePreviewSprite)
                        {
                            playerUpgradePanelUI.PreviewImage.sprite = propertyUpgradeInfo.PreviewSprite;
                            playerUpgradePanelUI.InfoPreviewSprite = propertyUpgradeInfo.PreviewSprite;
                        }
                    }
                    else
                    {
                        playerUpgradePanelUI.UpgradeButton.gameObject.SetActive(false);
                        playerUpgradePanelUI.MaxButton.gameObject.SetActive(true);
                        playerUpgradePanelUI.UpgradeNameText.gameObject.SetActive(false);
                        playerUpgradePanelUI.UpgradeValueText.gameObject.SetActive(false);
                        playerUpgradePanelUI.Tick.SetActive(true);

                        var propertyUpgradeInfo = playerUpgrade.PropertyUpgradesInfo[^1];
                        if (propertyUpgradeInfo.UsePreviewSprite)
                        {
                            playerUpgradePanelUI.PreviewImage.sprite = propertyUpgradeInfo.PreviewSprite;
                            playerUpgradePanelUI.InfoPreviewSprite = propertyUpgradeInfo.PreviewSprite;
                        }
                    }

                    return playerUpgradePanelUI;
                }
            }

            return null;
        }

        public PlayerUpgradePanelUI UpgradePlayerProperty(PlayerUpgradeType PlayerUpgradeType, PlayerUpgradePanelUI playerUpgradePanelUI)
        {
            PlayerData playerData = m_PlayerUpdateInfo.Data.PlayerData;
            if (PlayerUpgradeType == PlayerUpgradeType.Capacity)
            {
                if (playerData.CurrentCapacityIndex < playerData.Capacity.Count - 1)
                {
                    playerData.CurrentCapacityIndex++;
                    playerData.IsUpgraded = true;
                    m_PlayerUpdateInfo.Data.Save();
                }

                Transform parent = playerUpgradePanelUI.transform.parent;
                int hierarchyIndex = playerUpgradePanelUI.transform.GetSiblingIndex();

                Destroy(playerUpgradePanelUI.gameObject);

                PlayerUpgradePanelUI newPlayerUpgradePanelUI = GetAndSetPlayerUpgradePanel(parent, PlayerUpgradeType.Capacity);

                newPlayerUpgradePanelUI.transform.SetSiblingIndex(hierarchyIndex);

                return newPlayerUpgradePanelUI;
            }
            else if (PlayerUpgradeType == PlayerUpgradeType.WalkSpeed)
            {
                if (playerData.CurrentWalkSpeedIndex < playerData.WalkSpeed.Count - 1)
                {
                    playerData.CurrentWalkSpeedIndex++;
                    playerData.IsUpgraded = true;
                    m_PlayerUpdateInfo.Data.Save();
                }

                Transform parent = playerUpgradePanelUI.transform.parent;
                int hierarchyIndex = playerUpgradePanelUI.transform.GetSiblingIndex();

                Destroy(playerUpgradePanelUI.gameObject);

                PlayerUpgradePanelUI newPlayerUpgradePanelUI = GetAndSetPlayerUpgradePanel(parent, PlayerUpgradeType.WalkSpeed);
                newPlayerUpgradePanelUI.transform.SetSiblingIndex(hierarchyIndex);

                return newPlayerUpgradePanelUI;
            }


            return playerUpgradePanelUI;
        }
        #endregion

        #region Booster Related
        public List<BoosterInfo> GetBoostersInfo()
        {
            return m_BoostersInfo;
        }
        #endregion

        #region Chair Related
        public ChairCapacityUpgradePanelUI GetAndSetChairCapacityUpgradePanel(Transform holder, ChairUpgradeType chairUpgradeType)
        {
            foreach (var chairUpdateInfotem in m_ChairsUpdateInfo)
            {
                if(chairUpdateInfotem.ChairType == chairUpgradeType)
                {
                    int level = DataManager.CurrentMapLevelIndex + 1;
                    if(chairUpdateInfotem.ShowAfterAndAtLevel > level)
                    {
                        return null;
                    }

                    ChairCapacityUpgradePanelUI chairCapacityUpgradePanelUI = Instantiate(chairUpdateInfotem.ChairCapacityUpgradePanelUIPrefab, holder);
                    chairCapacityUpgradePanelUI.Setup();
                    chairCapacityUpgradePanelUI.ChairUpgradeType = chairUpdateInfotem.ChairType;
                    chairCapacityUpgradePanelUI.HeaderText.text = chairUpdateInfotem.HeaderText;
                    chairCapacityUpgradePanelUI.PreviewImage.sprite = chairUpdateInfotem.BasePreviewSprite;
                    chairCapacityUpgradePanelUI.UpgradeNameText.text = chairUpdateInfotem.UpgradeNameText;

                    chairCapacityUpgradePanelUI.InfoDiscriptionText = chairUpdateInfotem.InfoDiscriptionText;
                    chairCapacityUpgradePanelUI.InfoPreviewSprite = chairUpdateInfotem.BasePreviewSprite;

                    chairCapacityUpgradePanelUI.NotifiationSeenKey = chairUpdateInfotem.NotifationSeenKey;

                    chairCapacityUpgradePanelUI.UpgradeNameText.gameObject.SetActive(false);
                    chairCapacityUpgradePanelUI.UpgradeValueText.gameObject.SetActive(false);
                    chairCapacityUpgradePanelUI.UpgradeButton.gameObject.SetActive(false);
                    chairCapacityUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                    chairCapacityUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                    chairCapacityUpgradePanelUI.MaxButton.gameObject.SetActive(false);
                    chairCapacityUpgradePanelUI.Tick.SetActive(false);


                    int alreadyUnocked = 0;
                    foreach (var chairCapacityUpgrade in chairUpdateInfotem.ChairsCapacityUpgrade)
                    {
                        if (chairCapacityUpgrade.IsAlreadyUnlocked == false)
                        {
                            break;
                        }
                        alreadyUnocked++;
                    }

                    int currentUnlockedIndex = 0;
                    foreach (var chairCapacityUpgrade in chairUpdateInfotem.ChairsCapacityUpgrade)
                    {
                        if(chairUpdateInfotem.ChairType == ChairUpgradeType.Salon)
                        {
                            DataSalonChair dataSalonChair = (DataSalonChair)chairCapacityUpgrade.ChairData;
                            if (dataSalonChair.SalonChairData.IsUnlocked == false)
                            {
                                break;
                            }
                        }
                        else if(chairUpdateInfotem.ChairType == ChairUpgradeType.Cafe)
                        {
                            DataCafeChair dataCafeChair = (DataCafeChair)chairCapacityUpgrade.ChairData;
                            if (dataCafeChair.CafeChairData.IsUnlocked == false)
                            {
                                break;
                            }
                        }
                        currentUnlockedIndex++;
                    }

                    chairCapacityUpgradePanelUI.StepFillBar.SetupBars(chairUpdateInfotem.ChairsCapacityUpgrade.Count - alreadyUnocked);


                    if (currentUnlockedIndex <= chairUpdateInfotem.ChairsCapacityUpgrade.Count - 1)
                    {
                        chairCapacityUpgradePanelUI.StepFillBar.SetFillAmount(currentUnlockedIndex - alreadyUnocked);
                        chairCapacityUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                        chairCapacityUpgradePanelUI.UpgradeNameText.gameObject.SetActive(true);

                        ChairCapacityUpgrade chairCapacityUpgrade = chairUpdateInfotem.ChairsCapacityUpgrade[currentUnlockedIndex];

                        if (chairCapacityUpgrade.UpdateType == CurrencyType.Coin)
                        {
                            chairCapacityUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                            chairCapacityUpgradePanelUI.UpgradeButtonText.text = chairCapacityUpgrade.RequiredCoins.ToString();
                            chairCapacityUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                            chairCapacityUpgradePanelUI.UpgradeCost = chairCapacityUpgrade.RequiredCoins;

                        }
                        else if (chairCapacityUpgrade.UpdateType == CurrencyType.Gem)
                        {
                            chairCapacityUpgradePanelUI.UpgradeButtonGem.SetActive(true);
                            chairCapacityUpgradePanelUI.UpgradeButtonText.text = chairCapacityUpgrade.RequiredGems.ToString();
                            chairCapacityUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                            chairCapacityUpgradePanelUI.UpgradeCost = chairCapacityUpgrade.RequiredGems;
                        }

                        chairCapacityUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);
                        if (currentUnlockedIndex == 0)
                        {
                            chairCapacityUpgradePanelUI.UpgradeValueText.text = "-> 1";
                        }
                        else
                        {
                            chairCapacityUpgradePanelUI.UpgradeValueText.text = currentUnlockedIndex + " -> " + (currentUnlockedIndex + 1);
                        }
                    }
                    else
                    {
                        chairCapacityUpgradePanelUI.StepFillBar.SetFillAmount(chairUpdateInfotem.ChairsCapacityUpgrade.Count - alreadyUnocked);

                        chairCapacityUpgradePanelUI.Tick.SetActive(true);
                        chairCapacityUpgradePanelUI.MaxButton.gameObject.SetActive(true);
                    }
                     return chairCapacityUpgradePanelUI;

                }
            }

            return null;

            // ChairCapacityUpgradePanelUI chairCapacityUpgradePanelUI = Instantiate(m_ChairUpdateInfo.ChairCapacityUpgradePanelUIPrefab, holder);
            // chairCapacityUpgradePanelUI.Setup();
            // chairCapacityUpgradePanelUI.HeaderText.text = m_ChairUpdateInfo.HeaderText;
            // chairCapacityUpgradePanelUI.PreviewImage.sprite = m_ChairUpdateInfo.BasePreviewSprite;
            // chairCapacityUpgradePanelUI.UpgradeNameText.text = m_ChairUpdateInfo.UpgradeNameText;

            // chairCapacityUpgradePanelUI.InfoDiscriptionText = m_ChairUpdateInfo.InfoDiscriptionText;
            // chairCapacityUpgradePanelUI.InfoPreviewSprite = m_ChairUpdateInfo.BasePreviewSprite;

            // chairCapacityUpgradePanelUI.NotifiationSeenKey = m_ChairUpdateInfo.NotifationSeenKey;

            // chairCapacityUpgradePanelUI.UpgradeNameText.gameObject.SetActive(false);
            // chairCapacityUpgradePanelUI.UpgradeValueText.gameObject.SetActive(false);
            // chairCapacityUpgradePanelUI.UpgradeButton.gameObject.SetActive(false);
            // chairCapacityUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
            // chairCapacityUpgradePanelUI.UpgradeButtonGem.SetActive(false);
            // chairCapacityUpgradePanelUI.MaxButton.gameObject.SetActive(false);
            // chairCapacityUpgradePanelUI.Tick.SetActive(false);


            // int alreadyUnocked = 0;
            // foreach (var chairCapacityUpgrade in m_ChairUpdateInfo.ChairsCapacityUpgrade)
            // {
            //     if (chairCapacityUpgrade.IsAlreadyUnlocked == false)
            //     {
            //         break;
            //     }
            //     alreadyUnocked++;
            // }

            // int currentUnlockedIndex = 0;
            // foreach (var chairCapacityUpgrade in m_ChairUpdateInfo.ChairsCapacityUpgrade)
            // {
            //     if (chairCapacityUpgrade.SalonChairData.SalonChairData.IsUnlocked == false)
            //     {
            //         break;
            //     }
            //     currentUnlockedIndex++;
            // }

            // chairCapacityUpgradePanelUI.StepFillBar.SetupBars(m_ChairUpdateInfo.ChairsCapacityUpgrade.Count - alreadyUnocked);


            // if (currentUnlockedIndex <= m_ChairUpdateInfo.ChairsCapacityUpgrade.Count - 1)
            // {
            //     chairCapacityUpgradePanelUI.StepFillBar.SetFillAmount(currentUnlockedIndex - alreadyUnocked);
            //     chairCapacityUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
            //     chairCapacityUpgradePanelUI.UpgradeNameText.gameObject.SetActive(true);

            //     ChairCapacityUpgrade chairCapacityUpgrade = m_ChairUpdateInfo.ChairsCapacityUpgrade[currentUnlockedIndex];

            //     if (chairCapacityUpgrade.UpdateType == CurrencyType.Coin)
            //     {
            //         chairCapacityUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
            //         chairCapacityUpgradePanelUI.UpgradeButtonText.text = chairCapacityUpgrade.RequiredCoins.ToString();
            //         chairCapacityUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
            //         chairCapacityUpgradePanelUI.UpgradeCost = chairCapacityUpgrade.RequiredCoins;

            //     }
            //     else if (chairCapacityUpgrade.UpdateType == CurrencyType.Gem)
            //     {
            //         chairCapacityUpgradePanelUI.UpgradeButtonGem.SetActive(true);
            //         chairCapacityUpgradePanelUI.UpgradeButtonText.text = chairCapacityUpgrade.RequiredGems.ToString();
            //         chairCapacityUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
            //         chairCapacityUpgradePanelUI.UpgradeCost = chairCapacityUpgrade.RequiredGems;
            //     }

            //     chairCapacityUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);
            //     if (currentUnlockedIndex == 0)
            //     {
            //         chairCapacityUpgradePanelUI.UpgradeValueText.text = "-> 1";
            //     }
            //     else
            //     {
            //         chairCapacityUpgradePanelUI.UpgradeValueText.text = currentUnlockedIndex + " -> " + (currentUnlockedIndex + 1);
            //     }
            // }
            // else
            // {
            //     chairCapacityUpgradePanelUI.StepFillBar.SetFillAmount(m_ChairUpdateInfo.ChairsCapacityUpgrade.Count - alreadyUnocked);

            //     chairCapacityUpgradePanelUI.Tick.SetActive(true);
            //     chairCapacityUpgradePanelUI.MaxButton.gameObject.SetActive(true);

            // }

            //return chairCapacityUpgradePanelUI;
        }

        public ChairCapacityUpgradePanelUI UpgradeChairCapacityProperty(ChairCapacityUpgradePanelUI chairCapacityUpgradePanelUI)
        {
            ChairUpgradeType chairUpgradeType = chairCapacityUpgradePanelUI.ChairUpgradeType;

            ChairUpdateInfo chairUpdateInfo = m_ChairsUpdateInfo.Find((x) => x.ChairType == chairUpgradeType);

            foreach (var chairCapacityUpgrade in chairUpdateInfo.ChairsCapacityUpgrade)
            {
                if(chairUpdateInfo.ChairType == ChairUpgradeType.Salon)
                {
                    DataSalonChair dataSalonChair = (DataSalonChair)chairCapacityUpgrade.ChairData;
                    if (chairCapacityUpgrade.IsAlreadyUnlocked == false
                        && dataSalonChair.SalonChairData.IsUnlocked == false)
                    {
                        dataSalonChair.SalonChairData.IsUnlocked = true;
                        dataSalonChair.SalonChairData.HasJustUnlocked = true;
                        dataSalonChair.Save();
                        break;
                    }
                }
                else if(chairUpdateInfo.ChairType == ChairUpgradeType.Cafe)
                {
                    DataCafeChair dataCafeChair = (DataCafeChair)chairCapacityUpgrade.ChairData;
                    if (chairCapacityUpgrade.IsAlreadyUnlocked == false
                        && dataCafeChair.CafeChairData.IsUnlocked == false)
                    {
                        dataCafeChair.CafeChairData.IsUnlocked = true;
                        dataCafeChair.CafeChairData.HasJustUnlocked = true;
                        dataCafeChair.Save();
                        break;
                    }
                }
            }

            Transform parent = chairCapacityUpgradePanelUI.transform.parent;
            int hierarchyIndex = chairCapacityUpgradePanelUI.transform.GetSiblingIndex();

            Destroy(chairCapacityUpgradePanelUI.gameObject);

            ChairCapacityUpgradePanelUI newChairCapacityUpgradePanelUI = GetAndSetChairCapacityUpgradePanel(parent, chairUpgradeType);

            newChairCapacityUpgradePanelUI.transform.SetSiblingIndex(hierarchyIndex);

            return newChairCapacityUpgradePanelUI;
        }
        #endregion

        #region Worker Related

        /// <summary>
        /// Returns the list of new unlockable WorkerOrder (WorkerSprite, WorkerOrderSymbolSprite, WorkerTypeSprite, onUnlocked callback)
        /// Returns empty list if non found
        /// </summary>
        public List<Tuple<Sprite, Sprite, Sprite, Action>> GetNewWorkerAndWorkerOrderUnlockable()
        {
            List<Tuple<Sprite, Sprite, Sprite, Action>> unlockedNewWorkersandWorkerOrders = new List<Tuple<Sprite, Sprite, Sprite, Action>>();

            int currentLevel = DataManager.CurrentMapLevelIndex + 1;

            foreach (var workerUnlockingInfo in m_WorkerUpdateInfo.WorkersUnlockingInfo)
            {
                if (workerUnlockingInfo.UnlockAtLevel <= currentLevel)
                {
                    if (workerUnlockingInfo.Data.WorkerOrderData.IsUnlocked == false)
                    {
                        Action onUnlocked = () =>
                        {
                            workerUnlockingInfo.Data.WorkerOrderData.IsUnlocked = true;
                            workerUnlockingInfo.Data.WorkerOrderData.HasJustUnlocked = true;
                        };

                        Tuple<Sprite, Sprite, Sprite, Action> unlockedNewWorkerandWorkerOrder = new Tuple<Sprite, Sprite, Sprite, Action>
                        (
                            workerUnlockingInfo.Worker1Sprite,
                            workerUnlockingInfo.WorkerOrderSymbolSprite,
                            workerUnlockingInfo.WorkerTypeSprite,
                            onUnlocked
                        );


                        unlockedNewWorkersandWorkerOrders.Add(unlockedNewWorkerandWorkerOrder);
                    }
                }
            }

            return unlockedNewWorkersandWorkerOrders;
        }

        public Tuple<bool, List<WorkerQuantityUpgradeUIPanel>> GetWorkerQuantityUpgrade(WorkerUIManager workerUIManager, Transform holder, WorkerQuantityUpgradeUIPanel workerCapacityUpgradeUIPanelPrefab)
        {
            int level = DataManager.CurrentMapLevelIndex + 1;

            bool shouldShow = false;
            string key = "";
            foreach (var workerCapacityUpgradeInfo in m_WorkerUpdateInfo.WorkersUpgradeQuantityInfo)
            {
                if (workerCapacityUpgradeInfo.ShowAtLevel <= level
                   && DataManager.GetBool(workerCapacityUpgradeInfo.Key, false) == false)
                {
                    shouldShow = true;
                    key = workerCapacityUpgradeInfo.Key;
                    break;
                }
            }

            if (shouldShow)
            {
                List<WorkerQuantityUpgradeUIPanel> workersQuantityUpgradeUIPanel = new List<WorkerQuantityUpgradeUIPanel>();

                foreach (var workerUnlockingInfo in m_WorkerUpdateInfo.WorkersUnlockingInfo)
                {
                    WorkerOrderData workerOrderData = workerUnlockingInfo.Data.WorkerOrderData;
                    if (workerOrderData.IsUnlocked
                        && workerOrderData.WorkerQunaityUpgradeIndex < workerOrderData.WorkerQunaityUpgrades.Count - 1)
                    {
                        WorkerQuantityUpgradeUIPanel newworkerCapacityUpgradeUIPanel = Instantiate(workerCapacityUpgradeUIPanelPrefab, holder);

                        Action onUpgradeCallback = () =>
                        {
                            workerOrderData.WorkerQunaityUpgradeIndex++;
                            workerOrderData.HasJustUpgraded = true;
                            workerUnlockingInfo.Data.Save();
                            DataManager.SetBool(key, true);
                        };

                        newworkerCapacityUpgradeUIPanel.Setup(workerUIManager, 
                            workerUnlockingInfo.WorkerTypeSprite, 
                            workerUnlockingInfo.WorkerOrderSymbolSpriteForUpgrade, 
                            workerUnlockingInfo.Worker2Sprite, 
                            onUpgradeCallback);

                        workersQuantityUpgradeUIPanel.Add(newworkerCapacityUpgradeUIPanel);
                    }
                }

                if (workersQuantityUpgradeUIPanel.Count > 0)
                {
                    return new Tuple<bool, List<WorkerQuantityUpgradeUIPanel>>(true, workersQuantityUpgradeUIPanel);
                }
                else
                {
                    return new Tuple<bool, List<WorkerQuantityUpgradeUIPanel>>(false, null);
                }
            }
            else
            {
                return new Tuple<bool, List<WorkerQuantityUpgradeUIPanel>>(false, null);
            }
        }

        public List<WorkerUpgradePanelUI> GetAndSetWorkerUpgradePanels(Transform holder)
        {
            List<WorkerUpgradePanelUI> workerUpgradePanelsUI = new List<WorkerUpgradePanelUI>();
            foreach (var workerUnlcokingInfo in m_WorkerUpdateInfo.WorkersUnlockingInfo)
            {
                WorkerOrderData workerOrderData = workerUnlcokingInfo.Data.WorkerOrderData;
                if (workerOrderData.IsUnlocked)
                {
                    WorkerUpgradeServingTimeInfo upgradeServingTimeInfo = workerUnlcokingInfo.UpgradeServingTimeInfo;
                    WorkerUpgradePanelUI workerUpgradePanelUI = Instantiate(upgradeServingTimeInfo.WorkerUpgradePanelUIPrefab, holder);
                    workerUpgradePanelsUI.Add(workerUpgradePanelUI);
                    workerUpgradePanelUI.Setup(workerUnlcokingInfo.Data);

                    workerUpgradePanelUI.HeaderText.text = upgradeServingTimeInfo.HeaderText;
                    workerUpgradePanelUI.PanelImage.sprite = upgradeServingTimeInfo.PanelSprite;

                    workerUpgradePanelUI.PreviewImage1.sprite = upgradeServingTimeInfo.PreviewSprite1;
                    workerUpgradePanelUI.PreviewImage2.sprite = upgradeServingTimeInfo.PreviewSprite2;
                    workerUpgradePanelUI.PreviewImage2.gameObject.SetActive(false);

                    workerUpgradePanelUI.InfoPreviewSprite = upgradeServingTimeInfo.PreviewSprite1;
                    workerUpgradePanelUI.InfoDiscriptionText = upgradeServingTimeInfo.InfoDiscriptionText;

                    workerUpgradePanelUI.NotifiationSeenKey = upgradeServingTimeInfo.NotifationSeenKey;

                    if (workerOrderData.WorkerQunaityUpgradeIndex > 0)
                    {
                        workerUpgradePanelUI.PreviewImage2.gameObject.SetActive(true);
                    }

                    //workerUpgradePanelUI.LevelImage.sprite = upgradeServingTimeInfo.LevelSprite;
                    workerUpgradePanelUI.LevelText.text = workerOrderData.WorkerCurrentLevelDifficulty.ToString();

                    workerUpgradePanelUI.WorkerOrderSymbolImage.sprite = upgradeServingTimeInfo.WorkerOrderSymbolSprite;
                    workerUpgradePanelUI.UpgradeButtonImage.sprite = upgradeServingTimeInfo.UpgradeButtonSprite;

                    workerUpgradePanelUI.UpgradeFromText.gameObject.SetActive(false);
                    workerUpgradePanelUI.UpgradeToText.gameObject.SetActive(false);
                    workerUpgradePanelUI.UpgradeText.gameObject.SetActive(false);
                    workerUpgradePanelUI.UpgradeArrow.gameObject.SetActive(false);
                    workerUpgradePanelUI.UpgradeButton.gameObject.SetActive(false);
                    workerUpgradePanelUI.UpgradeButtonCoin.gameObject.SetActive(false);
                    workerUpgradePanelUI.UpgradeButtonGem.gameObject.SetActive(false);
                    workerUpgradePanelUI.MaxButton.gameObject.SetActive(false);

                    int currentMapLevelDifficulty = DataManager.CurrentMapLevelDifficulty;

                    WorkerServingDurationUpgrade workerServing = workerOrderData.WorkerServingDurationUpgrades[^1];
                    foreach (var workerServingDurationUpgrade in workerOrderData.WorkerServingDurationUpgrades)
                    {
                        if (workerServingDurationUpgrade.LevelDifficultyRangeMin <= currentMapLevelDifficulty
                            && workerServingDurationUpgrade.LevelDifficultyRangeMax >= currentMapLevelDifficulty)
                        {
                            workerServing = workerServingDurationUpgrade;
                            break;
                        }
                    }

                    WorkerUpgradeServingTimeRangeInfo workerServingRangeInfo = upgradeServingTimeInfo.UpgradeServingTimeRangesInfo[^1];
                    foreach (var upgradeServingTimeRangeInfo in upgradeServingTimeInfo.UpgradeServingTimeRangesInfo)
                    {
                        if (upgradeServingTimeRangeInfo.LevelDifficultyRangeMin <= currentMapLevelDifficulty
                            && upgradeServingTimeRangeInfo.LevelDifficultyRangeMax >= currentMapLevelDifficulty)
                        {
                            workerServingRangeInfo = upgradeServingTimeRangeInfo;
                            break;
                        }
                    }

                    int diffiernceInDifficulty = currentMapLevelDifficulty - workerOrderData.WorkerCurrentLevelDifficulty;
                    if (diffiernceInDifficulty == 0)
                    {
                        var servingDuration = workerServing.LevelDifficultyServingDurations[0];
                        workerUpgradePanelUI.UpgradeText.gameObject.SetActive(true);
                        workerUpgradePanelUI.MaxButton.gameObject.SetActive(true);
                        workerUpgradePanelUI.UpgradeText.text = servingDuration.WorkerServingDuration.ToString() + "s";
                    }
                    else if (diffiernceInDifficulty < workerServing.LevelDifficultyServingDurations.Count)
                    {
                        var servingDurationPrevious = workerServing.LevelDifficultyServingDurations[diffiernceInDifficulty - 1];
                        var servingDurationCurrent = workerServing.LevelDifficultyServingDurations[diffiernceInDifficulty];


                        workerUpgradePanelUI.UpgradeFromText.text = servingDurationCurrent.WorkerServingDuration + "s";
                        workerUpgradePanelUI.UpgradeToText.text = servingDurationPrevious.WorkerServingDuration + "s";

                        workerUpgradePanelUI.UpgradeFromText.gameObject.SetActive(true);
                        workerUpgradePanelUI.UpgradeToText.gameObject.SetActive(true);
                        workerUpgradePanelUI.UpgradeArrow.gameObject.SetActive(true);

                        WorkerUpgradeServingTimePrice UpgradeServingTimePrice = workerServingRangeInfo.UpgradeServingTimePrices[diffiernceInDifficulty - 1];

                        if (UpgradeServingTimePrice.UpdateType == CurrencyType.Coin)
                        {
                            workerUpgradePanelUI.UpgradeButtonText.text = UpgradeServingTimePrice.RequiredCoins.ToString();
                            workerUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                            workerUpgradePanelUI.UpgradeCost = UpgradeServingTimePrice.RequiredCoins;
                            workerUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                            workerUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                        }
                        else if (UpgradeServingTimePrice.UpdateType == CurrencyType.Gem)
                        {
                            workerUpgradePanelUI.UpgradeButtonText.text = UpgradeServingTimePrice.RequiredGems.ToString();
                            workerUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                            workerUpgradePanelUI.UpgradeCost = UpgradeServingTimePrice.RequiredGems;
                            workerUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                            workerUpgradePanelUI.UpgradeButtonGem.SetActive(true);
                        }
                    }
                    else
                    {
                        int maxDifficultyCount = workerServing.LevelDifficultyServingDurations.Count - 1;
                        int previousDifficultyCount = maxDifficultyCount - 1;
                        var servingDurationPrevious = workerServing.LevelDifficultyServingDurations[previousDifficultyCount];
                        var servingDurationCurrent = workerServing.LevelDifficultyServingDurations[maxDifficultyCount];


                        workerUpgradePanelUI.UpgradeFromText.text = servingDurationCurrent.WorkerServingDuration + "s";
                        workerUpgradePanelUI.UpgradeToText.text = servingDurationPrevious.WorkerServingDuration + "s";

                        workerUpgradePanelUI.UpgradeFromText.gameObject.SetActive(true);
                        workerUpgradePanelUI.UpgradeToText.gameObject.SetActive(true);
                        workerUpgradePanelUI.UpgradeArrow.gameObject.SetActive(true);

                        WorkerUpgradeServingTimePrice UpgradeServingTimePrice = workerServingRangeInfo.UpgradeServingTimePrices[previousDifficultyCount];

                        if (UpgradeServingTimePrice.UpdateType == CurrencyType.Coin)
                        {
                            workerUpgradePanelUI.UpgradeButtonText.text = UpgradeServingTimePrice.RequiredCoins.ToString();
                            workerUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                            workerUpgradePanelUI.UpgradeCost = UpgradeServingTimePrice.RequiredCoins;
                            workerUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                            workerUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                        }
                        else if (UpgradeServingTimePrice.UpdateType == CurrencyType.Gem)
                        {
                            workerUpgradePanelUI.UpgradeButtonText.text = UpgradeServingTimePrice.RequiredGems.ToString();
                            workerUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                            workerUpgradePanelUI.UpgradeCost = UpgradeServingTimePrice.RequiredGems;
                            workerUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                            workerUpgradePanelUI.UpgradeButtonGem.SetActive(true);
                        }
                    }
                }
            }

            return workerUpgradePanelsUI;
        }

        public WorkerUpgradePanelUI GetAndSetWorkerUpgradePanel(Transform holder, DataWorker dataWorker)
        {
            foreach (var workerUnlcokingInfo in m_WorkerUpdateInfo.WorkersUnlockingInfo)
            {
                if (workerUnlcokingInfo.Data == dataWorker)
                {
                    WorkerOrderData workerOrderData = workerUnlcokingInfo.Data.WorkerOrderData;
                    if (workerOrderData.IsUnlocked)
                    {
                        WorkerUpgradeServingTimeInfo upgradeServingTimeInfo = workerUnlcokingInfo.UpgradeServingTimeInfo;
                        WorkerUpgradePanelUI workerUpgradePanelUI = Instantiate(upgradeServingTimeInfo.WorkerUpgradePanelUIPrefab, holder);
                        workerUpgradePanelUI.Setup(workerUnlcokingInfo.Data);

                        workerUpgradePanelUI.HeaderText.text = upgradeServingTimeInfo.HeaderText;
                        workerUpgradePanelUI.PanelImage.sprite = upgradeServingTimeInfo.PanelSprite;

                        workerUpgradePanelUI.PreviewImage1.sprite = upgradeServingTimeInfo.PreviewSprite1;
                        workerUpgradePanelUI.PreviewImage2.sprite = upgradeServingTimeInfo.PreviewSprite2;
                        workerUpgradePanelUI.PreviewImage2.gameObject.SetActive(false);

                        workerUpgradePanelUI.InfoPreviewSprite = upgradeServingTimeInfo.PreviewSprite1;
                        workerUpgradePanelUI.InfoDiscriptionText = upgradeServingTimeInfo.InfoDiscriptionText;

                        workerUpgradePanelUI.NotifiationSeenKey = upgradeServingTimeInfo.NotifationSeenKey;

                        if (workerOrderData.WorkerQunaityUpgradeIndex > 0)
                        {
                            workerUpgradePanelUI.PreviewImage2.gameObject.SetActive(true);
                        }

                        //workerUpgradePanelUI.LevelImage.sprite = upgradeServingTimeInfo.LevelSprite;
                        workerUpgradePanelUI.LevelText.text = workerOrderData.WorkerCurrentLevelDifficulty.ToString();

                        workerUpgradePanelUI.WorkerOrderSymbolImage.sprite = upgradeServingTimeInfo.WorkerOrderSymbolSprite;
                        workerUpgradePanelUI.UpgradeButtonImage.sprite = upgradeServingTimeInfo.UpgradeButtonSprite;

                        workerUpgradePanelUI.UpgradeFromText.gameObject.SetActive(false);
                        workerUpgradePanelUI.UpgradeToText.gameObject.SetActive(false);
                        workerUpgradePanelUI.UpgradeText.gameObject.SetActive(false);
                        workerUpgradePanelUI.UpgradeArrow.gameObject.SetActive(false);
                        workerUpgradePanelUI.UpgradeButton.gameObject.SetActive(false);
                        workerUpgradePanelUI.UpgradeButtonCoin.gameObject.SetActive(false);
                        workerUpgradePanelUI.UpgradeButtonGem.gameObject.SetActive(false);
                        workerUpgradePanelUI.MaxButton.gameObject.SetActive(false);

                        int currentMapLevelDifficulty = DataManager.CurrentMapLevelDifficulty;

                        WorkerServingDurationUpgrade workerServing = workerOrderData.WorkerServingDurationUpgrades[^1];
                        foreach (var workerServingDurationUpgrade in workerOrderData.WorkerServingDurationUpgrades)
                        {
                            if (workerServingDurationUpgrade.LevelDifficultyRangeMin <= currentMapLevelDifficulty
                                && workerServingDurationUpgrade.LevelDifficultyRangeMax >= currentMapLevelDifficulty)
                            {
                                workerServing = workerServingDurationUpgrade;
                                break;
                            }
                        }

                        WorkerUpgradeServingTimeRangeInfo workerServingRangeInfo = upgradeServingTimeInfo.UpgradeServingTimeRangesInfo[^1];
                        foreach (var upgradeServingTimeRangeInfo in upgradeServingTimeInfo.UpgradeServingTimeRangesInfo)
                        {
                            if (upgradeServingTimeRangeInfo.LevelDifficultyRangeMin <= currentMapLevelDifficulty
                                && upgradeServingTimeRangeInfo.LevelDifficultyRangeMax >= currentMapLevelDifficulty)
                            {
                                workerServingRangeInfo = upgradeServingTimeRangeInfo;
                                break;
                            }
                        }

                        int diffiernceInDifficulty = currentMapLevelDifficulty - workerOrderData.WorkerCurrentLevelDifficulty;
                        if (diffiernceInDifficulty == 0)
                        {
                            var servingDuration = workerServing.LevelDifficultyServingDurations[0];
                            workerUpgradePanelUI.UpgradeText.gameObject.SetActive(true);
                            workerUpgradePanelUI.MaxButton.gameObject.SetActive(true);
                            workerUpgradePanelUI.UpgradeText.text = servingDuration.WorkerServingDuration.ToString() + "s";
                        }
                        else if (diffiernceInDifficulty < workerServing.LevelDifficultyServingDurations.Count)
                        {
                            var servingDurationPrevious = workerServing.LevelDifficultyServingDurations[diffiernceInDifficulty - 1];
                            var servingDurationCurrent = workerServing.LevelDifficultyServingDurations[diffiernceInDifficulty];


                            workerUpgradePanelUI.UpgradeFromText.text = servingDurationCurrent.WorkerServingDuration + "s";
                            workerUpgradePanelUI.UpgradeToText.text = servingDurationPrevious.WorkerServingDuration + "s";

                            workerUpgradePanelUI.UpgradeFromText.gameObject.SetActive(true);
                            workerUpgradePanelUI.UpgradeToText.gameObject.SetActive(true);
                            workerUpgradePanelUI.UpgradeArrow.gameObject.SetActive(true);

                            WorkerUpgradeServingTimePrice UpgradeServingTimePrice = workerServingRangeInfo.UpgradeServingTimePrices[diffiernceInDifficulty - 1];

                            if (UpgradeServingTimePrice.UpdateType == CurrencyType.Coin)
                            {
                                workerUpgradePanelUI.UpgradeButtonText.text = UpgradeServingTimePrice.RequiredCoins.ToString();
                                workerUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                                workerUpgradePanelUI.UpgradeCost = UpgradeServingTimePrice.RequiredCoins;
                                workerUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                                workerUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                            }
                            else if (UpgradeServingTimePrice.UpdateType == CurrencyType.Gem)
                            {
                                workerUpgradePanelUI.UpgradeButtonText.text = UpgradeServingTimePrice.RequiredGems.ToString();
                                workerUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                                workerUpgradePanelUI.UpgradeCost = UpgradeServingTimePrice.RequiredGems;
                                workerUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                                workerUpgradePanelUI.UpgradeButtonGem.SetActive(true);
                            }
                        }
                        else
                        {
                            int maxDifficultyCount = workerServing.LevelDifficultyServingDurations.Count - 1;
                            int previousDifficultyCount = maxDifficultyCount - 1;
                            var servingDurationPrevious = workerServing.LevelDifficultyServingDurations[previousDifficultyCount];
                            var servingDurationCurrent = workerServing.LevelDifficultyServingDurations[maxDifficultyCount];


                            workerUpgradePanelUI.UpgradeFromText.text = servingDurationCurrent.WorkerServingDuration + "s";
                            workerUpgradePanelUI.UpgradeToText.text = servingDurationPrevious.WorkerServingDuration + "s";

                            workerUpgradePanelUI.UpgradeFromText.gameObject.SetActive(true);
                            workerUpgradePanelUI.UpgradeToText.gameObject.SetActive(true);
                            workerUpgradePanelUI.UpgradeArrow.gameObject.SetActive(true);

                            WorkerUpgradeServingTimePrice UpgradeServingTimePrice = workerServingRangeInfo.UpgradeServingTimePrices[previousDifficultyCount];

                            if (UpgradeServingTimePrice.UpdateType == CurrencyType.Coin)
                            {
                                workerUpgradePanelUI.UpgradeButtonText.text = UpgradeServingTimePrice.RequiredCoins.ToString();
                                workerUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                                workerUpgradePanelUI.UpgradeCost = UpgradeServingTimePrice.RequiredCoins;
                                workerUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                                workerUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                            }
                            else if (UpgradeServingTimePrice.UpdateType == CurrencyType.Gem)
                            {
                                workerUpgradePanelUI.UpgradeButtonText.text = UpgradeServingTimePrice.RequiredGems.ToString();
                                workerUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                                workerUpgradePanelUI.UpgradeCost = UpgradeServingTimePrice.RequiredGems;
                                workerUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                                workerUpgradePanelUI.UpgradeButtonGem.SetActive(true);
                            }
                        }

                        return workerUpgradePanelUI;
                    }
                }
            }

            return null;
        }

        public WorkerUpgradePanelUI UpgradeWorkerServingDurationProperty(DataWorker dataWorker, WorkerUpgradePanelUI workerUpgradePanelUI)
        {
            foreach (var workerUnlockingInfo in m_WorkerUpdateInfo.WorkersUnlockingInfo)
            {
                if (workerUnlockingInfo.Data == dataWorker)
                {
                    workerUnlockingInfo.Data.WorkerOrderData.WorkerCurrentLevelDifficulty++;
                    workerUnlockingInfo.Data.Save();

                    Transform parent = workerUpgradePanelUI.transform.parent;
                    int hierarchyIndex = workerUpgradePanelUI.transform.GetSiblingIndex();

                    Destroy(workerUpgradePanelUI.gameObject);

                    WorkerUpgradePanelUI newWorkerUpgradePanelUI = GetAndSetWorkerUpgradePanel(parent, dataWorker);
                    newWorkerUpgradePanelUI.transform.SetSiblingIndex(hierarchyIndex);

                    return newWorkerUpgradePanelUI;
                }
            }

            return workerUpgradePanelUI;
        }
        #endregion

        #region Station Upgrade Related
        public List<StationUpgradePanelUI> GetAndSetStationUpgradePanels(Transform holder, StationUpgradeType stationUpgradeType)
        {
            List<StationUpgradePanelUI> stationUpgradePanelsUI = new List<StationUpgradePanelUI>();

            foreach (var stationUpdateInfo in m_StationsUpdateInfo)
            {
                StationData data = stationUpdateInfo.Data.StationData;
                if (data.IsUnlocked)
                {
                    foreach (var stationUpgradeInfo in stationUpdateInfo.StationUpgradeInfo)
                    {
                        if (stationUpgradeInfo.UpgradeType == stationUpgradeType)
                        {
                            StationUpgradePanelUI stationUpgradePanelUI = Instantiate(stationUpgradeInfo.StationUpgradePanelUIPrefab, holder);
                            stationUpgradePanelUI.Setup(stationUpdateInfo.Data, stationUpgradeInfo.UpgradeType);
                            stationUpgradePanelsUI.Add(stationUpgradePanelUI);

                            stationUpgradePanelUI.HeaderText.text = stationUpgradeInfo.HeaderText;
                            stationUpgradePanelUI.PreviewImage.sprite = stationUpgradeInfo.BasePreviewSprite;

                            stationUpgradePanelUI.InfoPreviewSprite = stationUpgradeInfo.BasePreviewSprite;
                            stationUpgradePanelUI.InfoDiscriptionText = stationUpgradeInfo.InfoDiscriptionText;

                            stationUpgradePanelUI.NotifiationSeenKey = stationUpgradeInfo.NotifationSeenKey;


                            stationUpgradePanelUI.UpgradeNameText.gameObject.SetActive(false);
                            stationUpgradePanelUI.UpgradeValueText.gameObject.SetActive(false);
                            stationUpgradePanelUI.UpgradeButton.gameObject.SetActive(false);
                            stationUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                            stationUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                            stationUpgradePanelUI.MaxButton.gameObject.SetActive(false);
                            stationUpgradePanelUI.Tick.SetActive(false);

                            int totalUpgrade = 0;

                            if (stationUpgradeInfo.UpgradeType == StationUpgradeType.StationUpgradeType1)
                            {
                                StationUpgrade capacityStationUpgrade = data.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
                                if (capacityStationUpgrade != null)
                                {
                                    totalUpgrade += capacityStationUpgrade.Upgrade.Count;
                                }

                                StationUpgrade durationStationUpgrade = data.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
                                if (durationStationUpgrade != null)
                                {
                                    totalUpgrade += durationStationUpgrade.Upgrade.Count;
                                }

                                int upgradeIndex = 0;
                                int fill = 0;
                                bool hasUpgrade = true;


                                if (capacityStationUpgrade != null
                                    && capacityStationUpgrade.CurrentUpgradeIndex < capacityStationUpgrade.Upgrade.Count - 1)
                                {
                                    int capacityIndex = capacityStationUpgrade.CurrentUpgradeIndex;

                                    stationUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);
                                    stationUpgradePanelUI.UpgradeValueText.text = (capacityIndex + 1) + " -> " + (capacityIndex + 2);

                                    upgradeIndex = capacityStationUpgrade.CurrentUpgradeIndex + 1;
                                    fill = capacityStationUpgrade.CurrentUpgradeIndex;
                                    totalUpgrade--;
                                }
                                else if (durationStationUpgrade != null
                                    && durationStationUpgrade.CurrentUpgradeIndex < durationStationUpgrade.Upgrade.Count - 1)
                                {
                                    int durationIndex = durationStationUpgrade.CurrentUpgradeIndex;

                                    float baseValue = durationStationUpgrade.Upgrade[0];
                                    float currentValue = durationStationUpgrade.Upgrade[durationStationUpgrade.CurrentUpgradeIndex];
                                    float nextValue = durationStationUpgrade.Upgrade[durationStationUpgrade.CurrentUpgradeIndex + 1];

                                    float currentValuePercentage = Mathf.RoundToInt(((currentValue - baseValue) / baseValue) * 100);
                                    float nextValuePercentage = Mathf.RoundToInt(((nextValue - baseValue) / baseValue) * 100);

                                    stationUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);
                                    if (currentValuePercentage == 0)
                                    {
                                        stationUpgradePanelUI.UpgradeValueText.text = nextValuePercentage + "%";
                                    }
                                    else
                                    {
                                        stationUpgradePanelUI.UpgradeValueText.text = currentValuePercentage + "% ->" + nextValuePercentage + "%";
                                    }


                                    if (capacityStationUpgrade != null && capacityStationUpgrade.Upgrade.Count > 0)
                                    {
                                        upgradeIndex += capacityStationUpgrade.Upgrade.Count;
                                        fill = capacityStationUpgrade.Upgrade.Count - 1;
                                        totalUpgrade--;
                                    }

                                    upgradeIndex += durationStationUpgrade.CurrentUpgradeIndex + 1;
                                    fill += durationStationUpgrade.CurrentUpgradeIndex;

                                }
                                else
                                {
                                    stationUpgradePanelUI.Tick.SetActive(true);
                                    hasUpgrade = false;
                                }

                                stationUpgradePanelUI.StepFillBar.SetupBars(totalUpgrade - 1);
                                if (hasUpgrade)
                                {
                                    stationUpgradePanelUI.StepFillBar.SetFillAmount(fill);
                                }
                                else
                                {
                                    stationUpgradePanelUI.StepFillBar.SetFillAmount(totalUpgrade - 1);
                                }

                                if (hasUpgrade && upgradeIndex < stationUpgradeInfo.PropertyUpgradesInfo.Count)
                                {
                                    StationPropertyUpgradeInfo stationPropertyUpgradeInfo = stationUpgradeInfo.PropertyUpgradesInfo[upgradeIndex];

                                    if (stationPropertyUpgradeInfo.UpdateType == CurrencyType.Coin)
                                    {
                                        stationUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                                        stationUpgradePanelUI.UpgradeButtonText.text = stationPropertyUpgradeInfo.RequiredCoins.ToString();
                                        stationUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                                        stationUpgradePanelUI.UpgradeCost = stationPropertyUpgradeInfo.RequiredCoins;
                                    }
                                    else if (stationPropertyUpgradeInfo.UpdateType == CurrencyType.Gem)
                                    {
                                        stationUpgradePanelUI.UpgradeButtonGem.SetActive(true);
                                        stationUpgradePanelUI.UpgradeButtonText.text = stationPropertyUpgradeInfo.RequiredGems.ToString();
                                        stationUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                                        stationUpgradePanelUI.UpgradeCost = stationPropertyUpgradeInfo.RequiredGems;
                                    }

                                    stationUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                                    stationUpgradePanelUI.UpgradeNameText.gameObject.SetActive(true);
                                    stationUpgradePanelUI.UpgradeNameText.text = stationPropertyUpgradeInfo.PropertyUpgradeNameText;

                                    if (stationPropertyUpgradeInfo.UsePreviewSprite)
                                    {
                                        stationUpgradePanelUI.PreviewImage.sprite = stationPropertyUpgradeInfo.PreviewSprite;
                                        stationUpgradePanelUI.InfoPreviewSprite = stationPropertyUpgradeInfo.PreviewSprite;

                                    }
                                }
                                else
                                {
                                    int lastUpgradeIndex = stationUpgradeInfo.PropertyUpgradesInfo.Count - 1;
                                    StationPropertyUpgradeInfo stationPropertyUpgradeInfo = stationUpgradeInfo.PropertyUpgradesInfo[lastUpgradeIndex];

                                    stationUpgradePanelUI.MaxButton.gameObject.SetActive(true);

                                    if (stationPropertyUpgradeInfo.UsePreviewSprite)
                                    {
                                        stationUpgradePanelUI.PreviewImage.sprite = stationPropertyUpgradeInfo.PreviewSprite;
                                        stationUpgradePanelUI.InfoPreviewSprite = stationPropertyUpgradeInfo.PreviewSprite;
                                    }
                                }
                            }
                            else if (stationUpgradeInfo.UpgradeType == StationUpgradeType.StationUpgradeType2)
                            {
                                StationUpgrade costStationUpgrade = data.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
                                if (costStationUpgrade != null)
                                {
                                    totalUpgrade += costStationUpgrade.Upgrade.Count;
                                }

                                int upgradeIndex = 0;
                                bool hasUpgrade = true;

                                if (costStationUpgrade != null
                                  && costStationUpgrade.CurrentUpgradeIndex < costStationUpgrade.Upgrade.Count - 1)
                                {
                                    int durationIndex = costStationUpgrade.CurrentUpgradeIndex;

                                    float baseValue = costStationUpgrade.Upgrade[0];
                                    float currentValue = costStationUpgrade.Upgrade[costStationUpgrade.CurrentUpgradeIndex];
                                    float nextValue = costStationUpgrade.Upgrade[costStationUpgrade.CurrentUpgradeIndex + 1];

                                    float currentValuePercentage = Mathf.RoundToInt(((currentValue - baseValue) / baseValue) * 100);
                                    float nextValuePercentage = Mathf.RoundToInt(((nextValue - baseValue) / baseValue) * 100);

                                    stationUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);
                                    if (currentValuePercentage == 0)
                                    {
                                        stationUpgradePanelUI.UpgradeValueText.text = "+" + nextValuePercentage + "%";
                                    }
                                    else
                                    {
                                        stationUpgradePanelUI.UpgradeValueText.text = "+" + currentValuePercentage + "% -> +" + nextValuePercentage + "%";
                                    }


                                    upgradeIndex = costStationUpgrade.CurrentUpgradeIndex + 1;

                                }
                                else
                                {
                                    stationUpgradePanelUI.Tick.SetActive(true);
                                    hasUpgrade = false;
                                }

                                stationUpgradePanelUI.StepFillBar.SetupBars(totalUpgrade - 1);
                                if (hasUpgrade)
                                {
                                    stationUpgradePanelUI.StepFillBar.SetFillAmount(upgradeIndex - 1);
                                }
                                else
                                {
                                    stationUpgradePanelUI.StepFillBar.SetFillAmount(totalUpgrade - 1);
                                }

                                if (hasUpgrade && upgradeIndex < stationUpgradeInfo.PropertyUpgradesInfo.Count)
                                {
                                    StationPropertyUpgradeInfo stationPropertyUpgradeInfo = stationUpgradeInfo.PropertyUpgradesInfo[upgradeIndex];

                                    if (stationPropertyUpgradeInfo.UpdateType == CurrencyType.Coin)
                                    {
                                        stationUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                                        stationUpgradePanelUI.UpgradeButtonText.text = stationPropertyUpgradeInfo.RequiredCoins.ToString();
                                        stationUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                                        stationUpgradePanelUI.UpgradeCost = stationPropertyUpgradeInfo.RequiredCoins;
                                    }
                                    else if (stationPropertyUpgradeInfo.UpdateType == CurrencyType.Gem)
                                    {
                                        stationUpgradePanelUI.UpgradeButtonGem.SetActive(true);
                                        stationUpgradePanelUI.UpgradeButtonText.text = stationPropertyUpgradeInfo.RequiredGems.ToString();
                                        stationUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                                        stationUpgradePanelUI.UpgradeCost = stationPropertyUpgradeInfo.RequiredGems;
                                    }

                                    stationUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                                    stationUpgradePanelUI.UpgradeNameText.gameObject.SetActive(true);
                                    stationUpgradePanelUI.UpgradeNameText.text = stationPropertyUpgradeInfo.PropertyUpgradeNameText;

                                    if (stationPropertyUpgradeInfo.UsePreviewSprite)
                                    {
                                        stationUpgradePanelUI.PreviewImage.sprite = stationPropertyUpgradeInfo.PreviewSprite;
                                        stationUpgradePanelUI.InfoPreviewSprite = stationPropertyUpgradeInfo.PreviewSprite;
                                    }
                                }
                                else
                                {
                                    int lastUpgradeIndex = stationUpgradeInfo.PropertyUpgradesInfo.Count - 1;
                                    StationPropertyUpgradeInfo stationPropertyUpgradeInfo = stationUpgradeInfo.PropertyUpgradesInfo[lastUpgradeIndex];

                                    stationUpgradePanelUI.MaxButton.gameObject.SetActive(true);

                                    if (stationPropertyUpgradeInfo.UsePreviewSprite)
                                    {
                                        stationUpgradePanelUI.PreviewImage.sprite = stationPropertyUpgradeInfo.PreviewSprite;
                                        stationUpgradePanelUI.InfoPreviewSprite = stationPropertyUpgradeInfo.PreviewSprite;
                                    }
                                }
                            }

                        }
                    }
                }
            }


            return stationUpgradePanelsUI;
        }

        private StationUpgradePanelUI GetAndSetStationUpgradePanel(Transform holder, DataStation dataStation, StationUpgradeType stationUpgradeType)
        {

            foreach (var stationUpdateInfo in m_StationsUpdateInfo)
            {
                if (stationUpdateInfo.Data == dataStation)
                {
                    StationData data = stationUpdateInfo.Data.StationData;
                    foreach (var stationUpgradeInfo in stationUpdateInfo.StationUpgradeInfo)
                    {
                        if (stationUpgradeInfo.UpgradeType == stationUpgradeType)
                        {
                            StationUpgradePanelUI stationUpgradePanelUI = Instantiate(stationUpgradeInfo.StationUpgradePanelUIPrefab, holder);
                            stationUpgradePanelUI.Setup(stationUpdateInfo.Data, stationUpgradeInfo.UpgradeType);

                            stationUpgradePanelUI.HeaderText.text = stationUpgradeInfo.HeaderText;
                            stationUpgradePanelUI.PreviewImage.sprite = stationUpgradeInfo.BasePreviewSprite;

                            stationUpgradePanelUI.InfoPreviewSprite = stationUpgradeInfo.BasePreviewSprite;
                            stationUpgradePanelUI.InfoDiscriptionText = stationUpgradeInfo.InfoDiscriptionText;

                            stationUpgradePanelUI.NotifiationSeenKey = stationUpgradeInfo.NotifationSeenKey;

                            stationUpgradePanelUI.UpgradeNameText.gameObject.SetActive(false);
                            stationUpgradePanelUI.UpgradeValueText.gameObject.SetActive(false);
                            stationUpgradePanelUI.UpgradeButton.gameObject.SetActive(false);
                            stationUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                            stationUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                            stationUpgradePanelUI.MaxButton.gameObject.SetActive(false);
                            stationUpgradePanelUI.Tick.SetActive(false);

                            int totalUpgrade = 0;

                            if (stationUpgradeInfo.UpgradeType == StationUpgradeType.StationUpgradeType1)
                            {
                                StationUpgrade capacityStationUpgrade = data.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
                                if (capacityStationUpgrade != null)
                                {
                                    totalUpgrade += capacityStationUpgrade.Upgrade.Count;
                                }

                                StationUpgrade durationStationUpgrade = data.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
                                if (durationStationUpgrade != null)
                                {
                                    totalUpgrade += durationStationUpgrade.Upgrade.Count;
                                }

                                int upgradeIndex = 0;
                                int fill = 0;
                                bool hasUpgrade = true;


                                if (capacityStationUpgrade != null
                                    && capacityStationUpgrade.CurrentUpgradeIndex < capacityStationUpgrade.Upgrade.Count - 1)
                                {
                                    int capacityIndex = capacityStationUpgrade.CurrentUpgradeIndex;

                                    stationUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);
                                    stationUpgradePanelUI.UpgradeValueText.text = (capacityIndex + 1) + " -> " + (capacityIndex + 2);

                                    upgradeIndex = capacityStationUpgrade.CurrentUpgradeIndex + 1;
                                    fill = capacityStationUpgrade.CurrentUpgradeIndex;
                                    totalUpgrade--;
                                }
                                else if (durationStationUpgrade != null
                                    && durationStationUpgrade.CurrentUpgradeIndex < durationStationUpgrade.Upgrade.Count - 1)
                                {
                                    int durationIndex = durationStationUpgrade.CurrentUpgradeIndex;

                                    float baseValue = durationStationUpgrade.Upgrade[0];
                                    float currentValue = durationStationUpgrade.Upgrade[durationStationUpgrade.CurrentUpgradeIndex];
                                    float nextValue = durationStationUpgrade.Upgrade[durationStationUpgrade.CurrentUpgradeIndex + 1];

                                    float currentValuePercentage = Mathf.RoundToInt(((currentValue - baseValue) / baseValue) * 100);
                                    float nextValuePercentage = Mathf.RoundToInt(((nextValue - baseValue) / baseValue) * 100);

                                    stationUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);
                                    if (currentValuePercentage == 0)
                                    {
                                        stationUpgradePanelUI.UpgradeValueText.text = nextValuePercentage + "%";
                                    }
                                    else
                                    {
                                        stationUpgradePanelUI.UpgradeValueText.text = currentValuePercentage + "% -> " + nextValuePercentage + "%";
                                    }


                                    if (capacityStationUpgrade != null && capacityStationUpgrade.Upgrade.Count > 0)
                                    {
                                        upgradeIndex += capacityStationUpgrade.Upgrade.Count;
                                        fill = capacityStationUpgrade.Upgrade.Count - 1;
                                        totalUpgrade--;
                                    }
                                    upgradeIndex += durationStationUpgrade.CurrentUpgradeIndex + 1;
                                    fill += durationStationUpgrade.CurrentUpgradeIndex;
                                }
                                else
                                {
                                    stationUpgradePanelUI.Tick.SetActive(true);
                                    hasUpgrade = false;
                                }

                                stationUpgradePanelUI.StepFillBar.SetupBars(totalUpgrade - 1);
                                if (hasUpgrade)
                                {
                                    stationUpgradePanelUI.StepFillBar.SetFillAmount(fill);
                                }
                                else
                                {
                                    stationUpgradePanelUI.StepFillBar.SetFillAmount(totalUpgrade - 1);
                                }

                                if (hasUpgrade && upgradeIndex < stationUpgradeInfo.PropertyUpgradesInfo.Count)
                                {
                                    StationPropertyUpgradeInfo stationPropertyUpgradeInfo = stationUpgradeInfo.PropertyUpgradesInfo[upgradeIndex];

                                    if (stationPropertyUpgradeInfo.UpdateType == CurrencyType.Coin)
                                    {
                                        stationUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                                        stationUpgradePanelUI.UpgradeButtonText.text = stationPropertyUpgradeInfo.RequiredCoins.ToString();
                                        stationUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                                        stationUpgradePanelUI.UpgradeCost = stationPropertyUpgradeInfo.RequiredCoins;
                                    }
                                    else if (stationPropertyUpgradeInfo.UpdateType == CurrencyType.Gem)
                                    {
                                        stationUpgradePanelUI.UpgradeButtonGem.SetActive(true);
                                        stationUpgradePanelUI.UpgradeButtonText.text = stationPropertyUpgradeInfo.RequiredGems.ToString();
                                        stationUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                                        stationUpgradePanelUI.UpgradeCost = stationPropertyUpgradeInfo.RequiredGems;
                                    }

                                    stationUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                                    stationUpgradePanelUI.UpgradeNameText.gameObject.SetActive(true);
                                    stationUpgradePanelUI.UpgradeNameText.text = stationPropertyUpgradeInfo.PropertyUpgradeNameText;

                                    if (stationPropertyUpgradeInfo.UsePreviewSprite)
                                    {
                                        stationUpgradePanelUI.PreviewImage.sprite = stationPropertyUpgradeInfo.PreviewSprite;
                                        stationUpgradePanelUI.InfoPreviewSprite = stationPropertyUpgradeInfo.PreviewSprite;
                                    }
                                }
                                else
                                {
                                    int lastUpgradeIndex = stationUpgradeInfo.PropertyUpgradesInfo.Count - 1;
                                    StationPropertyUpgradeInfo stationPropertyUpgradeInfo = stationUpgradeInfo.PropertyUpgradesInfo[lastUpgradeIndex];

                                    stationUpgradePanelUI.MaxButton.gameObject.SetActive(true);

                                    if (stationPropertyUpgradeInfo.UsePreviewSprite)
                                    {
                                        stationUpgradePanelUI.PreviewImage.sprite = stationPropertyUpgradeInfo.PreviewSprite;
                                        stationUpgradePanelUI.InfoPreviewSprite = stationPropertyUpgradeInfo.PreviewSprite;
                                    }
                                }
                            }
                            else if (stationUpgradeInfo.UpgradeType == StationUpgradeType.StationUpgradeType2)
                            {
                                StationUpgrade costStationUpgrade = data.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
                                if (costStationUpgrade != null)
                                {
                                    totalUpgrade += costStationUpgrade.Upgrade.Count;
                                }

                                int upgradeIndex = 0;
                                bool hasUpgrade = true;

                                if (costStationUpgrade != null
                                  && costStationUpgrade.CurrentUpgradeIndex < costStationUpgrade.Upgrade.Count - 1)
                                {
                                    int durationIndex = costStationUpgrade.CurrentUpgradeIndex;

                                    float baseValue = costStationUpgrade.Upgrade[0];
                                    float currentValue = costStationUpgrade.Upgrade[costStationUpgrade.CurrentUpgradeIndex];
                                    float nextValue = costStationUpgrade.Upgrade[costStationUpgrade.CurrentUpgradeIndex + 1];

                                    float currentValuePercentage = Mathf.RoundToInt(((currentValue - baseValue) / baseValue) * 100);
                                    float nextValuePercentage = Mathf.RoundToInt(((nextValue - baseValue) / baseValue) * 100);

                                    stationUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);
                                    if (currentValuePercentage == 0)
                                    {
                                        stationUpgradePanelUI.UpgradeValueText.text = "+" + nextValuePercentage + "%";
                                    }
                                    else
                                    {
                                        stationUpgradePanelUI.UpgradeValueText.text = "+" + currentValuePercentage + "% -> +" + nextValuePercentage + "%";
                                    }


                                    upgradeIndex = costStationUpgrade.CurrentUpgradeIndex + 1;

                                }
                                else
                                {
                                    stationUpgradePanelUI.Tick.SetActive(true);
                                    hasUpgrade = false;
                                }

                                stationUpgradePanelUI.StepFillBar.SetupBars(totalUpgrade - 1);
                                if (hasUpgrade)
                                {
                                    stationUpgradePanelUI.StepFillBar.SetFillAmount(upgradeIndex - 1);
                                }
                                else
                                {
                                    stationUpgradePanelUI.StepFillBar.SetFillAmount(totalUpgrade - 1);
                                }

                                if (hasUpgrade && upgradeIndex < stationUpgradeInfo.PropertyUpgradesInfo.Count)
                                {
                                    StationPropertyUpgradeInfo stationPropertyUpgradeInfo = stationUpgradeInfo.PropertyUpgradesInfo[upgradeIndex];

                                    if (stationPropertyUpgradeInfo.UpdateType == CurrencyType.Coin)
                                    {
                                        stationUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                                        stationUpgradePanelUI.UpgradeButtonText.text = stationPropertyUpgradeInfo.RequiredCoins.ToString();
                                        stationUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                                        stationUpgradePanelUI.UpgradeCost = stationPropertyUpgradeInfo.RequiredCoins;
                                    }
                                    else if (stationPropertyUpgradeInfo.UpdateType == CurrencyType.Gem)
                                    {

                                        stationUpgradePanelUI.UpgradeButtonGem.SetActive(true);
                                        stationUpgradePanelUI.UpgradeButtonText.text = stationPropertyUpgradeInfo.RequiredGems.ToString();
                                        stationUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                                        stationUpgradePanelUI.UpgradeCost = stationPropertyUpgradeInfo.RequiredGems;
                                    }

                                    stationUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                                    stationUpgradePanelUI.UpgradeNameText.gameObject.SetActive(true);
                                    stationUpgradePanelUI.UpgradeNameText.text = stationPropertyUpgradeInfo.PropertyUpgradeNameText;

                                    if (stationPropertyUpgradeInfo.UsePreviewSprite)
                                    {
                                        stationUpgradePanelUI.PreviewImage.sprite = stationPropertyUpgradeInfo.PreviewSprite;
                                        stationUpgradePanelUI.InfoPreviewSprite = stationPropertyUpgradeInfo.PreviewSprite;
                                    }
                                }
                                else
                                {
                                    int lastUpgradeIndex = stationUpgradeInfo.PropertyUpgradesInfo.Count - 1;
                                    StationPropertyUpgradeInfo stationPropertyUpgradeInfo = stationUpgradeInfo.PropertyUpgradesInfo[lastUpgradeIndex];

                                    stationUpgradePanelUI.MaxButton.gameObject.SetActive(true);

                                    if (stationPropertyUpgradeInfo.UsePreviewSprite)
                                    {
                                        stationUpgradePanelUI.PreviewImage.sprite = stationPropertyUpgradeInfo.PreviewSprite;
                                        stationUpgradePanelUI.InfoPreviewSprite = stationPropertyUpgradeInfo.PreviewSprite;
                                    }
                                }
                            }


                            return stationUpgradePanelUI;
                        }
                    }
                }
            }


            return null;
        }

        //Returns the upgraded StationUpgradePanelUI
        public StationUpgradePanelUI UpgradeStationProperty(DataStation dataStation, StationUpgradeType stationUpgradeType, StationUpgradePanelUI stationUpgradePanelUI)
        {
            foreach (var stationUpdateInfo in m_StationsUpdateInfo)
            {
                if (stationUpdateInfo.Data == dataStation)
                {
                    StationData data = stationUpdateInfo.Data.StationData;
                    if (stationUpgradeType == StationUpgradeType.StationUpgradeType1)
                    {
                        StationUpgrade capacityStationUpgrade = data.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
                        StationUpgrade durationStationUpgrade = data.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);

                        if (capacityStationUpgrade != null
                            && capacityStationUpgrade.CurrentUpgradeIndex < capacityStationUpgrade.Upgrade.Count - 1)
                        {
                            capacityStationUpgrade.CurrentUpgradeIndex++;
                            data.HasUpgraded = true;
                            stationUpdateInfo.Data.Save();
                        }
                        else if (durationStationUpgrade != null
                            && durationStationUpgrade.CurrentUpgradeIndex < durationStationUpgrade.Upgrade.Count - 1)
                        {
                            durationStationUpgrade.CurrentUpgradeIndex++;
                            data.HasUpgraded = true;
                            stationUpdateInfo.Data.Save();
                        }

                        if (stationUpdateInfo.DependentsUpgradableData != null)
                        {
                            foreach (var dependentUpgradableData in stationUpdateInfo.DependentsUpgradableData)
                            {
                                foreach (var propertyUpgrade in dependentUpgradableData.StationData.Upgrades)
                                {
                                    if (propertyUpgrade.UpgradeType == PropertyUpgradeType.Capacity
                                        && propertyUpgrade.Upgrade.Count > capacityStationUpgrade.CurrentUpgradeIndex)
                                    {
                                        propertyUpgrade.CurrentUpgradeIndex = capacityStationUpgrade.CurrentUpgradeIndex;
                                    }
                                    else if (propertyUpgrade.UpgradeType == PropertyUpgradeType.Duration
                                       && propertyUpgrade.Upgrade.Count > durationStationUpgrade.CurrentUpgradeIndex)
                                    {
                                        propertyUpgrade.CurrentUpgradeIndex = durationStationUpgrade.CurrentUpgradeIndex;
                                    }
                                }

                                dependentUpgradableData.Save();
                            }
                        }

                        Transform parent = stationUpgradePanelUI.transform.parent;
                        int hierarchyIndex = stationUpgradePanelUI.transform.GetSiblingIndex();

                        Destroy(stationUpgradePanelUI.gameObject);

                        StationUpgradePanelUI newStationUpgradePanelUI = GetAndSetStationUpgradePanel(parent, stationUpdateInfo.Data, StationUpgradeType.StationUpgradeType1);

                        newStationUpgradePanelUI.transform.SetSiblingIndex(hierarchyIndex);

                        return newStationUpgradePanelUI;
                    }
                    else
                    {
                        StationUpgrade costStationUpgrade = data.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
                        if (costStationUpgrade != null
                            && costStationUpgrade.CurrentUpgradeIndex < costStationUpgrade.Upgrade.Count - 1)
                        {
                            costStationUpgrade.CurrentUpgradeIndex++;
                            data.HasUpgraded = true;
                            stationUpdateInfo.Data.Save();
                        }

                        if (stationUpdateInfo.DependentsUpgradableData != null)
                        {
                            foreach (var dependentUpgradableData in stationUpdateInfo.DependentsUpgradableData)
                            {
                                foreach (var propertyUpgrade in dependentUpgradableData.StationData.Upgrades)
                                {
                                    if (propertyUpgrade.UpgradeType == PropertyUpgradeType.Cost
                                        && propertyUpgrade.Upgrade.Count > costStationUpgrade.CurrentUpgradeIndex)
                                    {
                                        propertyUpgrade.CurrentUpgradeIndex = costStationUpgrade.CurrentUpgradeIndex;
                                    }
                                }

                                dependentUpgradableData.Save();
                            }
                        }

                        Transform parent = stationUpgradePanelUI.transform.parent;
                        int hierarchyIndex = stationUpgradePanelUI.transform.GetSiblingIndex();

                        Destroy(stationUpgradePanelUI.gameObject);

                        StationUpgradePanelUI newStationUpgradePanelUI = GetAndSetStationUpgradePanel(parent, stationUpdateInfo.Data, StationUpgradeType.StationUpgradeType2);

                        newStationUpgradePanelUI.transform.SetSiblingIndex(hierarchyIndex);

                        return newStationUpgradePanelUI;
                    }
                }
            }

            return stationUpgradePanelUI;
        }
        #endregion

        #region Station and Decoration Unlock Related

        /// <summary>
        /// return if has unlockble, preview, name, discritpion, stars, coin reward, gem reward, callback
        /// returns null if no star item found
        /// </summary>
        public StarItemInfo GetNextUnlockable()
        {
            int stars = DataManager.StarCurrency;

            int unlockingOrder = int.MaxValue;
                
            StationUpdateInfo toBeUnlockedStation = null;
            DecorationUpdateInfo toBeUnlockedDecoration = null;

            foreach (var stationsUpdateInfo in m_StationsUpdateInfo)
            {
                if (stationsUpdateInfo.UnlockingOrder < unlockingOrder
                    && !stationsUpdateInfo.Data.StationData.IsUnlocked)
                {
                    unlockingOrder = stationsUpdateInfo.UnlockingOrder;
                    toBeUnlockedStation = stationsUpdateInfo;
                }
            }

            foreach (var decorationsUpdateInfo in m_DecorationsUpdateInfo)
            {
                if (decorationsUpdateInfo.UnlockingOrder < unlockingOrder
                    && !decorationsUpdateInfo.Data.EnvironmentDecorationData.IsUnlocked)
                {
                    unlockingOrder = decorationsUpdateInfo.UnlockingOrder;
                    toBeUnlockedDecoration = decorationsUpdateInfo;
                }
            }

            if (toBeUnlockedDecoration != null)
            {
                bool canBeUnlocked = false;
                if (toBeUnlockedDecoration.RequiredStars <= stars)
                {
                    canBeUnlocked = true;
                }

                return new StarItemInfo(
                        canBeUnlocked,
                        toBeUnlockedDecoration.Preview,
                        toBeUnlockedDecoration.NameDiscption,
                        toBeUnlockedDecoration.Discription,
                        toBeUnlockedDecoration.RequiredStars, 
                        toBeUnlockedDecoration.UnlockingCoinReward, 
                        toBeUnlockedDecoration.UnlockingGemReward,
                        () => 
                        {
                            DataManager.StarCurrency -= toBeUnlockedDecoration.RequiredStars;
                            DataManager.SaveData();
                            toBeUnlockedDecoration.Data.EnvironmentDecorationData.IsUnlocked = true;
                            toBeUnlockedDecoration.Data.EnvironmentDecorationData.HasJustUnlocked = true;
                            toBeUnlockedDecoration.Data.Save();
                        });
            }
            else if (toBeUnlockedStation != null)
            {
                bool canBeUnlocked = false;
                if (toBeUnlockedStation.RequiredStars <= stars)
                {
                    canBeUnlocked = true;
                }

                return new StarItemInfo(
                    canBeUnlocked,
                    toBeUnlockedStation.Preview,
                    toBeUnlockedStation.NameDiscption,
                    toBeUnlockedStation.Discription,
                    toBeUnlockedStation.RequiredStars,
                    toBeUnlockedStation.UnlockingCoinReward,
                    toBeUnlockedStation.UnlockingGemReward, 
                    () =>
                    {
                        DataManager.StarCurrency -= toBeUnlockedStation.RequiredStars;
                        DataManager.SaveData();
                        toBeUnlockedStation.Data.StationData.IsUnlocked = true;
                        toBeUnlockedStation.Data.StationData.HasJustUnlocked = true;
                        toBeUnlockedStation.Data.Save();
                    });
            }
            
            return null;
        }

        #endregion

        #region Patience Related
        public List<PatienceUpgradePanelUI> GetAndSetPatienceUpgradePanels(Transform holder)
        {
            return m_DataMapPatienceUpdate.GetAndSetPatienceUpgradePanels(holder);
        }

        public PatienceUpgradePanelUI GetAndSetPatienceUpgradePanel(DataPatience dataPatience, Transform holder)
        {
            return m_DataMapPatienceUpdate.GetAndSetPatienceUpgradePanel(dataPatience, holder);
        }

        public PatienceUpgradePanelUI UpgradePatienceUpgradePanel(DataPatience dataPatience, PatienceUpgradePanelUI patienceUpgradePanelUI)
        {
            return m_DataMapPatienceUpdate.UpgradePatienceUpgradePanel(dataPatience, patienceUpgradePanelUI);
        }
        #endregion

        #region Achievement Related
        public DataMapAchievementUpdate GetMapAchievementUpdate()
        {
            return m_MapAchievementUpdate;
        }
        #endregion

        #region Profile Related
        public DataProfile GetDataProfile()
        {
            return m_DataProfile;
        }
        #endregion
    }

    #region Player Related
    [Serializable]  
    public class PlayerUpdateInfo
    {
        public DataPlayer Data;
        [Header("-Include base value (Index 0) of upgrade aswell to match index (Necessary)")]
        public List<PlayerUpgradeInfo> PlayerUpgradesInfo;
    }

    public enum PlayerUpgradeType
    {
        Capacity, WalkSpeed
    }

    [Serializable]
    public class PlayerUpgradeInfo
    {
        public PlayerUpgradeType UpgradeType;
        public PlayerUpgradePanelUI PlayerUpgradePanelUIPrefab;
        public string HeaderText;
        public string UpgradeNameText;

        public Sprite BasePreviewSprite;

        [TextArea]
        public string InfoDiscriptionText;

        [Space]
        public string NotifationSeenKey;

        [Space, NonReorderable]
        public List<PlayerPropertyUpgradeInfo> PropertyUpgradesInfo;
    }

    [Serializable]
    public class PlayerPropertyUpgradeInfo
    {
        [EnumFilter((int)CurrencyType.Coin, (int)CurrencyType.Gem)]
        public CurrencyType UpdateType = CurrencyType.Coin;
        [AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Coin)]
        public int RequiredCoins;
        [AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Gem)]
        public int RequiredGems;

        public bool UsePreviewSprite;
        [AllowNesting, ShowIf(nameof(UsePreviewSprite)), Tooltip("Going to override BasePreviewSprite")]
        public Sprite PreviewSprite;
    }

    #endregion

    #region Worker Related

    [Serializable]
    public class WorkerUpdateInfo
    {
        public List<WorkerUnlockingInfo> WorkersUnlockingInfo;
        [Header("-Quantity Upgrade of the current unlocked workers (will going to give choice)")]
        public List<WorkerUpgradeQuantityInfo> WorkersUpgradeQuantityInfo;
    }

    [Serializable]
    public class WorkerUnlockingInfo
    {
        public DataWorker Data;
        public int UnlockAtLevel;

        public Sprite Worker1Sprite;
        public Sprite Worker2Sprite;
        public Sprite WorkerOrderSymbolSprite;

        public Sprite WorkerOrderSymbolSpriteForUpgrade;
        public Sprite WorkerTypeSprite;

        public WorkerUpgradeServingTimeInfo UpgradeServingTimeInfo;
    }

    [Serializable]
    public class WorkerUpgradeServingTimeInfo
    {
        public WorkerUpgradePanelUI WorkerUpgradePanelUIPrefab;
        public string HeaderText;
        public Sprite PanelSprite;
        public Sprite PreviewSprite1;
        public Sprite PreviewSprite2;

        [TextArea]
        public string InfoDiscriptionText;

        public Sprite WorkerOrderSymbolSprite;
        public Sprite UpgradeButtonSprite;

        [Space]
        public string NotifationSeenKey;

        [Space]
        public List<WorkerUpgradeServingTimeRangeInfo> UpgradeServingTimeRangesInfo;
    }

    [Serializable]
    public class WorkerUpgradeServingTimeRangeInfo
    {
        public int LevelDifficultyRangeMin;
        public int LevelDifficultyRangeMax;
        [NonReorderable]
        public List<WorkerUpgradeServingTimePrice> UpgradeServingTimePrices;
    }

    [Serializable]
    public class WorkerUpgradeServingTimePrice
    {
        [EnumFilter((int)CurrencyType.Coin, (int)CurrencyType.Gem)]
        public CurrencyType UpdateType = CurrencyType.Coin;
        [AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Coin)]
        public int RequiredCoins;
        [AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Gem)]
        public int RequiredGems;
    }

    [Serializable]
    public class WorkerUpgradeQuantityInfo
    {
        [Header("-The upgrade option can only be shown once thats why there is key")]
        public string Key;
        public int ShowAtLevel;
    }
    #endregion

    #region Station Related
    [Serializable]
    public class StationUpdateInfo
    {
        public DataStation Data;
        public List<DataStation> DependentsUpgradableData;
        public int UnlockingOrder;
        public int RequiredStars;
        public Sprite Preview;
        public string NameDiscption;
        public string Discription;

        public int UnlockingCoinReward;
        public int UnlockingGemReward;

        public List<StationUpgradeInfo> StationUpgradeInfo;

    }

    public enum StationUpgradeType
    {
        StationUpgradeType1, StationUpgradeType2
    }

    [Serializable]
    public class StationUpgradeInfo
    {
        public StationUpgradeType UpgradeType;
        public StationUpgradePanelUI StationUpgradePanelUIPrefab;
        public string HeaderText;
        public Sprite BasePreviewSprite;

        [TextArea]
        public string InfoDiscriptionText;

        [Space]
        public string NotifationSeenKey;

        [Space]
        public List<StationPropertyUpgradeInfo> PropertyUpgradesInfo;
    }

    [Serializable]
    public class StationPropertyUpgradeInfo
    {
        [EnumFilter((int)CurrencyType.Coin, (int)CurrencyType.Gem)]
        public CurrencyType UpdateType = CurrencyType.Coin;
        [AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Coin)]
        public int RequiredCoins;
        [AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Gem)]
        public int RequiredGems;

        public string PropertyUpgradeNameText;
        public bool UsePreviewSprite;
        [AllowNesting, ShowIf(nameof(UsePreviewSprite)), Tooltip("Going to override BasePreviewSprite")]
        public Sprite PreviewSprite;
    }
    #endregion

    #region Chair Related

    [Serializable]
    public class ChairUpdateInfo
    {
        public ChairUpgradeType ChairType;
        public int ShowAfterAndAtLevel;
        public ChairCapacityUpgradePanelUI ChairCapacityUpgradePanelUIPrefab;
        public string HeaderText;
        public Sprite BasePreviewSprite;
        public string UpgradeNameText;

        [TextArea]
        public string InfoDiscriptionText;

        [Space]
        public string NotifationSeenKey;

        [Space]
        public List<ChairCapacityUpgrade> ChairsCapacityUpgrade;
    }

    [Serializable]
    public class ChairCapacityUpgrade
    {
        //public DataSalonChair SalonChairData;
        //public DataCafeChair CafeChairData;

        public DataSaver ChairData;

        public bool IsAlreadyUnlocked;
        [EnumFilter((int)CurrencyType.Coin, (int)CurrencyType.Gem)]
        public CurrencyType UpdateType = CurrencyType.Coin;
        [AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Coin)]
        public int RequiredCoins;
        [AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Gem)]
        public int RequiredGems;
    }

    public enum ChairUpgradeType
    {
        Salon, Cafe
    }
    #endregion

    #region Decoration Related
    [Serializable]
    public class DecorationUpdateInfo
    {
        public DataEnvironmentDecoration Data;
        public int UnlockingOrder;
        public int RequiredStars;
        public Sprite Preview;
        public string NameDiscption;
        public string Discription;

        public int UnlockingCoinReward;
        public int UnlockingGemReward;
    }
    #endregion

    #region Booster Related

    public enum BoosterType
    {
        TimeFroze, WaitressSpeed, InstanceOrderFill
    }

    [Serializable]
    public class BoosterInfo
    {
        public BoosterType BoosterType;
        public int UnlockAtLevel;
        [AllowNesting, HideIf(nameof(BoosterType), BoosterType.InstanceOrderFill)]
        public float AppliedDuration;
    }

    #endregion

    #region Star Item Info
    //unlockble, preview, name, discritpion, stars, coin reward, gem reward, callback
    public class StarItemInfo
    {
        public bool IsUnloackble;
        public Sprite PreviewSprite;
        public string NameText;
        public string DiscriptionText;
        public int StarRequired;
        public int CoinReward;
        public int GemReward;
        public Action OnUnlocked;

        public StarItemInfo(
            bool isUnloackble,
            Sprite previewSprite,
            string nameText,
            string discriptionText,
            int starRequired,
            int coinReward,
            int gemReward,
            Action onUnlocked)
        {
            IsUnloackble = isUnloackble;
            PreviewSprite = previewSprite;
            NameText = nameText;
            DiscriptionText = discriptionText;
            StarRequired = starRequired;
            CoinReward = coinReward;
            GemReward = gemReward;
            OnUnlocked = onUnlocked;
        }
    }
    #endregion
}


