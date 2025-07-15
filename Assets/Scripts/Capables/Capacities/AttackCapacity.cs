using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// AttackCapacity is a capacity that allows a being to attack
/// It is a collider that is enabled during the attack animation
/// </summary>

public class AttackCapacity : Capacity
{
    [Header("Damage parameters")]
    public float distance_to_attack = 1f;
    public int kills = 0;
    public float damage = 10f;
    [SerializeField] private float random_damage_modifier_at_start = 0; // damage += random.range(-5,5) in the start method if this modifier = 5
    [SerializeField] private bool is_attacking = false;
    [SerializeField] private bool perforant_attack = false; // if true, the attack won't stop on the first enemy hit
    [SerializeField] private float delay_between_perforations = 0.01f; // delay between each perforation
    private float last_perforation_time = 0f; // time of the last perforation
    [SerializeField] List<Collider2D> hit_enemies = new List<Collider2D> {};
    [SerializeField] private List<string> not_attackable_tags = new List<string> {};
    

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
        bank = GameObject.Find("/utils/bank").GetComponent<SpriteBank>();

        // we set the damage variable
        damage += Random.Range(-random_damage_modifier_at_start, random_damage_modifier_at_start);
    }


    // 2 - USING THE CAPACITY

    // USE
    public override void Use(Capable capable)
    {
        // we set the bearer and its components
        bearer = capable;
        anim_player = bearer.GetComponent<AnimPlayer>();
        sr = bearer.GetComponent<SpriteRenderer>();

        // we play the animation
        Anim anim = anim_player.Play("attack");
        if (anim == null)
        {
            // we remove the animation from the pile
            anim_player.StopPlaying("attack", true);
            if (debug) { Debug.LogWarning($"(AttackCapacity) {bearer.name} tried to attack the animation can't be played right now."); }
            return;
        }

        // we start the cooldown for the time of the animation
        float anim_duration = anim.GetDuration();
        startCooldown(anim_duration);
        is_attacking = true;
        hit_enemies.Clear();
    }
    
    // UPDATE
    protected override void Update()
    {
        base.Update();

        if (!is_attacking) { return; }
        if (!anim_player.current_capacity.Equals("attack"))
        {
            // checks if we are still attacking & the animation is not the attack animation anymore
            if (is_attacking)
            {
                is_attacking = false;
                hit_enemies.Clear();
                last_perforation_time = 0f;
            }
            return;
        }

        // we check if the sprite has a collider
        Sprite sprite = sr.sprite;
        if (bank.HasDamageCollider(sprite))
        {
            pc.enabled = true;

            // we flip the collider if the sprite is flipped
            if (sr.flipX && transform.localScale.x > 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (!sr.flipX && transform.localScale.x < 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }

            // we update the collider
            updateCollider(sprite);

            // we update the attack
            if (!perforant_attack || Time.time - last_perforation_time > delay_between_perforations)
            {
                // we update the attack
                updateAttack();
            }
        }
        else
        {
            pc.enabled = false;
        }
    }
    private void updateCollider(Sprite sprite)
    {
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

        int i = 0;
        while (i < hit_enemies.Count)
        {
            // we remove the being's body collider from the list
            if (being != null && hit_enemies[i] == being.body_collider) { hit_enemies.RemoveAt(i); continue; }

            // we remove not attackable tags
            Being enemy_being = hit_enemies[i].transform.parent.GetComponent<Being>();
            if (enemy_being == null || not_attackable_tags.Contains(enemy_being.gameObject.tag)) { hit_enemies.RemoveAt(i); continue; }

            // we remove not alive beings
            if (!enemy_being.Alive) { hit_enemies.RemoveAt(i); continue; }

            i++;
        }

        if (debug)
        {
            string hit_enemies_str = transform.parent.name + " attack enemies : " + hit_enemies.Count + " :\n";
            foreach (Collider2D enemy in hit_enemies)
            {
                hit_enemies_str += "\t"+enemy + "\n";
            }
            Debug.Log(hit_enemies_str);
        }
        
        // if no target, return
        if (hit_enemies.Count == 0) { return; }

        // calculate damage dealt to single target
        float damage_dealt_to_single_target = damage /* / hit_enemies.Length */;


        // calculate knockback
        float advantage_attacker_weight = (being != null ? being.weight : 0.5f) * attackant_advantage; // l'attaquant a un avantage de poids afin de recevoir moins de knockback
        float total_knockback_weight = hit_enemies.Select(enemy => enemy.transform.parent.GetComponent<Being>().weight).Sum() + advantage_attacker_weight;
        Vector2 attacker_knockback_direction = Vector2.zero;

        bool killed_an_enemy = false;

        // deal damage to target
        foreach (Collider2D enemy in hit_enemies)
        {
            // get enemy being
            Being enemy_being = enemy.transform.parent.GetComponent<Being>();

            // get direction and weight of enemy
            float dx = enemy.transform.position.x - transform.position.x;
            float dy = enemy.transform.position.y - transform.position.y;
            Vector2 direction_enemy = new Vector2(dx, dy);
            float enemy_weight = enemy_being.weight;

            // calculate knockback magnitude proportionnal to weight
            float knockback_magnitude = knockback_base * (total_knockback_weight - enemy_weight)
                                         / total_knockback_weight;
            Force knockback = new Force("knockback",direction_enemy.normalized, knockback_magnitude);
            attacker_knockback_direction += -direction_enemy.normalized * knockback_magnitude;

            // apply damage and knockback
            enemy_being.take_damage(damage_dealt_to_single_target, knockback);

            // check if enemy is dead
            if (!enemy_being.Alive) { kills += 1; killed_an_enemy = true; }
        }

        

        if (being != null)
        {
            if (bearer is Perso)
            {
                // on shake la caméra
                // float shake_magnitude = damage * 2f;
                CameraShaker.Instance.Shake(killed_an_enemy ? base_kill_shake_magnitude : base_attack_shake_magnitude);
            }
            
            // on recoit un knockback inverse
            float knockback_magnitude_inverse = knockback_base * (total_knockback_weight - being.weight)
                                                 / (total_knockback_weight * attackant_advantage);
            Force knockback_inverse = new Force("knockback",attacker_knockback_direction.normalized, knockback_magnitude_inverse);
            being.AddForce(knockback_inverse);
        }

        // we clear the hit enemies
        hit_enemies.Clear();

        // check if we are perforant if yes we don't stop the attack (will automatically stop when the animation is over)
        if (perforant_attack)
        {
            last_perforation_time = Time.time;
            return;
        }

        // we set the attacking to false
        is_attacking = false;
    }

    // COLLISION ENTER
    private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if the other is on the Beings layer
        if (!other.gameObject.layer.Equals(LayerMask.NameToLayer("Beings"))) { return; }

        // we check if we are attacking
        if (!is_attacking) { return; }
        if (anim_player.current_capacity != "attack") { return; }

        // we check if the pc is enabled
        if (pc.enabled)
        {
            // we add the other to the hit enemies
            hit_enemies.Add(other);
        }
    }

    // WHITE LISTING
    public async void WhiteListTagShortly(string tag, float duration)
    {
        // we check if the tag is not already in the list
        if (not_attackable_tags.Contains(tag)) { return; }

        if (debug)
        {
            Debug.Log("(AttackCapacity) Adding tag " + tag + " to the not attackable tags for " + duration + " seconds");
        }

        // we add the tag to the list
        not_attackable_tags.Add(tag);

        // wait for a frame to let the click happen
        await System.Threading.Tasks.Task.Delay((int) (duration * 1000));

        if (debug)
        {
            Debug.Log("(AttackCapacity) Removing tag " + tag + " from the not attackable tags");
        }
        // we remove the tag from the list
        not_attackable_tags.Remove(tag);
    }
}