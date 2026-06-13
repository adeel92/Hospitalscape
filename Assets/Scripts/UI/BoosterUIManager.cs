using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.Customer;

namespace Isometric.UI
{
    public class BoosterUIManager : UIPopupBase
    {
        [Serializable]
        private class BoosterButtonInfo
        {
            public BoosterType BoosterType;
            public GameObject Holder;
            public Button ActivationButton;
            public Image Fill;
            public UnityEvent OnFillStart;
            public UnityEvent OnFillEnd;
            public TextMeshProUGUI CountText;
            [AllowNesting, ReadOnly, HideIf(nameof(BoosterType), BoosterType.InstanceOrderFill)]
            public float AppliedDuration;
        }

        [Serializable]
        private class InstanceOrderFillInfo
        {
            public DataConsumable OrderType;
            public GameObject CollectionPrefab;
            public DataStation DataStation;
            [AllowNesting, ReadOnly]
            public int OrderCost;
        }

        [SerializeField] DataMapUpdate m_DataMapUpdate;
        [SerializeField] GameObject m_Popup;
        [SerializeField] List<BoosterButtonInfo> m_BoostersButtonInfo;
        [SerializeField] Transform m_InstanceOrderStartPosition;
        [SerializeField] float m_InstanceOrderFillDuration;
        [SerializeField] Vector3 m_InstanceOrderEndPositionOffest;
        [SerializeField] List<InstanceOrderFillInfo> m_InstanceOrderFillInfos;


        public override void Setup()
        {
            int level = DataManager.CurrentMapLevelIndex + 1;
            List<BoosterInfo> boostersInfo = m_DataMapUpdate.GetBoostersInfo();

            foreach (var boostersButtonInfo in m_BoostersButtonInfo)
            {
                BoosterInfo temBoosterInfo = boostersInfo.Find((x) => x.BoosterType == boostersButtonInfo.BoosterType);
                if (temBoosterInfo != null && level >= temBoosterInfo.UnlockAtLevel)
                {
                    boostersButtonInfo.Holder.SetActive(true);
                    boostersButtonInfo.Fill.gameObject.SetActive(false);
                    boostersButtonInfo.AppliedDuration = temBoosterInfo.AppliedDuration;
                    if (boostersButtonInfo.BoosterType == BoosterType.TimeFroze)
                    {
                        boostersButtonInfo.CountText.text = DataManager.TimeFrozeBoosterCount.ToString();
                        boostersButtonInfo.ActivationButton.onClick.RemoveAllListeners();
                        boostersButtonInfo.ActivationButton.onClick.AddListener(() => OnBoosterActivation(boostersButtonInfo));
                    }
                    else if (boostersButtonInfo.BoosterType == BoosterType.WaitressSpeed)
                    {
                        boostersButtonInfo.CountText.text = DataManager.WaitressSpeedBoosterCount.ToString();
                        boostersButtonInfo.ActivationButton.onClick.RemoveAllListeners();
                        boostersButtonInfo.ActivationButton.onClick.AddListener(() => OnBoosterActivation(boostersButtonInfo));
                    }
                    else if (boostersButtonInfo.BoosterType == BoosterType.InstanceOrderFill)
                    {
                        boostersButtonInfo.CountText.text = DataManager.InstanceOrderFillBoosterCount.ToString();
                        boostersButtonInfo.ActivationButton.onClick.RemoveAllListeners();
                        boostersButtonInfo.ActivationButton.onClick.AddListener(() => OnBoosterActivation(boostersButtonInfo));
                    }
                }
                else
                {
                    boostersButtonInfo.Holder.SetActive(false);
                }
            }

            foreach (var instanceOrderFillInfo in m_InstanceOrderFillInfos)
            {
                StationData stationData = instanceOrderFillInfo.DataStation.StationData;
                if (stationData != null)
                {
                    StationUpgrade stationUpgrade = stationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
                    if (stationUpgrade != null && stationUpgrade.CurrentUpgradeIndex < stationUpgrade.Upgrade.Count)
                    {
                        int cost = Mathf.RoundToInt(stationUpgrade.Upgrade[stationUpgrade.CurrentUpgradeIndex]);

                        instanceOrderFillInfo.OrderCost = cost;
                    }
                    else
                    {
                        instanceOrderFillInfo.OrderCost = 4;
                    }
                }
                else
                {
                    instanceOrderFillInfo.OrderCost = 4;
                }
            }
        }

        public override void OpenPopup(Action onComplete)
        {
            Setup();
            m_Popup.SetActive(true);
            onComplete?.Invoke();
        }

        public override void ClosePopup(Action onComplete)
        {
            m_Popup.SetActive(false);
            onComplete?.Invoke();
        }

        private void OnBoosterActivation(BoosterButtonInfo boosterButtonInfo)
        {
            if (boosterButtonInfo.BoosterType == BoosterType.TimeFroze)
            {
                if (DataManager.TimeFrozeBoosterCount > 0)
                {
                    DataManager.TimeFrozeBoosterCount--;
                    DataManager.SaveData();

                    boosterButtonInfo.CountText.text = DataManager.TimeFrozeBoosterCount.ToString();
                    boosterButtonInfo.ActivationButton.enabled = false;
                    boosterButtonInfo.Fill.fillAmount = 1;
                    boosterButtonInfo.Fill.gameObject.SetActive(true);
                    boosterButtonInfo.OnFillStart?.Invoke();

                    GlobalEventHolder.OnTimeFrozeBooster?.Invoke(true);

                    StartCoroutine(BoosterActivationTime(boosterButtonInfo));
                }
            }
            else if (boosterButtonInfo.BoosterType == BoosterType.WaitressSpeed)
            {
                if (DataManager.WaitressSpeedBoosterCount > 0)
                {
                    DataManager.WaitressSpeedBoosterCount--;
                    DataManager.SaveData();

                    boosterButtonInfo.CountText.text = DataManager.WaitressSpeedBoosterCount.ToString();
                    boosterButtonInfo.ActivationButton.enabled = false;
                    boosterButtonInfo.Fill.fillAmount = 1;
                    boosterButtonInfo.Fill.gameObject.SetActive(true);
                    boosterButtonInfo.OnFillStart?.Invoke();

                    GlobalEventHolder.OnWaitressSpeedBooster?.Invoke(true);

                    StartCoroutine(BoosterActivationTime(boosterButtonInfo));
                }
            }
            else if (boosterButtonInfo.BoosterType == BoosterType.InstanceOrderFill)
            {
                List<Tuple<CustomerSalonController, List<Tuple<DataConsumable, Vector3>>>> customersOrderInfo = CustomerManager.GetCurrentWaitressOrders();
                if (customersOrderInfo != null && customersOrderInfo.Count > 0)
                {
                    if (DataManager.InstanceOrderFillBoosterCount > 0)
                    {
                        DataManager.InstanceOrderFillBoosterCount--;
                        DataManager.SaveData();

                        boosterButtonInfo.CountText.text = DataManager.InstanceOrderFillBoosterCount.ToString();


                        foreach (var customerOrderInfo in customersOrderInfo)
                        {
                            if (customerOrderInfo.Item1 != null)
                            {
                                List<Tuple<GameObject, Vector3>> collectionOrdersInfo = new List<Tuple<GameObject, Vector3>>();
                                List<Tuple<DataConsumable, int>> ordersCompletionInfo = new List<Tuple<DataConsumable, int>>();

                                foreach (var order in customerOrderInfo.Item2)
                                {
                                    InstanceOrderFillInfo temInstanceOrderFillInfo = m_InstanceOrderFillInfos.Find((x) => x.OrderType == order.Item1);
                                    if (temInstanceOrderFillInfo != null)
                                    {
                                        Vector3 finalPosition = order.Item2 + m_InstanceOrderEndPositionOffest;

                                        Tuple<GameObject, Vector3> collectionOrderInfo =
                                            new Tuple<GameObject, Vector3>(temInstanceOrderFillInfo.CollectionPrefab, finalPosition);

                                        Tuple<DataConsumable, int> orderCompletionInfo =
                                            new Tuple<DataConsumable, int>(temInstanceOrderFillInfo.OrderType, temInstanceOrderFillInfo.OrderCost);

                                        collectionOrdersInfo.Add(collectionOrderInfo);
                                        ordersCompletionInfo.Add(orderCompletionInfo);
                                    }
                                }

                                if (collectionOrdersInfo != null)
                                {
                                    customerOrderInfo.Item1.LockWaitressOrders();
                                    CollectionUIManager.CollectLinearMultiple(m_InstanceOrderFillDuration, 
                                        collectionOrdersInfo,
                                        m_InstanceOrderStartPosition.position,
                                        null, 
                                        () =>
                                        {
                                            customerOrderInfo.Item1.FillCurrentWaitressOrders(ordersCompletionInfo);
                                            customerOrderInfo.Item1.UnlockWaitressOrders();
                                        });
                                }
                            }
                        }
                    }
                }
            }

            IEnumerator BoosterActivationTime(BoosterButtonInfo boosterButtonInfo)
            {
                if (boosterButtonInfo.BoosterType == BoosterType.TimeFroze)
                {
                    float duration = boosterButtonInfo.AppliedDuration;
                    float counter = duration;

                    while (counter > 0)
                    {
                        boosterButtonInfo.Fill.fillAmount = (counter / duration);
                        counter -= Time.deltaTime;
                        yield return null;
                    }

                    boosterButtonInfo.Fill.gameObject.SetActive(false);
                    boosterButtonInfo.ActivationButton.enabled = true;
                    boosterButtonInfo.OnFillEnd?.Invoke();

                    GlobalEventHolder.OnTimeFrozeBooster?.Invoke(false);
                }
                else if (boosterButtonInfo.BoosterType == BoosterType.WaitressSpeed)
                {
                    float duration = boosterButtonInfo.AppliedDuration;
                    float counter = duration;

                    while (counter > 0)
                    {
                        boosterButtonInfo.Fill.fillAmount = (counter / duration);
                        counter -= Time.deltaTime;
                        yield return null;
                    }

                    boosterButtonInfo.Fill.gameObject.SetActive(false);
                    boosterButtonInfo.ActivationButton.enabled = true;
                    boosterButtonInfo.OnFillEnd?.Invoke();
                    
                    GlobalEventHolder.OnWaitressSpeedBooster?.Invoke(false);
                }

                yield return null;
            }
        }
    }
}
