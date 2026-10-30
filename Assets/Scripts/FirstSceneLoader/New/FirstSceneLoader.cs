
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Isometric.UI;

namespace Arc
{
    public class FirstSceneLoader : MonoBehaviour
    {
        [Header("---Loading Settings---")]
        [SerializeField] float m_Delay = 2f;
        [SerializeField] float m_MinimumLoadingDuration = 3f;
        [SerializeField] float m_LoadingScreenCloseDelay = 1f;
        [SerializeField] string m_NextSceneName = "NextScene";
        [SerializeField] bool m_IsLogsEnabled;

        [Header("---Loading UI---")]
        [SerializeField] GameObject m_LoadingScreen;
        [SerializeField] Image m_LoadingFillBar;
        [SerializeField] TextMeshProUGUI m_LoadingPercentage;

        private bool m_IsFirst = true;
        private bool m_IsLoading;
        private Coroutine m_LoadingCoroutine;

        private void Start()
        {
            Debug.unityLogger.logEnabled = m_IsLogsEnabled;

            DontDestroyOnLoad(gameObject);

            m_LoadingCoroutine = StartCoroutine(LoadSceneAfterDelay(m_Delay));
        }

        private IEnumerator LoadSceneAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            m_LoadingCoroutine = null;

            CheckInternetAndAct();
        }

        public void CheckInternetAndAct()
        {
            if (m_IsLoading)
                return;

            if (InternetUIManager.IsInternetAvailable())
            {
                m_IsLoading = true;
                m_LoadingCoroutine = StartCoroutine(LoadSceneRoutine());
            }
        }

        private IEnumerator LoadSceneRoutine()
        {
            AsyncOperation loadingOperation =
                SceneManager.LoadSceneAsync(m_NextSceneName);

            loadingOperation.allowSceneActivation = false;

            float elapsedTime = 0f;

            UpdateLoadingUI(0f);

            while (loadingOperation.progress < 0.9f ||
                elapsedTime < m_MinimumLoadingDuration ||
                !InternetUIManager.IsInternetAvailable())
            {
                elapsedTime += Time.unscaledDeltaTime;

                float sceneProgress = loadingOperation.progress / 0.9f;

                float durationProgress = m_MinimumLoadingDuration <= 0f
                    ? 1f
                    : elapsedTime / m_MinimumLoadingDuration;

                float progress = Mathf.Min(sceneProgress, durationProgress);

                UpdateLoadingUI(progress);

                yield return null;
            }

            UpdateLoadingUI(1f);

            loadingOperation.allowSceneActivation = true;

            yield return new WaitUntil(() => loadingOperation.isDone);

            yield return new WaitForSeconds(m_LoadingScreenCloseDelay);
            // Hide the loading screen before destroying this loader.
            m_LoadingScreen.SetActive(false);

            Destroy(gameObject);
        }

        private void UpdateLoadingUI(float progress)
        {
            progress = Mathf.Clamp01(progress);

            m_LoadingFillBar.fillAmount = progress;

            m_LoadingPercentage.text =
                Mathf.RoundToInt(progress * 100f) + "%";
        }

        private bool IsInternetAvailable()
        {
            return Application.internetReachability !=
                   NetworkReachability.NotReachable;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!m_IsFirst)
            {
                if (m_IsLoading)
                    return;

                if (hasFocus)
                {
                    if (m_LoadingCoroutine == null)
                        m_LoadingCoroutine = StartCoroutine(
                            LoadSceneAfterDelay(1.8f));
                }
                else
                {
                    if (m_LoadingCoroutine != null)
                    {
                        StopCoroutine(m_LoadingCoroutine);
                        m_LoadingCoroutine = null;
                    }
                }
            }
            else
            {
                m_IsFirst = false;
            }
        }
    }
}