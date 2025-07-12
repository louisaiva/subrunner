using System;
using System.Collections;
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

    [Header("Item Rule")]
    public string item_rule = ""; // the rule to check if the item is valid

    [Header("Components")]
    [SerializeField] protected ItemBank bank;
    public Description Descriptor; // the description of the item pool

    [Header("Logs")]
    [SerializeField] protected bool debug = false;

    public virtual void Init()
    {
        // we get the item bank
        bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();

        // we destroy the existing slots
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // we check if we are scalable or not
        if (!Scalable)
        {
            // we create the slots
            for (int i = 0; i < MaxSlots; i++)
            {
                // we create the item
                GameObject ui_slot = bank.CreateUI_Item(null);
                ui_slot.transform.SetParent(transform);

                // reset the scale to 1
                ui_slot.transform.localScale = Vector3.one;

                // we change the layer of the slot to the same as the pool
                ui_slot.layer = gameObject.layer;
            }
        }
    }

    // RULE CHECK
    public bool CanStore(Item item)
    {
        // we check if the item is valid
        if (item == null) { return false; }

        return item.ValidateRule(item_rule);
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
        foreach (Transform slot in transform)
        {
            // we get the slot
            UI_Item ui_item = slot.GetComponent<UI_Item>();
            if (ui_item == null) { continue; }

            // we check if the slot can take the item
            bool stored = ui_item.Store(item);
            if (stored) { return true; }
        }

        // if we are here, we didn't find a slot to stack the item
        if (!Scalable) { return false; }

        // if we are here, we have a scalable inventory
        // we add a new slot to the pool
        GameObject ui_slot = bank.CreateUI_Item(item);
        ui_slot.transform.SetParent(transform);
        
        // reset the scale to 1
        ui_slot.transform.localScale = Vector3.one;

        // we change the layer of the slot to the same as the pool
        ui_slot.layer = gameObject.layer;

        return true;
    }
    public bool Drop(Item item)
    {
        // we check if we can remove the item
        if (item == null) { return false; }

        // we go through the children to find the item
        foreach (Transform slot in transform)
        {
            // we get the slot
            UI_Item ui_item = slot.GetComponent<UI_Item>();
            if (ui_item == null) { continue; }

            // we try to unstore the item
            bool unstored = ui_item.Unstore(item);
            if (unstored)
            {
                // we successfully unstored the item !!
                // we check if the slot is empty & we are scalable
                if (ui_item.Quantity == 0 && Scalable)
                {
                    // we destroy the item
                    Destroy(slot.gameObject);
                }
                return true;
            }
        }

        return false;
    }
}