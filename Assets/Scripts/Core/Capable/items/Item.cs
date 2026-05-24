using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Item is a Movable that can be grabbed by other Capables with GrabCapacity + InteractCapacity.
/// </summary>
public class Item : Movable, EndlessInteractable
{

    [Header("Item")]
    public ItemData idata => (ItemData)data;
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
    public string PrefixReference
    {
        get
        {
            if (!Reference.Contains(":")) { return Reference; }
            return Reference.Split(':')[0];
        }
    }
    public Action<Item> OnReferenceChanged = delegate { };
    public Color Color = Color.yellow;
    public int MaxQty = 1;
    public bool Stackable { get => MaxQty > 1; }
    public string ItemDescription = "description of the item";

    // GRAB / DROP / PLACING
    [SerializeField] protected bool _grabbed = false;
    public bool Grabbed { get => _grabbed; }
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
    public event Action<Item, Capable> OnGrabbed = delegate { };
    public event Action<Item> OnDropped = delegate { };

    // HOLDER
    // public Capable _holder = null;
    public Capable Holder { get { return ItemPoolHolder?.Capable; } }
    private ItemPool _item_pool_holder = null;
    public ItemPool ItemPoolHolder
    {
        get
        {
            if (_item_pool_holder != null) { return _item_pool_holder; }
            if (transform.parent == null) { return null; }
            _item_pool_holder = transform.parent.GetComponent<ItemPool>();
            return _item_pool_holder;
        }
    }
    public ItemStack ItemStackHolder
    {
        get
        {
            ItemPool pool = ItemPoolHolder;
            if (pool == null) { return null; }
            return pool.GetStackOfItem(this);
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
    /// ex : "laptop:blue" & "module:cpu" both do NOT pass the rule "!laptop;!module"
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
        if (rule == "device") { return this is Device; }
        if (rule == "virtual")
        {
            // if (this is File) { return true; }
            if (this.Reference.StartsWith("key:")) { return true; }
            if (this.Reference.StartsWith("file:")) { return true; }
            if (this.Reference.StartsWith("program:")) { return true; }
            if (this.Reference.StartsWith("exploit:")) { return true; }
            return false;
        }

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
    public virtual void BeGrabbed(Capable grabber)
    {
        // we free the item from room so the trigger event are not called after
        CapableEngine.Instance?.OnItemGrabbed(this);


        _grabbed = true;
        idata.is_grabbed = true; // we set this to true before calling on_grabbed so the events are triggered with the correct value
        on_grabbed();

        // we load the capacities we have not load yet & remove the room
        if (CapableEngine.Instance.log_loading_extended) { Debug.Log($"(Item - OnGrabbed) {data.id} unloading capacities : {string.Join(" ", dynamic_capacity_ids)}"); }
        CapacityEngine.Instance?.UnloadCapacities(dynamic_capacity_ids, this);

        // finally we reset the holder (so next time we check for it it will recalculate it)
        _item_pool_holder = null;
    }
    public virtual void BeDropped(Capable dropper)
    {
        _grabbed = false;
        idata.is_grabbed = false; // we set this to false before calling on_dropped so the events are triggered with the correct value
        on_dropped();

        // we load the capacities and get a room
        if (CapableEngine.Instance.log_loading_extended) { Debug.Log($"(Item - OnDropped) {data.id} loading capacities : {string.Join(" ", dynamic_capacity_ids)}"); }
        CapacityEngine.Instance?.LoadCapacities(dynamic_capacity_ids, this);
        CapableEngine.Instance?.OnItemDropped(this);

        // finally we reset the holder
        _item_pool_holder = null;
    }
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
        // check if we are not destroyed
        if (going_to_be_destroyed) { return; }

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
            AnimPlayer.Hide();
            if (Inventory != null)
            {
                List<Item> items = Inventory.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].AnimPlayer.Hide();
                }
            }
        }
        else
        {
            AnimPlayer.Show();
            if (Inventory != null)
            {
                List<Item> items = Inventory.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (!items[i].Placed) { continue; }
                    items[i].AnimPlayer.Show();
                }
            }
        }

        // FEET & RB & BEING CARRIED EFFECT
        if (!Placed && !Grabbed)
        {
            // enable rigidbody & feet
            if (FeetCollider != null) { FeetCollider.enabled = true; }

            // we add the rigidbody
            if (Rb == null)
            {
                gameObject.AddComponent<Rigidbody2D>();
                Rb.gravityScale = 0;
                Rb.freezeRotation = true;
            }

            // we remove the being carried effect
            RemoveEffect(Effect.BeingCarried);
        }
        else
        {
            // disable rigidbody & feet
            if (FeetCollider != null) { FeetCollider.enabled = false; }

            // we remove the rigidbody
            if (Rb != null) { Destroy(Rb); }

            // we set the effect IsBeingCarried to -888f (infinite time)
            AddEffect(Effect.BeingCarried, -888f);

            // we remove all the forces
            ClearForces();
        }
    }












    // DATA MANAGEMENT
    private List<string> dynamic_capacity_ids = new List<string>(); // this list is used to store the capacities that are loaded dynamically on grab, so we can unload them on drop
    public override void LoadData(CapableData data)
    {
        if (data is not ItemData idata)
        {
            if (log) { Debug.LogError($"(Item - LoadData) The data provided is not of type ItemData for item '{name}'"); }
            return;
        }

        // we store our dynamic capacities ids
        dynamic_capacity_ids = CapacityEngine.Instance.GetDynamicItemCapacitiesIDs(idata, out List<string> static_ids);

        // we do a trick to make base.LoadData(data) only load the capacities we want to !
        if (idata.is_grabbed) // if we are grabbed, we only load the static capacities, the dynamic ones will be loaded only when dropped
        {
            data.capacities_ids = static_ids;
        }
        base.LoadData(data);
        if (idata.is_grabbed)
        {
            data.capacities_ids.AddRange(dynamic_capacity_ids); // we restore the dynamic capacities ids list in case we need it later
        }

        // we load the item data
        this.Reference = idata.reference;
        this.Color = idata.color;
        this.MaxQty = idata.max_qty;
        this.ItemDescription = idata.item_description;
        this._grabbed = idata.is_grabbed;

        // and update the grab n place state (which reenable renderers, rigidbody, feet, etc... based on the grabbed / placed state)
        update_grab_n_place();
    }
    /* public override void UnloadData()
    {
        base.UnloadData();

        // ? really useful ? no but it's better to have a safe guard
        // todo if perf problem when loading items, remove this
        // this.Reference = "category:item";
        // this.Color = Color.yellow;
        // this.MaxQty = 1;
        // this.ItemDescription = "description of the item (item data was not loaded, is there a problem ?)";
    } */
    public override ICapableData GetStaticData()
    {
        ItemData static_data = new ItemData((CapableData)base.GetStaticData())
        {
            // set item data things
            reference = this.Reference,
            color = this.Color,
            max_qty = this.MaxQty,
            item_description = this.ItemDescription,
            is_grabbed = GetStaticGrabbed()
        };

        return static_data;
    }
    public bool GetStaticGrabbed()
    {
        // we need to check if we have another capable in our parents or above, if yes it means we are grabbed
        if (transform.parent == null) { return false; }
        Capable parent_capable = transform.parent.GetComponentInParent<Capable>(includeInactive: true);
        return parent_capable != null;
    }
    public override void SaveDynamicData()
    {
        base.SaveDynamicData();

        if (data is not ItemData item_data) { return; }
        item_data.is_grabbed = Grabbed;
    }
}



// ITEM DATA
[Serializable]
public class ItemData : CapableData
{
    public string reference;
    public Color color;
    public int max_qty;
    public string item_description;
    public bool is_grabbed; // only a flag, for the CapacityEngine to know which capacities not to load

    // CONSTRUCTOR
    public ItemData() : base() { }
    public ItemData(CapableData parent) : base(parent) { }

    // DUPLICATE
    public override ICapableData Duplicate()
    {
        return new ItemData(base.Duplicate() as CapableData)
        {
            reference = this.reference,
            color = this.color,
            max_qty = this.max_qty,
            item_description = this.item_description,
            is_grabbed = this.is_grabbed
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = base.GetDetails();
        details += $"  - reference : {reference}\n";
        details += $"  - color : {color}\n";
        details += $"  - max_qty : {max_qty}\n";
        details += $"  - item_description : {item_description}\n";
        details += $"  - is_grabbed : {is_grabbed}\n";
        return details;
    }





    // METAMORPHING FROM CAPABLE TO ITEM
    public virtual void InitFromCapable(CapableData capdata, ItemData template = null)
    {
        // we transfer some of the capable data to the corpse data
        position = capdata.position;
        orientation = capdata.orientation;
        tag = capdata.tag;

        // anim data
        if (capdata.anim_data != null)
        {
            AnimPlayerData new_anim_data = capdata.anim_data.Duplicate();
            new_anim_data.anim_capacity_priorities = this.anim_data.anim_capacity_priorities; // we keep the same anim capa priorities as the corpse template (ex : corpse anim capa priorities will be different from player anim capa priorities for example, because we want the corpse to play the "die" animation which has a higher priority than the "walk" animation for example, while for the player we want the "walk" animation to have a higher priority than the "die" animation for example)
            anim_data = new_anim_data;
        }

        if (template is null)
        {
            string entity_type = capdata.kind.ToLowerInvariant();
            
            // set default item data things
            reference = "leftover:" + entity_type;
            color = Color.softRed;
            max_qty = 24;
            item_description = "a \"thing\" coming from a " + entity_type + ". I don't really want to know more about it, since it definitely involved dark magic and forbidden science. Do not get closer";
            is_grabbed = false;
        }
        else
        {
            reference = template.reference;
            color = template.color;
            max_qty = template.max_qty;
            item_description = template.item_description;
            is_grabbed = template.is_grabbed;
        }
    }
}

public enum ItemType
{
    None,
    All,
    Physical, // this item is shown inside a "item" type of ui_slot
    Virtual // same with "file" type of ui_slot
}