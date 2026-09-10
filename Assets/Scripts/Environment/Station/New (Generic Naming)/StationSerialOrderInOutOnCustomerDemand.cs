using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.TaskSystem;
using Isometric.Cam;
using Isometric.UI;
using Isometric.Tutorial;
using System.Linq;
using Unity.VisualScripting;

namespace Isometric.Environment
{
    public class StationSerialOrderInOutOnCustomerDemand : MonoBehaviour
    {
        [Serializable]
        private class StationOutInfo
        {
            [AllowNesting, ReadOnly]
            public bool IsHolding = false;
            public DataConsumable OrderOutType;
            public UnityEvent OnOrderInSuccesful;
            public UnityEvent OnOrderOutSuccesful;
        }

        //---Setup---
        const string MetaSetupFoldOut = "---Setup---";
        [SerializeField, Foldout(MetaSetupFoldOut), Expandable] DataStation m_Data;
        [SerializeField, Foldout(MetaSetupFoldOut)] TaskTrigger m_TaskTrigger;
        [SerializeField, Foldout(MetaSetupFoldOut)] MainServiceOrderUIController m_OrderUIController;
        [SerializeField, Foldout(MetaSetupFoldOut)] List<InputOrderInfo> m_InputOrderInfos = new();
        [Space(), SerializeField, Foldout(MetaSetupFoldOut), ReadOnly] List<InputOrderInfo> m_CurrentRequiredInputOrderInfo = new();
        [SerializeField, Foldout(MetaSetupFoldOut)] List<PendingInputOrderInfo> m_CurrentPendingToProcessInputOrderInfo = new();
        [SerializeField, Foldout(MetaSetupFoldOut)] List<OutputOrderInfo> m_CurrentOutputOrderInfos = new();
        [SerializeField, Foldout(MetaSetupFoldOut), ReadOnly] bool m_IsProcessing = false;

        //---Menu Calls---
        const string MetaMenuCallsFoldOut = "---Menu Calls---";
        [Header("-Station is locked"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsLockedMenu;
        [Header("-Station is unlocked (Not CALLED FIRST TIME)"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsUnlockdMenu;
        /* [Header("-Unlocking for the first time")]
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] Vector2 m_CameraFocusPosition;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraZoom;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraFocusDuration;
        [Foldout(MetaMenuCallsFoldOut)] public UnityEvent OnHasUnlockedMenu; */
        [Header("-Upgraded any of the properties"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnHasUpgradedMenu;

        //---Gameplay Calls---
        const string MetaGameplayCallsFoldOut = "---Gameplay Calls---";
        [Header("-Station is locked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsLockedGameplay;
        [Header("-Station is unlocked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsUnlockdGameplay;
        [Header("-Unlocking for the first time")]
        [SerializeField, Foldout(MetaGameplayCallsFoldOut)] bool m_UseCameraFocus = false;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut), ShowIf(nameof(m_UseCameraFocus))] Vector2 m_CameraFocusPosition;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut), ShowIf(nameof(m_UseCameraFocus))] float m_CameraZoom;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut)] bool m_UseSoftMaskFocus = true;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut), ShowIf(nameof(m_UseSoftMaskFocus))] Transform m_FocusTarget;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut), ShowIf(nameof(m_UseSoftMaskFocus))] Vector2 m_FocusPositionOffset;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut), ShowIf(nameof(m_UseSoftMaskFocus))] Vector2 m_FocusScale;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut), ShowIf(nameof(m_UseSoftMaskFocus))] FocusShapeType m_FocusShapeType;
        [SerializeField, Foldout(MetaGameplayCallsFoldOut)] float m_FocusDuration;
        [Foldout(MetaGameplayCallsFoldOut)] public UnityEvent OnHasUnlockedGameplay;
        [Header("-Upgraded any of the properties"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnHasUpgradedGameplay;

        //---Upgrade Properties---
        const string MetaUpgradePropertiesFoldOut = "---Upgrade Properties---";
        public float DurationProperty => m_DurationProperty;
        public float ExtraDurationForAnimation {get {return m_ExtraDurationForAnimation;} set {m_ExtraDurationForAnimation = value;}}
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] float m_DurationProperty;
        [Tooltip("This duration is added to the actual Duration Property to cover the initial input item animation duration too.")]
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut), ReadOnly] float m_ExtraDurationForAnimation;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] int m_CapacityProperty = 3;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] int m_CostProperty = 3;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] UnityEvent<int> OnDurationSetup;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] UnityEvent<float> OnDurationStart;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] UnityEvent OnDurationComplete;
        // [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] List<StationOutInfo> m_StationOutInfo;

        private Coroutine m_StartProcessCoroutine = null;
        private int m_AvailableOutputOrders = 0;
        private int m_AvailablePendingToProcessInputOrders = 0;
        private List<InputOrderInfo> m_CurrentInputOrderInfosForProcessing = new();


        [ContextMenu("SetupForMenu")]
        public void SetupForMenu()
        {
            if (!m_Data.StationData.IsUnlocked)
            {
                OnIsLockedMenu?.Invoke();
            }
            else if (m_Data.StationData.IsUnlocked /* && !m_Data.StationData.HasJustUnlocked */)
            {
                OnIsUnlockdMenu?.Invoke();
            }

            if (m_Data.StationData.HasUpgraded)
            {
                OnHasUpgradedMenu?.Invoke();
                m_Data.StationData.HasUpgraded = false;
                m_Data.Save();
            }
        }

        [ContextMenu("SetupForGameplay")]
        public void SetupForGameplay()
        {
            if (!m_Data.StationData.IsUnlocked)
            {
                OnIsLockedGameplay?.Invoke();
            }
            else if (m_Data.StationData.IsUnlocked && !m_Data.StationData.HasJustUnlocked)
            {
                OnIsUnlockdGameplay?.Invoke();
            }

            bool hasJustUnlocked = false;
            if (m_Data.StationData.HasJustUnlocked)
            {
                hasJustUnlocked = true;
                if (m_UseCameraFocus)
                {
                    CameraController.RegisterFocusCamera(m_CameraFocusPosition, m_CameraZoom, 1.4f, 
                    () =>
                    {
                        UIManager.UIInteractionOff();
                        // GameManager.PauseGame();
                    }, 
                    () =>
                    {
                        OnHasUnlockedGameplay?.Invoke();
                        CoroutineManager.LateAction(() =>
                        {
                            if (CameraController.NextFocusCamera() == false)
                            {
                                CameraController.SetupForGameplay(() =>
                                {
                                    UIManager.CheckNextGameplayUpdatable();
                                });
                            }

                            ApplyUpgradeProperties();
                        }, m_FocusDuration);
                    });
                }
                else if (m_UseSoftMaskFocus)
                {
                    /* NOT 100% COMPLETE! BELOW FOCUS WILL WORK BUT ADDITIONAL DESCRIPTION MECHANISM IS PENDING */

                    TutorialFocusManager.FocusAtTransform(m_FocusTarget, m_FocusPositionOffset, m_FocusScale, m_FocusShapeType,
                    () =>
                    {
                        UIManager.UIInteractionOff();
                    },
                    () =>
                    {
                        OnHasUnlockedGameplay?.Invoke();
                        CoroutineManager.LateAction(() =>
                        {
                            TutorialFocusManager.StopAllFocus();
                            CameraController.SetupForGameplay(() =>
                            {
                                UIManager.CheckNextGameplayUpdatable();
                            });

                            ApplyUpgradeProperties();
                        }, m_FocusDuration);
                    });
                }

                m_Data.StationData.HasJustUnlocked = false;
                m_Data.Save();
            }

            if (m_Data.StationData.HasUpgraded)
            {
                OnHasUpgradedGameplay?.Invoke();
                m_Data.StationData.HasUpgraded = false;
                m_Data.Save();
            }

            if(m_Data.StationData.IsUnlocked && !hasJustUnlocked)
                ApplyUpgradeProperties();
        }

        private void ApplyUpgradeProperties()
        {
            StationUpgrade upgradeCapacity = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Capacity);
            if (upgradeCapacity != null)
            {
                m_CapacityProperty = (int)upgradeCapacity.Upgrade[upgradeCapacity.CurrentUpgradeIndex];
            }

            StationUpgrade upgradeDuration = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
            if (upgradeDuration != null)
            {
                m_DurationProperty = upgradeDuration.Upgrade[upgradeDuration.CurrentUpgradeIndex];
                OnDurationSetup?.Invoke(upgradeDuration.CurrentUpgradeIndex);
            }

            StationUpgrade upgradeCost = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
            if (upgradeCost != null)
            {
                m_CostProperty = Mathf.RoundToInt(upgradeCost.Upgrade[upgradeCost.CurrentUpgradeIndex]);
            }
        }

        private void OnEnable()
        {
            m_TaskTrigger.OnTaskStart += OnTaskStart;
            GlobalEventHolder.OnTrayItemPicked += UpdateRequiredInputOrdersForCustomerDemand;
        }

        private void OnDisable()
        {
            m_TaskTrigger.OnTaskStart -= OnTaskStart;
            GlobalEventHolder.OnTrayItemPicked -= UpdateRequiredInputOrdersForCustomerDemand;
        }

        private void UpdateRequiredInputOrdersForCustomerDemand(DataConsumable pickedOrder)
        {
            foreach(InputOrderInfo inputOrderInfo in m_InputOrderInfos)
            {
                if(inputOrderInfo.InputOrderType == pickedOrder)
                {
                    m_CurrentRequiredInputOrderInfo.Add(inputOrderInfo);
                    List<DataConsumable> requiredInputOrderTypes = m_CurrentRequiredInputOrderInfo.Select(x => x.InputOrderType).ToList();
                    m_OrderUIController.CleanPreviousOrders();
                    m_OrderUIController.SetOrders(requiredInputOrderTypes, null);
                    break;
                }
            }
        }
        private void UpdateCurrentPendingInputOrdersToProcess(InputOrderInfo inputOrderInfo)
        {
            PendingInputOrderInfo pendingInputOrderInfo = m_CurrentPendingToProcessInputOrderInfo.Find(x => x.IsOccupied == false);
            if(pendingInputOrderInfo != null)
            {
                pendingInputOrderInfo.InputOrderType = inputOrderInfo.InputOrderType;
                pendingInputOrderInfo.OutputOrderType = inputOrderInfo.OutputOrderType;
                pendingInputOrderInfo.OutputOrderDisplayObjPrefab = inputOrderInfo.OutputOrderDisplayObjPrefab;
                pendingInputOrderInfo.OutputOrderDisplayObj = null;
                pendingInputOrderInfo.InputOrderDisplayObjPrefab = inputOrderInfo.InputOrderDisplayObjPrefab;
                GameObject displayObj = Instantiate(pendingInputOrderInfo.InputOrderDisplayObjPrefab, pendingInputOrderInfo.PendingInputOrderHolder);
                displayObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                displayObj.transform.localScale = Vector3.one;
                pendingInputOrderInfo.InputOrderDisplayObj = displayObj;
                pendingInputOrderInfo.IsOccupied = true;
                m_AvailablePendingToProcessInputOrders++;
            }
            else
            {
                Debug.LogWarning($"{nameof(StationSerialOrderInOutOnCustomerDemand)} -- No space is left inside pending orders tray for the recently retrieved input orders!");
            }
        }
        private void UpdateCurrentOutputOrdersForCustomerDemand()
        {
            foreach(InputOrderInfo inputOrderInfo in m_CurrentInputOrderInfosForProcessing)
            {
                OutputOrderInfo outputOrderInfo = m_CurrentOutputOrderInfos.Find(x => x.IsOccupied == false);
                if(outputOrderInfo != null)
                {
                    outputOrderInfo.InputOrderType = inputOrderInfo.InputOrderType;
                    outputOrderInfo.OutputOrderType = inputOrderInfo.OutputOrderType;
                    outputOrderInfo.OutputOrderDisplayObjPrefab = inputOrderInfo.OutputOrderDisplayObjPrefab;
                    Debug.Log(
                        "Adeel Check 1:" +
                        $"Prefab={inputOrderInfo.OutputOrderDisplayObjPrefab != null}, " +
                        $"OutputPrefab={outputOrderInfo.OutputOrderDisplayObjPrefab != null}, " +
                        $"Holder={outputOrderInfo.OutputOrderHolder != null}"
                    );
                    GameObject displayObj = Instantiate(outputOrderInfo.OutputOrderDisplayObjPrefab, outputOrderInfo.OutputOrderHolder);
                    displayObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    displayObj.transform.localScale = Vector3.one;
                    outputOrderInfo.OutputOrderDisplayObj = displayObj;
                    outputOrderInfo.IsOccupied = true;
                    m_AvailableOutputOrders++;
                }
                else
                {
                    Debug.LogWarning($"{nameof(StationSerialOrderInOutOnCustomerDemand)} -- No space is left inside output orders tray for the recently processed orders!");
                }
            }
        }

        private bool IsAnyInputOrderRequired()
        {
            return m_CurrentRequiredInputOrderInfo.Count > 0;
        }
        private bool IsAnyOutputOrderAvailable()
        {
            return m_AvailableOutputOrders > 0;
        }
        private bool IsAnyPendingToProcessInputOrderAvailable()
        {
            return m_AvailablePendingToProcessInputOrders > 0;
        }

        private void ResetRequiredInputOrders()
        {
            m_CurrentRequiredInputOrderInfo.Clear();
            m_OrderUIController.CleanPreviousOrders();
        }        
        private void ResetPendingToProcessInputOrderInfo(PendingInputOrderInfo pendingInputOrderInfo)
        {
            m_AvailablePendingToProcessInputOrders--;
            pendingInputOrderInfo.InputOrderType = null;
            pendingInputOrderInfo.InputOrderDisplayObjPrefab = null;
            pendingInputOrderInfo.OutputOrderType = null;
            pendingInputOrderInfo.OutputOrderDisplayObjPrefab = null;
            Destroy(pendingInputOrderInfo.InputOrderDisplayObj);
            pendingInputOrderInfo.InputOrderDisplayObj = null;
            pendingInputOrderInfo.IsOccupied = false;
        }
        private void ResetOutputOrderInfo(OutputOrderInfo outputOrderInfo)
        {
            m_AvailableOutputOrders--;
            outputOrderInfo.InputOrderType = null;
            outputOrderInfo.OutputOrderType = null;
            outputOrderInfo.OutputOrderDisplayObjPrefab = null;
            Destroy(outputOrderInfo.OutputOrderDisplayObj);
            outputOrderInfo.OutputOrderDisplayObj = null;
            outputOrderInfo.IsOccupied = false;
        }
        private void ResetProcess()
        {
            m_CurrentInputOrderInfosForProcessing.Clear();
            m_IsProcessing = false;
        }

        private void CheckForPendingToProcessInputOrders()
        {
            if (IsAnyPendingToProcessInputOrderAvailable())
            {
                foreach(PendingInputOrderInfo pendingInputOrderInfo in m_CurrentPendingToProcessInputOrderInfo)
                {
                    if(!pendingInputOrderInfo.IsOccupied)
                        continue;
                        
                    InputOrderInfo inputOrderInfo = new()
                    {
                        InputOrderType = pendingInputOrderInfo.InputOrderType,
                        InputOrderDisplayObjPrefab = pendingInputOrderInfo.InputOrderDisplayObjPrefab,
                        OutputOrderType = pendingInputOrderInfo.OutputOrderType,
                        OutputOrderDisplayObjPrefab = pendingInputOrderInfo.OutputOrderDisplayObjPrefab
                    };
                    m_CurrentInputOrderInfosForProcessing.Add(inputOrderInfo);
                    ResetPendingToProcessInputOrderInfo(pendingInputOrderInfo);
                }

                m_StartProcessCoroutine = StartCoroutine(StartProcess());
            }
        }

        private IEnumerator StartProcess()
        {
            m_IsProcessing = true;
            OnDurationStart?.Invoke(m_DurationProperty);
            float totalDuration = m_DurationProperty + m_ExtraDurationForAnimation;
            yield return new WaitForSeconds(totalDuration);

            OnDurationComplete?.Invoke();
            UpdateCurrentOutputOrdersForCustomerDemand();
            ResetProcess();
            CheckForPendingToProcessInputOrders();
        }

        private void OnTaskStart(TaskTarget taskTarget)
        {
            if (taskTarget.TryGetComponent(out IEnvironmentInteractable interactable))
            {
                bool hasProvidedInputOrder = false;
                bool hasTakenOutputOrder = false;

                if (IsAnyInputOrderRequired())
                {
                    foreach(InputOrderInfo inputOrderInfo in m_CurrentRequiredInputOrderInfo)
                    {
                        DataConsumable inputOrder = inputOrderInfo.InputOrderType;
                        if (interactable.GetDataConsumable(inputOrder).Item1)
                        {
                            if (m_IsProcessing)
                            {
                                UpdateCurrentPendingInputOrdersToProcess(inputOrderInfo);
                            }
                            else
                            {
                                m_CurrentInputOrderInfosForProcessing.Add(inputOrderInfo); 
                            }
                            hasProvidedInputOrder = true;
                        }
                    }
                    ResetRequiredInputOrders();
                }

                if (!IsAnyPendingToProcessInputOrderAvailable() && !m_IsProcessing && hasProvidedInputOrder)
                {
                    m_StartProcessCoroutine = StartCoroutine(StartProcess());
                }

                if (IsAnyOutputOrderAvailable())
                {
                    foreach(OutputOrderInfo outputOrderInfo in m_CurrentOutputOrderInfos)
                    {
                        if (!outputOrderInfo.IsOccupied)
                            continue;

                        if (interactable.SendDataConsumable(outputOrderInfo.OutputOrderType, m_CostProperty))
                        {
                            ResetOutputOrderInfo(outputOrderInfo);
                            hasTakenOutputOrder = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (hasProvidedInputOrder || hasTakenOutputOrder)
                {
                    m_TaskTrigger.SendTaskResult(TaskResult.Success);
                }
                else
                {
                    m_TaskTrigger.SendTaskResult(TaskResult.Failed);
                    // GlobalEventHolder.OnHintByConsumable?.Invoke(m_OrderTypeIn);
                }
            }
            else
            {
                m_TaskTrigger.SendTaskResult(TaskResult.Failed);
            }
        }

        /* private void OnTaskStart(TaskTarget taskTarget)
        {
            if (taskTarget.TryGetComponent(out IEnvironmentInteractable interactable))
            {
                bool isTaskSuccessful = false;
                bool gotNextFoodType = false;

                List<StationOutInfo> stationOutInfos = m_StationOutInfo.FindAll((x) => x.IsHolding == true);

                if (stationOutInfos.Count <= 1 && m_IsProcessing == false)
                {
                    if (interactable.GetDataConsumable(m_OrderTypeIn).Item1)
                    {
                        gotNextFoodType = true;
                    }
                }

                foreach (var item in m_StationOutInfo)
                {
                    if (item.IsHolding)
                    {
                        if (interactable.SendDataConsumable(item.OrderOutType, m_CostProperty))
                        {
                            item.IsHolding = false;
                            item.OnOrderOutSuccesful?.Invoke();
                            isTaskSuccessful = true;
                            break;
                        }
                    }
                }

                if (gotNextFoodType)
                {
                    m_IsProcessing = true;
                    OnDurationStart?.Invoke(m_DurationProperty);
                    float totalDuration = m_DurationProperty + m_ExtraDurationForAnimation;
                    CoroutineManager.LateAction(() =>
                    {
                        OnDurationComplete?.Invoke();
                        int count = 0;
                        foreach (var item in m_StationOutInfo)
                        {
                            if (count < m_CapacityProperty)
                            {
                                item.IsHolding = true;
                                item.OnOrderInSuccesful?.Invoke();
                            }
                            count++;
                        }
                        m_IsProcessing = false;
                    }, totalDuration);
                    isTaskSuccessful = true;
                }
               

                if (isTaskSuccessful)
                {
                    m_TaskTrigger.SendTaskResult(TaskResult.Success);
                    return;
                }
                else
                {
                    GlobalEventHolder.OnHintByConsumable?.Invoke(m_OrderTypeIn);
                }
            }
            
            m_TaskTrigger.SendTaskResult(TaskResult.Failed);
        } */


        [Serializable]
        public class InputOrderInfo
        {
            public DataConsumable InputOrderType;
            public GameObject InputOrderDisplayObjPrefab;
            public DataConsumable OutputOrderType;
            public GameObject OutputOrderDisplayObjPrefab;
        }

        [Serializable]
        public class PendingInputOrderInfo
        {
            [ReadOnly, AllowNesting] public DataConsumable InputOrderType;
            [ReadOnly, AllowNesting] public GameObject InputOrderDisplayObjPrefab;
            [ReadOnly, AllowNesting] public GameObject InputOrderDisplayObj;
            [ReadOnly, AllowNesting] public DataConsumable OutputOrderType;
            [ReadOnly, AllowNesting] public GameObject OutputOrderDisplayObjPrefab;
            [ReadOnly, AllowNesting] public GameObject OutputOrderDisplayObj;
            [ReadOnly, AllowNesting] public bool IsOccupied = false;
            [SerializeField] Transform m_PendingInputOrderHolder;

            public Transform PendingInputOrderHolder => m_PendingInputOrderHolder;
        }

        [Serializable]
        public class OutputOrderInfo
        {
            [ReadOnly, AllowNesting] public DataConsumable InputOrderType;
            [ReadOnly, AllowNesting] public DataConsumable OutputOrderType;
            [ReadOnly, AllowNesting] public GameObject OutputOrderDisplayObjPrefab;
            [ReadOnly, AllowNesting] public GameObject OutputOrderDisplayObj;
            [ReadOnly, AllowNesting] public bool IsOccupied = false;
            [SerializeField] Transform m_OutputOrderHolder;

            public Transform OutputOrderHolder => m_OutputOrderHolder;
        }
    }
}
