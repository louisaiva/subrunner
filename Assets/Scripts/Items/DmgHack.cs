using UnityEngine;

public class DmgHack : Hack
{

    public float damage = 0f; // damage infligés par le hack par seconde


    // unity functions

    new void Start()
    {
        base.Start();
        
        // on change le type de l'item
        item_type = "virtual";

    }

}

