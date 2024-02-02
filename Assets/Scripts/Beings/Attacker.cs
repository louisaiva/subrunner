using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Attacker : Being
{


    [Header("ATTACKER")]
    // DAMAGE DONE
    public float damage = 10f;
    public Transform attack_point;
    public float attack_range = 0.25f; // distance entre le point d'attaque et le being qui attaque
    public float damage_range = 0.25f; // rayon du cercle de centre attack_point qui va toucher les ennemis
    public LayerMask enemy_layers;

    public float cooldown_attack = 0.6f; // temps entre chaque attaque (en secondes)
    public float knockback_base = 10f; // une attaque répartit le knockb
    public float attackant_advantage = 3f;

    // ANIMATIONS
    // protected bool isAttacking = false;

    // unity functions
    protected new void Start()
    {
        base.Start();
        anims = new AttackerAnims();

        // on ajoute les capacités
        addCapacity("hit", cooldown_attack);

        // on récupère le point d'attaque
        attack_point = transform.Find("attack_point");

        // on défini les layers des ennemis
        enemy_layers = LayerMask.GetMask("Enemies");
    }

    protected override void Update()
    {
        // ! à mettre tjrs au début de la fonction update
        if (!isAlive()) { return; }


        base.Update();

        // met à jour l'attack point en fonction du regard du being
        if (attack_point != null)
        {
            Vector2 attack_point_position = attack_range * lookin_at;
            attack_point.localPosition = new Vector3(attack_point_position.x, attack_point_position.y, 0);
        }

    }
    
    // DEAL DAMAGE
    protected virtual void attack()
    {
        // start cooldown
        startCapacityCooldown("hit");

        // play attack animation
        anim_handler.ChangeAnimTilEnd(((AttackerAnims) anims).attack);

        // check if there is a target
        Collider2D[] hit_enemies = Physics2D.OverlapCircleAll(attack_point.position, damage_range, enemy_layers);

        // if no target, return
        if (hit_enemies.Length == 0) { return; }

        // calculate damage dealt to single target
        float damage_dealt_to_single_target = damage / hit_enemies.Length;


        // calculate knockback
        float advantage_attacker_weight = weight* attackant_advantage; // l'attaquant a un avantage de poids afin de recevoir moins de knockback
        float total_knockback_weight = hit_enemies.Select(enemy => enemy.GetComponent<Being>().weight).Sum() + advantage_attacker_weight;
        Vector2 attacker_knockback_direction = Vector2.zero;

        // deal damage to target
        foreach (Collider2D enemy in hit_enemies)
        {
            // get direction and weight of enemy
            float dx = enemy.transform.position.x - transform.position.x;
            float dy = enemy.transform.position.y - transform.position.y;
            Vector2 direction_enemy = new Vector2(dx, dy);
            float enemy_weight = enemy.GetComponent<Being>().weight;

            // calculate knockback magnitude proportionnal to weight
            float knockback_magnitude = knockback_base * ( total_knockback_weight - enemy_weight )
                                         / total_knockback_weight;
            Force knockback = new Force(direction_enemy.normalized, knockback_magnitude);
            attacker_knockback_direction += -direction_enemy.normalized * knockback_magnitude;

            // apply damage and knockback
            enemy.GetComponent<Being>().take_damage(damage_dealt_to_single_target, knockback);
        }

        // on recoit un knockback inverse
        float knockback_magnitude_inverse = knockback_base * ( total_knockback_weight - weight )
                                             / (total_knockback_weight * attackant_advantage);
        Force knockback_inverse = new Force(attacker_knockback_direction.normalized, knockback_magnitude_inverse);
        forces.Add(knockback_inverse);
    }

    // draw gizmos
    protected new void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (attack_point == null)
            return;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(attack_point.position, damage_range);
    }


}


public class AttackerAnims : BeingAnims
{

    public bool has_up_down_attack = false;

    // ANIMATIONS
    public string attack = "attack_RL";

    public override void init(string name)
    {
        base.init(name);
        attack = name + "_" + attack;
    }
}