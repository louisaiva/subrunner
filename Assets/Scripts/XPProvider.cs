using UnityEngine;

// [RequireComponent(typeof(ParticleSystem))]
public class ParticleFlow : MonoBehaviour
{
    ParticleSystem m_System;
    // ParticleSystem.Particle[] m_Particles;

    private void Start() {
        // m_System = GetComponent<ParticleSystem>();
        // m_Particles = new ParticleSystem.Particle[m_System.main.maxParticles];
    }

    private void LateUpdate()
    {
        /* InitializeIfNeeded();

        // GetParticles is allocation free because we reuse the m_Particles buffer between updates
        int numParticlesAlive = m_System.GetParticles(m_Particles);

        // Change only the particles that are alive
        for (int i = 0; i < numParticlesAlive; i++)
        {
            m_Particles[i].velocity += Vector3.up * m_Drift;
        }

        // Apply the particle changes to the Particle System
        m_System.SetParticles(m_Particles, numParticlesAlive); */
    }
}