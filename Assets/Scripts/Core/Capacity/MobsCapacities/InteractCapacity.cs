
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
            if (closest_hover.Capable == null) { return null; }
            if (closest_hover.Capable is Interactable interactable) { return interactable; }
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

    // INTERACTABLE SELECTION
    public Action<Capable> OnHoverSelect;
    public Action<Capable> OnHoverDeselect;
    private void select_hover(HoverCapacity hover)
    {
        // we switch the current hover
        if (closest_hover != null) { unselect_hover(); }
        closest_hover = hover;
        if (log) { Debug.Log("(InteractCapacity) " + hover.Capable.name + " selected as closest hover"); }

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
        if (log) { Debug.Log("(InteractCapacity) " + ((interactive != null) ? interactive.name : "") + " unselected as closest hover"); }
        closest_hover = null;

        // we invoke the callback
        if (interactive == null) { return; }
        OnHoverDeselect?.Invoke(interactive);
    }

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
        Capable interactive = closest_hover.Capable;

        // interact with interactable & select + grab items
        if (interactive is Interactable interactable && !endless) { interactable.OnInteract(Capable); }
        else if (interactive is EndlessInteractable interactable_endless && endless) { interactable_endless.OnEndlessInteract(Capable); }
    }

    // TRIGGER ENTER
    private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if the other has a HoverCapacity
        HoverCapacity hover = other.transform.parent.GetComponent<HoverCapacity>();
        if (hover == null) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " hovered " + other.name + " but it has no HoverCapacity"); } return; }

        // we get the capable of the hover capacity
        Capable interacted_capable = hover.Capable;
        if (interacted_capable == null) { return; }

        // we check if we can interact with it
        if (interacted_capable is not Interactable interactive) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " hovered " + interacted_capable.name + " but it is no Interactable"); } return; }
        if (!interact_types.Contains(interactive.InteractionType)) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " hovered " + interactive.name + " but its type is not allowed"); } return; }
        if (interactive is Item item && !item.ValidateRule(ItemRule)) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " hovered " + interactive.name + " but it is excluded by the rule"); } return; }
        if (interactive is not Door)
        {

            // check that room of Capable & room of Interactable are the same (if not same room we don't do nothing)
            // this is here and not upper because it can be a heavy call. AND NOT ON DOOR BC WE WANT TO BE ABLE TO ALWAYS INTERACT WITH THEM
            string room_of_capable = Capable.GetRealRoom();
            if (string.IsNullOrEmpty(room_of_capable)) { if (log_triggers) { Debug.Log($"(InteractCapacity) capable '{Capable.ID}' (interactor) has null or empty room, can't hover."); } return; }
            string room_of_interactable = interacted_capable.GetRealRoom();
            if (string.IsNullOrEmpty(room_of_interactable)) { if (log_triggers) { Debug.Log($"(InteractCapacity) capable '{interacted_capable.ID}' (interactable) has null or empty room, can't hover."); } return; }
            if (room_of_capable != room_of_interactable && room_of_interactable != "item_was_just_dropped") { if (log_triggers) { Debug.Log($"(InteractCapacity) '{Capable.ID}' (room : '{room_of_capable}') tried to interact with '{interacted_capable.ID}' (room : '{room_of_interactable}') but they are in different rooms"); } return; }

        }

        if (hover == closest_hover) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " hovered " + interactive.name + " but it is already hovered"); } return; }
        if (waiting_hovers.Contains(hover)) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " hovered " + interactive.name + " but it is already in the waiting hovers"); } return; }

        // we add the capable to the waiting hovers
        waiting_hovers.Add(hover);

        if (log) { Debug.Log("(InteractCapacity) " + interactive.name + " added to waiting hovers"); }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (log_triggers) { Debug.Log($"(InteractCapacity) {other.name} JUST EXIT"); }

        // we check if the other has a HoverCapacity
        HoverCapacity hover = other.transform.parent.GetComponent<HoverCapacity>();
        if (hover == null) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " exits hovered " + other.name + " but it has no HoverCapacity"); } return; }


        // we get the capable of the hover capacity
        Capable interactive = hover.Capable;
        if (interactive == null) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " exits hovered " + hover.name + " but it has no Capable"); } return; }

        // we check if it's an Interactable
        if (interactive is not Interactable) { if (log_triggers) { Debug.Log("(InteractCapacity) " + name + " exits hovered " + interactive.name + " but it is no Interactable"); } return; }

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
            if (log) { Debug.Log("(InteractCapacity) " + interactive.name + " removed from waiting hovers"); }
        }
    }

    // INFORM HOVER LOST
    public void HoverLostItself(HoverCapacity hover)
    {
        if (log || log_triggers) { Debug.Log($"(InteractCapacity) {hover.name} told us it wants to lose itself, so we do !"); }

        // the hover decided that it won't be hovered anymore...
        if (closest_hover != null && hover == closest_hover) { unselect_hover(); return; }

        // check if it is inside the waitings hover
        if (!waiting_hovers.Contains(hover)) { return; }
        waiting_hovers.Remove(hover);
    }



    ///
    //
    /// DATA MANAGEMENT
    //
    ///


    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData capable_data)
    {
        base.LoadData(data, capable_data);

        if (data is not InteractData idata) { return; }
        
        // we load the interact types
        this.interact_types = new List<InteractType>(idata.interact_types);
        // we load the item rule
        this.item_rule = idata.item_rule;
    }
    public override void UnloadData()
    {
        this.interact_types = new List<InteractType>() {};
        this.item_rule = "";
        base.UnloadData();
    }


    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        InteractData static_data = new InteractData(base.GetStaticData())
        {
            interact_types = new List<InteractType>(this.interact_types),
            item_rule = this.item_rule
        };

        return static_data;
    }

}

public enum InteractType
{
    Other,
    Chest,
    Device,
    Door,
    Kitchen,
    LivingRoom,
    Spawner,
    Corpse,
    Item,
    Crafter
}

[Serializable] public class InteractData : CapacityData
{
    // data
    public List<InteractType> interact_types;
    public string item_rule;

    // CONSTRUCTOR
    public InteractData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new InteractData(base.Duplicate() as CapacityData)
        {
            interact_types = new List<InteractType>(this.interact_types),
            item_rule = this.item_rule
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        if (this.interact_types != null) { details += $"  - interact types: {string.Join(", ", this.interact_types)}\n"; }
        else { details += $"  - no interact types\n"; }
        details += $"  - item rule: {this.item_rule}\n";
        return base.GetDetails() + details;
    }
}