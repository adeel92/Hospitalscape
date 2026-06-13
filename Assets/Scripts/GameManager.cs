using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Isometric.Environment;
using Isometric.Player;
using Isometric.Worker;
using Isometric.Customer;
using Isometric.UI;
using Isometric.Cam;
using Isometric.Data;
using Isometric.Sound;
using Isometric.Tutorial;

namespace Isometric
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager s_Instance;

        private void Awake()
        {
            if (s_Instance == null)
            {
                Application.targetFrameRate = 60;
                s_Instance = this;
            }
        }

        private void Start()
        {
            Setup();
        }

        public static void Setup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            //Order Matters
            LevelManager.Setup();
            EnvironmentManager.SetupForMenu();
            CameraController.SetEnvironemntInteractiblity(false);
            UIManager.Setup();
            WorkerManager.Setup();
            PlayerManager.SetupForMenu();

            UIManager.UIInteractionOff();
            CameraController.Interactability(false);
            CameraController.SetupForMenu(() =>
            {
                if (!TutorialManager.PlayTutorial(TutorialCallType.BeforeNextUpdatable))
                {
                    UIManager.CheckNextUpdatable();
                }
            });

            SoundManager.SetMusic(DataManager.GetBool(SoundCategroy.Music.ToString(), true));
            SoundManager.SetSound(DataManager.GetBool(SoundCategroy.Sound.ToString(), true));

            SoundManager.StopSound(SoundType.GameMusic1);
            SoundManager.PlaySound(SoundType.MenuMusic1, true, false);

            //GameManager.SetupForGameplay();
        }

        public static void SetupForMenu()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            //Order Matters
            EnvironmentManager.SetupForMenu();
            PlayerManager.SetupForMenu();
            WorkerManager.Setup();

            CameraController.Interactability(false);
            CameraController.SetupForMenu(() => 
            {

                CameraController.Interactability(true);
            });
        }

        public static void SetupForGameplay()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            //Order Matters
            LevelManager.SetupForGameplay();
            EnvironmentManager.SetupForGameplay();
            CustomerManager.Setup();
            CustomerPatienceManager.Setup();
            PlayerManager.SetupForGameplay();
            CameraController.SetEnvironemntInteractiblity(true);
            

            SoundManager.StopFadeOut(SoundType.MenuMusic1, 0.3f, false);
            SoundManager.PlaySoundFadeIn(SoundType.GameMusic1, 0.3f, true, false);

            int level = DataManager.CurrentMapLevelIndex + 1;
            FirebaseManager.LogEvent("Level_" + level + "_", FirebaseLogType.GameStart);
        }

        public static void PauseGame()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            GlobalEventHolder.OnGameplayPause?.Invoke();

            Time.timeScale = 0;
        }

        public static void UnPauseGame()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            GlobalEventHolder.OnGameplayUnause?.Invoke();

            Time.timeScale = 1;
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                GameManager.UnPauseGame();
                DOTween.KillAll();
                CoroutineManager.StopAllCoroutine();
                UnityEngine.SceneManagement.SceneManager.LoadScene(gameObject.scene.name);
            }

            if(Input.GetKeyDown(KeyCode.Z))
            {
                Time.timeScale = 1;
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                Time.timeScale = 2;
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                Time.timeScale = 4;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Application.targetFrameRate = 10;
            }
            
            if (Input.GetKeyDown(KeyCode.W))
            {
                Application.targetFrameRate = 60;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                if (Time.timeScale > 0)
                {
                    PauseGame();
                }
                else
                {
                    UnPauseGame();
                }
            }
        }
#endif

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(GameManager) + " is null");
        }
    }
   
}


namespace Isometric
{
    public enum CurrencyType
    {
        Level, Star, Coin, Gem, Life
    }
}