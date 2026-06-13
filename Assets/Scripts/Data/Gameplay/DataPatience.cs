using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using UnityEngine;
using NaughtyAttributes;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataPatience", menuName = "GameData/DataPatience")]
    public class DataPatience : DataSaver
    {
        public PatienceData PatienceData => m_PatienceData;
        [SerializeField] PatienceData m_PatienceData;

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
            m_PatienceData.IsUnlocked = m_PatienceData.IsUnlockedDefaultValue;
            m_PatienceData.HasJustUnlocked = m_PatienceData.HasJustUnlockedDefaultValue;
            m_PatienceData.HasUpgraded = m_PatienceData.HasUpgradedDefaultValue;

            m_PatienceData.CurrentUpgradeIndex = 0;
        }

        [ContextMenu("Save Data")]
        public void Save()
        {
            SaveData(m_PatienceData);
        }

        [ContextMenu("Load Data")]
        public void Load()
        {
            PatienceData patienceData = LoadData<PatienceData>();
            if (patienceData != null)
            {
                m_PatienceData.IsUnlocked = patienceData.IsUnlocked;
                m_PatienceData.HasJustUnlocked = patienceData.HasJustUnlocked;
                m_PatienceData.HasUpgraded = patienceData.HasUpgraded;

                m_PatienceData.CurrentUpgradeIndex = patienceData.CurrentUpgradeIndex;
            }
        }

    }

    [Serializable]
    public class PatienceData
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

        [XmlIgnore]
        public bool EditUpgradeIndex;
        [AllowNesting, EnableIf(nameof(EditUpgradeIndex))]
        public int CurrentUpgradeIndex;

        [XmlIgnore]
        public List<float> PatienceUpgrade;
    }

}
