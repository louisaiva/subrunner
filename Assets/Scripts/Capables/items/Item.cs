using UnityEngine;
/// <summary>
/// Item is a Movable that can be grabbed by other Capables with GrabCapacity + InteractCapacity.
/// </summary>
public class Item : Movable
{

    [Header("Item")]
    public string Reference = "category:item";
    public int MaxQty = 1;
    public bool Stackable { get => MaxQty > 1; }
    public string ItemDescription = "description of the item";

    // Grabbable
    private bool _grabbed = false;
    public bool Grabbed
    {
        get => _grabbed;
        set
        {
            // check if the value is the same
            if (value == _grabbed) { return; }

            // we set the value
            _grabbed = value;
            if (_grabbed) { on_grabbed(); }
            else { on_dropped(); }
        }
    }

    // BEING GRABBED / DROPPED
    protected virtual void on_grabbed()
    {
        // we remove the rigidbody
        Destroy(rb);

        // we disable the HoverCapacity's collider
        GetCapacity<HoverCapacity>().transform.GetComponent<Collider2D>().enabled = false;

        // we disable the feet collider
        feet_collider.enabled = false;

        // we disable the sprite renderer
        GetComponent<SpriteRenderer>().enabled = false;

        // we set the effect IsBeingCarried to -888f (infinite time)
        AddEffect(Effect.BeingCarried, -888f);

        // we remove all the forces
        ClearForces();

    }
    protected virtual void on_dropped()
    {
        // we add the rigidbody
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        // we enable the HoverCapacity's collider
        GetCapacity<HoverCapacity>().transform.GetComponent<Collider2D>().enabled = true;

        // we enable the feet collider
        feet_collider.enabled = true;

        // we enable the sprite renderer
        GetComponent<SpriteRenderer>().enabled = true;

        // we remove the effect IsBeingCarried
        RemoveEffect(Effect.BeingCarried);
    }


    // USE
    public virtual void Use()
    {
        // only for items that have a use (apple : being eaten, katana : make an attack, etc.)
        // use the capacity of the item BUT with the capable holding this item as the user
        // if katana make a Do("attack") for example, the katana will be the user of the attack
        // we want the perso, holding the katana, to be the user of the attack

        // we check if the item is grabbed
        if (!Grabbed) { return; }

        // we find the holder of the item
        Capable holder = transform.parent.GetComponent<Inventory>().capable;
        if (holder == null) { return; }

        // and then we use the item
    }

}