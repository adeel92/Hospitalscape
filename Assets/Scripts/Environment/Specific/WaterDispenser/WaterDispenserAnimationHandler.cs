using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Isometric.Data;
using NaughtyAttributes;
using UnityEngine;

namespace Isometric.Environment
{
	public class WaterDispenserAnimationHandler : MonoBehaviour
	{
		[Header("---Station Info---")]
        [SerializeField] DataStation m_DataStation;
		[SerializeField] StationAutoOrderOutVisualUpgrade m_StationAutoOrderOutVisualUpgrade;
        [Space]
        [SerializeField] float m_DelayBeforePouring;
        [SerializeField, ReadOnly] WaterDispenserAnimationInfo m_CurrentWaterDispenserInfo;

		[Header("---Animation Handlers---")]
        [SerializeField] List<WaterDispenserAnimationInfo> m_WaterDispenserAnimationInfos = new();

        private StationUpgrade m_UpgradeCapacity;


        private void Awake()
        {
            m_UpgradeCapacity = m_DataStation.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
        }
        
        public void StartFilling(float duration)
        {
            int capacityProperty = GetCurrentCapacity();
            m_CurrentWaterDispenserInfo = m_WaterDispenserAnimationInfos.Find(x => x.CapacityValue == capacityProperty);

            if(m_CurrentWaterDispenserInfo != null)
            {
                StationAutoOrderOutItemAnimationHandler stationAutoOrderOutItemAnimationHandler = m_CurrentWaterDispenserInfo.StationAutoOrderOutItemAnimationHandler;
                stationAutoOrderOutItemAnimationHandler.PlayState(StationAutoOrderOutItemAnimatorStates.Fill, null, null);
                StartCoroutine(PourWater(duration));
            }
            else
            {
                Debug.LogWarning($"Water dispenser with capacity {capacityProperty} is not present inside WaterDispenserAnimationInfo of {nameof(WaterDispenserAnimationHandler)}!");
            }
        }

        private IEnumerator PourWater(float duration)
        {
            yield return new WaitForSeconds(m_DelayBeforePouring);

            List<WaterGlassInfo> waterGlassInfos = m_CurrentWaterDispenserInfo.WaterGlassInfos;
            List<ParticleSystem> particles = m_CurrentWaterDispenserInfo.FillParticles;

            for(int i = 0; i < waterGlassInfos.Count; i++)
            {
                particles[i].Play();
                Transform water = waterGlassInfos[i].WaterTransform;
                water.localPosition = waterGlassInfos[i].WaterUnfillLocalPos;
                water.DOLocalMove(waterGlassInfos[i].WaterFillLocalPos, duration);
            }
        }

        public void StopFilling()
        {
            if(m_CurrentWaterDispenserInfo != null)
            {
                foreach(ParticleSystem particle in m_CurrentWaterDispenserInfo.FillParticles)
                {
                    particle.Stop();
                }

                m_CurrentWaterDispenserInfo.StationAutoOrderOutItemAnimationHandler.PlayState(StationAutoOrderOutItemAnimatorStates.Init, null, null);
                m_CurrentWaterDispenserInfo = null;
            }
            else
            {
                Debug.LogWarning($"Water dispenser is null!");
            }
        }

        private int GetCurrentCapacity()
        {
            if(m_UpgradeCapacity == null)
            {
                return 0;
            } 
            
            int capacityProperty = (int)m_UpgradeCapacity.Upgrade[m_UpgradeCapacity.CurrentUpgradeIndex];
            return capacityProperty;
        }


        [Serializable]
        public class WaterDispenserAnimationInfo
        {
            public int CapacityValue = 1;
            public StationAutoOrderOutItemAnimationHandler StationAutoOrderOutItemAnimationHandler;
            public List<ParticleSystem> FillParticles = new();
            public List<WaterGlassInfo> WaterGlassInfos = new();
        }

        [Serializable]
        public class WaterGlassInfo
        {
            public Transform WaterTransform;
            public Vector3 WaterUnfillLocalPos;
            public Vector3 WaterFillLocalPos;
        }
	}
}
