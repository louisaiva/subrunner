using System;
using System.Collections.Generic;
using UnityEngine;


[Obsolete("Use HealthCapacity instead")] public class Being : Movable
{

    [Header("BEING")]
    protected float health = 100f;
    protected int max_health = 100;
    [SerializeField] private int random_life_modifier_at_start = 0; // max_life += random.range(-5,5) in the start method if this modifier = 5

    public bool Alive { get { return health > 0f; } }
    public float LifePourcent { get { return health / (float)max_health; } }
    public float Health { get { return health; } }
    public int MaxHealth { get { return max_health; } set { this.max_health = value; } }
    public float regen_life = 0f; // en point de life par seconde

    public List<Collider2D> _health_colliders;
    public List<Collider2D> HealthColliders
    {
        get
        {
            if (_health_colliders == null || _health_colliders.Count == 0)
            {
                Transform body_transform = transform.Find("body");
                if (body_transform == null) { return new List<Collider2D>(); }
                _health_colliders = new List<Collider2D>(body_transform.GetComponentsInChildren<Collider2D>(includeInactive:true));
            }
            return _health_colliders;
        }
    }
    public Collider2D HealthCollider { get { return HealthColliders.Count > 0 ? HealthColliders[0] : null; } }

    public int body_meats = 1; // nombre de viande dans le corps du being
    public int body_bones = 1; // nombre d'os dans le corps du being

    [Header("Logs")]
    [SerializeField] private bool log_taking_dmg = false;

    // START
    protected override void Start()
    {
        base.Start();

        // on initialise les capacités
        AddCapacity("hurted");
        AddEffect(Effect.RegenLife, -888f);

        // on initialise la vie
        max_health = max_health + UnityEngine.Random.Range(-random_life_modifier_at_start, random_life_modifier_at_start);
        health = (float)max_health;
    }



    // UPDATE
    protected override void Update()
    {
        // on vérifie si le perso est mort
        if (!Alive)
        {
            inputs = Vector2.zero;
            base.Update();
            return;
        }

        // on récupère les inputs
        UpdateBeingEffects();

        base.Update();
    }
    public virtual void UpdateBeingEffects()
    {
        // boiling
        if (HasEffect(Effect.Boiling))
        {
            // on fait des dégats au being
            TakeDamage(5f * Time.deltaTime);
        }

        // life regen
        if (HasEffect(Effect.RegenLife) && health < max_health)
        {
            // life += regen_life * Time.deltaTime;
            AddLife(regen_life * Time.deltaTime);
        }

        // Invisible
        if (HasEffect(Effect.Invisible) && HealthCollider != null)
        {
            // change the body collider to Ghosts layer
            HealthCollider.gameObject.layer = LayerMask.NameToLayer("Ghosts");
        }
        else if (HealthCollider != null)
        {
            // reset the body collider to Beings layer
            HealthCollider.gameObject.layer = LayerMask.NameToLayer("Beings");
        }
    }


    // DAMAGE
    public virtual bool TakeDamage(float damage, Force knockback=null)
    {
        if (!Alive) { return false; }

        // on vérifie si on est invincible
        if (HasEffect(Effect.Invincible))
        {
            // floating missing text
            FloatingDmgProvider.Instance.AddMissed(transform.position);
            return false;
        }

        // si on est ici on prend des dégats
        health -= damage;
        if (log_taking_dmg) { Debug.Log($"(Being - TakeDamage) {name} took {damage} damage, life left: {health}"); }

        // play hurt animation
        if (!HasEffect(Effect.Unstoppable))
        {
            GetCapacity("hurted")?.Use(this);
        }
        else { knockback.magnitude *= 0.125f; } // reduce knockback magnitude by 8

        // knockback
        if (knockback != null)
        {
            AddForce(knockback);

            // change the flipX of the renderers if needed
            if (knockback.direction.x != 0f) { AnimPlayer.FlipCurrentAnim(knockback.direction.x < 0f); }
        }

        // floating dmg
        FloatingDmgProvider.Instance.AddFloatingDmg(this,-1f * damage);

        // check if dead
        if (health <= 0f) { GetCapacity<DieCapacity>().Use(this); }

        return true;
    }

    // DIE
    public virtual void Die()
    {
        // if we are controlled by a controller we reset the controller
        if (Controller.Instance.Capable == this)
        {
            Controller.Instance.ResetCapableTarget();
        }
    }    


    // SETTERS
    public virtual void AddLife(float life)
    {
        this.health += life;
        if (this.health > max_health)
        {
            // floating dmg
            FloatingDmgProvider.Instance.AddFloatingDmg(this, max_health - this.health);

            this.health = max_health;
        }

        // floating dmg
        FloatingDmgProvider.Instance.AddFloatingDmg(this, life);
    }
    public void Heal(int nb_heal=2)
    {
        // each heal gives 10% of max life
        float heal = max_health * 0.1f * nb_heal;
        AddLife(heal);
    }
    public void HealMax()
    {
        // restore max life
        AddLife(max_health-health);
    }



    // gizmos
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        // on dessine le Collider de life du Being
        if (!HealthCollider) { return; }
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(HealthCollider.bounds.center, HealthCollider.bounds.size);
    }
}