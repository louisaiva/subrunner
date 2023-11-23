using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker : Being
{

    // DAMAGE DONE
    public float damage = 10f;
    public Transform attack_point;
    public float attack_range = 0.25f; // distance entre le point d'attaque et le being qui attaque
    public float damage_range = 0.25f; // rayon du cercle de centre attack_point qui va toucher les ennemis
    public LayerMask enemy_layers;

    public float cooldown_attack = 0.6f; // temps entre chaque attaque (en secondes)
    private float last_attack_time = 0f; // temps de la dernière attaque


    // unity functions
    protected new void Start()
    {
        base.Start();

        // on récupère le point d'attaque
        attack_point = transform.Find("attack_point");

        // on défini les layers des ennemis
        enemy_layers = LayerMask.GetMask("Enemies");
    }

    protected new void Update()
    {
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
        if (Time.time - last_attack_time < cooldown_attack)
        {
            return;
        }
        // update last attack time
        last_attack_time = Time.time;

        print(gameObject.name + " attacks");

        // play attack animation
        if (HasParameter(animator, "attack"))
        {
            animator.SetTrigger("attack");
        }

        // check if there is a target
        Collider2D[] hit_enemies = Physics2D.OverlapCircleAll(attack_point.position, damage_range, enemy_layers);        

        // deal damage to target
        foreach (Collider2D enemy in hit_enemies)
        {
            enemy.GetComponent<Being>().take_damage(damage);
            // print("hit " + enemy.name);
        }
    }

    // draw gizmos
    void OnDrawGizmosSelected()
    {
        if (attack_point == null)
            return;

        Gizmos.DrawWireSphere(attack_point.position, damage_range);
    }

}