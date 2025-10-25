
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// InteractCapacity is a capacity that allows a Capable to interact with Interactable objects.
/// When approaching an Interactable, the capacity make the object play hover.
/// And when pressing the Interact Button, the PIC (PersoInputController) will call the this.Interact() method
/// which will trigger Interactable.OnInteract() method (or Interactable.OnEndlessInteract())
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class InteractCapacity : Capacity
{

    public bool log_triggers = false;

    [Header("Current Hover")]
    [SerializeField] private HoverCapacity closest_hover;
    public Interactable interactable
    {
        get
        {
            if (closest_hover == null) { return null; }
            if (closest_hover.capable == null) { return null; }
            if (closest_hover.capable is Interactable interactable) { return interactable; }
            return null;
        }
    }
    public HoverCapacity CurrentHover { get { return closest_hover; } }

    [Header("Waiting hovers")]
    [SerializeField] private List<HoverCapacity> waiting_hovers = new List<HoverCapacity>();

    [Header("Interaction rules")]
    // [SerializeField] private GrabCapacity grab_capacity;
    [SerializeField] private List<InteractType> interact_types = new List<InteractType>() {};
    [SerializeField] private string item_rule = ""; // rule to check if an item is interactable with us
    public string ItemRule { get => item_rule; }

    // UPDATE
    protected override void Update()
    {
        base.Update();

        // we remove nulls and items that are grabbed
        for (int i = waiting_hovers.Count - 1; i >= 0; i--)
        {
            HoverCapacity hover = waiting_hovers[i];
            if (hover == null || hover.capable == null)
            {
                waiting_hovers.RemoveAt(i);
                continue;
            }
            if (hover.capable is Item item && item.Grabbed)
            {
                waiting_hovers.RemoveAt(i);
                continue;
            }
        }

        // we check if we have a something in the waiting hovers
        if (waiting_hovers.Count == 0) { return; }

        // we update the waiting hovers by distance
        waiting_hovers.Sort((a, b) => Vector2.Distance(a.transform.position, capable.transform.position).CompareTo(Vector2.Distance(b.transform.position, capable.transform.position)));

        // we check if we have a current hover
        if (closest_hover == null)
        {
            // we select the hover of the first waiting hover
            select_hover(waiting_hovers[0]);
            waiting_hovers.RemoveAt(0);

            return;
        }

        // we check if the current hover is still the closest
        if (Vector2.Distance(closest_hover.transform.position, capable.transform.position)
            <= Vector2.Distance(waiting_hovers[0].transform.position, capable.transform.position)) { return; }

        // we switch the current hover
        waiting_hovers.Add(closest_hover);
        unselect_hover();
        select_hover(waiting_hovers[0]);
        waiting_hovers.RemoveAt(0);
    }

    // INTERACTABLE SELECTION
    public Action<Capable> OnHoverSelect;
    public Action<Capable> OnHoverDeselect;
    private void select_hover(HoverCapacity hover)
    {
        // we switch the current hover
        if (closest_hover != null) { unselect_hover(); }
        closest_hover = hover;
        if (debug) { Debug.Log("(InteractCapacity) " + hover.capable.name + " selected as closest hover"); }

        // we play the hover animation
        closest_hover.Hover(this.capable);

        // we invoke the callback
        OnHoverSelect?.Invoke(closest_hover.capable);
    }
    private void unselect_hover()
    {
        // we chack if we have a current hover
        if (closest_hover == null) { return; }

        // we stop the hover animation
        closest_hover.Unhover(this.capable);

        // we deselect the grabbing if it's an item
        Capable interactive = closest_hover.capable;

        // we reset the current hover
        if (debug) { Debug.Log("(InteractCapacity) " + interactive.name + " unselected as closest hover"); }
        closest_hover = null;

        // we invoke the callback
        OnHoverDeselect?.Invoke(interactive);
    }

    // HANDLE INTERACT INPUT
    public void Interact(bool endless = false)
    {
        if (closest_hover == null) { return; }
        Capable interactive = closest_hover.capable;

        // interact with interactable & select + grab items
        if (interactive is Interactable interactable && !endless) { interactable.OnInteract(capable); }
        else if (interactive is EndlessInteractable interactable_endless && endless) { interactable_endless.OnEndlessInteract(capable); }
    }

    // TRIGGER ENTER
    private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if the other has a HoverCapacity
        HoverCapacity hover = other.GetComponent<HoverCapacity>();
        if (hover == null) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " hovered " + other.name + " but it has no HoverCapacity"); } return; }

        // we get the capable of the hover capacity
        Capable interacted_capable = hover.capable;
        if (interacted_capable == null) { return; }

        // we check if it's an Interactable or an Item
        if (interacted_capable is not Interactable interactive) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " hovered " + interacted_capable.name + " but it is no Interactable"); } return; }
        if (!interact_types.Contains(interactive.InteractionType)) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " hovered " + interactive.name + " but its type is not allowed"); } return; }
        if (interactive is Item item && !item.ValidateRule(ItemRule)) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " hovered " + interactive.name + " but it is excluded by the rule"); } return; } // we check if the item is excluded by the rule

        // we check if the capable is already hovered
        if (hover == closest_hover) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " hovered " + interactive.name + " but it is already hovered"); } return; }

        // or if it's already in the waiting hovers
        if (waiting_hovers.Contains(hover)) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " hovered " + interactive.name + " but it is already in the waiting hovers"); } return; }

        // we add the capable to the waiting hovers
        waiting_hovers.Add(hover);

        if (debug) { Debug.Log("(InteractCapacity) " + interactive.name + " added to waiting hovers"); }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // we check if the other has a HoverCapacity
        HoverCapacity hover = other.GetComponent<HoverCapacity>();
        if (hover == null) { return; }

        // we get the capable of the hover capacity
        Capable interactive = hover.capable;
        if (interactive == null) { return; }

        // we check if it's an Interactable
        if (interactive is not Interactable) { return; }

        // we check if hover is the current hover
        if (hover == closest_hover)
        {
            unselect_hover();
            return;
        }

        // we check if the hover is in the waiting hovers
        if (waiting_hovers.Contains(hover))
        {
            waiting_hovers.Remove(hover);
            if (debug) { Debug.Log("(InteractCapacity) " + interactive.name + " removed from waiting hovers"); }
        }
    }
}

public enum InteractType
{
    Other,
    Chest,
    Device,
    Door,
    Kitchen,
    Spawner,
    Corpse,
    Item
}