using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : Capable, Interactable, Openable
{
    // OPENABLE
    public bool is_open { get; set; }
    public bool is_moving { get; set; }

    // INTERACTABLE
    public InteractCapacity Interactor { get; set; } // there is only ONE because it's the one that is Controlled
    public bool AuthorizeEndlessInteraction => false;
    [SerializeField] private List<Capable> interactors = new List<Capable>(); // store all interactors, not just the one controlled

    // START
    protected virtual void Start()
    {
        is_open = false;
        is_moving = false;

        // we subscribe to the hover events
        GetCapacity<HoverCapacity>().OnHoverLost += OnHoverLost;
    }

    // ON INTERACT / HOVER LOST
    public void OnInteract(Capable interactor)
    {
        // first interaction
        if (!interactors.Contains(interactor)) { interactors.Add(interactor); }
        if (debug) { Debug.Log("(Chest) " + name + " was interacted by " + interactor.name); }

        // we open if it's the first interactor we have !!
        if (interactors.Count == 1) { GetCapacity<OpenCapacity>().Use(interactor); }

        // only if the interactor is controlled
        if (interactor == PersoInputsController.Instance.Capable && !ui_inventory_shown)
        {
            // we set the interactor
            Interactor = interactor.GetCapacity<InteractCapacity>();

            // we show the inventory UI
            ShowUI_Inventory();
        }
    }
    public void OnHoverLost(Capable interactor)
    {
        if (!interactors.Contains(interactor)) { return; }
        interactors.Remove(interactor);

        // if there is no more interactor we close the chest
        if (interactors.Count == 0) { GetCapacity<CloseCapacity>().Use(interactor); }

        // if there is no more controlled interactors we hide the ui inventory
        if (!ui_inventory_shown) { return; }
        foreach (Capable c in interactors)
        {
            if (c == PersoInputsController.Instance.Capable) { return; } // we still have the controlled interactor so we dont hide the ui
        }

        // we hide the inventory UI
        HideUI_Inventory();
        Interactor = null; // we reset the interactor
    }


    // UI INVENTORY SHOWING / HIDING
    private bool ui_inventory_shown = false;
    private void ShowUI_Inventory()
    {
        if (Inventory == null || Inventory.ui == null) { return; }
        Inventory.ui.Show();
        (UI_Manager.Instance.GetPool("hud") as UI_HUD).RegisterChest(Inventory.ui);

        ui_inventory_shown = true;
    }
    private void HideUI_Inventory()
    {
        if (Inventory == null || Inventory.ui == null) { return; }
        Inventory.ui.Hide();
        (UI_Manager.Instance.GetPool("hud") as UI_HUD).RemoveChest(Inventory.ui);

        ui_inventory_shown = false;
    }

}