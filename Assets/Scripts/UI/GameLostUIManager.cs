using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using DG.Tweening;
using NaughtyAttributes;
using Arc;
using Isometric.Data;
using Isometric.Sound;

namespace Isometric.UI
{
    public class GameLostUIManager : UIPopupBase
    {
        [Header("---Reason for GameLost Popup---")]
        [SerializeField] int m_ShowTillLevelIndex;
        [SerializeField] GameObject m_ReasonForGameLostPopup;
        [SerializeField] PlayDoTweenSequence m_OpeningSequenceReasonForGameLostPopup;
        [SerializeField] PlayDoTweenSequence m_ClosingSequenceReasonForGameLostPopup;
        [SerializeField] GameObject m_TimeFailure;
        [SerializeField] GameObject m_CustomerFailure;
        [SerializeField] GameObject m_DoNotLoseCustomerFailure;

        [Header("---Lost Popup---")]
        [SerializeField] GameObject m_GameLostPopup;
        [SerializeField] Animator m_OpeningAnimator;
        [SerializeField] string m_OpeningAnimatorState;
        [SerializeField] float m_FailedRewardMultiplier;
        [SerializeField] TextMeshProUGUI m_CoinText;
        [SerializeField] int m_VideoReward;
        [SerializeField] RectTransform m_RewardButton;
        [SerializeField] TextMeshProUGUI m_RewardButtonText;
        [SerializeField] RectTransform m_ContinueButton;
        [SerializeField] float m_ButtonsScaleDuration;
        [SerializeField] Ease m_ButtonsScaleEase;

        [Header("---Currency---")]
        [SerializeField] RectTransform m_CoinCurrency;
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

        bool m_HasReward = false;

        public override void Setup(){}

        public override void OpenPopup(Action onComplete)
        {
            GlobalEventHolder.OnPlayerStopMoving?.Invoke();

            int levelIndex = DataManager.CurrentMapLevelIndex;

            Debug.Log("----------Game Ended Failed");

            SoundManager.StopFadeOut(SoundType.GameMusic1, 0.3f, false);
            if (levelIndex > m_ShowTillLevelIndex)
            {
                OpenGameLostPopup(onComplete);
            }
            else
            {
                OpenReasonForGameLostPopup(onComplete);
            }
        }

        private void OpenReasonForGameLostPopup(Action onComplete)
        {
            m_ReasonForGameLostPopup.SetActive(true);
            m_TimeFailure.SetActive(false);
            m_CustomerFailure.SetActive(false);
            m_DoNotLoseCustomerFailure.SetActive(false);
            
            LevelLostReason levelLostReason = LevelManager.GetLevelLostReason();

            if(levelLostReason == LevelLostReason.NoMoreTime)
            {
                m_TimeFailure.SetActive(true);
            }
            else if (levelLostReason == LevelLostReason.NoMoreCustomers)
            {
                m_CustomerFailure.SetActive(true);
            }
            else if(levelLostReason == LevelLostReason.LostACustomer)
            {
                m_DoNotLoseCustomerFailure.SetActive(true);
            }

            m_OpeningSequenceReasonForGameLostPopup.PlaySequence(() =>
            {
                onComplete?.Invoke();
            });
        }

        public void OnReasonForGameLostPopupCloseButton()
        {
            UIManager.UIInteractionOff();
            m_ClosingSequenceReasonForGameLostPopup.PlaySequence(() =>
            {
                m_ReasonForGameLostPopup.SetActive(false);
                OpenGameLostPopup(() =>
                {
                    UIManager.UIInteractionOn();
                });
            });
        }

        private void OpenGameLostPopup(Action onComplete)
        {
            SoundManager.PlaySound(SoundType.GameLost, false, false);

            m_GameLostPopup.SetActive(true);
            m_CoinText.text = Mathf.RoundToInt((LevelManager.GetCollectedCoins() * m_FailedRewardMultiplier)).ToString();
            int coins = Mathf.RoundToInt(LevelManager.GetCollectedCoins() * m_FailedRewardMultiplier);
            int rewardCoins = (coins * m_VideoReward) - coins;
            m_RewardButtonText.text = "+" + rewardCoins;
            m_RewardButton.gameObject.SetActive(false);
            m_ContinueButton.gameObject.SetActive(false);

            GlobalFunctions.PlayAnimationWithCallbackUpdate(this, m_OpeningAnimator, m_OpeningAnimatorState, () =>
            {
                if (false)
                {
                    m_ContinueButton.gameObject.SetActive(true);
                    Vector3 position = m_ContinueButton.anchoredPosition;
                    position.x = 0;
                    m_ContinueButton.anchoredPosition = position;
                    m_ContinueButton.localScale = Vector3.zero;
                    m_ContinueButton.DOScale(Vector3.one, m_ButtonsScaleDuration)
                    .SetEase(m_ButtonsScaleEase)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        onComplete?.Invoke();
                    });
                }
                else
                {
                    m_ContinueButton.gameObject.SetActive(true);
                    m_RewardButton.gameObject.SetActive(true);
                    m_ContinueButton.localScale = Vector3.zero;
                    m_RewardButton.localScale = Vector3.zero;

                    m_ContinueButton.DOScale(Vector3.one, m_ButtonsScaleDuration)
                    .SetEase(m_ButtonsScaleEase)
                    .SetUpdate(true);
                    m_RewardButton.DOScale(Vector3.one, m_ButtonsScaleDuration)
                    .SetEase(m_ButtonsScaleEase)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        onComplete?.Invoke();
                    });
                }
            });
        }

        public override void ClosePopup(Action onComplete)
        {
            m_GameLostPopup.SetActive(false);
            onComplete?.Invoke();
        }

        public void OnRewardButton()
        {
            m_CoinText.text = (Mathf.RoundToInt((LevelManager.GetCollectedCoins() * m_FailedRewardMultiplier)) * m_VideoReward).ToString();
            int countFrom = Mathf.RoundToInt(LevelManager.GetCollectedCoins() * m_FailedRewardMultiplier);
            int countTo = Mathf.RoundToInt((LevelManager.GetCollectedCoins() * m_FailedRewardMultiplier)) * m_VideoReward;
            m_CoinText.TextCounter(countFrom, countTo, 0.6f, CoroutineManager.Instance, false);
            m_HasReward = true;

            Vector2 postion = m_ContinueButton.anchoredPosition;
            postion.x = 0;

            m_ContinueButton.anchoredPosition = postion;

            SoundManager.PlaySound(SoundType.Reward);

            m_RewardButton.gameObject.SetActive(false);
        }

        public void OnContinueButton()
        {
            int coins = Mathf.RoundToInt(LevelManager.GetCollectedCoins() * m_FailedRewardMultiplier);
            if (m_HasReward)
            {
                coins = coins * m_VideoReward;
            }
            
            int originalCoinCurrency = DataManager.CoinCurrency;
            int addedCoinCurrency = DataManager.CoinCurrency +  coins;

            DataManager.CoinCurrency += coins;
            GlobalEventHolder.OnNewCoinCurrencyAdded?.Invoke(coins);


            if (HeartTimeCurrencyCounter.HasHeartTimeCurrency() == false)
            {
                if (DataManager.HeartCurrency > 0)
                {
                    HeartCurrencyUIController.CheckLastTimeSaved();
                    DataManager.HeartCurrency--;
                }
            }

            HeartCurrencyUIController.SetValueForShouldBeMinusOne(false);

            DataManager.SaveData();

            m_CoinCollectabeCountText.text = originalCoinCurrency.ToString();

            UIManager.UIInteractionOff();
            m_CoinCurrency.DOAnchorPos(m_CoinCurrencyEndPosition, m_CoinCurrencyMoveDuration)
               .OnComplete(() =>
               {
                   CoinCollection(m_CoinCollectabeStartPosition.position, originalCoinCurrency, addedCoinCurrency, () =>
                   {
                       UIManager.RestartGame();
                   });
               }).SetEase(Ease.OutBack).SetUpdate(true);
        }


        #region Collectable Animation
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
