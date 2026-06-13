using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isometric.Sound
{
    public class SoundGamplayMute : MonoBehaviour
    {
        [SerializeField] List<AudioSource> m_AudioSource;

        private void OnEnable()
        {
            GlobalEventHolder.OnGameplayPause += OnGameplayPause;
            GlobalEventHolder.OnGameplayUnause += OnGameplayUnpause;
        }

        private void OnDisable()
        {
            GlobalEventHolder.OnGameplayPause -= OnGameplayPause;
            GlobalEventHolder.OnGameplayUnause -= OnGameplayUnpause;
        }

        public void OnGameplayPause()
        {
            foreach (var audioSource in m_AudioSource)
            {
                audioSource.mute = true;
            }
        }

        public void OnGameplayUnpause()
        {
            foreach (var audioSource in m_AudioSource)
            {
                audioSource.mute = false;
            }
        }
    }
}