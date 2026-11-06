using System.Collections.Generic;
using UnityEngine;

namespace Isometric.Environment
{
    public class MainServiceStationsHolder : MonoBehaviour
    {
        [Header("---Main Service Instant Order Out---")]
        [SerializeField] List<StationMainServiceInstantOrderOutOnCustomerDemand> m_MainServiceInstantOrderOutStations = new();


        public bool DoAnyMainServiceHaveInstantOrderOutAvailable()
        {
            foreach(StationMainServiceInstantOrderOutOnCustomerDemand stationMainService in m_MainServiceInstantOrderOutStations)
            {
                if (stationMainService.HasAnyDemandedCustomer())
                {
                    return true;
                }
            }

            return false;
        }
    }
}