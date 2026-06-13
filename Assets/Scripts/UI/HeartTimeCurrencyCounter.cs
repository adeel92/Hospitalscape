using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyAttributes;
using Isometric.Data;


namespace Isometric
{
    public class HeartTimeCurrencyCounter : MonoBehaviour
    {
        private static HeartTimeCurrencyCounter s_Instance;

        public const string LastTimeKey = "HeartTimeCurrency_LastTime";

        [SerializeField, ReadOnly] bool m_HasHeartTimeCurrency = false;
        [SerializeField,
        EnableIf(nameof(m_HasHeartTimeCurrency))] double m_TimeCounter = 0;

        DateTime? m_LastTime = null;
        private Coroutine m_Timer = null;

        private void OnEnable()
        {
            GlobalEventHolder.OnHeartTimeCurrencyUpdate += OnHeartTimeCurrencyUpdate;
        }

        private void OnDisable()
        {
            GlobalEventHolder.OnHeartTimeCurrencyUpdate -= OnHeartTimeCurrencyUpdate;
        }

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
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

            if (s_Instance.m_LastTime == null)
            {
                s_Instance.StopTimer();

                s_Instance.m_LastTime = DateTime.UtcNow;
                if (DateTime.TryParse(DataManager.GetString(LastTimeKey, DateTime.UtcNow.ToString()), out DateTime savedTime))
                {
                    s_Instance.m_LastTime = savedTime;
                }
                TimeSpan timePassed = DateTime.UtcNow - (DateTime)s_Instance.m_LastTime;
                Debug.Log("TotalSeconds TotalSeconds " + timePassed.TotalSeconds);
                double timeRemaingSeconds = DataManager.HeartTimeCurrency - timePassed.TotalSeconds;

                if (timeRemaingSeconds > 0)
                {
                    s_Instance.m_HasHeartTimeCurrency = true;
                    DataManager.HeartTimeCurrency = timeRemaingSeconds;
                    DataManager.HeartCurrency = DataManager.HeartCurrencyMaxValue;
                    DataManager.SaveData();


                    s_Instance.m_Timer = s_Instance.StartCoroutine(s_Instance.Timer());
                }
                else
                {
                    s_Instance.m_LastTime = null;
                    s_Instance.m_HasHeartTimeCurrency = false;
                    s_Instance.m_TimeCounter = 0;
                    DataManager.HeartTimeCurrency = 0;
                    DataManager.SaveData();
                }
            }


           
        }

        public static bool HasHeartTimeCurrency()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return false;
            }

            return s_Instance.m_HasHeartTimeCurrency;
        }

        public static string GetHeartTimeInFormate()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return "";
            }

            return GlobalFunctions.FormatSmartDurationShort(DataManager.HeartTimeCurrency - s_Instance.m_TimeCounter);
        }

        IEnumerator Timer()
        {
            DateTime startTime = DateTime.UtcNow;
            m_TimeCounter = 0;

            while (m_TimeCounter < DataManager.HeartTimeCurrency)
            {
                TimeSpan timePassed = DateTime.UtcNow - startTime;
                m_TimeCounter = timePassed.TotalSeconds;
                yield return null;
            }

            m_HasHeartTimeCurrency = false;
            m_TimeCounter = 0;
            s_Instance.m_LastTime = null;
            DataManager.HeartTimeCurrency = 0;
            DataManager.SaveData();
        }

        private void SaveData()
        {
            DataManager.SetString(LastTimeKey, DateTime.UtcNow.ToString());
            DataManager.HeartTimeCurrency = DataManager.HeartTimeCurrency - m_TimeCounter;
            DataManager.SaveData();
        }

        private void StopTimer()
        {
            if (m_Timer != null)
            {
                StopCoroutine(m_Timer);
                m_Timer = null;
            }
        }

        private void OnApplicationPause(bool pause)
        {
            Debug.Log("OnApplicationPause");
            if (pause == true)
            {
                StopTimer();
                if (m_LastTime != null)
                {
                    SaveData();
                    m_LastTime = null;
                }
            }
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            StopTimer();
            if (m_LastTime != null)
            {
                SaveData();
                m_LastTime = null;
            }
        }

        private void OnHeartTimeCurrencyUpdate(double value)
        {
            if (m_LastTime == null)
            {
                DataManager.SetString(LastTimeKey, DateTime.UtcNow.ToString());
                DataManager.SaveData();
            }
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(HeartTimeCurrencyCounter) + " is null");
        }

    }
}
