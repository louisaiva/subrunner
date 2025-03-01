using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GrabCapacity is a Capacity that allows the Capable to grab items.
/// </summary>

public class GrabCapacity : Capacity
{
    public override bool Able
    {
        get
        {
            // checks if we have a selected item
            if (selected_item == null) { return false; }
            return true;
        }
    }

    [Header("Items")]
    [SerializeField] private Item selected_item;
    [SerializeField] private Transform items_parent;
    public UI_Inventory ui_inventory;

    [Header("Bank")] [SerializeField] private ItemBank bank;

    // [Header("UI Grab Callback")]
    // public event System.Action grab_callback;

    // START
    private void Start()
    {
        // on récupère la bank
        bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();

        // on récupère le parent des items
        items_parent = capable.transform.Find("inventory");

        // on récupère les items
        foreach (Transform child in items_parent)
        {
            Item item = child.GetComponent<Item>();
            if (item != null)
            {
                grab(item);
            }
        }
    }

    // SELECT
    public void Select(Item item = null)
    {
        selected_item = item;
    }

    // USE
    public override void Use(Capable capable)
    {
        // we check if we have a selected item
        if (selected_item == null)
        {
            if (debug) { Debug.LogError("(GrabCapacity) no selected item"); }
            return;
        }

        // we grab
        grab(selected_item);
    }

    
    // GRAB / DROP
    protected void grab(Item item)
    {
        if (item == null) { return; }

        // we set the item to grabbed
        item.Grabbed = true;
        // todo which way is better ??
        // we deactivate the hover capacity of the item
        // item.transform.Find("hover").gameObject?.SetActive(false);


        // we move the item to the parent of the capable
        item.transform.SetParent(items_parent);

        // we create the ui_item if we have a ui_inventory
        if (ui_inventory != null)
        {
            ui_inventory.Grab(item);
        }

        if (debug) { Debug.Log("(GrabCapacity) grabbed : " + item.name); }
    }
}