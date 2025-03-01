using UnityEngine;
using UnityEngine.InputSystem;
using System;

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

    [Header("Input & Callbacks")]
    [SerializeField] private InputActionReference grabInput;
    private InputAction grabAction;
    private event Action<InputAction.CallbackContext> grabCallback;

    // START
    private void Start()
    {
        // on récupère la bank
        bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();

        // on récupère l'action grab
        grabAction = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().GetAction(grabInput);

        // on définit le callback
        grabCallback = ctx => Use(capable);


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


    // SELECT / DESELECT
    public void Select(Item item)
    {
        selected_item = item;

        // we set the callback
        grabAction.performed += grabCallback;

        if (debug) { Debug.Log("(GrabCapacity) selected (and callback set) : " + item.name); }
    }
    public void Deselect()
    {
        if (selected_item == null) { return; }

        if (debug) { Debug.Log("(GrabCapacity) deselected (and callback removed) : " + selected_item.name); }

        selected_item = null;

        // we remove the callback
        grabAction.performed -= grabCallback;

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

        // we set the item to grabbed (which disables the hover collider)
        item.Grabbed = true;

        // we move the item to the parent of the capable
        item.transform.SetParent(items_parent);

        // we create the ui_item if we have a ui_inventory
        ui_inventory?.Grab(item);

        if (debug) { Debug.Log("(GrabCapacity) grabbed : " + item.name); }
    }
}