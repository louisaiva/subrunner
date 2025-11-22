using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_LaptopItemSlot : UI_Item, Awakable
{

    // SINGLETON LOGIC
    public static UI_LaptopItemSlot Instance { get; private set; }
    public void InitAwake()
    {
        // Singleton logic
        if (Instance != null) { Destroy(Instance.gameObject); }
        Instance = this;

        if (log) { Debug.Log($"(UI_LaptopItemSlot) {name} initialized as singleton"); }
    }


    // LAPTOP GRABBING / DROPPING
    public override bool Store(Item item)
    {
        if (!base.Store(item)) { return false; }
        // if we are here we successfully grabbed item
        Perso.Instance.Laptop = item as Laptop;

        if (log) { Debug.Log($"(UI_LaptopItemSlot) Laptop changed, new item: {items[0].Reference} with {items[0].Inventory.Count} modules installed"); }
        return true;
    }
    public override bool Unstore(Item item)
    {
        if (!base.Unstore(item)) { return false; }

        Perso.Instance.Laptop = null;

        if (log) { Debug.Log($"(UI_LaptopItemSlot) Removed laptop from slot"); }
        return true;
    }
    public override void Clear()
    {
        Perso.Instance.Laptop = null;

        base.Clear();
        if (log) { Debug.Log($"(UI_LaptopItemSlot) Cleared laptop slot"); }
    }
    public override void SwitchItems(List<Item> items, bool items_moved = true)
    {
        if (log) { Debug.Log($"(UI_LaptopItemSlot) Switching laptop items from {Perso.Instance.Laptop?.Reference} to {items[0]?.Reference}"); }
        base.SwitchItems(items, items_moved);

        // we update the perso laptop reference
        Perso.Instance.Laptop = Item as Laptop;
    }

    // HAS LAPTOP
    public bool HasLaptop => this.Item != null && Item is Laptop;
    public Laptop Laptop => this.Item != null && Item is Laptop laptop ? laptop : null;
}