using System.Collections.Generic;
using UnityEngine;

namespace Isometric.Environment
{
    public class EnvironmentItemsDisabler : MonoBehaviour
    {
        private static EnvironmentItemsDisabler s_Instance;
        [SerializeField] bool m_AlwaysDisableUnusedItems = true;
        [Space, SerializeField] List<GameObject> m_ItemsToDisableForGameplay = new();


        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }

        public static void EnableItemsForMenu()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            if (!s_Instance.m_AlwaysDisableUnusedItems)
            {
                return;
            }

            if(s_Instance.m_ItemsToDisableForGameplay != null && s_Instance.m_ItemsToDisableForGameplay.Count > 0)
            {
                foreach(GameObject item in s_Instance.m_ItemsToDisableForGameplay)
                {
                    item.SetActive(true);
                }
            }
        }
        public static void DisableItemsForGameplay()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            if (!s_Instance.m_AlwaysDisableUnusedItems)
            {
                return;
            }

            if(s_Instance.m_ItemsToDisableForGameplay != null && s_Instance.m_ItemsToDisableForGameplay.Count > 0)
            {
                foreach(GameObject item in s_Instance.m_ItemsToDisableForGameplay)
                {
                    item.SetActive(false);
                }
            }
        }


        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(EnvironmentItemsDisabler) + " is null");
        }
    }
}