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


        // todo make this global through capable.LoadData() or something so any capable can be a trigger for the xp provider
        /* if (Controller.Perso == null) { return; } // no player, no trigger
        ParticuleSystem.trigger.SetCollider(0, Controller.Perso.transform.Find("particles").GetComponent<Collider2D>()); */
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
    private List<ParticleCollisionEvent> triggered_particles = new List<ParticleCollisionEvent>();
    private void OnParticleCollision(GameObject receiver)
    {
        if (log_triggers) { Debug.Log("(XPProvider) Particles are colliding with : " + receiver.name); }

        // check if receiver has a ExpCapacity
        if (!receiver.TryGetComponent(out Capable capable)) { return; }
        if (!capable.TryGetCapacity(out ExpCapacity exp_capacity)) { return; }

        // on récupère les particules
        if (log_triggers)
        {
            int triggeredParticles = ParticuleSystem.GetCollisionEvents(receiver, triggered_particles);
            Debug.Log("(XPProvider) Colliding particles: " + triggeredParticles);
        }

        exp_capacity.AddXP(1);
    }


    // REGISTER TRIGGERS
    public void RegisterTrigger(Collider2D collider)
    {
        ParticuleSystem.trigger.AddCollider(collider);
    }
    public void UnregisterTrigger(Collider2D collider)
    {
        ParticuleSystem.trigger.RemoveCollider(collider);
    }
}