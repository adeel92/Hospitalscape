using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using UnityEngine;
using NaughtyAttributes;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataGame", menuName = "GameData/DataGame")]
    public class DataGame : DataSaver
    {
        public GameData GameData => m_GameData;
        [SerializeField] GameData m_GameData;

        [SerializeField] List<MapInfo> m_MapsInfo;



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

            SetCurrentDataMap();
        }

        public void SetCurrentDataMap()
        {
            MapInfo mapInfo = m_GameData.MapsInfo.Find((x) => x.Type == m_GameData.CurrentMapType);
            if (mapInfo != null)
            {
                mapInfo.DataMap.Setup();
            }
        }

        public override void SetDataToDefault()
        {
            m_GameData.CurrentMapType = m_GameData.CurrentMapTypeDefaultValue;
            foreach (var mapInfo in m_GameData.MapsInfo)
            {
                mapInfo.IsUnlocked = mapInfo.IsUnlockedDefaultValue;
            }

            m_GameData.CoinCurrency = m_GameData.CoinCurrencyDefaultValue;
            m_GameData.GemCurrency = m_GameData.GemCurrencyDefaultValue;
            m_GameData.StarCurrency = m_GameData.StarCurrencyDefaultValue;
            m_GameData.KeyCurrency = m_GameData.KeyCurrencyDefaultValue;
            m_GameData.HeartCurrency = m_GameData.HeartCurrencyDefaultValue;
            m_GameData.HeartTimeCurrency = m_GameData.HeartTimeCurrencyDefaultValue;

            m_GameData.GeneralData = new DataGeneral();
            m_GameData.GeneralData.GeneralInfo = new List<DataGeneralInfo>();

            m_GameData.InstanceOrderFillBooster = m_GameData.InstanceOrderFillBoosterDefaultValue;
            m_GameData.TimeFrozeBooster = m_GameData.TimeFrozeBoosterDefaultValue;
            m_GameData.WaitressSpeedBooster = m_GameData.WaitressSpeedBoosterDefaultValue;

            m_GameData.NoAdsPurchase = m_GameData.NoAdsPurchaseDefaultValue;
        }

        [ContextMenu("Save Data")]
        public void Save()
        {
            SaveData(m_GameData);
        }

        [ContextMenu("Load Data")]
        public void Load()
        {
            GameData gameData = LoadData<GameData>();
            if (gameData != null)
            {
                m_GameData.CurrentMapType = gameData.CurrentMapType;
                for (int i = 0; i < gameData.MapsInfo.Count && i < m_GameData.MapsInfo.Count; i++)
                {
                    m_GameData.MapsInfo[i].IsUnlocked = gameData.MapsInfo[i].IsUnlocked;
                }

                m_GameData.CoinCurrency = gameData.CoinCurrency;
                m_GameData.GemCurrency = gameData.GemCurrency;
                m_GameData.StarCurrency = gameData.StarCurrency;
                m_GameData.StarCurrency = gameData.StarCurrency;
                m_GameData.KeyCurrency = gameData.KeyCurrency;
                m_GameData.HeartCurrency = gameData.HeartCurrency;
                m_GameData.HeartTimeCurrency = gameData.HeartTimeCurrency;

                m_GameData.GeneralData = gameData.GeneralData;

                m_GameData.TimeFrozeBooster = gameData.TimeFrozeBooster;
                m_GameData.WaitressSpeedBooster = gameData.WaitressSpeedBooster;
                m_GameData.InstanceOrderFillBooster = gameData.InstanceOrderFillBooster;

                m_GameData.NoAdsPurchase = gameData.NoAdsPurchase;
            }
        }

        #region Map Related
        public DataMap GetCurrentDataMap()
        {
            MapInfo mapInfo = GameData.MapsInfo.Find((x) => x.Type == m_GameData.CurrentMapType);
            if (mapInfo != null)
            {
                return mapInfo.DataMap;
            }

            return null;
        }

        public string GetMapName(MapType mapType)
        {
            MapInfo mapInfo = GameData.MapsInfo.Find((x) => x.Type == mapType);
            if (mapInfo != null)
            {
                return mapInfo.MapName;
            }
            else
            {
                return "";
            }
        }

        public bool IsMapUnlocked(MapType mapType)
        {
            MapInfo mapInfo = GameData.MapsInfo.Find((x) => x.Type == mapType);
            if (mapInfo != null)
            {
                return mapInfo.IsUnlocked;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region General Data
        public void SetBool(string key, bool value)
        {
            if (m_GameData.GeneralData == null)
            {
                m_GameData.GeneralData = new DataGeneral();
                m_GameData.GeneralData.GeneralInfo = new List<DataGeneralInfo>();
            }

            if (m_GameData.GeneralData.GeneralInfo == null)
            {
                m_GameData.GeneralData.GeneralInfo = new List<DataGeneralInfo>();
            }

            DataGeneralInfo dataGeneralInfo = m_GameData.GeneralData.GeneralInfo.Find((x) => x.Key == key);
            if (dataGeneralInfo != null)
            {
                dataGeneralInfo.Value = value.ToString();
            }
            else
            {
                dataGeneralInfo = new DataGeneralInfo();
                dataGeneralInfo.Key = key;
                dataGeneralInfo.Value = value.ToString();

                m_GameData.GeneralData.GeneralInfo.Add(dataGeneralInfo);
            }

            Save();
        }

        public bool GetBool(string key, bool defaulValue)
        {
            if (m_GameData.GeneralData == null)
            {
                m_GameData.GeneralData = new DataGeneral();
                m_GameData.GeneralData.GeneralInfo = new List<DataGeneralInfo>();
            }

            if (m_GameData.GeneralData.GeneralInfo == null)
            {
                m_GameData.GeneralData.GeneralInfo = new List<DataGeneralInfo>();
            }

            DataGeneralInfo dataGeneralInfo = m_GameData.GeneralData.GeneralInfo.Find((x) => x.Key == key);

            if (dataGeneralInfo != null)
            {
                if (bool.TryParse(dataGeneralInfo.Value, out bool value))
                {
                    return value;
                }
                else
                {
                    return false;
                }
            }
            else
            {

                dataGeneralInfo = new DataGeneralInfo();
                dataGeneralInfo.Key = key;
                dataGeneralInfo.Value = defaulValue.ToString();

                m_GameData.GeneralData.GeneralInfo.Add(dataGeneralInfo);

                return defaulValue;
            }

        }

        public void SetString(string key, string value)
        {
            if (m_GameData.GeneralData == null)
            {
                m_GameData.GeneralData = new DataGeneral();
                m_GameData.GeneralData.GeneralInfo = new List<DataGeneralInfo>();
            }

            if (m_GameData.GeneralData.GeneralInfo == null)
            {
                m_GameData.GeneralData.GeneralInfo = new List<DataGeneralInfo>();
            }

            DataGeneralInfo dataGeneralInfo = m_GameData.GeneralData.GeneralInfo.Find((x) => x.Key == key);
            if (dataGeneralInfo != null)
            {
                dataGeneralInfo.Value = value;
            }
            else
            {
                dataGeneralInfo = new DataGeneralInfo();
                dataGeneralInfo.Key = key;
                dataGeneralInfo.Value = value;

                m_GameData.GeneralData.GeneralInfo.Add(dataGeneralInfo);
            }

            Save();
        }

        public string GetString(string key, string defaulValue)
        {
            if (m_GameData.GeneralData == null)
            {
                m_GameData.GeneralData = new DataGeneral();
                m_GameData.GeneralData.GeneralInfo = new List<DataGeneralInfo>();
            }

            if (m_GameData.GeneralData.GeneralInfo == null)
            {
                m_GameData.GeneralData.GeneralInfo = new List<DataGeneralInfo>();
            }

            DataGeneralInfo dataGeneralInfo = m_GameData.GeneralData.GeneralInfo.Find((x) => x.Key == key);

            if (dataGeneralInfo != null)
            {
                return dataGeneralInfo.Value;
            }
            else
            {
                dataGeneralInfo = new DataGeneralInfo();
                dataGeneralInfo.Key = key;
                dataGeneralInfo.Value = defaulValue.ToString();

                m_GameData.GeneralData.GeneralInfo.Add(dataGeneralInfo);

                return defaulValue;
            }

        }

        public void SetInt(string key, int value)
        {
            if (m_GameData.GeneralData == null)
            {
                m_GameData.GeneralData = new DataGeneral();
                m_GameData.GeneralData.GeneralInfo = new List<DataGeneralInfo>();
            }

            if (m_GameData.GeneralData.GeneralInfo == null)
            {
                m_GameData.GeneralData.GeneralInfo = new List<DataGeneralInfo>();
            }

            DataGeneralInfo dataGeneralInfo = m_GameData.GeneralData.GeneralInfo.Find((x) => x.Key == key);
            if (dataGeneralInfo != null)
            {
                dataGeneralInfo.Value = value.ToString();
            }
            else
            {
                dataGeneralInfo = new DataGeneralInfo();
                dataGeneralInfo.Key = key;
                dataGeneralInfo.Value = value.ToString();

                m_GameData.GeneralData.GeneralInfo.Add(dataGeneralInfo);
            }

            Save();
        }

        public int GetInt(string key, int defaulValue)
        {
            if (m_GameData.GeneralData == null)
            {
                m_GameData.GeneralData = new DataGeneral();
                m_GameData.GeneralData.GeneralInfo = new List<DataGeneralInfo>();
            }

            if (m_GameData.GeneralData.GeneralInfo == null)
            {
                m_GameData.GeneralData.GeneralInfo = new List<DataGeneralInfo>();
            }

            DataGeneralInfo dataGeneralInfo = m_GameData.GeneralData.GeneralInfo.Find((x) => x.Key == key);

            if (dataGeneralInfo != null)
            {
                if (int.TryParse(dataGeneralInfo.Value, out int value))
                {
                    return value;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                dataGeneralInfo = new DataGeneralInfo();
                dataGeneralInfo.Key = key;
                dataGeneralInfo.Value = defaulValue.ToString();

                m_GameData.GeneralData.GeneralInfo.Add(dataGeneralInfo);

                return defaulValue;
            }

        }
        #endregion
    }



    [Serializable]
    public class GameData
    {
        [Space, Header("---Values---")]
        [XmlIgnore]
        public bool EditValues;
        [AllowNesting, EnableIf(nameof(EditValues))]
        public int CoinCurrency;
        [AllowNesting, EnableIf(nameof(EditValues))]
        public int GemCurrency;
        [AllowNesting, EnableIf(nameof(EditValues))]
        public int StarCurrency;
        [AllowNesting, EnableIf(nameof(EditValues))]
        public int KeyCurrency;
        [AllowNesting, EnableIf(nameof(EditValues))]
        public int HeartCurrency;
        [Tooltip("Differnce between HeartCurrency and HeartTimeCurrency is HeartTimeCurrency can be purshed. " +
            "Also for that time the HeartCurrency is going to be unimited. Unit (Seconds)"),
            AllowNesting, EnableIf(nameof(EditValues))]
        public double HeartTimeCurrency;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditValues))]
        public int HeartRefillTime;

        [Space, Header("---Map---")]
        [AllowNesting, EnableIf(nameof(EditValues))]
        public MapType CurrentMapType;
        public List<MapInfo> MapsInfo;

        [Space, Header("---Booster---")]
        [AllowNesting, EnableIf(nameof(EditValues))]
        public int TimeFrozeBooster;
        [AllowNesting, EnableIf(nameof(EditValues))]
        public int WaitressSpeedBooster;
        [AllowNesting, EnableIf(nameof(EditValues))]
        public int InstanceOrderFillBooster;

        [Space, Header("---Purchase---")]
        public bool NoAdsPurchase;

        [Space, Header("---General Data (Similar to PlayerPrefs)---")]
        [XmlIgnore]
        public bool EditGeneralData;
        [AllowNesting, EnableIf(nameof(EditGeneralData)), Space]
        public DataGeneral GeneralData;

        [Space, Header("---Default Values---")]
        [XmlIgnore]
        public bool EditDefaultValues;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public int CoinCurrencyDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public int GemCurrencyDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public int StarCurrencyDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public int KeyCurrencyDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public int HeartCurrencyDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public double HeartTimeCurrencyDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public MapType CurrentMapTypeDefaultValue;

        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues)), Space]
        public int TimeFrozeBoosterDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public int WaitressSpeedBoosterDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public int InstanceOrderFillBoosterDefaultValue;

        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValues))]
        public bool NoAdsPurchaseDefaultValue;
    }

    [Serializable]
    public class MapInfo
    {
        public MapType Type;
        public bool IsUnlocked;
        [XmlIgnore]
        public string MapName;
        [XmlIgnore, AllowNesting, Expandable]
        public DataMap DataMap;

        [XmlIgnore]
        public bool EditDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValue))]
        public bool IsUnlockedDefaultValue;
    }

    [Serializable]
    public class DataGeneral
    {
        public List<DataGeneralInfo> GeneralInfo;
    }

    [Serializable]
    public class DataGeneralInfo
    {
        public string Key;
        public string Value;
    }
}
