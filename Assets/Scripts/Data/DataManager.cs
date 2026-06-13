using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using NaughtyAttributes;

namespace Isometric.Data
{
    public class DataManager : MonoBehaviour
    {
        private static DataManager s_Instance;
        
        [SerializeField] DataGame m_DataGame;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
                Setup();    
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static void Setup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_DataGame.Setup();
        }

        #region Currency
        public static int StarCurrency
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance.m_DataGame.GameData.StarCurrency;
                }
                else
                {
                    PrintNullInstanceError();
                    return 0;
                }
            }
            set
            {
                if (s_Instance != null)
                {
                    s_Instance.m_DataGame.GameData.StarCurrency = value;
                    GlobalEventHolder.OnStarCurrencyUpdate?.Invoke(s_Instance.m_DataGame.GameData.StarCurrency);
                }
                else
                {
                    PrintNullInstanceError();
                }
            }
        }
        public static int CoinCurrency
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance.m_DataGame.GameData.CoinCurrency;
                }
                else
                {
                    PrintNullInstanceError();
                    return 0;
                }
            }
            set
            {
                if (s_Instance != null)
                {
                    s_Instance.m_DataGame.GameData.CoinCurrency = value;
                    GlobalEventHolder.OnCoinCurrencyUpdate?.Invoke(s_Instance.m_DataGame.GameData.CoinCurrency);
                }
                else
                {
                    PrintNullInstanceError();
                }
            }
        }
        public static int GemCurrency
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance.m_DataGame.GameData.GemCurrency;
                }
                else
                {
                    PrintNullInstanceError();
                    return 0;
                }
            }
            set
            {
                if (s_Instance != null)
                {
                    s_Instance.m_DataGame.GameData.GemCurrency = value;
                    GlobalEventHolder.OnGemCurrencyUpdate?.Invoke(s_Instance.m_DataGame.GameData.GemCurrency);
                }
                else
                {
                    PrintNullInstanceError();
                }
            }
        }

        public static int KeyCurrency
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance.m_DataGame.GameData.KeyCurrency;
                }
                else
                {
                    PrintNullInstanceError();
                    return 0;
                }
            }
            set
            {
                if (s_Instance != null)
                {
                    s_Instance.m_DataGame.GameData.KeyCurrency = value;
                    GlobalEventHolder.OnKeyCurrencyUpdate?.Invoke(s_Instance.m_DataGame.GameData.KeyCurrency);
                }
                else
                {
                    PrintNullInstanceError();
                }
            }
        }

        public static int HeartCurrency
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance.m_DataGame.GameData.HeartCurrency;
                }
                else
                {
                    PrintNullInstanceError();
                    return 0;
                }
            }
            set
            {
                if (s_Instance != null)
                {
                    s_Instance.m_DataGame.GameData.HeartCurrency = value;
                    GlobalEventHolder.OnHeartCurrencyUpdate?.Invoke(s_Instance.m_DataGame.GameData.HeartCurrency);
                }
                else
                {
                    PrintNullInstanceError();
                }
            }
        }

        public static double HeartTimeCurrency
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance.m_DataGame.GameData.HeartTimeCurrency;
                }
                else
                {
                    PrintNullInstanceError();
                    return 0;
                }
            }
            set
            {
                if (s_Instance != null)
                {
                    s_Instance.m_DataGame.GameData.HeartTimeCurrency = value;
                    GlobalEventHolder.OnHeartTimeCurrencyUpdate?.Invoke(s_Instance.m_DataGame.GameData.HeartTimeCurrency);
                }
                else
                {
                    PrintNullInstanceError();
                }
            }
        }

        public static int HeartCurrencyRefillTime
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance.m_DataGame.GameData.HeartRefillTime;
                }
                else
                {
                    PrintNullInstanceError();
                    return 0;
                }
            }
        }

        public static int HeartCurrencyMaxValue
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance.m_DataGame.GameData.HeartCurrencyDefaultValue;
                }
                else
                {
                    PrintNullInstanceError();
                    return 0;
                }
            }
        }
        #endregion

        #region Map Related
        public static void SetCurrentMap(MapType mapType)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_DataGame.GameData.CurrentMapType = mapType;
            s_Instance.m_DataGame.SetCurrentDataMap();
        }

        public static string GetMapName(MapType mapType)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return "";
            }

            return s_Instance.m_DataGame.GetMapName(mapType);
        }

        public static bool IsMapUnlocked(MapType mapType)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return false;
            }

            return s_Instance.m_DataGame.IsMapUnlocked(mapType);
        }
        #endregion

        #region Purchase
        public static bool NoAdsPurchase
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance.m_DataGame.GameData.NoAdsPurchase;
                }
                else
                {
                    PrintNullInstanceError();
                    return false;
                }
            }
            set
            {
                if (s_Instance != null)
                {
                    s_Instance.m_DataGame.GameData.NoAdsPurchase = value;
                }
                else
                {
                    PrintNullInstanceError();
                }
            }
        }

        #endregion

        #region Boosters
        public static int TimeFrozeBoosterCount
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance.m_DataGame.GameData.TimeFrozeBooster;
                }
                else
                {
                    PrintNullInstanceError();
                    return 0;
                }
            }
            set
            {
                if (s_Instance != null)
                {
                    s_Instance.m_DataGame.GameData.TimeFrozeBooster = value;
                }
                else
                {
                    PrintNullInstanceError();
                }
            }
        }

        public static int WaitressSpeedBoosterCount
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance.m_DataGame.GameData.WaitressSpeedBooster;
                }
                else
                {
                    PrintNullInstanceError();
                    return 0;
                }
            }
            set
            {
                if (s_Instance != null)
                {
                    s_Instance.m_DataGame.GameData.WaitressSpeedBooster = value;
                }
                else
                {
                    PrintNullInstanceError();
                }
            }
        }

        public static int InstanceOrderFillBoosterCount
        {
            get
            {
                if (s_Instance != null)
                {
                    return s_Instance.m_DataGame.GameData.InstanceOrderFillBooster;
                }
                else
                {
                    PrintNullInstanceError();
                    return 0;
                }
            }
            set
            {
                if (s_Instance != null)
                {
                    s_Instance.m_DataGame.GameData.InstanceOrderFillBooster = value;
                }
                else
                {
                    PrintNullInstanceError();
                }
            }
        }
        #endregion

        #region Level Related
        public static MapType CurrentMapType
        {
            get
            {
                if (s_Instance == null)
                {
                    PrintNullInstanceError();
                    return 0;
                }
                else if (s_Instance.m_DataGame.GameData == null)
                {
                    Debug.LogWarning("Current Map not found");
                    return 0;
                }

                return s_Instance.m_DataGame.GameData.CurrentMapType;
            }
        }

        public static int CurrentMapLevelIndex 
        { 
            get
            {
                if (s_Instance == null)
                {
                    PrintNullInstanceError();
                    return 0;
                }
                else if (s_Instance.m_DataGame.GetCurrentDataMap() == null)
                {
                    Debug.LogWarning("Current Map not found");
                    return 0;
                }
                
                return s_Instance.m_DataGame.GetCurrentDataMap().GetLevelIndex();
            } 
        }

        public static int CurrentMapTotalLevels
        {
            get
            {
                if (s_Instance == null)
                {
                    PrintNullInstanceError();
                    return 0;
                }
                else if (s_Instance.m_DataGame.GetCurrentDataMap() == null)
                {
                    Debug.LogWarning("Current Map not found");
                    return 0;
                }

                return s_Instance.m_DataGame.GetCurrentDataMap().GetTotalLevels();
            }
        }

        public static int CurrentMapLevelDifficulty
        {
            get
            {
                if (s_Instance == null)
                {
                    PrintNullInstanceError();
                    return 0;
                }
                else if(s_Instance.m_DataGame.GetCurrentDataMap() == null)
                {
                    Debug.LogWarning("Current Map not found");
                    return 0;
                }

                return s_Instance.m_DataGame.GetCurrentDataMap().GetLevelDifficulty();
            }
        }

        public static DataLevel GetCurrentDataLevel()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return null;
            }
            else if (s_Instance.m_DataGame.GetCurrentDataMap() == null)
            {
                Debug.LogWarning("Current Map not found");
                return null;
            }
            else if (s_Instance.m_DataGame.GetCurrentDataMap().GetCurrentDataLevel() == null)
            {
                Debug.LogWarning("Current Map LevelData not found");
                return null;
            }

            return s_Instance.m_DataGame.GetCurrentDataMap().GetCurrentDataLevel();
        }

        /// <summary>
        /// Returns if level index was increased successfuly
        /// </summary>
        public static bool IncreaseCurrentMapLevel()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return false;
            }
            else if (s_Instance.m_DataGame.GetCurrentDataMap() == null)
            {
                Debug.LogWarning("Current Map not found");
                return false;
            }

            return s_Instance.m_DataGame.GetCurrentDataMap().IncreaseLevelIndex();
        }
        #endregion

        /// <summary>
        /// This is going to save game and map data
        /// </summary>
        public static void SaveData()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_DataGame.Save();

            if (s_Instance.m_DataGame.GetCurrentDataMap() == null)
            {
                Debug.LogWarning("Current Map not found");
                return;
            }

            s_Instance.m_DataGame.GetCurrentDataMap().Save();
        }

        #region General Data

        public static void SetBool(string key, bool value)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_DataGame.SetBool(key, value);
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return false;
            }

            return s_Instance.m_DataGame.GetBool(key, defaultValue);
        }

        public static void SetString(string key, string value)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_DataGame.SetString(key, value);
        }

        public static string GetString(string key, string defaultValue)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return "";
            }

            return s_Instance.m_DataGame.GetString(key, defaultValue);
        }

        public static void SetInt(string key, int value)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_DataGame.SetInt(key, value);
        }

        public static int GetInt(string key, int defaultValue)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return 0;
            }

            return s_Instance.m_DataGame.GetInt(key, defaultValue);
        }
        #endregion

        /// <summary>
        /// This is going to save game, map, and sub data (Player, Stations etc)
        /// </summary>
        public static void SaveAllData()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }
            s_Instance.m_DataGame.Save();

            if (s_Instance.m_DataGame.GetCurrentDataMap() == null)
            {
                Debug.LogWarning("Current Map not found");
                return;
            }

            s_Instance.m_DataGame.GetCurrentDataMap().Save();
            s_Instance.m_DataGame.GetCurrentDataMap().SaveSubData();
        }

        public static void DeleteData()
        {
            try
            {
                string persistentDataPath = Application.persistentDataPath;

                if (Directory.Exists(persistentDataPath))
                {
                    Directory.Delete(persistentDataPath, true);
                    Directory.CreateDirectory(persistentDataPath);
                    Debug.Log("All persistent data has been deleted.");
                }
                else
                {
                    Debug.LogWarning("Persistent data path does not exist.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete persistent data: {ex.Message}");
            }

            PlayerPrefs.DeleteAll();
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(DataManager) + " is null");
        }

        

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Data/Delete Saved Data")]
        public static void DeletePersistentData()
        {
            if (!UnityEditor.EditorUtility.DisplayDialog(
                "Delete Persistent Data",
                "Are you sure you want to delete all persistent data including PayerPrefs?" +
                " This action cannot be undone.",
                "Delete", "Cancel"))
            {
                Debug.Log("Persistent data deletion canceled.");
                return;
            }
            try
            {
                string persistentDataPath = Application.persistentDataPath;

                if (Directory.Exists(persistentDataPath))
                {
                    Directory.Delete(persistentDataPath, true);
                    Directory.CreateDirectory(persistentDataPath);
                    Debug.Log("All persistent data has been deleted.");
                }
                else
                {
                    Debug.LogWarning("Persistent data path does not exist.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete persistent data: {ex.Message}");
            }

            PlayerPrefs.DeleteAll();
        }

        [UnityEditor.MenuItem("Data/Open Data Folder")]
        public static void OpenPersistentDataFolder()
        {
            string path = Application.persistentDataPath;
            if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start(path);
                Debug.Log("Opened persistent data folder.");
            }
            else
            {
                Debug.LogWarning("Persistent data path does not exist.");
            }
        }
#endif
    }


    public enum MapType
    {
        Map1, Map2
    }
}
