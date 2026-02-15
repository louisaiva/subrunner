using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Inventory : MonoBehaviour, ItemStorer
{
    
    [Header("ItemPools")]
    public List<ItemPool> pools = new List<ItemPool>();
    public List<Item> Items { get { return pools.SelectMany(p => p.Items).ToList(); } }
    public int Count
    {
        get
        {
            return pools.Sum(pool => pool.Count);
        }
    }

    // EVENTS
    public event Action<Item> OnItemGrabbedAtStart = delegate { };
    public event Action<Item> OnItemGrabbed = delegate { };
    public event Action<Item> OnItemDropped = delegate { };

    /* [Header("Components")]
    [SerializeField] private List<UI_Inventory> uis = new List<UI_Inventory>();
    public UI_Inventory ui { get { return uis.Count > 0 ? uis[0] : null; } }
    public UI_Slottable MainUI { get
        {
            if (ui == null) { return null; }
            if (ui.Mixer != null) { return ui.Mixer; }
            return ui;
        }
    } */
    public Capable capable { get { return transform.parent.GetComponent<Capable>(); } }

    [Header("Logs")]
    [SerializeField] protected bool log = false;

    // AWAKE
    protected virtual void Awake()
    {
        /* if (capable is Perso && uis.Count > 0 && uis[0] == null)
        {
            // we just revived we don't have any uis, so we make them
            uis = new List<UI_Inventory>
            {
                UI_Manager.Instance.GetPool("inventory").transform.Find("ui_inventory").GetComponent<UI_Inventory>(),
                // UI_Manager.Instance.GetPool("quick").GetComponent<UI_HUD>().perso_quick_inventory
                UI_Manager.Instance.GetPool<UI_ChestPool>().UI
            };
        } */

        // we attach the inventory to the pools
        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i] == null) { continue; }
            pools[i].AttachToInventory(this);
        }
    }

    // START
    /* protected virtual void Start()
    {
        // on initialise l'UI
        foreach (UI_Inventory ui in uis)
        {
            if (ui == null)
            {
                Debug.LogWarning("(Inventory) " + name + $" has a null UI_Inventory : skipping initialization");
                continue;
            }
            ui.Init();
        }

        // on récupère les items
        foreach (Transform child in transform)
        {
            if (child == null || child.gameObject.activeSelf == false) { continue; }
            Grab(child.GetComponent<Item>());
        }
    } */


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

        // we try to make the ui grab the item
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

        // we set the item to dropped (which enables the hover collider)
        item.Grabbed = false;

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
    public void GrabAtStart(Item item)
    {
        if (item == null) { return; }

        // we trigger the events
        OnItemGrabbedAtStart.Invoke(item);

        if (log) { Debug.Log("(Inventory) " + capable.name + " grabbed at start : " + item.name); }
    }




    // GETTERS
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
    /* public Item GetItem(string reference)
    {
        // we check if the item is in the inventory
        foreach (Item item in Items)
        {
            if (item.Reference == reference) { return item; }
        }
        return null;
    } */
    // todo these methods are not efficient, we should have them in the pool and check type there instead of going through Items
    public T GetItem<T>() where T : Item
    {
        // we get the first item of type T
        foreach (Item item in Items)
        {
            if (item is T) { return item as T; }
        }
        return null;
    }
    public List<T> GetItemsByType<T>() where T : Item
    {
        // we get all the items of type T
        List<T> items = new List<T>();
        foreach (Item item in Items)
        {
            if (item is T) { items.Add(item as T); }
        }
        return items;
    }
    public List<Item> GetItemsByRule(string rule = "")
    {
        // we get all the items that match the rule
        List<Item> items = new List<Item>();
        for (int i = 0; i < Items.Count; i++)
        {
            Item item = Items[i];
            if (item.ValidateRule(rule)) { items.Add(item); }
        }
        return items;
    }
    public Device GetDeviceItem()
    {
        // we check if one of our items is a device
        foreach (Item item in Items)
        {
            if (item is Device) { return item as Device; }
        }
        return null;
    }
    public bool HasItem(Item item)
    {
        // we check if we have the item
        return Items.Contains(item);
    }

    // UI MANAGEMENT
    /* public void RemoveAllUIs()
    {
        // we remove all UIs
        while (uis.Count > 0)
        {
            RemoveUI(uis[0]);
        }
    }
    public void RemoveUI(UI_Inventory ui_inventory)
    {
        if (!uis.Contains(ui_inventory)) { return; }

        // we remove the UI from the list
        uis.Remove(ui_inventory);
        ui_inventory.Inventory = null;
        if (log) { Debug.Log("(Inventory) " + capable.name + " removed UI_Inventory : " + ui_inventory.name); }
    }
    public void AddUI(UI_Inventory ui_inventory)
    {
        if (uis.Contains(ui_inventory)) { return; }

        // we add the UI to the list
        uis.Add(ui_inventory);
        ui_inventory.Inventory = this;
        if (log) { Debug.Log("(Inventory) " + capable.name + " added UI_Inventory : " + ui_inventory.name); }
    } */




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
