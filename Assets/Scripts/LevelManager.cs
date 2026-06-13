using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.UI;
using Isometric.Cam;

namespace Isometric
{ 
    public class LevelManager : MonoBehaviour
    {
        private static LevelManager s_Instance;

        [Header("---BaseInfo---")]
        [SerializeField, ReadOnly] int m_CurrentLevel;

        [Header("---Goal---")]
        [SerializeField, ReadOnly] LevelGoalType m_GoalType;
        [SerializeField, ReadOnly] int m_TargetValue;
        [SerializeField, ReadOnly] int m_TargetValueCounter;
        [SerializeField, ReadOnly] int m_TargetKey1Counter;
        [SerializeField, ReadOnly] int m_TargetKey2Counter;

        [SerializeField, ReadOnly] int m_CoinsCollectedCounter;
        [SerializeField, ReadOnly] int m_CustomerServedCounter;
        [SerializeField, ReadOnly] int m_CustomerLostCounter;


        [Header("---Constraint---")]
        [SerializeField, ReadOnly] bool m_HasTimeConstraint;
        [SerializeField, ReadOnly] int m_TimeConstraintValue;
        [SerializeField, ReadOnly] float m_CurrentTimeConstraintValue;
        private Coroutine m_TimeConstraintCounterCorotine;

        [SerializeField, ReadOnly] bool m_HasCustomerConstraint;
        [SerializeField, ReadOnly] int m_CustomerConstraintValue;
        [SerializeField, ReadOnly] int m_CurrentCustomerConstraintValue;


        [SerializeField, ReadOnly] bool m_HasDontLostCustomerConstraint;
        [SerializeField, ReadOnly] bool m_DontLostCustomerConstraintValue;
        [SerializeField, ReadOnly] bool m_HasLostCustomer;

        [SerializeField, ReadOnly] bool m_HasGameWon = false;
        [SerializeField, ReadOnly] bool m_HasGameLost = false;
        [SerializeField, ReadOnly] bool m_HasLevelChanceAvailed = false;

        [Header("---Level Failed---")]
        [SerializeField, ReadOnly] LevelLostReason m_LevelLostReason;

        [Header("---Helper---")]
        [SerializeField, ReadOnly] bool m_TimerLock = false;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }

        private void OnEnable()
        {
            GlobalEventHolder.OnCustomerEntered += OnCustomerEntered;
            GlobalEventHolder.OnCustomerServed += OnCustomerServed;
            GlobalEventHolder.OnCustomerLost += OnCustomerLost;

            GlobalEventHolder.OnCoinsCollected += OnCoinsCollected;
        }

        private void OnDisable()
        {
            
            GlobalEventHolder.OnCustomerEntered -= OnCustomerEntered;
            GlobalEventHolder.OnCustomerServed -= OnCustomerServed;
            GlobalEventHolder.OnCustomerLost -= OnCustomerLost;

            GlobalEventHolder.OnCoinsCollected -= OnCoinsCollected;
        }

        public static void Setup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_CurrentLevel = DataManager.CurrentMapLevelIndex + 1;

            DataLevel currentLevel = DataManager.GetCurrentDataLevel();
            if (currentLevel != null && currentLevel.HasNewCustomer)
            {
                CustomerNewInfo customerNewInfo = currentLevel.NewCustomersInfo;

                foreach (CustomerId newCustomerInfo in customerNewInfo.NewCustomersInfo)
                {
                    if (DataManager.GetBool("New" + newCustomerInfo, false) == false)
                    {
                        GlobalEventHolder.OnNewCustomerUnlocked?.Invoke(newCustomerInfo);
                        DataManager.SetBool("New" + newCustomerInfo, true);
                    }
                }
            }
        }


        public static void SetupForGameplay()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_TargetValueCounter = 0;
            s_Instance.m_CoinsCollectedCounter = 0;
            s_Instance.m_CustomerServedCounter = 0;
            s_Instance.m_CurrentTimeConstraintValue = 0;
            s_Instance.m_CurrentCustomerConstraintValue = 0;
            s_Instance.m_DontLostCustomerConstraintValue = false;
            s_Instance.m_HasLostCustomer = false;

            s_Instance.m_HasGameWon = false;
            s_Instance.m_HasGameLost = false;
            s_Instance.m_HasLevelChanceAvailed = false;

            s_Instance.m_TimerLock = false;

            DataLevel dataLevel = DataManager.GetCurrentDataLevel();

            if (dataLevel != null)
            {
                if (dataLevel.LevelGoal.GoalType == LevelGoalType.CollectCoins)
                {
                    s_Instance.m_GoalType = dataLevel.LevelGoal.GoalType;
                    s_Instance.m_TargetValue = dataLevel.LevelGoal.NumberCoinsToCollect;
                }
                else if (dataLevel.LevelGoal.GoalType == LevelGoalType.ServeCustomers)
                {
                    s_Instance.m_GoalType = dataLevel.LevelGoal.GoalType;
                    s_Instance.m_TargetValue = dataLevel.LevelGoal.NumberOfCustomersToServe;
                }

                s_Instance.m_TargetKey1Counter = dataLevel.Key1TargetValue;
                s_Instance.m_TargetKey2Counter = dataLevel.Key2TargetValue;

                s_Instance.m_HasTimeConstraint = false;
                s_Instance.m_HasCustomerConstraint = false;
                s_Instance.m_HasDontLostCustomerConstraint = false;

                foreach (var levelConstraintInfo in dataLevel.LevelConstraintInfos)
                {
                    if (levelConstraintInfo.ConstraintType == LevelConstraintType.TimeConstraint
                        && s_Instance.m_HasTimeConstraint == false)
                    {
                        s_Instance.m_HasTimeConstraint = true;
                        s_Instance.m_TimeConstraintValue = levelConstraintInfo.TimeConstraints;
                    }
                    else if (levelConstraintInfo.ConstraintType == LevelConstraintType.NumberOfCustomers
                        && s_Instance.m_HasCustomerConstraint == false)
                    {
                        s_Instance.m_HasCustomerConstraint = true;
                        s_Instance.m_CustomerConstraintValue = levelConstraintInfo.NumberOfCustomers;
                    }
                    else if (levelConstraintInfo.ConstraintType == LevelConstraintType.DoNotLoseCustomer
                        && s_Instance.m_HasDontLostCustomerConstraint == false)
                    {
                        s_Instance.m_HasDontLostCustomerConstraint = true;
                    }
                }
            }

            s_Instance.SetupTimeConstraint();
        }

        #region Goal/Target Related
        private void OnCoinsCollected(int collectedCoinsTarget)
        {
            if (m_HasGameLost == false && m_HasGameWon == false)
            {
                if (m_GoalType == LevelGoalType.CollectCoins)
                {
                    m_TargetValueCounter += collectedCoinsTarget;
                    GlobalEventHolder.OnTargetValueUpdate?.Invoke(m_TargetValueCounter, m_TargetValue);
                }

                m_CoinsCollectedCounter += collectedCoinsTarget;
            }
        }

        private void OnCustomerServed()
        {
            if (m_HasGameLost == false && m_HasGameWon == false)
            {
                if (m_GoalType == LevelGoalType.ServeCustomers)
                {
                    m_TargetValueCounter += 1;
                    GlobalEventHolder.OnTargetValueUpdate?.Invoke(m_TargetValueCounter, m_TargetValue);
                }

                m_CustomerServedCounter += 1;

                CheckIsGameOver();
            }
        }
        #endregion

        #region Constraint Related
        private void SetupTimeConstraint()
        {
            if (m_HasTimeConstraint)
            {
                StopTimeConstraintCounter();
                m_TimeConstraintCounterCorotine = StartCoroutine(TimeConstraintCounter(m_TimeConstraintValue));
            }
        }

        private void StopTimeConstraintCounter()
        {
            if (m_TimeConstraintCounterCorotine != null)
            {
                StopCoroutine(m_TimeConstraintCounterCorotine);
                m_TimeConstraintCounterCorotine = null;
            }
        }

        IEnumerator TimeConstraintCounter(int timeTarget)
        {
            m_CurrentTimeConstraintValue = timeTarget;
            while (m_CurrentTimeConstraintValue > 0)
            {
                yield return null;
                if (!m_TimerLock)
                {
                    m_CurrentTimeConstraintValue -= Time.deltaTime;
                }
                GlobalEventHolder.OnTimeConstraintValueUpdate(Mathf.RoundToInt(m_CurrentTimeConstraintValue), m_TimeConstraintValue);
            }

            CheckIsGameOver();
        }

        private void OnCustomerEntered()
        {
            if (m_HasCustomerConstraint)
            {
                m_CurrentCustomerConstraintValue++;

                if (m_CurrentCustomerConstraintValue < m_CustomerConstraintValue)
                {
                    GlobalEventHolder.OnCustomerConstraintValueUpdate?.Invoke(m_CustomerConstraintValue - m_CurrentCustomerConstraintValue);
                }
                else
                {
                    GlobalEventHolder.OnCustomerConstraintValueUpdate?.Invoke(0);
                }
            }
        }

        private void OnCustomerLost()
        {
            m_HasLostCustomer = true;

            if (m_HasDontLostCustomerConstraint)
            {
                m_DontLostCustomerConstraintValue = true;
                CheckIsGameOver();
            }

            m_CustomerLostCounter += 1;

            if (m_HasCustomerConstraint)
            {
                CheckIsGameOver();
            }

        }
        #endregion

        #region GameOver/GameWon/GameLost
        private void CheckIsGameOver()
        {
            if (m_HasTimeConstraint && m_CurrentTimeConstraintValue <= 0)
            {
                if (HasGameWon())
                {
                    GameWon();
                }
                else
                {
                    GameLost(LevelLostReason.NoMoreTime);
                }

                return;
            }

            if (m_HasCustomerConstraint && 
                (m_CustomerServedCounter >= m_CustomerConstraintValue))
            {
                StopTimeConstraintCounter();

                if (HasGameWon())
                {
                    GameWon();
                }
                else
                {
                    GameLost(LevelLostReason.NoMoreCustomers);
                }

                return;
            }

            int totalCustomers = m_CustomerServedCounter + m_CustomerLostCounter;
            if (m_HasCustomerConstraint &&
                (totalCustomers >= m_CustomerConstraintValue))
            {
                StopTimeConstraintCounter();

                if (HasGameWon())
                {
                    GameWon();
                }
                else
                {
                    GameLost(LevelLostReason.NoMoreCustomers);
                }

                return;
            }

            if (m_HasDontLostCustomerConstraint && m_DontLostCustomerConstraintValue)
            {
                StopTimeConstraintCounter();

                GameLost(LevelLostReason.LostACustomer);
                return;
            }
        }

        private bool HasGameWon()
        {
            if (m_TargetValueCounter >= m_TargetValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void GameWon()
        {
            if (m_HasGameWon == false && m_HasGameLost == false)
            {
                CameraController.SetEnvironemntInteractiblity(false);
                UIManager.UIInteractionOff();
                GlobalEventHolder.OnGameWon?.Invoke();
                GlobalEventHolder.OnPlayerStopMoving?.Invoke();
                m_HasGameWon = true;
                if (m_HasLostCustomer == false)
                {
                    GlobalEventHolder.OnHasNotLostACustomerOnLevel?.Invoke();
                }
            }
        }

        private void GameLost(LevelLostReason levelFailureReason)
        {
            if (m_HasGameWon == false && m_HasGameLost == false)
            {
                m_LevelLostReason = levelFailureReason;
                if (m_HasLevelChanceAvailed == false)
                {
                    UIManager.OpenLevelChancePopup(m_GoalType, m_TargetValue, m_TargetValueCounter, levelFailureReason);
                    m_HasGameLost = true;
                }
                else
                {
                    CameraController.SetEnvironemntInteractiblity(false);
                    UIManager.UIInteractionOff();
                    GlobalEventHolder.OnGameLost?.Invoke();
                    m_HasGameLost = true;
                }
            }
        }
        #endregion

        #region Helper

        public static int GetTargetValue()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return 0;
            }

            return s_Instance.m_TargetValue;
        }

        public static int GetCurrentTargetValue()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return 0;
            }

            return s_Instance.m_TargetValueCounter;
        }

        public static int GetTargetKey1Value()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return 0;
            }

            return s_Instance.m_TargetKey1Counter;
        }

        public static int GetTargetKey2Value()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return 0;
            }

            return s_Instance.m_TargetKey2Counter;
        }

        public static int GetCollectedCoins()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return 0;
            }

            return s_Instance.m_CoinsCollectedCounter;
        }


        public static void SetHasLevelChanceAvailed(bool value)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_HasLevelChanceAvailed = value;
            s_Instance.m_HasGameLost = false;
        }

        public static void SetTimeConstraint(int timeTarget)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.StopTimeConstraintCounter();
            s_Instance.m_TimeConstraintCounterCorotine = s_Instance.StartCoroutine(s_Instance.TimeConstraintCounter(timeTarget));
        }

        public static void IncreaseCustomerConstraintBy(int customerIncreased)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_CustomerConstraintValue += customerIncreased;

            if (s_Instance.m_CurrentCustomerConstraintValue < s_Instance.m_CustomerConstraintValue)
            {
                GlobalEventHolder.OnCustomerConstraintValueUpdate?.Invoke(s_Instance.m_CustomerConstraintValue - s_Instance.m_CurrentCustomerConstraintValue);
            }
            else
            {
                GlobalEventHolder.OnCustomerConstraintValueUpdate?.Invoke(0);
            }
        }

        public static void SetToGameOver()
        {
            GlobalEventHolder.OnGameLost?.Invoke();
        }

        public static LevelLostReason GetLevelLostReason()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return LevelLostReason.None;
            }

            return s_Instance.m_LevelLostReason;
        }

        public static void SetTimerLock(bool value)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_TimerLock = value;
        }
        #endregion

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(LevelManager) + " is null");
        }
    }

    public enum LevelLostReason
    {
        None, NoMoreTime, NoMoreCustomers, LostACustomer
    }
}
