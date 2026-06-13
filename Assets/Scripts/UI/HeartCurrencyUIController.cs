using System;
using UnityEngine;
using TMPro;
using NaughtyAttributes;
using Isometric.Data;

namespace Isometric.UI
{
    public class HeartCurrencyUIController : MonoBehaviour
    {
        public const string c_LastTimeKey = "HeartCurrency_LastTime";
        public const string c_HeartCurrencyMinus = "HeartCurrency_ShouldBeMinusOne";

        [SerializeField] GameObject m_HeartSymbol;
        [SerializeField] GameObject m_HeartInfinitySymbol;
        [SerializeField] TextMeshProUGUI m_HeartText;
        [SerializeField] TextMeshProUGUI m_FillTimerText;
        [SerializeField] bool m_IsAutomaticUpdate = true;

        [SerializeField, ReadOnly] float m_RefillTimer;
        [SerializeField, ReadOnly] bool m_HasHeartInfinite;

        //The first setup call is going to be from the start 
        private bool m_HasCalledOnStart = false;

        private static DateTime s_LastTime;

        private void OnEnable()
        {
            GlobalEventHolder.OnHeartCurrencyUpdate += OnHeartCurrencyUpdate;
            GlobalEventHolder.OnHeartTimeCurrencyUpdate += OnHeartTimeCurrencyUpdate;

            if (m_HasCalledOnStart == true)
            {
                if (m_IsAutomaticUpdate)
                {
                    Setup();
                }
            }
        }

        private void OnDisable()
        {
            GlobalEventHolder.OnHeartCurrencyUpdate -= OnHeartCurrencyUpdate;
            GlobalEventHolder.OnHeartTimeCurrencyUpdate -= OnHeartTimeCurrencyUpdate;
        }

        private void Start()
        {
            m_HasCalledOnStart = true;
            if (m_IsAutomaticUpdate)
            {
                Setup();
            }
        }

        public void Setup()
        {
            HeartTimeCurrencyCounter.Setup();

            m_HasHeartInfinite = HeartTimeCurrencyCounter.HasHeartTimeCurrency();

            if (m_HasHeartInfinite)
            {
                m_FillTimerText.gameObject.SetActive(false);
                m_HeartSymbol.SetActive(false);
                m_HeartInfinitySymbol.SetActive(true);
            }
            else
            {
                m_FillTimerText.gameObject.SetActive(false);
                m_HeartSymbol.SetActive(true);
                m_HeartInfinitySymbol.SetActive(false);

                CalculateOfflineProgress();
                UpdateUI();
            }
        }

        private void EventSetup()
        {
            m_HasHeartInfinite = HeartTimeCurrencyCounter.HasHeartTimeCurrency();

            if (m_HasHeartInfinite)
            {
                m_FillTimerText.gameObject.SetActive(false);
                m_HeartSymbol.SetActive(false);
                m_HeartInfinitySymbol.SetActive(true);
            }
            else
            {
                m_FillTimerText.gameObject.SetActive(false);
                m_HeartSymbol.SetActive(true);
                m_HeartInfinitySymbol.SetActive(false);
                UpdateUI();
            }
        }

        private void Update()
        {
            if (m_HasHeartInfinite)
            {
                m_HeartText.text = HeartTimeCurrencyCounter.GetHeartTimeInFormate();
                
                if (HeartTimeCurrencyCounter.HasHeartTimeCurrency() == false)
                {
                    m_HeartInfinitySymbol.SetActive(false);
                    m_FillTimerText.gameObject.SetActive(false);
                    m_HeartSymbol.SetActive(true);
                    UpdateUI();

                    m_HasHeartInfinite = false;
                }
            }
            else
            {
                if (DataManager.HeartCurrency >= DataManager.HeartCurrencyMaxValue)
                {
                    m_FillTimerText.gameObject.SetActive(false);
                    return;
                }

                TimeSpan timePassed = DateTime.UtcNow - s_LastTime;
                float leftoverSeconds = (float)timePassed.TotalSeconds % DataManager.HeartCurrencyRefillTime;
                m_RefillTimer = leftoverSeconds;

                if (timePassed.TotalSeconds >= DataManager.HeartCurrencyRefillTime)
                {
                    /*AddHeart();
                    m_RefillTimer = 0f;*/
                    Setup();
                }

                UpdateUI();
            }
        }

        private void AddHeart()
        {
            if (DataManager.HeartCurrency < DataManager.HeartCurrencyMaxValue)
            {
                DataManager.HeartCurrency++;
                SaveData();
            }
        }

        private void CalculateOfflineProgress()
        {
            DateTime now = DateTime.UtcNow;
            s_LastTime = now;
            if (DateTime.TryParse(
                DataManager.GetString(c_LastTimeKey, 
                now.ToString("o")), 
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind, 
                out DateTime savedTime))
            {
                s_LastTime = savedTime.ToUniversalTime();
            }
            else
            {
                s_LastTime = DateTime.MinValue;
            }
            TimeSpan timePassed = now - s_LastTime;

            int heartsToAdd = Mathf.FloorToInt((float)timePassed.TotalSeconds / DataManager.HeartCurrencyRefillTime);
            bool shouldBeMenusOne = DataManager.GetBool(c_HeartCurrencyMinus, false);
            if (shouldBeMenusOne)
            {
                heartsToAdd -= 1;
                DataManager.SetBool(c_HeartCurrencyMinus, false);
            }
            DataManager.HeartCurrency = Math.Max(0, Mathf.Min(DataManager.HeartCurrency + heartsToAdd, DataManager.HeartCurrencyMaxValue));

            if (DataManager.HeartCurrency < DataManager.HeartCurrencyMaxValue && heartsToAdd > 0)
            {
                float extraSeconds = (float)timePassed.TotalSeconds % DataManager.HeartCurrencyRefillTime;

                if (extraSeconds > 0)
                {
                    DateTime adjustedNow = now.AddSeconds(-extraSeconds); 
                    DataManager.SetString(c_LastTimeKey, adjustedNow.ToString("o"));
                }
                else
                {
                    DataManager.SetString(c_LastTimeKey, now.ToString("o"));
                }
            }

            DataManager.SaveData();
            float leftoverSeconds = (float)timePassed.TotalSeconds % DataManager.HeartCurrencyRefillTime;
            m_RefillTimer = leftoverSeconds;
        }

        private static void SaveData()
        {
            DataManager.SetString(c_LastTimeKey, DateTime.UtcNow.ToString("o"));
            DataManager.SaveData();
        }

        public static void SetValueForShouldBeMinusOne(bool value)
        {
            if (DataManager.HeartCurrency >= DataManager.HeartCurrencyMaxValue)
            {
                DataManager.SetString(c_LastTimeKey, DateTime.UtcNow.ToString("o"));
            }
            DataManager.SetBool(c_HeartCurrencyMinus, value);
            DataManager.SaveData();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause == false)
            {
                if (m_HasCalledOnStart && m_IsAutomaticUpdate)
                {
                    Setup();
                }
            }
        }

        private void UpdateUI()
        {
            m_HeartText.text = $"{DataManager.HeartCurrency}/{DataManager.HeartCurrencyMaxValue}";

            if (DataManager.HeartCurrency < DataManager.HeartCurrencyMaxValue)
            {
                m_FillTimerText.gameObject.SetActive(true);

                //float remainingTime = Mathf.Clamp(DataManager.HeartCurrencyRefillTime - m_RefillTimer, 0, float.MaxValue);
                /*TimeSpan time = TimeSpan.FromSeconds(remainingTime);*/
                /*TimeSpan timePassed = DateTime.UtcNow - s_LastTime;
                TimeSpan time = TimeSpan.FromSeconds(DataManager.HeartCurrencyRefillTime - timePassed.Seconds);
                m_FillTimerText.text = $"{time.Minutes:D2}:{time.Seconds:D2}";*/

                double totalSecondsPassed = (DateTime.UtcNow - s_LastTime).TotalSeconds;
                double secondsIntoCurrentCycle = totalSecondsPassed % DataManager.HeartCurrencyRefillTime;
                double remainingSeconds = DataManager.HeartCurrencyRefillTime - secondsIntoCurrentCycle;

                double seconds = remainingSeconds;

                if (double.IsNaN(seconds) || double.IsInfinity(seconds))
                {
                    seconds = 0f;
                }

                TimeSpan time = TimeSpan.FromSeconds(seconds);
                m_FillTimerText.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
            }
            else
            {
                m_FillTimerText.gameObject.SetActive(false);
            }
        }

        public static void CheckLastTimeSaved()
        {
            if (DataManager.HeartCurrency == DataManager.HeartCurrencyMaxValue)
            {
                SaveData();
            }
        }

        private void OnHeartCurrencyUpdate(int value)
        {
            if (m_IsAutomaticUpdate)
            {
                EventSetup();
            }
        }


        private void OnHeartTimeCurrencyUpdate(double value)
        {
            if (m_IsAutomaticUpdate)
            {
                EventSetup();
            }
        }
    }
}
