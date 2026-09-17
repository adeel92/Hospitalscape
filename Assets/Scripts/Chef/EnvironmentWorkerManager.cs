using System.Collections.Generic;
using UnityEngine;

namespace Isometric.Environment.Worker
{
    public class EnvironmentWorkerManager : MonoBehaviour
    {
        private static EnvironmentWorkerManager s_Instance;

        [SerializeField] List<EnvironmentWorkerController> m_WorkerControllers = new();
        

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
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(EnvironmentWorkerManager) + " is null");
        }
    }
}
