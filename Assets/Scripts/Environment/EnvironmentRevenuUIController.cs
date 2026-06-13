using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Arc;
using Isometric.Customer;

namespace Isometric.Environment
{
    public class EnvironmentRevenuUIController : MonoBehaviour
    {
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;

        [SerializeField] TextMeshProUGUI m_RevenueText;

        [SerializeField] Image m_TipFace;
        [SerializeField] Sprite m_HappyFace;
        [SerializeField] Sprite m_AngryFace;
        [SerializeField] Sprite m_FuriousFace;


        public void ShowTip(int tipValue, CustomerWaitState customerWaitState)
        {
            if (customerWaitState == CustomerWaitState.Happy)
            {
                m_TipFace.sprite = m_HappyFace;
            }
            else if (customerWaitState == CustomerWaitState.Angry)
            {
                m_TipFace.sprite = m_AngryFace;
            }
            if (customerWaitState == CustomerWaitState.Furious)
            {
                m_TipFace.sprite = m_FuriousFace;
            }

            m_TipFace.gameObject.SetActive(true);
            m_RevenueText.text = tipValue.ToString();
            m_RevenueText.gameObject.SetActive(true);

            m_OpeningSequence.PlaySequence(() =>
            {
                m_TipFace.gameObject.SetActive(false);
                m_RevenueText.gameObject.SetActive(false);
            });
        }

        public void ShowReveneu(int reveneuValue)
        {
            m_RevenueText.text = reveneuValue.ToString();
            m_RevenueText.gameObject.SetActive(true);

            m_OpeningSequence.PlaySequence(() =>
            {
                m_RevenueText.gameObject.SetActive(false);
            });
        }
    }

    
}
