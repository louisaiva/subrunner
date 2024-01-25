using System.Collections;
using System.Collections.Generic;
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
    // private float last_attack_time = 0f; // temps de la dernière attaque
    // public const float knockback_per_damage_per_weight = 1/100f; // knockback par point de damage (1/30 corresspond à 1 unité de knockback pour 30 points de damage)
    public float knockback_base = 6f; // knockback de base pour un poids de 1 (si le poids de la cible est de 2, elle recevra 2 fois moins de knockback 1/2)

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

    protected new void Update()
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
        // check if we can attack
        // if (Time.time - last_attack_time < cooldown_attack){ return; }
        // if (anim_handler.IsForcing()){ return; }

        // update last attack time
        // last_attack_time = Time.time;

        // print(gameObject.name + " attacks");

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

        // deal damage to target
        foreach (Collider2D enemy in hit_enemies)
        {
            // get direction and weight of enemy
            float dx = enemy.transform.position.x - transform.position.x;
            float dy = enemy.transform.position.y - transform.position.y;
            Vector2 direction_enemy = new Vector2(dx, dy);
            float enemy_weight = enemy.GetComponent<Being>().weight;

            // calculate knockback force
            // Vector2 knockback_force = () * direction_enemy.normalized;

            Force knockback = new Force(direction_enemy.normalized, knockback_base / enemy_weight);

            // apply damage and knockback
            enemy.GetComponent<Being>().take_damage(damage_dealt_to_single_target, knockback);

            // on recoit un knockback inverse
            Force knockback_inverse = new Force(-knockback.direction, knockback.magnitude/10f);
            forces.Add(knockback_inverse);
        }
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