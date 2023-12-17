using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(AnimationHandler))]
public class Bed : MonoBehaviour, I_Interactable
{

    // la classe BED permet de dormir
    // dormir sauvegarde votre position de spawn


    // ANIMATION
    private AnimationHandler anim_handler;
    private BedAnims anims = new BedAnims();

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

public class BedAnims
{

    // ANIMATIONS
    public string idle_open = "bed_idle_open";
    public string idle_closed = "bed_idle_closed";
    public string openin = "bed_openin";
    public string closin = "bed_closin";
    // public string hackin = "chest_hacked";

}