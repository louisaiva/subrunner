using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class InteractChest : InventoryChest, I_Interactable
{

    public bool is_interacting { get; set; } // est en train d'interagir

    // INTERACTIONS
    public bool isInteractable()
    {
        return !is_interacting/*  || is_open */;
    }

    public void interact(GameObject target)
    {
        if (!is_interacting)
        {
            print("interact with " + gameObject.name);

            // interaction
            is_interacting = true;

            open();
        }
        /* else if (is_open)
        {
            // on récupère tous les objets
            foreach (Item item in inventory.getItems())
            {
                // on ajoute l'item au perso
                inventory.dropItem(item);
            }
        } */
    }

    public void stopInteract()
    {
        print("stop interact with "+gameObject.name);
        
        close();

        // on arrête l'interaction
        is_interacting = false;
    }

}
