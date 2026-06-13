using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;

namespace Isometric.Player
{
    public class PlayerTrayHandler : MonoBehaviour
    {
        [Serializable]
        private class TrayInfo
        {
            [Serializable]
            public class TryHolder
            {
                [ReadOnly, AllowNesting]
                public string Key = "";
                [ReadOnly, AllowNesting]
                public GameObject HoldingItem;
                [ReadOnly, AllowNesting]
                public int HoldingItemCost;

                public int SortingOrder;
                public Transform Hold;
            }

            public GameObject Tray;
            public List<TryHolder> TrayHolder;
        }

        [SerializeField] List<TrayInfo> m_TraysInfo;
        private TrayInfo m_CurrentTray = null;

        /// <summary>
        /// Set the tray's capacity
        /// </summary>
        public void Setup(int capacity)
        {
            if (m_TraysInfo != null)
            {
                foreach (var tray in m_TraysInfo)
                {
                    if (tray.TrayHolder.Count == capacity)
                    {
                        tray.Tray.SetActive(true);
                        m_CurrentTray = tray;
                        foreach (var hold in m_CurrentTray.TrayHolder)
                        {
                            hold.Key = "";
                            hold.HoldingItemCost = 0;
                            hold.HoldingItem = null;
                        }
                    }
                    else
                    {
                        tray.Tray.SetActive(false);
                    }
                }
            }
        }

        public bool HasCapacity()
        {
            if (m_CurrentTray != null)
            {
                return m_CurrentTray.TrayHolder.Find((x) => x.Key == "") != null;
            }
            else
            {
                return false;
            }
        }

        public void AddItem(string key, GameObject item, int itemCost) 
        {
            if (m_CurrentTray != null)
            {
                foreach (var holder in m_CurrentTray.TrayHolder)
                {
                    if (holder.Key == "")
                    {
                        holder.Key = key;
                        holder.HoldingItemCost = itemCost;
                        foreach (var spriterRendereer in item.GetComponentsInChildren<SpriteRenderer>())
                        {
                            spriterRendereer.sortingOrder = holder.SortingOrder;
                        }
                        holder.HoldingItem = item;
                        item.transform.parent = holder.Hold;
                        item.transform.localScale = Vector3.one;
                        item.transform.localPosition = Vector3.zero;
                        break;
                    }
                }
            }
        }
        
        public bool HasItem(string key)
        {
            if (m_CurrentTray != null)
            {
                return m_CurrentTray.TrayHolder.Find((x) => x.Key == key) != null;
            }
            else
            {
                return false;
            }
        }

        public Tuple<GameObject, int> RemoveItem(string key)
        {
            if(m_CurrentTray != null)
            {
                var holder = m_CurrentTray.TrayHolder.Find((x) => x.Key == key);
                if (holder != null)
                {
                    holder.Key = "";
                    int itemCost = holder.HoldingItemCost;
                    holder.HoldingItemCost = 0;
                    if (holder.HoldingItem != null)
                    {
                        GameObject item = holder.HoldingItem;
                        item.transform.parent = null;
                        holder.HoldingItem = null;
                        return new Tuple<GameObject, int>(item, itemCost);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public List<GameObject> RemoveAllItems()
        {
            if (m_CurrentTray != null)
            {
                List<GameObject> items = new List<GameObject>();
                foreach (var item in m_CurrentTray.TrayHolder)
                {
                    item.Key = "";
                    item.HoldingItemCost = 0;
                    if (item.HoldingItem != null)
                    {
                        GameObject holdingItem = item.HoldingItem;
                        holdingItem.transform.parent = null;
                        item.HoldingItem = null;
                        items.Add(holdingItem);
                    }
                }

                return items;
            }
            else
            {
                return null;
            }
        }
        
    }
}
