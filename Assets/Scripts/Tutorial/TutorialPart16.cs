using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Isometric.UI;

namespace Isometric.Tutorial.Old
{
    public class TutorialPart16 : TutorialContainer
    {
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

        private Button m_ChefUpgradePane1Button = null;
        private Button m_ChefUpgradePane2Button = null;

        public override void Play()
        {
            Step1();
        }

        public void Step1()
        {
            TutorialManager.ActivateTutorailText(TutorailTextPosition.Up);
            TutorialManager.ActivateTapToContinueText(TutorailTextPosition.Down);

            m_Popup.SetActive(true);
            m_DialogStep1.SetActive(true);
            //m_ArrowStep1.gameObject.SetActive(true);

            /* GameObject difficultyHolder = m_UpgradePopupUIManager.GetLevelDifficultyHolder();

            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.FocusAtPosition(difficultyHolder.transform.position, m_FocusOffsetStep1, m_FocusScaleSetp1, FocusShapeType.Circle); */
        }

        public void Step2()
        {
            TutorialFocusManager.RemoveBackgroundButtonCallback();
            TutorialFocusManager.StopAllFocus();
            TutorialManager.DeactivateTapToContinueText();

            m_DialogStep1.SetActive(false);
            //m_ArrowStep1.gameObject.SetActive(false);

            m_DialogStep2.SetActive(true);
            

            Transform chefUpgradePaneTransform1 = m_UpgradePopupUIManager.GetPanelAtSiblingIndex(4);
            Transform chefUpgradePaneTransform2 = m_UpgradePopupUIManager.GetPanelAtSiblingIndex(5);

            /* if (chefUpgradePaneTransform1 != null
                && chefUpgradePaneTransform1.TryGetComponent(out ChefUpgradePanelUI chefUpgradePane1)
                && chefUpgradePaneTransform2 != null
                && chefUpgradePaneTransform2.TryGetComponent(out ChefUpgradePanelUI chefUpgradePane2))
            {
                if (chefUpgradePane1.UpgradeButton.gameObject.activeSelf == true
                    || chefUpgradePane2.UpgradeButton.gameObject.activeSelf == true)
                {
                    //UIManager.FullUIInteractionOff();

                    if (chefUpgradePaneTransform1.TryGetComponent(out RectTransform rectTransform))
                    {
                        m_UpgradePopupUIManager.ScrollTo(rectTransform, m_ScrollRectSetupOffset);
                    }

                    m_ChefUpgradePane1Button = chefUpgradePane1.UpgradeButton;
                    m_ChefUpgradePane2Button = chefUpgradePane2.UpgradeButton;



                    CoroutineManager.LateAction(() =>
                    {
                        m_UpgradePopupUIManager.ScrollRectOff();

                        chefUpgradePane1.UpgradeButton.onClick.AddListener(Complete);
                        chefUpgradePane2.UpgradeButton.onClick.AddListener(Complete);


                        if (chefUpgradePane1.UpgradeButton.gameObject.activeSelf)
                        {
                            TutorialFocusManager.FocusAtPosition(chefUpgradePaneTransform1.transform.position, m_PanelFocusOffest, m_PanelFocusSize, FocusShapeType.Square);
                            m_Arrow1Step2.transform.position = chefUpgradePaneTransform1.position + m_ArrowOffset;
                            m_Arrow1Step2.SetActive(true);
                        }

                        if (chefUpgradePane2.UpgradeButton.gameObject.activeSelf)
                        {
                            TutorialFocusManager.FocusAtPosition(chefUpgradePaneTransform2.transform.position, m_PanelFocusOffest, m_PanelFocusSize, FocusShapeType.Square);
                            m_Arrow2Step2.transform.position = chefUpgradePaneTransform2.position + m_ArrowOffset;
                            m_Arrow2Step2.SetActive(true);
                        }

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
            } */
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
            m_ChefUpgradePane1Button.onClick.RemoveListener(Complete);
            m_ChefUpgradePane1Button.onClick.RemoveListener(Complete);

            m_UpgradePopupUIManager.ScrollRectOn();
            TutorialFocusManager.StopAllFocus();

            m_DialogStep2.SetActive(false);
            m_Arrow1Step2.SetActive(false);
            m_Arrow2Step2.SetActive(false);
            m_Popup.SetActive(false);

            Stop();
        }
    }
}
