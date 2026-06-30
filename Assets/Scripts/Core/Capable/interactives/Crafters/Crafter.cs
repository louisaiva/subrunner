using System.Collections.Generic;
using UnityEngine;

public class Crafter : Capable, Interactable, Openable
{

    ///
    //
    /// CRAFTER
    //
    ///
    public virtual string EmptyInventoryDesc => "no items";
    public virtual string ItemRule => "";
    public virtual string UI_PoolName => "";



    ///
    //
    /// INTERACTABLE + OPENABLE
    //
    ///

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
            if (string.IsNullOrEmpty(UI_PoolName) && TryGetCapacity(out CraftCapacity craft_capacity)) { craft_capacity.Craft(); }
            return;
        }
        interactors.Add(interactor);
        if (log) { Debug.Log("(Crafter) " + ID + " was interacted by " + interactor.ID); }

        // we open if it's the first interactor we have !!
        if (interactors.Count == 1 && TryGetCapacity(out OpenCapacity open_capacity)) { open_capacity.Open(); }

        // we get the craft pool
        UI_CraftPool pool = UI_Manager.Instance.GetPool(UI_PoolName) as UI_CraftPool;
        if (pool == null)
        {
            Debug.LogError($"(Crafter) {ID} cannot find UI_CraftPool '{UI_PoolName}' to show craft inventory");
            return;
        }

        // we only show ui if the interactor is controlled & ui not shown yet
        if (interactor != Controller.Capable || pool.IsShown(this)) { return; }

        // we show the inventory UI
        pool.ShowCraftPool(this);
    }
    public void OnHoverLost(Capable interactor)
    {
        if (!interactors.Contains(interactor)) { return; }
        interactors.Remove(interactor);

        // if there is no more interactor we close the crafter
        if (interactors.Count == 0 && TryGetCapacity(out CloseCapacity close_capa)) { close_capa.Close(); }


        // we hide the craft pool
        for (int i = 0; i < interactors.Count; i++)
        {
            if (interactors[i] == Controller.Capable) { return; } // we still have the controlled interactor so we dont hide the ui
        }
        if (string.IsNullOrEmpty(UI_PoolName)) { return; }
        UI_CraftPool pool = UI_Manager.Instance.GetPool(UI_PoolName) as UI_CraftPool;
        if (pool == null)
        {
            Debug.LogError($"(Crafter) {ID} cannot find UI_CraftPool '{UI_PoolName}' to hide craft inventory");
            return;
        }
        if (!pool.IsShown(this)) { return; }
        pool.HideCraftPool();
    }
    public async void ExitHover()
    {
        await System.Threading.Tasks.Task.Yield(); // wait a bit to avoid issues with OnHoverLost called just after
        OnHoverLost(Controller.Capable);
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