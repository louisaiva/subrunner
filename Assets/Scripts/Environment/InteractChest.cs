using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class InteractChest : InventoryChest, I_Interactable
{

    public bool is_interacting { get; set; } // est en train d'interagir

    // Update
    void Update()
    {
        // on regarde si on appuie sur la touche d'interaction (E)
        // lorsqu'on est déjà ouvert
        if (is_open && !is_moving && Input.GetButtonDown("interact"))
        {
            // on transvase tous les objets dans l'inventaire du perso
            foreach (Item item in inventory.getItems())
            {
                // on ajoute l'item au perso
                inventory.dropItem(item);
            }
        }
    }

    // INTERACTIONS
    public bool isInteractable()
    {
        return !is_interacting;
    }

    public void interact()
    {
        if (is_interacting) { return;}

        print("interact with "+gameObject.name);

        // interaction
        is_interacting = true;
        
        open();
    }

    public void stopInteract()
    {
        print("stop interact with "+gameObject.name);
        
        close();

        // on arrête l'interaction
        is_interacting = false;
    }

}
