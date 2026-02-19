using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Inventory : MonoBehaviour, ItemStorer
{
    
    [Header("ItemPools")]
    public List<ItemPool> pools = new List<ItemPool>();
    public List<Item> Items { get { return pools.SelectMany(p => p.Items).ToList(); } }
    public int Count { get { return pools.Sum(pool => pool.Count); } }

    // specific pool getters
    protected ItemPool _shoes_stack = null;
    protected ItemPool shoes_stack { 
        get
        {
            if (_shoes_stack == null) { _shoes_stack = get_itempool("shoes_stack"); }
            return _shoes_stack;
        }
    }
    protected ItemPool _weapon_stack = null;
    protected ItemPool weapon_stack
    {
        get
        {
            if (_weapon_stack == null) { _weapon_stack = get_itempool("weapon_stack"); }
            return _weapon_stack;
        }
    }



    // EVENTS
    public event Action<Item> OnItemGrabbedFromLowerLevel = delegate { };
    public event Action<Item> OnItemGrabbed = delegate { };
    public event Action<Item> OnItemDropped = delegate { };

    public Capable capable { get { return transform.parent.GetComponent<Capable>(); } }

    [Header("Logs")]
    [SerializeField] protected bool log = false;

    // AWAKE
    protected virtual void Awake()
    {
        // we attach the inventory to the pools
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i] == null) { continue; }
            pools[i].AttachToInventory(this);
        }
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
                if (log) { Debug.Log("(Inventory) " + capable.name + " grabbed : " + item.name + " in shoes_stack"); }
                return true;
            }
        }
        else if (item is Weapon && weapon_stack != null && weapon_stack.HasSpaceLeft)
        {
            if (weapon_stack.Grab(item))
            {
                OnItemGrabbed.Invoke(item);
                if (log) { Debug.Log("(Inventory) " + capable.name + " grabbed : " + item.name + " in weapon_stack"); }
                return true;
            }
        }


        // we try to make all the pools grab the item
        if (!pool_grab(item)) { return false; }


        // we trigger the events
        OnItemGrabbed.Invoke(item);

        if (log) { Debug.Log("(Inventory) " + capable.name + " grabbed : " + item.name); }

        return true;
    }
    public virtual bool Drop(Item item)
    {
        // we check if we can remove the item
        if (item == null) { return false; }

        if (!pool_drop(item)) { return false; }

        // todo call the potential DropCapacity of the capable ?

        // we trigger the event
        OnItemDropped.Invoke(item);

        // we update the UI
        // ui_drop(item, uis_to_ignore);

        if (log) { Debug.Log("(Inventory) " + capable.name + " dropped : " + item.name); }

        return true;
    }
    public virtual bool Remove(Item item)
    {
        // only for items that are going to be destroyed

        // we check if we can remove the item
        if (item == null) { return false; }
        for (int i = 0; i < pools.Count; i++)
        {
            if (!pools[i].Drop(item)) { continue; }

            if (log) { Debug.Log("(Inventory) " + capable.name + " removed : " + item.name); }
            return true;
            
        }

        if (log) { Debug.LogWarning("(Inventory) " + capable.name + " can't remove : " + item.name); }
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
        }
        return false;
    }
    protected bool pool_drop(Item item)
    {
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i].Drop(item)) { return true; }
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
        OnItemGrabbedFromLowerLevel.Invoke(item);

        if (log) { Debug.Log("(Inventory) " + capable.name + " grabbed from lower level : " + item.name); }
    }


    // SPECIFIC GETTERS
    public Usable GetShoes() { return get_item_from_rule_in_itempool("shoes_stack") as Usable; }
    public Usable GetWeapon() { return get_item_from_rule_in_itempool("weapon_stack") as Usable; }
    public Device GetDeviceItem() { return get_item_from_rule_in_itempool("device_stack","device") as Device; }
    public Usable GetConso(int index) { return get_item_from_rule_in_itempool("conso_" + index) as Usable; }
    private Item get_item_from_rule_in_itempool(string pool_name,string rule="usable")
    {
        // checks if we have the pool
        ItemPool pool = get_itempool(pool_name);
        if (pool == null)
        {
            if (log) { Debug.LogWarning($"(Inventory) {pool_name} ItemPool was NOT found :O"); }
            return null;
        }
    
        // find the matching item rule items
        List<Item> usables_in_pool = pool.GetItemsByRule(rule);
        if (usables_in_pool.Count > 0) { return usables_in_pool[0]; }

        // else we have no matching item in the pool
        if (log) { Debug.LogWarning($"(Inventory) {pool_name} ItemPool was found but no \"{rule}\" inside ://"); }
        return null;
    }
    private ItemPool get_itempool(string pool_name)
    {
        // checks if we have the pool
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i].name != pool_name) { continue; }
            return pools[i];
        }

        // else we have no pool named like this
        // if (log) { Debug.LogWarning($"(Inventory) {pool_name} ItemPool was NOT found :O"); }
        return null;
    }
    /* public ItemPool GetItemPoolThatHoldsItemStack(ItemStack stack)
    {
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i].HasStack(stack)) { return pools[i]; }
        }
        return null;
    } */


    // GLOBAL GETTERS
    public Inventory GetInteractingInventory()
    {
        string s = "(Inventory) " + capable.name + " is looking for an interacting inventory\n\n";

        // check if we are the interactable (so we look for the interactor)
        // typically we are dropping an item from a Chest's UI_Inventory
        if (capable is Interactable)
        {
            // this is the other capable
            s += "we are the interactable\n";
            InteractCapacity interactor = (capable as Interactable).Interactor;

            // check if we have an interactor
            if (interactor == null) { if (log) { Debug.LogWarning(s + "we don't have an interactor\n"); } return null; }

            // yes we do !! return its inventory
            if (log)
            {
                Debug.Log(s + "we have an interactor : " + interactor.capable.name
                + "\nand its inventory is " + interactor.capable.Inventory.name);
            }
            return interactor.capable.Inventory;
        }

        // check if we are the interactor (so we look for the interactable)
        // typically we are dropping from an item our perso_quick_inventory or the UI_InventoryMenu
        else if (capable.GetCapacity<InteractCapacity>() != null)
        {
            // this is our capable
            s += "we are the interactor\n";
            InteractCapacity interactor = capable.GetCapacity<InteractCapacity>();

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

    // ITEM RULE
    public string ItemRule
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
