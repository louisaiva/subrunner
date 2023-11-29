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

    // heal and bits generation
    private float life_percent = 0.01f; // 1% des particules sont des vies
    private float bit_percent = 0.01f; // 1% des particules sont des bits
    // private Color life_color = new Color(226f / 255f, 144f / 255f, 144f / 255f);
    private Color life_color = new Color(1f, 0f, 0f);
    // private Color bit_color = new Color(106f / 255f, 190f / 255f, 48f / 255f);
    private Color bit_color = new Color(9f / 255f, 1f, 0f);

    // materials
    public Material life_material;
    public Material bit_material;
    public Material xp_material;

    // PLAYER
    public GameObject player;

    // generator continue
    public bool generate_continuously = false;
    public Vector3 generator_position = new Vector3(-47, -9, 0);
    public float generator_strengh = 1f;

    private void Start()
    {
        // on récupère le particle system
        generator = GetComponent<ParticleSystem>();
        // on emet une particule
        // EmitXP(500, new Vector3(0, -1, 0),10f);
        // EmitXP(500, new Vector3(-30, -12, 0), 10f);
    }

    void Update()
    {
        if (generate_continuously)
        {
            emitEndlessly();
        }
    }

    private void OnParticleTrigger()
    {

        // on récupère les particules
        int triggeredParticles = generator.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);

        // print("we just collided with " + triggeredParticles+" particles");

        int life_bonus = 0;
        int xp_bonus = 0;
        int bit_bonus = 0;

        // on change la vie des particules
        for (int i = 0; i < triggeredParticles; i++)
        {
            ParticleSystem.Particle p = particles[i];

            // on regarde la couleur de la particule
            Color color = p.GetCurrentColor(generator);
            if (color == life_color)
            {
                // on ajoute de la vie
                life_bonus += 1;
            }
            else if (color == bit_color)
            {
                // on ajoute des bits
                bit_bonus += 1;
            }
            else
            {
                // on ajoute de l'xp
                xp_bonus += 1;
            }

            // on change la vie de la particule
            p.remainingLifetime = 0;
            particles[i] = p;
        }

        // on applique les changements
        generator.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);


        // on ajoute l'xp au player
        if (xp_bonus > 0) { player.GetComponent<Perso>().addXP(xp_bonus); }
        
        // on ajoute des bits au player
        if (bit_bonus > 0) { player.GetComponent<Perso>().addBits(bit_bonus); }

        // on ajoute de la vie au player
        if (life_bonus > 0) { player.GetComponent<Perso>().heal(life_bonus); }
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

            // on change la couleur de l'émission
            float rand = Random.Range(0f, 1f);
            emitParams.startColor = Color.white;
            if (rand < life_percent)
            {
                // on change la couleur de la particule
                emitParams.startColor = life_color;
            }
            else if (rand > 1f - bit_percent)
            {
                // on change la couleur de la particule
                emitParams.startColor = bit_color;
            }


            // on emet les particules
            generator.Emit(emitParams, 1);
        }
    }

    private void emitEndlessly()
    {
        EmitXP((int) generator_strengh, generator_position);
    }

}