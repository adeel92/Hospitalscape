using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Isometric
{
    public class CoroutineManager : MonoBehaviour
    {
        public static CoroutineManager Instance => s_Instance;
        private static CoroutineManager s_Instance;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        public static void StopAllCoroutine()
        {
            try
            {
                if (s_Instance)
                {
                    s_Instance.StopAllCoroutines();
                }
                else
                {
                    Debug.LogWarning("CoroutineManager is null");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error starting coroutine: {ex.Message}");
            }
        }

        public static Coroutine StartACoroutine(IEnumerator routine)
        {
            try
            {
                if (s_Instance)
                {
                    return s_Instance.StartCoroutine(routine);
                }
                else
                {
                    Debug.LogWarning("CoroutineManager is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error starting coroutine: {ex.Message}");
                return null;
            }
        }

        public static void StopACoroutine(Coroutine routine)
        {
            try
            {
                if (s_Instance)
                {
                    s_Instance.StopCoroutine(routine);
                }
                else
                {
                    Debug.LogWarning("CoroutineManager is null");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error stopping coroutine: {ex.Message}");
            }
        }

        public static Coroutine LateAction(Action action, float delay)
        {
            try
            {
                if (s_Instance)
                {
                    return s_Instance.StartCoroutine(ActionLate(action, delay));
                }
                else
                {
                    Debug.LogWarning("CoroutineManager is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in LateAction: {ex.Message}");
                return null;
            }
        }

        private static IEnumerator ActionLate(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in ActionLate execution: {ex.Message}");
            }
        }

        public static Coroutine LateActionRealTime(Action action, float delay)
        {
            try
            {
                if (s_Instance)
                {
                    return s_Instance.StartCoroutine(ActionLateRealTime(action, delay));
                }
                else
                {
                    Debug.LogWarning("CoroutineManager is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in LateActionRealTime: {ex.Message}");
                return null;
            }
        }

        private static IEnumerator ActionLateRealTime(Action action, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in ActionLateRealTime execution: {ex.Message}");
            }
        }

        public static Coroutine ActionAtEndOfFrame(Action action)
        {
            try
            {
                if (s_Instance)
                {
                    return s_Instance.StartCoroutine(AtEndOfFrameAction(action));
                }
                else
                {
                    Debug.LogWarning("CoroutineManager is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in ActionAtEndOfFrame: {ex.Message}");
                return null;
            }
        }

        private static IEnumerator AtEndOfFrameAction(Action action)
        {
            yield return new WaitForEndOfFrame();
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in AtEndOfFrameAction execution: {ex.Message}");
            }
        }
    }

}
