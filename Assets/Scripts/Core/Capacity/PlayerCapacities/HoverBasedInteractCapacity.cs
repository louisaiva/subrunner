
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HoverBasedInteractCapacity is a specific InteractCapacity that allows a Capable to interact with Interactable objects through
/// their hover & Collider's OnTriggerMethods
/// When approaching an Interactable, the capacity make the object play hover.
/// And when pressing the Interact Button, the PIC (PersoInputController) will call the this.Interact() method
/// which will trigger Interactable.OnInteract() method (or Interactable.OnEndlessInteract())
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class HoverBasedInteractCapacity : InteractCapacity
{

    [Header("Current Hover")]
    [SerializeField] private HoverCapacity closest_hover;
    public Interactable interactable
    {
        get
        {
            if (closest_hover == null) { return null; }
            if (closest_hover.Capable == null) { return null; }
            if (closest_hover.Capable is Interactable interactable) { return interactable; }
            return null;
        }
    }
    public HoverCapacity CurrentHover { get { return closest_hover; } }

    [Header("Waiting hovers")]
    [SerializeField] private List<HoverCapacity> waiting_hovers = new List<HoverCapacity>();
    
    [Header("Logs")]
    public bool log_triggers = false;

    private Collider2D _interact_collider = null;
    public Collider2D InteractCollider
    {
        get
        {
            if (_interact_collider != null) { return _interact_collider; }
            _interact_collider = GetComponentInChildren<Collider2D>(includeInactive:true);
            return _interact_collider;
        }
    }

    ///
    //
    /// INTERACT METHOD + SELECTION GESTION
    //
    ///

    // HANDLE INTERACT INPUT
    public void Interact(bool endless = false)
    {
        // check if we are on a sofa // todo : or a container
        if (TryGetSiblingCapacity(out SitCapacity sit_capacity) && sit_capacity.CurrentSofa != null)
        {
            sit_capacity.ExitSofa();
            return;
        }


        if (closest_hover == null) { return; }
        Capable capable = closest_hover.Capable;
        if (capable is not Interactable interactable) { return; }
        InteractWithInteractable(interactable, endless);
    }

    // INTERACTABLE SELECTION
    public Action<Capable> OnHoverSelect;
    public Action<Capable> OnHoverDeselect;
    private void select_hover(HoverCapacity hover)
    {
        // we switch the current hover
        if (closest_hover != null) { unselect_hover(); }
        closest_hover = hover;
        if (log) { Debug.Log("(HBInteractCapacity) " + hover.Capable.name + " selected as closest hover"); }

        // we play the hover animation
        closest_hover.Hover(this.Capable);

        // we invoke the callback
        OnHoverSelect?.Invoke(closest_hover.Capable);
    }
    private void unselect_hover()
    {
        // we chack if we have a current hover
        if (closest_hover == null) { return; }

        // we stop the hover animation
        closest_hover.Unhover(this.Capable);

        // we deselect the grabbing if it's an item
        Capable interactive = closest_hover.Capable;

        // we reset the current hover
        if (log) { Debug.Log("(HBInteractCapacity) " + ((interactive != null) ? interactive.name : "") + " unselected as closest hover"); }
        closest_hover = null;

        // we invoke the callback
        if (interactive == null) { return; }
        OnHoverDeselect?.Invoke(interactive);
    }

    // UPDATE
    protected void Update()
    {
        // we remove nulls and items that are grabbed
        for (int i = waiting_hovers.Count - 1; i >= 0; i--)
        {
            HoverCapacity hover = waiting_hovers[i];
            if (hover == null || hover.Capable == null)
            {
                waiting_hovers.RemoveAt(i);
                continue;
            }
            if (hover.Capable is Item item && item.Grabbed)
            {
                waiting_hovers.RemoveAt(i);
                continue;
            }
        }

        // we check if we have a something in the waiting hovers
        if (waiting_hovers.Count == 0) { return; }

        // we update the waiting hovers by distance
        waiting_hovers.Sort((a, b) => Vector2.Distance(a.transform.position, Capable.transform.position).CompareTo(Vector2.Distance(b.transform.position, Capable.transform.position)));

        // we check if we have a current hover
        if (closest_hover == null)
        {
            // we select the hover of the first waiting hover
            select_hover(waiting_hovers[0]);
            waiting_hovers.RemoveAt(0);

            return;
        }

        // we check if the current hover is still the closest
        if (Vector2.Distance(closest_hover.transform.position, Capable.transform.position)
            <= Vector2.Distance(waiting_hovers[0].transform.position, Capable.transform.position)) { return; }

        // we switch the current hover
        waiting_hovers.Add(closest_hover);
        unselect_hover();
        select_hover(waiting_hovers[0]);
        waiting_hovers.RemoveAt(0);
    }


    // INFORM HOVER LOST
    public void HoverLostItself(HoverCapacity hover)
    {
        if (log || log_triggers) { Debug.Log($"(HBInteractCapacity) {hover.name} told us it wants to lose itself, so we do !"); }
        remove_from_waiting_list(hover);
    }




    ///
    //
    /// WAITING LIST GESTION
    //
    ///

    // TRIGGER ENTER / EXIT
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (log_triggers) { Debug.Log($"(HBInteractCapacity) {other.name} JUST ENTER"); }

        // verify if we can interact with it
        if (!CanInteractWithCollider(other, out HoverCapacity hover)) { return; }
        add_to_waiting_list(hover);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (log_triggers) { Debug.Log($"(HBInteractCapacity) {other.name} JUST EXIT"); }

        // we check if the other has a HoverCapacity
        HoverCapacity hover = other.transform.parent.GetComponent<HoverCapacity>();
        if (hover == null) { if (log_triggers) { Debug.Log("(HBInteractCapacity) " + name + " exits hovered " + other.name + " but it has no HoverCapacity"); } return; }

        // we get the capable of the hover capacity
        Capable interactive = hover.Capable;
        if (interactive == null) { if (log_triggers) { Debug.Log("(HBInteractCapacity) " + name + " exits hovered " + hover.name + " but it has no Capable"); } return; }

        // we check if it's an Interactable
        if (interactive is not Interactable) { if (log_triggers) { Debug.Log("(HBInteractCapacity) " + name + " exits hovered " + interactive.name + " but it is no Interactable"); } return; }

        remove_from_waiting_list(hover);
    }

    // INTERACTION CHECKS
    private bool CanInteractWithCollider(Collider2D other, out HoverCapacity hover)
    {
        // we check if the other has a HoverCapacity
        hover = other.transform.parent.GetComponent<HoverCapacity>();
        if (hover == null) { if (log_triggers) { Debug.Log("(HBInteractCapacity) " + ID + " hovered " + other.name + " but it has no HoverCapacity"); } return false; }

        // we get the capable of the hover capacity
        Capable interacted_capable = hover.Capable;
        if (interacted_capable == null) { if (log_triggers) { Debug.Log("(HBInteractCapacity) " + ID + " hovered " + other.name + " but it has no capable"); } return false; }

        // we check if we can interact with it
        if (!check_interaction_requirements(interacted_capable)) { if (log_triggers) { Debug.Log("(HBInteractCapacity) " + ID + " hovered " + other.name + " but it can't be interacted by us"); } return false; }
        if (!check_if_in_same_room(interacted_capable)) { if (log_triggers) { Debug.Log("(HBInteractCapacity) " + ID + " hovered " + other.name + " but we are not in the same room"); } return false; }
        return true;
    }
    private bool check_interaction_requirements(Capable interacted_capable)
    {
        if (interacted_capable is not Interactable interactive) { if (log_triggers) { Debug.Log("(HBInteractCapacity) " + name + " hovered " + interacted_capable.name + " but it is no Interactable"); } return false; }
        if (!interact_types.Contains(interactive.InteractionType)) { if (log_triggers) { Debug.Log("(HBInteractCapacity) " + name + " hovered " + interactive.name + " but its type is not allowed"); } return false; }
        if (interactive is Item item && !item.ValidateRule(ItemRule)) { if (log_triggers) { Debug.Log("(HBInteractCapacity) " + name + " hovered " + interactive.name + " but it is excluded by the rule"); } return false; }
        return true;
    }
    private bool check_if_in_same_room(Capable interacted_capable)
    {
        // check that room of Capable & room of Interactable are the same
        // this is here and not upper because it can be a heavy call. AND NOT ON DOOR BC WE WANT TO BE ABLE TO ALWAYS INTERACT WITH THEM
        if (interacted_capable is Door) { return true; }

        // also, if at least one the rooms can't be find, it means the thing was just unfreed,
        // so we consider we can interact with anything (maybe we just dropped an item or quit a sofa)
        string room_of_interactable = interacted_capable.GetRealRoom();
        if (string.IsNullOrEmpty(room_of_interactable) || room_of_interactable == "item_was_just_dropped") { return true; }
        string room_of_capable = Capable.GetRealRoom();
        if (string.IsNullOrEmpty(room_of_capable)) { return true; }

        // finally, if we have 2 valid rooms, we only allow interaction if we are in same rooms !
        if (room_of_capable == room_of_interactable) { return true; }
        if (log_triggers) { Debug.Log($"(HBInteractCapacity) '{Capable.ID}' (room : '{room_of_capable}') tried to interact with '{interacted_capable.ID}' (room : '{room_of_interactable}') but they are in different rooms"); }
        return false;
    }

    // ADD / REMOVE FROM WAITING LIST
    private void add_to_waiting_list(HoverCapacity hover)
    {
        // check if we are not already interacting with it somehow
        if (hover == closest_hover) { if (log_triggers) { Debug.Log("(HBInteractCapacity) " + ID + " hovered " + hover.Capable.ID + " but it is already hovered"); } return; }
        if (waiting_hovers.Contains(hover)) { if (log_triggers) { Debug.Log("(HBInteractCapacity) " + ID + " hovered " + hover.Capable.ID + " but it is already in the waiting hovers"); } return; }

        // we add the capable to the waiting hovers
        waiting_hovers.Add(hover);
        if (log || log_triggers) { Debug.Log("(HBInteractCapacity) " + hover.Capable.ID + " added to waiting hovers"); }
    }
    private void remove_from_waiting_list(HoverCapacity hover)
    {
        // the hover decided that it won't be hovered anymore...
        if (closest_hover != null && hover == closest_hover)
        {
            unselect_hover();
            if (log || log_triggers) { Debug.Log("(HBInteractCapacity) " + hover.Capable.ID + " removed from waiting hovers (it was the selected one actually)"); }
            return;
        }

        // check if it is inside the waitings hover
        if (!waiting_hovers.Contains(hover)) { return; }
        waiting_hovers.Remove(hover);
        if (log || log_triggers) { Debug.Log("(HBInteractCapacity) " + hover.Capable.ID + " removed from waiting hovers"); }
    }



    ///
    //
    /// HANDLE CAPABLE CHANGED ROOM
    //
    ///

    // HANDLE CAPABLE CHANGED ROOM
    private void handle_capable_changed_room()
    {
        if (log) { Debug.Log($"(HBIC) capable {Capable.ID} just changed room, refreshing waiting hovers ..."); }
        List<Collider2D> results = new List<Collider2D>();
        Physics2D.OverlapCollider(InteractCollider, results);

        foreach (Collider2D col in results)
        {
            if (!CanInteractWithCollider(col, out HoverCapacity hover))
            {
                remove_from_waiting_list(hover);
                continue;
            }

            add_to_waiting_list(hover);
        }

        // and that's it ! next update will re sort everything nicely (sort non null waiting hovers by distance) and select the closest hover !
    }


    // LOAD / UNLOAD DATA (only useful for the callback)
    private Action<CapableData> room_changed_callback = null;
    public override void LoadData(CapacityData data, CapableData capable_data)
    {
        base.LoadData(data, capable_data);
        room_changed_callback = (cd) => handle_capable_changed_room();
        capable_data.OnRoomChanged += room_changed_callback;
    }
    public override void UnloadData()
    {
        Capable.data.OnRoomChanged -= room_changed_callback;
        room_changed_callback = null;
        base.UnloadData();
    }

}