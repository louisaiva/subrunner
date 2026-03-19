
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HealthCapacity is a capacity that allow any Capable to have some Health and Body colliders.
/// Which means it can take damage and be hurted. It also allows the being to heal and regen life.
/// </summary>

public class HealthCapacity : Capacity
{
    [Header("Health Capacity")]
    [SerializeField] protected float health = 100f;
    [SerializeField] protected int max_health = 100;
    [SerializeField] protected float regen_health = 0f; // en point de life par seconde
    public bool Alive { get { return health > 0f; } }
    public float LifePourcent { get { return health / max_health; } }
    public float Health { get { return health; } }
    public int MaxHealth { get { return max_health; } set { max_health = value; } }

    [Header("Body Colliders")]

    public List<Collider2D> _health_colliders;
    public List<Collider2D> HealthColliders
    {
        get
        {
            if (_health_colliders == null || _health_colliders.Count == 0)
            {
                _health_colliders = new List<Collider2D>(GetComponentsInChildren<Collider2D>(includeInactive:true));
            }
            return _health_colliders;
        }
    }
    public Collider2D HealthCollider { get { return HealthColliders.Count > 0 ? HealthColliders[0] : null; } }


    [Header("Hurted")]
    private string hurted_animation = "hurted";


    [Header("Logs")]
    [SerializeField] private bool log_taking_dmg = false;

    // START
    protected void Start()
    {
        // on initialise les capacités
        Capable.AddEffect(Effect.RegenLife, -888f);
    }


    // UPDATE
    protected void Update()
    {
        // on vérifie si le perso est mort
        if (!Alive) { return; }

        // on récupère les inputs
        UpdateBeingEffects();
    }
    protected void UpdateBeingEffects()
    {
        // boiling
        if (Capable.HasEffect(Effect.Boiling))
        {
            // on fait des dégats au being
            TakeDamage(5f * Time.deltaTime);
        }

        // life regen
        if (Capable.HasEffect(Effect.RegenLife) && health < max_health)
        {
            // life += regen_life * Time.deltaTime;
            AddLife(regen_health * Time.deltaTime);
        }

        // Invisible
        if (Capable.HasEffect(Effect.Invisible) && HealthCollider != null)
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
    public virtual bool TakeDamage(float damage, Force knockback = null)
    {
        if (!Alive) { return false; }

        // on vérifie si on est invincible
        if (Capable.HasEffect(Effect.Invincible))
        {
            // floating missing text
            FloatingDmgProvider.Instance.AddMissed(transform.position);
            return false;
        }

        // si on est ici on prend des dégats
        health -= damage;
        if (log_taking_dmg) { Debug.Log($"(HealthCapacity - TakeDamage) {name} took {damage} damage, life left: {health}"); }

        // knockback
        if (knockback != null && Capable is Movable movable) { movable.AddForce(knockback); }
        Vector2 knockback_inverse_direction = knockback != null ? -1f * knockback.direction : Vector2.zero;

        // play hurt animation
        if (!Capable.HasEffect(Effect.Unstoppable)) { Capable.AnimPlayer.PlayWithOrientation(hurted_animation, knockback_inverse_direction); }
        else if (knockback != null) { knockback.magnitude *= 0.125f; } // reduce knockback magnitude by 8

        // floating dmg
        FloatingDmgProvider.Instance.AddFloatingDmg(Capable, -1f * damage);

        // check if dead
        if (health <= 0f)
        {
            // if we can't die we come back to max health
            if (Capable.HasEffect(Effect.CantDie)) { health = MaxHealth; return true; } // we can't die

            // if we are part of the capable system we call CapableSystem.SwitchToCorpse(Capable)
            if (CapableBank.Instance.HasCapable(Capable))
            {
                CapableSystem.Instance.SwitchToCorpse(Capable);
                return true;
            }

            // else we are no part of the capable system (old way)
            // we call DieCapacity if it exists, else we just set health to 0
            Capable.GetCapacity<DieCapacity>()?.Use(Capable);
            health = 0f;
        }

        return true;
    }

    // DIE
    public virtual void Die()
    {
        if (Controller.Instance.Capable != Capable) { return; }
        
        // if we are controlled by a controller we reset the controller
        Controller.Instance.ResetCapableTarget();
    }


    // HEALING
    public virtual void AddLife(float life)
    {
        health += life;
        if (health > max_health)
        {
            FloatingDmgProvider.Instance.AddFloatingDmg(Capable, max_health - health); // floating dmg
            health = max_health;
            return;
        }

        // floating dmg
        FloatingDmgProvider.Instance.AddFloatingDmg(Capable, life);
    }
    public void Heal(int nb_heal = 2)
    {
        // each heal gives 10% of max life
        float heal = max_health * 0.1f * nb_heal;
        AddLife(heal);
    }
    public void HealMax() { AddLife(max_health - health); }

    // GIZMOS
    protected void OnDrawGizmos()
    {
        // on dessine le Collider de life du Being
        if (!HealthCollider) { return; }
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(HealthCollider.bounds.center, HealthCollider.bounds.size);
    }



    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data)
    {
        if (data is not HealthCapacityData hdata) { return; }

        // load health
        health = hdata.health;
        max_health = hdata.max_health;
        regen_health = hdata.regen_health;
        hurted_animation = hdata.hurted_animation;

        // load colliders
        foreach (BoxData colldata in hdata.box_colliders)
        {
            _health_colliders.Add(ColliderBank.Instance.LoadCollider(colldata,transform));
        }
        foreach (CircleData colldata in hdata.circle_colliders)
        {
            _health_colliders.Add(ColliderBank.Instance.LoadCollider(colldata,transform));
        }

        base.LoadData(data);
    }
    public override void UnloadData()
    {
        // unload colliders
        foreach (Collider2D collider in _health_colliders)
        {
            ColliderBank.Instance.UnloadCollider(collider.gameObject);
        }
        _health_colliders.Clear();

        // save dynamic health data
        SaveDynamicData();

        base.UnloadData();
    }

    // SAVE DYNAMIC DATA
    public void SaveDynamicData()
    {
        // update the dynamic fields data with the current values of the capacity
        if (this.data == null) { return; }
        if (this.data is not HealthCapacityData hdata) { return; }

        hdata.health = health;
        hdata.max_health = max_health;
        hdata.regen_health = regen_health;
        hdata.hurted_animation = hurted_animation;
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        HealthCapacityData static_data = new HealthCapacityData(base.GetStaticData())
        {
            health = this.health,
            max_health = this.max_health,
            regen_health = this.regen_health,
            hurted_animation = this.hurted_animation,
            box_colliders = new(),
            circle_colliders = new()
        };

        // we get the health colliders data
        foreach (Collider2D collider in HealthColliders)
        {
            if (collider is BoxCollider2D) { static_data.box_colliders.Add(ColliderBank.GetColliderData(collider) as BoxData); }
            else if (collider is CircleCollider2D) { static_data.circle_colliders.Add(ColliderBank.GetColliderData(collider) as CircleData); }
        }

        return static_data;
    }
}

[Serializable] public class HealthCapacityData : CapacityData
{
    // global health parameters
    public float health;
    public int max_health;
    public float regen_health;
    public string hurted_animation;

    // health colliders
    public List<BoxData> box_colliders;
    public List<CircleData> circle_colliders;




    // CONSTRUCTOR
    public HealthCapacityData(CapacityData parent)
    {
        foreach (var prop in parent.GetType().GetProperties()) { prop.SetValue(this, prop.GetValue(parent)); }
        foreach (var prop in parent.GetType().GetFields()) { prop.SetValue(this, prop.GetValue(parent)); }
    }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new HealthCapacityData(base.Duplicate() as CapacityData)
        {
            health = this.health,
            max_health = this.max_health,
            regen_health = this.regen_health,
            hurted_animation = this.hurted_animation,
            box_colliders = new List<BoxData>(this.box_colliders),
            circle_colliders = new List<CircleData>(this.circle_colliders)
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - health : {health}\n";
        details += $"  - max_health : {max_health}\n";
        details += $"  - regen_health : {regen_health}\n";
        details += $"  - hurted_animation : {hurted_animation}\n";
        details += $"  - health_colliders :\n";
        details += $"     - box_colliders : {(box_colliders != null ? box_colliders.Count.ToString() : "null")}\n";
        details += $"     - circle_colliders : {(circle_colliders != null ? circle_colliders.Count.ToString() : "null")}\n";
        return base.GetDetails() + details;
    }
}