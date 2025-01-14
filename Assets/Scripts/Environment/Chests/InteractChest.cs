using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class InteractChest : InventoryChest, I_Interactable
{

    public bool is_interacting { get; set; } // est en train d'interagir

    public Transform interact_tuto_label { get; set; }

    // UNITY FUNCTIONS
    new void Start()
    {
        // on récupère le label
        interact_tuto_label = transform.Find("interact_tuto_label");

        base.Start();
    }


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
    }

    public void stopInteract()
    {
        OnPlayerInteractRangeExit();
        print("stop interact with "+gameObject.name);
        
        close();

        // on arrête l'interaction
        is_interacting = false;
    }

    public void OnPlayerInteractRangeEnter()
    {
        if (interact_tuto_label == null)
        {
            interact_tuto_label = transform.Find("interact_tuto_label");
            if (interact_tuto_label == null) { return; }
        }

        // on affiche le label
        interact_tuto_label.gameObject.SetActive(true);
    }

    public void OnPlayerInteractRangeExit()
    {
        if (interact_tuto_label == null) { return; }

        // on cache le label
        interact_tuto_label.gameObject.SetActive(false);
    }
}
