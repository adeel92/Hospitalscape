using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isometric.Customer
{
    [CreateAssetMenu(fileName = "CustomerCommonSetting", menuName = "Customer/CustomerCommonSetting")]
    public class CustomerCommonSetting : ScriptableObject
    {
        [Header("---Wait Related---")]
        public float WaitingDuration;

        [SerializeField]
        Color m_DeafultColor = Color.green;

        [SerializeField, Range(1, 100)]
        float m_FirstWaitOverBelowPercentage = 50f;
        [SerializeField]
        Color m_FirstWaitOverColor = Color.yellow;

        [SerializeField, Range(1, 100)]
        float m_SecondWaitOverBelowPercentage = 25f;
        [SerializeField]
        Color m_SecondWaitOverColor = Color.red;

        [Header("---Base Revenu---")]
        [SerializeField] private int m_BaseRevenue;


        [Header("---Tip---")]
        [SerializeField] private int m_HappyTip;
        [SerializeField] private int m_AngryTip;
        [SerializeField] private int m_FuriousTip;

        [Header("---VIP---")]
        [SerializeField] private int m_PerOrderTip;
        [SerializeField] private int m_LastTip;

        public bool IsFirstWaitOver(float currentWait, float totalWait)
        {
            float firstWait = m_FirstWaitOverBelowPercentage / 100;
            return (currentWait / totalWait) < firstWait;
        }

        public Color DefaultColor()
        {
            return m_DeafultColor;
        }

        public Color FirstWaitColor()
        {
            return m_FirstWaitOverColor;
        }

        public bool IsSecondWaitOver(float currentWait, float totalWait)
        {
            float secondWait = m_SecondWaitOverBelowPercentage / 100;
            return (currentWait / totalWait) < secondWait;
        }

        public Color SecondWaitColor()
        {
            return m_SecondWaitOverColor;
        }

        public int GetBaseRevenue()
        {
            return m_BaseRevenue;
        }

        public int GetTip(CustomerWaitState customerWaitState)
        {
            if (customerWaitState == CustomerWaitState.Happy)
            {
                return m_HappyTip;
            }
            else if (customerWaitState == CustomerWaitState.Angry)
            {
                return m_AngryTip;
            }
            else
            {
                return m_FuriousTip;
            }
        }

        public int GetPerOrderTipVIP()
        {
            return m_PerOrderTip;
        }

        public int GetLastOrderTipVIP()
        {
            return m_LastTip;
        }

    }
}