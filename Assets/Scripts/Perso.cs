using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perso : Attacker
{

    // exploits (xp)
    public int level = 1;
    public int xp = 0;
    public int total_xp = 0;
    public int xp_to_next_level = 10;

    // bits (mana)
    public float bits = 3f; // bits = mana (lance des sorts de hacks)
    public int max_bits = 8;
    private float regen_bits = 0.1f; // pourcentage de max_bits par seconde 

    // hack
    public float hack_range = 1f; // distance entre le perso et la porte pour hacker
    private LayerMask hack_layer;

    /*


    */

    // unity functions
    new void Start(){

        // on start de d'habitude
        base.Start();

        // on met les differents paramètres du perso
        max_vie = 100;
        vie = (float) max_vie;
        vitesse = 3f;
        damage = 24f;
        attack_range = 0.3f;
        damage_range = 0.5f;
        cooldown_attack = 0.5f;

        // on met à jour les animations
        anims.init("perso");

        // on met à jour les layers du hack
        hack_layer = LayerMask.GetMask("Doors");
    }

    public override void Events()
    {
        // Z,S,Q,D
        inputs = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // attaque
        if (Input.GetKeyDown(KeyCode.Space))
        {
            attack();
        }

        // hack de porte
        if (Input.GetKeyDown(KeyCode.E))
        {
            hack();
        }
    }

    // update de d'habitude
    new void Update()
    {
        // ! à mettre tjrs au début de la fonction update
        if (!isAlive()) { return; }

        // régèn des bits
        if (bits < max_bits)
        {
            bits += regen_bits * max_bits * Time.deltaTime;
        }

        // update de d'habitude
        base.Update();

    }


    // XP
    public void addXP(int count)
    {
        xp += count;
        total_xp += count;
        if (xp >= xp_to_next_level)
        {
            levelUp();
        }
    }

    private void levelUp()
    {
        level += 1;
        xp = 0;
        xp_to_next_level = (int)(xp_to_next_level * 1.5f);
        // max_bits += 1;
        // bits = max_bits;
        Debug.Log("LEVEL UP ! level " + level);

        // on augmente les stats
        max_vie += 10*level;
        damage += 2*level;
        cooldown_attack -= 0.05f;
        vitesse += 0.1f;
    }

    // DAMAGE
    protected override void die()
    {
        Debug.Log("YOU DIED");
        base.die();
    }


    // HACK
    private void hack()
    {
        // on regarde si on a assez de bits
        if (bits < 1) { return; }

        // on regarde si on peut hacker une porte
        Collider2D[] hit_hackable = Physics2D.OverlapCircleAll(transform.position, hack_range, hack_layer);
        if (hit_hackable.Length == 0) { return; }

        // on hack la porte
        hit_hackable[0].GetComponent<Door>().hack(level);

        // on enlève des bits
        bits -= 1;
    }
}
