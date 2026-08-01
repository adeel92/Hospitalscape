using System.Collections.Generic;
using Isometric.Data;
using UnityEngine;
using UnityEngine.Events;

namespace Isometric.Environment
{
    public class StationDependableHolderUnlocker : MonoBehaviour
    {
        [SerializeField] List<DataStation> m_DependableStations = new();
        [Space, SerializeField] UnityEvent m_OnIsLocked;
        [SerializeField] UnityEvent m_OnIsUnlocked;
        
        public void UnlockHolder()
        {
            m_OnIsUnlocked?.Invoke();
        }

        public void LockHolder()
        {
            bool stationUnlocked = false;
            foreach(DataStation dependableStation in m_DependableStations)
            {
                if (dependableStation.StationData.IsUnlocked)
                {
                    stationUnlocked = true;
                    break;
                }
            }

            if (!stationUnlocked)
            {
                m_OnIsLocked?.Invoke();
            }
        }
    }
}