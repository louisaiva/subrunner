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

        // we subscribe to our own OnItemChanged event
        this.OnItemChanged += HandleLaptopChanged;
    }

    // LOGGING
    private void HandleLaptopChanged(List<Item> items)
    {
        if (items == null || items.Count == 0)
        {
            Debug.Log($"(UI_LaptopItemSlot) Removed laptop from slot");
            return;
        }
        Debug.Log($"(UI_LaptopItemSlot) Laptop changed, new item: {items[0].Reference} with {items[0].Inventory.Count} modules installed");
    }

    // HAS LAPTOP
    public bool HasLaptop => this.Item != null && Item is Laptop;
    public Laptop Laptop => this.Item != null && Item is Laptop laptop ? laptop : null;
}