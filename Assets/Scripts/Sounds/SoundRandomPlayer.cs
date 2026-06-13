using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Isometric.Sound
{
    public class SoundRandomPlayer : MonoBehaviour
    {
        [Serializable]
        private class SoundInfo
        {
            public List<AudioSource> AudioSources;
        }

        [SerializeField] List<SoundInfo> m_SoundsInfo;

        public void PlayAtIndex(int index)
        {
            if (index >= 0 && index < m_SoundsInfo.Count)
            {
                int randomIndex = UnityEngine.Random.Range(0, m_SoundsInfo[index].AudioSources.Count);
                AudioSource targetAudioSource = m_SoundsInfo[index].AudioSources[randomIndex];
                targetAudioSource.Play();
            }
            else
            {
                Debug.LogWarning("No sound found at index " + index);
            }
        }
    }
}
