using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using NaughtyAttributes;
using Arc;
using Isometric.Data;
using Isometric.Sound;

namespace Isometric.UI
{
    public class UpgradePopupUIManager : UIPopupBase
    {
        private static UpgradePopupUIManager s_Instance;

        private enum SideButtonType
        {
            Staff, Stations, Reveneu, Patience
        }

        [Serializable]
        private class SideButtonInfo
        {
            public SideButtonType Type;

            public GameObject Holder;
            public GameObject Selected;
            public Button UnselectedButton;
            [Header("-On category select button offset to the highlighted object")]
            public float HighlightedChildSelectionOffset;
            [AllowNesting, ReadOnly]
            public RectTransform HighlightedChild;


            public bool IsVisibleInViewport(ScrollRect scrollRect, float offsetX)
            {
                if (HighlightedChild == null) return false;

                RectTransform content = scrollRect.content;

                float contentX = content.anchoredPosition.x;
                float childX = HighlightedChild.anchoredPosition.x;

                bool isVisible = childX <= (contentX * -1 ) + offsetX;

                return isVisible;
            }

        }

        [SerializeField] Canvas m_Canvas;

        [Header("---Upgrade Popup---")]
        [SerializeField] GameObject m_Popup;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;
        [SerializeField] DataMapUpdate m_DataMapUpdate;
        [SerializeField] ScrollRect m_ScrollRect;
        [SerializeField] Transform m_PanelHolder;
        [SerializeField] GameObject m_FirstHolding;
        [SerializeField] float m_ScrollDuration;
        [SerializeField] Ease m_ScrollEase;
        [SerializeField] float m_ContentAutoSelectOffset = 50f; // Offset to adjust selection trigger
        [SerializeField] TextMeshProUGUI m_LevelText;
        [SerializeField] TextMeshProUGUI m_LevelDifficultyText;
        [SerializeField] GameObject m_PlayButton;
        [SerializeField] GameObject m_PlayLockedButton;
        [SerializeField] List<SideButtonInfo> m_SideButtonsInfo;

        [SerializeField] NotificationParentUI m_NotificationStaff;
        [SerializeField] NotificationParentUI m_NotificationStations;
        [SerializeField] NotificationParentUI m_NotificationRevenue;
        [SerializeField] NotificationParentUI m_NotificationPatience;


        [Header("---Info Popup---")]
        [SerializeField] GameObject m_InfoPopup;
        [SerializeField] PlayDoTweenSequence m_InfoPopupOpeningSequence;
        [SerializeField] PlayDoTweenSequence m_InfoPopupClosingSequence;
        [SerializeField] TextMeshProUGUI m_InfoHeaderText;
        [SerializeField] TextMeshProUGUI m_InfoDiscriptionText;
        [SerializeField] Image m_InfoPreview;

        [Header("---Map Complete Popup---")]
        [SerializeField] GameObject m_MapCompletePopup;
        [SerializeField] PlayDoTweenSequence m_MapCompleteOpeningSequence;
        [SerializeField] PlayDoTweenSequence m_MapCompleteClosingSequence;


        private List<PlayerUpgradePanelUI> m_PlayerUpgradePanelsUI;
        private List<ChairCapacityUpgradePanelUI> m_ChairCapacityUpgradePanelsUI;
        private List<WorkerUpgradePanelUI> m_WorkerUpgradePanelsUI;
        private List<StationUpgradePanelUI> m_StationUpgradePanelsUI;
        private List<StationUpgradePanelUI> m_StationUpgradeRevenuePanelsUI;
        private List<PatienceUpgradePanelUI> m_PatienceUpgradePanelsUI;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                m_ChairCapacityUpgradePanelsUI = new List<ChairCapacityUpgradePanelUI>();
            }
        }

        public override void Setup()
        {
            foreach (Transform child in m_PanelHolder.transform)
            {
                if(m_FirstHolding != child.gameObject)
                {
                    Destroy(child.gameObject);
                }
            }

            if (DataManager.GetCurrentDataLevel() == null)
            {
                m_LevelText.text = "Level # " + (DataManager.CurrentMapLevelIndex);
            }
            else
            {
                m_LevelText.text = "Level # " + (DataManager.CurrentMapLevelIndex + 1);
            }
            m_LevelDifficultyText.text = DataManager.CurrentMapLevelDifficulty.ToString();

            m_PlayerUpgradePanelsUI = m_DataMapUpdate.GetAndSetPlayerUpgradePanels(m_PanelHolder);
            foreach (var panel in m_PlayerUpgradePanelsUI)
            {
                panel.UpgradePopupUIManager = this;
                panel.SetupNotification(m_NotificationStaff);
            }

            ChairCapacityUpgradePanelUI chairSalonCapacityUpgradePanelUI = m_DataMapUpdate.GetAndSetChairCapacityUpgradePanel(m_PanelHolder, ChairUpgradeType.Salon);
            if(chairSalonCapacityUpgradePanelUI != null)
            {
                m_ChairCapacityUpgradePanelsUI.Add(chairSalonCapacityUpgradePanelUI);
            }

            ChairCapacityUpgradePanelUI chairCafeCapacityUpgradePanelUI = m_DataMapUpdate.GetAndSetChairCapacityUpgradePanel(m_PanelHolder, ChairUpgradeType.Cafe);
            if(chairCafeCapacityUpgradePanelUI != null)
            {
                m_ChairCapacityUpgradePanelsUI.Add(chairCafeCapacityUpgradePanelUI);
            }

            foreach (var panel in m_ChairCapacityUpgradePanelsUI)
            {
                panel.UpgradePopupUIManager = this;
                panel.SetupNotification(m_NotificationStaff);
            }

            m_WorkerUpgradePanelsUI = m_DataMapUpdate.GetAndSetWorkerUpgradePanels(m_PanelHolder);
            foreach (var panel in m_WorkerUpgradePanelsUI)
            {
                panel.UpgradePopupUIManager = this;
                panel.SetupNotification(m_NotificationStaff);
            }

            m_StationUpgradePanelsUI = m_DataMapUpdate.GetAndSetStationUpgradePanels(m_PanelHolder, StationUpgradeType.StationUpgradeType1);

            foreach (var panel in m_StationUpgradePanelsUI)
            {
                panel.UpgradePopupUIManager = this;
                panel.SetupNotification(m_NotificationStations);
            }

            m_StationUpgradeRevenuePanelsUI = m_DataMapUpdate.GetAndSetStationUpgradePanels(m_PanelHolder, StationUpgradeType.StationUpgradeType2);

            foreach (var panel in m_StationUpgradeRevenuePanelsUI)
            {
                panel.UpgradePopupUIManager = this;
                panel.SetupNotification(m_NotificationRevenue);
            }

            m_PatienceUpgradePanelsUI = m_DataMapUpdate.GetAndSetPatienceUpgradePanels(m_PanelHolder);

            foreach (var panel in m_PatienceUpgradePanelsUI)
            {
                panel.UpgradePopupUIManager = this;
                panel.SetupNotification(m_NotificationPatience);
            }

            foreach (var sideButtonInfo in m_SideButtonsInfo)
            {
                if (sideButtonInfo.Type == SideButtonType.Staff
                    && m_PlayerUpgradePanelsUI.Count > 0)
                {
                    sideButtonInfo.HighlightedChild = m_PlayerUpgradePanelsUI[0].GetComponent<RectTransform>();
                    sideButtonInfo.UnselectedButton.onClick.RemoveAllListeners();
                    sideButtonInfo.UnselectedButton.onClick.AddListener(() => { ScrollTo(sideButtonInfo.HighlightedChild, sideButtonInfo.HighlightedChildSelectionOffset); });
                }
                else if (sideButtonInfo.Type == SideButtonType.Stations
                   && m_StationUpgradePanelsUI.Count > 0)
                {
                    sideButtonInfo.HighlightedChild = m_StationUpgradePanelsUI[0].GetComponent<RectTransform>();
                    sideButtonInfo.UnselectedButton.onClick.RemoveAllListeners();
                    sideButtonInfo.UnselectedButton.onClick.AddListener(() => { ScrollTo(sideButtonInfo.HighlightedChild, sideButtonInfo.HighlightedChildSelectionOffset); });
                }
                else if (sideButtonInfo.Type == SideButtonType.Reveneu
                   && m_StationUpgradeRevenuePanelsUI.Count > 0)
                {
                    sideButtonInfo.HighlightedChild = m_StationUpgradeRevenuePanelsUI[0].GetComponent<RectTransform>();
                    sideButtonInfo.UnselectedButton.onClick.RemoveAllListeners();
                    sideButtonInfo.UnselectedButton.onClick.AddListener(() => { ScrollTo(sideButtonInfo.HighlightedChild, sideButtonInfo.HighlightedChildSelectionOffset); });
                }
                else if (sideButtonInfo.Type == SideButtonType.Patience
                    && m_PatienceUpgradePanelsUI.Count > 0)
                {
                    sideButtonInfo.HighlightedChild = m_PatienceUpgradePanelsUI[0].GetComponent<RectTransform>();
                    sideButtonInfo.UnselectedButton.onClick.RemoveAllListeners();
                    sideButtonInfo.UnselectedButton.onClick.AddListener(() => { ScrollTo(sideButtonInfo.HighlightedChild, sideButtonInfo.HighlightedChildSelectionOffset); });
                }
                else
                {
                    sideButtonInfo.Holder.SetActive(false);
                }
            }

            DataLevel currentLevel = DataManager.GetCurrentDataLevel();
            if (currentLevel != null)
            {
                m_PlayButton.SetActive(true);
                m_PlayLockedButton.SetActive(false);
            }
            else
            {
                m_PlayButton.SetActive(false);
                m_PlayLockedButton.SetActive(true);
            }

            ResetScroll();
        }

        public override void OpenPopup(Action callback)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            ResetScroll();
            m_Popup.SetActive(true);
            m_OpeningSequence.PlaySequence(() =>
            {
                callback?.Invoke();
            });
        }

        public override void ClosePopup(Action callback)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_ClosingSequence.PlaySequence(() =>
            {
                m_Popup.SetActive(false);
                callback?.Invoke();
            });
        }

        public void OnCloseUpgradePopupButton()
        {
            UIManager.CloseUpgradePopup();
        }

        public void OnPlayButton()
        {
            DataLevel currrentLevel = DataManager.GetCurrentDataLevel();
            if (currrentLevel == null)
            {
                OpenMapCompletePopup();
            }
            else
            {
                if (HeartTimeCurrencyCounter.HasHeartTimeCurrency() || DataManager.HeartCurrency > 0)
                {
                    HeartCurrencyUIController.SetValueForShouldBeMinusOne(true);
                    UIManager.UIInteractionOff();
                    ClosePopup(() =>
                    {
                        UIManager.UIInteractionOn();
                        UIManager.SetupForGameplay();
                    });
                }
                else
                {
                    GeneralPopupUIManager.OpenNoMoreHeartPopup();
                }
            }
        }


        #region Upgrade Related
        public void UpgradePlayer(PlayerUpgradeType PlayerUpgradeType, PlayerUpgradePanelUI playerUpgradePanelUI)
        {
            int index = m_PlayerUpgradePanelsUI.IndexOf(playerUpgradePanelUI);
            PlayerUpgradePanelUI newPlayerUpgradePanelUI = m_DataMapUpdate.UpgradePlayerProperty(PlayerUpgradeType, playerUpgradePanelUI);
            newPlayerUpgradePanelUI.UpgradePopupUIManager = this;
            m_PlayerUpgradePanelsUI[index] = newPlayerUpgradePanelUI;
            newPlayerUpgradePanelUI.SetupNotification(m_NotificationStaff);

            if (index == 0)
            {
                SideButtonInfo sideButton = m_SideButtonsInfo.Find((x) => x.Type == SideButtonType.Staff);
                if (sideButton != null)
                {
                    sideButton.HighlightedChild = newPlayerUpgradePanelUI.GetComponent<RectTransform>();
                }
            }
        }

        public void UpgradeChairCapacity(ChairCapacityUpgradePanelUI chairCapacityUpgradePanelUI)
        {
            m_ChairCapacityUpgradePanelsUI.Remove(chairCapacityUpgradePanelUI);
            ChairCapacityUpgradePanelUI newChairCapacityUpgradePanelUI = m_DataMapUpdate.UpgradeChairCapacityProperty(chairCapacityUpgradePanelUI);
            newChairCapacityUpgradePanelUI.UpgradePopupUIManager = this;
            m_ChairCapacityUpgradePanelsUI.Add(newChairCapacityUpgradePanelUI);
            newChairCapacityUpgradePanelUI.SetupNotification(m_NotificationStaff);
        }


        public void UpgradeWorker(DataWorker dataWorker, WorkerUpgradePanelUI workerUpgradePanelUI)
        {
            m_WorkerUpgradePanelsUI.Remove(workerUpgradePanelUI);
            WorkerUpgradePanelUI newWorkerUpgradePanelUI = m_DataMapUpdate.UpgradeWorkerServingDurationProperty(dataWorker, workerUpgradePanelUI);
            newWorkerUpgradePanelUI.UpgradePopupUIManager = this;
            m_WorkerUpgradePanelsUI.Add(newWorkerUpgradePanelUI);
            newWorkerUpgradePanelUI.SetupNotification(m_NotificationStaff);
        }

        public void UpgradeStation(DataStation dataStaion, StationUpgradeType stationUpgradeType, StationUpgradePanelUI stationUpgradePanelUI)
        {
            int index = 0;
            if (stationUpgradeType == StationUpgradeType.StationUpgradeType1)
            {
                index = m_StationUpgradePanelsUI.IndexOf(stationUpgradePanelUI);
            }
            else
            {
                index = m_StationUpgradeRevenuePanelsUI.IndexOf(stationUpgradePanelUI);
            }

            StationUpgradePanelUI newStationUpgradePanelUI = m_DataMapUpdate.UpgradeStationProperty(dataStaion, stationUpgradeType, stationUpgradePanelUI);

            newStationUpgradePanelUI.UpgradePopupUIManager = this;

            if (stationUpgradeType == StationUpgradeType.StationUpgradeType1)
            {
                m_StationUpgradePanelsUI[index] = newStationUpgradePanelUI;
                newStationUpgradePanelUI.SetupNotification(m_NotificationStations);
                if (index == 0)
                {
                    SideButtonInfo sideButton = m_SideButtonsInfo.Find((x) => x.Type == SideButtonType.Stations);
                    if (sideButton != null)
                    {
                        sideButton.HighlightedChild = newStationUpgradePanelUI.GetComponent<RectTransform>();
                    }
                }
            }
            else
            {
                m_StationUpgradeRevenuePanelsUI[index] = newStationUpgradePanelUI;
                newStationUpgradePanelUI.SetupNotification(m_NotificationRevenue);

                if (index == 0)
                {
                    SideButtonInfo sideButton = m_SideButtonsInfo.Find((x) => x.Type == SideButtonType.Reveneu);
                    if (sideButton != null)
                    {
                        sideButton.HighlightedChild = newStationUpgradePanelUI.GetComponent<RectTransform>();
                    }
                }
            }
        }

        public void UpgradePatience(DataPatience dataPatience, PatienceUpgradePanelUI patienceUpgradePanelUI)
        {
            m_PatienceUpgradePanelsUI.Remove(patienceUpgradePanelUI);
            PatienceUpgradePanelUI newPatienceUpgradePanelUI = m_DataMapUpdate.UpgradePatienceUpgradePanel(dataPatience, patienceUpgradePanelUI);
            newPatienceUpgradePanelUI.UpgradePopupUIManager = this;
            m_PatienceUpgradePanelsUI.Add(newPatienceUpgradePanelUI);
            newPatienceUpgradePanelUI.SetupNotification(m_NotificationPatience);
        }
        #endregion

        #region Scroll Related
        public void ResetScroll()
        {
            //m_ScrollRect.horizontalNormalizedPosition = 0f;
            RectTransform content = m_ScrollRect.content;
            Vector2 anchorPosition = content.anchoredPosition;
            anchorPosition.x = 0;
            content.anchoredPosition = anchorPosition;

            foreach (var sideButtonInfo in m_SideButtonsInfo)
            {
                if (sideButtonInfo.Type == SideButtonType.Staff)
                {
                    sideButtonInfo.Selected.SetActive(true);
                    sideButtonInfo.UnselectedButton.gameObject.SetActive(false);
                }
                else
                {
                    sideButtonInfo.Selected.SetActive(false);
                    sideButtonInfo.UnselectedButton.gameObject.SetActive(true);
                }
            }
        }

        public void OnScrollChanged()
        {
            SideButtonInfo selectedSideButton = m_SideButtonsInfo[0];
            foreach (var sideButtonInfo in m_SideButtonsInfo)
            {
                sideButtonInfo.Selected.SetActive(false);
                sideButtonInfo.UnselectedButton.gameObject.SetActive(true);
                if (sideButtonInfo.IsVisibleInViewport(m_ScrollRect, m_ContentAutoSelectOffset))
                {
                    selectedSideButton = sideButtonInfo;
                }
            }

            selectedSideButton.Selected.SetActive(true);
            selectedSideButton.UnselectedButton.gameObject.SetActive(true);
        }

        public void ScrollTo(RectTransform target, float xOffset)
        {
            RectTransform contentHolder = m_ScrollRect.content;
            Vector3 targetLocalPosition = contentHolder.InverseTransformPoint(target.position);
            float adjustedX = targetLocalPosition.x + xOffset;
            float normalizedX = Mathf.Clamp01(adjustedX / contentHolder.rect.width);
            if (m_ScrollRect.horizontal)
            {
                m_ScrollRect.DOKill();
                m_ScrollRect.DOHorizontalNormalizedPos(normalizedX, m_ScrollDuration).SetEase(m_ScrollEase);
            }
        }
        #endregion

        #region InfoPopup
        public void OpenInfoPopup(string headerText, string discriptionText, Sprite previewSprite)
        {
            m_InfoHeaderText.text = headerText;
            m_InfoDiscriptionText.text = discriptionText;
            m_InfoPreview.sprite = previewSprite;

            SoundManager.PlaySound(SoundType.PopupWhoosh);
            UIManager.UIInteractionOff();
            m_InfoPopup.SetActive(true);
            m_InfoPopupOpeningSequence.PlaySequence(() =>
            {
                UIManager.UIInteractionOn();
            });
        }

        public void CloseInfoPopup()
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            UIManager.UIInteractionOff();
            m_InfoPopupClosingSequence.PlaySequence(() =>
            {
                m_InfoPopup.SetActive(false);
                UIManager.UIInteractionOn();
            });
        }
        #endregion

        #region MapComplete Popup
        public void OpenMapCompletePopup()
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            UIManager.UIInteractionOff();
            m_MapCompletePopup.SetActive(true);
            m_MapCompleteOpeningSequence.PlaySequence(() =>
            {
                UIManager.UIInteractionOn();
            });
        }

        public void CloseMapCompletePopup()
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            UIManager.UIInteractionOff();
            m_MapCompleteClosingSequence.PlaySequence(() =>
            {
                m_MapCompletePopup.SetActive(false);
                UIManager.UIInteractionOn();
            });
        }
        #endregion

        #region Helper

        public Transform GetPanelAtSiblingIndex(int index)
        {
            if (m_PanelHolder == null || index < 0 || index >= m_PanelHolder.childCount)
                return null;

            return m_PanelHolder.GetChild(index);
        }

        public void ScrollRectOff()
        {
            m_ScrollRect.enabled = false;
        }

        public void ScrollRectOn()
        {
            m_ScrollRect.enabled = true;
        }


        public NotificationParentUI GetNotificationStaff()
        {
            return m_NotificationStaff;
        }

        public NotificationParentUI GetNotificationStations()
        {
            return m_NotificationStations;
        }

        public NotificationParentUI GetNotificationRevenue()
        {
            return m_NotificationRevenue;
        }

        public NotificationParentUI GetNotificationPatience()
        {
            return m_NotificationPatience;
        }
        #endregion
    }
}
