#pragma warning disable 4014
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_InventoryMenu : UI_Pool
{
    [SerializeField] private bool log_refresh_pools = false;
    [SerializeField] private bool log_ui_attachment = false;
    [SerializeField] private bool log_specific_get_item_pools = false;

    [Header("Inventory Menu Components")]
    [SerializeField] private Transform no_inventory_panel;

    [Header("Base Item Pool Transitions")]
    [SerializeField] private float base_transition = 0.2f;
    [SerializeField] private bool never_fade_pools = true;

    [Header("Input Feedbacks Pools")]
    [SerializeField] private FeedbackPoolBuilder IFs;

    [Header("Components")]
    public UI_SlottableMixer slottable_mixer;
    private UI_ParentBasedSlottable parent_based_slottable;

    // AWAKE START
    private List<GameObject> saved_slots = new List<GameObject>();
    private List<Transform> saved_parent_based_slottable_parents = new List<Transform>();
    protected override void Awake()
    {
        // we save the current parents of the parent based slottables in saved_parent_based_slottable_parents
        parent_based_slottable = GetComponent<UI_ParentBasedSlottable>();
        if (parent_based_slottable != null) { saved_parent_based_slottable_parents = new List<Transform>(parent_based_slottable.SlotsParents); }

        // we save the current ui_elements state in saved_state
        saved_slots = new List<GameObject>(ui_elements);
        base.Awake();

        // we add all the UI_ItemPools in the ui_elements as slottables inside our UI_SlottableMixer
        List<UI_ItemPool> ui_item_pools = GetItemPools();
        for (int i = 0; i < ui_item_pools.Count; i++) { slottable_mixer.AddSlottable(ui_item_pools[i]); }
    }
    protected void Start()
    {
        UI_Navigator.Instance.OnSlotHoverEnter += handleUI_ItemHoverEnter;
        if (log) { Debug.Log($"(UI_InventoryMenu) subscribed to OnSlotHoverEnter"); }
    }



    ///
    //
    /// BUILDING INVENTORY UI
    //
    ///
    protected override void before_adding_to_stack() { attach_controller_inventory(); }
    protected override void after_removed_from_stack() { detach_inventory(); }
    private void attach_controller_inventory()
    {
        // get the controlled capable if any
        if (Controller.Capable == null)
        {
            if (log_ui_attachment) { Debug.LogWarning($"(UI_InventoryMenu) No capable controlled by the controller! Skipping inventory UI attachment."); }
            return;
        }
        Inventory inventory = Controller.Capable.Inventory;
        if (inventory == null)
        {
            if (log_ui_attachment) { Debug.LogWarning($"(UI_InventoryMenu) Capable '{Controller.Capable.ID}' has no inventory! Skipping inventory UI attachment."); }
            return;
        }

        // on récupère les ui_item_pools du ui_inventoryMenu
        List<UI_ItemPool> ui_pools = GetItemPools();

        // we go through all ui_pools found in the menu
        for (int i = 0; i < ui_pools.Count; ++i)
        {
            UI_ItemPool ui_pool = ui_pools[i];
            if (ui_pool == null) { continue; }

            // on regarde si on a un item pool dans l'inventaire qui a la même pool_id
            ItemPool pool = inventory.GetItemPool(ui_pool.PoolID);
            if (pool == null)
            {
                // if we don't have an item pool for this ui pool, we skip it
                if (log_ui_attachment) { Debug.LogWarning($"(UI_InventoryMenu) No ItemPool found for UI_ItemPool with id {ui_pool.PoolID} in inventory of capable '{Controller.Capable.ID}'! Skipping UI attachment for this pool."); }
                continue;
            }

            if (log_ui_attachment) { Debug.Log($"(UI_InventoryMenu) Attaching ItemPool with id {pool.PoolID} to UI_ItemPool {ui_pool.name} for capable '{Controller.Capable.ID}'."); }

            // on attache la pool à l'ui pool
            ui_pool.AttachToPool(pool);
        }
        if (log_ui_attachment) { Debug.Log($"(UI_InventoryMenu) Attached all UI_ItemPools to relative ItemPools of '{Controller.Capable.ID}'"); }
    }
    private void detach_inventory()
    {
        List<UI_ItemPool> ui_pools = GetItemPools();
        for (int i = 0; i < ui_pools.Count; ++i) { ui_pools[i].DetachFromPool(); }
        if (log_ui_attachment) { Debug.Log($"(UI_InventoryMenu) Detached all UI_ItemPools from their ItemPools."); }
    }






    ///
    //
    /// SHOWING / ENABLE / DISABLE / REFRESH UI
    //
    ///


    // LOW SHOWING
    protected override IEnumerator show_coroutine(List<GameObject> dont_show = null, float duration_override = -1f, bool was_stacked = false)
    {
        // vérifie si on a des items dans notre inventaire
        ui_elements.Clear();
        if (Controller.Perso == null || Controller.Perso.Inventory.Count == 0)
        {
            ui_elements.Add(no_inventory_panel.gameObject);
            parent_based_slottable.SlotsParents = new List<Transform>() { no_inventory_panel };
        }
        else
        {
            ui_elements.AddRange(saved_slots);
            parent_based_slottable.SlotsParents = new List<Transform>(saved_parent_based_slottable_parents);
        }

        // on affiche les items pool & indicators et on les refresh
        List<GameObject> manually_shown = get_all_uis_with_item_pools();
        manually_shown.AddRange(get_all_indicators());
        for (int i = 0; i < manually_shown.Count; i++) { manually_shown[i].SetActive(true); }
        RefreshItemPools(duration_override >= 0f ? duration_override : Settings.Duration);

        // on affiche les autres elements du menu (sans s'occuper des item pool & indicators)
        if (dont_show == null) { dont_show = new List<GameObject>(); }
        dont_show.AddRange(manually_shown);
        yield return base.show_coroutine(dont_show, duration_override);
    }

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        // on active le navigator si on a des items
        // if (Controller.Perso == null || Controller.Perso.Inventory.Count == 0) { yield break; }
        slottable_mixer.Enable(ingame: false);
        yield break;
    }
    protected override IEnumerator disable_coroutine()
    {
        // on désactive le navigator
        slottable_mixer.Disable();
        yield break;
    }

    // ITEM POOL MANAGEMENT
    public void RefreshItemPools(float duration = -99f)
    {
        if (duration == -99f) { duration = base_transition; }

        // preparing logs
        string log_msg = $"(UI_InventoryMenu) refreshing item pools with duration {duration}";

        // on fade out les item pools qui sont vides & fade in ceux qui sont pleins
        List<UI_ItemPool> item_pools = GetItemPools();
        for (int i = 0; i < item_pools.Count; i++) { refresh_item_pool(item_pools[i], duration, ref log_msg); }

        if (log_refresh_pools) { Debug.Log(log_msg); }
    }
    private void refresh_item_pool(UI_ItemPool item_pool, float duration, ref string log_msg)
    {
        // on récupère l'item pool & le transitioner
        if (item_pool == null) { return; }
        Transitioner transitioner = item_pool.GetComponentInParent<Transitioner>(includeInactive: true);

        log_msg += $"\n - investigating ui_itempool {item_pool.name} (shown ? {transitioner.Shown} vs hidden ? {transitioner.Hidden}) with "
            + item_pool.Count + " slots and " + item_pool.FullCount + " items slots " + $"and {item_pool.EnabledCount} enabled slots ";


        // check whether the pool should be shown or not
        bool should_pool_be_shown = false;
        if (never_fade_pools) { should_pool_be_shown = true; }
        else if (item_pool.AlwaysShow) { should_pool_be_shown = true; }
        if (item_pool is UI_ModulePool) { should_pool_be_shown = false; } // specific override for UI_ModulePool
        if (item_pool.EnabledCount > 0) { should_pool_be_shown = true; }

        if (should_pool_be_shown) { transitioner.Show(duration); log_msg += " -> should be shown"; }
        else { transitioner.Hide(duration); log_msg += " -> should be hidden"; }
    }



    ///
    //
    /// GETTER / IF LABELS UPDATE
    //
    ///

    private List<GameObject> get_all_uis_with_item_pools()
    {
        List<GameObject> item_pools = new List<GameObject>();
        foreach (GameObject ui in ui_elements)
        {
            UI_ItemPool item_pool = ui.GetComponentInChildren<UI_ItemPool>(includeInactive: true);
            if (item_pool != null) { item_pools.Add(ui); }
        }
        return item_pools;
    }
    public List<UI_ItemPool> GetItemPools()
    {
        List<UI_ItemPool> pools = GetComponentsInChildren<UI_ItemPool>(includeInactive: true).ToList();
        if (log_specific_get_item_pools) { Debug.Log($"(UI_InventoryMenu) Gathered {pools.Count} UI_ItemPools in the inventory menu."); }
        return pools;

        /* // find the pools in the ui_elements
        for (int i = 0; i < ui_elements.Count; i++)
        {
            GameObject ui = ui_elements[i];
            List<UI_ItemPool> ui_item_pools = new List<UI_ItemPool>(ui.GetComponentsInChildren<UI_ItemPool>(includeInactive: true));
            if (ui_item_pools.Count == 0) { continue; }
            for (int j = ui_item_pools.Count - 1; j >= 0; j--)
            {
                UI_ItemPool ui_item_pool = ui_item_pools[j];
                if (ui_item_pool == null) { ui_item_pools.RemoveAt(j); continue; }
                pools.Add(ui_item_pool);
            }
            if (log_specific_get_item_pools) { Debug.Log($"(UI_InventoryMenu) Gathered {ui_item_pools.Count} UI_ItemPools in ui element {ui.name}."); }
        }
        if (log_specific_get_item_pools) { Debug.Log($"(UI_InventoryMenu) Gathered a total of {pools.Count} UI_ItemPools in the inventory menu."); }
        return pools; */
    }
    private List<GameObject> get_all_indicators()
    {
        return GetComponentsInChildren<UI_PanelIndicator>(includeInactive: true)
            .Select(indicator => indicator.gameObject)
            .Where(indicator_go => indicator_go != null)
            .ToList();
        /* foreach (GameObject ui in ui_elements)
        {
            UI_PanelIndicator indicator = ui.GetComponent<UI_PanelIndicator>();
            if (indicator != null) { indicators.Add(ui); }
        }
        return indicators; */
    }

    // IF SWITCHING
    private void handleUI_ItemHoverEnter(UI_Slot slot)
    {
        if (!Showed) { return; }
        if (slot is not UI_ItemStack ui_item) { return; }

        // we handle the DROP (activate it only if it is a UI_ItemStack that has Item & not a UI_Module)
        if (ui_item is UI_Module || ui_item.Stack.Item == null)
        {
            IFs.DisableRows("drop");
        }
        else
        {
            IFs.EnableRows("drop");
        }

        // ACTIVATE
        update_activate_if(ui_item);
    }
    private void update_activate_if(UI_ItemStack ui_item)
    {
        if (log) { Debug.Log($"(UI_InventoryMenu) updating activate IF for ui_item with item {ui_item.Stack.Item?.name}"); }

        // if we have no item or no usable & no inspectable
        if (ui_item.Stack.Item == null || (ui_item.Stack.Item is not Usable && ui_item.Stack.Item is not Inspectable)
        || (ui_item.Stack.Item is Usable usable && usable.UseLabel == "")
        || (ui_item.Stack.Item is Inspectable inspectable && inspectable.InspectLabel == ""))
        {
            IFs.DisableRows("activate");
            return;
        }

        // we have a usable or an inspectable
        string label = "";
        if (ui_item.Stack.Item is Usable usable_item) { label = usable_item.UseLabel; }
        else if (ui_item.Stack.Item is Inspectable inspectable_item) { label = inspectable_item.InspectLabel; }

        // we enable the button & set the label
        IFs.EnableRows("activate");
        IFs.SetTextOnRows("activate", label);
    }
    public void UpdateIFLabels(Item item)
    {
        // we get the ui_item from the item
        UI_Slot slot = UI_Navigator.Instance.GetCurrentSlot();
        if (slot == null || slot is not UI_ItemStack ui_item) { return; }

        // we update the activate if needed
        if (ui_item.Stack.Item != item) { return; }
        update_activate_if(ui_item);
    }
}

public interface Panelable
{
    public UI_PanelManager PanelManager { get;  }
}

public interface Scrollable
{
    public UI_Scroller Scroller { get; }
}