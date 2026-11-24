using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

namespace Isometric.UI
{
    public class StepFillBarUIHolder : MonoBehaviour
    {
        [SerializeField] List<StepFillBarUI> m_StepFillBarsUI = new();
        [SerializeField, ReadOnly] StepFillBarUI m_CurrentFillBarUI = null;

        public StepFillBarUI GetTargetFillBarUI(int numberOfBars)
        {
            if(m_CurrentFillBarUI != null)
            {
                m_CurrentFillBarUI.gameObject.SetActive(true);
                return m_CurrentFillBarUI;
            }

            m_CurrentFillBarUI = m_StepFillBarsUI[numberOfBars - 1];
            m_CurrentFillBarUI.gameObject.SetActive(true);
            return m_CurrentFillBarUI;
        }
    }
}
