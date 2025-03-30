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

    [Header("Selection")]
    [SerializeField] private Item selected_item;

    [Header("Components")]
    [SerializeField] private ItemBank bank;
    [SerializeField] private Inventory inventory;

    [Header("Input & Callbacks")]
    [SerializeField] private InputActionReference dropInput;
    private InputAction dropAction;
    private event Action<InputAction.CallbackContext> dropCallback;

    // START
    private void Start()
    {
        // on récupère la bank
        bank = GameObject.Find("/utils/bank").GetComponent<ItemBank>();
        inventory = capable.inventory;

        // on récupère l'action drop
        dropAction = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().GetAction(dropInput);

        // on définit le callback
        dropCallback = ctx => Use(capable);
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


    public void SelectLastItem()
    {
        // we check if we have items
        if (inventory.Items.Count == 0) { return; }

        // we select the last item
        Select(inventory.Items[inventory.Items.Count - 1]);
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

        // we drop the item
        bool drop = inventory.Drop(selected_item);
        if (debug) { Debug.Log("(DropCapacity) " + capable.name + (drop ? " :D dropped" : " :/ could not drop") + " : " + selected_item.name); }
    }
}