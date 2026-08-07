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
            m_EnvironmentDecoration.CurrentDesignIndex = m_EnvironmentDecoration.CurrentDesignIndexDefaultValue;
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
        public int CurrentDesignIndex;
        [XmlIgnore]
        [Space] public List<DecorationDesignInfo> DecorationDesignInfos = new();

        [Space, Header("---Default Values---")]
        [XmlIgnore]
        public bool EditDefaultValues;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public bool IsUnlockedDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public bool HasJustUnlockedDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public int CurrentDesignIndexDefaultValue;
    }

    [Serializable]
    public class DecorationDesignInfo
    {
        public Sprite UISprite;
        public List<DecorationSubItemInfo> DecorationSubItemInfos = new();
    }

    [Serializable]
    public class DecorationSubItemInfo
    {
        public DecorationSubItem SubItemKey;
        public Sprite EnvironmentSprite;
    }

    public enum DecorationSubItem
    {
        Item1,
        Item2,
        Item3,
        Item4,
        Item5,
        Item6,    
    }
}
