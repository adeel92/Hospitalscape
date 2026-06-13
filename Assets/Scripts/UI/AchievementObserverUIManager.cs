using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using DG.Tweening;
using NaughtyAttributes;
using Arc;
using Isometric.Data;

namespace Isometric.UI
{
    public class AchievementObserverUIManager : MonoBehaviour
    {
        public static AchievementObserverUIManager s_Instance;

        [SerializeField] DataMapUpdate m_DataMapUpdate;
        [SerializeField] GameObject m_Canvas;

        [Header("---Notification---")]
        [SerializeField] RectTransform m_Notification;
        [SerializeField] float m_NotificationMovmentDuration;
        [SerializeField] Vector2 m_NotificationHidePosition;
        [SerializeField] Vector2 m_NotificationShowPosition;
        [SerializeField] float m_NotficationShowingDelay;
        [SerializeField] TextMeshProUGUI m_NotificationText;
        Queue<string> m_NotificationQueue = new Queue<string>();
        private bool m_IsShowingNotification = false;

        [Header("---Achievement Numbers---")]
        [SerializeField, ReadOnly]
        private float m_CustomersUnlockedCounter = 0;
        [SerializeField, ReadOnly]
        private float m_CompleteAllLevelCounterCounter = 0;
        [SerializeField, ReadOnly]
        private float m_KeyCollectionCounterCounter = 0;
        [SerializeField, ReadOnly]
        private float m_CompleteLevelCounterCounter = 0;
        [SerializeField, ReadOnly]
        private float m_PlayerWakDistanceCounterCounter = 0;
        [SerializeField, ReadOnly]
        private float m_CustomerNotLostPerLevelCounter = 0;
        [SerializeField, ReadOnly]
        private float m_CustomerServerdCounter = 0;
        [SerializeField, ReadOnly]
        private float m_CustomerServerdHappyCounter = 0;
        [SerializeField, ReadOnly]
        private float m_CustomerOrderServedCounter = 0;
        [SerializeField, ReadOnly]
        private float m_CoinCurrencyCounter = 0;
        [SerializeField, ReadOnly]
        private float m_CoinCurrencyOnUpgradeCounter = 0;
        [SerializeField, ReadOnly]
        private float m_CustomerOrderOnFastBoosterCounter = 0;
        [SerializeField, ReadOnly]
        private float m_WorkerOrderCounter = 0;
        [SerializeField, ReadOnly]
        private float m_CustomersWaitFrozeCounter = 0;
        [SerializeField, ReadOnly]
        private float m_ContinousPlayTimer = 0;
        [SerializeField, ReadOnly]
        private float m_PlayerConstentlyMovingTimer = 0;


        private bool m_HasSetup = false;
        private Coroutine m_OnPlayerStartedMoving;
        private bool m_IsOnFastBooster;

        [Space, SerializeField, ReadOnly]
        private bool m_IsApplicationFocused = false;

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

        private void OnEnable()
        {
            GlobalEventHolder.OnNewCustomerUnlocked += OnNewCustomerUnlocked;

            GlobalEventHolder.OnLevelComplete += OnLevelComplete;
            GlobalEventHolder.OnKeyCollected += OnKeyCollected;
            GlobalEventHolder.OnPlayerWalkDistance += OnPlayerWalkDistance;
            GlobalEventHolder.OnHasNotLostACustomerOnLevel += OnHasNotLostACustomerOnLevel;
            GlobalEventHolder.OnCustomerServed += OnCustomerServed;

            GlobalEventHolder.OnCustomerSitHappy += OnCustomerSitOrServedHappy;
            GlobalEventHolder.OnCounterCustomerServedHappy += OnCustomerSitOrServedHappy;

            GlobalEventHolder.OnCustomerOrderServed += OnCustomerOrderServed;
            GlobalEventHolder.OnNewCoinCurrencyAdded += OnNewCoinCurrencyAdded;
            GlobalEventHolder.OnCoinCurrencySpendOnUpgrade += OnCoinCurrencySpendOnUpgrade;

            GlobalEventHolder.OnPlayerStartedMoving += OnPlayerStartedMoving;
            GlobalEventHolder.OnPlayerStopMoving += OnPlayerStopMoving;
            GlobalEventHolder.OnCurrentTaskTargetCancle += OnPlayerStopMoving;

            GlobalEventHolder.OnCustomerOrderServed += OnCustomerOrderServed;
            GlobalEventHolder.OnWaitressSpeedBooster += OnWaitressSpeedBooster;
            GlobalEventHolder.OnCustomerOrderServed += OnCustomerOrderServedOnFastBooster;
            GlobalEventHolder.OnWorkerServesOrder += OnWorkerServesOrder;
            GlobalEventHolder.OnACustomersWaitFrozen += OnACustomersWaitFrozen;
        }

        private void OnDisable()
        {
            GlobalEventHolder.OnNewCustomerUnlocked -= OnNewCustomerUnlocked;
            GlobalEventHolder.OnLevelComplete -= OnLevelComplete;
            GlobalEventHolder.OnKeyCollected -= OnKeyCollected;
            GlobalEventHolder.OnPlayerWalkDistance -= OnPlayerWalkDistance;
            GlobalEventHolder.OnHasNotLostACustomerOnLevel -= OnHasNotLostACustomerOnLevel;
            GlobalEventHolder.OnCustomerServed -= OnCustomerServed;

            GlobalEventHolder.OnCustomerSitHappy -= OnCustomerSitOrServedHappy;
            GlobalEventHolder.OnCounterCustomerServedHappy -= OnCustomerSitOrServedHappy;

            GlobalEventHolder.OnCustomerOrderServed -= OnCustomerOrderServed;
            GlobalEventHolder.OnNewCoinCurrencyAdded -= OnNewCoinCurrencyAdded;
            GlobalEventHolder.OnCoinCurrencySpendOnUpgrade -= OnCoinCurrencySpendOnUpgrade;

            GlobalEventHolder.OnPlayerStartedMoving -= OnPlayerStartedMoving;
            GlobalEventHolder.OnPlayerStopMoving -= OnPlayerStopMoving;
            GlobalEventHolder.OnCurrentTaskTargetCancle -= OnPlayerStopMoving;

            GlobalEventHolder.OnCustomerOrderServed -= OnCustomerOrderServed;
            GlobalEventHolder.OnWaitressSpeedBooster -= OnWaitressSpeedBooster;
            GlobalEventHolder.OnCustomerOrderServed -= OnCustomerOrderServedOnFastBooster;
            GlobalEventHolder.OnWorkerServesOrder -= OnWorkerServesOrder;
            GlobalEventHolder.OnACustomersWaitFrozen -= OnACustomersWaitFrozen;
        }

        public static void Setup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            if (s_Instance.m_HasSetup == false)
            {
                if (s_Instance.IsAchievementComplete(AchievementType.AchievementPlayForHour) == false)
                {
                    s_Instance.StartCoroutine(s_Instance.AchievementPlayForHours());
                }

                s_Instance.m_HasSetup = true;
            }
        }


        private void OnNewCustomerUnlocked(CustomerId customerId)
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.AchievementCollectCustomers, 1);
            if (hasJustAchieved)
            {
                string message = GetNotificationMessage(AchievementType.AchievementCollectCustomers);
                PushNotification(message);
            }

            m_CustomersUnlockedCounter = achievementUpdate.GetAchievementRecord(AchievementType.AchievementCollectCustomers);
        }

        private void OnLevelComplete()
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            bool hasAchieved1 = achievementUpdate.SetAchievementRecord(AchievementType.AchievementCompleteAllLevels, 1);
            if (hasAchieved1)
            {
                string message = GetNotificationMessage(AchievementType.AchievementCompleteAllLevels);
                PushNotification(message);
            }

            m_CompleteAllLevelCounterCounter = achievementUpdate.GetAchievementRecord(AchievementType.AchievementCompleteAllLevels);

            bool hasAchieved2 = achievementUpdate.SetAchievementRecord(AchievementType.AchievementCompleteLevels, 1);
            if (hasAchieved2)
            {
                string message = GetNotificationMessage(AchievementType.AchievementCompleteLevels);
                PushNotification(message);
            }

            m_CompleteLevelCounterCounter = achievementUpdate.GetAchievementRecord(AchievementType.AchievementCompleteLevels);

        }

        private void OnKeyCollected()
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.Achievement25KeyCollected, 1);
            if (hasJustAchieved)
            {
                string message = GetNotificationMessage(AchievementType.Achievement25KeyCollected);
                PushNotification(message);
            }

            m_KeyCollectionCounterCounter = achievementUpdate.GetAchievementRecord(AchievementType.Achievement25KeyCollected);
        }

        private void OnPlayerWalkDistance(int distance)
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.AchievementPlayerWalksDistance, distance);
            if (hasJustAchieved)
            {
                string message = GetNotificationMessage(AchievementType.AchievementPlayerWalksDistance);
                PushNotification(message);
            }

            m_PlayerWakDistanceCounterCounter = achievementUpdate.GetAchievementRecord(AchievementType.AchievementPlayerWalksDistance);
        }

        private void OnHasNotLostACustomerOnLevel()
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.AchievementNotLostACustomer, 1);
            if (hasJustAchieved)
            {
                string message = GetNotificationMessage(AchievementType.AchievementNotLostACustomer);
                PushNotification(message);
            }

            m_CustomerNotLostPerLevelCounter = achievementUpdate.GetAchievementRecord(AchievementType.AchievementNotLostACustomer);
        }

        private void OnCustomerServed()
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.AchievementCustomersServed, 1);
            if (hasJustAchieved)
            {
                string message = GetNotificationMessage(AchievementType.AchievementCustomersServed);
                PushNotification(message);
            }

            m_CustomerServerdCounter = achievementUpdate.GetAchievementRecord(AchievementType.AchievementCustomersServed);

        }

        private void OnCustomerSitOrServedHappy()
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.AchievementServedHappy, 1);
            if (hasJustAchieved)
            {
                string message = GetNotificationMessage(AchievementType.AchievementServedHappy);
                PushNotification(message);
            }

            m_CustomerServerdHappyCounter = achievementUpdate.GetAchievementRecord(AchievementType.AchievementServedHappy);
        }

        private void OnCustomerOrderServed()
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.AchievementCustomerOrders, 1);
            if (hasJustAchieved)
            {
                string message = GetNotificationMessage(AchievementType.AchievementCustomerOrders);
                PushNotification(message);
            }

            m_CustomerOrderServedCounter = achievementUpdate.GetAchievementRecord(AchievementType.AchievementCustomerOrders);
        }

        private void OnNewCoinCurrencyAdded(int value)
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.AchievementCoinCurrencyEarned, value);
            if (hasJustAchieved)
            {
                string message = GetNotificationMessage(AchievementType.AchievementCoinCurrencyEarned);
                PushNotification(message);
            }

            m_CoinCurrencyCounter = achievementUpdate.GetAchievementRecord(AchievementType.AchievementCoinCurrencyEarned);
        }

        private void OnCoinCurrencySpendOnUpgrade(int value)
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.AchievementCoinCurrencySpendOnUpgrades, value);
            if (hasJustAchieved)
            {
                string message = GetNotificationMessage(AchievementType.AchievementCoinCurrencySpendOnUpgrades);
                PushNotification(message);
            }

            m_CoinCurrencyOnUpgradeCounter = achievementUpdate.GetAchievementRecord(AchievementType.AchievementCoinCurrencySpendOnUpgrades);
        }

        IEnumerator AchievementPlayForHours()
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            int target = achievementUpdate.GetAchievementTarget(AchievementType.AchievementPlayForHour);
            float targetTimeInHours = target * 60 * 60;
            m_ContinousPlayTimer = 0;

            while (m_ContinousPlayTimer < targetTimeInHours)
            {
                if (m_IsApplicationFocused)
                {
                    m_ContinousPlayTimer += Time.unscaledDeltaTime;
                }
                yield return null;
            }


            bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.AchievementPlayForHour, target);
            if (hasJustAchieved)
            {
                string message = GetNotificationMessage(AchievementType.AchievementPlayForHour);
                PushNotification(message);
            }
        }

        private void OnPlayerStartedMoving()
        {
            m_OnPlayerStartedMoving = StartCoroutine(AchievmentPlayerMoving());
        }

        private void OnPlayerStopMoving()
        {
            if (m_OnPlayerStartedMoving != null)
            {
                StopCoroutine(m_OnPlayerStartedMoving);
                m_OnPlayerStartedMoving = null;
            }
        }

        IEnumerator AchievmentPlayerMoving()
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            int target = achievementUpdate.GetAchievementTarget(AchievementType.AchievementPlayerMovingForSeconds);
            float targetTimeInSceonds = target;
            m_PlayerConstentlyMovingTimer = 0;

            while (m_PlayerConstentlyMovingTimer < targetTimeInSceonds)
            {
                if (m_IsApplicationFocused)
                {
                    m_PlayerConstentlyMovingTimer += Time.deltaTime;
                }
                yield return null;
            }

            bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.AchievementPlayerMovingForSeconds, target);
            if (hasJustAchieved)
            {
                string message = GetNotificationMessage(AchievementType.AchievementPlayerMovingForSeconds);
                PushNotification(message);
            }
        }

        private void OnWaitressSpeedBooster(bool isOnFastBooster)
        {
            m_IsOnFastBooster = isOnFastBooster;
        }

        private void OnCustomerOrderServedOnFastBooster()
        {
            if (m_IsOnFastBooster)
            {
                DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
                bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.AchievementCompleteOrdersOnFastBooster, 1);
                if (hasJustAchieved)
                {
                    string message = GetNotificationMessage(AchievementType.AchievementCompleteOrdersOnFastBooster);
                    PushNotification(message);
                }

                m_CustomerOrderOnFastBoosterCounter = achievementUpdate.GetAchievementRecord(AchievementType.AchievementCompleteOrdersOnFastBooster);
            }
        }

        private void OnWorkerServesOrder()
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.AchievementWorkerServesOrder, 1);
            if (hasJustAchieved)
            {
                string message = GetNotificationMessage(AchievementType.AchievementWorkerServesOrder);
                PushNotification(message);
            }

            m_WorkerOrderCounter = achievementUpdate.GetAchievementRecord(AchievementType.AchievementWorkerServesOrder);
        }

        private void OnACustomersWaitFrozen()
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            bool hasJustAchieved = achievementUpdate.SetAchievementRecord(AchievementType.AchievementFreezeTheCustomersWait, 1);
            if (hasJustAchieved)
            {
                string message = GetNotificationMessage(AchievementType.AchievementFreezeTheCustomersWait);
                PushNotification(message);
            }

            m_CustomersWaitFrozeCounter = achievementUpdate.GetAchievementRecord(AchievementType.AchievementFreezeTheCustomersWait);
        }

        #region Notfication
        private void PushNotification(string notificationMessage)
        {
            if (m_IsShowingNotification == false)
            {
                m_Canvas.SetActive(true);
                m_IsShowingNotification = true;
                m_NotificationText.text = notificationMessage;
                StartCoroutine(ShoingNotification(() =>
                {
                    m_IsShowingNotification = false;

                    if (m_NotificationQueue.Count > 0)
                    {
                        string message = m_NotificationQueue.Dequeue();
                        PushNotification(message);
                    }
                    else
                    {
                        m_Canvas.SetActive(false);
                    }
                }));
            }
            else
            {
                m_NotificationQueue.Enqueue(notificationMessage);
            }
        }

        #endregion

        [Button("Som")]
        public void Som()
        {
            StartCoroutine(ShoingNotification(null));
        }

        #region Menual Movment
        private IEnumerator ShoingNotification(Action action)
        {
            yield return MoveToPosition(m_Notification, m_NotificationShowPosition, m_NotificationMovmentDuration);
            yield return new WaitForSecondsRealtime(m_NotficationShowingDelay);
            yield return MoveToPosition(m_Notification, m_NotificationHidePosition, m_NotificationMovmentDuration);

            action?.Invoke();
        }

        private IEnumerator MoveToPosition(RectTransform target, Vector2 targetPos, float duration)
        {
            Vector2 startPos = target.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                target.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsed / duration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            target.anchoredPosition = targetPos;
        }
        #endregion

        #region Helper
        private bool IsAchievementComplete(AchievementType achievementType)
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            return achievementUpdate.IsAchievementComplete(achievementType);
        }
        private string GetNotificationMessage(AchievementType achievementType)
        {
            DataMapAchievementUpdate achievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            return achievementUpdate.GetAchievementNotificationMessage(achievementType);
        }
        #endregion

        private void OnApplicationFocus(bool focus)
        {
            m_IsApplicationFocused = focus;
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(AchievementObserverUIManager) + " is null");
        }
    }
}
