using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Isometric.Data;

namespace Isometric.Environment
{
    public class SalonChairOrderUIController : MonoBehaviour
    {
        [Serializable]
        private class OrderHolderInfo
        {
            public int Quantity;
            public GameObject Holder;
            public List<Transform> HoldingTransfroms;
        }


        [SerializeField] List<OrderHolderInfo> m_OrderHoldersInfo;

        //Returns the positions of the UI orders
        public List<Vector3> SetOrders(List<DataConsumable> ordersConsumable)
        {
            OrderHolderInfo orderHolderInfo = m_OrderHoldersInfo.Find((x) => x.Quantity == ordersConsumable.Count);
            if (orderHolderInfo != null)
            {
                List<Vector3> positions = new List<Vector3>();

                for (int i = 0; i < ordersConsumable.Count; i++)
                {
                    GameObject order = Instantiate(ordersConsumable[i].ConsumableOrderPrefab, orderHolderInfo.HoldingTransfroms[i]);
                    if(order.TryGetComponent(out StationVisualUpgradeApplier stationVisualUpgradeApplier))
                    {
                        stationVisualUpgradeApplier.ApplyVisualUpgrade();
                    }
                    order.transform.localPosition = Vector3.zero;

                    positions.Add(order.transform.position);
                }

                orderHolderInfo.Holder.SetActive(true);

                return positions;
            }

            return null;
        }

        public void CleanPreviousOrders()
        {
            foreach (var holderInfo in m_OrderHoldersInfo)
            {
                foreach (var holdingTransform in holderInfo.HoldingTransfroms)
                {
                    for (int i = holdingTransform.childCount - 1; i >= 0; i--)
                    {
                        Destroy(holdingTransform.GetChild(i).gameObject);
                    }
                }

                holderInfo.Holder.SetActive(false);
            }
        }
    }
}
