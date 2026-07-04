using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coffee.UIExtensions;

namespace Isometric
{
    public class ShinyEffectForUGUIEnabler : MonoBehaviour
    {

        [SerializeField] List<ShinyEffectForUGUI> m_ShinyEffectsForUGUI;
        [SerializeField] AnimatorUpdateMode m_UpdateMode;
        [SerializeField] float m_Duration;
        [SerializeField] bool m_PlayOnStart = true;

        private void Start()
        {
            if (m_PlayOnStart)
            {
                PlayShine();
            }
        }

        public void PlayShine()
        {
            StartCoroutine(Play());
        }

        IEnumerator Play()
        {
            while (true)
            {
                float time = 0;
                while (time < m_Duration)
                {
                    foreach (var item in m_ShinyEffectsForUGUI)
                    {
                        item.location = time / m_Duration;
                    }
                    time += m_UpdateMode == AnimatorUpdateMode.UnscaledTime
                        ? Time.unscaledDeltaTime
                        : Time.deltaTime;
                    yield return null;
                }
            }
        }

        public void PlayShineOne()
        {
            StartCoroutine(PlayOne());
        }

        IEnumerator PlayOne()
        {
            float time = 0;
            while (time < m_Duration)
            {
                foreach (var item in m_ShinyEffectsForUGUI)
                {
                    item.location = time / m_Duration;
                }
                time += m_UpdateMode == AnimatorUpdateMode.UnscaledTime
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;
                yield return null;
            }
        }

        public void StopShine()
        {
            StopAllCoroutines();
            foreach (var item in m_ShinyEffectsForUGUI)
            {
                item.location = 0;
            }
        }
    }
}
