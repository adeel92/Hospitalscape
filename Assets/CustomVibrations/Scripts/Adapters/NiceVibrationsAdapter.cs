using UnityEngine;
using MoreMountains.NiceVibrations;

namespace CustomVibrations
{
    public class NiceVibrationsAdapter : MonoBehaviour
    {
        public void PlaySimple(long duration, int amplitude)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            duration = System.Math.Max(1, duration);
            amplitude = Mathf.Clamp(amplitude, 0, 255);

            MMNVAndroid.AndroidVibrate(duration, amplitude);
#endif
        }

        public void PlayPattern(long[] timings, int[] amplitudes)
        {
            if (timings == null || amplitudes == null)
                return;

            if (timings.Length == 0 || timings.Length != amplitudes.Length)
            {
                Debug.LogWarning("Invalid haptic pattern configuration.", this);
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            MMNVAndroid.AndroidVibrate(timings, amplitudes, -1);
#endif
        }
    }
}