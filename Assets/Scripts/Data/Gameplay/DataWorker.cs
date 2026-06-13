using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using UnityEngine;
using NaughtyAttributes;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataWorker", menuName = "GameData/DataWorker")]
    public class DataWorker : DataSaver
    {
        public WorkerOrderData WorkerOrderData => m_WorkerOrderData;
        [SerializeField] WorkerOrderData m_WorkerOrderData;

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
            m_WorkerOrderData.IsUnlocked = m_WorkerOrderData.IsUnlockedDefaultValue;
            m_WorkerOrderData.HasJustUnlocked = m_WorkerOrderData.HasJustUnlockedDefaultValue;
            m_WorkerOrderData.HasJustUpgraded = m_WorkerOrderData.HasJustUpgradedDefaultValue;
            m_WorkerOrderData.WorkerQunaityUpgradeIndex = m_WorkerOrderData.WorkerQunaityUpgradeIndexDefaultValue;
            m_WorkerOrderData.WorkerCurrentLevelDifficulty = m_WorkerOrderData.WorkerCurrentLevelDifficultyDefaultValue;
        }

        [ContextMenu("Save Data")]
        public void Save()
        {
            SaveData(m_WorkerOrderData);
        }

        [ContextMenu("Load Data")]
        public void Load()
        {
            WorkerOrderData workerOrderData = LoadData<WorkerOrderData>();
            if (workerOrderData != null)
            {
                m_WorkerOrderData.IsUnlocked = workerOrderData.IsUnlocked;
                m_WorkerOrderData.HasJustUnlocked = workerOrderData.HasJustUnlocked;
                m_WorkerOrderData.HasJustUpgraded = workerOrderData.HasJustUpgraded;
                m_WorkerOrderData.WorkerQunaityUpgradeIndex = workerOrderData.WorkerQunaityUpgradeIndex;
                m_WorkerOrderData.WorkerCurrentLevelDifficulty = workerOrderData.WorkerCurrentLevelDifficulty;
            }
        }
    }

    [Serializable]
    public class WorkerOrderData
    {
        public bool IsUnlocked;
        public bool HasJustUnlocked;
        public bool HasJustUpgraded;

        [XmlIgnore]
        public int OrderCost;

        [Space]
        public bool EditWorkerQunaityUpgradeIndex;
        [AllowNesting, EnableIf(nameof(EditWorkerQunaityUpgradeIndex))]
        public int WorkerQunaityUpgradeIndex;
        [XmlIgnore]
        public List<WorkerQunaityUpgrade> WorkerQunaityUpgrades;

        [Space]
        //Workers Current Difficuty (This is going to compeared to Current
        //Level Difficult and going to select the duration based on the differnce)
        public bool EditWorkerCurrentLevelDifficulty;
        [AllowNesting, EnableIf(nameof(EditWorkerCurrentLevelDifficulty))]
        public int WorkerCurrentLevelDifficulty;
        [XmlIgnore]
        public List<WorkerServingDurationUpgrade> WorkerServingDurationUpgrades;

        [Space, Header("---Default Values---")]
        [XmlIgnore]
        public bool EditDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValue))]
        public bool IsUnlockedDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValue))]
        public bool HasJustUnlockedDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValue))]
        public bool HasJustUpgradedDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValue))]
        public int WorkerQunaityUpgradeIndexDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValue))]
        public int WorkerCurrentLevelDifficultyDefaultValue;

    }

    [Serializable]
    public class WorkerQunaityUpgrade
    {
        public int Quantity;
    }

    [Serializable] 
    public class WorkerServingDurationUpgrade
    {
        public int LevelDifficultyRangeMin;
        public int LevelDifficultyRangeMax;
        [Header("-Apply this duration in the Level Difficuty Range using the differnce"), NonReorderable]
        public List<LevelDifficultyServingDuration> LevelDifficultyServingDurations;
    }

    [Serializable]
    public class LevelDifficultyServingDuration
    {
        //public int LevelDifficultyDiffernce;
        public float WorkerServingDuration;
    }
}
