using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

[RequireComponent(typeof(ParticleSystem))]
public class XPProvider : Singleton<XPProvider>
{
    // PARTICLES
    protected List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();
    private ParticleSystem _part_system;
    public ParticleSystem ParticuleSystem
    {
        get
        {
            if (_part_system == null) { _part_system = GetComponent<ParticleSystem>(); }
            return _part_system;
        }
    }

    // PARTICLES RADIUS GENERATION
    public float radius = 0.5f;

    // heal generation
    private float life_percent = 0.01f; // 1% des particules sont des vies
    private Color life_color = new Color(1f, 0f, 0f);

    // materials
    // public Material xp_material;
    // public Material life_material;
    private LocalKeyword visibleKeyword;

    // generator continue
    public bool generate_continuously = false;
    public Vector3 generator_position = new Vector3(-47, -9, 0);
    public float generator_strengh = 1f;

    [Header("Logs")]
    public bool log_triggers = false;

    // START
    private void Start()
    {
        ParticleSystemRenderer renderer = ParticuleSystem.GetComponent<ParticleSystemRenderer>();
        visibleKeyword = new LocalKeyword(renderer.material.shader, "_VISIBLE");
        renderer.material.EnableKeyword(visibleKeyword);

        if (Perso.Instance == null) { return; } // no player, no trigger
        ParticuleSystem.trigger.SetCollider(0, Perso.Instance.transform.Find("particles").GetComponent<Collider2D>());
    }

    // UPDATE
    private void Update()
    {
        if (generate_continuously)
        {
            emitEndlessly();
        }
    }

    // XP EMISSION
    public void EmitXP(int count, Vector3 position,float strengh = 1f)
    {
        // on crée un EmitParams pour pouvoir changer la position de l'émission
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();

        for (int i = 0; i < count; i++)
        {
            // on change la position de l'émission
            // dans un rayon de radius autour de la position
            Vector2 position2D = Random.insideUnitCircle;
            emitParams.position = -transform.position + position + radius * new Vector3(position2D.x, position2D.y, 0);

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


            // on emet les particules
            ParticuleSystem.Emit(emitParams, 1);
        }
    }
    private void emitEndlessly()
    {
        EmitXP((int) generator_strengh, generator_position);
    }

    // TRIGGERS
    private void OnParticleTrigger()
    {
        if (Perso.Instance == null) { return; } // no player, no trigger

        // on récupère les particules
        int triggeredParticles = ParticuleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);

        if (log_triggers) { Debug.Log("(XPProvider) Triggered particles: " + triggeredParticles); }

        int life_bonus = 0;
        int xp_bonus = 0;

        // on change la life des particules
        for (int i = 0; i < triggeredParticles; i++)
        {
            ParticleSystem.Particle p = particles[i];

            // on regarde la couleur de la particule
            Color color = p.GetCurrentColor(ParticuleSystem);
            if (color == life_color)
            {
                // on ajoute de la life
                life_bonus += 1;
            }
            else
            {
                // on ajoute de l'xp
                xp_bonus += 1;
            }

            // on change la life de la particule
            p.remainingLifetime = 0;
            particles[i] = p;
        }

        // on applique les changements
        ParticuleSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);



        // on ajoute l'xp au player
        if (xp_bonus > 0) { Perso.Instance.addXP(xp_bonus); }

        // on ajoute de la life au player
        if (life_bonus > 0) { Perso.Instance.GetCapacity<HealthCapacity>().Heal(life_bonus); }
    }

}