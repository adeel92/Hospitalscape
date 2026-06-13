using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Isometric.PathSystem;
using System;
using Isometric.Environment;

namespace Isometric.Player
{
    public class PlayerTrayController : MonoBehaviour
    {
        [Serializable]
        private class TrayHandlerInfo
        {
            //public PathDirection Direction;
            public PlayerTrayHandler TrayHandler;
        }

        [SerializeField] List<TrayHandlerInfo> m_TraysHandler;


        public void Setup(int capacity)
        {
            foreach (var trayInfo in m_TraysHandler)
            {
                trayInfo.TrayHandler.Setup(capacity);
            }
        }

        /*public void SetTrayDirection(PathDirection direction)
        {
            if (direction == PathDirection.Right || direction == PathDirection.Down)
            {
                foreach (var trayInfo in m_TraysHandler)
                {
                    if (trayInfo.Direction == PathDirection.Right)
                    {
                        trayInfo.TrayHandler.gameObject.SetActive(true);
                    }
                    else
                    {
                        trayInfo.TrayHandler.gameObject.SetActive(false);

                    }
                }
            }
        }*/

        public bool HasCapacity()
        {
            if (m_TraysHandler != null)
            {
                return m_TraysHandler.TrueForAll((x) => x.TrayHandler.HasCapacity());
            }
            else
            {
                return false;
            }
        }

        public int GetNumberTrayHandlers()
        {
            return m_TraysHandler.Count;
        }

        public bool AddItem(string key, GameObject itemPrefab, int itemCost)
        {
            if (m_TraysHandler != null && HasCapacity())
            {
                foreach (var trayInfo in m_TraysHandler)
                {
                    GameObject itemCopy = Instantiate(itemPrefab);
                    if(itemCopy.TryGetComponent(out StationVisualUpgradeApplier stationVisualUpgradeApplier))
                    {
                        stationVisualUpgradeApplier.ApplyVisualUpgrade();
                    }
                    trayInfo.TrayHandler.AddItem(key, itemCopy, itemCost);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool HasItem(string key)
        {
            if (m_TraysHandler != null)
            {
                return m_TraysHandler.TrueForAll((x) => x.TrayHandler.HasItem(key));
            }
            else
            {
                return false;
            }
        }

        // Returns if it was able to remove and the cost of the item
        public Tuple<bool, int> RemoveItem(string key)
        {
            int itemCost = 0;
            if (m_TraysHandler != null && HasItem(key))
            {
                foreach (var tray in m_TraysHandler)
                {
                    Tuple<GameObject, int> items = tray.TrayHandler.RemoveItem(key);
                    GameObject copyItem = items.Item1;
                    itemCost = items.Item2;
                    Destroy(copyItem);
                }

                return new Tuple<bool, int>(true, itemCost) ;
            }
            else
            {
                return new Tuple<bool, int>(false, itemCost);
            }
        }

        public void RemoveAllItem()
        {
            if (m_TraysHandler != null)
            {
                foreach (var tray in m_TraysHandler)
                {
                    List<GameObject> copyItems = tray.TrayHandler.RemoveAllItems();

                    foreach (var item in copyItems)
                    {
                        Destroy(item);
                    }
                }
            }
        }

        /*public List<GameObject> RemoveItem(string key)
        {
            if (m_TraysHandler != null)
            {
                List<GameObject> items = new List<GameObject>();
                foreach (var tray in m_TraysHandler)
                {
                    items.Add(tray.TrayHandler.RemoveItem(key));
                }
                return items;
            }
            else
            {
                return null;
            }
        }*/


    }
}
