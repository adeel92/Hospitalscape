using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NaughtyAttributes;
using Arc.Attribute;
using Isometric.Data;

namespace Isometric.UI
{
    public class CurrencyUIController : MonoBehaviour
    {

        [SerializeField] CurrencyType m_CurrencyType;
        [SerializeField] bool m_IsAutomaticUpdate;
        [SerializeField, ShowIf(nameof(m_IsAutomaticUpdate))]
        private bool m_UseCountUpdate;
        [SerializeField, ShowIf(EConditionOperator.And, nameof(m_IsAutomaticUpdate), nameof(m_UseCountUpdate))]
        private float m_CountUpdateDuration = 0.3f;
        [SerializeField] TextMeshProUGUI m_CurrencyText;
        [SerializeField] bool m_UseTimeUnscaledCount = true;
        [SerializeField] bool m_IsOnMenu = false;

        private Coroutine m_CurrencyCounting = null;
        private int m_CurrentCurrencyValue = 0;

        private bool m_HasCalledOnStart = false;

        private void OnEnable()
        {
            if (m_CurrencyType == CurrencyType.Coin)
            {
                GlobalEventHolder.OnCoinCurrencyUpdate += OnCurrencyUpdate;
            }
            else if (m_CurrencyType == CurrencyType.Gem)
            {
                GlobalEventHolder.OnGemCurrencyUpdate += OnCurrencyUpdate;
            }
            else if (m_CurrencyType == CurrencyType.Star)
            {
                GlobalEventHolder.OnStarCurrencyUpdate += OnCurrencyUpdate;
            }

            if (m_IsAutomaticUpdate && m_HasCalledOnStart)
            {
                UpdateCurrency();
            }
        }

        private void OnDisable()
        {
            GlobalEventHolder.OnCoinCurrencyUpdate -= OnCurrencyUpdate;
            GlobalEventHolder.OnGemCurrencyUpdate -= OnCurrencyUpdate;
            GlobalEventHolder.OnStarCurrencyUpdate -= OnCurrencyUpdate;
        }

        private void Start()
        {
            m_HasCalledOnStart = true;
            if (m_IsAutomaticUpdate)
            {
                UpdateCurrency();
            }
        }


        private void UpdateCurrency()
        {
            if (m_CurrencyType == CurrencyType.Coin)
            {
                m_CurrencyText.text = DataManager.CoinCurrency.ToString();
                m_CurrentCurrencyValue = DataManager.CoinCurrency;
            }
            else if (m_CurrencyType == CurrencyType.Gem)
            {
                m_CurrencyText.text = DataManager.GemCurrency.ToString();
                m_CurrentCurrencyValue = DataManager.GemCurrency;
            }
            if (m_CurrencyType == CurrencyType.Star)
            {
                m_CurrencyText.text = DataManager.StarCurrency.ToString();
                m_CurrentCurrencyValue = DataManager.StarCurrency;
            }
        }

        public void SetCurrencyValue(int currency)
        {
            m_CurrencyText.text = currency.ToString();
            m_CurrentCurrencyValue = currency;
        }

        private void OnCurrencyUpdate(int currency)
        {
            if (m_IsAutomaticUpdate)
            {
                if (m_UseCountUpdate)
                {
                    if (m_CurrencyCounting != null)
                    {
                        StopCoroutine(m_CurrencyCounting);
                        m_CurrencyCounting = null;
                    }

                    m_CurrencyCounting = StartCoroutine(CurrencyCounting(m_CurrentCurrencyValue, currency));
                    m_CurrentCurrencyValue = currency;
                }
                else
                {
                    m_CurrencyText.text = currency.ToString();
                    m_CurrentCurrencyValue = currency;
                }
            }
        }

        IEnumerator CurrencyCounting(int currentCurrencyValue, int targetCurrencyValue)
        {
            float lerpValue = 0;

            while (lerpValue < m_CountUpdateDuration)
            {
                m_CurrencyText.text = Mathf.RoundToInt(Mathf.Lerp(currentCurrencyValue, targetCurrencyValue, (lerpValue / m_CountUpdateDuration))).ToString();
                if (m_UseTimeUnscaledCount)
                {
                    lerpValue += Time.unscaledDeltaTime;
                }
                else
                {
                    lerpValue += Time.deltaTime;
                }
                yield return null;
            }

            m_CurrencyText.text = targetCurrencyValue.ToString();
        }

        public void OnPlusButton()
        {
            GlobalEventHolder.OnOpenShop?.Invoke(m_IsOnMenu);
        }
    }
}
