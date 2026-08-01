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
        [SerializeField] GameObject m_WonPanel;
        [SerializeField] GameObject m_CharacterPanel;
        [SerializeField] Animator m_OpeningAnimator;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;
        [SerializeField] string m_OpeningAnimatorState;
        [SerializeField] TextMeshProUGUI m_CoinText;
        [SerializeField] GameObject m_Key1Holder;
        [SerializeField] GameObject m_Key1;
        [SerializeField] GameObject m_Key2Holder;
        [SerializeField] GameObject m_Key2;
        [SerializeField] RectTransform m_ContinueButton;
        [SerializeField] RectTransform m_VideoButton;
        [SerializeField] GameObject m_OuterParticles;

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

        [Header("---Key Collection---")]
        [SerializeField] int m_RequiredKeysToOpenChestBoxReward;
        [SerializeField] TextMeshProUGUI m_NumberOfKeysText;
        [SerializeField] string m_OneKeyCollectionAnimatorState;
        [SerializeField] string m_TwoKeyCollectionAnimatorState;
        [SerializeField] string m_TransitionToCollectAnimatorState;
        [SerializeField] string m_CollectChestRewardAnimatorState;
        [SerializeField] GameObject m_KeyRewardCollectionButton;
        [SerializeField] GameObject m_TapToCollectText;
        [SerializeField] ParticleSystem m_ChestBoxAuraEffect;
        [SerializeField] List<KeyCollectionReward> m_KeyCollectionRewards;


        int m_VideoCoins = 0;
        bool m_HasKey1 = false;
        bool m_HasKey2 = false;

        public override void Setup() {}

        public override void OpenPopup(Action onComplete)
        {
            SoundManager.StopFadeOut(SoundType.GameMusic1, 0.3f, false);
            SoundManager.PlaySound(SoundType.GameWon, false, false);

            m_VideoButton.gameObject.SetActive(true);
            m_ContinueButton.gameObject.SetActive(true);

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

            m_OpeningSequence.PlaySequence(() =>
            {
               onComplete?.Invoke(); 
            });

            // GlobalFunctions.PlayAnimationWithCallback(this, m_OpeningAnimator, m_OpeningAnimatorState, () =>
            // {
            //     m_OpeningAnimator.enabled = false;
            //     onComplete?.Invoke();
            // });
        }

        public override void ClosePopup(Action onComplete)
        {
            m_ClosingSequence.PlaySequence(() =>
            {
                m_Popup.SetActive(false);
                onComplete?.Invoke();
            });
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
                        if (KeyRewardManager.IsUsingKeyReward())
                        {
                            KeysHandling();
                        }
                        else
                        {
                            UIManager.RestartGame();
                        }
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

        private void KeysHandling()
        {
            if (m_HasKey1 && m_HasKey2)
            {
                m_NumberOfKeysText.text = (DataManager.KeyCurrency - 2) + "/" + m_RequiredKeysToOpenChestBoxReward;
                SetChsetSettings();
                GlobalFunctions.PlayAnimationWithCallbackUpdate(this, m_OpeningAnimator, m_TwoKeyCollectionAnimatorState, () =>
                {
                    SetForCollection();
                });
            }
            else if (m_HasKey1)
            {
                m_NumberOfKeysText.text = (DataManager.KeyCurrency - 1) + "/" + m_RequiredKeysToOpenChestBoxReward;
                SetChsetSettings();
                GlobalFunctions.PlayAnimationWithCallbackUpdate(this, m_OpeningAnimator, m_OneKeyCollectionAnimatorState, () =>
                {
                    SetForCollection();
                });
            }
            else if (DataManager.KeyCurrency >= m_RequiredKeysToOpenChestBoxReward)
            {
                SetChsetSettings();
                GlobalFunctions.PlayAnimationWithCallbackUpdate(this, m_OpeningAnimator, m_TransitionToCollectAnimatorState, () =>
                {
                    SetForCollection();
                });
            }
            else
            {
                UIManager.RestartGame();
            }
        }

        private void SetChsetSettings()
        {
            m_CoinCurrency.DOAnchorPos(m_CoinCurrencyStartPosition, m_CoinCurrencyMoveDuration).SetUpdate(true);
            m_WonPanel.SetActive(false);
            m_CharacterPanel.SetActive(false);
            m_OuterParticles.SetActive(false);
        }

        private void SetForCollection()
        {
            if (DataManager.KeyCurrency >= m_RequiredKeysToOpenChestBoxReward)
            {
                m_ChestBoxAuraEffect.Play();
                UIManager.UIInteractionOn();
                m_TapToCollectText.SetActive(true);
                m_KeyRewardCollectionButton.SetActive(true);
            }
            else
            {
                CoroutineManager.LateActionRealTime(() =>
                {
                    UIManager.RestartGame();
                }, 1);
            }
        }

        public void OnRewardCollectionButton()
        {
            m_TapToCollectText.SetActive(false);
            m_KeyRewardCollectionButton.SetActive(false);

            KeyCollectionReward keyCollectionReward = m_KeyCollectionRewards[UnityEngine.Random.Range(0, m_KeyCollectionRewards.Count)];
            if (keyCollectionReward != null)
            {
                int? coins = null;
                int? gems = null;

                if (keyCollectionReward.HasCoinReward)
                {
                    coins = keyCollectionReward.CoinReward;
                    DataManager.CoinCurrency += keyCollectionReward.CoinReward;
                }

                if (keyCollectionReward.HasGemReward)
                {
                    gems = keyCollectionReward.GemReward;
                    DataManager.GemCurrency += keyCollectionReward.GemReward;
                }

                DataManager.KeyCurrency -= m_RequiredKeysToOpenChestBoxReward;
                DataManager.SaveData();

                SoundManager.PlaySound(SoundType.MediumReward);

                UIManager.UIInteractionOff();
                GlobalFunctions.PlayAnimationWithCallbackUpdate(this, m_OpeningAnimator, m_CollectChestRewardAnimatorState, () =>
                {
                    UIManager.SetRewardCollectionPopupMessage("Tap to Collect!", RewardCollectionTextPoistion.FarDownMiddle);
                    UIManager.OpenRewardCollectionPopup(coins, gems,
                        null, null, null, null, null,
                        () =>
                        {
                            SoundManager.PlaySound(SoundType.Reward);
                        },
                        () =>
                        {
                            CoroutineManager.LateActionRealTime(() =>
                            {
                                UIManager.RestartGame();
                            },1);
                        });
                });
            }
            else
            {
                CoroutineManager.LateActionRealTime(() =>
                {
                    UIManager.RestartGame();
                }, 1);
            }
        }

        public void OnAnimationCallbackOneKey()
        {
            m_NumberOfKeysText.text = (DataManager.KeyCurrency) + "/" + m_RequiredKeysToOpenChestBoxReward;
            SoundManager.PlaySound(SoundType.Coin);
        }

        public void OnAnimationCallbackTwoKey_FirstKeyCollection()
        {
            m_NumberOfKeysText.text = (DataManager.KeyCurrency - 1) + "/" + m_RequiredKeysToOpenChestBoxReward;
            SoundManager.PlaySound(SoundType.Coin);
        }

        public void OnAnimationCallbackTwoKey_SecondKeyCollection()
        {
            m_NumberOfKeysText.text = (DataManager.KeyCurrency) + "/" + m_RequiredKeysToOpenChestBoxReward;
            SoundManager.PlaySound(SoundType.Coin);
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

    [Serializable]
    public class KeyCollectionReward
    {
        public bool HasCoinReward;
        [AllowNesting, NaughtyAttributes.ShowIf(nameof(HasCoinReward))]
        public int CoinReward;
        public bool HasGemReward;
        [AllowNesting, NaughtyAttributes.ShowIf(nameof(HasGemReward))]
        public int GemReward;
    }
}
