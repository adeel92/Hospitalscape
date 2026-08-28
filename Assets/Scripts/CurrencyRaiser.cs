using UnityEngine;
using Isometric.Data;
using Isometric;
using System.Collections.Generic;
using TMPro;


public class CurrencyRaiser : MonoBehaviour
{
    [SerializeField] CurrencyType m_CurrencyType = CurrencyType.Coin;
    [SerializeField] int m_Amount = 500;
    [SerializeField] List<TextMeshProUGUI> m_AllTexts = new();


    public void UpdateCurrency()
    {
        int updatedValue = 0;

        switch (m_CurrencyType)
        {
            case CurrencyType.Coin:
            DataManager.CoinCurrency += m_Amount;
            updatedValue = DataManager.CoinCurrency;
            break;

            case CurrencyType.Gem:
            DataManager.GemCurrency += m_Amount;
            updatedValue = DataManager.GemCurrency;
            break;
        }

        DataManager.SaveData();

        foreach(TextMeshProUGUI textMeshPro in m_AllTexts)
        {
            textMeshPro.text = updatedValue.ToString();
        }
    }
}
