using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace CustomVibrations
{
    [CreateAssetMenu(fileName = "HapticDatabase", menuName = "CustomVibrations/Haptic Database")]
    public class HapticDatabase : ScriptableObject
    {
        [SerializeField] List<HapticConfiguration> configurations = new();

        public IReadOnlyList<HapticConfiguration> HapticConfigurations => configurations;

        private void OnValidate()
        {
            foreach (HapticConfiguration configuration in configurations)
            {
                configuration.GeneratePattern();
            }
        }
    }

    [Serializable]
    public class HapticConfiguration
    {
        [Header("---Configuration---")]
        [SerializeField] HapticType hapticType;
        [SerializeField] HapticConfigurationType configurationType;

        [Header("---Simple Haptic---")]
        [ShowIf(nameof(IsSimpleHaptic)), AllowNesting]
        [SerializeField] long simpleHapticDuration = 100;

        [ShowIf(nameof(IsSimpleHaptic)), AllowNesting]
        [SerializeField] int simpleHapticAmplitude = 150;

        [Header("---Pattern Haptic---")]
        [ShowIf(nameof(IsPatternFixedValueHaptic)), AllowNesting]
        [SerializeField] int patternFixedPulseCount = 3;

        [ShowIf(nameof(IsPatternFixedValueHaptic)), AllowNesting]
        [SerializeField] long patternFixedPulseDuration = 30;

        [ShowIf(nameof(IsPatternFixedValueHaptic)), AllowNesting]
        [SerializeField] long patternFixedPauseDuration = 100;

        [ShowIf(nameof(IsPatternFixedValueHaptic)), AllowNesting]
        [SerializeField] long patternFixedStartDelay;

        [ShowIf(nameof(IsPatternFixedValueHaptic)), AllowNesting]
        [SerializeField] int patternFixedAmplitude = 150;

        [Header("---Generated Pattern Values---")]
        [ShowIf(nameof(IsPatternFixedValueHaptic)), AllowNesting]
        [SerializeField] GeneratedPatternValues generatedPatternFixedValues = new();

        private bool IsSimpleHaptic => configurationType == HapticConfigurationType.SimpleHaptic;
        private bool IsPatternFixedValueHaptic => configurationType == HapticConfigurationType.PatternFixedValuesHaptic;
        
        public HapticType HapticType => hapticType;
        public HapticConfigurationType ConfigurationType => configurationType;

        public long SimpleHapticDuration => simpleHapticDuration;
        public int SimpleHapticAmplitude => simpleHapticAmplitude;
        public long[] PatternFixedTimings => generatedPatternFixedValues.timings;
        public int[] PatternFixedAmplitudes => generatedPatternFixedValues.amplitudes;


        public void GeneratePattern()
        {
            if (!IsPatternFixedValueHaptic)
                return;

            long[] patternFixedTimings = generatedPatternFixedValues.timings;
            int[] patternFixedAmplitudes = generatedPatternFixedValues.amplitudes;

            patternFixedPulseCount = Mathf.Max(1, patternFixedPulseCount);

            patternFixedTimings = new long[patternFixedPulseCount * 2];
            patternFixedAmplitudes = new int[patternFixedPulseCount * 2];

            patternFixedTimings[0] = Math.Max(0, patternFixedStartDelay);
            patternFixedAmplitudes[0] = 0;

            for (int i = 0; i < patternFixedPulseCount; i++)
            {
                int vibrationIndex = 1 + (i * 2);

                patternFixedTimings[vibrationIndex] = Math.Max(1, patternFixedPulseDuration);
                patternFixedAmplitudes[vibrationIndex] = Mathf.Clamp(patternFixedAmplitude, 0, 255);

                if (i < patternFixedPulseCount - 1)
                {
                    int pauseIndex = vibrationIndex + 1;

                    patternFixedTimings[pauseIndex] = Math.Max(0, patternFixedPauseDuration);
                    patternFixedAmplitudes[pauseIndex] = 0;
                }
            }

            generatedPatternFixedValues.timings = patternFixedTimings;
            generatedPatternFixedValues.amplitudes = patternFixedAmplitudes;
        }

        [Serializable]
        private class GeneratedPatternValues
        {
            public long[] timings;
            public int[] amplitudes;
        }
    }

    public enum HapticConfigurationType
    {
        SimpleHaptic,
        PatternFixedValuesHaptic
    }

    public enum HapticType
    {
        DemoHapticType,
        TaskFailNormalHint,
        TaskFailConsumableHint,
        TrashThrow,
        GameplayInteractableItemTap,
        PickItem,
        DropItem,
        MainServiceCleaningComplete,
        LevelLostReaction,
        LevelWonReaction,
        CustomerPick,
        CustomerDrop
    }
}