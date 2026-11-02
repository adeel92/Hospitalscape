
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Isometric.UI
{
    public class InternetUIManager : MonoBehaviour
    {
        private static InternetUIManager s_Instance;

        [Header("---Settings---")]
        [SerializeField] bool m_StopGameOnNoInternet = true;
        [SerializeField] float m_InternetCheckInterval = 2f;

        [Header("---UI---")]
        [SerializeField] GameObject m_InternetPopup;

        private bool m_IsInternetAvailable;
        private Coroutine m_InternetCheckRoutine;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (s_Instance != this || !m_StopGameOnNoInternet)
                return;

            CheckInternetStatus();

            m_InternetCheckRoutine = StartCoroutine(InternetCheckRoutine());
        }

        private IEnumerator InternetCheckRoutine()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(m_InternetCheckInterval);

                CheckInternetStatus();
            }
        }

        public static bool IsInternetAvailable()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return true;
            }

            if (!s_Instance.m_StopGameOnNoInternet)
            {
                return true;
            }

            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        public void CheckInternetStatus()
        {
            bool isAvailable = IsInternetAvailable();

            if (m_IsInternetAvailable == isAvailable)
                return;

            m_IsInternetAvailable = isAvailable;

            if (m_IsInternetAvailable)
            {
                m_InternetPopup.SetActive(false);
                Time.timeScale = 1f;
            }
            else
            {
                m_InternetPopup.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
                s_Instance = null;
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(InternetUIManager) + " is null");
        }
    }
}