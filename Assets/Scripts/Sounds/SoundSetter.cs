using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Isometric.Sound
{
    public class SoundSetter : MonoBehaviour
    {
        [System.Serializable]
        private class AudioSourceInfo
        {
            public float DefaultVolume;
            public AudioSource AudioSource;
        }

        [SerializeField] List<AudioSourceInfo> m_AudioSource;

        private void OnEnable()
        {
            SoundManager.OnSoundUpdate += Setup;
        }

        private void OnDisable()
        {
            SoundManager.OnSoundUpdate -= Setup;
        }

        private void Start()
        {
            Setup();
        }

        public void Setup()
        {
            if (SoundManager.IsSoundOn)
            {
                foreach (var item in m_AudioSource)
                {
                    item.AudioSource.volume = item.DefaultVolume;
                }
            }
            else
            {
                foreach (var item in m_AudioSource)
                {
                    item.AudioSource.volume = 0;
                }
            }
        }
    }
}
