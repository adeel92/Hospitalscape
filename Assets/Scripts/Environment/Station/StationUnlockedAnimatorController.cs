using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyAttributes;
using Isometric.Data;

namespace Isometric.Environment
{
    public class StationUnlockedAnimatorController : MonoBehaviour
    {
        [Serializable]
        private class DurationParamterInfo
        {
            public int MatchValueIndex;
            public string AnimatorParameterName;
            public float AnimatorParameterValue;
        }

        [Serializable]
        private class CapacityParamterInfo
        {
            public int MatchValueIndex;
            public string AnimatorParameterName;
            public int AnimatorParameterValue;
        }

        [Serializable]
        private class CostParamterInfo
        {
            public int MatchValueIndex;
            public string AnimatorParameterName;
            public int AnimatorParameterValue;
        }

        [SerializeField] Animator m_Animator;

        [SerializeField] List<DurationParamterInfo> m_DurationParamterInfo;
        [SerializeField] List<CapacityParamterInfo> m_CapacityParamterInfo;
        [SerializeField] List<CostParamterInfo> m_CostParamterInfo;

        public void SetDurationValueIndex(int durationValueIndex)
        {
            if (m_DurationParamterInfo != null)
            {
                foreach (var item in m_DurationParamterInfo)
                {
                    if (item.MatchValueIndex == durationValueIndex)
                    {
                        m_Animator.SetFloat(item.AnimatorParameterName, item.AnimatorParameterValue);
                        return;
                    }
                }

                Debug.LogWarning("No Match value found for the duration " + durationValueIndex);
            }
        }

        public void SetCapacityValueIndex(int capacityValueIndex)
        {
            if (m_CapacityParamterInfo != null)
            {
                foreach (var item in m_CapacityParamterInfo)
                {
                    if (item.MatchValueIndex == capacityValueIndex)
                    {
                        m_Animator.SetInteger(item.AnimatorParameterName, item.AnimatorParameterValue);
                        return;
                    }
                }

                Debug.LogWarning("No Match value found for the capacity");
            }
        }

        public void SetCostValueIndex(int costValueIndex)
        {
            if (m_CostParamterInfo != null)
            {
                foreach (var item in m_CostParamterInfo)
                {
                    if (item.MatchValueIndex == costValueIndex)
                    {
                        m_Animator.SetInteger(item.AnimatorParameterName, item.AnimatorParameterValue);
                        return;
                    }
                }

                Debug.LogWarning("No Match value found for the capacity");
            }
        }

        public void PlayState(string stateName)
        {
            m_Animator.Play(stateName);
        }
    }
}
