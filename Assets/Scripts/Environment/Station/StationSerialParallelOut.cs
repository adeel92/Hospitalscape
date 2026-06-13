using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.TaskSystem;
using Isometric.PathSystem;
using Isometric.Cam;
using Isometric.UI;


namespace Isometric.Environment
{
    public class StationSerialParallelOut : MonoBehaviour
    {
        private const string MetaMenuCallsFoldOut = "---Menu Calls---";
        private const string MetaGameplayCallsFoldOut = "---Gameplay Calls---";
        private const string MetaStationsFoldOut = "---Stations---";

        [Serializable]
        public class UpgradeCapacityPropertyCallbackInfo
        {
            public int CapacityMatchIndex;

            [Header("-called at Menu and Gameplay (Called at first unlock too)")]
            public UnityEvent SetupCallback;
            [Header("-setting duration fill amount before processing and sending currently holding (Capacity)")]
            public UnityEvent<float, float> OnSetDurationFillAmount;
            [Header("-start processing and send final duration and sending currently holding (Capacity)")]
            public UnityEvent<float, float> OnDurationStart;
            public UnityEvent OnDurationComplete;
            public UnityEvent OnOrderOutSuccesful;
        }

        [Serializable]
        private class StationInfo
        {
            

            [Header("---Setup---")]
            public TaskTrigger TaskTrigger;
            [Expandable, AllowNesting]
            public DataStation m_Data;
            public DataConsumable OrderTypeOut;

            [Header("---Menu Calls---")]
            [Header("-Station is locked")]
            public UnityEvent OnIsLockedMenu;
            [Header("-Station is unlocked (Not CALLED FIRST TIME)")]
            public UnityEvent OnIsUnlockdMenu;
            [Header("-Unlocking for the first time")]
            public Vector2 CameraFocusPosition;
            public float CameraZoom;
            public float CameraFocusDuration;
            public UnityEvent OnHasUnlockedMenu;
            [Header("-Upgraded any of the properties")]
            public UnityEvent OnHasUpgradedMenu;

            [Header("---Gameplay Calls---")]
            [Header("-Station is locked")]
            public UnityEvent OnIsLockedGameplay;
            [Header("-Station is unlocked")]
            public UnityEvent OnIsUnlockdGameplay;
            [Header("-Upgraded any of the properties")]
            public UnityEvent OnHasUpgradedGameplay;

            [Header("---Upgrade Properties---")]
            public float DurationProperty;
            private float DurationFillCounter = 0;
            private bool IsProcessing = false;

            public int CapacityProperty = 3;
            [AllowNesting, ReadOnly]
            public int CurrentlyHolding = 3;

            public int CostProperty;

            public StationSerialParallelOut Station;

            [HideInInspector]
            public UpgradeCapacityPropertyCallbackInfo CurrentPropertyCallbackInfo ;
            [Space]
            public List<UpgradeCapacityPropertyCallbackInfo> UpgradeCapacityPropertyCallbackInfos;

            public void OnTaskStart(TaskTarget taskTarget)
            {
                if (CurrentPropertyCallbackInfo != null &&
                    taskTarget.TryGetComponent(out IEnvironmentInteractable interactable))
                {
                    if (CurrentlyHolding > 0)
                    {
                        if (interactable.SendDataConsumable(OrderTypeOut, CostProperty))
                        {
                            CurrentlyHolding--;
                            CurrentPropertyCallbackInfo.OnOrderOutSuccesful?.Invoke();
                            CurrentPropertyCallbackInfo.OnSetDurationFillAmount?.Invoke(DurationFillCounter, CurrentlyHolding);
                            if (IsProcessing)
                            {
                                CurrentPropertyCallbackInfo.OnDurationStart?.Invoke(DurationProperty, CurrentlyHolding);
                            }
                            TaskTrigger.SendTaskResult(TaskResult.Success);
                        }
                        else
                        {
                            TaskTrigger.SendTaskResult(TaskResult.Failed);
                        }
                    }
                    else
                    {
                        TaskTrigger.SendTaskResult(TaskResult.Failed);
                    }

                    if (CurrentlyHolding != CapacityProperty)
                    {
                        Station.ProcessStationInfo(this);
                    }
                }
                else
                {
                    TaskTrigger.SendTaskResult(TaskResult.Failed);
                }
            }

            public void StartProcessing()
            {
                if (CurrentPropertyCallbackInfo != null)
                {
                    CoroutineManager.StartACoroutine(Processing());
                }
            }

            IEnumerator Processing()
            {
                IsProcessing = true;
                CurrentPropertyCallbackInfo.OnDurationStart?.Invoke(DurationProperty, CurrentlyHolding);
                DurationFillCounter = 0;

                while (DurationFillCounter < DurationProperty)
                {
                    DurationFillCounter += Time.deltaTime;
                    yield return null;
                }

                DurationFillCounter = 0;
                CurrentPropertyCallbackInfo.OnDurationComplete?.Invoke();
                CurrentlyHolding = CapacityProperty;
                IsProcessing = false;
                Station.ProcessedStationInfo(this);
            }
        }

        //---Menu Calls---
        [Header("-Station is locked (not called if any of the sub-stations are unlocked)"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsLockedMenu;
        [Header("-Station is unlocked (called if any of the sub-stations are unlocked not on first time unlocking)"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsUnlockdMenu;
        [Header("-On Unlocking any of the sub-stations for the first time (called only once)"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnHasUnlockedMenu;

        //---Gameplay Calls---
        [Header("-Station is locked (not called if any of the sub-stations are unlocked)"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsLockedGameplay;
        [Header("-Station is unlocked (called if any of the sub-stations are unlocked not on first time unlocking)"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsUnlockdGameplay;

        //---Stations---
        [Header("-Delay Betweeen processing one of the OrderType")]
        [SerializeField, Foldout(MetaStationsFoldOut)] float m_ProcessingDelay;
        [SerializeField, Foldout(MetaStationsFoldOut)] List<StationInfo> m_StationInfo;
        private List<StationInfo> m_StationStack = new List<StationInfo>();
        private bool m_IsProcessing = false;


        [ContextMenu("SetupForMenu")]
        public void SetupForMenu()
        {
            int numberOfStationsUnlocked = 0;
            bool justUnlocked = false;
            foreach (var subStation in m_StationInfo)
            {
                if (subStation.m_Data.StationData.HasJustUnlocked)
                {
                    justUnlocked = true;
                }
                if (subStation.m_Data.StationData.IsUnlocked)
                {
                    numberOfStationsUnlocked++;
                }
            }

            if (numberOfStationsUnlocked <= 0)
            {
                OnIsLockedMenu?.Invoke();
            }
            else if (numberOfStationsUnlocked == 1 && justUnlocked)
            {
                CoroutineManager.LateAction(() =>
                {
                    OnHasUnlockedMenu?.Invoke();
                }, 1.4f);
            }
            else if(numberOfStationsUnlocked > 0)
            {
                OnIsUnlockdMenu?.Invoke();
            }


            foreach (var stationInfo in m_StationInfo)
            {
                DataStation stationData = stationInfo.m_Data;
                if (!stationData.StationData.IsUnlocked)
                {
                    stationInfo.OnIsLockedMenu?.Invoke();
                }
                else if (stationData.StationData.IsUnlocked && !stationData.StationData.HasJustUnlocked)
                {
                    stationInfo.OnIsUnlockdMenu?.Invoke();
                }

                bool hasJustUnlocked = false;
                if (stationData.StationData.HasJustUnlocked)
                {
                    //hasJustUnlocked = true;
                    CameraController.RegisterFocusCamera(stationInfo.CameraFocusPosition, stationInfo.CameraZoom, 1.4f, 
                    () =>
                    {
                        UIManager.UIInteractionOff();
                        UIManager.HideMenu(null);
                        CameraController.Interactability(false);
                    },
                    () =>
                    {
                        stationInfo.OnHasUnlockedMenu?.Invoke();
                        CoroutineManager.LateAction(() =>
                        {
                            if (CameraController.NextFocusCamera() == false)
                            {
                                CameraController.SetupForMenu(() =>
                                {
                                    UIManager.CheckNextUpdatable();
                                });
                            }

                        }, stationInfo.CameraFocusDuration);
                    });
                    stationData.StationData.HasJustUnlocked = false;
                    stationData.Save();
                }

                if (stationData.StationData.HasUpgraded)
                {
                    stationInfo.OnHasUpgradedMenu?.Invoke();
                    stationData.StationData.HasUpgraded = false;
                    stationData.Save();
                }

                if (stationData.StationData.IsUnlocked
                    && hasJustUnlocked == false)
                {
                    StationUpgrade durationUpgrade = stationData.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
                    if (durationUpgrade != null)
                    {
                        stationInfo.DurationProperty = durationUpgrade.Upgrade[durationUpgrade.CurrentUpgradeIndex];
                    }

                    StationUpgrade costUpgrade = stationData.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
                    if (costUpgrade != null)
                    {
                        stationInfo.CostProperty = Mathf.RoundToInt(costUpgrade.Upgrade[costUpgrade.CurrentUpgradeIndex]);
                    }

                    StationUpgrade capacityUpgrade = stationData.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
                    if (capacityUpgrade != null)
                    {
                        stationInfo.CapacityProperty = (int)capacityUpgrade.Upgrade[capacityUpgrade.CurrentUpgradeIndex];
                        stationInfo.CurrentlyHolding = stationInfo.CapacityProperty;

                        UpgradeCapacityPropertyCallbackInfo UpgradeCapacityPropertyCallbackInfo = stationInfo.UpgradeCapacityPropertyCallbackInfos.Find((x) => x.CapacityMatchIndex == capacityUpgrade.CurrentUpgradeIndex);
                        if (UpgradeCapacityPropertyCallbackInfo != null)
                        {
                            UpgradeCapacityPropertyCallbackInfo.SetupCallback?.Invoke();
                            stationInfo.CurrentPropertyCallbackInfo = UpgradeCapacityPropertyCallbackInfo;
                        }
                    }
                }
            }
        }

        [ContextMenu("SetupForGameplay")]
        public void SetupForGameplay()
        {
            bool stationsUnlocked = false;
            foreach (var subStation in m_StationInfo)
            {
                if (subStation.m_Data.StationData.IsUnlocked)
                {
                    stationsUnlocked = true;
                }
            }

            if (stationsUnlocked == false)
            {
                OnIsLockedGameplay?.Invoke();
            }
            else if (stationsUnlocked == true)
            {
                OnIsUnlockdGameplay?.Invoke();
            }

            foreach (var stationInfo in m_StationInfo)
            {
                DataStation stationData = stationInfo.m_Data;
                if (!stationData.StationData.IsUnlocked)
                {
                    stationInfo.OnIsLockedGameplay?.Invoke();
                }
                else if (stationData.StationData.IsUnlocked && !stationData.StationData.HasJustUnlocked)
                {
                    stationInfo.OnIsUnlockdGameplay?.Invoke();
                }

                if (stationData.StationData.HasUpgraded)
                {
                    stationInfo.OnHasUpgradedGameplay?.Invoke();
                    stationData.StationData.HasUpgraded = false;
                    stationData.Save();
                }

                if (stationData.StationData.IsUnlocked)
                {
                    StationUpgrade durationUpgrade = stationData.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
                    if (durationUpgrade != null)
                    {
                        stationInfo.DurationProperty = durationUpgrade.Upgrade[durationUpgrade.CurrentUpgradeIndex];
                    }

                    StationUpgrade costUpgrade = stationData.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
                    if (costUpgrade != null)
                    {
                        stationInfo.CostProperty = Mathf.RoundToInt(costUpgrade.Upgrade[costUpgrade.CurrentUpgradeIndex]);
                    }

                    StationUpgrade capacityUpgrade = stationData.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
                    if (capacityUpgrade != null)
                    {
                        stationInfo.CapacityProperty = (int)capacityUpgrade.Upgrade[capacityUpgrade.CurrentUpgradeIndex];
                        stationInfo.CurrentlyHolding = stationInfo.CapacityProperty;

                        UpgradeCapacityPropertyCallbackInfo UpgradeCapacityPropertyCallbackInfo = stationInfo.UpgradeCapacityPropertyCallbackInfos.Find((x) => x.CapacityMatchIndex == capacityUpgrade.CurrentUpgradeIndex);
                        if (UpgradeCapacityPropertyCallbackInfo != null)
                        {
                            UpgradeCapacityPropertyCallbackInfo.SetupCallback?.Invoke();
                            stationInfo.CurrentPropertyCallbackInfo = UpgradeCapacityPropertyCallbackInfo;
                        }
                    }
                }

            }
        }

        private void OnEnable()
        {
            foreach (var item in m_StationInfo)
            {
                item.TaskTrigger.OnTaskStart += item.OnTaskStart;
            }
        }

        private void OnDisable()
        {
            foreach (var item in m_StationInfo)
            {
                item.TaskTrigger.OnTaskStart -= item.OnTaskStart;
            }
        }

        private void ProcessStationInfo(StationInfo stationInfo)
        {
            if (!m_StationStack.Contains(stationInfo))
            {
                m_StationStack.Add(stationInfo);
                if (m_IsProcessing == false)
                {
                    m_IsProcessing = true;
                    CoroutineManager.LateAction(() =>
                    {
                        stationInfo.StartProcessing();

                    }, m_ProcessingDelay);
                }
            }
        }

        private void ProcessedStationInfo(StationInfo stationInfo)
        {
            m_StationStack.Remove(stationInfo);

            if (m_StationStack.Count > 0)
            {
                StationInfo newStationInfo = m_StationStack[0];
                CoroutineManager.LateAction(() =>
                {
                    newStationInfo.StartProcessing();

                }, m_ProcessingDelay);
            }
            else
            {
                m_IsProcessing = false;
            }
        }
    }
}
