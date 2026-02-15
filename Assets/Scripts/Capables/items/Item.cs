using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Item is a Movable that can be grabbed by other Capables with GrabCapacity + InteractCapacity.
/// </summary>
public class Item : Movable, EndlessInteractable
{

    [Header("Item")]
    [SerializeField] private string _reference = "category:item";
    public string Reference
    {
        get => _reference;
        set
        {
            if (_reference == value) { return; }
            _reference = value;
            OnReferenceChanged?.Invoke(this);
        }
    }
    public Action<Item> OnReferenceChanged = delegate { };
    public Color Color = Color.yellow;
    public int MaxQty = 1;
    public bool Stackable { get => MaxQty > 1; }
    public string ItemDescription = "description of the item";

    // GRAB / DROP / PLACING
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
            if (value) { on_grabbed(); }
            else { on_dropped(); }
        }
    }
    [SerializeField] private bool _placed = false;
    public bool Placed
    {
        get => _placed;
        set
        {
            // check if the value is the same
            if (value == _placed) { return; }

            // we set the value
            _placed = value;
            update_grab_n_place();
        }
    }

    // events
    public event System.Action<Item, Capable> OnGrabbed = delegate { };
    public event System.Action<Item> OnDropped = delegate { };

    // HOLDER
    public Capable Holder => ItemPoolHolder != null ? ItemPoolHolder.Inventory.capable : null;
    public ItemPool ItemPoolHolder
    {
        get
        {
            if (transform.parent == null) { return null; }
            return transform.parent.GetComponent<ItemPool>();
        }
    }



    /// <summary>
    /// Return true if the item pass the string rule in parameter.
    /// The rule must be in format "category:item,category:item, ..."
    /// If one of the rule match the Reference, it passes, otherwise it return false (understand the "," as a OR)
    /// you don't have to write the precise item name if you want all the category to pass
    /// ex: the item "food:meat" passes the rule "food,weapon:katana"
    /// but the item "weapon:shuriken" does not
    /// 
    /// you can also make the inverse of a rule with "!" ex : item "weapon:katana" does not pass the "!weapon" rule
    ///
    /// but careful only ONE rule needs to pass for the whole rule set to pass. so if you put "!laptop,!module",
    /// both "laptop:blue" & "module:cpu" items validate the rule bcz laptop:blue is no module & module:cpu is not laptop ;-;
    /// 
    /// you can specify ";" caracters instead of "," to split the rule into 2 rules that need both to passes in order for the full rule to pass (it is kind of a AND door)
    /// ex : "laptop:blue" & "module:cpu" does not pass the rule "!laptop;!module"
    /// 
    /// finally "|" is the higher OR door that wins over ";".
    /// ex : item "food:pasta" passes the rule "food|!food;!pot:clean"
    /// 
    /// </summary>
    /// <param name="item_rule">the rule to test the item</param>
    /// <returns>true if the item pass the rule, false otherwise</returns>
    public bool ValidateRule(string rule)
    {
        // all items passes an empty rule
        if (rule == "") { return true; }

        // [OR] - we need at least one rule to be valid
        if (rule.Contains("|"))
        {
            string[] rules = rule.Split('|');
            for (int i = 0; i < rules.Length; i++)
            {
                if (ValidateRule(rules[i])) { return true; }
            }
            return false;
        }

        // [AND] - we need ALL rules to be valid 
        if (rule.Contains(";"))
        {
            string[] rules = rule.Split(';');
            for (int i = 0; i < rules.Length; i++)
            {
                if (!ValidateRule(rules[i])) { return false; }
            }
            return true;
        }

        // [OR] - we need at least one rule to be valid
        if (rule.Contains(","))
        {
            string[] rules = rule.Split(',');
            for (int i = 0; i < rules.Length; i++)
            {
                if (ValidateRule(rules[i])) { return true; }
            }
            return false;
        }

        // we have only one rule, we check the rule

        // [NOT] - check for negation
        if (rule.StartsWith("!")) { return !ValidateRule(rule.Substring(1)); }

        // check special rule
        if (rule == "usable") { return this is Usable; }

        // specific item -> we check if the item is the same
        if (rule.Contains(":")) { return Reference == rule; }

        // we check if the item is in the category
        if (Reference.Contains(rule)) { return true; }

        return false;
    }

    // START
    protected override void Start()
    {
        base.Start();

        update_grab_n_place();
    }


    // INTERACTABLE
    public InteractCapacity Interactor => null;
    public InteractType InteractionType => InteractType.Item;
    public void OnInteract(Capable interactor)
    {
        if (interactor.Inventory == null) { return; }
        interactor.Inventory.Grab(this);
    }
    public void OnEndlessInteract(Capable interactor) { OnInteract(interactor); }


    // BEING GRABBED / DROPPED
    protected virtual async void on_grabbed()
    {
        // on veut etre sur qu'on est pas placé
        _placed = false;
        update_grab_n_place();

        await System.Threading.Tasks.Task.Yield();
        if (this == null) { return; } // in case the item was destroyed during the await

        // we call the event
        OnGrabbed?.Invoke(this, Holder);
    }
    protected virtual void on_dropped()
    {
        update_grab_n_place();

        // we call the event
        OnDropped?.Invoke(this);
    }

    // MAIN LOW LEVEL UPGRADE GRABBING & PLACING
    protected virtual void update_grab_n_place()
    {
        // RENDERERs
        transform.localScale = Vector3.one;
        if (!Placed && Grabbed)
        {
            anim_player.DisableRenderer();
            if (Inventory != null)
            {
                List<Item> items = Inventory.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].anim_player.DisableRenderer();
                }
            }
        }
        else
        {
            anim_player.EnableRenderer();
            if (Inventory != null)
            {
                List<Item> items = Inventory.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (!items[i].Placed) { continue; }
                    items[i].anim_player.EnableRenderer();
                }
            }
        }

        // FEET & RB & BEING CARRIED EFFECT
        if (!Placed && !Grabbed)
        {
            // enable rigidbody & feet
            if (feet_collider != null) { feet_collider.enabled = true; }

            // we add the rigidbody
            if (GetComponent<Rigidbody2D>() == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                rb.freezeRotation = true;
            }

            // we remove the being carried effect
            RemoveEffect(Effect.BeingCarried);
        }
        else
        {
            // disable rigidbody & feet
            if (feet_collider != null) { feet_collider.enabled = false; }

            // we remove the rigidbody
            if (rb != null)
            {
                Destroy(rb);
                rb = null;
            }

            // we set the effect IsBeingCarried to -888f (infinite time)
            AddEffect(Effect.BeingCarried, -888f);

            // we remove all the forces
            ClearForces();
        }

        // HOVER
        if (Hover == null) { return; }
        if (!Grabbed)
        {
            Hover.transform.localPosition = Vector3.zero;
            // we enable the HoverCapacity's collider
            Hover.GetComponent<Collider2D>().enabled = true;
        }
        else 
        {
            // we disable the HoverCapacity's collider
            Hover.GetComponent<Collider2D>().enabled = false;
        }
    }


    // ON DESTROY
    protected override void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) { return; } // this happens when the scene is destroyed when we quit the scene
        if (Holder != null) { Holder.Inventory.Remove(this); } // we remove the item from the holder's inventory

        base.OnDestroy();
    }
}