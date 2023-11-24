using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class XPProvider : MonoBehaviour
{
    // PARTICLES
    List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();
    ParticleSystem generator;

    // PARTICLES RADIUS GENERATION
    public float radius = 0.5f;

    // PLAYER
    public GameObject player;

    private void Start()
    {
        // on récupère le player
        // player = GameObject.Find("/world/perso");

        // on récupère le particle system
        generator = GetComponent<ParticleSystem>();

        // on emet une particule
        generator.Emit(1);
    }

    private void OnParticleTrigger()
    {

        // on récupère les particules
        int triggeredParticles = generator.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);

        // print("we just collided with " + triggeredParticles+" particles");

        // on change la vie des particules
        for (int i = 0; i < triggeredParticles; i++)
        {
            ParticleSystem.Particle p = particles[i];
            p.remainingLifetime = 0;
            particles[i] = p;
        }

        // on applique les changements
        generator.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);

        // on ajoute l'xp au player
        player.GetComponent<Perso>().addXP(triggeredParticles);

    }

    public void EmitXP(int count, Vector3 position,float strengh = 1f)
    {
        // on crée un EmitParams pour pouvoir changer la position de l'émission
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();

        for (int i = 0; i < count; i++)
        {
            // on change la position de l'émission
            // dans un rayon de radius autour de la position
            Vector2 position2D = Random.insideUnitCircle;
            emitParams.position = position + radius * new Vector3(position2D.x, position2D.y, 0);

            // on change la vitesse de l'émission en fonction de la strengh
            // dans une direction 2D aléatoire en x et y
            Vector2 direction = Random.insideUnitCircle;
            emitParams.velocity = strengh * new Vector3(direction.x, direction.y, 0);

            // on emet les particules
            generator.Emit(emitParams, 1);
        }
    }

}