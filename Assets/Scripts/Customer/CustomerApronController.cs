using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Arc;
using Isometric.Data;

namespace Isometric.Customer
{
    public class CustomerApronController : MonoBehaviour
    {
        [Serializable]
        private class OrderSymbolInfo
        {
            public DataConsumable Order;
            public UnityEvent OnMatch;
        }

        [SerializeField] List<OrderSymbolInfo> m_OrdersSymbolInfo;
        [SerializeField] SpriteGroup m_SpriteGroupApron;
        [SerializeField] float m_FadeDuration;
        [SerializeField] GameObject m_Apron;


        public void ShowApron(DataConsumable orderType)
        {
            OrderSymbolInfo symbol = m_OrdersSymbolInfo.Find((x) => x.Order == orderType);
            if(symbol != null) symbol.OnMatch?.Invoke();

            m_Apron.SetActive(true);

            DOTween.To(() => m_SpriteGroupApron.GetAlpha(), x => m_SpriteGroupApron.SetAlpha(x), 1f, m_FadeDuration);
        }

        public void HideApron()
        {
            m_Apron.SetActive(false);
        }
    }
}
