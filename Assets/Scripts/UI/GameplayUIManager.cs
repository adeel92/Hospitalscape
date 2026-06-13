using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using NaughtyAttributes;
using DG.Tweening;
using Arc;
using Isometric.Data;
using Isometric.Reward;
using Isometric.Sound;

namespace Isometric.UI
{
    public class GameplayUIManager : UIPopupBase
    {
        [SerializeField, Scene] string m_SceneName;
        [SerializeField] GameObject m_GameplayCanvas;
        [SerializeField] GameObject m_GoalCanvas;

        #region Gamplay Gaol Related
        [Serializable]
        private class GameplayGoalInfo
        {
            [Header("---Target---")]
            public GameObject CoinTarget;
            public GameObject CustomerTarget;
            public float BarFillDuration = 0.3f;
            public Image BarFill;
            public TextMeshProUGUI TargetText;
            public RectTransform StarTargetMarkHolder;
            public GameObject StarTargetArrow;
            public GameObject StarTargetMarkOff;
            public GameObject StarTargetMarkOn;

            public RectTransform Key1TargetMarkHolder;
            public GameObject Key1TargetMarkOff;
            public GameObject Key1TargetMarkOn;
            public RectTransform Key2TargetMarkHolder;
            public GameObject Key2TargetMarkOff;
            public GameObject Key2TargetMarkOn;

            [Header("---Constraint---")]
            public GameObject TimeConstraintHolder;
            public TextMeshProUGUI TimeConstraintText;
            public Image TimeConstraintFillBar;
            public float TimeConstraintFillBarAlarmingValue;
            public Color TimeConstraintFillBarNormalColor;
            public Color TimeConstraintFillBarAlarmingColor;
            public GameObject CustomerConstraintHolder;
            public TextMeshProUGUI CustomerConstraintText;
            public GameObject DontLostCustomerConstraintHolder;

            [Header("---Weather Symbol---")]
            public GameObject WeatherSymbol;
        }

        [SerializeField] GameplayGoalInfo m_GameplayGoalInfo;
        #endregion

        #region Goal Popup Related

        [Serializable]
        private class GoalPopupInfo
        {
            public GameObject GoalPopup;
            public PlayDoTweenSequence GoalOpeningSequence;
            public PlayDoTweenSequence GoalClosingSequence;
            public ChildResizer m_GoalChildResizer;

            [Header("---Target Earn Coins---")]
            public GameObject CoinTargetHolder;
            public TextMeshProUGUI CoinTargetText;

            [Header("---Target Serve Customers---")]
            public GameObject CustomersTargetHolder;
            public TextMeshProUGUI CustomersTargetText;

            [Header("---Time Constraint---")]
            public GameObject TimeConstraintHolder;
            public TextMeshProUGUI TimeConstraintText;

            [Header("---Customer Constraint---")]
            public GameObject CustomerConstraintHolder;
            public TextMeshProUGUI CusomterConstraintText;

            [Header("---Do not Lose Cusomter Constraint---")]
            public GameObject DontLoseCustomerConstraintHolder;
        }

        [SerializeField] GoalPopupInfo m_GoalPopupInfo;
        #endregion


        private void OnEnable()
        {
            GlobalEventHolder.OnTargetValueUpdate += OnTargetValueUpdate;

            GlobalEventHolder.OnTimeConstraintValueUpdate += OnTimeConstraintValueUpdate;
            GlobalEventHolder.OnCustomerConstraintValueUpdate += OnCustomerConstraintValueUpdate;
        }

        private void OnDisable()
        {
            GlobalEventHolder.OnTargetValueUpdate -= OnTargetValueUpdate;

            GlobalEventHolder.OnTimeConstraintValueUpdate -= OnTimeConstraintValueUpdate;
            GlobalEventHolder.OnCustomerConstraintValueUpdate -= OnCustomerConstraintValueUpdate;
        }

        public override void Setup() {}


        public override void OpenPopup(Action onComplete)
        {
            m_GameplayCanvas.SetActive(true);
            m_GoalCanvas.SetActive(true);

            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_GoalPopupInfo.GoalPopup.SetActive(true);

            SetupGameplayGoal();
            SetupGoalPopup();

            m_GoalPopupInfo.GoalOpeningSequence.PlaySequence(() =>
            {
                onComplete?.Invoke();
            });
        }

        public override void ClosePopup(Action onComplete)
        {
            m_GameplayCanvas.SetActive(false);
            m_GoalCanvas.SetActive(false);
            onComplete?.Invoke();
        }

        private void SetupGameplayGoal()
        {
            DataLevel dataLevel = DataManager.GetCurrentDataLevel();
            
            m_GameplayGoalInfo.CoinTarget.SetActive(false);
            m_GameplayGoalInfo.CustomerTarget.SetActive(false);

            m_GameplayGoalInfo.StarTargetMarkOff.SetActive(true);
            m_GameplayGoalInfo.StarTargetMarkOn.SetActive(false);

            m_GameplayGoalInfo.Key1TargetMarkHolder.gameObject.SetActive(false);
            m_GameplayGoalInfo.Key2TargetMarkHolder.gameObject.SetActive(false);

            m_GameplayGoalInfo.TimeConstraintHolder.SetActive(false);
            m_GameplayGoalInfo.CustomerConstraintHolder.SetActive(false);
            m_GameplayGoalInfo.DontLostCustomerConstraintHolder.SetActive(false);
            

            if (dataLevel != null)
            {
                int targetValue = 0;
                if (dataLevel.LevelGoal.GoalType == LevelGoalType.CollectCoins)
                {
                    m_GameplayGoalInfo.CoinTarget.SetActive(true);
                    targetValue = dataLevel.LevelGoal.NumberCoinsToCollect;
                }
                else if (dataLevel.LevelGoal.GoalType == LevelGoalType.ServeCustomers)
                {
                    m_GameplayGoalInfo.CustomerTarget.SetActive(true);
                    targetValue = dataLevel.LevelGoal.NumberOfCustomersToServe;
                }

                foreach (var levelConstraintInfo in dataLevel.LevelConstraintInfos)
                {
                    if (levelConstraintInfo.ConstraintType == LevelConstraintType.TimeConstraint
                        && m_GameplayGoalInfo.TimeConstraintHolder.activeSelf == false)
                    {
                        m_GameplayGoalInfo.TimeConstraintHolder.SetActive(true);
                        m_GameplayGoalInfo.TimeConstraintText.text = levelConstraintInfo.TimeConstraints.FormatTimeMSS();
                        m_GameplayGoalInfo.TimeConstraintFillBar.fillAmount = 1;
                        m_GameplayGoalInfo.TimeConstraintFillBar.color = m_GameplayGoalInfo.TimeConstraintFillBarNormalColor;
                    }
                    else if (levelConstraintInfo.ConstraintType == LevelConstraintType.NumberOfCustomers
                        && m_GameplayGoalInfo.CustomerConstraintHolder.activeSelf == false)
                    {
                        m_GameplayGoalInfo.CustomerConstraintHolder.SetActive(true);
                        m_GameplayGoalInfo.CustomerConstraintText.text = levelConstraintInfo.NumberOfCustomers.FormatNumberMB();
                    }
                    else if (levelConstraintInfo.ConstraintType == LevelConstraintType.DoNotLoseCustomer
                        && m_GameplayGoalInfo.DontLostCustomerConstraintHolder.activeSelf == false)
                    {
                        m_GameplayGoalInfo.DontLostCustomerConstraintHolder.SetActive(true);
                    }
                }

                if (KeyRewardManager.IsUsingKeyReward())
                {
                    int firstStarTarget = dataLevel.Key1TargetValue;
                    int secondStarTarget = dataLevel.Key2TargetValue;


                    m_GameplayGoalInfo.BarFill.fillAmount = ((float)targetValue / (float)secondStarTarget);
                    m_GameplayGoalInfo.StarTargetMarkHolder.anchoredPosition = GlobalFunctions.GetImageFillEndLocalPosition(m_GameplayGoalInfo.BarFill);


                    m_GameplayGoalInfo.BarFill.fillAmount = ((float)firstStarTarget / (float)secondStarTarget);
                    m_GameplayGoalInfo.Key1TargetMarkHolder.anchoredPosition = GlobalFunctions.GetImageFillEndLocalPosition(m_GameplayGoalInfo.BarFill);


                    m_GameplayGoalInfo.Key1TargetMarkHolder.gameObject.SetActive(true);
                    m_GameplayGoalInfo.Key1TargetMarkOn.SetActive(false);
                    m_GameplayGoalInfo.Key1TargetMarkOff.SetActive(true);

                    m_GameplayGoalInfo.Key2TargetMarkHolder.gameObject.SetActive(true);
                    m_GameplayGoalInfo.Key2TargetMarkOn.SetActive(false);
                    m_GameplayGoalInfo.Key2TargetMarkOff.SetActive(true);
                }
                else
                {
                    m_GameplayGoalInfo.StarTargetArrow.SetActive(false);
                    m_GameplayGoalInfo.StarTargetMarkHolder.anchoredPosition = m_GameplayGoalInfo.Key2TargetMarkHolder.anchoredPosition;
                }

                m_GameplayGoalInfo.BarFill.fillAmount = 0;
                m_GameplayGoalInfo.TargetText.text = "0";
            }

        }

        private void SetupGoalPopup()
        {
            DataLevel dataLevel = DataManager.GetCurrentDataLevel();

            m_GoalPopupInfo.CoinTargetHolder.SetActive(false);
            m_GoalPopupInfo.CustomersTargetHolder.SetActive(false);
            m_GoalPopupInfo.TimeConstraintHolder.SetActive(false);
            m_GoalPopupInfo.CustomerConstraintHolder.SetActive(false);
            m_GoalPopupInfo.DontLoseCustomerConstraintHolder.SetActive(false);

            if (dataLevel != null)
            {
                if (dataLevel.LevelGoal.GoalType == LevelGoalType.CollectCoins)
                {
                    m_GoalPopupInfo.CoinTargetHolder.SetActive(true);
                    m_GoalPopupInfo.CoinTargetText.text = dataLevel.LevelGoal.NumberCoinsToCollect.FormatNumberMB();
                }
                else if (dataLevel.LevelGoal.GoalType == LevelGoalType.ServeCustomers)
                {
                    m_GoalPopupInfo.CustomersTargetHolder.SetActive(true);
                    m_GoalPopupInfo.CustomersTargetText.text = dataLevel.LevelGoal.NumberOfCustomersToServe.FormatNumberMB();
                }

                foreach (var levelConstraintInfo in dataLevel.LevelConstraintInfos)
                {
                    if (levelConstraintInfo.ConstraintType == LevelConstraintType.TimeConstraint
                        && m_GoalPopupInfo.TimeConstraintHolder.activeSelf == false)
                    {
                        m_GoalPopupInfo.TimeConstraintHolder.SetActive(true);
                        m_GoalPopupInfo.TimeConstraintText.text = levelConstraintInfo.TimeConstraints.FormatTimeMSS();
                    }
                    else if (levelConstraintInfo.ConstraintType == LevelConstraintType.NumberOfCustomers
                        && m_GoalPopupInfo.CustomerConstraintHolder.activeSelf == false)
                    {
                        m_GoalPopupInfo.CustomerConstraintHolder.SetActive(true);
                        m_GoalPopupInfo.CusomterConstraintText.text = levelConstraintInfo.NumberOfCustomers.FormatNumberMB();
                    }
                    else if (levelConstraintInfo.ConstraintType == LevelConstraintType.DoNotLoseCustomer
                        && m_GoalPopupInfo.DontLoseCustomerConstraintHolder.activeSelf == false)
                    {
                        m_GoalPopupInfo.DontLoseCustomerConstraintHolder.SetActive(true);
                    }
                }

                m_GoalPopupInfo.m_GoalChildResizer.Resize();
            }
        }

        public void CloseGoalPopup(Action onComplete)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_GoalPopupInfo.GoalClosingSequence.PlaySequence(() =>
            {
                m_GoalPopupInfo.GoalPopup.SetActive(false);
                m_GoalCanvas.SetActive(false);
                onComplete?.Invoke();
            });
        }

        public void OnStartGameplay()
        {
            UIManager.StartGame();
        }

        #region Goal Related
        private void OnTargetValueUpdate(int currentValue, int targetValue)
        {
            m_GameplayGoalInfo.TargetText.text = currentValue.ToString();

            if (!KeyRewardManager.IsUsingKeyReward())
            { 
                float fill = (float)currentValue / (float)targetValue;
                if (m_GameplayGoalInfo.BarFill.fillAmount < 1)
                {
                    m_GameplayGoalInfo.BarFill.DOKill();
                    m_GameplayGoalInfo.BarFill.DOFillAmount(fill, m_GameplayGoalInfo.BarFillDuration);
                }
            }   

            if (currentValue >= targetValue && !m_GameplayGoalInfo.StarTargetMarkOn.activeSelf)
            {
                m_GameplayGoalInfo.StarTargetMarkOn.SetActive(true);
            }

            if (KeyRewardManager.IsUsingKeyReward())
            {
                int key1TargetValue = LevelManager.GetTargetKey1Value();
                int key2TargetValue = LevelManager.GetTargetKey2Value();

                float fill = (float)currentValue / (float)key2TargetValue;

                if (m_GameplayGoalInfo.BarFill.fillAmount < 1)
                {
                    m_GameplayGoalInfo.BarFill.DOKill();
                    m_GameplayGoalInfo.BarFill.DOFillAmount(fill, m_GameplayGoalInfo.BarFillDuration);
                }

                if (key1TargetValue <= currentValue && !m_GameplayGoalInfo.Key1TargetMarkOn.activeSelf)
                {
                    m_GameplayGoalInfo.Key1TargetMarkOn.SetActive(true);
                }

                if (key2TargetValue <= currentValue && !m_GameplayGoalInfo.Key2TargetMarkOn.activeSelf)
                {
                    m_GameplayGoalInfo.Key2TargetMarkOn.SetActive(true);
                }
            }
        }
        #endregion

        #region Constraint Related
        private void OnTimeConstraintValueUpdate(int timeConstraintCurrentValue, int timeConstraintValue)
        {
            float fillAmount = (float)timeConstraintCurrentValue / (float)timeConstraintValue;
            if (fillAmount < m_GameplayGoalInfo.TimeConstraintFillBarAlarmingValue)
            {
                m_GameplayGoalInfo.TimeConstraintFillBar.color = m_GameplayGoalInfo.TimeConstraintFillBarAlarmingColor;
            }
            m_GameplayGoalInfo.TimeConstraintFillBar.fillAmount = fillAmount;
            m_GameplayGoalInfo.TimeConstraintText.text = timeConstraintCurrentValue.FormatTimeMSS();
        }

        private void OnCustomerConstraintValueUpdate(int customerConstraintValue)
        {
            m_GameplayGoalInfo.CustomerConstraintText.text = customerConstraintValue.ToString();
        }
        #endregion

        public void SetActiveWeatherSymbol(bool value)
        {
            m_GameplayGoalInfo.WeatherSymbol.SetActive(value);
        }


        public void OnStopPlayerTasks()
        {
            GlobalEventHolder.OnCurrentTaskTargetCancle?.Invoke();
        }

        public void OnRestartButton()
        {
            DOTween.KillAll();
            CoroutineManager.StopAllCoroutine();
            SceneManager.LoadScene(m_SceneName);
        }
    }
}
