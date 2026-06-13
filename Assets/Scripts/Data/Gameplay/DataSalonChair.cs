using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using UnityEngine;
using NaughtyAttributes;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataSalonChair", menuName = "GameData/DataSalonChair")]

    public class DataSalonChair : DataSaver
    {
        public SalonChairData SalonChairData => m_SalonChairData;
        [SerializeField] SalonChairData m_SalonChairData;

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
            m_SalonChairData.IsUnlocked = m_SalonChairData.IsUnlockedDefaultValue;
            m_SalonChairData.HasJustUnlocked = m_SalonChairData.HasJustUnlockedDefaultValue;
            m_SalonChairData.IsAutoCleaningUnlocked = m_SalonChairData.IsAutoCleaningUnlockedDefaultValue;
        }

        [ContextMenu("Save Data")]
        public void Save()
        {
            SaveData(m_SalonChairData);
        }

        [ContextMenu("Load Data")]
        public void Load()
        {
            SalonChairData chair = LoadData<SalonChairData>();
            if (chair != null)
            {
                m_SalonChairData.IsUnlocked = chair.IsUnlocked;
                m_SalonChairData.HasJustUnlocked = chair.HasJustUnlocked;
                m_SalonChairData.IsAutoCleaningUnlocked = chair.IsAutoCleaningUnlocked;
            }
        }
    }

    [Serializable]
    public class SalonChairData
    {
        public bool IsUnlocked;
        public bool HasJustUnlocked;
        public bool IsAutoCleaningUnlocked;

        [Space, Header("---Default Values---")]
        [XmlIgnore]
        public bool EditDefaultValues;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public bool IsUnlockedDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public bool HasJustUnlockedDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public bool IsAutoCleaningUnlockedDefaultValue;
    }
}
