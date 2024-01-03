using UnityEngine;

public class Hack : ActiveItem
{

    public string hack_type_target = "nothin";


    // unity functions

    protected new void Awake()
    {
        base.Awake();

        // on change le type de l'item
        item_type = "virtual";

    }

    // activation

    public void use(I_Hackable target)
    {
        base.use();

        // on lance le hack
    }
}

