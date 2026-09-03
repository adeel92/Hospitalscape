using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


namespace Isometric.Environment
{
    public class SterileGauzeMachineAnimationHandler : MonoBehaviour
    {
        [Header("---Station Info---")]
        [SerializeField] StationSerialOrderInOut m_StationSerialOrderInOut;

        [Header("---Animation Handlers---")]
        [SerializeField] StationSerialOrderInOutItemAnimationHandler m_MachineAnimationHandler;

        [Header("---Timer---")]
        [SerializeField] List<SpriteRenderer> m_FillSpriteRenderers = new();
        [SerializeField] SpriteRenderer m_InsideGauze;
        [SerializeField] SpriteRenderer m_OutputGauze;
        [SerializeField] float m_DelayBeforeTimerStarts = 0.45f;
        [SerializeField] float m_FillResetDuration = 0.25f;
        
        [Header("---Particles---")]
        [SerializeField] List<ParticleSystem> m_Particles = new();

        private Coroutine m_FillCoroutine;


        private void Awake()
        {
            m_StationSerialOrderInOut.ExtraDurationForAnimation = m_DelayBeforeTimerStarts;
            ResetTimer();
        }

        // OnDurationStart
        public void PlayGauzeProcessAnimation(float duration)
        {
            Color insideGauzeColor = m_InsideGauze.color;
            insideGauzeColor.a = 1f;
            m_InsideGauze.color = insideGauzeColor;
            Color outputGauzeColor = m_OutputGauze.color;
            outputGauzeColor.a = 0f;
            m_OutputGauze.color = outputGauzeColor;
            m_MachineAnimationHandler.PlayState(StationSerialOrderInOutItemAnimatorStates.Process, null, null);

            if(m_FillCoroutine != null)
            {
                StopCoroutine(m_FillCoroutine);
                m_FillCoroutine = null;
            }
            m_FillCoroutine = StartCoroutine(StartTimer(duration));
        }

        // OnOrderIn
        public void PlayGauzeProduceAnimation()
        {
            StopParticles();
            Color insideGauzeColor = m_InsideGauze.color;
            insideGauzeColor.a = 0f;
            m_InsideGauze.color = insideGauzeColor;
            Color outputGauzeColor = m_OutputGauze.color;
            outputGauzeColor.a = 1f;
            m_OutputGauze.color = outputGauzeColor;
            m_MachineAnimationHandler.PlayState(StationSerialOrderInOutItemAnimatorStates.Produce, null, null);
        }

        // OnOrderOut
        public void PlayMachineInitAnimation()
        {
            m_MachineAnimationHandler.PlayState(StationSerialOrderInOutItemAnimatorStates.Init, null, null);
            ResetTimer();
        }

        private IEnumerator StartTimer(float duration)
        {
            yield return new WaitForSeconds(m_DelayBeforeTimerStarts);

            PlayParticles();
            m_InsideGauze.DOFade(0f, duration);
            m_OutputGauze.DOFade(1f, duration);

            float fadeDuration = duration / m_FillSpriteRenderers.Count;
            foreach (SpriteRenderer spriteRenderer in m_FillSpriteRenderers)
            {
                spriteRenderer.DOFade(1f, fadeDuration);
                yield return new WaitForSeconds(fadeDuration);
            }
        }

        private void ResetTimer()
        {
            if(m_FillCoroutine != null)
            {
                StopCoroutine(m_FillCoroutine);
                m_FillCoroutine = null;
            }
            foreach (SpriteRenderer spriteRenderer in m_FillSpriteRenderers)
            {
                spriteRenderer.DOFade(0f, m_FillResetDuration);
            }
        }

        private void PlayParticles()
        {
            foreach(ParticleSystem particle in m_Particles)
            {
                particle.Play();
            }
        }
        private void StopParticles()
        {
            foreach(ParticleSystem particle in m_Particles)
            {
                particle.Stop();
            }
        }
    }
}