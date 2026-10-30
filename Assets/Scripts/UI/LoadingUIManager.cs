using System;
using UnityEngine;
using Arc;
using Isometric.Sound;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using Isometric.Cam;
using NaughtyAttributes;

namespace Isometric.UI
{
    public class LoadingUIManager : MonoBehaviour
    {
        private static LoadingUIManager s_Instance;
        
        [Header("---Popup---")]
        [SerializeField] GameObject m_Popup;
        [SerializeField] Camera m_LoadingCamera;
        [SerializeField] PlayDoTweenSequence m_OpeningSequence;
        [SerializeField] PlayDoTweenSequence m_ClosingSequence;
        [SerializeField] Image m_LoadingBarFillImage;
        [SerializeField] float m_LoadingDuration = 4f;
        [SerializeField] float m_CharacterDisplayDelay = 0.5f;
        
        [Header("---Character---")]
        [SerializeField] Vector2 m_CharacterStartAnchorPosition;
        [SerializeField] PlayDoTween m_CharacterHolderTween;

        [ReadOnly, Space, SerializeField] bool m_IsLoadingScreenActive = false;
        [ReadOnly, Space, SerializeField] bool m_IsSceneLoadingInProcess = false;

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static void Setup()
        {
        }

        /// <summary>
        /// Opens a loading screen and loads the specified scene after completing an inside loading duration.
        /// </summary>
        public static void LoadScene(string sceneName)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            if (s_Instance.m_IsLoadingScreenActive)
                return;

            s_Instance.StartCoroutine(s_Instance.LoadSceneRoutine(sceneName));
        }
        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            AsyncOperation loadingOperation = null;
            m_IsSceneLoadingInProcess = true;
            m_IsLoadingScreenActive = true;
            GameManager.PauseGame();
            CameraController.SetEnvironemntInteractiblity(false);

            Debug.Log($"ADEEL LOADING 1");

            OpenPopup(() =>
            {
                Debug.Log($"ADEEL LOADING 3");
                Debug.Log($"ADEEL LOADING 4");
                // TweenDiagnostics.LogActiveTweens("Before Kill");
                DOTween.KillAll();
                // TweenDiagnostics.LogActiveTweens("After Kill");
                loadingOperation = SceneManager.LoadSceneAsync(sceneName);
            });

            Debug.Log($"ADEEL LOADING 2");

            while (loadingOperation == null || !loadingOperation.isDone)
            {
                Debug.Log($"ADEEL LOADING 5");
                yield return null;
            }

            yield return new WaitUntil(() => InternetUIManager.IsInternetAvailable());

            DOTween.KillAll();
            GameManager.UnPauseGame();
            m_IsSceneLoadingInProcess = false;
            // TweenDiagnostics.LogActiveTweens("After Scene Load");

            Debug.Log($"ADEEL LOADING 6");
            // Debug.Log($"ADEEL LOADING 6 (timescale): {Time.timeScale}");
            // Time.timeScale = 1f;    // mrcHefF
            m_LoadingBarFillImage.DOFillAmount(1f, m_LoadingDuration).OnComplete(() =>
            {
                Debug.Log($"ADEEL LOADING 8");
                ClosePopup(() =>
                {
                    CameraController.SetEnvironemntInteractiblity(true);
                });
            });
            Debug.Log($"ADEEL LOADING 7");
            // s_Instance.StartCoroutine(s_Instance.ShowCharacter());
        }

        /* private IEnumerator LoadSceneRoutine(string sceneName)
        {
            m_IsLoading = true;

            CameraController.SetEnvironemntInteractiblity(false);

            yield return null;

            TweenDiagnostics.LogActiveTweens("Before Kill");

            DOTween.KillAll();

            TweenDiagnostics.LogActiveTweens("After Kill");

            SceneManager.LoadScene(sceneName);

            m_IsLoading = false;
        } */

        /// <summary>
        /// Opens a loading screen with opened callback and closes it automatically after an inside loading duration followed by a closed callback.
        /// </summary>
        public static void ShowLoadingScreen(Action openedCallback, Action closedCallback)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            if (s_Instance.m_IsLoadingScreenActive)
                return;

            s_Instance.m_IsLoadingScreenActive = true;
            OpenPopup(() =>
            {
                s_Instance.m_LoadingBarFillImage.DOFillAmount(1f, s_Instance.m_LoadingDuration).OnComplete(() =>
                {
                    ClosePopup(() =>
                    {
                        // s_Instance.HideCharacter();
                        closedCallback?.Invoke();
                    });
                });
                // s_Instance.StartCoroutine(s_Instance.ShowCharacter());
                openedCallback?.Invoke();
            });
        }

        public static void OpenPopup(Action callback)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_LoadingCamera.gameObject.SetActive(true);
            s_Instance.m_LoadingBarFillImage.fillAmount = 0f;
            s_Instance.m_CharacterHolderTween.GetComponent<RectTransform>().anchoredPosition = s_Instance.m_CharacterStartAnchorPosition;
            UIManager.UIInteractionOff();
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            s_Instance.m_Popup.SetActive(true);
            s_Instance.m_OpeningSequence.PlaySequence(() =>
            {
                callback?.Invoke();
            });
        }

        private IEnumerator ShowCharacter()
        {
            if(m_CharacterHolderTween != null)
            {
                yield return new WaitForSeconds(m_CharacterDisplayDelay);
                m_CharacterHolderTween.Play();
            }
        }
        private void HideCharacter()
        {
            if(m_CharacterHolderTween != null)
            {
                m_CharacterHolderTween.Stop();
            }
        }

        public static void ClosePopup(Action callback)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            SoundManager.PlaySound(SoundType.PopupWhoosh);
            s_Instance.m_ClosingSequence.PlaySequence(() =>
            {
                s_Instance.m_Popup.SetActive(false);
                UIManager.UIInteractionOn();
                s_Instance.m_IsLoadingScreenActive = false;
                s_Instance.m_LoadingCamera.gameObject.SetActive(false);
                callback?.Invoke();
            });
        }

        public static bool IsLoadingScreenActive()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return false;
            }

            return s_Instance.m_IsLoadingScreenActive;
        }
        public static bool IsSceneLoadingInProcess()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return false;
            }

            return s_Instance.m_IsSceneLoadingInProcess;
        }
        
        public static float GetLoadingDuration()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return 0f;
            }

            return s_Instance.m_LoadingDuration;
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(LoadingUIManager) + " is null");
        }
    }
}
