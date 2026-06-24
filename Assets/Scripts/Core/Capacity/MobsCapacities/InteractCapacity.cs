
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// InteractCapacity is a capacity that allows a Capable to interact with Interactable objects.
/// When approaching an Interactable, the capacity make the object play hover.
/// And when pressing the Interact Button, the PIC (PersoInputController) will call the this.Interact() method
/// which will trigger Interactable.OnInteract() method (or Interactable.OnEndlessInteract())
/// </summary>

public class InteractCapacity : Capacity
{

    [Header("Interaction rules")]
    // [SerializeField] private GrabCapacity grab_capacity;
    [SerializeField] protected List<InteractType> interact_types = new List<InteractType>() {};
    [SerializeField] protected string item_rule = ""; // rule to check if an item is interactable with us
    public string ItemRule { get => item_rule; }


    ///
    //
    /// MANUAL INTERACTION
    //
    ///

    /// <summary>
    /// This method is a manual implementation of the main Interact()
    /// method, i.e. for IAs with actions that need to interact with capable
    /// without needing to go through a hover/etc kind of thing
    /// </summary>
    public void InteractWithInteractable(Interactable interactive, bool endless = false)
    {
        // interact with interactable & select + grab items
        if (!endless) { interactive.OnInteract(Capable); }
        else if (interactive is EndlessInteractable interactable_endless && endless) { interactable_endless.OnEndlessInteract(Capable); }
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