using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using UnityEngine;
using NaughtyAttributes;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataMap", menuName = "GameData/DataMap")]
    public class DataMap : DataSaver
    {
        public MapData MapData => m_MapData;
        [SerializeField] MapData m_MapData;
        [Space]
        [SerializeField] List<DataLevel> m_DataLevels;
        [Space]
        [SerializeField, Expandable] List<DataPlayer> m_DataPlayers;
        [Space]
        [SerializeField, Expandable] List<DataWorker> m_DataWorkers;
        [Space]
        [SerializeField, Expandable] List<DataStation> m_DataStaions;
        [Space]
        [SerializeField, Expandable] List<DataSalonChair> m_DataSalonChairs;
        [Space]
        [SerializeField, Expandable] List<DataCafeChair> m_DataCafeChairs;
        [Space]
        [SerializeField, Expandable] List<DataEnvironmentDecoration> m_DataEnvironmentDecorations;
        [Space]
        [SerializeField, Expandable] List<DataPatience> m_DataPatiences;
        [Space]
        [SerializeField, Expandable] DataProfile m_DataProfile;

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

            foreach (var dataPlayer in m_DataPlayers)
            {
                dataPlayer.Setup();
            }

            foreach (var dataWorker in m_DataWorkers)
            {
                dataWorker.Setup();
            }

            foreach (var dataStation in m_DataStaions)
            {
                dataStation.Setup();
            }

            foreach (var dataTable in m_DataSalonChairs)
            {
                dataTable.Setup();
            }

            foreach (var dataTable in m_DataCafeChairs)
            {
                dataTable.Setup();
            }

            foreach (var dataEnvironmentDecorations in m_DataEnvironmentDecorations)
            {
                dataEnvironmentDecorations.Setup();
            }

            foreach (var dataPatience in m_DataPatiences)
            {
                dataPatience.Setup();
            }

            m_DataProfile.Setup();
        }

        public override void SetDataToDefault()
        {
            m_MapData.IsUnlocked = m_MapData.IsUnlockedDefaultValue;
            m_MapData.LevelIndex = m_MapData.LevelIndexDefaultValue;
        }

        public DataLevel GetCurrentDataLevel()
        {
            if (m_DataLevels.Count > m_MapData.LevelIndex && m_MapData.LevelIndex >= 0)
            {
                return m_DataLevels[m_MapData.LevelIndex];
            }
            else
            {
                return null;
            }
        }

        public int GetLevelIndex()
        {
            return m_MapData.LevelIndex;
        }

        /// <summary>
        /// Returns if level index was increased successfuly
        /// </summary>
        public bool IncreaseLevelIndex()
        {
            if (m_MapData.LevelIndex < m_DataLevels.Count)
            {
                m_MapData.LevelIndex++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetTotalLevels()
        {
            return m_DataLevels.Count;
        }

        public int GetLevelDifficulty()
        {
            if (m_DataLevels.Count > GetLevelIndex() && GetLevelIndex() >= 0)
            {
                return m_DataLevels[GetLevelIndex()].LevelDifficulty;
            }
            else if(m_DataLevels.Count <= GetLevelIndex() && m_DataLevels.Count > 0)
            {
                return m_DataLevels[m_DataLevels.Count - 1].LevelDifficulty;
            }
            else
            {
                Debug.LogWarning("LevelIndex out of range");
                return 0;
            }
        }

        [ContextMenu("Save Data")]
        public void Save()
        {
            SaveData(MapData);
        }

        public void SaveSubData()
        {
            foreach (var dataPlayer in m_DataPlayers)
            {
                dataPlayer.Save();
            }

            foreach (var dataWorker in m_DataWorkers)
            {
                dataWorker.Save();
            }

            foreach (var dataStation in m_DataStaions)
            {
                dataStation.Save();
            }

            foreach (var dataTable in m_DataSalonChairs)
            {
                dataTable.Save();
            }

            foreach (var dataTable in m_DataCafeChairs)
            {
                dataTable.Save();
            }

            foreach (var dataEnvironmentDecorations in m_DataEnvironmentDecorations)
            {
                dataEnvironmentDecorations.Save();
            }

            foreach (var dataPatience in m_DataPatiences)
            {
                dataPatience.Save();
            }

        }

        [ContextMenu("Load Data")]
        public void Load()
        {
            MapData mapData = LoadData<MapData>();
            if (mapData != null)
            {
                m_MapData.IsUnlocked = mapData.IsUnlocked;
                m_MapData.LevelIndex = mapData.LevelIndex;
            }
        }
    }

    [Serializable]
    public class MapData
    {
        [Space, Header("---Values---")]
        [XmlIgnore]
        public bool EditValues;
        [AllowNesting, EnableIf(nameof(EditValues))]
        public bool IsUnlocked;
        [AllowNesting, EnableIf(nameof(EditValues))]
        public int LevelIndex;

        [Space, Header("---Default Values---")]
        [XmlIgnore]
        public bool EditDefaultValues;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public bool IsUnlockedDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public int LevelIndexDefaultValue;
    }
}
