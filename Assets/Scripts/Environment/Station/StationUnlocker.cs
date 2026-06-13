using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Isometric.Data;
using Isometric.PathSystem;

namespace Isometric.Environment
{
    public class StationUnlocker : MonoBehaviour
    {
        public DataStation Data => m_Data;
        [SerializeField] DataStation m_Data;

        [Header("---Station is locked---")]
        public UnityEvent OnIsLocked;
        
        [Header("---Station is unlocked (Not CALLED FIRST TIME)---")]
        public UnityEvent OnIsUnlockd;

        [Header("---Unlocking for the first time---")]
        public UnityEvent OnHasUnlocked;

        [Header("---Upgraded any of the properties---")]
        public UnityEvent OnHasUpgraded;


        [ContextMenu("Setup")]
        public void Setup()
        {
            if (!m_Data.StationData.IsUnlocked)
            {
                OnIsLocked?.Invoke();
            }
            else if(m_Data.StationData.IsUnlocked
                && !m_Data.StationData.HasJustUnlocked)
            {
                OnIsUnlockd?.Invoke();
            }

            if (m_Data.StationData.HasJustUnlocked)
            {
                OnHasUnlocked?.Invoke();
                m_Data.StationData.HasJustUnlocked = false;
                m_Data.Save();
            }

            if (m_Data.StationData.HasUpgraded)
            {
                OnHasUpgraded?.Invoke();
                m_Data.StationData.HasUpgraded = false;
                m_Data.Save();
            }
        }
    }

    public interface IEnvironmentInteractable
    {
        //Returns if it got dataConsumable and returns the cost
        public Tuple<bool, int> GetDataConsumable(DataConsumable dataConsumable);

        public bool SendDataConsumable(DataConsumable dataConsumable, int itemCost);

        public void EngageInteractable(PathDirection direction);

        public void RemoveAllDataConsumables();
    }
}
