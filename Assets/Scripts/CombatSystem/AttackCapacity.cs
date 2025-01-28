using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackCapacity : Capacity
{
    // handle the collisions between an attack animation and a being
    // test de mécanique pour voir si une gestion pixelperfect du combat est agréable
    
    [Header("Damage parameters")]
    public float damage = 10f;
    public float knockback_base = 10f; // une attaque répartit le knockb
    public float attackant_advantage = 3f;
    [SerializeField] private bool is_attacking = false;

    [SerializeField] private List<string> not_attackable_tags = new List<string> {};


    [Header("Components")]
    private Being being;
    private SpriteBank bank;
    private SpriteRenderer sr;
    private AnimPlayer anim_player;
    private PolygonCollider2D pc;

    // START
    private void Start()
    {
        // we get the sprite renderer
        sr = transform.parent.GetComponent<SpriteRenderer>();
        anim_player = transform.parent.GetComponent<AnimPlayer>();
        being = transform.parent.GetComponent<Being>();

        // we get the polygon collider
        pc = GetComponent<PolygonCollider2D>();
        pc.enabled = false;

        // we get the sprite bank
        bank = GameObject.Find("/utils/bank").GetComponent<SpriteBank>();
    }

    // UPDATE
    protected override void Update()
    {
        base.Update();

        if (!is_attacking) { return; }
        if (!anim_player.current_capacity.Equals("attack")) { return; }

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
        }
        else
        {
            pc.enabled = false;
        }
    }
    private void FixedUpdate()
    {
        if (!is_attacking) { return; }
        if (!anim_player.current_capacity.Equals("attack")) { return; }

        // we check if the pc is enabled
        if (pc.enabled)
        {
            // we update the attack
            updateAttack();
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
        // we get the colliders
        Collider2D[] hit_enemies = new Collider2D[20];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Beings"));
        int count = pc.OverlapCollider(filter, hit_enemies);

        // verify that our life_collider is not in the list
        hit_enemies = hit_enemies.Where(enemy => enemy != being.life_collider && enemy != null).ToArray();

        // we remove the not attackable tags
        hit_enemies = hit_enemies.Where(enemy => !not_attackable_tags.Contains(enemy.tag)).ToArray();

        // we remove the not alive beings
        hit_enemies = hit_enemies.Where(enemy => enemy.transform.parent.GetComponent<Being>().isAlive()).ToArray();

        string hit_enemies_str = transform.parent.name + " attack enemies : " + hit_enemies.Length + " :\n";
        foreach (Collider2D enemy in hit_enemies)
        {
            hit_enemies_str += "\t"+enemy + "\n";
        }
        Debug.Log(hit_enemies_str);
        
        // if no target, return
        if (hit_enemies.Length == 0) { return; }

        // calculate damage dealt to single target
        float damage_dealt_to_single_target = damage / hit_enemies.Length;


        // calculate knockback
        float advantage_attacker_weight = being.weight * attackant_advantage; // l'attaquant a un avantage de poids afin de recevoir moins de knockback
        float total_knockback_weight = hit_enemies.Select(enemy => enemy.transform.parent.GetComponent<Being>().weight).Sum() + advantage_attacker_weight;
        Vector2 attacker_knockback_direction = Vector2.zero;

        // bool killed_an_enemy = false;

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
            Force knockback = new Force(direction_enemy.normalized, knockback_magnitude);
            attacker_knockback_direction += -direction_enemy.normalized * knockback_magnitude;

            // apply damage and knockback
            if (!enemy_being.take_damage(damage_dealt_to_single_target, knockback))
            {
                Debug.Log("Error : enemy " + enemy.name + " didn't take damage");
                return;
            }

            // check if enemy is dead
            /* if (!enemy_being.isAlive())
            {
                killed_an_enemy = true;
            } 
            
            if (transform.parent.parent.parent.name == "perso")
            {
                // on shake la caméra
                GameObject cam = GameObject.Find("/main_camera");
            }
            */
        }

        // on recoit un knockback inverse
        float knockback_magnitude_inverse = knockback_base * (total_knockback_weight - being.weight)
                                             / (total_knockback_weight * attackant_advantage);
        Force knockback_inverse = new Force(attacker_knockback_direction.normalized, knockback_magnitude_inverse);
        being.addForce(knockback_inverse);

        // we set the attacking to false
        is_attacking = false;
    }

    // trigger the attack
    public override void Use(Capable capable)
    {
        // we play the animation
        Anim anim = play_anim(capable.anim_player, name);

        if (anim != null)
        {
            // we start the cooldown for the time of the animation
            float anim_duration = anim.GetDuration();
            startCooldown(anim_duration);
        }
        else
        {
            startCooldown();
        }

        is_attacking = true;
        // Debug.Log(transform.parent.name + " just used attack");
    }


}