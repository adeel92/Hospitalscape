using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isometric.Player
{
    public class PlayerManager : MonoBehaviour
    {
        private static PlayerManager s_Instance;

        [SerializeField] List<PlayerController> m_PlayerControllers;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }

        public static void SetupForMenu()
        {
            if(s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            foreach (var playerController in s_Instance.m_PlayerControllers)
            {
                playerController.SetupForMenu();
            }
        }

        public static void SetupForGameplay()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            foreach (var playerController in s_Instance.m_PlayerControllers)
            {
                playerController.SetupForGameplay();
            }
        }

        public static PlayerController GetPlayerAtIndex(int index)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return null;
            }

            if (s_Instance.m_PlayerControllers.Count > index && index >= 0)
            {
                return s_Instance.m_PlayerControllers[index];
            }

            return null;
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(PlayerManager) + " is null");
        }
    }
}
