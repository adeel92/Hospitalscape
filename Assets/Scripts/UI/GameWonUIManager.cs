using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using DG.Tweening;
using NaughtyAttributes;
using Arc;
using Isometric.Data;
using Isometric.Reward;
using Isometric.Sound;

namespace Isometric.UI
{
    public class GameWonUIManager : UIPopupBase
    {
        [SerializeField] GameObject m_Popup;
        [SerializeField] Animator m_OpeningAnimator;
        [SerializeField] string m_OpeningAnimatorState;
        [SerializeField] TextMeshProUGUI m_CoinText;
        [SerializeField] GameObject m_Key1Holder;
        [SerializeField] GameObject m_Key1;
        [SerializeField] GameObject m_Key2Holder;
        [SerializeField] GameObject m_Key2;
        [SerializeField] RectTransform m_ContinueButton;
        [SerializeField] RectTransform m_VideoButton;

        [Header("---Currency---")]
        [SerializeField] RectTransform m_CoinCurrency;
        [SerializeField] Vector2 m_CoinCurrencyStartPosition;
        [SerializeField] Vector2 m_CoinCurrencyEndPosition;
        [SerializeField] float m_CoinCurrencyMoveDuration;

        [Header("---Collection---")]
        [SerializeField] float m_CollectionDuration = GlobalConstents.CollectionDuration;
        [SerializeField] float m_TargetBounceScale;
        [SerializeField] float m_TargetBounceDuration;
        [SerializeField] Ease m_TargetBounceEase;
        [SerializeField] Transform m_CollectableHolder;
        [SerializeField] GameObject m_CoinCollectabePrefab;
        [SerializeField] Transform m_CoinCollectabeStartPosition;
        [SerializeField] Transform m_CoinCollectabeEndPosition;
        [SerializeField] TextMeshProUGUI m_CoinCollectabeCountText;
        [SerializeField] Vector3 m_CollectableStartSize;
        [SerializeField] Vector3 m_CollectableEndSize;


        int m_VideoCoins = 0;
        bool m_HasKey1 = false;
        bool m_HasKey2 = false;

        public override void Setup() {}

        public override void OpenPopup(Action onComplete)
        {
            SoundManager.StopFadeOut(SoundType.GameMusic1, 0.3f, false);
            SoundManager.PlaySound(SoundType.GameWon, false, false);

            m_Popup.SetActive(true);
            m_CoinText.text = LevelManager.GetCollectedCoins().ToString();

            m_CoinCurrency.anchoredPosition = m_CoinCurrencyStartPosition;

            m_Key1Holder.SetActive(false);
            m_Key1.SetActive(false);
            m_Key2Holder.SetActive(false);
            m_Key2.SetActive(false);

            if (KeyRewardManager.IsUsingKeyReward())
            {
                m_Key1Holder.SetActive(true);
                m_Key2Holder.SetActive(true);

                int currentTargetValue = LevelManager.GetCurrentTargetValue();

                int key1RequiredValue = LevelManager.GetTargetKey1Value();
                int key2RequiredValue = LevelManager.GetTargetKey2Value();

                if (currentTargetValue >= key1RequiredValue)
                {
                    m_HasKey1 = true;
                    m_Key1.SetActive(true);
                }
                if (currentTargetValue >= key2RequiredValue)
                {
                    m_HasKey2 = true;
                    m_Key2.SetActive(true);
                }
            }

            GlobalFunctions.PlayAnimationWithCallback(this, m_OpeningAnimator, m_OpeningAnimatorState, () =>
            {
                m_OpeningAnimator.enabled = false;
                onComplete?.Invoke();
            });
        }

        public override void ClosePopup(Action onComplete)
        {
            m_Popup.SetActive(false);
            onComplete?.Invoke();
        }

        public void OnContinueButton()
        {
            int levelNumber = DataManager.CurrentMapLevelIndex + 1;
            FirebaseManager.LogEvent("Level_" + levelNumber + "_", FirebaseLogType.GameWon);

            int originalCoinCurrency = DataManager.CoinCurrency;
            int addedCoinCurrency = DataManager.CoinCurrency + LevelManager.GetCollectedCoins() + m_VideoCoins;

            bool hasIncreased = DataManager.IncreaseCurrentMapLevel();
            if (hasIncreased)
            {
                DataManager.StarCurrency += 1;
                DataManager.CoinCurrency += (LevelManager.GetCollectedCoins() + m_VideoCoins);
                GlobalEventHolder.OnNewCoinCurrencyAdded?.Invoke(LevelManager.GetCollectedCoins() + m_VideoCoins);
                GlobalEventHolder.OnLevelComplete?.Invoke();

                if (m_HasKey1)
                {
                    DataManager.KeyCurrency += 1;
                    GlobalEventHolder.OnKeyCollected?.Invoke();
                }

                if (m_HasKey2)
                {
                    DataManager.KeyCurrency += 1;
                    GlobalEventHolder.OnKeyCollected?.Invoke();
                }
            }

            HeartCurrencyUIController.SetValueForShouldBeMinusOne(false);

            DataManager.SaveData();

            m_CoinCollectabeCountText.text = originalCoinCurrency.ToString();

            SoundManager.PlaySound(SoundType.SmallWin);

            UIManager.UIInteractionOff();
            m_CoinCurrency.DOAnchorPos(m_CoinCurrencyEndPosition, m_CoinCurrencyMoveDuration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    CoinCollection(m_CoinCollectabeStartPosition.position, originalCoinCurrency, addedCoinCurrency, () =>
                    {
                        UIManager.RestartGame();
                    });
                }).SetEase(Ease.OutBack);
        }

        public void OnVideoButton()
        {
            int levelNumber = DataManager.CurrentMapLevelIndex + 1;
            FirebaseManager.LogEvent("Level_" + levelNumber + "_", FirebaseLogType.WonVideoReward);

            m_VideoCoins = Mathf.RoundToInt(LevelManager.GetCollectedCoins() * 0.3f);

            m_CoinText.text = (LevelManager.GetCollectedCoins() + m_VideoCoins).ToString();
            int countFrom = LevelManager.GetCollectedCoins();
            int countTo = LevelManager.GetCollectedCoins() + m_VideoCoins;
            m_CoinText.TextCounter(countFrom, countTo, 0.6f, CoroutineManager.Instance, false);

            Vector3 position = m_ContinueButton.anchoredPosition;
            position.x = 0;
            m_ContinueButton.anchoredPosition = position;

            SoundManager.PlaySound(SoundType.Reward);

            m_VideoButton.gameObject.SetActive(false);
        }

        #region Collectable Animation

        [Button]
        public void CollectionCoinCall()
        {
            CoinCollection(Vector3.zero, 100, 300, null);
        }

        [Button]
        public void CollectionGemCall()
        {
            CoinCollection(Vector3.zero, 100, 300, null);
        }

        public void CoinCollection(Vector3 startPositino, int currencyCurrent, int currencyFinalAmmount, Action onCompete)
        {
            int totalCount = 4;
            int counter = 0;

            Collection collection = CollectionUIManager.CollectCurve(totalCount,
                m_CollectionDuration,
                CurveType.CurveLeft,
                m_CoinCollectabePrefab,
                m_CollectableStartSize,
                m_CollectableEndSize,
                m_CollectableHolder,
                startPositino,
                m_CoinCollectabeEndPosition.position,
                false,
                () =>
                {
                    counter++;

                    int lerpValue = Mathf.RoundToInt(Mathf.Lerp(currencyCurrent, currencyFinalAmmount, ((float)counter / (float)totalCount)));
                    m_CoinCollectabeCountText.text = lerpValue.ToString();

                    m_CoinCollectabeEndPosition.DoBounceScale(Vector3.one, Vector3.one * m_TargetBounceScale, m_TargetBounceDuration).SetEase(m_TargetBounceEase);

                    SoundManager.PlaySound(SoundType.Coin);
                },
                () =>
                {
                    m_CoinCollectabeCountText.text = currencyFinalAmmount.ToString();
                    onCompete?.Invoke();
                });
        }
        #endregion

    }
}
