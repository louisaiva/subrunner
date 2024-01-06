using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class Chest : MonoBehaviour
{

    // ANIMATION
    protected AnimationHandler anim_handler;
    protected ChestAnims anims = new ChestAnims();

    [Header("OPENING")]
    [SerializeField] protected bool is_open = false;
    [SerializeField] protected bool is_moving = false;
    [SerializeField] protected float openin_duration = 0.5f;

 
    // inventory
    public Inventory inventory;

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

    protected virtual void Events() {}

    void Update()
    {
        // on check les events
        Events();
        
        // on met à jour l'inventaire
        if (!is_open)
        {
            inventory.setShow(false);
        }
    }


    // MAIN FUNCTIONS
    protected void open()
    {
        CancelInvoke();

        // on joue l'animation
        anim_handler.ChangeAnim(anims.openin, openin_duration);

        // on ouvre le coffre
        Invoke("success_open", openin_duration);

        is_moving = true;
    }

    protected void success_open()
    {
        // on ouvre le coffre
        is_open = true;
        is_moving = false;

        // on joue l'animation
        anim_handler.ChangeAnim(anims.idle_open);

        // on met à jour l'inventaire
        inventory.setShow(true);
    }

    protected void close()
    {
        CancelInvoke();

        // on met à jour l'inventaire
        inventory.setShow(false);

        // on ferme le coffre
        is_moving = true;

        // on joue l'animation
        anim_handler.ChangeAnim(anims.closin, openin_duration);
        Invoke("success_close", openin_duration);
        
    }

    protected void success_close()
    {
        // on joue l'animation
        anim_handler.ChangeAnim(anims.idle_closed);

        // on ferme le coffre
        is_open = false;
        is_moving = false;
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

}


public class ChestAnims
{

    // ANIMATIONS
    public string idle_open = "chest_idle_open";
    public string idle_closed = "chest_idle_closed";
    public string openin = "chest_openin";
    public string closin = "chest_closin";

}