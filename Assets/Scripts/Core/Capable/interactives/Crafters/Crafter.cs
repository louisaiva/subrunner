using System.Collections.Generic;
using UnityEngine;

public class Crafter : Capable, Interactable, Openable
{
    [SerializeField] private List<Capable> interactors = new List<Capable>(); // store all interactors, not just the one controlled
    public InteractCapacity Interactor
    {
        get
        {
            if (interactors.Count > 0) { return interactors[0].GetCapacity<InteractCapacity>(); }
            else { return null; }
        }
    }
    public InteractType InteractionType => InteractType.Crafter;

    [Header("Openable")]
    [field:SerializeField] public virtual bool is_moving { get; set; }
    [field:SerializeField] public virtual bool is_open { get; set; }

    // ON INTERACT / HOVER LOST
    public virtual void OnInteract(Capable interactor)
    {
        // only first interaction per interactor is authorized !!!
        if (interactors.Contains(interactor))
        {
            // we play the craft animation
            if (TryGetCapacity(out CraftCapacity craft_capacity)) { craft_capacity.Craft(); }
            return;
        }
        interactors.Add(interactor);
        if (log) { Debug.Log("(Crafter) " + name + " was interacted by " + interactor.name); }

        // we open if it's the first interactor we have !!
        if (interactors.Count == 1 && TryGetCapacity(out OpenCapacity open_capacity)) { open_capacity.Open(); }

        // we get the craft pool
        /* if (!UI_Manager.Instance.TryGetPool(out UI_ChestPool chest_pool))
        {
            Debug.LogError($"(Crafter) {name} cannot find UI_ChestPool to show chest inventory");
            return;
        } */

        // we only show ui if the interactor is controlled & ui not shown yet
        // if (interactor != Controller.LazyInstance.Capable || chest_pool.IsShown(this)) { return; }

        // we show the inventory UI
        // chest_pool.ShowChest(this);
    }
    public void OnHoverLost(Capable interactor)
    {
        if (!interactors.Contains(interactor)) { return; }
        interactors.Remove(interactor);

        // if there is no more interactor we close the crafter
        if (interactors.Count == 0 && TryGetCapacity(out CloseCapacity close_capa)) { close_capa.Close(); }

        // we get the chest pool & verify if it's shown
        /* if (!UI_Manager.Instance.TryGetPool(out UI_ChestPool chest_pool))
        {
            Debug.LogError($"(Crafter) {name} cannot find UI_ChestPool to show chest inventory");
            return;
        }
        if (!chest_pool.IsShown(this)) { return; }


        // if there is no more controlled interactors we hide the ui inventory
        for (int i = 0; i < interactors.Count; i++)
        {
            if (interactors[i] == Controller.LazyInstance.Capable) { return; } // we still have the controlled interactor so we dont hide the ui
        }

        // we hide the inventory UI
        chest_pool.HideChest(); */

        // if (debug) { Debug.Log("(Chest) " + name + " removed hover succesfully for " + interactor.name); }
    }
    public async void ExitHover()
    {
        await System.Threading.Tasks.Task.Yield(); // wait a bit to avoid issues with OnHoverLost called just after
        OnHoverLost(Controller.LazyInstance.Capable);
    }




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