using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI_ModulePool is a helper class to manage the module pool in the UI_Laptop
/// it directly inherits from UI_ItemPool
/// and is used to manage the modules in the laptop
/// </summary>
public class UI_ModulePool : UI_ItemPool
{
    private MotherboardBuilder mb => GetComponentInParent<MotherboardBuilder>(includeInactive: true);

    // ENABLING / DISABLING
    public void EnableModules()
    {
        // we enable all the ui_module
        foreach (UI_Item ui_item in ui_items)
        {
            if (ui_item is not UI_Module module) { continue; }
            module.Enable();
        }
    }
    public void DisableModules()
    {
        // we disable all the ui_module
        foreach (UI_Item ui_item in ui_items)
        {
            if (ui_item is not UI_Module module) { continue; }
            module.Disable();
        }
    }

    // DROPPING OVERHEAD SLOTS
    public void DropOverheadSlots()
    {
        if (log) { Debug.Log($"(UI_ModulePool) dropping overhead slots, current count: {ui_items.Count}"); }

        // we check if we have too many slots
        if (Count <= MaxSlots) { return; }

        // we only have full slots, but we still have too many slots
        // so we drop the last slots items and remove their slots
        // normally we are in this method only when we uninstalled a HDD module
        // so LaptopInventory already cleared the lasts slots for us so it's ok we can destroy them
        // they should be empty
        int full_slots_dropped = 0;
        while (Count > MaxSlots)
        {
            // we get the last slot
            UI_Item ui_item = ui_items[Count - 1];

            // we unstore the item
            if (ui_item.Quantity > 0)
            {
                full_slots_dropped++;
                Item item = ui_item.Item;
                ui_item.Unstore(item);
            }

            // we destroy the slot
            Destroy(ui_item.gameObject);
            ui_items.Remove(ui_item);
        }

        if (log) { Debug.Log($"(UI_ModulePool) dropped last slots ({full_slots_dropped} non-empty) and now we have {ui_items.Count}"); }
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

        // change the image color of the ui_module
        Image image = ui_slot.GetComponent<Image>();
        if (image != null) { image.color = mb?.Color ?? Color.white; }

        UI_Item ui_item = ui_slot.GetComponent<UI_Item>();
        ui_item.Init();

        // we assign the item to the UI_Item
        if (item != null) { ui_item.Store(item); }
        else { ui_item.Clear(); }

        // we add the item to the list
        ui_items.Add(ui_item);

        if (log) { Debug.Log($"(UI_ModulePool) created an ui_module with item {(item == null ? "null" : item.Reference)}"); }

        return ui_slot;
    }

    // LAPTOP INVENTORY MANAGEMENT
    private LaptopInventory laptop_inventory;
    public void InitFromInventory(LaptopInventory inventory)
    {
        for (int i = 0; i < ui_items.Count; i++)
        {
            if (ui_items[i] == null || ui_items[i] is not UI_Module module) { continue; }

            // we switch the items
            module.SwitchItems(inventory.GetItemsInSlot(i), items_moved: false);
        }

        laptop_inventory = inventory;
    }
    public void OnModuleMoved(UI_Module module)
    {
        int new_slot_index = ui_items.IndexOf(module);
        laptop_inventory.HandleUI_ModuleMoved(module.GetItems(), new_slot_index);
    }
}