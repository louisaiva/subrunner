using System.Collections.Generic;
using UnityEngine;

public class Crafter : Capable, Openable, Chestable
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
    public string ChestType => UI_PoolName;

    // ON INTERACT / HOVER LOST
    public virtual void OnInteract(Capable interactor)
    {
        if (!interactors.Contains(interactor)) { interactors.Add(interactor); }
        if (log) { Debug.Log("(Crafter) " + ID + " was interacted by " + interactor.ID); }
        if (TryGetCapacity(out OpenCapacity oc) && !oc.IsOpenOrOpening) { oc.Open(); }
        if (interactor == Controller.Capable) { show_ui_pool(); }
    }
    public void OnHoverLost(Capable interactor)
    {
        if (interactor == Controller.Capable) { hide_ui_pool(); }

        if (!interactors.Contains(interactor)) { return; }
        interactors.Remove(interactor);

        // if there is no more interactor we close the crafter
        if (interactors.Count == 0 && TryGetCapacity(out CloseCapacity close_capa)) { close_capa.Close(); }
    }
    public async void ExitHover()
    {
        await System.Threading.Tasks.Task.Yield(); // wait a bit to avoid issues with OnHoverLost called just after
        OnHoverLost(Controller.Capable);
    }

    // show / hide pool
    protected virtual void show_ui_pool()
    {
        if (string.IsNullOrEmpty(UI_PoolName)) { return; }
        UI_CraftPool pool = UI_Manager.Instance.GetPool(UI_PoolName) as UI_CraftPool;
        if (pool == null)
        {
            Debug.LogError($"(Crafter) {ID} cannot find UI_CraftPool '{UI_PoolName}' to show craft inventory");
            return;
        }

        if (pool.IsShown(this)) { return; } // we only show ui if not shown yet
        pool.ShowCraftPool(this);
    }
    protected virtual void hide_ui_pool()
    {
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