using UnityEngine;

public class Hack : Item
{

    public string hack_type_target = "nothin";


    // unity functions

    protected new void Start()
    {
        base.Start();

        // on change le type de l'item
        item_type = "virtual";

    }

}

