using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using NaughtyAttributes;
using Arc;
using Isometric.Data;
using Isometric.Sound;

namespace Isometric.UI
{
    public class MapUIManager : UIPopupBase
    {
        [Serializable]
        private class MapPanelInfo
        {
            public bool IsCommingSoon;
            [AllowNesting, HideIf(nameof(IsCommingSoon))]
            public MapType MapType;
            [AllowNesting, HideIf(nameof(IsCommingSoon))]
            public RectTransform Holder;
            public GameObject Selected;
            public TextMeshProUGUI SelectedHeadingText;
            public TextMeshProUGUI SelectedLevelText;
            public GameObject Unselected;
            public TextMeshProUGUI UnselectedHeadingText;
            public GameObject ComingSoon;
            public Button SelectButton;
            [AllowNesting, ReadOnly]
            public Vector3 AnchorPosition;
            [AllowNesting, ReadOnly]
            public bool IsUnlocked;
        }

        [Header("---Setup---")]
        [SerializeField] GameObject m_Popup;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;

        [Header("---Map Panel---")]
        [SerializeField] List<MapPanelInfo> m_MapPanelsInfo;
        [SerializeField] RectTransform m_HolderParent; // parent of all Holder
        [SerializeField] float spacing = 200f;
        [SerializeField] float snapDuration = 0.3f;

        private Vector2 m_DragStartPos;
        private Vector2 m_ContentStartPos;
        private bool m_IsDragging = false;

        public override void Setup()
        {
            MapType mapType = DataManager.CurrentMapType;
            int selectedIndex = 0;

            float startX = 0f;

            for (int i = 0; i < m_MapPanelsInfo.Count; i++)
            {
                m_MapPanelsInfo[i].Selected.SetActive(false);
                m_MapPanelsInfo[i].Unselected.SetActive(false);
                m_MapPanelsInfo[i].UnselectedHeadingText.gameObject.SetActive(false);
                m_MapPanelsInfo[i].ComingSoon.SetActive(false);
                m_MapPanelsInfo[i].SelectButton.gameObject.SetActive(false);

                if (m_MapPanelsInfo[i].IsCommingSoon)
                {
                    m_MapPanelsInfo[i].Unselected.SetActive(true);
                    m_MapPanelsInfo[i].ComingSoon.SetActive(true);
                    m_MapPanelsInfo[i].IsUnlocked = false;
                }
                else
                {
                    if (mapType == m_MapPanelsInfo[i].MapType)
                    {
                        m_MapPanelsInfo[i].Selected.SetActive(true);
                        m_MapPanelsInfo[i].SelectedHeadingText.text = DataManager.GetMapName(mapType);
                        int currentLevel = DataManager.CurrentMapTotalLevels <= DataManager.CurrentMapLevelIndex + 1? DataManager.CurrentMapTotalLevels : DataManager.CurrentMapLevelIndex + 1;
                        int totalLevels = DataManager.CurrentMapTotalLevels;

                        m_MapPanelsInfo[i].SelectedLevelText.text = currentLevel + "/" + totalLevels;

                        m_MapPanelsInfo[i].IsUnlocked = DataManager.IsMapUnlocked(mapType);

                        m_MapPanelsInfo[i].SelectButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        m_MapPanelsInfo[i].Unselected.SetActive(true);
                        m_MapPanelsInfo[i].UnselectedHeadingText.text = DataManager.GetMapName(m_MapPanelsInfo[i].MapType);
                        m_MapPanelsInfo[i].IsUnlocked = DataManager.IsMapUnlocked(m_MapPanelsInfo[i].MapType);
                    }

                    m_MapPanelsInfo[i].SelectButton.onClick.RemoveAllListeners();
                    MapType temMapType = m_MapPanelsInfo[i].MapType;
                    m_MapPanelsInfo[i].SelectButton.onClick.AddListener(() => OnSelectButton(temMapType));

                }

                var panel = m_MapPanelsInfo[i].Holder;
                panel.anchoredPosition = new Vector2(startX, panel.anchoredPosition.y);
                m_MapPanelsInfo[i].AnchorPosition.x = -panel.anchoredPosition.x;
                startX += spacing;
            }

            SnapToPanel(selectedIndex, true);
        }

        public override void OpenPopup(Action onComplete)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);

            MapType mapType = DataManager.CurrentMapType;
            int index = m_MapPanelsInfo.FindIndex((x) => x.MapType == mapType);
            SnapToPanel(index, true);

            m_Popup.SetActive(true);
            m_OpeningSequence.PlaySequence(() =>
            {
                onComplete?.Invoke();
            });
        }

        public override void ClosePopup(Action onComplete)
        {
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            m_ClosingSequence.PlaySequence(() =>
            {
                onComplete?.Invoke();
                m_Popup.SetActive(false);
            });
        }

        private void OnSelectButton(MapType mapType)
        {
            MapType currentMapType = DataManager.CurrentMapType;

            if (mapType == currentMapType)
            {
                UIManager.ClosePopup<MapUIManager>();
            }
        }

        public void OnCloseButton()
        {
            UIManager.ClosePopup<MapUIManager>();
        }

        public void StartDrag(BaseEventData eventData)
        {
            Vector2 inputPosition = ((PointerEventData)eventData).position;
            foreach (var mapPanelInfo in m_MapPanelsInfo)
            {
                mapPanelInfo.SelectButton.gameObject.SetActive(false);
            }

            m_DragStartPos = inputPosition;
            m_ContentStartPos = m_HolderParent.anchoredPosition;
            m_IsDragging = true;
        }

        public void ContinueDrag(BaseEventData eventData)
        {
            Vector2 inputPosition = ((PointerEventData)eventData).position;

            float deltaX = inputPosition.x - m_DragStartPos.x;
            float targetX = m_ContentStartPos.x + deltaX;

            float minX = m_MapPanelsInfo[^1].AnchorPosition.x; // last panel (right limit)
            float maxX = m_MapPanelsInfo[0].AnchorPosition.x;  // first panel (left limit)

            targetX = Mathf.Clamp(targetX, minX, maxX);

            m_HolderParent.anchoredPosition = new Vector2(targetX, m_HolderParent.anchoredPosition.y);
        }

        public void EndDrag(BaseEventData eventData)
        {
            m_IsDragging = false;
            SnapToClosestPanel();
        }

        private void SnapToClosestPanel()
        {
            float closestDistance = float.MaxValue;
            int closestIndex = 0;

            for (int i = 0; i < m_MapPanelsInfo.Count; i++)
            {
                float worldX = m_MapPanelsInfo[i].AnchorPosition.x;
                float distance = Mathf.Abs(worldX - m_HolderParent.anchoredPosition.x);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            SnapToPanel(closestIndex, false);
        }

        private void SnapToPanel(int index, bool snapInstant)
        {
            if (index < 0 || index >= m_MapPanelsInfo.Count) return;

            Vector2 newPos = m_MapPanelsInfo[index].AnchorPosition;

            foreach (var mapPanelInfo in m_MapPanelsInfo)
            {
                mapPanelInfo.SelectButton.gameObject.SetActive(false);
            }

            if (snapInstant)
            {
                m_HolderParent.anchoredPosition = newPos;
                if (m_MapPanelsInfo[index].IsUnlocked)
                {
                    m_MapPanelsInfo[index].SelectButton.gameObject.SetActive(true);
                }
            }
            else
            {
                m_HolderParent.DOAnchorPos(newPos, snapDuration).SetEase(Ease.OutCubic).OnComplete(() => 
                {
                    if (m_MapPanelsInfo[index].IsUnlocked)
                    {
                        m_MapPanelsInfo[index].SelectButton.gameObject.SetActive(true);
                    }
                });
            }
        }
    }
}
