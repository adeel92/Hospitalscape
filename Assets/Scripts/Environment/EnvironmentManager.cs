using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isometric.Environment
{
    public class EnvironmentManager : MonoBehaviour
    {
        private static EnvironmentManager s_Instance;
        [SerializeField] List<EnvironmentSetupCaller> m_SetupCallers;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }

        public static void SetupForMenu()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            foreach (var setupCaller in s_Instance.m_SetupCallers)
            {
                setupCaller.MenuCalls?.Invoke();
            }
        }

        public static void SetupForGameplay()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            foreach (var setupCaller in s_Instance.m_SetupCallers)
            {
                setupCaller.GameplayCalls?.Invoke();
            }
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(EnvironmentManager) + " is null");
        }
    }
}
