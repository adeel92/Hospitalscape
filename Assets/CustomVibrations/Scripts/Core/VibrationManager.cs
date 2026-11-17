using System.Collections.Generic;
using UnityEngine;

namespace CustomVibrations
{
    public class VibrationManager : MonoBehaviour
    {
        private static VibrationManager instance;

        [Header("---References---")]
        [SerializeField] HapticDatabase hapticDatabase;
        [SerializeField] NiceVibrationsAdapter niceVibrationsAdapter;

        [Header("---Settings---")]
        [SerializeField] bool isVibrationAllowed = true;

        private Dictionary<HapticType, HapticConfiguration> hapticConfigurations = new();
        public static bool IsVibrationOn { get; private set; }


        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeConfigurations();
        }

        private void InitializeConfigurations()
        {
            hapticConfigurations.Clear();

            if (hapticDatabase == null)
            {
                Debug.LogWarning("Haptic Database is not assigned!", this);
                return;
            }

            foreach (HapticConfiguration configuration in hapticDatabase.HapticConfigurations)
            {
                if (hapticConfigurations.ContainsKey(configuration.HapticType))
                {
                    Debug.LogWarning($"Duplicate Haptic Type: {configuration.HapticType}", this);
                    continue;
                }

                hapticConfigurations.Add(configuration.HapticType, configuration);
            }
        }

        /// <summary>
        /// Toggle vibration
        /// </summary>
        public static void SetVibration(bool value)
        {
            if(instance != null)
            {
                IsVibrationOn = value;
            }
            else
            {
                PrintNullInstanceError();
            }
        }

        /// <summary>
        /// Play vibration by using the preset haptic values (by NiceVibrations) without any customization.
        /// </summary>
        public static void Play(MoreMountains.NiceVibrations.HapticTypes presetHapticType)
        {
            if (instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            if (!instance.isVibrationAllowed)
            {
                Debug.Log($"Vibration is not allowed!", instance); 
                return;
            }

            if (!IsVibrationOn)
            {
                Debug.Log($"Vibration is not toggled on!", instance); 
                return;
            }

            if (instance.niceVibrationsAdapter == null)
            {
                Debug.LogWarning($"NiceVibrationAdapter is null!", instance);
                return;
            }
            

        }

        /// <summary>
        /// Play vibration by using the custom HapticType and HapticConfiguration inside HapticDatabase.
        /// </summary>
        public static void Play(HapticType hapticType)
        {
            if (instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            if (!instance.isVibrationAllowed)
            {
                Debug.Log($"Vibration is not enabled!", instance); 
                return;
            }

            if (!IsVibrationOn)
            {
                Debug.Log($"Vibration is not toggled on!", instance); 
                return;
            }

            if (instance.niceVibrationsAdapter == null)
            {
                Debug.LogWarning($"NiceVibrationAdapter is null!", instance);
                return;
            }

            if (!instance.hapticConfigurations.TryGetValue(hapticType, out HapticConfiguration configuration))
            {
                Debug.LogWarning($"Haptic configuration not found: {hapticType}", instance);
                return;
            }

            switch (configuration.ConfigurationType)
            {
                case HapticConfigurationType.SimpleHaptic:
                    instance.niceVibrationsAdapter.PlaySimple(configuration.SimpleHapticDuration, configuration.SimpleHapticAmplitude);
                    break;

                case HapticConfigurationType.PatternFixedValuesHaptic:
                    instance.niceVibrationsAdapter.PlayPattern(configuration.PatternFixedTimings, configuration.PatternFixedAmplitudes);
                    break;
            }
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(VibrationManager) + " is null");
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }

    public enum VibrationCategory
    {
        Vibration,
    }
}