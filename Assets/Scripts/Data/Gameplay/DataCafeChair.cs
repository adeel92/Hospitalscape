using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using UnityEngine;
using NaughtyAttributes;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataCafeChair", menuName = "GameData/DataCafeChair")]
    public class DataCafeChair : DataSaver
    {
        public CafeChairData CafeChairData => m_CafeChairData;
        [SerializeField] CafeChairData m_CafeChairData;

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
            m_CafeChairData.IsUnlocked = m_CafeChairData.IsUnlockedDefaultValue;
            m_CafeChairData.HasJustUnlocked = m_CafeChairData.HasJustUnlockedDefaultValue;
            m_CafeChairData.IsAutoCleaningUnlocked = m_CafeChairData.IsAutoCleaningUnlockedDefaultValue;
        }

        [ContextMenu("Save Data")]
        public void Save()
        {
            SaveData(m_CafeChairData);
        }

        [ContextMenu("Load Data")]
        public void Load()
        {
            CafeChairData chair = LoadData<CafeChairData>();
            if (chair != null)
            {
                m_CafeChairData.IsUnlocked = chair.IsUnlocked;
                m_CafeChairData.HasJustUnlocked = chair.HasJustUnlocked;
                m_CafeChairData.IsAutoCleaningUnlocked = chair.IsAutoCleaningUnlocked;
            }
        }
    }

    [Serializable]
    public class CafeChairData
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
