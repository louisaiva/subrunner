using UnityEngine;
using System;


public class Pot : Item, Usable
{
    [Header("Components")]
    private InteractCapacity interactor = null;
    private event Action<Item> OnUsabilityChanged = delegate { };
    private UI_InventoryMenu inventory_menu = null;

    protected override void Awake()
    {
        base.Awake();
        interactor = GetCapacity<InteractCapacity>();
        interactor.OnHoverSelect += on_interactor_hover;
        interactor.OnHoverDeselect += on_interactor_unhover;
    }
    protected override void Start()
    {
        base.Start();
        inventory_menu = UI_Manager.Instance.GetPool("inventory") as UI_InventoryMenu;
    }

    // UPDATE LABEL & USABILITY
    private void on_interactor_hover(Capable interactable)
    {
        // we check if we are hovering an oven
        if (interactable is Oven oven)
        {
            UseLabel = "heat pot";
            usable_now = true;
            OnUsabilityChanged?.Invoke(this);
            return;
        }

        /* if (interactable is Sink)
        {
            UseLabel = "fill pot";
            usable_now = true;
            return;
        } */

        if (!usable_now) { return; }
        on_interactor_unhover(interactable);
    }
    private void on_interactor_unhover(Capable interactable)
    {
        // we reset the use label and usability
        UseLabel = "";
        usable_now = false;
        OnUsabilityChanged?.Invoke(this);
    }

    // USABLE
    public string UseLabel { get; set; } = "";
    public bool usable_now = false;
    public void Use(Capable user)
    {
        // we check if we can interact with something
        if (!usable_now) { return; }

        // we use the interactable
        interactor.Interact();
    }


    // on grabbed / dropped

    // BEING GRABBED / DROPPED
    protected override async void on_grabbed()
    {
        base.on_grabbed();

        // we subscribe to the InventoryMenu On
        if (Holder == Perso.Instance) { OnUsabilityChanged += inventory_menu.UpdateIF; }
    }
    protected override void on_dropped()
    {
        // we unsubscribe to the InventoryMenu On
        if (Holder == Perso.Instance) { OnUsabilityChanged -= inventory_menu.UpdateIF; }

        // we drop
        base.on_dropped();
    }
}