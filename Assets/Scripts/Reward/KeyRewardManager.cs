using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isometric.Reward
{
    public class KeyRewardManager : MonoBehaviour
    {
        private static KeyRewardManager s_Instance;

        [SerializeField] bool m_IsUsingKey;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }

        public static bool IsUsingKeyReward()
        {
            if(s_Instance == null)
            {
                PrintNullInstanceError();
                return false;
            }

            return s_Instance.m_IsUsingKey;
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(KeyRewardManager) + " is null");
        }
    }
}
