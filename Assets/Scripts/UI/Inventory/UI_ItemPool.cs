using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI_ItemPool is a helper class to manage the item pool in the UI.
/// It can have a rule reference to filter the items that can be added to the pool.
/// It is a smaller pool of item inside a bigger UI_Pool (mainly UI_InventoryMenu)
/// </summary>
public class UI_ItemPool : MonoBehaviour
{
    [Header("Item Pool Parameters")]
    public int MaxSlots = 9; // the maximum number of slots in the pool
    public bool Scalable = false; // if true, the pool will dynamically add/remove slots
    [SerializeField] protected List<UI_Item> ui_items = new List<UI_Item>();
    public int Count { get { return ui_items.Count; } }

    [Header("Item Rule")]
    public string item_rule = ""; // the rule to check if the item is valid

    [Header("Components")]
    [SerializeField] protected ItemBank bank;
    public Description Descriptor; // the description of the item pool
    public UI_Inventory UI_Inventory;

    [Header("Logs")]
    [SerializeField] protected bool debug = false;

    public virtual void Init(UI_Inventory ui)
    {
        // we set the UI_Inventory
        this.UI_Inventory = ui;

        // we get the item bank
        bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();

        // we add all existing uis to ui_items
        foreach (Transform child in transform)
        {
            // we check if the child is an empty slot
            UI_Item ui_item = child.GetComponent<UI_Item>();
            if (ui_item == null) { continue; }

            // we init the slot
            ui_item.Init();
            ui_items.Add(ui_item);
        }
        int awake_slots = Count;

        // we destroy the existing empty slots & init the others
        DestroyEmptySlots();
        int remaining_slots = Count;
        // await System.Threading.Tasks.Task.Yield(); // we wait for a frame
        
        // we check if we are scalable or not
        if (!Scalable) { CreateEmptySlots(this.MaxSlots - Count); }
        if (debug) { Debug.Log($"(UI_ItemPool) {name} just finished Init(), destroyed {awake_slots - remaining_slots} empty children and kept "
                + $"{remaining_slots} then recreated {Count - remaining_slots} empty ones");}
    }

    // RULE CHECK
    public bool CanStore(Item item)
    {
        // we check if the item is valid
        if (item == null) { return false; }
        bool validate = item.ValidateRule(item_rule);
        if (!validate && debug)
        {
            Debug.LogWarning($"(UI_ItemPool) {name} can't store item {item.Reference} because it doesn't match the rule {item_rule}");
        }
        else if (debug)
        {
            Debug.Log($"(UI_ItemPool) {name} can store item {item.Reference} because it matches the rule {item_rule}");
        }

        return validate;
    }


    // GRAB / DROP
    public bool Grab(Item item)
    {
        // we check if we can add the item
        if (!CanStore(item))
        {
            if (debug) { Debug.Log("(UI_ItemPool) item " + item.Reference + " is not valid for this pool"); }
            return false;
        }

        // we try to store the item in the existing slots
        foreach (UI_Item ui_item in ui_items)
        {
            // we check if the slot can take the item
            bool stored = ui_item.Store(item);
            if (stored) { return true; }
        }

        // if we are here, we didn't find a slot to stack the item
        if (!Scalable)
        {
            if (debug) { Debug.Log("(UI_ItemPool) item " + item.Reference
            + $" is valid for this pool but no slot to store it found :// ({Count} slots currently in the pool)"); }
            return false;
        }

        // if we are here, we have a scalable inventory
        CreateItemSlot(item);
        return true;
    }
    public bool Drop(Item item)
    {
        // we check if we can remove the item
        if (item == null) { return false; }

        // we go through the children to find the item
        foreach (UI_Item ui_item in ui_items)
        {
            // we try to unstore the item
            bool unstored = ui_item.Unstore(item);
            if (unstored)
            {
                // we successfully unstored the item !!
                // we check if the slot is empty & we are scalable
                if (ui_item.Quantity == 0 && Scalable)
                {
                    // we destroy the item
                    Destroy(ui_item.gameObject);
                    ui_items.Remove(ui_item);
                }
                return true;
            }
        }

        return false;
    }

    // DESTROY / CREATE EMPTY ITEM SLOT
    public void DestroyEmptySlots()
    {
        // we go through the children to find the empty slots
        int i = 0;
        while (i < Count)
        {
            UI_Item ui_item = ui_items[i];
            if (ui_item.Quantity == 0)
            {
                // we destroy the empty slot
                Destroy(ui_item.gameObject);
                ui_items.RemoveAt(i);
                continue; // we don't increment i, we just remove the empty slot
            }

            i++;
        }
    }
    public void CreateEmptySlots(int count)
    {
        // we create the empty slots
        for (int i = 0; i < count; i++) { CreateItemSlot(); }
        if (debug) { Debug.Log($"(UI_ItemPool) created {count} empty slots in {name}"); }
    }
    public virtual GameObject CreateItemSlot(Item item = null)
    {
        // we create the item
        GameObject ui_slot = bank.CreateUI_Item();
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

        return ui_slot;
    }
}