using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Isometric.Sound
{
    public class SoundStopAtGameplayEnd : MonoBehaviour
    {
        [SerializeField] List<AudioSource> m_AudioSources;

        private void OnEnable()
        {
            GlobalEventHolder.OnGameWon += LevelEnd;
            GlobalEventHolder.OnGameLost += LevelEnd;
        }

        private void OnDisable()
        {
            GlobalEventHolder.OnGameWon -= LevelEnd;
            GlobalEventHolder.OnGameLost -= LevelEnd;
        }

        public void StopSounds()
        {
            foreach (var audioSoruce in m_AudioSources)
            {
                audioSoruce.Stop();
            }
        }

        private void LevelEnd()
        {
            StopSounds();
        }

    }
}
