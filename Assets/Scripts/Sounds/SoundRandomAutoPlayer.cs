using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isometric.Sound
{
    public class SoundRandomAutoPlayer : MonoBehaviour
    {
        [System.Serializable]
        private class SoundInfo
        {
            public AudioClip AudioClip;
            public float MinDelay;
            public float MaxDelay;
            public float SoundDelay;
        }

        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private List<SoundInfo> m_SoundsInfo;
        [SerializeField] private float m_InitalDealy = 6f;
        [SerializeField] private bool m_PlayInSequence = true;
        private Coroutine m_PlaySounds = null;
        private int m_CurrentIndex = 0;

        private void Start()
        {
            m_PlaySounds = StartCoroutine(PlaySounds());
        }

        public void Stop()
        {
            if (m_PlaySounds != null)
            {
                StopCoroutine(m_PlaySounds);
                m_PlaySounds = null;
            }
        }

        private IEnumerator PlaySounds()
        {
            yield return new WaitForSeconds(m_InitalDealy);

            while (true)
            {
                SoundInfo soundInfo;

                if (m_PlayInSequence)
                {
                    soundInfo = m_SoundsInfo[m_CurrentIndex];
                    m_CurrentIndex = (m_CurrentIndex + 1) % m_SoundsInfo.Count; // Loop back to the start
                }
                else
                {
                    soundInfo = m_SoundsInfo[Random.Range(0, m_SoundsInfo.Count)];
                }

                yield return new WaitForSeconds(Random.Range(soundInfo.MinDelay, soundInfo.MaxDelay));

                m_AudioSource.PlayOneShot(soundInfo.AudioClip);

                yield return new WaitForSeconds(soundInfo.SoundDelay);
            }
        }
    }
}
