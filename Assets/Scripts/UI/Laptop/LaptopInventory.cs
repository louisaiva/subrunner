using System.Collections.Generic;
using UnityEngine;

public class LaptopInventory : Inventory
{
    [Header("Motherboard size")]
    public int Columns = 2;
    public int Rows = 2;

    [Header("Items slots indexes")]
    protected Dictionary<Item, int> items_slots = new Dictionary<Item, int>(); // store les indexes de slot de chaque item via item.ID


    // GRAB DROP REMOVE
    public override bool Grab(Item item, List<UI_Inventory> uis_to_ignore = null)
    {
        // check if we can grab the item
        if (!base.Grab(item, uis_to_ignore)) { return false; }

        // we add an new index on the dictionary
        items_slots[item] = (ui as UI_Laptop).GetItemSlotIndex(item);
        return true;
    }
    public override bool Drop(Item item, List<UI_Inventory> uis_to_ignore = null)
    {
        // check if we can drop the item
        if (!base.Drop(item, uis_to_ignore)) { return false; }

        // we remove the item from the dictionary
        items_slots.Remove(item);
        return true;
    }
    public override bool Remove(Item item)
    {
        // check if we can remove the item
        if (!base.Remove(item)) { return false; }

        // we remove the item from the dictionary
        items_slots.Remove(item);
        return true;
    }


    // GETTERS
    public List<Item> GetItemsInSlot(int slot_index)
    {
        List<Item> items = new List<Item>();
        foreach (KeyValuePair<Item, int> kvp in items_slots)
        {
            if (kvp.Value == slot_index)
            {
                items.Add(kvp.Key);
            }
        }
        return items;
    }
}