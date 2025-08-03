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
    public string ActivationLabel = ""; // if an item has an use, this is its label (example : food -> "eat", shuriken -> "throw" etc) but most of items don't have an action at all

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
    public Capable Holder
    {
        get
        {
            if (transform.parent == null) { return null; }
            if (transform.parent.GetComponent<Inventory>() == null) { return null; }
            return transform.parent.GetComponent<Inventory>().capable;
        }
    }

    // Inventory
    public Inventory Inventory
    {
        get
        {
            if (transform.parent == null) { return null; }
            return transform.parent.GetComponent<Inventory>();
        }
    }

    /// <summary>
    /// Return true if the item pass the string rule in parameter.
    /// The rule must be in format "category:item,category:item, ..."
    /// If one of the rule match the Reference, it passes, otherwise it return false.
    /// you don't have to write the precise item name if you want all the category to pass
    /// ex: the item "food:meat" passes the rule "food,weapon:katana"
    /// but the item "hardware:laptop" does not
    /// </summary>
    /// <param name="item_rule">the rule to test the item</param>
    /// <returns>true if the item pass the rule, false otherwise</returns>
    public bool ValidateRule(string item_rule)
    {
        // all items passes an empty rule
        if (item_rule == "") { return true; }

        // we check if our rule has multiple entries
        string[] rules = item_rule.Split(',');

        // we need at least one rule to be valid
        foreach (string rule in rules)
        {
            // checks special rule
            if (rule == "activable")
            {
                if (ActivationLabel != "") { return true; }
                continue;
            }

            // check if the rule is a category or a specific item
                if (rule.Contains(":"))
                {
                    // specific item -> we check if the item is the same
                    if (Reference == rule) { return true; }
                    continue;
                }

            // we check if the item is in the category
            if (Reference.Contains(rule)) { return true; }
        }

        return false;
    }


    // BEING GRABBED / DROPPED
    protected virtual void on_grabbed()
    {
        // we change the rigidbody to a kinematic
        // rb.bodyType = RigidbodyType2D.kinematic;

        // we remove the rigidbody
        Destroy(rb);
        rb = null;

        // we disable the HoverCapacity's collider
        GetCapacity<HoverCapacity>().GetComponent<Collider2D>().enabled = false;

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
        // rb.bodyType = RigidbodyType2D.Dynamic; // we change the rigidbody to a dynamic

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
    public virtual void Use(Capable user)
    {
        // todo make this an interface
        // only for items that have a use (apple : being eaten, katana : make an attack, etc.)
        // use the capacity of the item BUT with the capable holding this item as the user
        // if katana make a Do("attack") for example, the katana will be the user of the attack
        // we want the perso, holding the katana, to be the user of the attack


        // we check if the item is grabbed
        if (!Grabbed) { return; }

        // we find the holder of the item
        // Capable holder = transform.parent.GetComponent<Inventory>().capable;
        // if (holder == null) { return; }

        // and then we use the item
    }

    // ON DESTROY
    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) { return; } // this happens when the scene is destroyed when we quit the scene
        if (Holder != null) { Holder.inventory.Remove(this); } // we remove the item from the holder's inventory
    }
}