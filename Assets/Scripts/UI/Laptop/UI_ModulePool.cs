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
        if (debug) { Debug.Log($"(UI_ModulePool) dropping overhead slots, current count: {ui_items.Count}"); }

        // we check if we have too many slots
        if (Count <= MaxSlots) { return; }

        // we first try to remove the empty slots
        DestroyEmptySlots();
        if (debug) { Debug.Log($"(UI_ModulePool) tried destroying empty slots first, remaining count: {ui_items.Count}"); }
        if (Count <= MaxSlots)
        {
            // if we are not scalable we create back some empty slots to match max slots
            if (!Scalable) { CreateEmptySlots(MaxSlots - Count); }
            if (debug) { Debug.Log($"(UI_ModulePool) recreated some to match {MaxSlots} : have now {ui_items.Count}"); }
            return;
        }

        // we only have full slots, but we still have too many slots
        // so we drop the last slots items and remove their slots
        // todo do this bcz for now we only remove them brutally -> will make ui_items disappear in the limbs of hell ig ?
        while (Count > MaxSlots)
        {
            // we get the last slot
            UI_Item ui_item = ui_items[Count - 1];

            // we unstore the item
            if (ui_item.Quantity > 0)
            {
                Item item = ui_item.Item;
                ui_item.Unstore(item);
            }

            // we destroy the slot
            Destroy(ui_item.gameObject);
            ui_items.Remove(ui_item);
        }

        if (debug) { Debug.Log($"(UI_ModulePool) still too many slots, dropped full items and now we have {ui_items.Count}"); }
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
        else { ui_item.Clear(); }

        // we add the item to the list
        ui_items.Add(ui_item);

        if (debug) { Debug.Log($"(UI_ModulePool) created an ui_module with item {(item == null ? "null" : item.Reference)}"); }

        return ui_slot;
    }

    // INIT FROM INVENTORY
    public void InitFromInventory(LaptopInventory inventory)
    {
        for (int i = 0; i < ui_items.Count; i++)
        {
            if (ui_items[i] == null || ui_items[i] is not UI_Module module) { continue; }

            // we switch the items
            module.SwitchItems(inventory.GetItemsInSlot(i));
        }
    }
}