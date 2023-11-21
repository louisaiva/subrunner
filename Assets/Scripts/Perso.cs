using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perso : Being
{

    // EXPLOITS
    private float exploits = 3f; // exploits = mana (lance des sorts de hacks)
    private int max_exploits = 3;
    private float regen_exploits = 0.1f; // exploits par seconde



    /*


    */

    // unity functions
    new void Start(){

        base.Start();
    }

    public override void Events()
    {
        print("waaaa4");

        // Z,S,Q,D
        inputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        print("inputs : " + inputs);
    }

    // update de d'habitude
    new void Update()
    {
        print("waaaa");

        // régèn des exploits
        if (exploits < max_exploits)
        {
            exploits += regen_exploits * Time.deltaTime;
        }

        // update de d'habitude
        base.Update();

    }

}
