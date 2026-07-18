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
using UnityEngine.UI;
using Achievement = Isometric.UI.AchievementPanelType;


namespace Isometric.UI
{
    public class AchievementUIManager : UIPopupBase
    {
        public enum AchievementTabs
        {
            Patient, Achievement
        }

        [Serializable]
        private class CustomerPanelInfo
        {
            public RectTransform Panel;
            public RectTransform Character;
            public bool IsRewardCollectable;
        }

        [SerializeField] GameObject m_Canvas;
        [SerializeField] GameObject m_PopupHolder;
        [SerializeField] DataMapUpdate m_DataMapUpdate;
        [SerializeField, ReadOnly] AchievementTabs m_CurrentTab;

        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;
        [SerializeField] GameObject m_CustomerAchievementPopupButton;
        [SerializeField] AnimatorStatePlayer m_CustomerAchievementButtonAnimatorPlayer;
        [SerializeField] string m_CustomerAchievementButtonActiveState;
        [SerializeField] string m_CustomerAchievementButtonInactiveState;
        [SerializeField] GameObject m_AchievementPopupButton;
        [SerializeField] AnimatorStatePlayer m_AchievementButtonAnimatorPlayer;
        [SerializeField] string m_AchievementButtonActiveState;
        [SerializeField] string m_AchievementButtonInactiveState;
        [SerializeField] Vector3 m_ButtonActiveScale, m_ButtonInactiveScale;

        [Header("---Notification---")]
        [SerializeField] NotificationParentUI m_NotificationParentCustomerAchievement;
        [SerializeField] NotificationParentUI m_NotificationParentAchievemet;

        private NotificationParentUI m_NotificationParentAchievementButton;

        [Header("---Customer---")]
        [SerializeField] Transform m_CustomerTab;
        [SerializeField] PlayDoTween m_CustomerTabEnableTween;
        [SerializeField] Transform m_CustomerPanelHolder;
        [SerializeField] Transform m_CustomerCharacterHolder;
        [SerializeField] float m_NextShowDuration;
        [SerializeField] RectTransform m_PanelHidePosition;
        [SerializeField] RectTransform m_PanelShowPosition;
        [SerializeField] RectTransform m_CharacterHideRightPosition;
        [SerializeField] RectTransform m_CharacterShowPosition;
        [SerializeField] RectTransform m_CharacterHideLeftPosition;
        
        [SerializeField] GameObject m_CustomerAchievementActiveLeftArrow;
        [SerializeField] GameObject m_CustomerAchievementInactiveLeftArrow;
        [SerializeField] GameObject m_CustomerAchievementActiveRightArrow;
        [SerializeField] GameObject m_CustomerAchievementInactiveRightArrow;

        private int m_CustomerAchievementToPanelIndex;
        [SerializeField, ReadOnly]
        private List<CustomerPanelInfo> m_CustomerAchievementPanelsInfo;
        private Queue<Action> m_CustomerNextPanelQueue;
        private Queue<Action> m_CustomerPreviousPanelQueue;

        private Sequence m_CustomerNextPanelSequence = null;
        private Sequence m_CustomerPreveviousPanelSequence = null;

        private bool m_IsCustomerNextPanelTurning = false;
        private bool m_IsCustomerPreviousPanelTurning = false;


        [Header("---Achievement---")]
        [SerializeField] Transform m_AchievementTab;
        [SerializeField] PlayDoTween m_AchievementTabEnableTween;
        [SerializeField] Transform m_AchievementPanelsHolder;
        [SerializeField, ReadOnly] List<UIGeneralPanel> m_AchievementPanels;

        [Header("---Collectable---")]
        [SerializeField] float m_CollectionDuration = GlobalConstents.CollectionDuration;
        [SerializeField] float m_TargetBounceScale;
        [SerializeField] float m_TargetBounceDuration;
        [SerializeField] Ease m_TargetBounceEase;
        [SerializeField] CurveType m_CurveType;
        [SerializeField] Transform m_CollectableHolder;
        [SerializeField] GameObject m_CoinCollectabePrefab;
        [SerializeField] Transform m_CoinCollectabeEndPosition;
        [SerializeField] TextMeshProUGUI m_CoinCollectabeCountText;
        [SerializeField] Vector3 m_CoinCollectableStartSize;
        [SerializeField] GameObject m_GemCollectabePrefab;
        [SerializeField] Transform m_GemCollectabeEndPosition;
        [SerializeField] TextMeshProUGUI m_GemCollectabeCountText;
        [SerializeField] Vector3 m_GemCollectableStartSize;
        [SerializeField] Vector3 m_CollectableEndSize;
        private List<Collection> m_Collections = new List<Collection>();

        public override void Setup()
        {
            m_CustomerNextPanelQueue = new Queue<Action>();
            m_CustomerPreviousPanelQueue = new Queue<Action>();

            if (UIManager.GetPopup<MenuUIManager>() != null)
            {
                m_NotificationParentAchievementButton = UIManager.GetPopup<MenuUIManager>().GetAchievementButtonNotificationParent();
            }
            else
            {
                Debug.LogWarning(nameof(MenuUIManager) + " is null");
            }

            m_NotificationParentAchievemet.AddNotificationParent(m_NotificationParentAchievementButton);
            m_NotificationParentCustomerAchievement.AddNotificationParent(m_NotificationParentAchievementButton);

            // FullResetCustomerPanels();   mrcHefF
            FullResetAchievementPanels();
            // OpenCustomerAchievementPopup();  mrcHefF
            m_CustomerAchievementButtonAnimatorPlayer.transform.localScale = m_ButtonInactiveScale;
            m_AchievementButtonAnimatorPlayer.transform.localScale = m_ButtonActiveScale;
            OpenAchievementPopup(true);

        }


        public override void OpenPopup(Action onComplete)
        {
            m_CoinCollectabeCountText.text = DataManager.CoinCurrency.ToString();
            m_GemCollectabeCountText.text = DataManager.GemCurrency.ToString();

            SoundManager.PlaySound(SoundType.PopupWhoosh);

            m_Canvas.SetActive(true);
            // m_PopupHolder.SetActive(false);
            
            m_OpeningSequence.PlaySequence(() =>
            {
                SoundManager.PlaySound(SoundType.PopupWhoosh);

                // m_PopupHolder.SetActive(true);

                // FullResetCustomerPanels();   mrcHefF
                // FullResetAchievementPanels();
                // OpenCustomerAchievementPopup();  mrcHefF
                m_CustomerAchievementButtonAnimatorPlayer.transform.localScale = m_ButtonInactiveScale;
                m_AchievementButtonAnimatorPlayer.transform.localScale = m_ButtonActiveScale;
                OpenAchievementPopup(true);
                onComplete?.Invoke();
            
                // m_ClosingSequence.PlaySequence(onComplete).SetDelay(0.6f);
            });
        }

        public override void ClosePopup(Action onComplete)
        {
            ResetCustomerPanels();

            foreach (var collection in m_Collections)
            {
                if (collection != null)
                {
                    collection.Stop();
                }
            }

            m_Collections.Clear();
            SoundManager.PlaySound(SoundType.PopupWhoosh);

            // m_PopupHolder.SetActive(false);
            m_ClosingSequence.PlaySequence(() =>
            {
                m_Canvas.SetActive(false);
                onComplete?.Invoke();
            });
        }

        #region Button Event
        public void OnCustomerAchievementButton()
        {
            if(m_CurrentTab == AchievementTabs.Patient)
                return;

            ResetCustomerPanels();
            OpenCustomerAchievementPopup(false);
        }

        public void OnAchievementButton()
        {
            if(m_CurrentTab == AchievementTabs.Achievement)
                return;

            ResetCustomerPanels();
            OpenAchievementPopup(false);
        }

        public void OnCloseButton()
        {
            UIManager.ClosePopup<AchievementUIManager>();
        }
        #endregion


        private void OpenCustomerAchievementPopup(bool setupCall)
        {
            m_CurrentTab = AchievementTabs.Patient;
            m_CustomerTab.gameObject.SetActive(true);
            m_AchievementTab.gameObject.SetActive(false);
            if(!setupCall)
            {
                m_CustomerAchievementButtonAnimatorPlayer.PlayTrigger(m_CustomerAchievementButtonActiveState);
                m_AchievementButtonAnimatorPlayer.PlayTrigger(m_AchievementButtonInactiveState);
                m_CustomerTabEnableTween.Play();
            }
        }

        private void OpenAchievementPopup(bool setupCall)
        {
            m_CurrentTab = AchievementTabs.Achievement;
            m_CustomerTab.gameObject.SetActive(false);
            m_AchievementTab.gameObject.SetActive(true);
            PlayAchievementUIAnimations();
            if(!setupCall)
            {
                m_AchievementButtonAnimatorPlayer.PlayTrigger(m_AchievementButtonActiveState);
                m_CustomerAchievementButtonAnimatorPlayer.PlayTrigger(m_CustomerAchievementButtonInactiveState);
                m_AchievementTabEnableTween.Play();
            }
        }
        private void PlayAchievementUIAnimations()
        {
            foreach(UIGeneralPanel achievementPanel in m_AchievementPanels)
            {
                Button claimButton = achievementPanel.GetPanelHolding<Button, Achievement>(Achievement.ClaimButton);
                if (claimButton.gameObject.activeSelf)
                {
                    AnimatorStatePlayer barAnimatorPlayer = achievementPanel.GetPanelHolding<AnimatorStatePlayer, Achievement>(Achievement.BarAnimatorPlayer);
                    barAnimatorPlayer.PlayTrigger("ScaleUpDown");

                    AnimatorStatePlayer ClaimButtonAnimatorPlayer = achievementPanel.GetPanelHolding<AnimatorStatePlayer, Achievement>(Achievement.ClaimButtonAnimatorPlayer);
                    ClaimButtonAnimatorPlayer.PlayTrigger("ScaleText");
                }
            }
        }


        #region Customer Achievement
        public void FullResetCustomerPanels()
        {
            m_CustomerNextPanelQueue.Clear();
            if (m_CustomerNextPanelSequence != null)
            {
                m_CustomerNextPanelSequence.Kill();
                m_CustomerNextPanelSequence = null;
                m_IsCustomerNextPanelTurning = false;
            }

            m_CustomerPreviousPanelQueue.Clear();
            if (m_CustomerPreveviousPanelSequence != null)
            {
                m_CustomerPreveviousPanelSequence.Kill();
                m_CustomerPreveviousPanelSequence = null;
                m_IsCustomerPreviousPanelTurning = false;
            }
           

            if (m_CustomerAchievementPanelsInfo != null)
            {
                foreach (var customerAchievementPanel in m_CustomerAchievementPanelsInfo)
                {
                    Destroy(customerAchievementPanel.Panel.gameObject);
                    Destroy(customerAchievementPanel.Character.gameObject);
                }
            }

            m_CustomerAchievementPanelsInfo = new List<CustomerPanelInfo>();

            DataMapAchievementUpdate mapAchievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            List<Tuple<RectTransform, RectTransform, bool>> customerPanelsInfo = mapAchievementUpdate.GetCustomerAchievementPanels(this, m_CustomerPanelHolder, m_CustomerCharacterHolder, OnCustomerRewardClaim);

            int targetIndex = 0;
            bool hasTargetIndex = false;
            int i = 0;
            foreach (var customerPage in customerPanelsInfo)
            {
                CustomerPanelInfo customerPanelInfo = new CustomerPanelInfo();
                customerPanelInfo.Panel = customerPage.Item1;
                customerPanelInfo.Panel.anchoredPosition = m_PanelHidePosition.anchoredPosition;
                customerPanelInfo.Character = customerPage.Item2;
                customerPanelInfo.Character.anchoredPosition = m_CharacterHideRightPosition.anchoredPosition;
                customerPanelInfo.IsRewardCollectable = customerPage.Item3;

                
                m_CustomerAchievementPanelsInfo.Add(customerPanelInfo);

                if (customerPanelInfo.IsRewardCollectable 
                    && hasTargetIndex == false)
                {
                    targetIndex = i;
                    customerPanelInfo.Panel.anchoredPosition = m_PanelShowPosition.anchoredPosition;
                    customerPanelInfo.Character.anchoredPosition = m_CharacterShowPosition.anchoredPosition;
                    hasTargetIndex = true;
                }
                i++;
            }

            if (m_CustomerAchievementPanelsInfo.Count > 0)
            {
                if(hasTargetIndex == false)
                {
                    m_CustomerAchievementPanelsInfo[targetIndex].Panel.anchoredPosition = m_PanelShowPosition.anchoredPosition;
                    m_CustomerAchievementPanelsInfo[targetIndex].Character.anchoredPosition = m_CharacterShowPosition.anchoredPosition;
                    hasTargetIndex = true;
                }
                m_CustomerAchievementToPanelIndex = targetIndex;
                
                SetCustomerAchievementArrows();
            }
        }

        public void ResetCustomerPanels()
        {
            m_CustomerNextPanelQueue.Clear();
            if (m_CustomerNextPanelSequence != null)
            {
                m_CustomerNextPanelSequence.Kill();
                m_CustomerNextPanelSequence = null;
                m_IsCustomerNextPanelTurning = false;
            }

            m_CustomerPreviousPanelQueue.Clear();
            if (m_CustomerPreveviousPanelSequence != null)
            {
                m_CustomerPreveviousPanelSequence.Kill();
                m_CustomerPreveviousPanelSequence = null;
                m_IsCustomerPreviousPanelTurning = false;
            }

            if (m_CustomerAchievementPanelsInfo != null)
            {
                int targetIndex = 0;
                bool hasTargetIndex = false;
                for (int i = 0; i < m_CustomerAchievementPanelsInfo.Count; i++)
                {
                    m_CustomerAchievementPanelsInfo[i].Panel.anchoredPosition = m_PanelHidePosition.anchoredPosition;
                    m_CustomerAchievementPanelsInfo[i].Character.anchoredPosition = m_CharacterHideRightPosition.anchoredPosition;

                    if (m_CustomerAchievementPanelsInfo[i].IsRewardCollectable
                        && hasTargetIndex == false)
                    {
                        m_CustomerAchievementPanelsInfo[i].Panel.anchoredPosition = m_PanelShowPosition.anchoredPosition;
                        m_CustomerAchievementPanelsInfo[i].Character.anchoredPosition = m_CharacterShowPosition.anchoredPosition;
                        targetIndex = i;
                        hasTargetIndex = true;
                    }
                }


                if (m_CustomerAchievementPanelsInfo.Count > 0)
                {
                    if(hasTargetIndex == false)
                    {
                        m_CustomerAchievementPanelsInfo[targetIndex].Panel.anchoredPosition = m_PanelShowPosition.anchoredPosition;
                        m_CustomerAchievementPanelsInfo[targetIndex].Character.anchoredPosition = m_CharacterShowPosition.anchoredPosition;
                        hasTargetIndex = true;
                    }
                    m_CustomerAchievementToPanelIndex = targetIndex;
                    SetCustomerAchievementArrows();
                }
                else
                {
                    Debug.LogWarning(nameof(m_CustomerAchievementPanelsInfo) + "has zero Null");
                }
            }
            else
            {
                Debug.LogWarning(nameof(m_CustomerAchievementPanelsInfo) + " is Null");
            }
        }

        public void CustomerAchievementNextPanel()
        {
            // Cancel Previous
            m_CustomerPreviousPanelQueue.Clear();
            if (m_CustomerPreveviousPanelSequence != null)
            {
                m_CustomerPreveviousPanelSequence.Kill();
                m_CustomerPreveviousPanelSequence = null;
                m_IsCustomerPreviousPanelTurning = false;
            }

            void TurnNext()
            {
                int index = m_CustomerAchievementToPanelIndex;
                if (m_CustomerAchievementPanelsInfo == null || index >= m_CustomerAchievementPanelsInfo.Count - 1)
                    return;

                m_IsCustomerNextPanelTurning = true;
                m_CustomerAchievementToPanelIndex++;

                SetCustomerAchievementArrows();

                var currentPanel = m_CustomerAchievementPanelsInfo[index];
                var nextPanel = m_CustomerAchievementPanelsInfo[index + 1];

                float duration = m_NextShowDuration / 2f;
                m_CustomerNextPanelSequence = DOTween.Sequence();

                m_CustomerNextPanelSequence.Append(
                    currentPanel.Panel.DOAnchorPos(m_PanelHidePosition.anchoredPosition, duration).SetEase(Ease.InSine)
                );

                m_CustomerNextPanelSequence.Join(
                    currentPanel.Character.DOAnchorPos(m_CharacterHideLeftPosition.anchoredPosition, duration).SetEase(Ease.Linear)
                );

                m_CustomerNextPanelSequence.Join(
                    nextPanel.Character.DOAnchorPos(m_CharacterShowPosition.anchoredPosition, duration).SetEase(Ease.Linear)
                );


                m_CustomerNextPanelSequence.Append(
                    nextPanel.Panel.DOAnchorPos(m_PanelShowPosition.anchoredPosition, duration).SetEase(Ease.OutSine)
                );

                m_CustomerNextPanelSequence.OnComplete(() =>
                {
                    m_CustomerNextPanelSequence = null;
                    m_IsCustomerNextPanelTurning = false;

                    if (m_CustomerNextPanelQueue.Count > 0)
                        m_CustomerNextPanelQueue.Dequeue().Invoke();
                });
            }

            if (m_IsCustomerNextPanelTurning && m_CustomerNextPanelQueue.Count < 4)
                m_CustomerNextPanelQueue.Enqueue(TurnNext);
            else if(!m_IsCustomerNextPanelTurning)
                TurnNext();
        }

        public void CustomerAchievementPreviousPanel()
        {
            // Cancel Next
            m_CustomerNextPanelQueue.Clear();
            if (m_CustomerNextPanelSequence != null)
            {
                m_CustomerNextPanelSequence.Kill();
                m_CustomerNextPanelSequence = null;
                m_IsCustomerNextPanelTurning = false;
            }

            void TurnPrev()
            {
                int index = m_CustomerAchievementToPanelIndex;
                if (m_CustomerAchievementPanelsInfo == null || index <= 0)
                    return;

                m_IsCustomerPreviousPanelTurning = true;
                m_CustomerAchievementToPanelIndex--;

                SetCustomerAchievementArrows();

                var currentPanel = m_CustomerAchievementPanelsInfo[index];
                var previousPanel = m_CustomerAchievementPanelsInfo[index - 1];
              

                float duration = m_NextShowDuration / 2f;
                m_CustomerPreveviousPanelSequence = DOTween.Sequence();

                m_CustomerPreveviousPanelSequence.Append(
                    currentPanel.Panel.DOAnchorPos(m_PanelHidePosition.anchoredPosition, duration).SetEase(Ease.InSine)
                );

                 m_CustomerNextPanelSequence.Join(
                    currentPanel.Character.DOAnchorPos(m_CharacterHideRightPosition.anchoredPosition, duration).SetEase(Ease.Linear)
                );

                m_CustomerNextPanelSequence.Join(
                    previousPanel.Character.DOAnchorPos(m_CharacterShowPosition.anchoredPosition, duration).SetEase(Ease.Linear)
                );


                m_CustomerPreveviousPanelSequence.Append(
                    previousPanel.Panel.DOAnchorPos(m_PanelShowPosition.anchoredPosition, duration).SetEase(Ease.OutSine)
                );


                m_CustomerPreveviousPanelSequence.OnComplete(() =>
                {
                    m_CustomerPreveviousPanelSequence = null;
                    m_IsCustomerPreviousPanelTurning = false;

                    if (m_CustomerPreviousPanelQueue.Count > 0)
                        m_CustomerPreviousPanelQueue.Dequeue().Invoke();
                });
            }

            if (m_IsCustomerPreviousPanelTurning && m_CustomerPreviousPanelQueue.Count < 4)
                m_CustomerPreviousPanelQueue.Enqueue(TurnPrev);
            else if(!m_IsCustomerPreviousPanelTurning)
                TurnPrev();
        }

        private void OnCustomerRewardClaim(int coinReward, Transform coinTransform, int gemReward, Transform gemTransform)
        {
            if (coinReward > 0)
            {
                int coinCurrency = DataManager.CoinCurrency;
                int coinCurrencyGained = DataManager.CoinCurrency + coinReward;

                Collection(coinTransform.position, CurrencyType.Coin, coinCurrency, coinCurrencyGained);

                DataManager.CoinCurrency += coinReward;
                GlobalEventHolder.OnNewCoinCurrencyAdded?.Invoke(coinReward);
            }

            if (gemReward > 0)
            {
                int gemCurrency = DataManager.GemCurrency;
                int gemCurrencyGained = DataManager.GemCurrency + gemReward;

                Collection(gemTransform.position, CurrencyType.Gem, gemCurrency, gemCurrencyGained);

                DataManager.GemCurrency += gemReward;
            }

            DataManager.SaveData();
        }

        private void SetCustomerAchievementArrows()
        {
            if (m_CustomerAchievementToPanelIndex >= m_CustomerAchievementPanelsInfo.Count - 1)
            {
                m_CustomerAchievementActiveRightArrow.SetActive(false);
                m_CustomerAchievementInactiveRightArrow.SetActive(true);
            }
            else
            {
                m_CustomerAchievementActiveRightArrow.SetActive(true);
                m_CustomerAchievementInactiveRightArrow.SetActive(false);
            }

            if (m_CustomerAchievementToPanelIndex <= 0)
            {
                m_CustomerAchievementActiveLeftArrow.SetActive(false);
                m_CustomerAchievementInactiveLeftArrow.SetActive(true);
            }
            else
            {
                m_CustomerAchievementActiveLeftArrow.SetActive(true);
                m_CustomerAchievementInactiveLeftArrow.SetActive(false);
            }
        }
        #endregion

        #region Achievement
        public void FullResetAchievementPanels()
        {
            if (m_AchievementPanels != null)
            {
                foreach (var achievementPanels in m_AchievementPanels)
                {
                    Destroy(achievementPanels.gameObject);
                }
            }

            DataMapAchievementUpdate mapAchievementUpdate = m_DataMapUpdate.GetMapAchievementUpdate();
            m_AchievementPanels = mapAchievementUpdate.GetAchievementPanels(this, m_AchievementPanelsHolder, OnAchievementRewardClaim);
        }

        private void OnAchievementRewardClaim(int coinReward, Transform coinTransform, int gemReward, Transform gemTransform)
        {
            if (coinReward > 0)
            {
                int coinCurrency = DataManager.CoinCurrency;
                int coinCurrencyGained = DataManager.CoinCurrency + coinReward;

                Collection(coinTransform.position, CurrencyType.Coin, coinCurrency, coinCurrencyGained);

                DataManager.CoinCurrency += coinReward;
                GlobalEventHolder.OnNewCoinCurrencyAdded?.Invoke(coinReward);
            }

            if (gemReward > 0)
            {
                int gemCurrency = DataManager.GemCurrency;
                int gemCurrencyGained = DataManager.GemCurrency + gemReward;

                Collection(gemTransform.position, CurrencyType.Gem, gemCurrency, gemCurrencyGained);

                DataManager.GemCurrency += gemReward;
            }

            DataManager.SaveData();
        }
        #endregion

        #region Helper
        public NotificationParentUI GetNotificationCustmoerAchievement()
        {
            return m_NotificationParentCustomerAchievement;
        }

        public NotificationParentUI GetNotificationAchievement()
        {
            return m_NotificationParentAchievemet;
        }
        #endregion

        #region Collectable Animation

        /*[Button]
        public void CollectionCoinCall()
        {
            Collection(Vector3.zero, CurrencyType.Coin, 100, 300);
        }

        [Button]
        public void CollectionGemCall()
        {
            Collection(Vector3.zero, CurrencyType.Gem, 100, 300);
        }*/

        public void Collection(Vector3 position, CurrencyType currencyType, int currencyCurrent, int currencyFinalAmmount)
        {
            GameObject collectablePrefab = currencyType == CurrencyType.Coin ? m_CoinCollectabePrefab : m_GemCollectabePrefab;
            Transform endPosition = currencyType == CurrencyType.Coin ? m_CoinCollectabeEndPosition : m_GemCollectabeEndPosition;
            Transform endBounce = currencyType == CurrencyType.Coin ? m_CoinCollectabeEndPosition : m_GemCollectabeEndPosition;
            TextMeshProUGUI targetText = currencyType == CurrencyType.Coin ? m_CoinCollectabeCountText : m_GemCollectabeCountText;
            Vector3 collectableStartSize = currencyType == CurrencyType.Coin ? m_CoinCollectableStartSize : m_GemCollectableStartSize;

            int totalCount = 4;
            int counter = 0;

            SoundManager.PlaySound(SoundType.SmallWin);
            Collection collection = CollectionUIManager.CollectCurve(totalCount, 
                m_CollectionDuration,
                m_CurveType, 
                collectablePrefab,
                collectableStartSize,
                m_CollectableEndSize,
                m_CollectableHolder,
                position,
                endPosition.position,
                false,
                () =>
                {
                    counter++;

                    int lerpValue = Mathf.RoundToInt(Mathf.Lerp(currencyCurrent, currencyFinalAmmount, ((float)counter / (float)totalCount)));
                    targetText.text = lerpValue.ToString();

                    endBounce.DoBounceScale(Vector3.one, Vector3.one * m_TargetBounceScale, m_TargetBounceDuration).SetEase(m_TargetBounceEase);

                    SoundManager.PlaySound(currencyType == CurrencyType.Coin ? SoundType.Coin : SoundType.Gem);
                },
                () =>
                {
                    targetText.text = currencyFinalAmmount.ToString();
                });

            m_Collections.Add(collection);
        }
        #endregion
    }
}
