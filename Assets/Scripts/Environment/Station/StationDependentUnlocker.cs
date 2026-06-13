using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Isometric.Data;

namespace Isometric.Environment
{
    public class StationDependentUnlocker : MonoBehaviour
    {
        [SerializeField] List<DataStation> m_DependentDataStations;
        [SerializeField] List<DataCafeChair> m_DependentDataCafeChairs;


        public void SetToUnlock()
        {
            foreach (var dependentDataStation in m_DependentDataStations)
            {
                dependentDataStation.StationData.IsUnlocked = true;
                dependentDataStation.StationData.HasJustUnlocked = true;

                dependentDataStation.Save();
            }

            foreach (var dependentDataCafeChair in m_DependentDataCafeChairs)
            {
                dependentDataCafeChair.CafeChairData.IsUnlocked = true;
                dependentDataCafeChair.CafeChairData.HasJustUnlocked = true;

                dependentDataCafeChair.Save();
            }
        }
    }
}
