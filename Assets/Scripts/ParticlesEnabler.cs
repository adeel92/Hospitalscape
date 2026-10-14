using System.Collections.Generic;
using UnityEngine;

namespace Isometric
{
    public class ParticlesEnabler : MonoBehaviour
    {
        [SerializeField] List<ParticleSystem> m_ParticleSystems = new();

        public void PlayParticles()
        {
            foreach(ParticleSystem particleSystem in m_ParticleSystems)
            {
                particleSystem.Play();
            }
        }

        public void StopParticles()
        {
            foreach(ParticleSystem particleSystem in m_ParticleSystems)
            {
                particleSystem.Stop();
            }
        }
    }
}