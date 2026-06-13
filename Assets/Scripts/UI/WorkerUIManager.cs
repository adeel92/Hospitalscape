using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using Arc;
using Isometric.Data;


namespace Isometric.UI
{
    public class WorkerUIManager : MonoBehaviour
    {

        [SerializeField] DataMapUpdate m_DataMapUpdate;
        [SerializeField] GameObject m_Popup;

        [Header("---Worker and WorkerOrder Unlocking---")]
        [SerializeField] GameObject m_NewWorkerAndWorkerOrderPopup;
        [SerializeField] PlayDoTweenSequence m_NewWorkerAndWorkerOrderPopupOpeningSequence;
        [SerializeField] PlayDoTweenSequence m_NewWorkerAndWorkerOrderPopupClosingSequence;
        [SerializeField] Image m_NewWorkerImage;
        [SerializeField] Image m_NewWorkerSymbolImage;
        [SerializeField] Image m_NewWorkerTypeImage;
        [SerializeField] Button m_NewWorkerAndWorkerOrderUnlockingButton;

        [Header("---Worker Quantity Upgrade---")]
        [SerializeField] GameObject m_WorkerQuantityUpgradePopup;
        [SerializeField] PlayDoTweenSequence m_WorkerQuantityUpgradePopupOpeningSequence;
        [SerializeField] PlayDoTweenSequence m_WorkerQuantityUpgradePopupClosingSequence;
        [SerializeField] Transform m_WorkerQuantityUpgradeUIPanelHolder;
        [SerializeField] WorkerQuantityUpgradeUIPanel m_WorkerQuantityUpgradeUIPanelPrefab;
        private WorkerQuantityUpgradeUIPanel m_SelectedWorkerQuantityUpgradeUIPanel = null;
        [SerializeField] GameObject m_TickButton;
        [SerializeField] GameObject m_TickButtonUnclickble;

        [Header("---Just New Worker Unlocking---")]
        [SerializeField] GameObject m_NewWorkerPopup;
        [SerializeField] PlayDoTweenSequence m_NewWorkerPopupOpeningSequence;
        [SerializeField] PlayDoTweenSequence m_NewWorkerPopupClosingSequence;
        [SerializeField] Image m_NewWorkerPopupWorkerImage;

        public bool CheckNextWorkerAndWorkerOrderUnlockable()
        {
            List<Tuple<Sprite, Sprite, Sprite, Action>> nextWorkerAndAndWorkerUnlockble = m_DataMapUpdate.GetNewWorkerAndWorkerOrderUnlockable();

            if (nextWorkerAndAndWorkerUnlockble.Count > 0)
            {
                Tuple<Sprite, Sprite, Sprite, Action> nextWorkerandWorkerOrder = nextWorkerAndAndWorkerUnlockble[0];

                m_NewWorkerImage.sprite = nextWorkerandWorkerOrder.Item1;
                m_NewWorkerSymbolImage.sprite = nextWorkerandWorkerOrder.Item2;
                //m_NewWorkerTypeImage.sprite = nextWorkerandWorkerOrder.Item3;

                Action callback = nextWorkerandWorkerOrder.Item4;

                m_NewWorkerAndWorkerOrderUnlockingButton.onClick.RemoveAllListeners();
                m_NewWorkerAndWorkerOrderUnlockingButton.onClick.AddListener(() =>
                {
                    callback?.Invoke();
                    UIManager.HasNextWorkerAndWorkerOrderUnlocked();
                });

                return true;
            }
            else
            {
                return false;
            }
        }

        public void OpenNewWorkerAndWorkerOrderPopup(Action callback)
        {
            m_Popup.SetActive(true);
            m_NewWorkerAndWorkerOrderPopup.SetActive(true);
            m_NewWorkerAndWorkerOrderPopupOpeningSequence.PlaySequence(() => 
            {
                callback?.Invoke();
            });
        }

        public void CloseNewWorkerAndWorkerOrderPopup(Action callback)
        {
            m_NewWorkerAndWorkerOrderPopupClosingSequence.PlaySequence(() =>
            {
                m_NewWorkerAndWorkerOrderPopup.SetActive(false);
                m_Popup.SetActive(false);
                callback?.Invoke();
            });
        }

        public bool CheckNextWorkerQuantityUpgrade()
        {
            foreach (Transform child in m_WorkerQuantityUpgradeUIPanelHolder.transform)
            {
                Destroy(child.gameObject);
            }

            Tuple<bool, List<WorkerQuantityUpgradeUIPanel>> workerQuantityUpgrade = m_DataMapUpdate.GetWorkerQuantityUpgrade(this, m_WorkerQuantityUpgradeUIPanelHolder, m_WorkerQuantityUpgradeUIPanelPrefab);

            if (workerQuantityUpgrade.Item1 == true)
            {
                m_TickButton.SetActive(false);
                m_TickButtonUnclickble.SetActive(true);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void OpenWorkerQuantityUpgradePopup(Action callback)
        {
            m_Popup.SetActive(true);
            m_WorkerQuantityUpgradePopup.SetActive(true);
            m_WorkerQuantityUpgradePopupOpeningSequence.PlaySequence(() =>
            {
                callback?.Invoke();
            });
        }

        public void OnWorkerQuantityUpgraded()
        {
            if (m_SelectedWorkerQuantityUpgradeUIPanel != null)
            {
                m_NewWorkerPopupWorkerImage.sprite = m_SelectedWorkerQuantityUpgradeUIPanel.OnUpgraded();
                m_SelectedWorkerQuantityUpgradeUIPanel = null;
            }

            UIManager.UIInteractionOff();
            m_WorkerQuantityUpgradePopupClosingSequence.PlaySequence(() =>
            {
                m_WorkerQuantityUpgradePopup.SetActive(false);
                m_NewWorkerPopup.SetActive(true);
                m_NewWorkerPopupOpeningSequence.PlaySequence(() =>
                {
                    UIManager.UIInteractionOn();
                });
            });
        }

        public void OnCloseNewWorkerPopup()
        {
            UIManager.HasNextWorkerQuanityUpgrade();
        }

        public void CloseNewWorkerPopup(Action onComplete)
        {
            m_NewWorkerPopupClosingSequence.PlaySequence(() =>
            {
                m_NewWorkerPopup.SetActive(false);
                m_Popup.SetActive(false);
                onComplete?.Invoke();
            });
        }

        public void SelectWorkerQuantityUpgradeUIPanel(WorkerQuantityUpgradeUIPanel workerQuantityUpgradeUIPanel)
        {
            if (m_SelectedWorkerQuantityUpgradeUIPanel == workerQuantityUpgradeUIPanel)
            {
                return;
            }

            if (m_SelectedWorkerQuantityUpgradeUIPanel != null)
            {
                m_SelectedWorkerQuantityUpgradeUIPanel.OnUnselected();
            }
            else if(m_SelectedWorkerQuantityUpgradeUIPanel == null)
            {
                m_TickButton.SetActive(true);
                m_TickButtonUnclickble.SetActive(false);
            }

            m_SelectedWorkerQuantityUpgradeUIPanel = workerQuantityUpgradeUIPanel;
        }
        
    }
}
