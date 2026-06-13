using System;
using System.Collections.Generic;
using Isometric.Data;
using UnityEngine;
using UnityEngine.Events;

namespace Isometric.Environment
{
    public class StationVisualUpgradeApplier : MonoBehaviour
    {
        [SerializeField] DataStation m_DataStation;
        [Space, SerializeField] List<StationVisualUpgradeInfo> StationVisualUpgradeInfos;

        public void ApplyVisualUpgrade()
        {
            foreach(StationVisualUpgradeInfo visualUpgradeInfo in StationVisualUpgradeInfos)
            {
                StationUpgrade stationUpgrade = m_DataStation.GetStationUpgrade(visualUpgradeInfo.UpgradeType);
                if(stationUpgrade != null)
                {
                    StationVisualUpgradeLevel visualUpgradeLevel = visualUpgradeInfo.StationVisualUpgradeLevels.Find(level => level.UpgradeLevelIndex == stationUpgrade.CurrentUpgradeIndex);

                    if(visualUpgradeLevel != null)
                    {
                        visualUpgradeLevel.OnUpgradeCallback?.Invoke();
                    }
                }
            }
        }

        #region Nested Classes
        [Serializable]
        public class StationVisualUpgradeInfo
        {
            public PropertyUpgradeType UpgradeType;
            public List<StationVisualUpgradeLevel> StationVisualUpgradeLevels;
        }

        [Serializable]
        public class StationVisualUpgradeLevel
        {
            public int UpgradeLevelIndex;
            public UnityEvent OnUpgradeCallback;
        }
        #endregion
    }
}
