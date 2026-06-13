using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using UnityEngine;
using NaughtyAttributes;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataStation", menuName = "GameData/DataStation")]
    public class DataStation : DataSaver
    {
        public StationData StationData => m_Station;
        [SerializeField] StationData m_Station;

        public override void Setup()
        {
            if (FileExists())
            {
                Load();
                Save();
            }
            else
            {
                SetDataToDefault();
                Save();
            }
        }


        public override void SetDataToDefault()
        {
            m_Station.IsUnlocked = m_Station.IsUnlockedDefaultValue;
            m_Station.HasJustUnlocked = m_Station.HasJustUnlockedDefaultValue;
            m_Station.HasUpgraded = m_Station.HasUpgradedDefaultValue;
            foreach (var upgrade in m_Station.Upgrades)
            {
                upgrade.EditUpgradeIndex = false;
                // upgrade.CurrentUpgradeIndex = 0; //mrcHefF
            }
        }

        public StationUpgrade GetStationUpgrade(PropertyUpgradeType upgradeType)
        {
            StationUpgrade stationUpgrade = m_Station.Upgrades.Find(x => x.UpgradeType == upgradeType);
            return stationUpgrade;
        }

        [ContextMenu("Save Data")]
        public void Save()
        {
            SaveData(m_Station);
        }

        [ContextMenu("Load Data")]
        public void Load()
        {
            StationData station = LoadData<StationData>();
            if(station != null)
            {
                m_Station.IsUnlocked = station.IsUnlocked;
                m_Station.HasJustUnlocked = station.HasJustUnlocked;
                m_Station.HasUpgraded = station.HasUpgraded;

                for (int i = 0; i < m_Station.Upgrades.Count || station.Upgrades.Count < i; i++)
                {
                    m_Station.Upgrades[i].UpgradeType = station.Upgrades[i].UpgradeType;
                    m_Station.Upgrades[i].CurrentUpgradeIndex = station.Upgrades[i].CurrentUpgradeIndex;
                }
            }
        }
    }


    [Serializable]
    public class StationData
    {
        [Header("---Unlocking---")]
        public bool IsUnlocked;
        public bool HasJustUnlocked;
        public bool HasUpgraded;

        [Space, Header("---Default Values---")]
        [XmlIgnore]
        public bool EditDefaultValues;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public bool IsUnlockedDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public bool HasJustUnlockedDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public bool HasUpgradedDefaultValue;

        [Space, NonReorderable]
        public List<StationUpgrade> Upgrades;
    }

    [Serializable]
    public class StationUpgrade
    {
        public PropertyUpgradeType UpgradeType;
        [XmlIgnore]
        public bool EditUpgradeIndex;
        [AllowNesting, EnableIf(nameof(EditUpgradeIndex))]
        public int CurrentUpgradeIndex;
        [XmlIgnore]
        public List<float> Upgrade;
    }

    public enum PropertyUpgradeType
    {
        Duration, Capacity, Cost
    }
}
