using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

namespace Isometric.Data
{
    /// <summary>
    /// Consumable can be used as an order
    /// </summary>
    [CreateAssetMenu(fileName = "DataConsumable", menuName = "GameData/DataConsumable")]
    public class DataConsumable : DataKeyed
    {
        public GameObject ConsumableTrayPrefab => m_ConsumableTrayPrefab;
        [SerializeField] GameObject m_ConsumableTrayPrefab;

        public GameObject ConsumableOrderPrefab => m_ConsumableOrderPrefab;
        [SerializeField] GameObject m_ConsumableOrderPrefab;

        public bool IsThrowable => m_IsThrowable;
        [Tooltip("Whether the item can be thrown inside trash or not")]
        [Space(5), SerializeField] bool m_IsThrowable = true;
    }
}
