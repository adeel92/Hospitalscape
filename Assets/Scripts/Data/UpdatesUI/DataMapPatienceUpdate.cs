using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyAttributes;
using Arc.Attribute;
using Isometric.UI;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataMapPatienceUpdate", menuName = "GameData/DataMapPatienceUpdate")]
    public class DataMapPatienceUpdate : ScriptableObject
    {
        [SerializeField] List<PatienceUpdateInfo> m_PatiencesUpdateInfo;

        public List<PatienceUpgradePanelUI> GetAndSetPatienceUpgradePanels(Transform holder)
        {
            List<PatienceUpgradePanelUI> patienceUpgradePanelsUI = new List<PatienceUpgradePanelUI>();

            int levelIndex = DataManager.CurrentMapLevelIndex;

            foreach (var patienceUpdateInfo in m_PatiencesUpdateInfo)
            {
                PatienceData data = patienceUpdateInfo.Data.PatienceData;
                if (patienceUpdateInfo.ShowOnAndAfterLevel <= (levelIndex + 1))
                {
                    PatienceUpgradePanelUI patienceUpgradePanelUI = Instantiate(patienceUpdateInfo.PatienceUpgradePanelUI, holder);
                    patienceUpgradePanelUI.Setup(patienceUpdateInfo.Data);
                    patienceUpgradePanelsUI.Add(patienceUpgradePanelUI);

                    patienceUpgradePanelUI.HeaderText.text = patienceUpdateInfo.HeaderText;
                    patienceUpgradePanelUI.PreviewImage.sprite = patienceUpdateInfo.PreviewSprite;

                    patienceUpgradePanelUI.UpgradeNameText.text = patienceUpdateInfo.NameText;

                    patienceUpgradePanelUI.InfoPreviewSprite = patienceUpdateInfo.PreviewSprite;
                    patienceUpgradePanelUI.InfoDiscriptionText = patienceUpdateInfo.InfoDiscriptionText;

                    patienceUpgradePanelUI.NotifiationSeenKey = patienceUpdateInfo.NotifationSeenKey;

                    patienceUpgradePanelUI.UpgradeNameText.gameObject.SetActive(false);
                    patienceUpgradePanelUI.UpgradeValueText.gameObject.SetActive(false);
                    patienceUpgradePanelUI.UpgradeButton.gameObject.SetActive(false);
                    patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                    patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                    patienceUpgradePanelUI.MaxButton.gameObject.SetActive(false);
                    patienceUpgradePanelUI.Tick.SetActive(false);

                    int numberOfUpgrades = data.PatienceUpgrade.Count;

                    patienceUpgradePanelUI.StepFillBar.SetupBars(numberOfUpgrades);

                    if (data.IsUnlocked == true)
                    {
                        int index = data.CurrentUpgradeIndex;
                        if (index >= data.PatienceUpgrade.Count - 1)
                        {
                            patienceUpgradePanelUI.StepFillBar.SetFillAmount(numberOfUpgrades);
                            patienceUpgradePanelUI.Tick.SetActive(true);
                            patienceUpgradePanelUI.MaxButton.gameObject.SetActive(true);
                        }
                        else
                        {
                            patienceUpgradePanelUI.StepFillBar.SetFillAmount(1 + index);
                            patienceUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);


                            if (patienceUpdateInfo.IsChairPatience)
                            {

                                if (index == 0)
                                {
                                    float value = data.PatienceUpgrade[data.CurrentUpgradeIndex + 1];
                                    patienceUpgradePanelUI.UpgradeValueText.text = Mathf.RoundToInt((value * 100)) + "%";
                                }
                                else
                                {
                                    float value1 = data.PatienceUpgrade[data.CurrentUpgradeIndex];
                                    float value2 = data.PatienceUpgrade[data.CurrentUpgradeIndex + 1];
                                    patienceUpgradePanelUI.UpgradeValueText.text = Mathf.RoundToInt((value1 * 100)) + "% -> " + Mathf.RoundToInt((value2 * 100)) + "%";
                                }
                            }
                            else
                            {
                                if (index == 0)
                                {
                                    float value = data.PatienceUpgrade[data.CurrentUpgradeIndex + 1];
                                    patienceUpgradePanelUI.UpgradeValueText.text = "+" + value + "s";
                                }
                                else
                                {
                                    float value1 = data.PatienceUpgrade[data.CurrentUpgradeIndex];
                                    float value2 = data.PatienceUpgrade[data.CurrentUpgradeIndex + 1];
                                    patienceUpgradePanelUI.UpgradeValueText.text = "+" + value1 + "s -> +" + value2 + "s";
                                }
                            }


                            PatiencePriceUpdateInfo patiencePriceUpdateInfo = patienceUpdateInfo.PatiencePriceUpdatesInfo[data.CurrentUpgradeIndex + 1];
                            if (patiencePriceUpdateInfo.UpdateType == CurrencyType.Coin)
                            {
                                patienceUpgradePanelUI.UpgradeButtonText.text = patiencePriceUpdateInfo.RequiredCoins.ToString();
                                patienceUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                                patienceUpgradePanelUI.UpgradeCost = patiencePriceUpdateInfo.RequiredCoins;
                                patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                                patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);

                                if (patiencePriceUpdateInfo.RequiredCoins == 0)
                                {
                                    patienceUpgradePanelUI.UpgradeButtonText.text = "Free";
                                    Vector2 position = patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition;
                                    position.x = 0;
                                    patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition = position;
                                    patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                                    patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                                }
                            }
                            else if (patiencePriceUpdateInfo.UpdateType == CurrencyType.Gem)
                            {
                                patienceUpgradePanelUI.UpgradeButtonText.text = patiencePriceUpdateInfo.RequiredGems.ToString();
                                patienceUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                                patienceUpgradePanelUI.UpgradeCost = patiencePriceUpdateInfo.RequiredGems;
                                patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                                patienceUpgradePanelUI.UpgradeButtonGem.SetActive(true);

                                if (patiencePriceUpdateInfo.RequiredGems == 0)
                                {
                                    patienceUpgradePanelUI.UpgradeButtonText.text = "Free";
                                    Vector2 position = patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition;
                                    position.x = 0;
                                    patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition = position;
                                    patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                                    patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                                }
                            }

                            patienceUpgradePanelUI.UpgradeNameText.gameObject.SetActive(true);
                            patienceUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        patienceUpgradePanelUI.StepFillBar.SetFillAmount(0);
                        patienceUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);

                        if (patienceUpdateInfo.IsChairPatience)
                        {

                            float value = data.PatienceUpgrade[0];
                            patienceUpgradePanelUI.UpgradeValueText.text = Mathf.RoundToInt((value * 100)) + "%";
                        }
                        else
                        {

                            float value = data.PatienceUpgrade[0];
                            patienceUpgradePanelUI.UpgradeValueText.text = "+" + value + "s";
                        }

                        PatiencePriceUpdateInfo patiencePriceUpdateInfo = patienceUpdateInfo.PatiencePriceUpdatesInfo[0];
                        if (patiencePriceUpdateInfo.UpdateType == CurrencyType.Coin)
                        {
                            patienceUpgradePanelUI.UpgradeButtonText.text = patiencePriceUpdateInfo.RequiredCoins.ToString();
                            patienceUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                            patienceUpgradePanelUI.UpgradeCost = patiencePriceUpdateInfo.RequiredCoins;
                            patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                            patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);

                            if (patiencePriceUpdateInfo.RequiredCoins == 0)
                            {
                                patienceUpgradePanelUI.UpgradeButtonText.text = "Free";
                                Vector2 position = patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition;
                                position.x = 0;
                                patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition = position;
                                patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                                patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                            }
                        }
                        else if (patiencePriceUpdateInfo.UpdateType == CurrencyType.Gem)
                        {
                            patienceUpgradePanelUI.UpgradeButtonText.text = patiencePriceUpdateInfo.RequiredGems.ToString();
                            patienceUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                            patienceUpgradePanelUI.UpgradeCost = patiencePriceUpdateInfo.RequiredGems;
                            patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                            patienceUpgradePanelUI.UpgradeButtonGem.SetActive(true);

                            if (patiencePriceUpdateInfo.RequiredGems == 0)
                            {
                                patienceUpgradePanelUI.UpgradeButtonText.text = "Free";
                                Vector2 position = patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition;
                                position.x = 0;
                                patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition = position;
                                patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                                patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                            }
                        }

                        patienceUpgradePanelUI.UpgradeNameText.gameObject.SetActive(true);
                        patienceUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                    }
                }
            }


            return patienceUpgradePanelsUI;
        }

        public PatienceUpgradePanelUI GetAndSetPatienceUpgradePanel(DataPatience dataPatience, Transform holder)
        {
            PatienceUpgradePanelUI patienceUpgradePanelUI = null;

            int levelIndex = DataManager.CurrentMapLevelIndex;

            PatienceUpdateInfo patienceUpdateInfo  = m_PatiencesUpdateInfo.Find((x) => x.Data == dataPatience);

            PatienceData data = patienceUpdateInfo.Data.PatienceData;
            if (patienceUpdateInfo.ShowOnAndAfterLevel <= (levelIndex + 1))
            {
                patienceUpgradePanelUI = Instantiate(patienceUpdateInfo.PatienceUpgradePanelUI, holder);
                patienceUpgradePanelUI.Setup(patienceUpdateInfo.Data);

                patienceUpgradePanelUI.HeaderText.text = patienceUpdateInfo.HeaderText;
                patienceUpgradePanelUI.PreviewImage.sprite = patienceUpdateInfo.PreviewSprite;

                patienceUpgradePanelUI.UpgradeNameText.text = patienceUpdateInfo.NameText;

                patienceUpgradePanelUI.InfoPreviewSprite = patienceUpdateInfo.PreviewSprite;
                patienceUpgradePanelUI.InfoDiscriptionText = patienceUpdateInfo.InfoDiscriptionText;

                patienceUpgradePanelUI.NotifiationSeenKey = patienceUpdateInfo.NotifationSeenKey;

                patienceUpgradePanelUI.UpgradeNameText.gameObject.SetActive(false);
                patienceUpgradePanelUI.UpgradeValueText.gameObject.SetActive(false);
                patienceUpgradePanelUI.UpgradeButton.gameObject.SetActive(false);
                patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                patienceUpgradePanelUI.MaxButton.gameObject.SetActive(false);
                patienceUpgradePanelUI.Tick.SetActive(false);

                int numberOfUpgrades = data.PatienceUpgrade.Count;

                patienceUpgradePanelUI.StepFillBar.SetupBars(numberOfUpgrades);

                if (data.IsUnlocked == true)
                {
                    int index = data.CurrentUpgradeIndex;
                    if (index >= data.PatienceUpgrade.Count - 1)
                    {
                        patienceUpgradePanelUI.StepFillBar.SetFillAmount(numberOfUpgrades);
                        patienceUpgradePanelUI.Tick.SetActive(true);
                        patienceUpgradePanelUI.MaxButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        patienceUpgradePanelUI.StepFillBar.SetFillAmount(1 + index);
                        patienceUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);

                        if (patienceUpdateInfo.IsChairPatience)
                        {

                            if (index == 0)
                            {
                                float value = data.PatienceUpgrade[data.CurrentUpgradeIndex + 1];
                                patienceUpgradePanelUI.UpgradeValueText.text = Mathf.RoundToInt((value * 100)) + "%";
                            }
                            else
                            {
                                float value1 = data.PatienceUpgrade[data.CurrentUpgradeIndex];
                                float value2 = data.PatienceUpgrade[data.CurrentUpgradeIndex + 1];
                                patienceUpgradePanelUI.UpgradeValueText.text = Mathf.RoundToInt((value1 * 100)) + "% -> " + Mathf.RoundToInt((value2 * 100)) + "%";
                            }
                        }
                        else
                        {
                            if (index == 0)
                            {
                                float value = data.PatienceUpgrade[data.CurrentUpgradeIndex + 1];
                                patienceUpgradePanelUI.UpgradeValueText.text = "+" + value + "s";
                            }
                            else
                            {
                                float value1 = data.PatienceUpgrade[data.CurrentUpgradeIndex];
                                float value2 = data.PatienceUpgrade[data.CurrentUpgradeIndex + 1];
                                patienceUpgradePanelUI.UpgradeValueText.text = "+" + value1 + "s -> +" + value2 + "s";
                            }
                        }


                        PatiencePriceUpdateInfo patiencePriceUpdateInfo = patienceUpdateInfo.PatiencePriceUpdatesInfo[data.CurrentUpgradeIndex + 1];
                        if (patiencePriceUpdateInfo.UpdateType == CurrencyType.Coin)
                        {
                            patienceUpgradePanelUI.UpgradeButtonText.text = patiencePriceUpdateInfo.RequiredCoins.ToString();
                            patienceUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                            patienceUpgradePanelUI.UpgradeCost = patiencePriceUpdateInfo.RequiredCoins;
                            patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                            patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);

                            if (patiencePriceUpdateInfo.RequiredCoins == 0)
                            {
                                patienceUpgradePanelUI.UpgradeButtonText.text = "Free";
                                Vector2 position = patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition;
                                position.x = 0;
                                patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition = position;
                                patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                                patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                            }
                        }
                        else if (patiencePriceUpdateInfo.UpdateType == CurrencyType.Gem)
                        {
                            patienceUpgradePanelUI.UpgradeButtonText.text = patiencePriceUpdateInfo.RequiredGems.ToString();
                            patienceUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                            patienceUpgradePanelUI.UpgradeCost = patiencePriceUpdateInfo.RequiredGems;
                            patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                            patienceUpgradePanelUI.UpgradeButtonGem.SetActive(true);

                            if (patiencePriceUpdateInfo.RequiredGems == 0)
                            {
                                patienceUpgradePanelUI.UpgradeButtonText.text = "Free";
                                Vector2 position = patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition;
                                position.x = 0;
                                patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition = position;
                                patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                                patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                            }
                        }

                        patienceUpgradePanelUI.UpgradeNameText.gameObject.SetActive(true);
                        patienceUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                    }
                }
                else
                {
                    patienceUpgradePanelUI.StepFillBar.SetFillAmount(0);
                    patienceUpgradePanelUI.UpgradeValueText.gameObject.SetActive(true);

                    if (patienceUpdateInfo.IsChairPatience)
                    {

                        float value = data.PatienceUpgrade[0];
                        patienceUpgradePanelUI.UpgradeValueText.text = Mathf.RoundToInt((value * 100)) + "%";
                    }
                    else
                    {

                        float value = data.PatienceUpgrade[0];
                        patienceUpgradePanelUI.UpgradeValueText.text = "+" + value + "s";
                    }

                    PatiencePriceUpdateInfo patiencePriceUpdateInfo = patienceUpdateInfo.PatiencePriceUpdatesInfo[0];
                    if (patiencePriceUpdateInfo.UpdateType == CurrencyType.Coin)
                    {
                        patienceUpgradePanelUI.UpgradeButtonText.text = patiencePriceUpdateInfo.RequiredCoins.ToString();
                        patienceUpgradePanelUI.UpgradeCurrency = CurrencyType.Coin;
                        patienceUpgradePanelUI.UpgradeCost = patiencePriceUpdateInfo.RequiredCoins;
                        patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(true);
                        patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);

                        if (patiencePriceUpdateInfo.RequiredCoins == 0)
                        {
                            patienceUpgradePanelUI.UpgradeButtonText.text = "Free";
                            Vector2 position = patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition;
                            position.x = 0;
                            patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition = position;
                            patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                            patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                        }
                    }
                    else if (patiencePriceUpdateInfo.UpdateType == CurrencyType.Gem)
                    {
                        patienceUpgradePanelUI.UpgradeButtonText.text = patiencePriceUpdateInfo.RequiredGems.ToString();
                        patienceUpgradePanelUI.UpgradeCurrency = CurrencyType.Gem;
                        patienceUpgradePanelUI.UpgradeCost = patiencePriceUpdateInfo.RequiredGems;
                        patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                        patienceUpgradePanelUI.UpgradeButtonGem.SetActive(true);

                        if (patiencePriceUpdateInfo.RequiredGems == 0)
                        {
                            patienceUpgradePanelUI.UpgradeButtonText.text = "Free";
                            Vector2 position = patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition;
                            position.x = 0;
                            patienceUpgradePanelUI.UpgradeButtonText.rectTransform.anchoredPosition = position;
                            patienceUpgradePanelUI.UpgradeButtonCoin.SetActive(false);
                            patienceUpgradePanelUI.UpgradeButtonGem.SetActive(false);
                        }
                    }

                    patienceUpgradePanelUI.UpgradeNameText.gameObject.SetActive(true);
                    patienceUpgradePanelUI.UpgradeButton.gameObject.SetActive(true);
                }
            }


            return patienceUpgradePanelUI;
        }

        public PatienceUpgradePanelUI UpgradePatienceUpgradePanel(DataPatience dataPatience, PatienceUpgradePanelUI patienceUpgradePanelUI)
        {
            foreach (var paticneUpdateInfo in m_PatiencesUpdateInfo)
            {
                if (paticneUpdateInfo.Data == dataPatience)
                {
                    if (dataPatience.PatienceData.IsUnlocked)
                    {
                        PatienceData patienceData = paticneUpdateInfo.Data.PatienceData;
                        int increase = patienceData.CurrentUpgradeIndex >= patienceData.PatienceUpgrade.Count - 1 ? 0 : 1;
                        patienceData.CurrentUpgradeIndex += increase;
                        patienceData.HasUpgraded = true;
                    }
                    else
                    {
                        dataPatience.PatienceData.IsUnlocked = true;
                        dataPatience.PatienceData.HasJustUnlocked = true;
                    }
                    paticneUpdateInfo.Data.Save();

                    Transform parent = patienceUpgradePanelUI.transform.parent;
                    int hierarchyIndex = patienceUpgradePanelUI.transform.GetSiblingIndex();

                    Destroy(patienceUpgradePanelUI.gameObject);

                    PatienceUpgradePanelUI newPatienceUpgradePanelUI = GetAndSetPatienceUpgradePanel(dataPatience, parent);
                    newPatienceUpgradePanelUI.transform.SetSiblingIndex(hierarchyIndex);

                    return newPatienceUpgradePanelUI;
                }
            }

            return patienceUpgradePanelUI;
        }

    }

    [Serializable]
    public class PatienceUpdateInfo
    {
        public DataPatience Data;
        public PatienceUpgradePanelUI PatienceUpgradePanelUI;
        public bool IsChairPatience;
        public int ShowOnAndAfterLevel;
        public string HeaderText;
        public Sprite PreviewSprite;
        public string NameText;
        [TextArea]
        public string InfoDiscriptionText;

        [Space]
        public string NotifationSeenKey;

        [Space]
        public List<PatiencePriceUpdateInfo> PatiencePriceUpdatesInfo;
    }

    [Serializable]
    public class PatiencePriceUpdateInfo
    {
        [Header("-First Index price is for unlocking")]
        [EnumFilter((int)CurrencyType.Coin, (int)CurrencyType.Gem)]
        public CurrencyType UpdateType = CurrencyType.Coin;
        [AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Coin)]
        public int RequiredCoins;
        [AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Gem)]
        public int RequiredGems;
    }
}
