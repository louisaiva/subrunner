using System;
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

    // HAS LAPTOP
    public bool HasLaptop => this.Item != null && Item is Laptop;
    public Laptop Laptop => this.Item != null && Item is Laptop laptop ? laptop : null;
}