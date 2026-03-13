using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// AttackCapacity is a capacity that allows a being to attack
/// It is a collider that is enabled during the attack animation
/// </summary>

public class AttackCapacity : CooldownCapacity
{
    [Header("Damage parameters")]
    public float distance_to_attack = 1f;
    public int kills = 0;
    public float damage = 10f;
    [SerializeField] private float random_damage_modifier_at_start = 0; // damage += random.range(-5,5) in the start method if this modifier = 5
    public bool IsAttacking = false;
    [SerializeField] List<Being> hit_enemies = new List<Being> { };
    [SerializeField] private List<string> base_excluded_tags = new List<string> { };
    private List<string> excluded_tags = new List<string> { };


    [Header("Attack parameters")]
    [SerializeField] private bool single_hit = false; // if true, the attack will stop after hitting one enemy
    [SerializeField] private bool perforant_attack = false; // if true, each touched enemy will got full damage
    [SerializeField] private float attack_duration = default;
    [SerializeField] private float attack_duration_random_variation = 0f; // random variation of the attack duration

    [Header("Unstoppable parameters")]
    [SerializeField] private bool unstoppable = false; // confers the Unstoppable effect during the attack -> can't be hurted, means will allways attack
    [SerializeField] private float unstoppable_rate = 1; // when unstoppable is true, percentage of an attack to trigger unstoppable effect (0 never to 1 always)

    [Header("Knockback parameters")]
    public float knockback_base = 10f; // une attaque répartit le knockb
    public float attackant_advantage = 3f;

    [Header("Screen shake parameters")]
    [SerializeField][Range(0f, 1f)] private float base_attack_shake_magnitude = 0.5f; // magnitude of the screen shake when attacking
    [SerializeField][Range(0f, 1f)] private float base_kill_shake_magnitude = 0.8f; // magnitude of the screen shake when kill performed

    [Header("Bearer")]
    private Capable bearer; // the capable that is using this attack capacity
    private Being being
    {
        get
        {
            if (bearer == null || !(bearer is Being)) { return null; }
            return bearer as Being;
        }
    }

    [Header("Components")]
    private SpriteBank bank;
    private SpriteRenderer sr;
    private AnimPlayer anim_player;
    private PolygonCollider2D pc;


    // START
    private void Start()
    {
        // we get the polygon collider
        pc = GetComponent<PolygonCollider2D>();
        pc.enabled = false;

        // we get the sprite bank
        bank = AnimBank.Instance.GetComponent<SpriteBank>();

        // we set the damage variable
        damage += Random.Range(-random_damage_modifier_at_start, random_damage_modifier_at_start);

        ResetTags();
    }

    // USE
    public override void Use(Capable capable)
    {
        // we set the bearer and its components
        bearer = capable;
        anim_player = bearer.AnimPlayer;
        sr = bearer.AnimPlayer.Renderer;
        if (anim_player.current_capacity == "attack") { return; } // we check if we are already attacking

        // we update damage value if capable is Perso
        if (bearer is Perso perso) { damage = perso.skillManager.GetSkillValue("stat:damage"); }

        // we calculate the duration of the attack
        float duration_override = attack_duration;
        if (duration_override != default && attack_duration_random_variation > 0)
        {
            duration_override += Random.Range(-attack_duration_random_variation, attack_duration_random_variation);
            duration_override = Mathf.Max(0.01f, duration_override); // we make sure the duration is not negative
        }

        // we play the animation
        Anim anim = anim_player.Play("attack", duration_override: duration_override);
        if (anim == null) { return; } // if the animation is not found, we return

        // we start the cooldown for the time of the animation
        float anim_duration = anim.GetDuration();
        startCooldown(anim_duration);
        IsAttacking = true;
        hit_enemies.Clear();

        // we check if we need to turn on unstoppable effect
        if (unstoppable && Random.Range(0f, 1f) < unstoppable_rate)
        {
            being.AddEffect(Effect.Unstoppable, -888f); // infinite unstoppable
        }
    }

    // UPDATE
    protected override void Update()
    {
        base.Update();

        if (!IsAttacking) { return; }
        if (!anim_player.current_capacity.Equals("attack"))
        {
            // checks if we are still attacking & the animation is not the attack animation anymore
            if (IsAttacking)
            {
                IsAttacking = false;
                hit_enemies.Clear();
                being.RemoveEffect(Effect.Unstoppable);
            }
            return;
        }

        // we check if the sprite has a collider
        Sprite sprite = sr.sprite;
        if (!bank.HasDamageCollider(sprite)) { pc.enabled = false; return; }
        pc.enabled = true;

        // we update the collider
        updateCollider(sprite);

        // if target we update the attack
        if (hit_enemies.Count > 0) { updateAttack(); }
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
    private void updateAttack()
    {
        if (log)
        {
            string hit_enemies_str = transform.parent.name + " attack enemies : " + hit_enemies.Count + " :\n";
            foreach (Being enemy in hit_enemies)
            {
                hit_enemies_str += "\t" + enemy.name + "\n";
            }
            Debug.Log(hit_enemies_str);
        }

        // calculate damage dealt to single target
        float single_target_damage
                            = perforant_attack || single_hit // also if single hit we don't care we will apply damage once
                            ? damage // if perforant attack, all enemies will receive the full damage
                            : damage / hit_enemies.Count;


        // calculate knockback
        float advantage_attacker_weight = (being != null ? being.weight : 0.5f) * attackant_advantage; // l'attaquant a un avantage de poids afin de recevoir moins de knockback
        float total_knockback_weight = hit_enemies.Sum(enemy => enemy.weight) + advantage_attacker_weight;
        Vector2 attacker_knockback_direction = Vector2.zero;

        bool killed_an_enemy = false;


        // if we single attack we want to make sure to attack closest enemy
        if (single_hit)
        {
            // we sort the hit enemies by distance to the attacker
            hit_enemies = hit_enemies.OrderBy(enemy => Vector2.Distance(transform.position, enemy.transform.position)).ToList();
        }


        // deal damage to target
        for (int i = 0; i < hit_enemies.Count; ++i)
        {
            Being enemy = hit_enemies[i];
            if (enemy == null) { continue; }
            applyDamageToEnemy(enemy, single_target_damage, total_knockback_weight, ref attacker_knockback_direction);

            // if we have a single_hit attack we break the loop
            if (single_hit) { break; }
        }

        // we stop the attack
        hit_enemies.Clear();
        if (single_hit) { IsAttacking = false; }
        if (being == null) { return; }


        // on shake la caméra
        if (bearer is Perso)
        {
            CameraShaker.Instance.Shake(killed_an_enemy ? base_kill_shake_magnitude : base_attack_shake_magnitude);
        }

        // apply knockback to attacker
        float knockback_magnitude_inverse = knockback_base * (total_knockback_weight - being.weight)
                                                / (total_knockback_weight * attackant_advantage);
        Force knockback_inverse = new Force("knockback", attacker_knockback_direction.normalized, knockback_magnitude_inverse);
        being.AddForce(knockback_inverse);
    }
    private bool applyDamageToEnemy(Being enemy, float damage, float total_knockback_weight, ref Vector2 attacker_knockback_direction)
    {
        // get direction and weight of enemy
        float dx = enemy.transform.position.x - transform.position.x;
        float dy = enemy.transform.position.y - transform.position.y;
        Vector2 direction_enemy = new Vector2(dx, dy);
        float enemy_weight = enemy.weight;

        // calculate knockback magnitude proportionnal to weight
        float knockback_magnitude = knockback_base * (total_knockback_weight - enemy_weight)
                                     / total_knockback_weight;
        Force knockback = new Force("knockback", direction_enemy.normalized, knockback_magnitude);
        attacker_knockback_direction += -direction_enemy.normalized * knockback_magnitude;

        // apply damage and knockback
        enemy.take_damage(damage, knockback);

        // check if enemy is dead
        if (!enemy.Alive) { kills += 1; return true; }
        return false;
    }

    // COLLISION ENTER
    private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if the other is on the Beings layer
        if (!other.gameObject.layer.Equals(LayerMask.NameToLayer("Beings"))) { return; }

        // we check if we are attacking
        if (!IsAttacking) { return; }
        if (anim_player.current_capacity != "attack") { return; }

        // we check if the pc is enabled
        if (!pc.enabled) { return; }

        if (being != null && being.BodyColliders.Contains(other)) { return; } // we don't attack ourselves

        // we remove not attackable tags
        Being enemy_being = other.GetComponentInParent<Being>();
        if (enemy_being == null || excluded_tags.Contains(enemy_being.gameObject.tag)) { return; }

        // we remove not alive beings
        if (!enemy_being.Alive) { return; }

        // we can add it !
        hit_enemies.Add(enemy_being);
    }

    // WHITE LISTING
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
}