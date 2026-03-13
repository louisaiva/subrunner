using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chest : Capable, Interactable, Openable
{
    public bool log_interact_kf = false;
    public bool log_buttons_registering = false;

    [Header("Openable")]
    public virtual bool is_open { get; set; }
    public virtual bool is_moving { get; set; }

    [Header("Interactable")]
    [SerializeField] private List<Capable> interactors = new List<Capable>(); // store all interactors, not just the one controlled
    public InteractCapacity Interactor { get; set; } // there is only ONE because it's the one that is Controlled
    public virtual InteractType InteractionType { get { return InteractType.Chest; } }

    [Header("Interact Key Feedback")]
    private Transform interact_kf;
    private Vector2 initial_kf_position;

    // START
    protected virtual void Start()
    {
        is_open = false;
        is_moving = false;

        // we subscribe to the hover events
        HoverCapacity hover_capacity = GetCapacity<HoverCapacity>();
        hover_capacity.OnHoverLost += OnHoverLost;
        interact_kf = hover_capacity.Canvas_kf;
        if (interact_kf != null) { initial_kf_position = interact_kf.localPosition; }
    }

    // ON INTERACT / HOVER LOST
    public virtual void OnInteract(Capable interactor)
    {
        // only first interaction per interactor is authorized !!!
        if (interactors.Contains(interactor)) { return; }
        interactors.Add(interactor);
        if (log) { Debug.Log("(Chest) " + name + " was interacted by " + interactor.name); }

        // we open if it's the first interactor we have !!
        if (interactors.Count == 1) { GetCapacity<OpenCapacity>()?.Use(interactor); }

        // only if the interactor is controlled
        if (interactor == Controller.Instance.Capable && !ui_inventory_shown)
        {
            // we set the interactor
            Interactor = interactor.GetCapacity<InteractCapacity>();

            // we show the inventory UI
            ShowUI_Inventory();
        }
    }
    public void OnHoverLost(Capable interactor)
    {
        // if (debug) { Debug.Log("(Chest) " + name + " hover lost by " + interactor.name + $" (is in interactors ?? {interactors.Contains(interactor)})"); }
        if (!interactors.Contains(interactor)) { return; }
        interactors.Remove(interactor);

        // if there is no more interactor we close the chest
        if (interactors.Count == 0) { GetCapacity<CloseCapacity>()?.Use(interactor); }

        // if there is no more controlled interactors we hide the ui inventory
        if (!ui_inventory_shown) { return; }
        for (int i = 0; i < interactors.Count; i++)
        {
            if (interactors[i] == Controller.Instance.Capable) { return; } // we still have the controlled interactor so we dont hide the ui
        }

        // we hide the inventory UI
        HideUI_Inventory();
        Interactor = null; // we reset the interactor

        // if (debug) { Debug.Log("(Chest) " + name + " removed hover succesfully for " + interactor.name); }
    }
    public async void ExitHover()
    {
        await System.Threading.Tasks.Task.Yield(); // wait a bit to avoid issues with OnHoverLost called just after
        OnHoverLost(Controller.Instance.Capable);
    }

    // UI INVENTORY SHOWING / HIDING
    protected bool ui_inventory_shown = false;
    protected void ShowUI_Inventory()
    {
        // attach the chest inventory & perso inventory to the ui chest pool
        UI_ChestPool ui_chest = UI_Manager.Instance.GetPool<UI_ChestPool>();
        ui_chest?.AttachChest(Inventory);
        ui_chest?.AttachPerso(Interactor?.Capable.Inventory);

        // then we show the ui_chest
        UI_Manager.Instance.SwitchTo("chest");
        ui_inventory_shown = true;

        // we move the interact key feedback if we have one
        if (interact_kf == null) { return; }
        interact_kf.localPosition = calculate_best_kf_position();
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

        // we move back the interact key feedback if we have one
        if (interact_kf == null) { return; }
        interact_kf.localPosition = initial_kf_position;
    }


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
}