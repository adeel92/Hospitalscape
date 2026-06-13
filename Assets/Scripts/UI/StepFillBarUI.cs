using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

namespace Isometric.UI
{
    public class StepFillBarUI : MonoBehaviour
    {
        public GameObject m_SingleBar;
        public List<GameObject> m_Bars;

        public void SetupBars(int numberOfBars)
        {
            if (m_SingleBar != null)
            {
                List<Transform> childrenToDestroy = new List<Transform>();
                m_Bars = new List<GameObject>();

                foreach (Transform child in transform)
                {
                    if (child != m_SingleBar.transform)
                    {
                        childrenToDestroy.Add(child);
                    }
                    else
                    {
                        Transform singleBarChild = m_SingleBar.transform.GetChild(0);
                        if (singleBarChild != null)
                        {
                            m_Bars.Add(singleBarChild.gameObject);
                        }
                    }
                }

                foreach (Transform child in childrenToDestroy)
                {
                    Destroy(child.gameObject);
                }

                for (int i = 0; i < numberOfBars - 1; i++)
                {
                    GameObject temBar = Instantiate(m_SingleBar, transform);
                    Transform child = temBar.transform.GetChild(0);
                    if (child != null)
                    {
                        m_Bars.Add(child.gameObject);
                    }
                }
            }
        }

        public void SetFillAmount(int fillAmount)
        {
            for (int i = 0; i < m_Bars.Count; i++)
            {
                m_Bars[i].SetActive(i < fillAmount);
            }
        }
    }
}
