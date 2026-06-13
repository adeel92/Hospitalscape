using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using DG.Tweening;
using NaughtyAttributes;
using Isometric.Cam;
using Isometric.Environment;
using Isometric.Worker;
using Isometric.Data;
using Isometric.Tutorial;
using Isometric.Sound;

namespace Isometric.UI
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager s_Instance;

        [SerializeField, Scene] string m_SceneName;
        [SerializeField] CanvasGroup m_CanvasGroup;
        [SerializeField] List<UIPopupBase> m_Popups;
        [SerializeField] WorkerUIManager m_WorkerUIManager;
        [SerializeField] float m_GameWonAndGameLostOpeningDelay;
        [SerializeField] RewardCollectionUIManager m_RewardCollectionUIManager;

        private EventSystem m_CurrentEventSystem;
        private StarItemInfo m_StarItemRewardInfo = null;

        private Queue<bool> m_MenuStamps = new Queue<bool>();
        private Queue<bool> m_CameraInteractionStamps = new Queue<bool>();

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }

        private void OnEnable()
        {
            GlobalEventHolder.OnGameWon += OnGameWon;
            GlobalEventHolder.OnGameLost += OnGameLost;

            GlobalEventHolder.OnOpenShop += OpenShopPopup;
        }

        private void OnDisable()
        {
            GlobalEventHolder.OnGameWon -= OnGameWon;
            GlobalEventHolder.OnGameLost -= OnGameLost;

            GlobalEventHolder.OnOpenShop -= OpenShopPopup;
        }

        public static void Setup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_CurrentEventSystem = EventSystem.current;

            foreach (var popup in s_Instance.m_Popups)
            {
                popup.Setup();
            }

            AchievementObserverUIManager.Setup();
        }

        #region Game Loop Execution
        public static void SetupForMenu()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            GameManager.SetupForMenu();
            UIInteractionOff();
            MenuUIManager menuUIManager = GetPopup<MenuUIManager>();
            if (menuUIManager != null)
            {
                menuUIManager.OpenPopup(() => { UIInteractionOn(); });
            }
            else
            {
                UIInteractionOn();
                Debug.LogWarning(nameof(MenuUIManager) + " is null");
            }
        }

        public static void CheckNextUpdatable()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            if (s_Instance.m_StarItemRewardInfo != null &&
                (s_Instance.m_StarItemRewardInfo.CoinReward > 0 ||
                s_Instance.m_StarItemRewardInfo.GemReward > 0))
            {
                SetRewardCollectionPopupMessage("Tap to Collect!", RewardCollectionTextPoistion.FarDownMiddle);
                OpenRewardCollectionPopup(s_Instance.m_StarItemRewardInfo.CoinReward > 0 ? s_Instance.m_StarItemRewardInfo.CoinReward : null,
                    s_Instance.m_StarItemRewardInfo.GemReward > 0 ? s_Instance.m_StarItemRewardInfo.GemReward : null,
                    null, null, null, null, null, () =>
                    {
                        SoundManager.PlaySound(SoundType.Reward);
                    },
                    CheckNextUpdatable);

                s_Instance.m_StarItemRewardInfo = null;
            }
            else if (GetPopup<StarItemUnlockingUIManager>() != null &&
                GetPopup<StarItemUnlockingUIManager>().CheckNextStarItemUnlockable())
            {
                CoroutineManager.LateAction(() =>
                {
                    HideMenu(() =>
                    {
                        GetPopup<StarItemUnlockingUIManager>().OpenPopup(() =>
                        {
                            UIInteractionOn();
                        });
                    });
                }, 0.5f);
            }
            else if (s_Instance.m_WorkerUIManager.CheckNextWorkerAndWorkerOrderUnlockable())
            {
                CoroutineManager.LateAction(() =>
                {
                    HideMenu(() =>
                    {
                        s_Instance.m_WorkerUIManager.OpenNewWorkerAndWorkerOrderPopup(() =>
                        {
                            UIInteractionOn();
                        });
                    });
                }, 0.5f);
            }
            else if (s_Instance.m_WorkerUIManager.CheckNextWorkerQuantityUpgrade())
            {
                CoroutineManager.LateAction(() =>
                {
                    HideMenu(() =>
                    {
                        s_Instance.m_WorkerUIManager.OpenWorkerQuantityUpgradePopup(() =>
                        {
                            UIInteractionOn();
                        });
                    });
                }, 0.5f);
            }
            else
            { 
                ShowMenu(() =>
                {
                    CameraController.Interactability(true);
                    UIInteractionOn();
                });
            }
        }


        public static void HasStarItemUnlocked(StarItemInfo starItemInfo)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            StarItemUnlockingUIManager starItemUnlockingUIManager = GetPopup<StarItemUnlockingUIManager>();
            if (starItemUnlockingUIManager != null)
            {
                s_Instance.m_StarItemRewardInfo = starItemInfo;
                UIInteractionOff();
                starItemUnlockingUIManager.ClosePopup(() =>
                {
                    UIInteractionOn();
                    EnvironmentManager.SetupForMenu();
                });
            }
            else
            {
                Debug.LogWarning(nameof(StarItemUnlockingUIManager) + " not found in the popup list");
            }
        }

        public static void HasNextWorkerAndWorkerOrderUnlocked()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UIInteractionOff();
            s_Instance.m_WorkerUIManager.CloseNewWorkerAndWorkerOrderPopup(() =>
            {
                UIInteractionOn();
                WorkerManager.Setup();
            });
        }

        public static void HasNextWorkerQuanityUpgrade()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UIInteractionOff();
            s_Instance.m_WorkerUIManager.CloseNewWorkerPopup(() =>
            {
                UIInteractionOn();
                WorkerManager.Setup();
            });
        }

        public static void SetupForGameplay()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            GameManager.SetupForGameplay();
            GameManager.PauseGame();

            GameplayUIManager gameplayUIManager = GetPopup<GameplayUIManager>();

            if (gameplayUIManager != null)
            {
                UIInteractionOff();
                if (GetPopup<PauseUIManager>() != null) GetPopup<PauseUIManager>().SetupForGameplay();
                else Debug.LogWarning(nameof(PauseUIManager) + " is null");
                if (GetPopup<BoosterUIManager>() != null) GetPopup<BoosterUIManager>().OpenPopup(null);
                else Debug.LogWarning(nameof(BoosterUIManager) + " is null");

                gameplayUIManager.OpenPopup(() =>
                {
                    UIInteractionOn();
                });
            }
            else
            {
                Debug.LogWarning(nameof(GameplayUIManager) + " not found in the popup list");
            }
        }

        public static void StartGame()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            GameplayUIManager gameplayUIManager = GetPopup<GameplayUIManager>();

            if (gameplayUIManager != null)
            {
                UIInteractionOff();
                gameplayUIManager.CloseGoalPopup(() =>
                {
                    UIInteractionOn();
                    GameManager.UnPauseGame();

                    TutorialManager.PlayTutorial(TutorialCallType.GameplayStart);
                });
            }
            else
            {
                Debug.LogWarning(nameof(GameplayUIManager) + " not found in the popup list");
            }
        }

        private void OnGameWon()
        {
            GameWonUIManager gameWonUIManager = GetPopup<GameWonUIManager>();
            if (gameWonUIManager != null)
            {
                CoroutineManager.LateAction(() =>
                {
                    GameManager.PauseGame();
                    UIInteractionOff();
                    gameWonUIManager.OpenPopup(() => { UIInteractionOn(); });
                }, m_GameWonAndGameLostOpeningDelay);
            }
            else
            {
                Debug.LogWarning(nameof(GameWonUIManager) + " not found in the popup list");
            }
        }

        private void OnGameLost()
        {
            GameLostUIManager gameLostUIManager = GetPopup<GameLostUIManager>();
            if (gameLostUIManager != null)
            {
                CoroutineManager.LateAction(() =>
                {
                    GameManager.PauseGame();
                    UIInteractionOff();
                    gameLostUIManager.OpenPopup(() => { UIInteractionOn(); });
                }, m_GameWonAndGameLostOpeningDelay);
            }
            else
            {
                Debug.LogWarning(nameof(GameLostUIManager) + " not found in the popup list");
            }
        }
        #endregion

        #region Helper

        #region Upgrade Popup
        public static void OpenUpgradePopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UpgradePopupUIManager upgradePopupUIManager = GetPopup<UpgradePopupUIManager>();
            if (upgradePopupUIManager != null)
            {
                UIInteractionOff();
                CameraController.Interactability(false);
                CameraController.SetupForGameplay(() =>
                {
                    upgradePopupUIManager.OpenPopup(() =>
                    {
                        UIInteractionOn();
                        TutorialManager.PlayTutorial(TutorialCallType.AfterUpdatePopup);
                    });
                });
            }
            else
            {
                Debug.LogWarning(nameof(UpgradePopupUIManager) + " not found in the popup list");
            }
        }

        public static void CloseUpgradePopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UpgradePopupUIManager upgradePopupUIManager = GetPopup<UpgradePopupUIManager>();
            if (upgradePopupUIManager != null)
            {
                UIInteractionOff();
                upgradePopupUIManager.ClosePopup(() =>
                {
                    UIInteractionOn();
                    SetupForMenu();
                });
            }
            else
            {
                Debug.LogWarning(nameof(UpgradePopupUIManager) + " not found in the popup list");
            }
        }
        #endregion

        #region Menu Popup
        public static void ShowMenu(Action onComplete)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            OpenPopup<MenuUIManager>(onComplete);
        }

        public static void HideMenu(Action onComplete)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            ClosePopup<MenuUIManager>(onComplete);

        }
        #endregion

        #region Achievement Popup
        /*public static void OpenAchievementPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            CameraInteractionOff();
            UIInteractionOff();
            HideMenu(() =>
            {
                s_Instance.m_AchievementUIManager.OpenPopup(() =>
                {
                    UIInteractionOn();
                });
            });
        }*/

        /*public static void CloseAchievementPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UIInteractionOff();
            s_Instance.m_AchievementUIManager.ClosePopup(() =>
            {
                ShowMenu(() =>
                {
                    UIInteractionOn();
                    CameraInteractionOn();
                });
            });
        }*/
        #endregion

        #region UI Interaction
        /// <summary>
        /// This Blocks the UI events under UIManager as parent
        /// </summary>
        public static void UIInteractionOff()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_CanvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// This Unlock the UI events under UIManager as parent
        /// </summary>
        public static void UIInteractionOn()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_CanvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// This Blocks all the UI events using EventSystem
        /// </summary>
        public static void FullUIInteractionOff()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_CurrentEventSystem.enabled = false;
        }

        /// <summary>
        /// This Unlocks all the UI events using EventSystem
        /// </summary>
        public static void FullUIInteractionOn()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_CurrentEventSystem.enabled = true;
        }
        #endregion

        #region Camera Interaction
        public static void CameraInteractionOn()
        {
            CameraController.Interactability(true);
        }

        public static void CameraInteractionOff()
        {
            CameraController.Interactability(false);
        }

        public static void CameraEnvironmentInteractionOff()
        {
            CameraController.SetEnvironemntInteractiblity(false);
        }

        public static void CameraEnvironmentInteractionOn()
        {
            CameraController.SetEnvironemntInteractiblity(true);
        }
        #endregion

        #region Game Pause
        public static void PauseGame()
        {
            GameManager.PauseGame();
        }

        public static void UnPauseGame()
        {
            GameManager.UnPauseGame();
        }
        #endregion

        #region Settings Popup
        /*public static void OpenSettingPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UIInteractionOff();
            CameraInteractionOff();
            s_Instance.m_SettingUIManager.OpenPopup(() =>
            {
                UIInteractionOn();
            });
        }

        public static void CloseSettingPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UIInteractionOff();
            s_Instance.m_SettingUIManager.ClosePopup(() =>
            {
                CameraInteractionOn();
                UIInteractionOn();
            });
        }*/
        #endregion

        #region Shop Popup
        private void OpenShopPopup(bool isOnMenu)
        {
            ShopUIManager shopUIManager = GetPopup<ShopUIManager>();
            if (shopUIManager != null)
            {
                OpenPopup<ShopUIManager>(isOnMenu, true);
            }
            else
            {
                Debug.LogWarning(nameof(ShopUIManager) + " not found in the popup list");
            }
        }

        private void CloseShopPopup()
        {
            ShopUIManager shopUIManager = GetPopup<ShopUIManager>();
            if (shopUIManager != null)
            {
                ClosePopup<ShopUIManager>();
            }
            else
            {
                Debug.LogWarning(nameof(ShopUIManager) + " not found in the popup list");
            }
        }
        #endregion

        #region Level Chance Popup
        public static void OpenLevelChancePopup(LevelGoalType levelGoalType, int goalTargetValue, int goalCurrentValue, LevelLostReason levelLostReason)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            LevelChanceUIManager levelChanceUIManager = GetPopup<LevelChanceUIManager>();
            if (levelChanceUIManager != null)
            {
                PauseGame();
                UIInteractionOff();
                levelChanceUIManager.OpenChancePopup(levelGoalType, goalTargetValue, goalCurrentValue, levelLostReason, () =>
                {
                    UIInteractionOn();
                });
            }
            else
            {
                Debug.LogWarning(nameof(LevelChanceUIManager) + " not found in the popup list");
            }
        }

        public static void CloseLevelChancePopupAfterAvailed()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            LevelChanceUIManager levelChanceUIManager = GetPopup<LevelChanceUIManager>();
            if (levelChanceUIManager != null)
            {
                UIInteractionOff();
                levelChanceUIManager.CloseChancePopup(() =>
                {
                    UnPauseGame();
                    UIInteractionOn();
                });
            }
            else
            {
                Debug.LogWarning(nameof(LevelChanceUIManager) + " not found in the popup list");
            }
        }

        public static void CloseLevelChancePopupAfterNotAvailed()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            LevelChanceUIManager levelChanceUIManager = GetPopup<LevelChanceUIManager>();
            if (levelChanceUIManager != null)
            {
                UIInteractionOff();
                levelChanceUIManager.CloseChancePopup(() =>
                {
                    UnPauseGame();
                    UIInteractionOn();
                    LevelManager.SetToGameOver();
                });
            }
            else
            {
                Debug.LogWarning(nameof(LevelChanceUIManager) + " not found in the popup list");
            }
        }

        public static bool CanAcquireDoNotLoseCustomerLevelChance()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return false;
            }

            LevelChanceUIManager levelChanceUIManager = GetPopup<LevelChanceUIManager>();
            if (levelChanceUIManager != null)
            {
                return levelChanceUIManager.CanAcquireDoNotLoseCustomerLevelChance();

            }
            else
            {
                Debug.LogWarning(nameof(LevelChanceUIManager) + " not found in the popup list");
                return false;
            }
        }
        #endregion

        #region Star Item Popup
        /*public static void OpenStarItemPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UIInteractionOff();
            CameraInteractionOff();
            HideMenu(() =>
            {
                s_Instance.m_StarItemUnlockingUIManager.OpenPopup(() =>
                {
                    UIInteractionOn();
                });
            });
        }*/

        /*public static void CloseStarItemPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UIInteractionOff();
            s_Instance.m_StarItemUnlockingUIManager.ClosePopup(() =>
            {
                ShowMenu(() =>
                {
                    UIInteractionOn();
                    CameraInteractionOn();
                });
            });
        }*/
        #endregion

        #region Reward Collection
        public static void SetRewardCollectionPopupMessage(string message, RewardCollectionTextPoistion textPostion)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_RewardCollectionUIManager.SetTextMessage(message, textPostion);
        }

        public static void OpenRewardCollectionPopup(int? coins,
            int? gems,
            double? heartTime,
            int? timeFrozeBooster,
            int? instanceOrderFillBooster,
            int? waitressSpeedBooster,
            Action onComplete,
            Action onCollect,
            Action onCloseComplete)
        
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UIInteractionOff();
            s_Instance.m_RewardCollectionUIManager.Open(coins,
                gems,
                heartTime,
                timeFrozeBooster,
                instanceOrderFillBooster,
                waitressSpeedBooster,
                () =>
                {
                    UIInteractionOn();
                    onComplete?.Invoke();
                },
                onCollect,
                onCloseComplete);
        }

        public static void CloseRewardCollectionPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UIInteractionOff();
            s_Instance.m_RewardCollectionUIManager.ClosePopup(() =>
            {
                UIInteractionOn();
            });
        }
        #endregion

        #region Game Restart
        public static void RestartGame()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            GameManager.UnPauseGame();
            DOTween.KillAll();
            CoroutineManager.StopAllCoroutine();
            SceneManager.LoadScene(s_Instance.m_SceneName);
        }
        #endregion

        #region Profile Popup
        /*public static void OpenProfilePopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            CameraInteractionOff();
            UIInteractionOff();
            HideMenu(() =>
            {
                s_Instance.m_ProfileUIManager.OpenPopup(() =>
                {
                    UIInteractionOn();
                });
            });
        }

        public static void CloseProfilePopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UIInteractionOff();
            s_Instance.m_ProfileUIManager.ClosePopup(() =>
            {
                ShowMenu(() =>
                {
                    UIInteractionOn();
                    CameraInteractionOn();
                });
            });
        }*/
        #endregion

        #region Map Popup
        /*public static void OpenMapPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            CameraInteractionOff();
            UIInteractionOff();
            s_Instance.m_MapUIManager.OpenPopup(() =>
            {
                UIInteractionOn();
            });
        }*/

        /*public static void CloseMapPopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UIInteractionOff();
            s_Instance.m_MapUIManager.ClosePopup(() =>
            {
                UIInteractionOn();
                CameraInteractionOn();
            });
        }*/
        #endregion

        #region Gameplay
        /*public static void SetActivateHeatSymbol(bool value)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            GameplayUIManager gameplayUIManager = GetPopup<GameplayUIManager>();
            if (gameplayUIManager != null)
            {
                gameplayUIManager.SetActiveHeatSymbol(value);
            }
            else
            {
                Debug.LogWarning(typeof(GameplayUIManager) + " not found in the popup list");
            }
        }*/
        #endregion

        #region Vending Machine Popup
        /*public static void OpenVendingMachinePopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            CameraInteractionOff();
            UIInteractionOff();
            HideMenu(() =>
            {
                s_Instance.m_VendingMachineUIManager.OpenPopup(() =>
                {
                    UIInteractionOn();
                });
            });
        }*/

        /*public static void CloseVendingMachinePopup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            UIInteractionOff();
            s_Instance.m_VendingMachineUIManager.ClosePopup(() =>
            {
                ShowMenu(() =>
                {
                    UIInteractionOn();
                    CameraInteractionOn();
                });
            });
        }*/

        /*public static VendingMachineUIManager GetVendingMachine()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return null;
            }

            return s_Instance.m_VendingMachineUIManager;
        }*/
        #endregion

        #endregion

        #region Helper
        public static T GetPopup<T>() where T : UIPopupBase
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return default;
            }

            return s_Instance.m_Popups.Find(popup => popup is T) as T;
        }

        private static void OpenPopup<T>(Action onComplete) where T : UIPopupBase
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            T targetPopup = s_Instance.m_Popups.Find(popup => popup is T) as T;
            if (targetPopup != null)
            {
                targetPopup.OpenPopup(onComplete);
            }
            else
            {
                onComplete?.Invoke();
                Debug.LogWarning(nameof(T) + " not found in the popup list");
            }
        }

        private static void ClosePopup<T>(Action onComplete) where T : UIPopupBase
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            T targetPopup = s_Instance.m_Popups.Find(popup => popup is T) as T;
            if (targetPopup != null)
            {
                targetPopup.ClosePopup(onComplete);
            }
            else
            {
                onComplete?.Invoke();
                Debug.LogWarning(nameof(T) + " not found in the popup list");
            }
        }

        public static void OpenPopup<T>(bool hideMenu, bool lockCameraInteraction) where T : UIPopupBase
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            T targetPopup = s_Instance.m_Popups.Find(popup => popup is T) as T;
            if (targetPopup != null)
            {
                s_Instance.m_MenuStamps.Enqueue(hideMenu);
                s_Instance.m_CameraInteractionStamps.Enqueue(lockCameraInteraction);
                if (lockCameraInteraction)
                {
                    CameraInteractionOff();
                }

                UIInteractionOff();

                if (hideMenu)
                {
                    HideMenu(() =>
                    {
                        targetPopup.OpenPopup(() =>
                        {
                            UIInteractionOn();
                        });
                    });
                }
                else
                {
                    targetPopup.OpenPopup(() => 
                    {
                        UIInteractionOn();
                    });
                }
            }
            else
            {
                Debug.LogWarning(nameof(T) + " not found in the popup list");
            }
        }


        public static void ClosePopup<T>() where T : UIPopupBase
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            T targetPopup = s_Instance.m_Popups.Find(popup => popup is T) as T;
            if (targetPopup != null)
            {
                bool showMenu = s_Instance.m_MenuStamps.Dequeue();
                bool isMenuLastStamp = s_Instance.m_MenuStamps.Count == 0;

                bool unlockCameraInteraction = s_Instance.m_CameraInteractionStamps.Dequeue();
                bool isCameraLastStamp = s_Instance.m_CameraInteractionStamps.Count == 0;

                UIInteractionOff();
                targetPopup.ClosePopup(() =>
                {
                    if (showMenu && isMenuLastStamp)
                    {
                        ShowMenu(() =>
                        {
                            if (unlockCameraInteraction && isCameraLastStamp)
                            {
                                CameraInteractionOn();
                            }
                            UIInteractionOn();
                        });
                    }
                    else
                    {
                        if (unlockCameraInteraction && isCameraLastStamp)
                        {
                            CameraInteractionOn();
                        }
                        UIInteractionOn();
                    }
                });
            }
            else
            {
                Debug.LogWarning(nameof(T) + " not found in the popup list");
            }
        }
        #endregion

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(UIManager) + " is null");
        }
    }
}
