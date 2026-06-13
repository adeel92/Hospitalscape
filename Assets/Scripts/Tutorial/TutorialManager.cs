using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.UI;

namespace Isometric.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        private static TutorialManager s_Instance;

        [SerializeField] bool m_IsEnabled = true;
        [SerializeField] GameObject m_TutorialTextCanvas;
        [SerializeField] GameObject m_TutorialTextDown;
        [SerializeField] GameObject m_TutorialTextUp;
        [SerializeField] List<TutorialInfo> m_TutorialsInfo;
        TutorialInfo m_CurrentTutorial = null;


        public void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }

        public static bool PlayTutorial(TutorialCallType tutorialSetupType)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return false;
            }

            if (s_Instance.m_IsEnabled == false)
            {
                return false;
            }

            if (s_Instance.m_CurrentTutorial != null)
            {
                Debug.Log("In Tutorial already!");
                return false;
            }

            MapType currentMapType = DataManager.CurrentMapType;
            int currentMapLevelIndex = DataManager.CurrentMapLevelIndex;

            TutorialInfo tutorialInfo = s_Instance.m_TutorialsInfo.Find((x) => x.ShowAtMapType == currentMapType 
            && x.ShowAtLevelIndex == currentMapLevelIndex
            && x.CallType == tutorialSetupType);

            if (tutorialInfo != null && tutorialInfo.CallType == tutorialSetupType)
            {
                if (DataManager.GetBool(tutorialInfo.Key, false) == false)
                {
                    s_Instance.m_CurrentTutorial = tutorialInfo;
                    s_Instance.m_CurrentTutorial.Tutorial.Play();
                    return true;
                }
            }

            return false;
        }

        public static void TutorialComplete(TutorialContainer tutorial)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            if (s_Instance.m_CurrentTutorial != null &&
                tutorial == s_Instance.m_CurrentTutorial.Tutorial)
            {
                DataManager.SetBool(s_Instance.m_CurrentTutorial.Key, true);
                TutorialCallType callType = s_Instance.m_CurrentTutorial.CallType;
                bool callTheNextReleventEvent = s_Instance.m_CurrentTutorial.callTheNextReleventEvent;
                s_Instance.m_CurrentTutorial = null;
                s_Instance.m_TutorialTextDown.SetActive(false);

                if (PlayTutorial(callType) == false && callTheNextReleventEvent)
                {
                    if (callType == TutorialCallType.BeforeNextUpdatable)
                    {
                        UIManager.CheckNextUpdatable();
                    }
                }
            }
        }

        public static void ActivateTutorailText(TutorailTextPosition textPosition)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_TutorialTextCanvas.SetActive(true);
            s_Instance.m_TutorialTextDown.SetActive(false);
            s_Instance.m_TutorialTextUp.SetActive(false);

            if (textPosition == TutorailTextPosition.Up)
            {
                s_Instance.m_TutorialTextUp.SetActive(true);
            }
            else
            {
                s_Instance.m_TutorialTextDown.SetActive(true);
            }
        }

        public static void DeactivateTutorailText()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_TutorialTextCanvas.SetActive(false);
            s_Instance.m_TutorialTextDown.SetActive(false);
            s_Instance.m_TutorialTextUp.SetActive(false);
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(TutorialManager) + " is null");
        }
    }

    [Serializable]
    public class TutorialInfo
    {
        public string Key;
        public TutorialCallType CallType;
        public MapType ShowAtMapType;
        public int ShowAtLevelIndex;
        public TutorialContainer Tutorial;
        [Header("-If call is BeforeNextUpdatable then NextUpdatable is going to be called")]
        public bool callTheNextReleventEvent;
    }

    public enum TutorialCallType
    {
        BeforeNextUpdatable, AfterUpdatePopup, GameplayStart
    }

    public abstract class TutorialContainer : MonoBehaviour
    {
        [SerializeField]
        protected bool m_EditDiscription;
        [SerializeField,TextArea, EnableIf(nameof(m_EditDiscription))]
        protected string m_Discription = "Add description";

        public abstract void Play();

        public virtual void Stop()
        {
            TutorialManager.DeactivateTutorailText();
            TutorialManager.TutorialComplete(this);
        }
    }

    public enum TutorailTextPosition
    {
        Down, Up
    }
}
