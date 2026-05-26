using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chest : Capable, Openable, Chestable
{
    public string ChestType { get; } = "chest";
    public bool log_interact_kf = false;
    public bool log_buttons_registering = false;

    [Header("Openable")]
    public virtual bool is_open { get; set; }
    public virtual bool is_moving { get; set; }

    [Header("Interactable")]
    [SerializeField] private List<Capable> interactors = new List<Capable>(); // store all interactors, not just the one controlled
    public InteractCapacity Interactor { get; set; } // there is only ONE because it's the one that is Controlled
    public virtual InteractType InteractionType { get { return InteractType.Chest; } }

    // [Header("Interact Key Feedback")]
    // private Transform interact_kf;
    // private Vector2 initial_kf_position;

    // START
    protected virtual void Start()
    {
        is_open = false;
        is_moving = false;

        // we subscribe to the hover events
    }

    // ON INTERACT / HOVER LOST
    public virtual void OnInteract(Capable interactor)
    {
        // only first interaction per interactor is authorized !!!
        if (interactors.Contains(interactor)) { return; }
        interactors.Add(interactor);
        if (log) { Debug.Log("(Chest) " + name + " was interacted by " + interactor.name); }

        // we open if it's the first interactor we have !!
        if (interactors.Count == 1 && TryGetCapacity(out OpenCapacity open_capacity)) { open_capacity.Open(); }

        // we get the chest pool
        if (!UI_Manager.Instance.TryGetPool(out UI_ChestPool chest_pool))
        {
            Debug.LogError($"(ServerRack) {name} cannot find UI_ChestPool to show chest inventory");
            return;
        }

        // only if the interactor is controlled & ui not shown yet
        if (interactor != Controller.LazyInstance.Capable || chest_pool.IsShown(this)) { return; }

        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // we show the inventory UI
        chest_pool.ShowChest(this);

        // we move the interact key feedback if we have one
        if (TryGetCapacity(out InputIndicationCapacity iic)) { iic.SetOffset(calculate_best_kf_position()); }
    }
    public void OnHoverLost(Capable interactor)
    {
        // if (debug) { Debug.Log("(Chest) " + name + " hover lost by " + interactor.name + $" (is in interactors ?? {interactors.Contains(interactor)})"); }
        if (!interactors.Contains(interactor)) { return; }
        interactors.Remove(interactor);

        // if there is no more interactor we close the chest
        if (interactors.Count == 0 && TryGetCapacity(out CloseCapacity close_capa)) { close_capa.Close(); }

        // we get the chest pool & verify if it's shown
        if (!UI_Manager.Instance.TryGetPool(out UI_ChestPool chest_pool))
        {
            Debug.LogError($"(ServerRack) {name} cannot find UI_ChestPool to show chest inventory");
            return;
        }
        if (!chest_pool.IsShown(this)) { return; }


        // if there is no more controlled interactors we hide the ui inventory
        for (int i = 0; i < interactors.Count; i++)
        {
            if (interactors[i] == Controller.LazyInstance.Capable) { return; } // we still have the controlled interactor so we dont hide the ui
        }

        // we hide the inventory UI
        chest_pool.HideChest();
        Interactor = null; // we reset the interactor

        // if (debug) { Debug.Log("(Chest) " + name + " removed hover succesfully for " + interactor.name); }

        // we reset back the interact key feedback if we have one
        if (TryGetCapacity(out InputIndicationCapacity iic)) { iic.ResetOffset(); }
    }
    public async void ExitHover()
    {
        await System.Threading.Tasks.Task.Yield(); // wait a bit to avoid issues with OnHoverLost called just after
        OnHoverLost(Controller.LazyInstance.Capable);
    }

    // UI INVENTORY SHOWING / HIDING
    // protected bool ui_inventory_shown = false;
    /* 
    protected void ShowUI_Inventory()
    {
        // attach the chest inventory & perso inventory to the ui chest pool
        UI_ChestPool ui_chest = UI_Manager.Instance.GetPool<UI_ChestPool>();
        ui_chest?.AttachChest(Inventory);
        ui_chest?.AttachPerso(Interactor?.Capable.Inventory);

        // then we show the ui_chest
        UI_Manager.Instance.SwitchTo("chest");
        ui_inventory_shown = true;
    }
    protected void HideUI_Inventory()
    {
        // we detach the chest inventory & perso inventory from the ui chest pool
        UI_ChestPool ui_chest = UI_Manager.Instance.GetPool<UI_ChestPool>();
        ui_chest?.DetachChest();
        ui_chest?.DetachPerso();

        // then we hide the ui_chest
        UI_Manager.Instance.UnstackPool("chest");
        ui_inventory_shown = false;

    } */


    // INTERACT KEY FEEDBACK
    protected virtual Vector2 calculate_best_kf_position()
    {
        // we calculate the position we need to give the kf's canvas
        return new Vector2(150.0f, 150.0f);

        /* // 1 - we get the inventory's canvas
        Transform ui_canvas = Inventory.ui.transform.parent;
        Vector2 kf_position = ui_canvas.transform.localPosition;

        // 2 - we apply an offset to the right by the columns count of the grid/2
        int columns = calculate_columns_count(Inventory.ui.GetComponent<GridLayoutGroup>());
        if (log_interact_kf) { Debug.Log("(Chest) " + name + " columns count is " + columns); }
        kf_position.x += columns / 4f; // divide by 2 bcz we move from center to right, and re-divide by 2 cz cellsize is 0.5

        // 3 - center vertically the KF + little offset on the right
        kf_position += new Vector2(0.25f, 0.25f);
        return kf_position; */
    }
    private int calculate_columns_count(GridLayoutGroup grid)
    {
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
        // float preffered_width = LayoutUtility.GetPreferredWidth(gridRect) * Inventory.ui.transform.localScale.x;

        List<float> columns_positions = new List<float>();

        // we go through all children to check their x position
        for (int i = 0; i < grid.transform.childCount; i++)
        {
            Transform child = grid.transform.GetChild(i);
            float x_position = child.localPosition.x;
            if (!columns_positions.Contains(x_position)) { columns_positions.Add(x_position); }

            // check if we have all columns
            if (columns_positions.Count >= grid.constraintCount) { break; }
        }
        
        return columns_positions.Count;
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