using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using System;
using UnityEngine.UI;
using Arc;
using DG.Tweening;

namespace Isometric.UI
{
    public class StepFillBarUI : MonoBehaviour
    {
        [SerializeField] bool m_UseBarsPreset = true;
        [HideIf(nameof(m_UseBarsPreset))] public GameObject SingleBar;
        [HideIf(nameof(m_UseBarsPreset))] public List<GameObject> m_Bars;
        public List<StepFillBarInfo> BarsInfo;
        [SerializeField] bool m_UseFillAmount = false, m_UseAlphaAmount = true;
        public float BarFillDuration = 0.25f;


        public void SetupBars(int numberOfBars)
        {
            if (m_UseBarsPreset)
            {
                return;
            }

            if (SingleBar != null)
            {
                List<Transform> childrenToDestroy = new List<Transform>();
                // m_Bars = new List<GameObject>();
                BarsInfo = new List<StepFillBarInfo>();

                foreach (Transform child in transform)
                {
                    if (child != SingleBar.transform)
                    {
                        childrenToDestroy.Add(child);
                    }
                    else
                    {
                        // Transform singleBarChild = m_SingleBar.transform.GetChild(0);
                        StepFillBarInfo singleBarChild = SingleBar.transform.GetComponentInChildren<StepFillBarInfo>();
                        if (singleBarChild != null)
                        {
                            BarsInfo.Add(singleBarChild);
                            // m_Bars.Add(singleBarChild.gameObject);
                        }
                    }
                }

                foreach (Transform child in childrenToDestroy)
                {
                    Destroy(child.gameObject);
                }

                for (int i = 0; i < numberOfBars - 1; i++)
                {
                    GameObject temBar = Instantiate(SingleBar, transform);
                    // Transform child = temBar.transform.GetChild(0);
                    StepFillBarInfo child = temBar.transform.GetComponentInChildren<StepFillBarInfo>();
                    if (child != null)
                    {
                        BarsInfo.Add(child);
                        // m_Bars.Add(singleBarChild.gameObject);
                    }
                }
            }
        }

        public void SetFillAmount(int fillAmount, bool doSetupOnly = false)
        {
            if (!m_UseFillAmount)
            {
                return;
            }

            for (int i = 0; i < BarsInfo.Count; i++)
            {
                BarsInfo[i].Bar.SetActive(true);
                Image barImage = BarsInfo[i].BarImage;
                if(i < fillAmount)
                {
                    if (doSetupOnly)
                    {
                        barImage.fillAmount = 1;
                    }
                    else
                    {
                        if(i == fillAmount - 1)
                        {
                            int targetIndex = i;
                            barImage.fillAmount = 0;
                            barImage.DOFillAmount(1, BarFillDuration).OnComplete(() =>
                            {
                                PlayDoTweenSequence tween = BarsInfo[targetIndex].BarPostActivationTween;
                                if(tween != null)
                                {
                                    tween.PlaySequence();
                                } 
                            });
                        }
                        else
                        {
                            barImage.fillAmount = 1;
                        }
                    }
                }
                else
                {
                    barImage.fillAmount = 0;
                }
            }
            /*for (int i = 0; i < m_Bars.Count; i++)
            {
                m_Bars[i].SetActive(i < fillAmount);
            }*/
        }

        public void SetAlphaAmount(int fillAmount, bool doSetupOnly = false)
        {
            if (!m_UseAlphaAmount)
            {
                return;
            }
            
            for (int i = 0; i < BarsInfo.Count; i++)
            {
                StepFillBarInfo stepFillBarInfo = BarsInfo[i];

                stepFillBarInfo.Bar.SetActive(true);
                Image barImage = stepFillBarInfo.BarImage;
                if(i < fillAmount)
                {
                    if (doSetupOnly)
                    {
                        Color color = barImage.color;
                        color.a = 1f;
                        barImage.color = color;
                    }
                    else
                    {
                        if(i == fillAmount - 1)
                        {
                            int targetIndex = i;
                            Color color = barImage.color;
                            color.a = 0f;
                            barImage.color = color;
                            barImage.DOFade(1f, BarFillDuration).OnComplete(() =>
                            {
                                PlayDoTweenSequence tween = BarsInfo[targetIndex].BarPostActivationTween;
                                if(tween != null)
                                {
                                    tween.PlaySequence();
                                } 
                            });
                        }
                        else
                        {
                            Color color = barImage.color;
                            color.a = 1f;
                            barImage.color = color;
                        }
                    }
                    stepFillBarInfo.OnUpgradeDone();
                }
                else
                {
                    Color color = barImage.color;
                    color.a = 0f;
                    barImage.color = color;

                    if (doSetupOnly)
                    {
                        stepFillBarInfo.OnNoUpgrade();
                    }
                }
            }
            /*for (int i = 0; i < m_Bars.Count; i++)
            {
                m_Bars[i].SetActive(i < fillAmount);
            }*/
        }
    }
}
