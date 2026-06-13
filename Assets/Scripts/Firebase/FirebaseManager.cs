using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
/*using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;*/

namespace Isometric
{
    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseManager Instance;

        //DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
        public bool IsLogEnabled = true;
        public List<FirebaseLogInfo> logsInfo;

        private bool firebaseInitialized = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            /*FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    InitializeFirebase();
                }
                else
                {
                    Debug.LogError(
                     "Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });*/
        }

        void InitializeFirebase()
        {

            /*DebugLog("Enabling data collection.");
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

            DebugLog("Set user properties.");

            firebaseInitialized = true;
            AnalyticsLogin();*/
        }

        public void DebugLog(string str)
        {
            if (IsLogEnabled)
            {
                Debug.Log(str);
            }
        }

        public void LogEvent(string info)
        {
            /*if (firebaseInitialized)
            {
                FirebaseAnalytics.LogEvent(info);

                DebugLog(info);
            }*/
        }

        public void LogEvent(string info, string paramenterName, int parameterValue)
        {
            //FirebaseAnalytics.LogEvent(info, paramenterName, parameterValue);
        }

        public void AnalyticsLogin()
        {
            /*DebugLog("Logging a login event.");
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);*/
        }


        public void AnalyticsScore()
        {
            /*DebugLog("Logging a post-score event.");
            FirebaseAnalytics.LogEvent(
                FirebaseAnalytics.EventPostScore,
                FirebaseAnalytics.ParameterScore,
                42);*/
        }

        public void AnalyticsGroupJoin()
        {
            /*DebugLog("Logging a group join event.");
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventJoinGroup, FirebaseAnalytics.ParameterGroupID, "spoon_welders");*/
        }

        public void AnalyticsLevelUp()
        {
            /*DebugLog("Logging a level up event.");
            FirebaseAnalytics.LogEvent(
                FirebaseAnalytics.EventLevelUp,
                new Parameter(FirebaseAnalytics.ParameterLevel, 5),
                new Parameter(FirebaseAnalytics.ParameterCharacter, "Play"),
                new Parameter("hit_accuracy", 3.14f));*/
        }

        public void ResetAnalyticsData()
        {
            /*DebugLog("Reset analytics data.");
            FirebaseAnalytics.ResetAnalyticsData();*/
        }


        public static void LogEvent(string firstConcateString, FirebaseLogType logType)
        {
            /*if (Instance != null)
            {
                var logInfo = Instance.logsInfo.Find((x) => x.Type == logType);
                if (logInfo != null)
                {
                    Instance.DebugLog(firstConcateString + logInfo.Log);
                    FirebaseAnalytics.LogEvent(firstConcateString + logInfo.Log);
                }
                else
                {
                    Debug.LogWarning("Log not found");
                }
            }
            else
            {
                Debug.Log("Firebase Manager Instance is null");
            }*/
        }

    }

    [Serializable]
    public class FirebaseLogInfo
    {
        public FirebaseLogType Type;
        public string Log;
    }

    public enum FirebaseLogType
    {
        GameStart, GameWon, GameLost, GameLeft, LostVideoReward, WonVideoReward
    }
}