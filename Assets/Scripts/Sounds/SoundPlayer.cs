using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isometric.Sound
{
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField] List<SoundType> m_SoundTypes;

        public void PlayAtIndex(int index)
        {
            if (index >= 0 && index < m_SoundTypes.Count)
            {
                SoundManager.PlaySound(m_SoundTypes[index]);
            }
        }
    }
}
