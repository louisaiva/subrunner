using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI_Inventory is the highest UI representation of the Inventory.
/// it is never triggered directly, but is showed by the Capacities & updated by the Inventory.
/// </summary>

public class UI_Inventory : UI_Slottable
{
    [SerializeField] private bool log_get_slots = false;

    [Header("UI_Item Pools")]
    public List<UI_ItemPool> pools = new List<UI_ItemPool>();

    [Header("Components")]
    public Inventory Inventory;

    // INIT
    public virtual void Init()
    {
        // we check if we have some pools, otherwise we set ourself as the pool
        if (pools.Count == 0)
        {
            try
            {
                Debug.LogError("(UI_Inventory) no pool found on " + transform.parent.parent.parent.name +
                ", please set at least one pool in the inspector");
                return;
            }
            catch
            {
                Debug.LogError("(UI_Inventory) no pool found on " + name);
            }
        }

        // on initialise les pools
        foreach (UI_ItemPool pool in pools)
        {
            if (pool == null)
            {
                Debug.LogWarning("(Inventory) " + name + $" has a null UI_ItemPool : {pool}, skipping initialization");
                continue;
            } // skip null UIs
            pool.Init(this);
        }
    }
    public void Refresh()
    {
        // on clear les pools
        foreach (UI_ItemPool pool in pools)
        {
            if (pool == null) { continue; } // skip null UIs
            pool.DestroyAllSlots();
        }

        if (Inventory == null)
        {
            if (log) { Debug.LogWarning("(UI_Inventory) " + name + " has no Inventory, cannot refresh"); }
            return;
        }

        // on récupère tous les items de l'Inventaire et on les fait grab si possible par nous mêmes
        foreach (Item item in Inventory.Items)
        {
            bool grabbed = UI_Grab(item);
            if (!grabbed)
            {
                Debug.LogWarning("(UI_Inventory) could not grab item " + item.Reference + " in " + name +
                ", maybe the pools are full or the item is incompatible");
            }
        }
    }

    // GRAB
    public virtual bool UI_Grab(Item item)
    {

        // if we have an UI_ItemPool called "shortcuts" then we check if we have any slots with the same item ref
        UI_ItemPool shortcuts_pool = pools.Find(pool => pool.name == "shortcuts_pool");
        if (shortcuts_pool != null && shortcuts_pool.CanStore(item) && shortcuts_pool.FullCount > 0)
        {
            // get the shortcuts slots
            UI_Item[] shortcuts_slots = shortcuts_pool.GetFilledSlots().ToArray();
            for (int i = 0; i < shortcuts_slots.Length; i++)
            {
                UI_Item ui_item = shortcuts_slots[i];

                // we check if the item references match
                if (ui_item.Item.Reference != item.Reference) { continue; }
            
                // we try to add the item to this slot
                bool stored = ui_item.Store(item);
                if (stored)
                {
                    if (log) { Debug.Log("(UI_Inventory) grabbed " + item.Reference + " in shortcut slot " + ui_item.name); }
                    return true;
                }
            }
        }


        // we try to grab the item in every pool
        foreach (UI_ItemPool pool in pools)
        {
            bool grabbed = pool.Grab(item);
            if (grabbed)
            {
                if (log) { Debug.Log("(UI_Inventory) grabbed " + item.Reference + " in " + pool.name); }
                return true;
            }
        }

        // if we are here, no pool could take the item
        if (log)
        {
            Debug.LogWarning("(UI_Inventory) no pool could take the item " + item.Reference +
        " in " + Inventory.capable.name + "'s ui_inventory, maybe they are full or the item is incompatible");
        }

        return false;
    }
    public virtual bool UI_Drop(Item item)
    {
        // we go through the children
        foreach (UI_ItemPool pool in pools)
        {
            // we try to drop the item in the pool
            bool dropped = pool.Drop(item);
            if (dropped)
            {
                if (log) { Debug.Log("(UI_Inventory) dropped " + item.Reference + " in " + pool.name); }
                return true;
            }
        }

        if (log)
        {
            Debug.LogWarning("(UI_Inventory) no pool could drop the item " + item.Reference +
        " in " + Inventory.capable.name + "'s ui_inventory, please check the pools and the item type");
        }

        return false;
    }
    public bool UI_Regrab(Item item)
    {
        // we try to drop the item AND directly after, grab it.
        // if it is successful we don't even warn the Inventory about this, it is just pure black market
        
        bool dropped = UI_Drop(item);
        if (!dropped) { Debug.LogError("(UI_Inventory) could not drop item " + item.Reference + " in " + name); return false; }

        bool grabbed = UI_Grab(item);
        if (grabbed) { if (log) { Debug.Log($"(UI_Inventory) successfully re-grabbed {item.Reference}"); } return true; }

        // otherwise we can't grab the item ://
        return false;
    }






    // SLOTTABLE
    public override List<UI_Slot> GetSlots()
    {
        string debug_slots = "";

        List<UI_Slot> slots = new List<UI_Slot>();
        Vector2 position = Vector2.negativeInfinity;
        for (int i=0; i< pools.Count; i++)
        {
            debug_slots += $"-- pool {pools[i].name} -- \n";
            UI_ItemPool pool = pools[i];
            for (int j=0; j< pool.transform.childCount; j++)
            {
                // we check if the ui_slot is enabled
                Transform child = pool.transform.GetChild(j);
                if (!child.gameObject.activeSelf) { continue; }

                // we check if the slot is a UI_Slot
                UI_Slot slot = child.GetComponent<UI_Slot>();
                if (slot == null) { continue; }
                if (slot.Disabled) { continue; }
                slots.Add(slot);
                debug_slots += $"    --> slot {slot.name} at position {child.position}\n";

                // we update the position to the first slot
                if (position == Vector2.negativeInfinity)
                {
                    position = child.position;
                }
            }
        }

        // we concatenate the ui_slottable's slots
        slots.AddRange(base.GetSlots());

        if (log_get_slots) { Debug.Log($"(UI_Inventory) {name} getting slots : {slots.Count} slots\n" + debug_slots); }

        return slots;
    }
    public override bool IsYourSlot(UI_Slot slot)
    {
        if (base.IsYourSlot(slot)) { return true; }

        // we check if the slot is in the inventory
        for (int i = 0; i < pools.Count; i++)
        {
            UI_ItemPool pool = pools[i];
            for (int j = 0; j < pool.transform.childCount; j++)
            {
                Transform child = pool.transform.GetChild(j);
                if (child.GetComponent<UI_Slot>() == slot) { return true; }
            }
        }
        return false;
    }
}