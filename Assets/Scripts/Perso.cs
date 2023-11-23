using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perso : Attacker
{

    // exploits (xp)
    public int level = 1;
    public int xp = 92;
    public int xp_to_next_level = 100;

    // bits (mana)
    public float bits = 3f; // bits = mana (lance des sorts de hacks)
    public int max_bits = 8;
    private float regen_bits = 0.1f; // bits par seconde

    /*


    */

    // unity functions
    new void Start(){

        base.Start();
        anims.init("perso");
    }

    public override void Events()
    {
        // Z,S,Q,D
        inputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    // update de d'habitude
    new void Update()
    {

        // régèn des bits
        if (bits < max_bits)
        {
            bits += regen_bits * Time.deltaTime;
        }

        // update de d'habitude
        base.Update();

    }


    // DAMAGE
    protected override void die()
    {
        Debug.Log("YOU DIED");
        
        base.die();
    }

}
