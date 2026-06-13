using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using UnityEngine;
using NaughtyAttributes;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataPlayer", menuName = "GameData/DataPlayer")]
    public class DataPlayer : DataSaver
    {
        public PlayerData PlayerData => m_PlayerData;
        [SerializeField] PlayerData m_PlayerData;

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
            m_PlayerData.IsUpgraded = false;
            m_PlayerData.CurrentCapacityIndex = 0;
            m_PlayerData.CurrentWalkSpeedIndex = 0;
        }

        [ContextMenu("Save Data")]
        public void Save()
        {
            SaveData(m_PlayerData);
        }

        [ContextMenu("Load Data")]
        public void Load()
        {
            PlayerData player = LoadData<PlayerData>();
            if (player != null)
            {
                m_PlayerData.IsUpgraded = player.IsUpgraded;
                m_PlayerData.CurrentCapacityIndex = player.CurrentCapacityIndex;
                m_PlayerData.CurrentWalkSpeedIndex = player.CurrentWalkSpeedIndex;
            }
        }
    }
    
    [Serializable]
    public class PlayerData
    {
        public bool IsUpgraded;

        [Space]
        public bool EditCapacityIndex;
        [AllowNesting, EnableIf(nameof(EditCapacityIndex))]
        public int CurrentCapacityIndex;
        [XmlIgnore]
        public List<int> Capacity;

        public bool EditSpeedIndex;
        [AllowNesting, EnableIf(nameof(EditSpeedIndex))]
        public int CurrentWalkSpeedIndex;
        [XmlIgnore]
        public List<float> WalkSpeed;
    }
}