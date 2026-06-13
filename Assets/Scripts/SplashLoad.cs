using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

namespace Isometric
{
    public class SplashLoad : MonoBehaviour
    {
        [SerializeField, Scene] string m_SceneToLoad;
        [SerializeField] float m_Delay;

        public IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(m_Delay);
            SceneManager.LoadScene(m_SceneToLoad);
        }
    }
}
