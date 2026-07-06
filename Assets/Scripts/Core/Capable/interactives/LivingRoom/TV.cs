using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this is a TV. Actually it can open/close lol
/// so this is a weird TV but open makes the TV turn on
/// and close turn off.
/// For now it mainly look like a Openable intermediate class tbf
/// : // todo : make this an Openable intermediate class that only handles on interact/interactors/exit properly.
/// </summary>

public class TV : Capable, Interactable, Openable
{
    private List<InteractCapacity> interactors = new List<InteractCapacity>();
    public InteractCapacity Interactor => interactors.Count > 0 ? interactors[0] : null;
    public InteractType InteractionType => InteractType.Other;

    public bool is_moving { get; set; } = false;
    public bool is_open { get; set; } = false;

    // MAIN METHODS
    public void OnInteract(Capable interactor)
    {
        if (!interactor.TryGetCapacity(out InteractCapacity interact_capacity)) { return; }
        if (interactors.Contains(interact_capacity)) { return; } // todo : call on hover exit here 
        interactors.Add(interact_capacity);

        // we check if we have an interactor already
        if (interactors.Count == 1 && TryGetCapacity(out OpenCapacity open_capacity)) { open_capacity.Open(); }
    }
    public void OnHoverLost(Capable interactor)
    {
        if (interactor.TryGetCapacity(out InteractCapacity interact_capacity) && interactors.Contains(interact_capacity))
        { interactors.Remove(interact_capacity); }

        // if there is no more interactor we close the thing
        if (interactors.Count == 0 && ((Openable)this).IsOpenOrOpening() && TryGetCapacity(out CloseCapacity close_capa))
        { close_capa.Close(); }
    }
    
    /* /// <summary>
    /// this is an UI helper method to avoid issues when calling onhoverlost too fast
    /// </summary>
    public async void ExitHover()
    {
        await System.Threading.Tasks.Task.Yield();
        OnHoverLost(Controller.Capable);
    } */

    ///
    //
    /// DATA MANAGEMENT
    //
    ///

    // LOAD / UNLOAD DATA
    public override void LoadData(CapableData data)
    {
        base.LoadData(data);

        // we subscribe to the hover events
        if (TryGetCapacity(out HoverCapacity hover_capacity))
        {
            hover_capacity.OnHoverLost += OnHoverLost;
        }
    }
    public override void UnloadData()
    {
        // we unsubscribe to the hover events
        if (TryGetCapacity(out HoverCapacity hover_capacity))
        {
            hover_capacity.OnHoverLost -= OnHoverLost;
        }

        base.UnloadData();
    }
}