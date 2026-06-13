using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.PathSystem;
using Isometric.Data;
using Isometric.Environment;
using Isometric.UI;
using Isometric.Sound;

namespace Isometric.Worker
{
    public class WorkerManager : MonoBehaviour
    {
        private static WorkerManager s_Instance;

        [Header("---Worker Testing---")]
        [SerializeField] bool m_TestWorker = false;
        [ShowIf(nameof(m_TestWorker)), SerializeField] MainServiceController m_TestMainServiceController;
        [ShowIf(nameof(m_TestWorker)), SerializeField] DataConsumable m_TestOrderTypeData;

        [Header("---Worker---")]
        [SerializeField] float m_WorkerOrderUpgradeDuration = 1.5f;
        [SerializeField] List<WorkerOrderInfo> m_WorkerOrderInfos;
        [SerializeField] PathNode m_StartNode;

        [Header("---Door---")]
        [SerializeField] PathNode m_DoorNode;
        [SerializeField] DoorAnimatorController m_DoorController;

        [SerializeField, ReadOnly] 
        private List<WorkerController> m_WorkerCloseToTheDoor;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                m_WorkerCloseToTheDoor = new List<WorkerController>();
            }
        }

        public static void Setup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            foreach (var WorkerOrderInfo in s_Instance.m_WorkerOrderInfos)
            {
                WorkerOrderData temWorkerOrderData = WorkerOrderInfo.Data.WorkerOrderData;
                if (temWorkerOrderData.IsUnlocked)
                {
                    WorkerOrderInfo.IsUnlocked = true;
                    WorkerOrderInfo.OnUnlocked?.Invoke();


                    if (temWorkerOrderData.HasJustUnlocked)
                    {
                        WorkerOrderInfo.OnJustUnlocked?.Invoke();

                        temWorkerOrderData.HasJustUnlocked = false;
                        WorkerOrderInfo.Data.Save();

                        CoroutineManager.LateAction(() =>
                        {
                            UIManager.CheckNextUpdatable();
                        }, s_Instance.m_WorkerOrderUpgradeDuration);
                    }
                    else if (temWorkerOrderData.HasJustUpgraded)
                    {
                        WorkerOrderInfo.OnUpgraded?.Invoke();

                        temWorkerOrderData.HasJustUpgraded = false;
                        WorkerOrderInfo.Data.Save();

                        CoroutineManager.LateAction(() =>
                        {
                            UIManager.CheckNextUpdatable();
                        }, s_Instance.m_WorkerOrderUpgradeDuration);
                    }

                    int quantityIndex = temWorkerOrderData.WorkerQunaityUpgradeIndex;
                    int workerQuantity = temWorkerOrderData.WorkerQunaityUpgrades[quantityIndex].Quantity;

                    int workerQuantityCounter = 0;
                    foreach (var workerInfo in WorkerOrderInfo.WorkerInfos)
                    {
                        if (workerQuantity > workerQuantityCounter)
                        {
                            workerInfo.IsUnlocked = true;
                            workerInfo.OrderCost = temWorkerOrderData.OrderCost;
                            workerInfo.ServingDuration = s_Instance.GetWorkerServingDuration(temWorkerOrderData);
                            workerInfo.OnIsUnlocked?.Invoke();
                            workerInfo.WorkerController.Setup(s_Instance.m_StartNode, workerInfo.ServingDuration, workerInfo.OrderCost);
                        }
                        else
                        {
                            workerInfo.OnLocked?.Invoke();
                            workerInfo.IsUnlocked = false;
                        }
                        workerQuantityCounter++;
                    }

                }
                else
                {
                    WorkerOrderInfo.IsUnlocked = false;
                    WorkerOrderInfo.OnLocked?.Invoke();
                }
            }
        }

        public static bool Serve(MainServiceController mainServiceSeat, DataConsumable orderType)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return false;
            }

            WorkerOrderInfo workerOrderInfo = s_Instance.m_WorkerOrderInfos.Find((x) => x.OrderType == orderType);
            if (workerOrderInfo.IsUnlocked)
            {
                foreach (var workerInfo in workerOrderInfo.WorkerInfos)
                {
                    if (workerInfo.IsUnlocked && !workerInfo.IsBusy)
                    {
                        SoundManager.PlaySound(SoundType.WorkerBell);

                        workerInfo.IsBusy = true;
                        workerInfo.OnIsBusy?.Invoke();
                        workerInfo.WorkerController.Serve(mainServiceSeat);
                        WorkerEnteringSalon(workerInfo.WorkerController);

                        SetWorkerAvailabiy(workerOrderInfo.WorkerInfos, workerOrderInfo.OrderType);

                        return true;
                    }
                }
            }

            return false;
        }

        //mrcHefF
        public static bool Serve(SalonChairController salonChair, DataConsumable orderType)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return false;
            }

            WorkerOrderInfo workerOrderInfo = s_Instance.m_WorkerOrderInfos.Find((x) => x.OrderType == orderType);
            if (workerOrderInfo.IsUnlocked)
            {
                foreach (var workerInfo in workerOrderInfo.WorkerInfos)
                {
                    if (workerInfo.IsUnlocked && !workerInfo.IsBusy)
                    {
                        SoundManager.PlaySound(SoundType.WorkerBell);

                        workerInfo.IsBusy = true;
                        workerInfo.OnIsBusy?.Invoke();
                        workerInfo.WorkerController.Serve(salonChair);
                        WorkerEnteringSalon(workerInfo.WorkerController);

                        SetWorkerAvailabiy(workerOrderInfo.WorkerInfos, workerOrderInfo.OrderType);

                        return true;
                    }
                }
            }

            return false;

        }

        public static void ServeComplete(WorkerController workerController)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            foreach (var workerOrderInfo in s_Instance.m_WorkerOrderInfos)
            {
                foreach (var workerInfo in workerOrderInfo.WorkerInfos)
                {
                    if (workerInfo.WorkerController == workerController && workerInfo.IsBusy)
                    {
                        WorkerLeftSalon(workerController);
                        workerInfo.IsBusy = false;
                        workerInfo.OnIsNotBusy?.Invoke();

                        SetWorkerAvailabiy(workerOrderInfo.WorkerInfos, workerOrderInfo.OrderType);

                        return;
                    }
                }
            }
        }

        public static void WorkerEnteringSalon(WorkerController workerController)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_WorkerCloseToTheDoor.Add(workerController);
            s_Instance.m_DoorController.OpenDoor();
        }

        public static void WorkerEnteredSalon(WorkerController workerController, PathNode enteredNode)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            if (enteredNode == s_Instance.m_DoorNode)
            {
                s_Instance.m_WorkerCloseToTheDoor.Remove(workerController);
                if (s_Instance.m_WorkerCloseToTheDoor.Count <= 0)
                {
                    s_Instance.m_DoorController.CloseDoor();
                }
            }
        }

        public static void WorkerLeavingSalon(WorkerController workerController, PathNode leaveingNode)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            if (leaveingNode == s_Instance.m_DoorNode)
            {
                s_Instance.m_WorkerCloseToTheDoor.Add(workerController);
                s_Instance.m_DoorController.OpenDoor();
            }
        }

        public static void WorkerLeftSalon(WorkerController workerController)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_WorkerCloseToTheDoor.Remove(workerController);
            if (s_Instance.m_WorkerCloseToTheDoor.Count <= 0)
            {
                s_Instance.m_DoorController.CloseDoor();
            }
        }

        private float GetWorkerServingDuration(WorkerOrderData workerOrderData)
        {
            int currentMapLevelDifficlty = DataManager.CurrentMapLevelDifficulty;
            float duration = 4;

            foreach (var workerServingDurationUpgrade in workerOrderData.WorkerServingDurationUpgrades)
            {
                if (workerServingDurationUpgrade.LevelDifficultyRangeMin <= currentMapLevelDifficlty
                    && workerServingDurationUpgrade.LevelDifficultyRangeMax >= currentMapLevelDifficlty)
                {
                    int diffiernceInDifficulty = currentMapLevelDifficlty - workerOrderData.WorkerCurrentLevelDifficulty;
                    if (diffiernceInDifficulty >= 0 && diffiernceInDifficulty < workerServingDurationUpgrade.LevelDifficultyServingDurations.Count)
                    {
                        duration = workerServingDurationUpgrade.LevelDifficultyServingDurations[diffiernceInDifficulty].WorkerServingDuration;
                        break;
                    }
                }
            }

            return duration;
        }

        private static void SetWorkerAvailabiy(List<WorkersInfo> workersInfo, DataConsumable orderType)
        {
            List<WorkersInfo> unlockedWorkers = workersInfo.FindAll((x) => x.IsUnlocked);
            bool areAllBusy = unlockedWorkers.TrueForAll((x) => x.IsBusy);
            if (areAllBusy)
            {
                GlobalEventHolder.OnAllWorkerBusy?.Invoke(orderType);
            }
            else
            {
                GlobalEventHolder.OnNotAllWorkerBusy?.Invoke(orderType);
            }
        }

        public static float GetWorkerServingDuration(DataConsumable orderType)
        {
            foreach (var workerOrderInfo in s_Instance.m_WorkerOrderInfos)
            {
                if (workerOrderInfo.OrderType == orderType)
                {
                    foreach (var workerInfo in workerOrderInfo.WorkerInfos)
                    {
                        if (workerInfo.IsUnlocked)
                        {
                            return workerInfo.ServingDuration;
                        }
                    }
                }
            }

            return 0;
        }

        public static Transform GetWorkerManagerTransform()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return null;
            }

            return s_Instance.transform;
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(WorkerManager) + " is null");
        }

        #region Testing
        [ContextMenu("TestServe")]
        private void TestServe()
        {
            if (!m_TestWorker)
            {
                Debug.LogWarning("Couldn't execute TestServe because TestWorker is not enabled!");
                return;
            }

            Serve(m_TestMainServiceController, m_TestOrderTypeData);
        }

        [ContextMenu("TestSetup")]
        private void TestSetup()
        {
            Setup();
        }
        #endregion
    }


    [Serializable]
    public class WorkerOrderInfo
    {
        public DataWorker Data;
        [AllowNesting, ReadOnly]
        public bool IsUnlocked;
        public DataConsumable OrderType;
        public UnityEvent OnLocked;
        public UnityEvent OnUnlocked;
        public UnityEvent OnJustUnlocked;
        public UnityEvent OnUpgraded;
        public List<WorkersInfo> WorkerInfos;
    }

    [Serializable]
    public class WorkersInfo
    {
        [AllowNesting, ReadOnly]
        public bool IsUnlocked;
        [AllowNesting, ReadOnly]
        public float ServingDuration;
        [AllowNesting, ReadOnly]
        public int OrderCost;

        public UnityEvent OnIsUnlocked;
        public UnityEvent OnLocked;
        public WorkerController WorkerController;
        public UnityEvent OnIsBusy;
        public UnityEvent OnIsNotBusy;
        [AllowNesting, ReadOnly]
        public bool IsBusy;
    }
}
