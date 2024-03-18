using UnityEngine;

public class Drink : Item
{

    // unity functions
    
    public void drink()
    {
        // on donne au perso la capacité de l'item
        perso.GetComponent<Perso>().heal();

        // on détruit proprement l'item
        destruct();
    }
}

