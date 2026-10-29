using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Isometric.UI;
using Isometric.Data;

namespace Isometric.Tutorial
{
    public class TutorialPart18 : TutorialContainer
    {
        [SerializeField] readonly string m_TutorialKey = "TutorialPart18";
        [SerializeField] UpgradePopupUIManager m_UpgradePopupUIManager;

        [Header("---Step 1---")]
        [SerializeField] GameObject m_Popup;
        [SerializeField] GameObject m_DialogStep1;
        [SerializeField] Transform m_ArrowStep1;
        [SerializeField] Vector2 m_FocusScaleSetp1;
        [SerializeField] Vector2 m_FocusOffsetStep1;

        [Header("---Step 2---")]
        [SerializeField] GameObject m_DialogStep2;
        [SerializeField] GameObject m_Arrow1Step2;
        [SerializeField] GameObject m_Arrow2Step2;
        [SerializeField] Vector3 m_ArrowOffset;
        [SerializeField] Vector3 m_PanelFocusOffest;
        [SerializeField] Vector3 m_PanelFocusSize;
        [SerializeField] float m_ScrollRectSetupDelay;
        [SerializeField] float m_ScrollRectSetupOffset;

        private Button m_WorkerUpgradePane1Button = null;
        private Button m_WorkerUpgradePane2Button = null;


        private void OnEnable()
        {
            m_UpgradePopupUIManager.OnUpgradePopupOpened += SetUpgradeRecommendationOption;
        }
        private void OnDisable()
        {
            m_UpgradePopupUIManager.OnUpgradePopupOpened -= SetUpgradeRecommendationOption;
        }

        private void SetUpgradeRecommendationOption()
        {
            if (DataManager.GetBool(m_TutorialKey, false) == false)
            {
                m_UpgradePopupUIManager.SetShowUpgradeRecommendationOption(false);
            }
        }

        public override void Play()
        {
            Step1();
        }

        public void Step1()
        {
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);
            TutorialManager.ActivateTapToContinueText(TutorailTextPosition.Down);

            UIManager.UIInteractionOff();
            m_UpgradePopupUIManager.ScrollRectOff();
            m_Popup.SetActive(true);
            m_DialogStep1.SetActive(true);
            m_ArrowStep1.gameObject.SetActive(true);

            GameObject difficultyHolder = m_UpgradePopupUIManager.GetLevelDifficultyHolder();

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.SetBackgroundButtonCallback(Step2);
            TutorialFocusManager.FocusAtPosition(difficultyHolder.transform.position, m_FocusOffsetStep1, m_FocusScaleSetp1, FocusShapeType.LongRectangle);
        }

        public void Step2()
        {
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            TutorialManager.DeactivateTapToContinueText();

            m_DialogStep1.SetActive(false);
            m_ArrowStep1.gameObject.SetActive(false);

            m_DialogStep2.SetActive(true);
            

            Transform workerUpgradePaneTransform1 = m_UpgradePopupUIManager.GetPanelAtSiblingIndex(3);
            Transform workerUpgradePaneTransform2 = m_UpgradePopupUIManager.GetPanelAtSiblingIndex(4);

            if (workerUpgradePaneTransform1 != null
                && workerUpgradePaneTransform1.TryGetComponent(out WorkerUpgradePanelUI workerUpgradePane1)
                && workerUpgradePaneTransform2 != null
                && workerUpgradePaneTransform2.TryGetComponent(out WorkerUpgradePanelUI workerUpgradePane2))
            {
                if (workerUpgradePane1.UpgradeButton.gameObject.activeSelf == true
                    || workerUpgradePane2.UpgradeButton.gameObject.activeSelf == true)
                {
                    //UIManager.FullUIInteractionOff();
                    m_UpgradePopupUIManager.ScrollRectOn();

                    if (workerUpgradePaneTransform1.TryGetComponent(out RectTransform rectTransform))
                    {
                        m_UpgradePopupUIManager.ScrollTo(rectTransform, m_ScrollRectSetupOffset);
                    }

                    m_WorkerUpgradePane1Button = workerUpgradePane1.UpgradeButton;
                    m_WorkerUpgradePane2Button = workerUpgradePane2.UpgradeButton;

                    CoroutineManager.LateAction(() =>
                    {
                        m_UpgradePopupUIManager.ScrollRectOff();

                        workerUpgradePane1.UpgradeButton.onClick.AddListener(Complete);
                        workerUpgradePane2.UpgradeButton.onClick.AddListener(Complete);

                        if (workerUpgradePane1.UpgradeButton.gameObject.activeSelf)
                        {
                            TutorialFocusManager.FocusAtPosition(workerUpgradePaneTransform1.transform.position, m_PanelFocusOffest, m_PanelFocusSize, FocusShapeType.Square);
                            m_Arrow1Step2.transform.position = workerUpgradePaneTransform1.position + m_ArrowOffset;
                            m_Arrow1Step2.SetActive(true);
                        }

                        if (workerUpgradePane2.UpgradeButton.gameObject.activeSelf)
                        {
                            TutorialFocusManager.FocusAtPosition(workerUpgradePaneTransform2.transform.position, m_PanelFocusOffest, m_PanelFocusSize, FocusShapeType.Square);
                            m_Arrow2Step2.transform.position = workerUpgradePaneTransform2.position + m_ArrowOffset;
                            m_Arrow2Step2.SetActive(true);
                        }
                        
                        UIManager.UIInteractionOn();

                    }, m_ScrollRectSetupDelay);
                }
                else
                {
                    Complete();
                }
            }
            else
            {
                Complete();
            }
        }

        /*public void CompleteCheck()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var hit in results)
            {
                if (m_ChefUpgradePane1Button.gameObject.activeSelf == true &&
                    hit.gameObject == m_ChefUpgradePane1Button.gameObject)
                {
                    m_ChefUpgradePane1Button.onClick?.Invoke();
                    Complete();
                    return;
                }

                if (m_ChefUpgradePane2Button.gameObject.activeSelf == true &&
                    hit.gameObject == m_ChefUpgradePane2Button.gameObject)
                {
                    m_ChefUpgradePane2Button.onClick?.Invoke();
                    Complete();
                    return;
                }
            }
        }*/

        public void Complete()
        {
            m_WorkerUpgradePane1Button.onClick.RemoveListener(Complete);
            m_WorkerUpgradePane2Button.onClick.RemoveListener(Complete);

            m_UpgradePopupUIManager.ScrollRectOn();
            m_UpgradePopupUIManager.SetShowUpgradeRecommendationOption(true);
            TutorialFocusManager.StopAllFocus();

            m_DialogStep2.SetActive(false);
            m_Arrow1Step2.SetActive(false);
            m_Arrow2Step2.SetActive(false);
            m_Popup.SetActive(false);

            Stop();
        }
    }
}
