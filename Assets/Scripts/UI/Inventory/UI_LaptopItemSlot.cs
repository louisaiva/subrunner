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
        if (log) { Debug.Log($"(UI_LaptopItemSlot) Laptop changed, new item: {items[0].Reference} with {items[0].Inventory.Count} modules installed"); }
        return true;
    }
    public override bool Unstore(Item item)
    {
        if (!base.Unstore(item)) { return false; }

        // if we are here we successfully dropped item
        // we check if we dropped a laptop that was using trojan / cyborg_puppet since we don't want them to
        // continue if we are not here to stop them !!!! (if perso is not controlled he can't grab back the laptop)
        // and if he can't grab the laptop he can't cancel the hack
        // so it is stuck in the trojan / cyborg

        if (item is Laptop laptop) { laptop.Hacker.CancelControlHacks(); }

        if (log) { Debug.Log($"(UI_LaptopItemSlot) Removed laptop from slot"); }
        return true;
    }
    public override void Clear()
    {
        if (Item is Laptop laptop) { laptop.Hacker.CancelControlHacks(); }

        base.Clear();
        if (log) { Debug.Log($"(UI_LaptopItemSlot) Cleared laptop slot"); }
    }

    // HAS LAPTOP
    public bool HasLaptop => this.Item != null && Item is Laptop;
    public Laptop Laptop => this.Item != null && Item is Laptop laptop ? laptop : null;
}