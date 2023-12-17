using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(AnimationHandler))]
public class Bed : MonoBehaviour, I_LongInteractable
{

    // la classe BED permet de dormir
    // dormir sauvegarde votre position de spawn

    // perso
    private GameObject perso;

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
        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();

        // on récupère l'inventaire
        inventory = transform.Find("inventory").GetComponent<Inventory>();
    }

    void Update()
    {

        // print("bed : " + is_open + " " + is_interacting + " " + is_being_activated);

        // on check les events
        Events();

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

    void Events()
    {
        if (is_being_activated && Input.GetButtonUp("Interact"))
        {
            // on arrête l'activation
            quitActivating();
        }
    }

    /* - 1 - INVENTORY
    */
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

    public bool grab(Item item)
    {
        // item.transform.SetParent(inventory.transform);
        return inventory.addItem(item);
    }

    public void forceGrab(Item item)
    {
        inventory.forceAddItem(item);
    }



    /* - 2 - INTERACTIONS
    */
    public bool isInteractable()
    {
        return !(is_interacting || is_being_activated);
    }

    public void interact()
    {
        // on ouvre le coffre
        startActivating();
    }

    public void stopInteract()
    {
        // on ferme le coffre
        if (is_open) { close(); }

        // on arrête l'interaction
        is_interacting = false;

        // on arrête l'activation
        if (is_being_activated) { quitActivating(); }
    }


    /* - 3 - LONG INTERACTIONS
    */

    // interactions
    [SerializeField] public bool is_being_activated { get; set; }
    [SerializeField] public float activation_time { get; set; } = 1f;

    // fonctions
    public void startActivating()
    {

        // on commence l'activation
        is_being_activated = true;

        // on lance l'invocation de l'activation
        Invoke("succeedActivating", activation_time);

        // on met à jour les animations
        anim_handler.ChangeAnimTilEnd(anims.openin, activation_time);

        print("start activating");
    }

    public void quitActivating()
    {
        // on arrête l'activation
        is_being_activated = false;

        // on arrête l'invocation
        CancelInvoke("succeedActivating");

        // on met à jour les animations
        // anim_handler.ForceSwapAnimTilEnd(anims.closin);
        anim_handler.StopForcing();
        anim_handler.ChangeAnim(anims.idle_closed);

        print("quit activating");
    }

    public void succeedActivating()
    {
        // on ouvre le coffre et on interagit
        is_open = true;
        is_interacting = true;

        // on désactive l'activation
        is_being_activated = false;

        print("succeed activating");

        // on met à jour le spawn point
        perso.GetComponent<Perso>().setRespawnBed(this);
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