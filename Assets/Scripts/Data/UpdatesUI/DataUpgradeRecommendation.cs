using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyAttributes;
using Isometric.UI;
using Isometric.Data;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataMapUpdate", menuName = "GameData/DataUpgradeRecommendation")]
    public class DataUpgradeRecommendation : ScriptableObject
    {
        [SerializeField] List<UpgradeRecommendationData> m_UpgradeRecommendations;

        public void SetUpgradeRecommendation(List<PlayerUpgradePanelUI> playerUpgradePanelsUI,
            ChairCapacityUpgradePanelUI chairCapacityUpgradePanelUI,
            List<WorkerUpgradePanelUI> workerUpgradePanelsUI,
            List<StationUpgradePanelUI> stationUpgradePanelsUI,
            List<PatienceUpgradePanelUI> patienceUpgradePanelsUI,
            // List<PurchaseOfferPanelUI> purchaseOfferPanelsUI,
            UpgradePopupUIManager upgradePopupUIManager,
            bool shouldScrollFocus)
        {
            int level = DataManager.CurrentMapLevelIndex + 1;

            UpgradeRecommendationData targetUpgradeRecommendation = null;
            foreach (var upgradeRecommendation in m_UpgradeRecommendations)
            {
                if (upgradeRecommendation.LevelFrom <= level && upgradeRecommendation.LevelTo >= level)
                {
                    targetUpgradeRecommendation = upgradeRecommendation;
                    break;
                }
            }

            if (targetUpgradeRecommendation != null)
            {
                foreach (var recommendationSetting in targetUpgradeRecommendation.RecommendationSettings)
                {
                    //Recommend player properties 
                    if (recommendationSetting.RecommendationType == UgradeRecommendationType.Player)
                    {
                        if (recommendationSetting.PlayerUpgradeType == PlayerUpgradeType.Capacity &&
                            recommendationSetting.DataPlayer.PlayerData.Capacity.Count > recommendationSetting.PlayerTargetUpgradeIndex &&
                            recommendationSetting.DataPlayer.PlayerData.CurrentCapacityIndex == recommendationSetting.PlayerTargetUpgradeIndex - 1)
                        {
                            PlayerUpgradePanelUI playerUpgradePanelUI = playerUpgradePanelsUI.Find((x) => x.PlayerUpgradeType == PlayerUpgradeType.Capacity);
                            if (playerUpgradePanelUI != null)
                            {
                                playerUpgradePanelUI.OnRecommendation?.Invoke();
                                playerUpgradePanelUI.IsRecommended = true;
                                if (shouldScrollFocus &&
                                    playerUpgradePanelUI.TryGetComponent(out RectTransform target))
                                {
                                    upgradePopupUIManager.TryScrollTo(target, 0);
                                }
                                return;
                            }
                        }
                        else if (recommendationSetting.PlayerUpgradeType == PlayerUpgradeType.WalkSpeed &&
                            recommendationSetting.DataPlayer.PlayerData.WalkSpeed.Count > recommendationSetting.PlayerTargetUpgradeIndex &&
                            recommendationSetting.DataPlayer.PlayerData.CurrentWalkSpeedIndex == recommendationSetting.PlayerTargetUpgradeIndex - 1)
                        {
                            PlayerUpgradePanelUI playerUpgradePanelUI = playerUpgradePanelsUI.Find((x) => x.PlayerUpgradeType == PlayerUpgradeType.WalkSpeed);
                            if (playerUpgradePanelUI != null)
                            {
                                playerUpgradePanelUI.OnRecommendation?.Invoke();
                                playerUpgradePanelUI.IsRecommended = true;
                                if (shouldScrollFocus &&
                                    playerUpgradePanelUI.TryGetComponent(out RectTransform target))
                                {
                                    upgradePopupUIManager.TryScrollTo(target, 0);
                                }
                                return;
                            }
                        }
                    }
                    else if (recommendationSetting.RecommendationType == UgradeRecommendationType.Chair)
                    {
                        if (recommendationSetting.ChairInfo.DataChairs.Count > recommendationSetting.ChairTargetUpgradeIndex &&
                            !recommendationSetting.ChairInfo.DataChairs[recommendationSetting.ChairTargetUpgradeIndex].SalonChairData.IsUnlocked)
                        {
                            if (chairCapacityUpgradePanelUI != null)
                            {
                                chairCapacityUpgradePanelUI.OnRecommendation?.Invoke();
                                chairCapacityUpgradePanelUI.IsRecommended = true;
                                if (shouldScrollFocus &&
                                    chairCapacityUpgradePanelUI.TryGetComponent(out RectTransform target))
                                {
                                    upgradePopupUIManager.TryScrollTo(target, 0);
                                }
                                return;
                            }
                        }
                    }
                    else if (recommendationSetting.RecommendationType == UgradeRecommendationType.Worker)
                    {
                        if (recommendationSetting.DataWorker.WorkerOrderData.WorkerCurrentLevelDifficulty == recommendationSetting.WorkerTargetLevelDifficulty - 1)
                        {
                            WorkerUpgradePanelUI workerUpgradePanelUI = workerUpgradePanelsUI.Find((x) => x.DataWorker == recommendationSetting.DataWorker);
                            if (workerUpgradePanelUI != null)
                            {
                                workerUpgradePanelUI.OnRecommendation?.Invoke();
                                workerUpgradePanelUI.IsRecommended = true;
                                if (shouldScrollFocus &&
                                    workerUpgradePanelUI.TryGetComponent(out RectTransform target))
                                {
                                    upgradePopupUIManager.TryScrollTo(target, 0);
                                }
                                return;
                            }
                        }
                    }
                    else if (recommendationSetting.RecommendationType == UgradeRecommendationType.Station)
                    {
                        if (recommendationSetting.StationUpgradeType == StationUpgradeType.StationUpgradeType1)
                        {
                            StationUpgrade stationCapacityUpgrade = recommendationSetting.DataStaion.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
                            if (stationCapacityUpgrade != null &&
                                stationCapacityUpgrade.Upgrade.Count > recommendationSetting.StationTargetUpgradeIndex &&
                                stationCapacityUpgrade.CurrentUpgradeIndex == recommendationSetting.StationTargetUpgradeIndex - 1)
                            {
                                StationUpgradePanelUI stationUpgradePanelUI = stationUpgradePanelsUI.Find((x) => x.DataStation == recommendationSetting.DataStaion && x.StationUpgradeType == recommendationSetting.StationUpgradeType);
                                if (stationUpgradePanelUI != null)
                                {
                                    stationUpgradePanelUI.OnRecommendation?.Invoke();
                                    stationUpgradePanelUI.IsRecommended = true;
                                    if (shouldScrollFocus &&
                                    stationUpgradePanelUI.TryGetComponent(out RectTransform target))
                                    {
                                        upgradePopupUIManager.TryScrollTo(target, 0);
                                    }
                                    return;
                                }
                            }

                            if (stationCapacityUpgrade != null
                                && stationCapacityUpgrade.Upgrade.Count <= recommendationSetting.StationTargetUpgradeIndex)
                            {
                                int upgradeIndex = recommendationSetting.StationTargetUpgradeIndex - stationCapacityUpgrade.Upgrade.Count + 1;
                                StationUpgrade stationDurationUpgrade = recommendationSetting.DataStaion.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
                                if (stationDurationUpgrade != null &&
                                    stationDurationUpgrade.Upgrade.Count > upgradeIndex &&
                                    stationDurationUpgrade.CurrentUpgradeIndex == upgradeIndex - 1)
                                {
                                    StationUpgradePanelUI stationUpgradePanelUI = stationUpgradePanelsUI.Find((x) => x.DataStation == recommendationSetting.DataStaion && x.StationUpgradeType == recommendationSetting.StationUpgradeType);
                                    if (stationUpgradePanelUI != null)
                                    {
                                        stationUpgradePanelUI.OnRecommendation?.Invoke();
                                        stationUpgradePanelUI.IsRecommended = true;
                                        if (shouldScrollFocus &&
                                        stationUpgradePanelUI.TryGetComponent(out RectTransform target))
                                        {
                                            upgradePopupUIManager.TryScrollTo(target, 0);
                                        }
                                        return;
                                    }
                                }
                            }
                            else if(stationCapacityUpgrade == null)
                            {
                                int upgradeIndex = recommendationSetting.StationTargetUpgradeIndex;
                                StationUpgrade stationDurationUpgrade = recommendationSetting.DataStaion.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
                                if (stationDurationUpgrade != null &&
                                    stationDurationUpgrade.Upgrade.Count > upgradeIndex &&
                                    stationDurationUpgrade.CurrentUpgradeIndex == upgradeIndex - 1)
                                {
                                    StationUpgradePanelUI stationUpgradePanelUI = stationUpgradePanelsUI.Find((x) => x.DataStation == recommendationSetting.DataStaion && x.StationUpgradeType == recommendationSetting.StationUpgradeType);
                                    if (stationUpgradePanelUI != null)
                                    {
                                        stationUpgradePanelUI.OnRecommendation?.Invoke();
                                        stationUpgradePanelUI.IsRecommended = true;
                                        if (shouldScrollFocus &&
                                        stationUpgradePanelUI.TryGetComponent(out RectTransform target))
                                        {
                                            upgradePopupUIManager.TryScrollTo(target, 0);
                                        }
                                        return;
                                    }
                                }
                            }
                        }
                        if (recommendationSetting.StationUpgradeType == StationUpgradeType.StationUpgradeType2)
                        {
                            StationUpgrade stationCostUpgrade = recommendationSetting.DataStaion.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
                            if (stationCostUpgrade != null &&
                                stationCostUpgrade.Upgrade.Count > recommendationSetting.StationTargetUpgradeIndex &&
                                stationCostUpgrade.CurrentUpgradeIndex == recommendationSetting.StationTargetUpgradeIndex - 1)
                            {
                                StationUpgradePanelUI stationUpgradePanelUI = stationUpgradePanelsUI.Find((x) => x.DataStation == recommendationSetting.DataStaion && x.StationUpgradeType == recommendationSetting.StationUpgradeType);
                                if (stationUpgradePanelUI != null)
                                {
                                    stationUpgradePanelUI.OnRecommendation?.Invoke();
                                    stationUpgradePanelUI.IsRecommended = true;

                                    if (shouldScrollFocus &&
                                    stationUpgradePanelUI.TryGetComponent(out RectTransform target))
                                    {
                                        upgradePopupUIManager.TryScrollTo(target, 0);
                                    }
                                    return;
                                }
                            }
                        }
                    }
                    else if (recommendationSetting.RecommendationType == UgradeRecommendationType.Patience)
                    {
                        if (recommendationSetting.PatienceTargetUpgradeIndex == 0 && !recommendationSetting.DataPatience.PatienceData.IsUnlocked)
                        {
                            PatienceUpgradePanelUI patienceUpgradePanelUI = patienceUpgradePanelsUI.Find((x) => x.DataPatience == recommendationSetting.DataPatience);
                            if (patienceUpgradePanelUI != null)
                            {
                                patienceUpgradePanelUI.OnRecommendation?.Invoke();
                                patienceUpgradePanelUI.IsRecommended = true;
                                if (shouldScrollFocus &&
                                    patienceUpgradePanelUI.TryGetComponent(out RectTransform target))
                                {
                                    upgradePopupUIManager.TryScrollTo(target, 0);
                                }
                                return;
                            }
                        }
                        else if (recommendationSetting.DataPatience.PatienceData.PatienceUpgrade.Count > recommendationSetting.PatienceTargetUpgradeIndex &&
                            recommendationSetting.DataPatience.PatienceData.CurrentUpgradeIndex == recommendationSetting.PatienceTargetUpgradeIndex - 1)
                        {
                            PatienceUpgradePanelUI patienceUpgradePanelUI = patienceUpgradePanelsUI.Find((x) => x.DataPatience == recommendationSetting.DataPatience);
                            if (patienceUpgradePanelUI != null)
                            {
                                patienceUpgradePanelUI.OnRecommendation?.Invoke();
                                patienceUpgradePanelUI.IsRecommended = true;

                                if (shouldScrollFocus &&
                                    patienceUpgradePanelUI.TryGetComponent(out RectTransform target))
                                {
                                    upgradePopupUIManager.TryScrollTo(target, 0);
                                }
                                return;
                            }
                        }
                    }
                    /* else if (recommendationSetting.RecommendationType == UgradeRecommendationType.PurchaseOffer)
                    {
                        PurchaseOfferPanelUI purchaseOfferPanelUI = purchaseOfferPanelsUI.Find((x) => x.PackageType == recommendationSetting.PackageType);
                        if (purchaseOfferPanelUI != null)
                        {
                            Debug.Log("Yayyaao");
                            purchaseOfferPanelUI.OnRecommendation?.Invoke();
                            purchaseOfferPanelUI.IsRecommended = true;

                            if (shouldScrollFocus &&
                                purchaseOfferPanelUI.TryGetComponent(out RectTransform target))
                            {
                                upgradePopupUIManager.TryScrollTo(target, 0);
                            }
                            return;
                        }
                    } */
                }
            }
            else
            {
                Debug.LogWarning("No upgrade recommendation found for level " + level);
            }

            Debug.LogWarning("No upgrade recommendation setting found for level " + level);
        }


        [Serializable]
        private class UpgradeRecommendationData
        {
            [Header("-Setting are applied to these level ranges (Inclusive)")]
            public int LevelFrom;
            public int LevelTo;

            [Header("-Upgrade priority is decneding as the list gose on")]
            public List<UgradeRecommendationSetting> RecommendationSettings;
        }

        [Serializable]
        private class UgradeRecommendationSetting
        {
            public UgradeRecommendationType RecommendationType;

            //----Player----
            [AllowNesting, ShowIf(nameof(RecommendationType), UgradeRecommendationType.Player)]
            public DataPlayer DataPlayer;
            [AllowNesting, ShowIf(nameof(RecommendationType), UgradeRecommendationType.Player)]
            public PlayerUpgradeType PlayerUpgradeType;
            [AllowNesting, ShowIf(nameof(RecommendationType), UgradeRecommendationType.Player)]
            public int PlayerTargetUpgradeIndex;

            //----Chair----
            [AllowNesting, ShowIf(nameof(RecommendationType), UgradeRecommendationType.Chair),
                Header("-Put in the order of chairs getting unlocked")]
            public UgradeRecommendationChairInfo ChairInfo;
            [AllowNesting, ShowIf(nameof(RecommendationType), UgradeRecommendationType.Chair)]
            public int ChairTargetUpgradeIndex;

            //----Worker----
            [AllowNesting, ShowIf(nameof(RecommendationType), UgradeRecommendationType.Worker)]
            public DataWorker DataWorker;
            [AllowNesting, ShowIf(nameof(RecommendationType), UgradeRecommendationType.Worker)]
            public int WorkerTargetLevelDifficulty;

            //----Staion----
            [AllowNesting, ShowIf(nameof(RecommendationType), UgradeRecommendationType.Station)]
            public DataStation DataStaion;
            [AllowNesting, ShowIf(nameof(RecommendationType), UgradeRecommendationType.Station)]
            public StationUpgradeType StationUpgradeType;
            [AllowNesting, ShowIf(nameof(RecommendationType), UgradeRecommendationType.Station)]
            public int StationTargetUpgradeIndex;

            //----Patience----
            [AllowNesting, ShowIf(nameof(RecommendationType), UgradeRecommendationType.Patience)]
            public DataPatience DataPatience;
            [AllowNesting, ShowIf(nameof(RecommendationType), UgradeRecommendationType.Patience)]
            public int PatienceTargetUpgradeIndex;

            [AllowNesting, ShowIf(nameof(RecommendationType), UgradeRecommendationType.PurchaseOffer)]
            public Arc.PurchasingPackageType PackageType;
        }

        [Serializable]
        private class UgradeRecommendationChairInfo
        {
            public List<DataSalonChair> DataChairs;
        }

        private enum UgradeRecommendationType
        {
            Player, Chair, Worker, Station, Patience, PurchaseOffer
        }
    }

}
