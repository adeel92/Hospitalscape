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

        [ReadOnly, Space, SerializeField] bool m_IsLoading = false;

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

        public static void LoadScene(string sceneName)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            if (s_Instance.m_IsLoading)
                return;

            s_Instance.StartCoroutine(s_Instance.LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            AsyncOperation loadingOperation = null;
            m_IsLoading = true;
            Debug.Log($"ADEEL LOADING 1");
            OpenPopup(() =>
            {
                Debug.Log($"ADEEL LOADING 3");
                DOTween.KillAll();
                Debug.Log($"ADEEL LOADING 4");
                loadingOperation = SceneManager.LoadSceneAsync(sceneName);
            });

            Debug.Log($"ADEEL LOADING 2");
            yield return null;

            while (loadingOperation == null || !loadingOperation.isDone)
            {
                Debug.Log($"ADEEL LOADING 5");
                yield return null;
            }

            Debug.Log($"ADEEL LOADING 6");
            m_LoadingBarFillImage.DOFillAmount(1f, m_LoadingDuration).OnComplete(() =>
            {
                Debug.Log($"ADEEL LOADING 8");
                ClosePopup(null);
            });
            Debug.Log($"ADEEL LOADING 7");
            yield return new WaitForSeconds(m_CharacterDisplayDelay);
            Debug.Log($"ADEEL LOADING 9");
            m_CharacterHolderTween.Play();
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
            CameraController.SetEnvironemntInteractiblity(false);
            UIManager.UIInteractionOff();
            SoundManager.PlaySound(SoundType.PopupWhoosh);
            s_Instance.m_Popup.SetActive(true);
            s_Instance.m_OpeningSequence.PlaySequence(() =>
            {
                callback?.Invoke();
            });
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
                CameraController.SetEnvironemntInteractiblity(true);
                UIManager.UIInteractionOn();
                s_Instance.m_IsLoading = false;
                s_Instance.m_LoadingCamera.gameObject.SetActive(false);
                callback?.Invoke();
            });
        }

        public static bool IsLoadingActive()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return false;
            }

            return s_Instance.m_IsLoading;
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
