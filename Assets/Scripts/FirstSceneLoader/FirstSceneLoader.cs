using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Arc
{
    public class FirstSceneLoader : MonoBehaviour
    {
        [SerializeField] float m_Delay = 2f; // Delay before loading the next scene
        [SerializeField] string m_NextSceneName = "NextScene"; // Name of the next scene to load
        [SerializeField] bool m_IsLogsEnabled;

        public UnityEvent onInternetAvailable;
        public UnityEvent onInternetNotAvailable;

        private bool m_IsFirst = true;
        private Coroutine m_Loading = null;

        private void Start()
        {
            Debug.unityLogger.logEnabled = m_IsLogsEnabled;
            m_Loading = StartCoroutine(LoadSceneAfterDelay(m_Delay));
        }


        private IEnumerator LoadSceneAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            CheckInternetAndAct();
        }

        public void CheckInternetAndAct()
        {
            if (IsInternetAvailable())
            {
                onInternetAvailable?.Invoke();
                SceneManager.LoadScene(m_NextSceneName);
            }
            else
            {
                onInternetNotAvailable?.Invoke();
            }
        }

        private bool IsInternetAvailable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!m_IsFirst)
            {
                if (hasFocus)
                {
                    m_Loading = StartCoroutine(LoadSceneAfterDelay(1.8f));
                }
                else
                {
                    StopCoroutine(m_Loading);
                }
            }
            else
            {
                m_IsFirst = false;
            }
        }
    }
}
