using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// AttackCapacity is a capacity that allows a being to attack
/// It is a collider that is enabled during the attack animation
/// </summary>

public class AttackCapacity : CooldownCapacity
{
    // logs
    public bool log_colliders = false;
    public bool log_health_collider = false;




    [Header("Damage parameters")]
    public float distance_to_attack = 1f;
    public int kills = 0;
    public float damage = 10f;
    [SerializeField] private float random_damage_modifier_at_start = 0; // damage += random.range(-5,5) in the start method if this modifier = 5
    public bool IsAttacking = false; // runtime only

    [Header("Enemies parameters")]
    [SerializeField] private List<string> base_excluded_tags = new List<string> { };


    [Header("Attack parameters")]
    [SerializeField] private bool single_hit = false; // if true, the attack will stop after hitting one enemy
    [SerializeField] private bool split_damage = false; // if true, the damage dealt will be split between all hit enemies, if false, each enemy will receive the full damage (overpowered but funier)
    [SerializeField] private bool perforant_attack = false; // if true, each touched enemy will got full damage
    [SerializeField] private float attack_duration = default;
    [SerializeField] private float attack_duration_random_variation = 0f; // random variation of the attack duration

    [Header("Unstoppable parameters")]
    [SerializeField] private bool unstoppable = false; // confers the Unstoppable effect during the attack -> can't be hurted, means will allways attack
    [SerializeField] private float unstoppable_rate = 1; // when unstoppable is true, percentage of an attack to trigger unstoppable effect (0 never to 1 always)

    [Header("Knockback parameters")]
    public float knockback_base = 10f; // une attaque répartit le knockback
    public float attackant_advantage = 3f;

    [Header("Screenshake parameters")]
    [SerializeField][Range(0f, 1f)] private float base_attack_shake_magnitude = 0.5f; // magnitude of the screen shake when attacking
    [SerializeField][Range(0f, 1f)] private float base_kill_shake_magnitude = 0.8f; // magnitude of the screen shake when kill performed

    [Header("Bearer")]
    private Capable bearer; // the capable that is using this attack capacity

    [Header("Components")]
    private SpriteBank bank;
    private SpriteRenderer sr;
    private AnimPlayer anim_player;
    private PolygonCollider2D pc;


    // EVENTS
    public Action<HealthCapacity, float> OnDamageDealt = delegate { };

    
    // START
    private void Start()
    {
        // we get the polygon collider
        pc = GetComponent<PolygonCollider2D>();
        pc.enabled = false;

        // we get the sprite bank
        bank = AnimBank.Instance.GetComponent<SpriteBank>();

        // we set the damage variable
        damage += UnityEngine.Random.Range(-random_damage_modifier_at_start, random_damage_modifier_at_start);

        ResetTags();
    }



    ///
    //
    /// USE & COLLISION DETECTION
    //
    ///


    // USE
    public override void Use(Capable capable)
    {
        // we set the bearer and its components
        bearer = capable;
        anim_player = bearer.AnimPlayer;
        sr = bearer.AnimPlayer.Renderer;
        if (anim_player.IsShowing("attack")) { return; } // we check if we are already attacking

        // we update damage value if capable is Perso
        // if (bearer is Perso perso) { damage = perso.skillManager.GetSkillValue("stat:damage"); }

        // we calculate the duration of the attack
        float duration_override = attack_duration;
        if (duration_override != default && attack_duration_random_variation > 0)
        {
            duration_override += UnityEngine.Random.Range(-attack_duration_random_variation, attack_duration_random_variation);
            duration_override = Mathf.Max(0.01f, duration_override); // we make sure the duration is not negative
        }

        // we play the animation
        Anim anim = anim_player.Play("attack", duration_override: duration_override);
        if (anim == null) { return; } // if the animation is not found, we return

        // we start the cooldown for the time of the animation
        float anim_duration = anim.GetDuration();
        startCooldown(anim_duration);
        IsAttacking = true;
        hitted_health_capa.Clear();

        // we check if we need to turn on unstoppable effect
        if (unstoppable && UnityEngine.Random.Range(0f, 1f) < unstoppable_rate)
        {
            bearer.AddEffect(Effect.Unstoppable, -888f); // infinite unstoppable
        }
    }

    // COLLISION ENTER
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (log_colliders) { Debug.Log("(AttackCapacity) Collider entered: " + other.name); }

        // we check if we are attacking
        if (!IsAttacking) { return; }
        if (!bearer.AnimPlayer.IsShowing("attack")) { return; }

        // we check if the pc is enabled
        if (!pc.enabled) { return; }

        // we don't attack ourselves
        if (bearer.TryGetCapacity(out HealthCapacity health) && health.HealthColliders.Contains(other))
        {
            if (log_colliders) { Debug.Log("(AttackCapacity) Collider " + other.name + " belongs to the bearer, we don't attack ourselves"); }
            return;
        }

        // we check if it has a HealthCapacity component
        HealthCapacity enemy_health_capa = other.GetComponentInParent<HealthCapacity>(includeInactive: true);
        // ! perf issue ???
        // ? comment on peut récupérer plus efficacement la health capa ?
        // à part faire un dict collider -> capacities dans ColliderBank mais wtf

        if (enemy_health_capa == null)
        {
            if (log_colliders) { Debug.Log("(AttackCapacity) No health capacity found on collider " + other.name); }
            return;
        }

        if (!enemy_health_capa.Alive)
        {
            if (log_colliders) { Debug.Log("(AttackCapacity) Health capacity found on collider " + other.name + " but it's not alive"); }
            return;
        }

        on_health_capa_enter(enemy_health_capa);
    }
    private void on_health_capa_enter(HealthCapacity health_capa)
    {
        if (log_health_collider) { Debug.Log("(AttackCapacity) Health collider entered: " + health_capa.Capable.ID); }

        // we remove not attackable tags
        if (excluded_tags.Contains(health_capa.gameObject.tag)) { return; }


        // we add it to the list of hitted health capa that 
        hitted_health_capa.Add(health_capa);
        if (log_health_collider) { Debug.Log($"(AttackCapacity) {health_capa.Capable.ID} is the new hitted health capacity !"); }
    }




    ///
    //
    /// ATTACK UPDATE & ATTACK MANAGEMENT
    //
    ///

    // UPDATE
    protected override void Update()
    {
        base.Update();

        if (!IsAttacking) { return; }
        if (!anim_player.IsShowing("attack"))
        {
            // checks if we are still attacking & the animation is not the attack animation anymore
            stop_attack();
            return;
        }

        // we check if the sprite has a collider
        Sprite sprite = sr.sprite;
        if (!bank.HasDamageCollider(sprite)) { pc.enabled = false; return; }
        pc.enabled = true;

        // we update the collider
        updateCollider(sprite);

        // if target we update the attack
        if (hitted_health_capa.Count > 0) { updateAttack(); }
    }
    private void updateCollider(Sprite sprite)
    {
        // we flip the collider if the sprite is flipped
        if (sr.flipX && transform.localScale.x > 0) { transform.localScale = new Vector3(-1, 1, 1); }
        else if (!sr.flipX && transform.localScale.x < 0) { transform.localScale = new Vector3(1, 1, 1); }

        // update count
        pc.pathCount = sprite.GetPhysicsShapeCount();

        // new paths variable
        List<Vector2> path = new List<Vector2>();

        // loop path count
        for (int i = 0; i < pc.pathCount; i++)
        {
            // clear
            path.Clear();
            // get shape
            sprite.GetPhysicsShape(i, path);

            // set path
            pc.SetPath(i, path.ToArray());
        }

        // Debug.Log("Updated pc to sprite: " + sprite.name + " with " + pc.pathCount + " points");
    }



    // ATTACK UPDATE
    private List<HealthCapacity> hitted_health_capa = new List<HealthCapacity> { }; // RTO
    private int EnemyCount => hitted_health_capa.Count; // RTO
    private void updateAttack()
    {
        if (log)
        {
            string hit_enemies_str = Capable.name + " attacked enemies : " + EnemyCount + " :\n";
            foreach (HealthCapacity health_capa_ in hitted_health_capa)
            {
                hit_enemies_str += "\t" + health_capa_.Capable.name + " (health capa)\n";
            }
            Debug.Log(hit_enemies_str);
        }

        // calculate damage dealt to single target
        float single_target_damage
                            = perforant_attack || single_hit // also if single hit we don't care we will apply damage once (so we skip the division)
                            ? damage // if perforant attack, all enemies will receive the full damage
                            : damage / EnemyCount;

        // calculate knockback weights
        float total_knockback_weight = calculate_knockabcks();
        Vector2 attacker_knockback_direction = Vector2.zero;
        bool killed_an_enemy = false;


        // if we single attack we want to make sure to attack closest enemy
        if (single_hit)
        {
            // we sort the hit enemies by distance to the attacker
            hitted_health_capa = hitted_health_capa.OrderBy(health_capa => Vector2.Distance(transform.position, health_capa.Capable.transform.position)).ToList();
        }

        // deal damage to health capa target
        HealthCapacity health_capa;
        for (int i = 0; i < hitted_health_capa.Count; ++i)
        {
            health_capa = hitted_health_capa[i];
            if (health_capa == null) { continue; }
            applyDamageToHealthCapa(health_capa, single_target_damage, total_knockback_weight, ref attacker_knockback_direction);

            // if we have a single_hit attack we break the loop
            if (single_hit) { break; }
        }


        // we stop the attack for this frame
        hitted_health_capa.Clear();
        if (single_hit) { IsAttacking = false; } // we stop definitely the attack if single hit
        if (bearer is not Movable movable) { return; }


        // on shake la caméra
        if (bearer is Perso)
        {
            CameraShaker.Instance.Shake(killed_an_enemy ? base_kill_shake_magnitude : base_attack_shake_magnitude);
        }

        // apply knockback to attacker
        float knockback_magnitude_inverse = knockback_base * (total_knockback_weight - movable.weight)
                                                / (total_knockback_weight * attackant_advantage);
        Force knockback_inverse = new Force("knockback", attacker_knockback_direction.normalized, knockback_magnitude_inverse);
        movable.AddForce(knockback_inverse);
    }
    private void stop_attack()
    {
        IsAttacking = false;
        hitted_health_capa.Clear();
        if (bearer != null)
        {
            bearer.RemoveEffect(Effect.Unstoppable);
            bearer.AnimPlayer.StopPlaying("attack");
        }
    }


    // DAMAGE AND KNOCKBACK CALCULATION
    private bool applyDamageToHealthCapa(HealthCapacity health_capa, float single_target_damage, float total_knockback_weight, ref Vector2 attacker_knockback_direction)
    {
        // get the capable of the health capa
        Capable enemy = health_capa.Capable;
        if (enemy == null) { return false; }

        // get direction and weight of enemy
        float dx = enemy.transform.position.x - transform.position.x;
        float dy = enemy.transform.position.y - transform.position.y;
        Vector2 direction_enemy = new Vector2(dx, dy);
        float enemy_weight = enemy is Movable movable ? movable.weight : 5f;

        // calculate knockback magnitude proportionnal to weight
        float knockback_magnitude = knockback_base * (total_knockback_weight - enemy_weight)
                                     / total_knockback_weight;
        Force knockback = new Force("knockback", direction_enemy.normalized, knockback_magnitude);
        attacker_knockback_direction += -direction_enemy.normalized * knockback_magnitude;

        // apply damage and knockback
        float dealt_damage = split_damage ? single_target_damage : damage;
        OnDamageDealt?.Invoke(health_capa, dealt_damage); // before applying damage bcz we want to have a data to process, if the prey dies from the attack it unloads the data
        health_capa.TakeDamage(dealt_damage, knockback);

        // check if enemy is dead
        if (!health_capa.Alive) { kills += 1; return true; }
        return false;
    }

    // KNOCKBACK
    private float? _advantage_attacker_weight;
    private float advantage_attacker_weight // l'attaquant a un avantage de poids afin de recevoir moins de knockback
    {
        get
        {
            if (_advantage_attacker_weight != null) { return _advantage_attacker_weight.Value; }

            // else we just pooled our capa so we need to calculate the advantage_attacker_weight
            if (bearer is not Movable movable)
            {
                _advantage_attacker_weight = 5f * attackant_advantage;
            }
            else
            {
                _advantage_attacker_weight = movable.weight * attackant_advantage;
            }
            return _advantage_attacker_weight.Value;
        }
    }
    private float calculate_knockabcks()
    {
        float total_knockback_weight = advantage_attacker_weight;
        for (int i = 0; i < hitted_health_capa.Count; ++i)
        {
            HealthCapacity health_capa = hitted_health_capa[i];
            if (health_capa == null) { continue; }
            if (health_capa.Capable is not Movable movable)
            {
                total_knockback_weight += 5f;
                continue;
            }
            total_knockback_weight += movable.weight;
        }
        return total_knockback_weight;
        // + hitted_health_capa.Sum(health_capa => health_capa.Capable is Movable movable ? movable.weight : 5f);
    }


    // WHITE LISTING
    private List<string> excluded_tags = new List<string> { }; // RTO
    public async void WhiteListTagShortly(string tag, float duration)
    {
        // we check if the tag is not already in the list
        if (excluded_tags.Contains(tag)) { return; }

        if (log)
        {
            Debug.Log("(AttackCapacity) Adding tag " + tag + " to the not attackable tags for " + duration + " seconds");
        }

        // we add the tag to the list
        excluded_tags.Add(tag);

        // wait for a frame to let the click happen
        await System.Threading.Tasks.Task.Delay((int)(duration * 1000));

        if (log)
        {
            Debug.Log("(AttackCapacity) Removing tag " + tag + " from the not attackable tags");
        }
        // we remove the tag from the list
        excluded_tags.Remove(tag);
    }
    public void ResetTags()
    {
        excluded_tags = new List<string>(base_excluded_tags);
    }
    public void ClearTags()
    {
        excluded_tags.Clear();
    }




    ///
    //
    /// DATA MANAGEMENT
    //
    ///


    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data)
    {
        if (data is not AttackData adata) { return; }

        // we load the static data
        distance_to_attack = adata.distance_to_attack;
        single_hit = adata.single_hit;
        split_damage = adata.split_damage;
        perforant_attack = adata.perforant_attack;
        attack_duration = adata.attack_duration;
        attack_duration_random_variation = adata.attack_duration_random_variation;
        unstoppable_rate = adata.unstoppable_rate;
        unstoppable = unstoppable_rate > 0;

        // we load the excluded tags
        base_excluded_tags = new List<string>(adata.base_excluded_tags);
        ResetTags();

        // and damage
        damage = adata.damage;

        base.LoadData(data);
    }
    public override void UnloadData()
    {
        _advantage_attacker_weight = null;

        // if we are attacking we stop the attack
        if (IsAttacking) { stop_attack(); }
        base.UnloadData();
    }

    // SAVE DYNAMIC DATA
    public override void SaveDynamicData()
    {
        base.SaveDynamicData();
        if (data is not AttackData adata) { return; }
        adata.damage = damage;
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        AttackData static_data = new AttackData(base.GetStaticData())
        {
            template_damage = damage,
            random_damage_modifier = random_damage_modifier_at_start,
            distance_to_attack = distance_to_attack,
            single_hit = single_hit,
            split_damage = split_damage,
            perforant_attack = perforant_attack,
            attack_duration = attack_duration,
            attack_duration_random_variation = attack_duration_random_variation,
            unstoppable_rate = unstoppable ? unstoppable_rate : 0f,
            base_excluded_tags = new List<string>(base_excluded_tags),

            // instance parameters are not included in static data
            damage = damage + UnityEngine.Random.Range(-random_damage_modifier_at_start, random_damage_modifier_at_start)
        };

        return static_data;
    }
}


[Serializable] public class AttackData : CapacityData
{

    // TYPE DATA (static at runtime, one per template)
    public float template_damage;
    public float random_damage_modifier; // replace with an "exp" system for mobs ?
    public float distance_to_attack;
    public bool single_hit, split_damage, perforant_attack;
    public float attack_duration, attack_duration_random_variation;
    public float unstoppable_rate;

    // knocback_base ?
    // attackant_advantage ?

    // excluded tags
    public List<string> base_excluded_tags = new List<string> { };

    // instance parameters
    public float damage;
    // kills ?


    // CONSTRUCTOR
    public AttackData(CapacityData parent)
    {
        foreach (var prop in parent.GetType().GetProperties()) { prop.SetValue(this, prop.GetValue(parent)); }
        foreach (var prop in parent.GetType().GetFields()) { prop.SetValue(this, prop.GetValue(parent)); }
    }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new AttackData(base.Duplicate() as CapacityData)
        {
            template_damage = template_damage,
            random_damage_modifier = random_damage_modifier,
            distance_to_attack = distance_to_attack,
            single_hit = single_hit,
            split_damage = split_damage,
            perforant_attack = perforant_attack,
            attack_duration = attack_duration,
            attack_duration_random_variation = attack_duration_random_variation,
            unstoppable_rate = unstoppable_rate,
            base_excluded_tags = new List<string>(base_excluded_tags),

            // instance parameters
            damage = template_damage + UnityEngine.Random.Range(-random_damage_modifier, random_damage_modifier)
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - template damage : {template_damage} (+/- {random_damage_modifier})\n";
        details += $"  - distance to attack : {distance_to_attack}\n";
        details += $"  - single hit : {single_hit}\n";
        details += $"  - split damage : {split_damage}\n";
        details += $"  - perforant attack : {perforant_attack}\n";
        details += $"  - attack duration : {attack_duration} (+/- {attack_duration_random_variation})\n";
        details += $"  - unstoppable rate : {unstoppable_rate}\n";
        details += $"  - excluded tags : {string.Join(", ", base_excluded_tags)}\n";
        details += $"  - damage : {damage}\n";
        return base.GetDetails() + details;
    }
}
