using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Chest : Capable, Interactable, Openable
{
    public bool log_interact_kf = false;

    [Header("Openable")]
    public bool is_open { get; set; }
    public bool is_moving { get; set; }

    [Header("Interactable")]
    [SerializeField] private List<Capable> interactors = new List<Capable>(); // store all interactors, not just the one controlled
    public InteractCapacity Interactor { get; set; } // there is only ONE because it's the one that is Controlled
    public bool AuthorizeEndlessInteraction => false;

    [Header("Interact Key Feedback")]
    private Transform interact_kf;
    private Vector2 initial_kf_position;
    [SerializeField] private Vector2 ui_opened_kf_position = Vector2.zero;

    // START
    protected virtual void Start()
    {
        is_open = false;
        is_moving = false;

        // we subscribe to the hover events
        HoverCapacity hover_capacity = GetCapacity<HoverCapacity>();
        hover_capacity.OnHoverLost += OnHoverLost;
        interact_kf = hover_capacity.Canvas_kf;
        if (interact_kf != null) { initial_kf_position = interact_kf.localPosition; }
    }

    // ON INTERACT / HOVER LOST
    public void OnInteract(Capable interactor)
    {
        // only first interaction per interactor is authorized !!!
        if (interactors.Contains(interactor)) { return; }
        interactors.Add(interactor);
        if (debug) { Debug.Log("(Chest) " + name + " was interacted by " + interactor.name); }

        // we open if it's the first interactor we have !!
        if (interactors.Count == 1) { GetCapacity<OpenCapacity>().Use(interactor); }

        // only if the interactor is controlled
        if (interactor == Controller.Instance.Capable && !ui_inventory_shown)
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
            if (c == Controller.Instance.Capable) { return; } // we still have the controlled interactor so we dont hide the ui
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
        (UI_Manager.Instance.GetPool("hud") as UI_HUD).RegisterChest(Inventory.ui);
        ui_inventory_shown = true;

        // we move the interact key feedback if we have one
        if (interact_kf == null) { return; }
        interact_kf.localPosition = calculate_best_kf_position();
    }
    private void HideUI_Inventory()
    {
        if (Inventory == null || Inventory.ui == null) { return; }
        (UI_Manager.Instance.GetPool("hud") as UI_HUD).RemoveChest(Inventory.ui);
        ui_inventory_shown = false;

        // we move back the interact key feedback if we have one
        if (interact_kf == null) { return; }
        interact_kf.localPosition = initial_kf_position;
    }


    // INTERACT KEY FEEDBACK
    private Vector2 calculate_best_kf_position()
    {
        // we calculate the position we need to give the kf's canvas

        // 1 - we get the inventory's canvas
        Transform ui_canvas = Inventory.ui.transform.parent;
        Vector2 kf_position = ui_canvas.transform.localPosition;

        // 2 - we apply an offset to the right by the columns count of the grid/2
        LayoutRebuilder.ForceRebuildLayoutImmediate(Inventory.ui.transform as RectTransform);
        float preffered_width = LayoutUtility.GetPreferredWidth(Inventory.ui.transform as RectTransform) * Inventory.ui.transform.localScale.x / 2f;
        if (log_interact_kf) { Debug.Log("(Chest) " + name + " preffered width of the inventory is " + preffered_width); }
        kf_position.x += preffered_width / 2f;

        // 3 - center vertically the KF + little offset on the right
        kf_position += new Vector2(0.25f, 0.25f);
        return kf_position;
    }
}