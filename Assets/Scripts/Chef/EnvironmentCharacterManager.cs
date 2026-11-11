using System.Collections.Generic;
using Isometric.Customer;
using UnityEngine;

namespace Isometric.Environment.Worker
{
    public class EnvironmentCharacterManager : MonoBehaviour
    {
        private static EnvironmentCharacterManager s_Instance;

        [SerializeField] List<EnvironmentWorkerController> m_WorkerControllers = new();
        [SerializeField] List<EnvironmentCustomerController> m_CustomerControllers = new();
        [SerializeField] List<EnvironmentVehicleController> m_VehicleControllers = new();
        

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }

        public static void Setup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            foreach(EnvironmentWorkerController workerController in s_Instance.m_WorkerControllers)
            {
                workerController.Setup();
            }

            foreach(EnvironmentCustomerController customerController in s_Instance.m_CustomerControllers)
            {
                customerController.Setup();
            }

            foreach(EnvironmentVehicleController vehicleController in s_Instance.m_VehicleControllers)
            {
                vehicleController.Setup();
            }
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(EnvironmentCharacterManager) + " is null");
        }
    }
}
