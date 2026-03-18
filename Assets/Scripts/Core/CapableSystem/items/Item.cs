using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
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
        /* set
        {
            // check if the value is the same
            if (value == _grabbed) { return; }

            // we set the value
            _grabbed = value;
            if (value) { on_grabbed(); }
            else { on_dropped(); }
        } */
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
    public event Action<Item, Capable> OnGrabbed = delegate { };
    public event Action<Item> OnDropped = delegate { };

    // HOLDER
    public Capable _holder = null;
    public Capable Holder { get { return _holder; }}/* ItemPoolHolder != null ? ItemPoolHolder.Inventory.capable : null; */
    public ItemPool ItemPoolHolder
    {
        get
        {
            if (transform.parent == null) { return null; }
            return transform.parent.GetComponent<ItemPool>();
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
        if (rule == "device") { return this is Device; }

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
        _grabbed = true;
        on_grabbed();

        // we load the capacities we have not load yet & remove the room
        if (CapableSystem.Instance.log_loading_extended) { Debug.Log($"(Item - OnGrabbed) {data.id} unloading capacities : {string.Join(" ", dynamic_capacity_ids)}"); }
        CapacityEngine.Instance?.UnloadCapacities(dynamic_capacity_ids, this);
        CapableSystem.Instance?.OnItemGrabbed(this, Holder);

        // finally set the holder
        _holder = grabber;
    }
    public virtual void BeDropped(Capable dropper)
    {
        _grabbed = false;
        on_dropped();

        // we load the capacities and get a room
        if (CapableSystem.Instance.log_loading_extended) { Debug.Log($"(Item - OnDropped) {data.id} loading capacities : {string.Join(" ", dynamic_capacity_ids)}"); }
        CapacityEngine.Instance?.LoadCapacities(dynamic_capacity_ids, this);
        CapableSystem.Instance?.OnItemDropped(this, Holder);

        // finally we reset the holder
        _holder = null;
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
            AnimPlayer.DisableRenderer();
            if (Inventory != null)
            {
                List<Item> items = Inventory.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].AnimPlayer.DisableRenderer();
                }
            }
        }
        else
        {
            AnimPlayer.EnableRenderer();
            if (Inventory != null)
            {
                List<Item> items = Inventory.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    if (!items[i].Placed) { continue; }
                    items[i].AnimPlayer.EnableRenderer();
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

        // HOVER
        /* if (Hover == null) { return; }
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
        } */
    }


    // ON DESTROY
    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (!gameObject.scene.isLoaded) { return; } // this happens when the scene is destroyed when we quit the scene
        if (Holder != null) { Holder.Inventory.Remove(this); } // we remove the item from the holder's inventory
    }




    // DATA MANAGEMENT
    private List<string> dynamic_capacity_ids = new List<string>(); // this list is used to store the capacities that are loaded dynamically on grab, so we can unload them on drop
    public override void LoadData(CapableData data)
    {
        // we store our dynamic capacities ids
        List<string> static_ids = new List<string>();
        dynamic_capacity_ids = CapacityEngine.Instance.GetDynamicItemCapacitiesIDs(data.capacities_ids,ref static_ids);

        // we do a trick to make base.LoadData(data) only load the capacities we want to !
        List<string> capa_ids_saved = new List<string>(data.capacities_ids);
        if ((data as ItemData).is_grabbed) { data.capacities_ids = static_ids; } // if we are grabbed, we only load the static capacities, the dynamic ones will be loaded only when dropped
        base.LoadData(data);
        data.capacities_ids = capa_ids_saved; // we restore the original capacities ids list in case we need it later

        // we check if the data is of the correct type
        ItemData item_data = data as ItemData;
        if (item_data == null)
        {
            if (log) { Debug.LogError($"(Item - LoadData) The data provided is not of type ItemData for item '{name}'"); }
            return;
        }

        // we load the item data
        this.Reference = item_data.reference;
        this.Color = item_data.color;
        this.MaxQty = item_data.max_qty;
        this.ItemDescription = item_data.item_description;
        // ! no need to apply grabbed since it's only a flag

    }
    public override void UnloadData()
    {
        // we save the dynamic data we need for next loading to be perfect
        SaveDynamicData();

        base.UnloadData();

        // ? really useful ? no but it's better to have a safe guard
        // todo if perf problem when loading items, remove this
        this.Reference = "category:item";
        this.Color = Color.yellow;
        this.MaxQty = 1;
        this.ItemDescription = "description of the item (item data was not loaded, is there a problem ?)";
    }
    public override ICapableData GetStaticData()
    {
        ItemData static_data = new ItemData((CapableData)base.GetStaticData())
        {
            // set item data things
            reference = this.Reference,
            color = this.Color,
            max_qty = this.MaxQty,
            item_description = this.ItemDescription,
            is_grabbed = get_static_grabbed()
        };

        return static_data;
    }
    protected bool get_static_grabbed()
    {
        // we need to check if we have another capable in our parents or above, if yes it means we are grabbed
        if (transform.parent == null) { return false; }
        Capable parent_capable = transform.parent.GetComponentInParent<Capable>(includeInactive: true);
        return parent_capable != null;
    }
    public /* override */ void SaveDynamicData()
    {
        if (data is not ItemData item_data) { return; }
        item_data.is_grabbed = Grabbed;
    }
}



// ITEM DATA
[Serializable] public class ItemData : CapableData
{
    public string reference;
    public Color color;
    public int max_qty;
    public string item_description;
    public bool is_grabbed; // only a flag, for the CapacityEngine to know which capacities not to load

    // CONSTRUCTOR
    public ItemData(CapableData parent)
    {
        foreach (var prop in parent.GetType().GetProperties()) { prop.SetValue(this, prop.GetValue(parent)); }
        foreach (var prop in parent.GetType().GetFields()) { prop.SetValue(this, prop.GetValue(parent)); }
    }

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
}