using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

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
    }
}
