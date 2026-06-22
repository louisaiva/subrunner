using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderAttackCapacity : ColliderCapacity
{
    [Header("Collider Attack Capacity")]
    [SerializeField] private bool is_attacking;
    [SerializeField] private float knockback_magnitude;
    [SerializeField] private float damage;
    [SerializeField] private int kills = 0;
    [SerializeField] private List<string> excluded_tags = new List<string> { };

    [Header("Debug")]
    [SerializeField] private bool log_colliders = false;
    [SerializeField] private bool log_health_collider = false;

    // COLLISION ENTER
    private void OnTriggerStay2D(Collider2D other) { OnTriggerEnter2D(other); }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (log_colliders) { Debug.Log("(ColliderAttackCapacity) Collider entered: " + other.name); }

        // we check if we are attacking
        if (!is_attacking) { return; }

        // we don't attack ourselves
        if (TryGetSiblingCapacity(out HealthCapacity health) && health.HealthColliders.Contains(other))
        {
            if (log_colliders) { Debug.Log("(ColliderAttackCapacity) Collider " + other.name + " belongs to the bearer, we don't attack ourselves"); }
            return;
        }

        // we check if it has a HealthCapacity component
        HealthCapacity enemy_health_capa = other.GetComponentInParent<HealthCapacity>(includeInactive: true);
        if (enemy_health_capa == null)
        {
            if (log_colliders) { Debug.Log("(ColliderAttackCapacity) No health capacity found on collider " + other.name); }
            return;
        }

        if (!enemy_health_capa.Alive)
        {
            if (log_colliders) { Debug.Log("(ColliderAttackCapacity) Health capacity found on collider " + other.name + " but it's not alive"); }
            return;
        }

        Vector2 impact_point = GetClosestPoint(other, enemy_health_capa);
        on_health_capa_enter(enemy_health_capa, impact_point);
    }
    private Vector2 GetClosestPoint(Collider2D collider, HealthCapacity health_capa)
    {
        Vector2 closest_point = new Vector2(float.MaxValue, float.MaxValue);
        float min_distance = float.MaxValue;
        foreach (CircleCollider2D circle in circles_colliders)
        {
            if (circle == null) { continue; }
            Vector2 point = circle.ClosestPoint(health_capa.transform.position);
            float distance = Vector2.Distance(point, health_capa.transform.position);
            if (distance > min_distance) { continue; }
            min_distance = distance;
            closest_point = point;
        }
        foreach (BoxCollider2D box in box_colliders)
        {
            if (box == null) { continue; }
            Vector2 point = box.ClosestPoint(health_capa.transform.position);
            float distance = Vector2.Distance(point, health_capa.transform.position);
            if (distance > min_distance) { continue; }
            min_distance = distance;
            closest_point = point;
        }
        return closest_point;
    }
    private void on_health_capa_enter(HealthCapacity health_capa, Vector2 impact_point)
    {
        if (log_health_collider) { Debug.Log("(ColliderAttackCapacity) Health collider entered: " + health_capa.Capable.ID); }

        // we remove not attackable tags
        if (excluded_tags.Contains(health_capa.gameObject.tag)) { return; }

        // we deal some damage to the health capacity
        deal_damage(health_capa, impact_point);
    }

    // DAMAGE AND KNOCKBACK CALCULATION
    private bool deal_damage(HealthCapacity health_capa, Vector2 impact_point)
    {
        // get the capable of the health capa
        Capable enemy = health_capa.Capable;
        if (enemy == null) { return false; }

        // get direction and weight of enemy
        Vector2 final_point = impact_point; // we use the impact point as reference for knockback direction
        if (enemy.transform.position.y > transform.position.y)
        {
            final_point.y = transform.position.y;
        }
        float dx = enemy.transform.position.x - final_point.x;
        float dy = enemy.transform.position.y - final_point.y;
        Vector2 direction_enemy = new Vector2(dx, dy);

        // calculate knockback magnitude proportionnal to weight
        Force knockback = new Force("knockback", direction_enemy.normalized, knockback_magnitude);

        // apply damage and knockback
        health_capa.TakeDamage(damage, knockback);

        // check if enemy is dead
        if (!health_capa.Alive)
        {
            kills += 1;
            DieEngine.Instance.Die(enemy, Capable);
            return true;
        }
        return false;
    }



    ///
    //
    /// DATA MANAGEMENT
    //
    ///

    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData capable_data)
    {
        base.LoadData(data, capable_data);

        if (data is not ColliderAttackData cad) { return; }
        
        this.excluded_tags = new List<string>(cad.excluded_tags);
        this.knockback_magnitude = cad.knockback_magnitude;
        this.damage = cad.damage;
        this.is_attacking = cad.is_attacking;
        this.kills = cad.kills;
    }
    public override void SaveDynamicData()
    {
        base.SaveDynamicData();
        if (data is not ColliderAttackData cad) { return; }

        cad.is_attacking = this.is_attacking;
        cad.kills = this.kills;
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        ColliderAttackData static_data = new ColliderAttackData(base.GetStaticData())
        {
            excluded_tags = new List<string>(this.excluded_tags),
            knockback_magnitude = this.knockback_magnitude,
            damage = this.damage,
            is_attacking = this.is_attacking,
            kills = this.kills
        };

        // set more complicated data fields check here if needed

        return static_data;
    }
}
[Serializable] public class ColliderAttackData : ColliderCapacityData
{
    public List<string> excluded_tags = new List<string> { };
    public float knockback_magnitude;
    public float damage;
    [InstanceSpecific] public bool is_attacking;
    [InstanceSpecific] public int kills = 0;

    // CONSTRUCTOR
    public ColliderAttackData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new ColliderAttackData(base.Duplicate() as CapacityData)
        {
            excluded_tags = new List<string>(excluded_tags),
            knockback_magnitude = knockback_magnitude,
            damage = damage,
            is_attacking = is_attacking,
            kills = kills
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        // add details to the string here
        // ex : details += $"  - slot color : {slot_color}\n";
        details += $"  - is attacking : {is_attacking}\n";
        details += $"  - damage : {damage}\n";
        details += $"  - kills : {kills}\n";
        details += $"  - excluded tags : {string.Join(", ", excluded_tags)}\n";
        details += $"  - knockback magnitude : {knockback_magnitude}\n";
        return base.GetDetails() + details;
    }
}