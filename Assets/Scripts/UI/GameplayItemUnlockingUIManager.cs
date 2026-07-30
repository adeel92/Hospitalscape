using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using Arc;
using Isometric.Data;
using Isometric.Sound;
using DG.Tweening;

namespace Isometric.UI
{
    public class GameplayItemUnlockingUIManager : UIPopupBase
    {
        [SerializeField] DataMapUpdate m_DataMapUpdate;

        [Header("---Item Unlocking---")]
        [SerializeField] GameObject m_Popup;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;
        // [SerializeField] GameObject m_AllItemsUnlockedMessage;
        [SerializeField] GameObject m_ItemUnlockingPopup;
        [SerializeField] TextMeshProUGUI m_NameText;
        [SerializeField] TextMeshProUGUI m_DiscriptionText;
        [SerializeField] TextMeshProUGUI m_LockedDiscriptionText;
        [SerializeField] Image m_PrevewImage;
        [SerializeField] Button m_UnlockOnButton;
        [SerializeField] TextMeshProUGUI m_UnlockOnButtonText;
        [SerializeField] GameObject m_LockButton;
        [SerializeField] TextMeshProUGUI m_LockOnButtonText;
        [SerializeField] GameObject m_CloseButton;

        [Header("---Item Focusing---")]
        [SerializeField] GameObject m_PopupFocus;
        [SerializeField] PlayDoTweenSequence m_OpeningSequenceFocus;
        [SerializeField] PlayDoTweenSequence m_ClosingSequenceFocus;
        [SerializeField] List<FocusDescriptionPanelInfo> FocusDescriptionPanels;
        [SerializeField] float m_DescriptionPanelFocusTweenDuration = 0.15f;
        [SerializeField] Ease m_DescriptionPanelFocusEaseType = Ease.InBack;
        private StationName m_CurrentTargetStation = StationName.None;

        public override void Setup() {}

        // Returns true of it has next gameplay item unlockable
        public bool CheckNextGameplayItemUnlockable()
        {
            GameplayUnlockableItemInfo gameplayUnlockableItemInfo = m_DataMapUpdate.GetNextGameplayUnlockable();

            if (gameplayUnlockableItemInfo != null)
            {
                // m_AllItemsUnlockedMessage.SetActive(false);
                m_ItemUnlockingPopup.SetActive(true);

                if (gameplayUnlockableItemInfo.IsUnloackble == true)
                {
                    m_PrevewImage.sprite = gameplayUnlockableItemInfo.PreviewSprite;
                    m_NameText.text = gameplayUnlockableItemInfo.NameText;
                    m_DiscriptionText.text = gameplayUnlockableItemInfo.DiscriptionText;
                    // m_UnlockOnButtonText.text = gameplayUnlockableItemInfo.StarRequired.ToString();

                    m_DiscriptionText.gameObject.SetActive(true);
                    m_LockedDiscriptionText.gameObject.SetActive(false);

                    m_CloseButton.SetActive(false);
                    m_LockButton.SetActive(false);
                    m_UnlockOnButton.gameObject.SetActive(true);


                    Action callback = gameplayUnlockableItemInfo.OnUnlocked;

                    m_UnlockOnButton.onClick.RemoveAllListeners();
                    m_UnlockOnButton.onClick.AddListener(() =>
                    {
                        callback?.Invoke();
                        if (gameplayUnlockableItemInfo.CoinReward > 0)
                        {
                            DataManager.CoinCurrency += gameplayUnlockableItemInfo.CoinReward;
                        }
                        if (gameplayUnlockableItemInfo.GemReward > 0)
                        {
                            DataManager.GemCurrency += gameplayUnlockableItemInfo.GemReward;
                        }
                        DataManager.SaveData();

                        UIManager.UIInteractionOff();
                        UIManager.HasGameplayItemUnlocked(gameplayUnlockableItemInfo);
                        /* CollectionUIManager.CollectCurve(gameplayUnlockableItemInfo.StarRequired, 
                            m_StarUseDuration,
                            m_StarUseCurveType,
                            m_StarUsePrefab, 
                            m_StarUseStartScale,
                            m_StarUseEndScale,
                            m_StarUseHolder, 
                            m_StarUseStartPosition.position, 
                            m_StarUseEndPosition.position, 
                            false,
                            () =>
                            {
                                SoundManager.PlaySound(SoundType.Coin);
                                m_StarUseEndPosition.DoBounceScale(Vector3.one, Vector3.one * 1.1f, 0.1f);
                            }, 
                            () =>
                            {
                            }); */
                    });


                    return true;
                }
                else
                {
                    /* m_PrevewImage.sprite = gameplayUnlockableItemInfo.PreviewSprite;
                    m_NameText.text = gameplayUnlockableItemInfo.NameText;
                    m_LockOnButtonText.text = gameplayUnlockableItemInfo.StarRequired.ToString();

                    m_DiscriptionText.gameObject.SetActive(false);
                    m_LockedDiscriptionText.gameObject.SetActive(true);

                    m_CloseButton.SetActive(true);
                    m_LockButton.SetActive(true);
                    m_UnlockOnButton.gameObject.SetActive(false); */

                    return false;
                }
            }
            else
            {
                /* m_CloseButton.SetActive(true);
                m_AllItemsUnlockedMessage.SetActive(true); */
                m_ItemUnlockingPopup.SetActive(false);
                return false;
            }
        }

        #region Unlockable Intro
        public override void OpenPopup(Action callback)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_Popup.SetActive(true);
            m_OpeningSequence.PlaySequence(() =>
            {
                callback?.Invoke();
            });
        }

        public override void ClosePopup(Action callback)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_UnlockOnButton.onClick.RemoveAllListeners();
            m_ClosingSequence.PlaySequence(() =>
            {
                m_Popup.SetActive(false);
                callback?.Invoke();
            });
        }

        public void OnCloseButton()
        {
            UIManager.ClosePopup<StarItemUnlockingUIManager>();
        }
        #endregion

        #region Unlockable Focus
        public void ShowFocusUI(StationName targetStation)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_PopupFocus.SetActive(true);
            m_OpeningSequenceFocus.PlaySequence();
        }
        public void ShowDescriptionPanel()  // Inside opening sequence
        {
            
        }
        #endregion
    }


    [Serializable]
    public class FocusDescriptionPanelInfo
    {
        public StationName StationName;
        public GameObject DescriptionPanel;
    }
}
