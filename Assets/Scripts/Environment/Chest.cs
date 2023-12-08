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

    // inventory
    public Inventory inventory;

    // interactions
    public bool is_interacting { get; set; } // est en train d'interagir
    
    // unity functions
    void Awake()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();

        // on récupère l'inventaire
        inventory = transform.Find("inventory").GetComponent<Inventory>();

        // randomize
        inventory.randomize();
    }

    void Update()
    {
        // on met à jour les animations
        if (is_open)
        {
            // on regarde si on a fini l'animation
            if (anim_handler.IsForcing()) { return; }

            // on met à jour les animations
            anim_handler.ChangeAnim(anims.idle_open);

            // on met à jour l'inventaire
            inventory.setShow(true);
        }
        else
        {
            // on met à jour l'inventaire
            inventory.setShow(false);

            // on regarde si on a fini l'animation
            if (anim_handler.IsForcing()) { return; }

            // on met à jour les animations
            anim_handler.ChangeAnim(anims.idle_closed);
        }
    }

    // MAIN FUNCTIONS
    public void open()
    {
        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.openin)) { return; }

        // on ouvre le coffre
        is_open = true;
    }

    public void close()
    {
        // on arrête de forcer l'animation -> pour être sûr qu'on puisse fermer le coffre
        anim_handler.StopForcing();

        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.closin)) { return; }

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
        // on ouvre le coffre
        open();
    }

    public void stopInteract()
    {
        // on ferme le coffre
        close();
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