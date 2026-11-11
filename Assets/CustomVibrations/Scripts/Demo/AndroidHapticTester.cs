
using UnityEngine;
using TMPro;
using MoreMountains.NiceVibrations;

public class AndroidHapticTester : MonoBehaviour
{
    [Header("---Simple Vibration---")]
    [SerializeField] long simpleDuration = 100;
    [SerializeField] int simpleAmplitude = 150;

    [Header("---Pattern Vibration---")]
    [SerializeField] long startDelay = 0;
    [SerializeField] long pulseDuration = 30;
    [SerializeField] long pauseDuration = 100;
    [SerializeField] int pulseCount = 3;
    [SerializeField] int patternAmplitude = 150;

    [Header("---Runtime Controls---")]
    [SerializeField] TMP_InputField simpleDurationInput;
    [SerializeField] TMP_InputField simpleAmplitudeInput;
    [Space, SerializeField] TMP_InputField pulseDurationInput;
    [SerializeField] TMP_InputField pauseDurationInput;
    [SerializeField] TMP_InputField pulseCountInput;
    [SerializeField] TMP_InputField amplitudeInput;

    public void TestSimpleVibration()
    {
        MMNVAndroid.AndroidVibrate(simpleDuration, simpleAmplitude);
    }

    public void TestPatternVibration()
    {
        pulseCount = Mathf.Max(1, pulseCount);

        long[] pattern = new long[pulseCount * 2];
        int[] amplitudes = new int[pulseCount * 2];

        pattern[0] = startDelay;
        amplitudes[0] = 0;

        for (int i = 0; i < pulseCount; i++)
        {
            int vibrationIndex = 1 + (i * 2);

            pattern[vibrationIndex] = pulseDuration;
            amplitudes[vibrationIndex] = Mathf.Clamp(patternAmplitude, 0, 255);

            if (i < pulseCount - 1)
            {
                int pauseIndex = vibrationIndex + 1;

                pattern[pauseIndex] = pauseDuration;
                amplitudes[pauseIndex] = 0;
            }
        }

        MMNVAndroid.AndroidVibrate(pattern, amplitudes, -1);
    }

    public void ApplySimpleDuration()
    {
        if (long.TryParse(simpleDurationInput.text, out long value))
        {
            simpleDuration = System.Math.Max(1, value);
            Debug.Log($"Simple Duration: {simpleDuration} ms");
        }
    }

    public void ApplyPulseDuration()
    {
        if (long.TryParse(pulseDurationInput.text, out long value))
        {
            pulseDuration = System.Math.Max(1, value);
            Debug.Log($"Pulse Duration: {pulseDuration} ms");
        }
    }

    public void ApplyPauseDuration()
    {
        if (long.TryParse(pauseDurationInput.text, out long value))
        {
            pauseDuration = System.Math.Max(0, value);
            Debug.Log($"Pause Duration: {pauseDuration} ms");
        }
    }

    public void ApplyPulseCount()
    {
        if (int.TryParse(pulseCountInput.text, out int value))
        {
            pulseCount = Mathf.Max(1, value);
            Debug.Log($"Pulse Count: {pulseCount}");
        }
    }

    public void ApplySimpleAmplitude()
    {
        if (int.TryParse(simpleAmplitudeInput.text, out int value))
        {
            simpleAmplitude = Mathf.Clamp(value, 0, 255);
            Debug.Log($"Simple Amplitude: {simpleAmplitude}");
        }
    }

    public void ApplyPatternAmplitude()
    {
        if (int.TryParse(amplitudeInput.text, out int value))
        {
            patternAmplitude = Mathf.Clamp(value, 0, 255);
            Debug.Log($"Pattern Amplitude: {patternAmplitude}");
        }
    }
}
