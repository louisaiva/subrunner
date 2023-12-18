using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(AnimationHandler))]
public class Chest : MonoBehaviour, I_Interactable
{

    // la classe CHEST sert à créer des coffres
    // peut contenir des objets physiques
    // pas obligatoirement alignée avec les tiles

    // mettre un champ de force qui attire les objets vers le coffre,
    // comme ça on peut juste drop les objets à côté du coffre

    // ANIMATION
    private AnimationHandler anim_handler;
    private ChestAnims anims = new ChestAnims();

    // OPENING
    public bool is_open = false;
    private float openin_time = 1f;

    // inventory
    public Inventory inventory;

    // interactions
    public bool is_interacting { get; set; } // est en train d'interagir
    
    // unity functions
    void Awake()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();
        anim_handler.debug = true;

        // on récupère l'inventaire
        inventory = transform.Find("inventory").GetComponent<Inventory>();

        // randomize
        inventory.randomize();
    }

    void Update()
    {
        // on check les events
        Events();

        // on met à jour les animations
        if (!is_open)
        {
            // on met à jour l'inventaire
            inventory.setShow(false);

            if (!is_interacting)
            {
                // on regarde si on a fini l'animation
                if (anim_handler.IsForcing()) { return; }

                // on met à jour les animations
                anim_handler.ChangeAnim(anims.idle_closed);
            }
        }
    }

    void Events()
    {
        // print("chest : " + is_open + " " + is_interacting);
        // on regarde si on appuie sur la touche d'interaction (E)
        // lorsqu'on est déjà ouvert
        if (Input.GetButtonDown("Interact") && is_open)
        {
            // on transvase tous les objets dans l'inventaire du perso
            foreach (Item item in inventory.getItems())
            {
                // on ajoute l'item au perso
                // perso.GetComponent<Inventory>().addItem(item);
                inventory.dropItem(item);
            }
        }
    }

    // MAIN FUNCTIONS
    public void open()
    {
        print("open chest");
        is_open = true;

        // on met à jour les animations
        anim_handler.StopForcing();
        anim_handler.ChangeAnim(anims.idle_open);

        // on met à jour l'inventaire
        inventory.setShow(true);
    }

    public void close()
    {
        print("close chest");

        // on arrête de forcer l'animation -> pour être sûr qu'on puisse fermer le coffre
        anim_handler.StopForcing();

        // on met à jour les animations
        anim_handler.ChangeAnimTilEnd(anims.closin);

        // on ferme le coffre
        is_open = false;
    }


    // INVENTORY FUNCTIONS
    public bool grab(Item item)
    {
        // item.transform.SetParent(inventory.transform);
        return inventory.addItem(item);
    }

    public void forceGrab(Item item)
    {
        inventory.forceAddItem(item);
    }

    // INTERACTIONS
    public bool isInteractable()
    {
        return !is_interacting;
    }

    public void interact()
    {
        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.openin, openin_time)) { return; }

        print("interact chest");

        // interaction
        is_interacting = true;

        // on ouvre le coffre dans openin_time
        Invoke("open", openin_time);
    }

    public void stopInteract()
    {
        print("stop interact chest");

        if (is_open) { close(); }

        if (is_interacting)
        {
            // on arrête l'invocation de "open"
            CancelInvoke("open");

            // on arrête l'interaction
            is_interacting = false;
        }
    }
}

public class ChestAnims
{

    // ANIMATIONS
    public string idle_open = "chest_idle_open";
    public string idle_closed = "chest_idle_closed";
    public string openin = "chest_openin";
    public string closin = "chest_closin";
    // public string hackin = "chest_hacked";

}