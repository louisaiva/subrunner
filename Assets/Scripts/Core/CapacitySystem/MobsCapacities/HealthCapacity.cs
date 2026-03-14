
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

    public List<Collider2D> _body_colliders;
    public List<Collider2D> BodyColliders
    {
        get
        {
            if (_body_colliders == null || _body_colliders.Count == 0)
            {
                _body_colliders = new List<Collider2D>(Capable.body.GetComponentsInChildren<Collider2D>(includeInactive:true));
            }
            return _body_colliders;
        }
    }
    public Collider2D body_collider { get { return BodyColliders.Count > 0 ? BodyColliders[0] : null; } }


    [Header("Hurted")]
    private string hurted_animation = "hurted";


    [Header("Logs")]
    [SerializeField] private bool log_taking_dmg = false;


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
        if (Capable.HasEffect(Effect.Invisible))
        {
            // change the body collider to Ghosts layer
            body_collider.gameObject.layer = LayerMask.NameToLayer("Ghosts");
        }
        else
        {
            // reset the body collider to Beings layer
            body_collider.gameObject.layer = LayerMask.NameToLayer("Beings");
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

        // play hurt animation
        // // todo make the health capa control replace entirely hurted capacity -> no need for it
        if (!Capable.HasEffect(Effect.Unstoppable)) { Capable.AnimPlayer.Play(hurted_animation); }
        else if (knockback != null) { knockback.magnitude *= 0.125f; } // reduce knockback magnitude by 8

        // knockback
        if (knockback != null && Capable is Movable movable)
        {
            movable.AddForce(knockback);

            // change the flipX of the renderers if needed
            if (knockback.direction.x != 0f) { Capable.AnimPlayer.FlipCurrentAnim(knockback.direction.x < 0f); }
        }

        // floating dmg
        FloatingDmgProvider.Instance.AddFloatingDmg(Capable, -1f * damage);

        // check if dead
        if (health <= 0f) { Capable.GetCapacity<DieCapacity>().Use(Capable); }

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
        if (!body_collider) { return; }
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(body_collider.bounds.center, body_collider.bounds.size);
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
        foreach (ColliderData colldata in hdata.body_colliders)
        {
            _body_colliders.Add(ColliderBank.Instance.LoadCollider(colldata,transform));
        }

        base.LoadData(data);
    }
    public override void UnloadData()
    {
        // unload colliders
        foreach (Collider2D collider in _body_colliders)
        {
            ColliderBank.Instance.UnloadCollider(collider.gameObject);
        }
        _body_colliders.Clear();

        base.UnloadData();
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
            body_colliders = new()
        };

        // we get the health colliders data
        foreach (Collider2D collider in BodyColliders)
        {
            static_data.body_colliders.Add(get_collider_data(collider));
        }

        return static_data;
    }
    private ColliderData get_collider_data(Collider2D collider)
    {
        // todo move this method to a static-friendly-Instance helper i guess like GameManager ?

        // setup basic data
        ColliderData data = new ColliderData
        {
            local_position = collider.transform.localPosition,
            layerID = collider.gameObject.layer,
            offset = collider.offset,
            is_trigger = collider.isTrigger,
            used_for_pathfinding = false // health colliders are never used for pathfinding
        };

        // check if circle
        if (collider is CircleCollider2D circle)
        {
            return new CircleData(data)
            {
                radius = circle.radius
            };
        }

        // check if box
        if (collider is BoxCollider2D box)
        {
            return new BoxData(data)
            {
                size = box.size
            };
        }

        return data;
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
    public List<ColliderData> body_colliders;




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
            body_colliders = new List<ColliderData>(this.body_colliders)
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
        details += $"  - body_colliders : {body_colliders.Count}\n";
        return base.GetDetails() + details;
    }
}