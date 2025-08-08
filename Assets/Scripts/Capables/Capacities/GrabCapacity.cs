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

    [Header("Selection")]
    [SerializeField] private Item selected_item;

    [Header("Components")]
    [SerializeField] private ItemBank bank;
    [SerializeField] private Inventory inventory;

    [Header("Input & Callbacks")]
    [SerializeField] private InputActionReference grabInput;
    private InputAction grabAction;
    private event Action<InputAction.CallbackContext> grabCallback;

    // START
    private void Start()
    {
        // on récupère la bank
        bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();
        inventory = capable.Inventory;

        // on récupère l'action grab
        grabAction = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().GetAction(grabInput);

        // on définit le callback
        grabCallback = ctx =>
        {
            if (ctx.ReadValue<float>() > 0.5f) { return; } // we verify that the button was released
            Use(capable);
        };
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

        // we grab the item
        string item_name = selected_item.name;
        bool grab = inventory.Grab(selected_item);
        if (debug)
        {
            Debug.Log("(GrabCapacity) " + capable.name + (grab ? " :D grabbed" : " :/ could not grab") + " : " + item_name);
        }
    }

    private void OnDestroy()
    {
        // we remove the callback
        grabAction.performed -= grabCallback;
    }
}