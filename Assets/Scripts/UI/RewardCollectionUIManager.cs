using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Arc;
using Isometric.Data;
using Isometric.Sound;

namespace Isometric.UI
{
    public class RewardCollectionUIManager : MonoBehaviour
    {
        [Serializable]
        private class TextStyleInfo
        {
            public RewardCollectionTextStyle TextStyle;
            public Material MaterialStyle;
        }

        [SerializeField] GameObject m_Popup;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;

        [SerializeField] float m_CollectionDuration;
        [SerializeField] float m_TargetBounceScale;
        [SerializeField] float m_TargetBounceDuration;
        [SerializeField] Ease m_TargetBounceEase;
        [SerializeField] Transform m_Holder;
        [SerializeField] Image m_VisiableBackground;
        [SerializeField] RectTransform m_AllRewardsHolder;
        [SerializeField] RectTransform m_FloatyRewardRectTransform;
        [SerializeField] PlayDoTween m_FloatyRewardTween;

        [Header("---Text Style---")]
        [SerializeField] List<TextStyleInfo> m_TextStyleInfos;
        [SerializeField] List<TextMeshProUGUI> m_TargetStyledTexts;


        [Header("---Coin---")]
        [SerializeField] GameObject m_CoinHolder;
        [SerializeField] Transform m_Coin;
        [SerializeField] TextMeshProUGUI m_CoinText;
        [SerializeField] GameObject m_CoinTargetHolder;
        [SerializeField] Transform m_CoinTarget;
        [SerializeField] TextMeshProUGUI m_CoinTargetText;
        [SerializeField] GameObject m_CoinCollectabePrefab;
        [SerializeField] Vector3 m_CoinCollectableStartSize;
        [SerializeField] Vector3  m_CoinCollectableEndSize;
        private int? m_CoinsToCollect = null;

        [Header("---Gem---")]
        [SerializeField] GameObject m_GemHolder;
        [SerializeField] Transform m_Gem;
        [SerializeField] TextMeshProUGUI m_GemText;
        [SerializeField] GameObject m_GemTargetHolder;
        [SerializeField] Transform m_GemTarget;
        [SerializeField] TextMeshProUGUI m_GemTargetText;
        [SerializeField] GameObject m_GemCollectabePrefab;
        [SerializeField] Vector3 m_GemCollectableStartSize;
        [SerializeField] Vector3 m_GemCollectableEndSize;
        private int? m_GemsToCollect = null;

        [Header("---Heart---")]
        [SerializeField] GameObject m_HeartHolder;
        [SerializeField] Transform m_Heart;
        [SerializeField] TextMeshProUGUI m_HeartText;
        [SerializeField] GameObject m_HeartTargetHolder;
        [SerializeField] Transform m_HeartTarget;
        [SerializeField] HeartCurrencyUIController m_HeartController;
        [SerializeField] GameObject m_HeartCollectabePrefab;
        [SerializeField] Vector3 m_HeartCollectableStartSize;
        [SerializeField] Vector3 m_HeartCollectableEndSize;
        private double? m_HeartTimeToCollect = null;

        [Header("---TimeFroze Booster---")]
        [SerializeField] GameObject m_TimeFrozeBoosterHolder;
        [SerializeField] Transform m_TimeFrozeBooster;
        [SerializeField] TextMeshProUGUI m_TimeFrozeBoosterText;
        [SerializeField] GameObject m_TimeFrozeBoosterTargetHolder;
        [SerializeField] Transform m_TimeFrozeBoosterTarget;
        [SerializeField] TextMeshProUGUI m_TimeFrozeBoosterTargetText;
        [SerializeField] GameObject m_TimeFrozeBoosterCollectabePrefab;
        [SerializeField] Vector3 m_TimeFrozeBoosterCollectableStartSize;
        [SerializeField] Vector3 m_TimeFrozeBoosterCollectableEndSize;
        private int? m_TimeForzeBoosterToCollect = null;

        [Header("---InstanceOrderFill Booster---")]
        [SerializeField] GameObject m_InstanceOrderFillBoosterHolder;
        [SerializeField] Transform m_InstanceOrderFillBooster;
        [SerializeField] TextMeshProUGUI m_InstanceOrderFillBoosterText;
        [SerializeField] GameObject m_InstanceOrderFillBoosterTargetHolder;
        [SerializeField] Transform m_InstanceOrderFillBoosterTarget;
        [SerializeField] TextMeshProUGUI m_InstanceOrderFillBoosterTargetText;
        [SerializeField] GameObject m_InstanceOrderFillBoosterCollectabePrefab;
        [SerializeField] Vector3 m_InstanceOrderFillBoosterCollectableStartSize;
        [SerializeField] Vector3 m_InstanceOrderFillBoosterCollectableEndSize;
        private int? m_InstanceOrderFillBoosterToCollect = null;

        [Header("---WaitressSpeed Booster---")]
        [SerializeField] GameObject m_WaitressSpeedBoosterHolder;
        [SerializeField] Transform m_WaitressSpeedBooster;
        [SerializeField] TextMeshProUGUI m_WaitressSpeedBoosterText;
        [SerializeField] GameObject m_WaitressSpeedBoosterTargetHolder;
        [SerializeField] Transform m_WaitressSpeedBoosterTarget;
        [SerializeField] TextMeshProUGUI m_WaitressSpeedBoosterTargetText;
        [SerializeField] GameObject m_WaitressSpeedBoosterCollectabePrefab;
        [SerializeField] Vector3 m_WaitressSpeedBoosterCollectableStartSize;
        [SerializeField] Vector3 m_WaitressSpeedBoosterCollectableEndSize;
        private int? m_WaitressSpeedBoosterToCollect = null;

        [Serializable]
        private class TextMessageInfo
        {
            public RewardCollectionTextPoistion TextPosition;
            public TextMeshProUGUI Text;
        }

        [Header("---Text---")]
        [SerializeField] List<TextMessageInfo> m_TextMessageInfos;

        private int m_OnCompleteCounter = 0;
        private Action onCollect = null;
        private Action onCloseComplete = null;


        private void Setup(int? coins,
            int? gems,
            double? heartTime,
            int? timeFrozeBooster,
            int? instanceOrderFillBooster,
            int? waitressSpeedBooster,
            Action onCollect,
            Action onCloseComplete)
        {
            m_OnCompleteCounter = 0;
            this.onCollect = onCollect;
            this.onCloseComplete = onCloseComplete;

            m_CoinTargetHolder.SetActive(false);
            m_CoinHolder.SetActive(false);
            m_GemTargetHolder.SetActive(false);
            m_GemHolder.SetActive(false);
            m_HeartTargetHolder.SetActive(false);
            m_HeartHolder.SetActive(false);
            m_TimeFrozeBoosterTargetHolder.SetActive(false);
            m_TimeFrozeBoosterHolder.SetActive(false);
            m_InstanceOrderFillBoosterTargetHolder.SetActive(false);
            m_InstanceOrderFillBoosterHolder.SetActive(false);
            m_WaitressSpeedBoosterTargetHolder.SetActive(false);
            m_WaitressSpeedBoosterHolder.SetActive(false);

            m_FloatyRewardTween.Stop();
            m_VisiableBackground.enabled = true;
            m_AllRewardsHolder.anchoredPosition = Vector2.zero;
            SetTextStyle(RewardCollectionTextStyle.Default); ;
            m_FloatyRewardRectTransform.anchoredPosition = Vector2.zero;

            m_CoinsToCollect = null;
            m_GemsToCollect = null;
            m_HeartTimeToCollect = null;
            m_TimeForzeBoosterToCollect = null;
            m_InstanceOrderFillBoosterToCollect = null;
            m_WaitressSpeedBoosterToCollect = null;

            m_CoinsToCollect = coins;
            m_GemsToCollect = gems;
            m_HeartTimeToCollect = heartTime;
            m_TimeForzeBoosterToCollect = timeFrozeBooster;
            m_InstanceOrderFillBoosterToCollect = instanceOrderFillBooster;
            m_WaitressSpeedBoosterToCollect = waitressSpeedBooster;

            if (coins != null)
            {
                m_CoinTargetText.text = ((DataManager.CoinCurrency - (int)coins) >= 0? (DataManager.CoinCurrency - (int)coins) : 0).ToString();
                m_CoinTargetHolder.SetActive(true);

                m_CoinText.text = coins.ToString();
                m_CoinHolder.SetActive(true);
            }

            if (gems != null)
            {
                m_GemTargetText.text = ((DataManager.GemCurrency - (int)gems) >= 0 ? (DataManager.GemCurrency - (int)gems) : 0).ToString();
                m_GemTargetHolder.SetActive(true);

                m_GemText.text = gems.ToString();
                m_GemHolder.SetActive(true);
            }

            if (heartTime != null)
            {
                m_HeartTargetHolder.SetActive(true);

                m_HeartText.text = GlobalFunctions.FormatSmartDurationShort((double)heartTime);
                m_HeartHolder.SetActive(true);

                m_HeartController.Setup();
            }

            if (timeFrozeBooster != null)
            {
                m_TimeFrozeBoosterTargetText.text = ((DataManager.TimeFrozeBoosterCount - (int)timeFrozeBooster) >= 0 ? (DataManager.TimeFrozeBoosterCount - (int)timeFrozeBooster) : 0).ToString();
                m_TimeFrozeBoosterTargetHolder.SetActive(true);

                m_TimeFrozeBoosterText.text = timeFrozeBooster.ToString();
                m_TimeFrozeBoosterHolder.SetActive(true);
            }

            if (instanceOrderFillBooster != null)
            {
                m_InstanceOrderFillBoosterTargetText.text = ((DataManager.InstanceOrderFillBoosterCount - (int)instanceOrderFillBooster) >= 0 ? (DataManager.InstanceOrderFillBoosterCount - (int)instanceOrderFillBooster) : 0).ToString();
                m_InstanceOrderFillBoosterTargetHolder.SetActive(true);

                m_InstanceOrderFillBoosterText.text = instanceOrderFillBooster.ToString();
                m_InstanceOrderFillBoosterHolder.SetActive(true);
            }

            if (waitressSpeedBooster != null)
            {
                m_WaitressSpeedBoosterTargetText.text = ((DataManager.WaitressSpeedBoosterCount - (int)waitressSpeedBooster) >= 0 ? (DataManager.WaitressSpeedBoosterCount - (int)waitressSpeedBooster) : 0).ToString();
                m_WaitressSpeedBoosterTargetHolder.SetActive(true);

                m_WaitressSpeedBoosterText.text = waitressSpeedBooster.ToString();
                m_WaitressSpeedBoosterHolder.SetActive(true);
            }
        }


        private void Setup(int? coins,
            int? gems,
            double? heartTime,
            int? timeFrozeBooster,
            int? instanceOrderFillBooster,
            int? waitressSpeedBooster,
            bool isBackgroundVisible,
            Vector2 middleRewardRectTransformOffset,
            RewardCollectionTextStyle textStyle,
            bool enableFloatyRewardTween,
            Action onCollect,
            Action onCloseComplete)
        {
            m_OnCompleteCounter = 0;
            this.onCollect = onCollect;
            this.onCloseComplete = onCloseComplete;

            m_CoinTargetHolder.SetActive(false);
            m_CoinHolder.SetActive(false);
            m_GemTargetHolder.SetActive(false);
            m_GemHolder.SetActive(false);
            m_HeartTargetHolder.SetActive(false);
            m_HeartHolder.SetActive(false);
            m_TimeFrozeBoosterTargetHolder.SetActive(false);
            m_TimeFrozeBoosterHolder.SetActive(false);
            m_InstanceOrderFillBoosterTargetHolder.SetActive(false);
            m_InstanceOrderFillBoosterHolder.SetActive(false);
            m_WaitressSpeedBoosterTargetHolder.SetActive(false);
            m_WaitressSpeedBoosterHolder.SetActive(false);

            if (isBackgroundVisible)
            {
                m_VisiableBackground.enabled = true;
            }
            else
            {
                m_VisiableBackground.enabled = false;
            }

            m_AllRewardsHolder.anchoredPosition = Vector2.zero;
            m_AllRewardsHolder.anchoredPosition = m_AllRewardsHolder.anchoredPosition + middleRewardRectTransformOffset;
            SetTextStyle(textStyle);

            if (enableFloatyRewardTween)
            {
                m_FloatyRewardTween.Play();
            }
            else
            {
                m_FloatyRewardTween.Stop();
                m_FloatyRewardRectTransform.anchoredPosition = Vector2.zero;
            }

            m_CoinsToCollect = null;
            m_GemsToCollect = null;
            m_HeartTimeToCollect = null;
            m_TimeForzeBoosterToCollect = null;
            m_InstanceOrderFillBoosterToCollect = null;
            m_WaitressSpeedBoosterToCollect = null;

            m_CoinsToCollect = coins;
            m_GemsToCollect = gems;
            m_HeartTimeToCollect = heartTime;
            m_TimeForzeBoosterToCollect = timeFrozeBooster;
            m_InstanceOrderFillBoosterToCollect = instanceOrderFillBooster;
            m_WaitressSpeedBoosterToCollect = waitressSpeedBooster;

            if (coins != null)
            {
                m_CoinTargetText.text = ((DataManager.CoinCurrency - (int)coins) >= 0 ? (DataManager.CoinCurrency - (int)coins) : 0).ToString();
                m_CoinTargetHolder.SetActive(true);

                m_CoinText.text = coins.ToString();
                m_CoinHolder.SetActive(true);
            }

            if (gems != null)
            {
                m_GemTargetText.text = ((DataManager.GemCurrency - (int)gems) >= 0 ? (DataManager.GemCurrency - (int)gems) : 0).ToString();
                m_GemTargetHolder.SetActive(true);

                m_GemText.text = gems.ToString();
                m_GemHolder.SetActive(true);
            }

            if (heartTime != null)
            {
                m_HeartTargetHolder.SetActive(true);

                m_HeartText.text = GlobalFunctions.FormatSmartDurationShort((double)heartTime);
                m_HeartHolder.SetActive(true);

                m_HeartController.Setup();
            }

            if (timeFrozeBooster != null)
            {
                m_TimeFrozeBoosterTargetText.text = ((DataManager.TimeFrozeBoosterCount - (int)timeFrozeBooster) >= 0 ? (DataManager.TimeFrozeBoosterCount - (int)timeFrozeBooster) : 0).ToString();
                m_TimeFrozeBoosterTargetHolder.SetActive(true);

                m_TimeFrozeBoosterText.text = timeFrozeBooster.ToString();
                m_TimeFrozeBoosterHolder.SetActive(true);
            }

            if (instanceOrderFillBooster != null)
            {
                m_InstanceOrderFillBoosterTargetText.text = ((DataManager.InstanceOrderFillBoosterCount - (int)instanceOrderFillBooster) >= 0 ? (DataManager.InstanceOrderFillBoosterCount - (int)instanceOrderFillBooster) : 0).ToString();
                m_InstanceOrderFillBoosterTargetHolder.SetActive(true);

                m_InstanceOrderFillBoosterText.text = instanceOrderFillBooster.ToString();
                m_InstanceOrderFillBoosterHolder.SetActive(true);
            }

            if (waitressSpeedBooster != null)
            {
                m_WaitressSpeedBoosterTargetText.text = ((DataManager.WaitressSpeedBoosterCount - (int)waitressSpeedBooster) >= 0 ? (DataManager.WaitressSpeedBoosterCount - (int)waitressSpeedBooster) : 0).ToString();
                m_WaitressSpeedBoosterTargetHolder.SetActive(true);

                m_WaitressSpeedBoosterText.text = waitressSpeedBooster.ToString();
                m_WaitressSpeedBoosterHolder.SetActive(true);
            }
        }

        /// <summary>
        /// Send null value if dont reward that value
        /// </summary>
        public void Open(int? coins,
            int? gems,
            double? heartTime,
            int? timeFrozeBooster,
            int? instanceOrderFillBooster,
            int? waitressSpeedBooster,
            Action onComplete,
            Action onCollect,
            Action onCloseComplete)
        {

            Setup(coins, gems, heartTime, timeFrozeBooster, instanceOrderFillBooster, waitressSpeedBooster, onCollect, onCloseComplete);

            m_Popup.SetActive(true);
            m_OpeningSequence.PlaySequence(() =>
            {
                onComplete?.Invoke();
            });
        }

        public void Open(int? coins,
            int? gems,
            double? heartTime,
            int? timeFrozeBooster,
            int? instanceOrderFillBooster,
            int? waitressSpeedBooster,
            bool isBackgroundVisible,
            Vector2 middleRewardRectTransformOffset,
            RewardCollectionTextStyle textStyle,
            bool enableFloatyRewardTween,
            Action onComplete,
            Action onCollect,
            Action onCloseComplete)
        {

            Setup(coins, gems, heartTime, timeFrozeBooster, instanceOrderFillBooster, waitressSpeedBooster, isBackgroundVisible, middleRewardRectTransformOffset, textStyle, enableFloatyRewardTween, onCollect, onCloseComplete);

            m_Popup.SetActive(true);
            m_OpeningSequence.PlaySequence(() =>
            {
                onComplete?.Invoke();
            });
        }

        public void SetTextMessage(string message, RewardCollectionTextPoistion textPostion)
        {
            DisableTextMessages();

            foreach (var textMessageInfo in m_TextMessageInfos)
            {
                if (textMessageInfo.TextPosition == textPostion)
                {
                    textMessageInfo.Text.text = message;
                    textMessageInfo.Text.gameObject.SetActive(true);
                    break;
                }
            }
        }

        private void DisableTextMessages()
        {
            foreach (var textMessageInfo in m_TextMessageInfos)
            {
                textMessageInfo.Text.gameObject.SetActive(false);
            }
        }

        public void OnCollectButton()
        {
            UIManager.UIInteractionOff();

            onCollect?.Invoke();

            if (m_CoinsToCollect != null)
            {
                int totalCoun = 4;
                int counter = 0;

                int currentCurrency = DataManager.CoinCurrency - (int)m_CoinsToCollect >= 0 ? DataManager.CoinCurrency - (int)m_CoinsToCollect : 0;
                int finalCurrency = DataManager.CoinCurrency;

                m_OnCompleteCounter++;

                Collection collection = CollectionUIManager.CollectCurve(totalCoun,
                m_CollectionDuration,
                CurveType.CurveLeft,
                m_CoinCollectabePrefab,
                m_CoinCollectableStartSize,
                m_CoinCollectableEndSize,
                m_Holder,
                m_Coin.position,
                m_CoinTarget.position,
                false,
                () =>
                {
                    counter++;

                    int lerpValue = Mathf.RoundToInt(Mathf.Lerp(currentCurrency, finalCurrency, ((float)counter / (float)totalCoun)));
                    m_CoinTargetText.text = lerpValue.ToString();

                    m_CoinTarget.DoBounceScale(Vector3.one, Vector3.one * m_TargetBounceScale, m_TargetBounceDuration).SetEase(m_TargetBounceEase);

                    SoundManager.PlaySound(SoundType.Coin);
                },
                () =>
                {
                    m_CoinTargetText.text = finalCurrency.ToString();
                    OnCollectComplete();
                });
            }

            if (m_GemsToCollect != null)
            {
                int totalCoun = Mathf.Min(4, (int)m_GemsToCollect);
                int counter = 0;

                int currentCurrency = DataManager.GemCurrency - (int)m_GemsToCollect >= 0 ? DataManager.GemCurrency - (int)m_GemsToCollect : 0;
                int finalCurrency = DataManager.GemCurrency;

                m_OnCompleteCounter++;

                Collection collection = CollectionUIManager.CollectCurve(totalCoun,
                m_CollectionDuration,
                CurveType.CurveUp,
                m_GemCollectabePrefab,
                m_GemCollectableStartSize,
                m_GemCollectableEndSize,
                m_Holder,
                m_Gem.position,
                m_GemTarget.position,
                false,
                () =>
                {
                    counter++;

                    int lerpValue = Mathf.RoundToInt(Mathf.Lerp(currentCurrency, finalCurrency, ((float)totalCoun / (float)counter)));
                    m_GemTargetText.text = lerpValue.ToString();

                    m_GemTarget.DoBounceScale(Vector3.one, Vector3.one * m_TargetBounceScale, m_TargetBounceDuration).SetEase(m_TargetBounceEase);

                    SoundManager.PlaySound(SoundType.Gem);
                },
                () =>
                {
                    m_GemTargetText.text = finalCurrency.ToString();
                    OnCollectComplete();
                });
            }

            if (m_HeartTimeToCollect != null)
            {
                m_OnCompleteCounter++;

                Collection collection = CollectionUIManager.CollectCurve(1,
                m_CollectionDuration,
                CurveType.CurveUp,
                m_HeartCollectabePrefab,
                m_HeartCollectableStartSize,
                m_HeartCollectableEndSize,
                m_Holder,
                m_Heart.position,
                m_HeartTarget.position,
                false,
                () =>
                {
                    m_HeartController.Setup();
                    m_HeartTarget.DoBounceScale(Vector3.one, Vector3.one * m_TargetBounceScale, m_TargetBounceDuration).SetEase(m_TargetBounceEase);
                },
                () =>
                {
                    OnCollectComplete();
                });
            }

            if (m_TimeForzeBoosterToCollect != null)
            {
                int totalCount = Mathf.Min(4, (int)m_TimeForzeBoosterToCollect);
                int counter = 0;

                int currentCurrency = DataManager.TimeFrozeBoosterCount - (int)m_TimeForzeBoosterToCollect >= 0 ? DataManager.TimeFrozeBoosterCount - (int)m_TimeForzeBoosterToCollect : 0;
                int finalCurrency = DataManager.TimeFrozeBoosterCount;

                m_OnCompleteCounter++;

                Collection collection = CollectionUIManager.CollectCurve(totalCount,
                m_CollectionDuration,
                CurveType.CurveDown,
                m_TimeFrozeBoosterCollectabePrefab,
                m_TimeFrozeBoosterCollectableStartSize,
                m_TimeFrozeBoosterCollectableEndSize,
                m_Holder,
                m_TimeFrozeBooster.position,
                m_TimeFrozeBoosterTarget.position,
                false,
                () =>
                {
                    counter++;

                    int lerpValue = Mathf.RoundToInt(Mathf.Lerp(currentCurrency, finalCurrency, ((float)totalCount / (float)counter)));
                    m_TimeFrozeBoosterTargetText.text = lerpValue.ToString();

                    m_TimeFrozeBoosterTarget.DoBounceScale(Vector3.one, Vector3.one * m_TargetBounceScale, m_TargetBounceDuration).SetEase(m_TargetBounceEase);
                },
                () =>
                {
                    m_TimeFrozeBoosterTargetText.text = finalCurrency.ToString();
                    OnCollectComplete();
                });
            }

            if (m_InstanceOrderFillBoosterToCollect != null)
            {
                int totalCount = Mathf.Min(4, (int)m_InstanceOrderFillBoosterToCollect);
                int counter = 0;

                int currentCurrency = DataManager.InstanceOrderFillBoosterCount - (int)m_InstanceOrderFillBoosterToCollect >= 0 ? DataManager.InstanceOrderFillBoosterCount - (int)m_InstanceOrderFillBoosterToCollect : 0;
                int finalCurrency = DataManager.InstanceOrderFillBoosterCount;

                m_OnCompleteCounter++;

                Collection collection = CollectionUIManager.CollectCurve(totalCount,
                m_CollectionDuration,
                CurveType.CurveDown,
                m_InstanceOrderFillBoosterCollectabePrefab,
                m_InstanceOrderFillBoosterCollectableStartSize,
                m_InstanceOrderFillBoosterCollectableEndSize,
                m_Holder,
                m_InstanceOrderFillBooster.position,
                m_InstanceOrderFillBoosterTarget.position,
                false,
                () =>
                {
                    counter++;

                    int lerpValue = Mathf.RoundToInt(Mathf.Lerp(currentCurrency, finalCurrency, ((float)totalCount / (float)counter)));
                    m_InstanceOrderFillBoosterTargetText.text = lerpValue.ToString();

                    m_InstanceOrderFillBoosterTarget.DoBounceScale(Vector3.one, Vector3.one * m_TargetBounceScale, m_TargetBounceDuration).SetEase(m_TargetBounceEase);
                },
                () =>
                {
                    m_InstanceOrderFillBoosterTargetText.text = finalCurrency.ToString();
                    OnCollectComplete();
                });
            }

            if (m_WaitressSpeedBoosterToCollect != null)
            {
                int totalCount = Mathf.Min(4, (int)m_WaitressSpeedBoosterToCollect);
                int counter = 0;

                int currentCurrency = DataManager.WaitressSpeedBoosterCount - (int)m_WaitressSpeedBoosterToCollect >= 0 ? DataManager.WaitressSpeedBoosterCount - (int)m_WaitressSpeedBoosterToCollect : 0;
                int finalCurrency = DataManager.WaitressSpeedBoosterCount;

                m_OnCompleteCounter++;

                Collection collection = CollectionUIManager.CollectCurve(totalCount,
                m_CollectionDuration,
                CurveType.CurveDown,
                m_WaitressSpeedBoosterCollectabePrefab,
                m_WaitressSpeedBoosterCollectableStartSize,
                m_WaitressSpeedBoosterCollectableEndSize,
                m_Holder,
                m_WaitressSpeedBooster.position,
                m_WaitressSpeedBoosterTarget.position,
                false,
                () =>
                {
                    counter++;

                    int lerpValue = Mathf.RoundToInt(Mathf.Lerp(currentCurrency, finalCurrency, ((float)totalCount / (float)counter)));
                    m_WaitressSpeedBoosterTargetText.text = lerpValue.ToString();

                    m_WaitressSpeedBoosterTarget.DoBounceScale(Vector3.one, Vector3.one * m_TargetBounceScale, m_TargetBounceDuration).SetEase(m_TargetBounceEase);
                },
                () =>
                {
                    m_WaitressSpeedBoosterTargetText.text = finalCurrency.ToString();
                    OnCollectComplete();
                });
            }
        }

        private void OnCollectComplete()
        {
            m_OnCompleteCounter--;
            if (m_OnCompleteCounter < 1)
            {
                UIManager.CloseRewardCollectionPopup();
            }
        }

        public void ClosePopup(Action onComplete)
        {
            m_ClosingSequence.PlaySequence(() =>
            {
                onCloseComplete?.Invoke();

                onCollect = null;
                onCloseComplete = null;
                m_CoinsToCollect = null;
                m_GemsToCollect = null;
                m_HeartTimeToCollect = null;
                m_TimeForzeBoosterToCollect = null;
                m_InstanceOrderFillBoosterToCollect = null;
                m_WaitressSpeedBoosterToCollect = null;

                DisableTextMessages();

                UIManager.UIInteractionOn();

                m_Popup.SetActive(false);
                onComplete?.Invoke();
            });
        }


        private void SetTextStyle(RewardCollectionTextStyle textStyle)
        {
            TextStyleInfo textStyleInfo = m_TextStyleInfos.Find((x) => x.TextStyle == textStyle);

            if (textStyleInfo != null)
            {
                foreach (var texts in m_TargetStyledTexts)
                {
                    texts.fontMaterial = textStyleInfo.MaterialStyle;
                }
            }
            else
            {
                Debug.LogWarning("Target text of type " + textStyle + " not found");
            }
        }
    }

    public enum RewardCollectionTextPoistion
    {
        FarUpMiddle, NearUpMiddle, FarDownMiddle, NearDownMiddle, CenterMiddle
    }

    public enum RewardCollectionTextStyle
    {
        Default, Orange1, Orange2
    }

}
