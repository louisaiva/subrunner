using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// DropCapacity is a Capacity that allows the Capable to drop items.
/// </summary>

public class DropCapacity : Capacity
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
    [SerializeField] private Transform parent_to_drop_items;
    [SerializeField] private Transform inventory;
    public UI_Inventory ui_inventory;

    [Header("Drop")]
    [SerializeField] private float drop_magnitude = 50f;

    [Header("Bank")] [SerializeField] private ItemBank bank;

    [Header("Input & Callbacks")]
    [SerializeField] private InputActionReference dropInput;
    private InputAction dropAction;
    private event Action<InputAction.CallbackContext> dropCallback;

    // START
    private void Start()
    {
        // on récupère la bank
        bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();

        // on récupère l'action drop
        dropAction = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().GetAction(dropInput);

        // on définit le callback
        dropCallback = ctx => Use(capable);

        // on récupère le parent des items
        inventory = capable.transform.Find("inventory");

    }

    // UPDATE
    protected override void Update()
    {
        base.Update();

        // we check if we have a selected item
        if (selected_item != null) { return; }
        
        // we check if we have an item in the inventory
        if (inventory.childCount == 0) { return; }

        // we select the first item in the inventory
        Select(inventory.GetChild(0).GetComponent<Item>());
    }


    // SELECT / DESELECT
    public void Select(Item item)
    {
        selected_item = item;

        // we set the callback
        dropAction.performed += dropCallback;

        if (debug) { Debug.Log("(DropCapacity) selected (and callback set) : " + item.name); }
    }
    public void Deselect()
    {
        if (selected_item == null) { return; }

        if (debug) { Debug.Log("(DropCapacity) deselected (and callback removed) : " + selected_item.name); }

        selected_item = null;

        // we remove the callback
        dropAction.performed -= dropCallback;

    }

    // USE
    public override void Use(Capable capable)
    {
        // we check if we have a selected item
        if (selected_item == null)
        {
            if (debug) { Debug.LogError("(DropCapacity) no selected item"); }
            return;
        }

        // we drop
        drop(selected_item);
    }

    // GRAB / DROP
    protected void drop(Item item)
    {
        if (item == null) { return; }

        // we set the item to dropped (which disables the hover collider)
        item.Grabbed = false;

        // we move the item back to the world
        item.transform.position = capable.transform.position + ((Vector3) capable.Orientation * 0.2f);
        item.transform.SetParent(parent_to_drop_items);

        // we delete the ui_item if we have a ui_inventory
        ui_inventory?.Drop(item);

        // we deselect the item
        Deselect();

        // we add a force to the item
        Force force = new Force("drop", capable.Orientation , drop_magnitude);
        item.AddForce(force);

        if (debug) { Debug.Log("(DropCapacity) dropped : " + item.name); }
    }
}