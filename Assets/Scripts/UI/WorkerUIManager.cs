using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Arc;
using Isometric.Data;
using NaughtyAttributes;


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
        //---Choice
        [SerializeField] bool m_UseChoiceBasedWorkerQuantityUpgrade = true;
        [SerializeField, NaughtyAttributes.ShowIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] GameObject m_WorkerQuantityUpgradePopup;
        [SerializeField, NaughtyAttributes.ShowIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] PlayDoTweenSequence m_WorkerQuantityUpgradePopupOpeningSequence;
        [SerializeField, NaughtyAttributes.ShowIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] PlayDoTweenSequence m_WorkerQuantityUpgradePopupClosingSequence;
        [SerializeField, NaughtyAttributes.ShowIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] Transform m_WorkerQuantityUpgradeUIPanelHolder;
        [SerializeField, NaughtyAttributes.ShowIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] WorkerQuantityUpgradeUIPanel m_WorkerQuantityUpgradeUIPanelPrefab;
        private WorkerQuantityUpgradeUIPanel m_SelectedWorkerQuantityUpgradeUIPanel = null;
        [SerializeField, NaughtyAttributes.ShowIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] GameObject m_TickButton;
        [SerializeField, NaughtyAttributes.ShowIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] GameObject m_TickButtonUnclickble;

        //---Without Choice
        [SerializeField, HideIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] GameObject m_WorkerQuantityUpgradeNoChoicePopup;
        [SerializeField, HideIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] PlayDoTweenSequence m_WorkerQuantityUpgradeNoChoicePopupOpeningSequence;
        [SerializeField, HideIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] PlayDoTweenSequence m_WorkerQuantityUpgradeNoChoicePopupClosingSequence;
        [SerializeField, HideIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] Image m_UpgradedWorkerImage;
        [SerializeField, HideIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] Image m_UpgradedWorkerSymbolImage;
        [SerializeField, HideIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] Image m_UpgradedWorkerTypeImage;
        [SerializeField, HideIf(nameof(m_UseChoiceBasedWorkerQuantityUpgrade))] Button m_WorkerQuantityUpgradeUnlockingButton;

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
            if (m_UseChoiceBasedWorkerQuantityUpgrade)
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

            else
            {
                List<Tuple<Sprite, Sprite, Sprite, Action>> noChoiceWorkerQuantityUpgrades = m_DataMapUpdate.GetNoChoiceWorkerQuantityUpgrade();
                if (noChoiceWorkerQuantityUpgrades != null && noChoiceWorkerQuantityUpgrades.Count > 0)
                {
                    Tuple<Sprite, Sprite, Sprite, Action> noChoiceWorkerQuantityUpgrade = noChoiceWorkerQuantityUpgrades[0];

                    m_UpgradedWorkerImage.sprite = noChoiceWorkerQuantityUpgrade.Item1;
                    m_UpgradedWorkerSymbolImage.sprite = noChoiceWorkerQuantityUpgrade.Item2;
                    //m_NewWorkerTypeImage.sprite = noChoiceWorkerQuantityUpgrade.Item3;

                    Action callback = noChoiceWorkerQuantityUpgrade.Item4;

                    m_WorkerQuantityUpgradeUnlockingButton.onClick.RemoveAllListeners();
                    m_WorkerQuantityUpgradeUnlockingButton.onClick.AddListener(() =>
                    {
                        callback?.Invoke();
                        UIManager.HasNextWorkerQuanityUpgrade(false);
                    });

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void SelectWorkerQuantityUpgradePopup(Action callback)
        {
            if (m_UseChoiceBasedWorkerQuantityUpgrade)
            {
                OpenWorkerQuantityUpgradePopup(callback);
            }
            else
            {
                OpenWorkerQuantityUpgradeNoChoicePopup(callback);
            }
        }

        public void OpenWorkerQuantityUpgradeNoChoicePopup(Action callback)
        {
            m_Popup.SetActive(true);
            m_WorkerQuantityUpgradeNoChoicePopup.SetActive(true);
            m_WorkerQuantityUpgradeNoChoicePopupOpeningSequence.PlaySequence(() => 
            {
                callback?.Invoke();
            });
        }

        public void CloceWorkerQuantityUpgradeNoChoicePopup(Action callback)
        {
            m_WorkerQuantityUpgradeNoChoicePopupClosingSequence.PlaySequence(() =>
            {
                m_WorkerQuantityUpgradeNoChoicePopup.SetActive(false);
                m_Popup.SetActive(false);
                callback?.Invoke();
            });
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
            UIManager.HasNextWorkerQuanityUpgrade(true);
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
