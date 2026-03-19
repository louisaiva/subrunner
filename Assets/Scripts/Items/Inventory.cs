using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.Mathematics;

public class Inventory : MonoBehaviour, ItemStorer
{





    
    [Header("ItemPools")]
    [SerializeField] private bool clear_and_assign_pools_in_awake = true;
    [SerializeField] private List<ItemPool> pools = new List<ItemPool>();
    public List<Item> Items { get { return pools.SelectMany(p => p.Items).ToList(); } }
    public int Count { get { return pools.Sum(pool => pool.Count); } }
    public List<ItemStack> Stacks { get { return pools.SelectMany(p => p.Stacks).ToList(); } }

    // specific pool getters
    protected ItemPool _shoes_stack = null;
    protected ItemPool shoes_stack { 
        get
        {
            if (_shoes_stack == null) { _shoes_stack = get_itempool("shoes"); }
            return _shoes_stack;
        }
    }
    protected ItemPool _weapon_stack = null;
    protected ItemPool weapon_stack
    {
        get
        {
            if (_weapon_stack == null) { _weapon_stack = get_itempool("weapon"); }
            return _weapon_stack;
        }
    }



    // EVENTS
    public event Action<Item> OnItemGrabbedFromLowerLevel = delegate { };
    public event Action<Item> OnItemGrabbed = delegate { };
    public event Action<Item> OnItemDropped = delegate { };

    // EVENTS
    public event Action<ItemStack> OnStackCreated = delegate { };
    public event Action<ItemStack> OnStackRemoved = delegate { };

    private Capable _capable;
    public Capable Capable { get
        {
            if (_capable == null) { _capable = transform.parent.GetComponent<Capable>(); }
            return _capable;
        }}

    [Header("Logs")]
    [SerializeField] protected bool log = false;
    [SerializeField] protected bool log_grab = false;
    [SerializeField] protected bool log_get_items = false;

    // AWAKE
    protected virtual void Awake()
    {
        if (clear_and_assign_pools_in_awake)
        {
            // we clear the pools list and assign it with all the ItemPool found in children
            pools = new List<ItemPool>(GetComponents<ItemPool>());
            
            // we go through DIRECT children and DIRECT only otherwise we will pick the ItemPool of items in their inventory which we DO NOT want
            for (int i = 0; i < transform.childCount; i++)
            {
                pools.AddRange(transform.GetChild(i).GetComponents<ItemPool>());
            }
            
        }

        // we attach the inventory to the pools
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i] == null) { continue; }
            pools[i].AttachToInventory(this);

            // & assign callbacks
            pools[i].OnStackCreated += (stack) => { OnStackCreated?.Invoke(stack); };
            pools[i].OnStackRemoved += (stack) => { OnStackRemoved?.Invoke(stack); };
        }
    }


    // LOADING / UNLOADING INVENTORY DATA
    public void LoadInventoryData(InventoryData data)
    {
        // check if we have data
        if (data == null /* || data.item_pools_data == null */) { return; }

        // we suppose we already have the right amount of ItemPools (should be built in CapableBank)

        // we gather the real ItemPool
        List<ItemPool> pools_to_fill = gameObject.GetComponents<ItemPool>().ToList();
        for (int i = 0; i < transform.childCount; i++)
        {
            pools_to_fill.AddRange(transform.GetChild(i).GetComponents<ItemPool>());
        }

        // we check if we have the same amount of what the data says
        if (pools_to_fill.Count != data.item_pools_data.Count)
        {
            Debug.LogWarning($"(Inventory - LoadInventoryData) Inventory data has {data.item_pools_data.Count} pools but we have {pools_to_fill.Count} pools on {Capable.name}");
        }

        // we load the data in the pools
        for (int i = 0; i < data.item_pools_data.Count; i++)
        {
            if (i >= pools_to_fill.Count) { break; }
            pools_to_fill[i].LoadPoolData(data.item_pools_data[i]);

            // we add the pool
            pools.Add(pools_to_fill[i]);

            // we attach the pool
            pools_to_fill[i].AttachToInventory(this);

            // & assign callbacks
            pools_to_fill[i].OnStackCreated += (stack) => { OnStackCreated?.Invoke(stack); };
            pools_to_fill[i].OnStackRemoved += (stack) => { OnStackRemoved?.Invoke(stack); };
        }
    }
    public void UnloadInventoryData()
    {
        // List<ItemPoolData> new_datas = new List<ItemPoolData>();

        for (int i = 0; i < pools.Count; i++)
        {
            // new_datas.Add(pools[i].GetDynamicPoolData()); // we get the data (so we can save it)
            pools[i].UnloadPoolData(); // we unload the pool
        }

        // we save the data into our capable.data.inventory.item_pools_data
        // Capable.data.inventory.item_pools_data = new_datas;

        // finally we remove the pools
        pools.Clear();
    }
    public void SaveDynamicInventoryData()
    {
        List<ItemPoolData> new_datas = new List<ItemPoolData>();

        for (int i = 0; i < pools.Count; i++)
        {
            new_datas.Add(pools[i].GetDynamicPoolData()); // we get the data (so we can save it)
        }

        // we save the data into our capable.data.inventory.item_pools_data
        Capable.data.inventory.item_pools_data = new_datas;
    }

    // STATIC DATA
    public InventoryData GetStaticInventoryData()
    {
        InventoryData data = new InventoryData()
        {
            item_pools_data = new List<ItemPoolData>()
        };

        // we gather the real ItemPool
        List<ItemPool> pools = GetStaticItemPools();
        for (int i = 0; i < pools.Count; i++)
        {
            data.item_pools_data.Add(pools[i].GetStaticPoolData());
        }       

        return data;
    }
    public List<ItemPool> GetStaticItemPools()
    {
        // we gather the real ItemPool
        List<ItemPool> pools = gameObject.GetComponents<ItemPool>().ToList();
        for (int i = 0; i < transform.childCount; i++)
        {
            pools.AddRange(transform.GetChild(i).GetComponents<ItemPool>());
        }
        return pools;
    }
    public List<Item> GetStaticItems()
    {
        // we gather the real ItemPool
        List<ItemPool> pools = GetStaticItemPools();
        List<Item> items = new List<Item>();
        for (int i = 0; i < pools.Count; i++)
        {
            items.AddRange(pools[i].GetStaticItems());
        }
        return items;
    }




    // GRAB / DROP
    /// <summary>
    /// these 3 methods are the main one. when they are activated they
    /// make the right pool do the action, then trigger the event
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public virtual bool Grab(Item item)
    {
        // we check if we can add the item
        if (item == null) { return false; }


        // special cases - if we have a shoes and shoes_stack is empty we force to drop it there first
        if (item is Shoes && shoes_stack != null && shoes_stack.HasSpaceLeft)
        {
            if (shoes_stack.Grab(item))
            {
                OnItemGrabbed.Invoke(item);
                if (log_grab) { Debug.Log("(Inventory) " + Capable.name + " grabbed : " + item.name + " in shoes_stack"); }
                return true;
            }
        }
        else if (item is Weapon && weapon_stack != null && weapon_stack.HasSpaceLeft)
        {
            if (weapon_stack.Grab(item))
            {
                OnItemGrabbed.Invoke(item);
                if (log_grab) { Debug.Log("(Inventory) " + Capable.name + " grabbed : " + item.name + " in weapon_stack"); }
                return true;
            }
        }


        // we try to make all the pools grab the item
        if (!pool_grab(item))
        {
            if (log_grab) { Debug.LogWarning("(Inventory) " + Capable.name + " can't grab : " + item.name); }
            return false;
        }


        // we trigger the events
        OnItemGrabbed.Invoke(item);

        if (log_grab) { Debug.Log("(Inventory) " + Capable.name + " grabbed : " + item.name); }

        return true;
    }
    public virtual bool Drop(Item item, bool on_ground = true)
    {
        // we check if we can remove the item
        if (item == null) { return false; }

        if (!pool_drop(item, on_ground)) { return false; }

        // todo call the potential DropCapacity of the capable ?

        // we trigger the event
        OnItemDropped.Invoke(item);

        // we update the UI
        // ui_drop(item, uis_to_ignore);

        if (log) { Debug.Log("(Inventory) " + Capable.name + " dropped : " + item.name); }

        return true;
    }
    public virtual bool Remove(Item item)
    {
        // only for items that are going to be destroyed

        // we check if we can remove the item
        if (item == null) { return false; }
        for (int i = 0; i < pools.Count; i++)
        {
            if (!pools[i].Drop(item, on_ground:false)) { continue; }

            if (log) { Debug.Log("(Inventory) " + Capable.name + " removed : " + item.name); }
            return true;
            
        }

        if (log) { Debug.LogWarning("(Inventory) " + Capable.name + " can't remove : " + item.name); }
        return false;
    }


    /// <summary>
    /// These methods are low level equivalent of the aboves. it go through all the ItemPool and tries to make them grab/drop the item. Returns true if 
    /// a ItemPool grabbed/dropped succesfully, false otherwise
    /// </summary>
    /// <returns></returns>
    protected bool pool_grab(Item item)
    {
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i].Grab(item)) { return true; }
            if (log_grab) { Debug.Log("(Inventory) " + Capable.name + " pool " + pools[i].name + " could not grab : " + item.name); }
        }
        return false;
    }
    protected bool pool_drop(Item item, bool on_ground = true)
    {
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i].Drop(item, on_ground: on_ground)) { return true; }
        }
        return false;
    }

    /// <summary>
    /// This particular method should be thought of the same as Grab() but
    /// the grab already happened in a lower level (ItemPool grabbed an Item during Start() probably)
    /// Then we need to fire the event so that's the only purpose of this method after all
    /// </summary>
    public void GrabFromLowerLevel(Item item)
    {
        if (item == null) { return; }

        // we trigger the events
        OnItemGrabbedFromLowerLevel?.Invoke(item);

        if (log) { Debug.Log("(Inventory) " + Capable.name + " grabbed from lower level : " + item.name); }
    }
    public void DropFromLowerLevel(Item item)
    {
        if (item == null) { return; }

        // we trigger the events
        OnItemDropped?.Invoke(item);

        if (log) { Debug.Log("(Inventory) " + Capable.name + " dropped from lower level : " + item.name); }
    }


    // STACK MANAGEMENT
    public bool GrabInStack(Item item, ItemStack stack)
    {
        // we check if we have the stack
        ItemPool pool = GetStackPool(stack);
        if (pool == null)
        {
            // we don't have the stack, we don't care we try to grab it normally
            bool grabbed = Grab(item);
            return grabbed;
        }

        // we have the stack, we try to grab it in it
        return pool.GrabInStack(item, stack);
    }
    public ItemPool GetStackPool(ItemStack stack)
    {
        // we get the stack in one of our pools
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i].HasStack(stack)) { return pools[i]; }
        }
        return null;
    }



    // SPECIFIC GETTERS
    public Usable GetShoes() { return get_item_from_rule_in_itempool("shoes") as Usable; }
    public Usable GetWeapon() { return get_item_from_rule_in_itempool("weapon") as Usable; }
    public Device GetDeviceItem() { return get_item_from_rule_in_itempool("device","device") as Device; }
    public Usable GetConso(int index) { return get_item_from_rule_in_itempool("conso_" + index) as Usable; }
    private Item get_item_from_rule_in_itempool(string pool_id,string rule="usable")
    {
        // checks if we have the pool
        ItemPool pool = get_itempool(pool_id);
        if (pool == null)
        {
            if (log_get_items) { Debug.LogWarning($"(Inventory) {pool_id} ItemPool was NOT found :O"); }
            return null;
        }
    
        // find the matching item rule items
        List<Item> usables_in_pool = pool.GetItemsByRule(rule);
        if (usables_in_pool.Count > 0) { return usables_in_pool[0]; }

        // else we have no matching item in the pool
        if (log_get_items) { Debug.LogWarning($"(Inventory) {pool_id} ItemPool was found but no \"{rule}\" inside ://"); }
        return null;
    }
    private ItemPool get_itempool(string pool_id)
    {
        // checks if we have the pool
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i].PoolID != pool_id) { continue; }
            return pools[i];
        }

        // else we have no pool named like this
        // if (log) { Debug.LogWarning($"(Inventory) {pool_id} ItemPool was NOT found :O"); }
        return null;
    }


    // GLOBAL GETTERS
    public Inventory GetInteractingInventory()
    {
        string s = "(Inventory) " + Capable.name + " is looking for an interacting inventory\n\n";

        // check if we are the interactable (so we look for the interactor)
        // typically we are dropping an item from a Chest's UI_Inventory
        if (Capable is Interactable)
        {
            // this is the other capable
            s += "we are the interactable\n";
            InteractCapacity interactor = (Capable as Interactable).Interactor;

            // check if we have an interactor
            if (interactor == null) { if (log) { Debug.LogWarning(s + "we don't have an interactor\n"); } return null; }

            // yes we do !! return its inventory
            if (log)
            {
                Debug.Log(s + "we have an interactor : " + interactor.Capable.name
                + "\nand its inventory is " + interactor.Capable.Inventory.name);
            }
            return interactor.Capable.Inventory;
        }

        // check if we are the interactor (so we look for the interactable)
        // typically we are dropping from an item our perso_quick_inventory or the UI_InventoryMenu
        else if (Capable.GetCapacity<InteractCapacity>() != null)
        {
            // this is our capable
            s += "we are the interactor\n";
            InteractCapacity interactor = Capable.GetCapacity<InteractCapacity>();

            // check if we have an interactable
            Capable interactable = interactor.interactable as Capable;
            if (interactable == null) { if (log) { Debug.LogWarning(s + "we don't have an interactable\n"); } return null; }

            s += "we have an interactable : " + interactable.name + "\n";

            // checks if this is a chest
            if (interactable is not Chest chest) { if (log) { Debug.LogWarning(s + "but it's not a Chest\n"); } return null; }
            else if (interactable.Inventory == null) { if (log) { Debug.LogWarning(s + "but it doesn't have an inventory\n"); } return null; }

            s += "and it's a Chest\n";

            // checks if the chest is not closed or closing
            if (!chest.is_open && !chest.is_moving) { if (log) { Debug.LogWarning(s + "but it's closed & not opening\n"); } return null; }
            else if (chest.is_open && chest.is_moving) { if (log) { Debug.LogWarning(s + "but it's closing\n"); } return null; }

            s += "and it's open !!\n";

            // we return the interactable's inventory
            if (log) { Debug.Log(s + "and its inventory is " + interactable.Inventory.name + "\n\n"); }
            return interactable.Inventory;
        }

        // we return null
        return null;
    }
    public T GetItem<T>() where T : Item
    {
        for (int i = 0; i < pools.Count; i++)
        {
            T item = pools[i].GetItem<T>();
            if (item != null) { return item; }
        }
        return null;
    }
    public List<T> GetItemsByType<T>() where T : Item
    {
        // we get all the items of type T
        List<T> items = new List<T>();
        for (int i = 0; i < pools.Count; i++)
        {
            items.AddRange(pools[i].GetItemsByType<T>());
        }
        return items;
    }
    public List<Item> GetItemsByRule(string rule = "")
    {
        // we get all the items that match the rule
        List<Item> items = new List<Item>();
        for (int i = 0; i < pools.Count; i++)
        {
            items.AddRange(pools[i].GetItemsByRule(rule));
        }
        return items;
    }
    public bool HasItem(Item item)
    {
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i].HasItem(item)) { return true; }
        }
        return false;
    }

    public ItemPool GetItemPool(string poolID)
    {
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i].PoolID == poolID) { return pools[i]; }
        }
        return null;
    }

    // ITEM RULE
    public bool ValidateRule(Item item) { return item.ValidateRule(item_rule); }

    public string ItemRule { get { return item_rule; } }
    private string item_rule
    {
        get
        {
            if (pools.Count > 0)
            {
                string rules = "";
                for (int i = 0; i < pools.Count; i++)
                {
                    rules += pools[i].item_rule + "|";
                }
                return rules.TrimEnd('|');
            }
            return "";
        }
    }
}


public interface ItemStorer
{
    public GameObject gameObject { get; }

    // items access
    public List<Item> Items { get; }
    public int Count { get; }
    public List<ItemStack> Stacks { get; }

    // events
    public event Action<ItemStack> OnStackCreated;
    public event Action<ItemStack> OnStackRemoved;
    public event Action<Item> OnItemGrabbed;
    public event Action<Item> OnItemDropped;

    // GRAB / DROP
    public bool Grab(Item item);
    public bool Drop(Item item, bool on_ground = true);

    // STACK MANAGEMENT
    // public void SwapStacks(ItemStack stack1, ItemStack stack2);
    // public void MergeIntoStack(ItemStack from, ItemStack to);
    public bool GrabInStack(Item item, ItemStack stack);
    
    // ITEM RULE
    public bool ValidateRule(Item item);
}