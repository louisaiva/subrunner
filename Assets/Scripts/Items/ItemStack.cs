
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable] public class ItemStack : Descriptable
{
    [Header("ItemPool Reference")]
    public ItemStorer Storer;

    [Header("Stack Parameters")]
    public string ItemReference
    {
        get
        {
            if (IsEmpty) { return ""; }
            return Items[0].Reference;
        }
    }
    public int MaxQty = 1;
    public List<Item> Items = new List<Item>();
    public bool IsFull { get { return Items.Count >= MaxQty; } }
    public bool IsEmpty { get { return Items.Count == 0; } }
    public int Quantity { get => Items.Count; }
    public Item Item => Items.Count > 0 ? Items[0] : null;
    public bool Stackable { get => MaxQty > 1; }

    // EVENTS
    public event System.Action OnUpdated = delegate { };


    // DESCRIPTABLE
    public string Name => ItemReference.Split(':').LastOrDefault() ?? "";
    public string Description => Item != null ? Item.ItemDescription : "/!\\ no data /!\\";


    /// <summary>
    /// Creates an ItemStack. The ItemPool pool parameter is used by the UI_ItemMover
    /// to find the ItemPool that created this stack. If null is provided, the UI_ItemMover
    /// will disable the ui_slots when moving. the rest should work fine.
    /// </summary>
    /// <param name="storer"></param>
    public ItemStack(ItemStorer storer) { Storer = storer; }


    // GETTERS
    public bool HasItem(Item item)
    {
        return Items.Contains(item);
    }
    public string GetDetails()
    {
        return $"Stack of '{ItemReference}'      -    qty/max qty : {Quantity}/{MaxQty}";
    }

    // RULE
    public bool ValidateRule(string rule) { return Item?.ValidateRule(rule) ?? false; }

    // CAN ADD
    public bool CanAdd(List<Item> items)
    {
        // checks if we can add the item to the slot (store or stack it on the slot)
        Item item = items.Count > 0 ? items[0] : null;

        // we check if the item is valid
        if (item == null) { return false; }

        // if we don't have any item, we can store it
        if (Quantity == 0) { return true; }

        // here we have already an item
        // we check if the item is the same
        if (ItemReference != item.Reference) { return false; }

        // we check if both items are stackable
        if (!item.Stackable || !Stackable) { return false; }

        // we check if they are modules and have the same upgrades
        if (item is Module module && Item is Module current_module)
        {
            if (!module.HasSameUpgrades(current_module)) { return false; }
        }

        // we check if the item is full
        if (Quantity + items.Count > MaxQty) { return false; }

        // we can stack the item !!
        return true;
    }
    public bool CanAdd(Item item) { return CanAdd(new List<Item>() { item }); }
    public bool CanAdd(ItemStack item_stack) { return CanAdd(item_stack.Items); }

    // ADD / REMOVE
    public void Add(Item item)
    {
        if (IsEmpty)
        {
            // we set the max qty
            MaxQty = item.MaxQty;
            Items.Add(item);
            OnUpdated?.Invoke();
            return;
        }

        // if (item.Reference != ItemReference) { return false; }
        // if (IsFull) { return false; }

        Items.Add(item);
        OnUpdated?.Invoke();
        return;
    }
    public void Remove(Item item)
    {
        if (IsEmpty) { return; }
        if (!Items.Contains(item)) { return; }

        bool removed = Items.Remove(item);
        if (!removed) { return; }

        // if we are empty, we reset the stack
        if (IsEmpty)
        {
            MaxQty = 1;
        }

        OnUpdated?.Invoke();
        return;
    }
    public void Clear()
    {
        Items.Clear();
        MaxQty = 1;
        OnUpdated?.Invoke();
    }
}
