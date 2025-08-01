using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

/// <summary>
/// UI_ModulePool is a helper class to manage the module pool in the UI_Laptop
/// it directly inherits from UI_ItemPool
/// and is used to manage the modules in the laptop
/// </summary>
public class UI_ModulePool : UI_ItemPool
{
    [Header("Laptop Module Pool")]
    [SerializeField] private UI_LaptopItemSlot UI_LaptopItemSlot;
    // public bool HasLaptop => UI_LaptopItemSlot != null && UI_LaptopItemSlot.FullCount > 0;
    public bool HasLaptop = false;


    // INIT
    public override void Init(UI_Inventory ui)
    {
        UI_LaptopItemSlot.Instance.OnItemChanged += HandleLaptopChanged;
        base.Init(ui);
    }

    // LAPTOP CHANGED
    private void HandleLaptopChanged(List<Item> items)
    {
        if (items.Count > 0 && items[0].Reference == "hardware:laptop")
        {
            HasLaptop = true;
            // we enable all the ui_module
            foreach (UI_Item ui_item in ui_items)
            {
                if (ui_item is not UI_Module module) { return; }
                module.Enable();
            }
            return;
        }
        HasLaptop = false;
        if (!Faded) { Fade(fade_in: false); }

        // we disable all the ui_module
        foreach (UI_Item ui_item in ui_items)
        {
            if (ui_item is not UI_Module module) { return; }
            module.Disable();
        }
    }


    // DROPPING OVERHEAD SLOTS
    public void DropOverheadSlots()
    {
        // we check if we have too many slots
        if (transform.childCount <= MaxSlots) { return; }

        // we first try to remove the empty slots
        DestroyEmptySlots();
        if (transform.childCount <= MaxSlots)
        {
            // if we are not scalable we create back some empty slots to match max slots
            if (!Scalable) { CreateEmptySlots(MaxSlots - transform.childCount); }
            return;
        }

        // we only have full slots, but we still have too many slots
        // so we drop the last slots items and remove their slots

        // todo do this bcz for now we only remove them

        for (int i = transform.childCount; i > MaxSlots; --i)
        {
            // we get the last slot
            Transform last_slot = transform.GetChild(i - 1);
            UI_Item ui_item = last_slot.GetComponent<UI_Item>();
            if (ui_item == null) { continue; }

            // we unstore the item
            if (ui_item.Quantity > 0)
            {
                Item item = ui_item.Item;
                ui_item.Unstore(item);
            }

            // we destroy the slot
            Destroy(last_slot.gameObject);
        }
    }

    // CREATE ITEM SLOT
    public override GameObject CreateItemSlot(Item item = null)
    {
        // we create the item
        GameObject ui_slot = bank.CreateUI_Module();
        ui_slot.transform.SetParent(transform);

        // reset the scale to 1
        ui_slot.transform.localScale = Vector3.one;

        // we change the layer of the slot to the same as the pool
        ui_slot.layer = gameObject.layer;

        UI_Item ui_item = ui_slot.GetComponent<UI_Item>();
        ui_item.Init();

        // we assign the item to the UI_Item
        if (item != null) { ui_item.Store(item); }
        else { ui_item.ClearUI(); }

        // we add the item to the list
        ui_items.Add(ui_item);

        if (debug) { Debug.Log($"(UI_ModulePool) created an ui_module with item {(item == null ? "null" : item.Reference)}"); }

        return ui_slot;
    }
}