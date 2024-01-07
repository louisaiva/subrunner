using UnityEngine;

public class Drink : Item
{

    // unity functions

    protected new void Awake()
    {
        base.Awake();

        // on change le type de l'item
        // item_type = "drink";
    }
    
    public void drink()
    {
        // on donne au perso la capacité de l'item
        perso.GetComponent<Perso>().heal();

        // on détruit proprement l'item
        destruct();
    }
}

