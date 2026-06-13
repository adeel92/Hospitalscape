using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Isometric.Sound
{
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager s_Instance;

        public static Action OnMusicUpdate;
        public static Action OnSoundUpdate;

        public static bool IsMusicOn { get; private set; }
        public static bool IsSoundOn { get; private set; }

        [System.Serializable]
        private class ActiveAudio
        {
            public SoundType soundType;
            public AudioSource audioSource;

            public ActiveAudio(SoundType soundType, AudioSource audioSource)
            {
                this.soundType = soundType;
                this.audioSource = audioSource;
            }
        }

        [SerializeField] List<SoundData> m_SoundsData;

        [SerializeField] List<SoundType> m_MusicTypes;

        private Queue<AudioSource> m_AudioSourcePool = new Queue<AudioSource>();
        private List<ActiveAudio> m_ActiveAudioSources = new List<ActiveAudio>();

        [SerializeField] int m_InitialPoolSize = 10;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSourcePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static void SetMusic(bool value)
        {
            if (s_Instance != null)
            {
                IsMusicOn = value;
                OnMusicUpdate?.Invoke();

                if (IsMusicOn)
                {
                    foreach (var item in s_Instance.m_MusicTypes)
                    {
                        float volume = 1;
                        SoundData soundData = s_Instance.m_SoundsData.Find((x) => x.SoundType == item);
                        if (soundData != null)
                        {
                            volume = soundData.Volume;
                        }

                        SetSoundVolume(item, volume);
                    }
                }
                else
                {
                    foreach (var item in s_Instance.m_MusicTypes)
                    {
                        SetSoundVolume(item, 0);
                    }
                }
            }
            else
            {
                PrintInstanceNullError();
            }
        }

        public static void SetSound(bool value)
        {
            if(s_Instance != null)
            {
                IsSoundOn = value;
                OnSoundUpdate?.Invoke();

                if (IsSoundOn)
                {
                    foreach (var item in s_Instance.m_SoundsData)
                    {
                        if (!s_Instance.IsMusicType(item.SoundType))
                        {
                            SetSoundVolume(item.SoundType, item.Volume);
                        }
                    }
                }
                else
                {
                    foreach (var item in s_Instance.m_SoundsData)
                    {
                        if (!s_Instance.IsMusicType(item.SoundType))
                        {
                            SetSoundVolume(item.SoundType, 0);
                        }
                    }
                }
            }
            else
            {
                PrintInstanceNullError();
            }
        }

        private void InitializeAudioSourcePool()
        {
            for (int i = 0; i < m_InitialPoolSize; i++)
            {
                AudioSource source = CreateNewAudioSource();
                m_AudioSourcePool.Enqueue(source);
            }
        }

        private AudioSource CreateNewAudioSource()
        {
            // Create a new GameObject for each AudioSource and parent it to the SoundManager
            GameObject audioObject = new GameObject("PooledAudioSource");
            audioObject.transform.parent = this.transform;
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        #region Play Sound
        public static void PlaySound(SoundType soundType)
        {
            PlaySound(soundType, loop: false);
        }

        public static void PlaySound(SoundType soundType, bool loop, bool isTimeScaled = true)
        {
            if (s_Instance != null)
            {
                SoundData soundData = s_Instance.FindSoundData(soundType);

                if (soundData != null)
                {
                    AudioSource source = s_Instance.GetAvailableAudioSource();
                    source.clip = soundData.Clip;
                    if (s_Instance.IsMusicType(soundType) && !IsMusicOn)
                    {
                        source.volume = 0;
                    }
                    else if (!s_Instance.IsMusicType(soundType) && !IsSoundOn)
                    {
                        source.volume = 0;
                    }
                    else
                    {
                        source.volume = soundData.Volume;
                    }
                    source.pitch = soundData.Pitch;
                    source.loop = loop;
                    source.Play();

                    s_Instance.m_ActiveAudioSources.Add(new ActiveAudio(soundType, source));

                    if (!loop)
                    {
                        // Return AudioSource to pool after playing if not looping
                        s_Instance.StartCoroutine(s_Instance.ReturnToPoolAfterPlay(source, soundType, isTimeScaled));
                    }
                }
                else
                {
                    Debug.LogWarning("Sound of type " + soundType + " not found!");
                }
            }
            else
            {
                PrintInstanceNullError();
            }
        }

        public static void PlaySoundFadeIn(SoundType soundType, float duration, bool loop = false, bool isTimeScaled = true)
        {
            if (s_Instance == null)
            {
                PrintInstanceNullError();
                return;
            }

            SoundData soundData = s_Instance.FindSoundData(soundType);
            if (soundData == null)
            {
                Debug.LogWarning("Sound of type " + soundType + " not found!");
                return;
            }

            AudioSource source = s_Instance.GetAvailableAudioSource();
            source.clip = soundData.Clip;
            source.volume = 0;
            source.pitch = soundData.Pitch;
            source.loop = loop;
            source.Play();
            s_Instance.m_ActiveAudioSources.Add(new ActiveAudio(soundType, source));

            if (s_Instance.IsMusicType(soundType) && IsMusicOn)
            {
                s_Instance.StartCoroutine(s_Instance.FadeVolume(source, 0, soundData.Volume, duration, isTimeScaled));
            }
            else if (!s_Instance.IsMusicType(soundType) && !IsSoundOn)
            {
                s_Instance.StartCoroutine(s_Instance.FadeVolume(source, 0, soundData.Volume, duration, isTimeScaled));
            }

            if (!loop)
            {
                s_Instance.StartCoroutine(s_Instance.ReturnToPoolAfterPlay(source, soundType, isTimeScaled));
            }
        }
        #endregion

        #region Stop Sound

        public static void StopSound(SoundType soundType)
        {
            if (s_Instance != null)
            {
                for (int i = s_Instance.m_ActiveAudioSources.Count - 1; i >= 0; i--)
                {
                    ActiveAudio activeAudio = s_Instance.m_ActiveAudioSources[i];
                    if (activeAudio.soundType == soundType && activeAudio.audioSource.isPlaying)
                    {
                        activeAudio.audioSource.Stop();
                        s_Instance.m_AudioSourcePool.Enqueue(activeAudio.audioSource);
                        s_Instance.m_ActiveAudioSources.RemoveAt(i);
                        break; // Stop only one instance of the sound
                    }
                }
            }
            else
            {
                PrintInstanceNullError();
            }
        }

        public static void StopFadeOut(SoundType soundType, float duration, bool isTimeScaled = true)
        {
            if (s_Instance == null)
            {
                PrintInstanceNullError();
                return;
            }

            for (int i = s_Instance.m_ActiveAudioSources.Count - 1; i >= 0; i--)
            {
                ActiveAudio activeAudio = s_Instance.m_ActiveAudioSources[i];
                if (activeAudio.soundType == soundType && activeAudio.audioSource.isPlaying)
                {
                    s_Instance.StartCoroutine(s_Instance.FadeOutAndStop(activeAudio, duration, isTimeScaled));
                    break; // Only fade out the first match
                }
            }
        }
        #endregion

        private static void SetSoundVolume(SoundType soundType, float volume)
        {
            if (s_Instance != null)
            {
                for (int i = s_Instance.m_ActiveAudioSources.Count - 1; i >= 0; i--)
                {
                    ActiveAudio activeAudio = s_Instance.m_ActiveAudioSources[i];
                    if (activeAudio.soundType == soundType && activeAudio.audioSource.isPlaying)
                    {
                        activeAudio.audioSource.volume = volume;
                        break; // Stop only one instance of the sound
                    }
                }
            }
            else
            {
                PrintInstanceNullError();
            }
        }

        private SoundData FindSoundData(SoundType soundType)
        {
            foreach (var soundData in m_SoundsData)
            {
                if (soundData.SoundType == soundType)
                {
                    return soundData;
                }
            }
            return null;
        }

        private AudioSource GetAvailableAudioSource()
        {
            if (m_AudioSourcePool.Count > 0)
            {
                return m_AudioSourcePool.Dequeue();
            }

            // Create a new AudioSource if pool is empty
            return CreateNewAudioSource();
        }

        private IEnumerator ReturnToPoolAfterPlay(AudioSource source, SoundType soundType,bool isTimeScaled)
        {
            if (isTimeScaled) yield return new WaitForSeconds(source.clip.length);
            else yield return new WaitForSecondsRealtime(source.clip.length);

            source.Stop();

            if (m_AudioSourcePool.Count < m_InitialPoolSize)
            {
                m_AudioSourcePool.Enqueue(source);
            }
            else
            {
                Destroy(source.gameObject); // Avoid memory buildup if more than initial pool
            }

            m_ActiveAudioSources.RemoveAll(a => a.soundType == soundType && a.audioSource == source);
        }

        private IEnumerator FadeVolume(AudioSource source, float from, float to, float duration, bool isTimeScaled)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (isTimeScaled) elapsed += Time.deltaTime;
                else elapsed += Time.unscaledDeltaTime;

                source.volume = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            source.volume = to;
        }

        private IEnumerator FadeOutAndStop(ActiveAudio activeAudio, float duration, bool isTimeScaled)
        {
            AudioSource source = activeAudio.audioSource;
            float startVolume = source.volume;

            yield return FadeVolume(source, startVolume, 0f, duration, isTimeScaled);

            source.Stop();
            if (m_AudioSourcePool.Count < m_InitialPoolSize)
            {
                m_AudioSourcePool.Enqueue(source);
            }
            else
            {
                Destroy(source.gameObject);
            }

            m_ActiveAudioSources.Remove(activeAudio);
        }

        private bool IsMusicType(SoundType soundType)
        {
            return m_MusicTypes.Contains(soundType);
        }

        protected static void PrintInstanceNullError()
        {
            Debug.LogWarning("SoundManager Instance is null");
        }
    }

    public enum SoundType
    {
        MenuMusic1,
        GameMusic1,
        GameWon, 
        GameLost,
        TaskAssigned,
        TaskInteractions,
        Coin,
        Gem,
        WorkerBell,
        ButtonDown,
        ButtonUp,
        PopupWhoosh,
        CameraWhoosh,
        ButtonSwitch,
        Upgrade,
        Reward,
        CrowdCheering,
        SmallWin,
        MediumReward,
        BigReward
    }

    public enum SoundCategroy
    {
        Music, Sound, 
    }

    [System.Serializable]
    public class SoundData
    {
        public SoundType SoundType;
        public AudioClip Clip;
        [Range(0, 1)]
        public float Volume = 1f;
        [Range(-3, 3)]
        public float Pitch = 1f;
    }
}
