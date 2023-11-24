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
    }

    public override void Events()
    {
        // Z,S,Q,D
        inputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // attaque
        if (Input.GetKeyDown(KeyCode.Space))
        {
            attack();
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

}
