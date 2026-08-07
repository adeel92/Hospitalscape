using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using Arc;
using Isometric.Data;
using Isometric.Sound;
using NaughtyAttributes;

namespace Isometric.UI
{
    public class StarItemUnlockingUIManager : UIPopupBase
    {
        [Header("---Popup---")]
        [SerializeField] DataMapUpdate m_DataMapUpdate;
        [SerializeField] GameObject m_Popup;
        [SerializeField] GameObject m_ChoicePopup;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;
        [SerializeField] PlayDoTweenSequence m_ChoiceOpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ChoiceClosingSequence;
        [SerializeField] GameObject m_AllItemsUnlockedMessage;

        [Header("---Star Use---")]
        [SerializeField] Transform m_StarUseHolder;
        [SerializeField] CurrencyUIController m_CurrencyUIController;
        [SerializeField] GameObject m_StarUsePrefab;
        [SerializeField] float m_StarUseDuration;
        [SerializeField] Vector3 m_StarUseStartScale;
        [SerializeField] Vector3 m_StarUseEndScale;
        [SerializeField] Transform m_StarUseStartPosition;
        [SerializeField] Transform m_StarUseEndPosition;
        [SerializeField] CurveType m_StarUseCurveType;

        [Header("---Star Unlocking---")]
        [SerializeField] GameObject m_StarUnlockingPopup;
        [SerializeField] TextMeshProUGUI m_NameText;
        [SerializeField] TextMeshProUGUI m_DiscriptionText;
        [SerializeField] TextMeshProUGUI m_LockedDiscriptionText;
        [SerializeField] Image m_PrevewImage;
        [SerializeField] Button m_UnlockOnButton;
        [SerializeField] TextMeshProUGUI m_UnlockOnButtonText;
        [SerializeField] GameObject m_LockButton;
        [SerializeField] TextMeshProUGUI m_LockOnButtonText;
        [SerializeField] GameObject m_CloseButton;

        [Header("---Star Item Choices---")]
        [SerializeField] StartItemChoiceButtonUI m_ChoiceButtonPrefab;
        [SerializeField] Transform m_ChoiceButtonsHolder;
        [ReadOnly, SerializeField] List<StartItemChoiceButtonUI> m_AvailableChoiceButtonUIs = new();
        [ReadOnly, SerializeField] StartItemChoiceButtonUI m_CurrentSelectedChoiceButton;
        [SerializeField] Button m_ChoiceApplyButton;

        private StarItemInfo m_CurrentStarItemInfo = null;

        public override void Setup() {}

        // Returns true of it has next star item unlockable
        public bool CheckNextStarItemUnlockable()
        {
            StarItemInfo starItemInfo = m_DataMapUpdate.GetNextStarUnlockable();

            if (starItemInfo != null)
            {
                m_CurrentStarItemInfo = starItemInfo;
                m_AllItemsUnlockedMessage.SetActive(false);
                m_StarUnlockingPopup.SetActive(true);

                if (starItemInfo.IsUnloackble == true)
                {
                    m_PrevewImage.sprite = starItemInfo.PreviewSprite;
                    m_NameText.text = starItemInfo.NameText;
                    m_DiscriptionText.text = starItemInfo.DiscriptionText;
                    m_UnlockOnButtonText.text = starItemInfo.StarRequired.ToString();

                    m_DiscriptionText.gameObject.SetActive(true);
                    m_LockedDiscriptionText.gameObject.SetActive(false);

                    m_CloseButton.SetActive(false);
                    m_LockButton.SetActive(false);
                    m_UnlockOnButton.gameObject.SetActive(true);

                    SetupChoiceButtons(starItemInfo.EnvironmentDecorationData);
                    m_ChoiceApplyButton.onClick.RemoveAllListeners();
                    m_ChoiceApplyButton.onClick.AddListener(
                    () =>
                    {
                        GlobalEventHolder.OnStarItemChoiceFinalSelection?.Invoke(m_CurrentSelectedChoiceButton.ChoiceIndex);    
                    });

                    Action callback = starItemInfo.OnUnlocked;

                    m_UnlockOnButton.onClick.RemoveAllListeners();
                    m_UnlockOnButton.onClick.AddListener(() =>
                    {
                        callback?.Invoke();
                        if (starItemInfo.CoinReward > 0)
                        {
                            DataManager.CoinCurrency += starItemInfo.CoinReward;
                        }
                        if (starItemInfo.GemReward > 0)
                        {
                            DataManager.GemCurrency += starItemInfo.GemReward;
                        }
                        // DataManager.SaveData();

                        UIManager.UIInteractionOff();
                        CollectionUIManager.CollectCurve(starItemInfo.StarRequired, 
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
                                UIManager.HasStarItemUnlocked(starItemInfo);
                            });
                    });


                    return true;
                }
                else
                {
                    m_PrevewImage.sprite = starItemInfo.PreviewSprite;
                    m_NameText.text = starItemInfo.NameText;
                    m_LockOnButtonText.text = starItemInfo.StarRequired.ToString();

                    m_DiscriptionText.gameObject.SetActive(false);
                    m_LockedDiscriptionText.gameObject.SetActive(true);

                    m_CloseButton.SetActive(true);
                    m_LockButton.SetActive(true);
                    m_UnlockOnButton.gameObject.SetActive(false);

                    return false;
                }
            }
            else
            {
                m_CloseButton.SetActive(true);
                m_AllItemsUnlockedMessage.SetActive(true);
                m_StarUnlockingPopup.SetActive(false);
                return false;
            }
        }

        public void SetupChoiceButtons(EnvironmentDecorationData environmentDecorationData)
        {
            for (int i = m_ChoiceButtonsHolder.childCount - 1; i >= 0; i--)
            {
                Transform child = m_ChoiceButtonsHolder.GetChild(i);
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            m_AvailableChoiceButtonUIs.Clear();
            m_CurrentSelectedChoiceButton = null;

            List<DecorationDesignInfo> designInfos = environmentDecorationData.DecorationDesignInfos;

            for (int i = 0; i < designInfos.Count; i++)
            {
                int choiceIndex = i;
                StartItemChoiceButtonUI choiceButtonUI = Instantiate(m_ChoiceButtonPrefab, m_ChoiceButtonsHolder);
                choiceButtonUI.Setup(choiceIndex, designInfos[choiceIndex].UISprite,
                () =>
                {
                    m_CurrentSelectedChoiceButton.Deselect();
                    choiceButtonUI.Select();
                    m_CurrentSelectedChoiceButton = choiceButtonUI;
                    GlobalEventHolder.OnStarItemChoiceButtonClick?.Invoke(choiceIndex);
                });
                choiceButtonUI.Deselect();
                m_AvailableChoiceButtonUIs.Add(choiceButtonUI);
            }

            if (m_AvailableChoiceButtonUIs.Count == 0)
                return;

            m_CurrentSelectedChoiceButton = m_AvailableChoiceButtonUIs[0];
            m_CurrentSelectedChoiceButton.Select();
        }
        private void DeselectAllChoiceButtons()
        {
            if(m_AvailableChoiceButtonUIs.Count == 0)
                return;

            foreach(StartItemChoiceButtonUI choiceButtonUI in m_AvailableChoiceButtonUIs)
            {
                choiceButtonUI.Deselect();
            }
        }

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

        public void OpenChoicePopup(Action callback)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_ChoicePopup.SetActive(true);
            m_ChoiceOpeningSequence.PlaySequence(() =>
            {
                callback?.Invoke();
            });
        } 

        public void CloseChoicePopup(Action callback)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_ChoiceClosingSequence.PlaySequence(() =>
            {
                m_ChoicePopup.SetActive(false);
                callback?.Invoke();
            });
        }

        public void OnCloseButton()
        {
            UIManager.ClosePopup<StarItemUnlockingUIManager>();
        }
    }
}
