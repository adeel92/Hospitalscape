using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using UnityEngine;
using NaughtyAttributes;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataEnvironmentDecoration", menuName = "GameData/DataEnvironmentDecoration")]
    public class DataEnvironmentDecoration : DataSaver
    {
        public EnvironmentDecorationData EnvironmentDecorationData => m_EnvironmentDecoration;
        [SerializeField] EnvironmentDecorationData m_EnvironmentDecoration;

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
            m_EnvironmentDecoration.IsUnlocked = m_EnvironmentDecoration.IsUnlockedDefaultValue;
            m_EnvironmentDecoration.HasJustUnlocked = m_EnvironmentDecoration.HasJustUnlockedDefaultValue;
        }

        [ContextMenu("Save Data")]
        public void Save()
        {
            SaveData(m_EnvironmentDecoration);
        }

        [ContextMenu("Load Data")]
        public void Load()
        {
            EnvironmentDecorationData decorationData = LoadData<EnvironmentDecorationData>();
            if (decorationData != null)
            {
                m_EnvironmentDecoration.IsUnlocked = decorationData.IsUnlocked;
                m_EnvironmentDecoration.HasJustUnlocked = decorationData.HasJustUnlocked;
            }
        }

    }

    [Serializable]
    public class EnvironmentDecorationData
    {
        [Header("---Unlocking---")]
        public bool IsUnlocked;
        public bool HasJustUnlocked;

        [Space, Header("---Default Values---")]
        [XmlIgnore]
        public bool EditDefaultValues;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public bool IsUnlockedDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public bool HasJustUnlockedDefaultValue;
    }
}
