#pragma warning disable 4014
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_InventoryMenu : UI_Pool/* , Panelable */
{
    private List<GameObject> saved_slots = new List<GameObject>();
    [Header("Inventory Menu Components")]
    // [SerializeField] private UI_Inventory ui_inventory;
    // [SerializeField] private UI_Inventory ui_laptop;
    [SerializeField] private Transform no_inventory_panel;
    /* private UI_PanelManager _panel_manager;
    public UI_PanelManager PanelManager { get
            {
                if (_panel_manager == null)
                {
                    _panel_manager = GetComponent<UI_PanelManager>();
                }
                return _panel_manager;
        } } */


    [Header("Base Item Pool Transitions")]
    [SerializeField] private float base_transition = 0.2f;
    [SerializeField] private bool fade_all_disabled = true;

    [Header("Input Feedbacks Pools")]
    [SerializeField] private FeedbackPoolBuilder IFs;

    [Header("Components")]
    public UI_SlottableMixer slottable_mixer;

    // AWAKE START
    protected override void Awake()
    {
        /* if (ui_inventory == null)
        {
            Debug.LogError("(UI_InventoryMenu) missing ui_inventory on " + name);
        } */

        /* if (ui_laptop == null)
        {
            Debug.LogError("(UI_InventoryMenu) missing ui_laptop on " + name);
        } */

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

    // LOW SHOWING
    protected override IEnumerator show_coroutine(List<GameObject> dont_show = null, float duration_override = -1f, bool was_stacked = false)
    {
        // vérifie si on a des items dans notre inventaire
        ui_elements.Clear();
        if (Perso.Instance.Inventory.Count == 0) { ui_elements.Add(no_inventory_panel.gameObject); }
        else { ui_elements.AddRange(saved_slots); }

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
        if (Perso.Instance.Inventory.Count == 0) { yield break; }
        // UI_Navigator.Instance.Enable(this);
        slottable_mixer.Enable(ingame: false);
        yield break;
    }
    protected override IEnumerator disable_coroutine()
    {
        // on récupère la position du slot actuel (pour le remettre quand on reouvre l'inventaire)
        // if (UI_Manager.Instance.CurrentPool == Reference) { SavedPosition = UI_Navigator.Instance.GetCurrentSlotPosition(); }

        // on désactive le navigator
        // UI_Navigator.Instance.Disable(this);
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
        for (int i = 0; i < item_pools.Count; i++)
        {
            // on récupère l'item pool & le transitioner
            UI_ItemPool item_pool = item_pools[i];
            if (item_pool == null) { continue; }
            Transitioner transitioner = item_pool.GetComponentInParent<Transitioner>(includeInactive: true);

            log_msg += $"\n - investigating ui_itempool {item_pool.name} (shown ? {transitioner.Shown} vs hidden ? {transitioner.Hidden}) with "
                + item_pool.Count + " slots and " + item_pool.FullCount + " items slots " + $"and {item_pool.EnabledCount} enabled slots ";

            // on regarde si la pool doit être affichée ou non
            if (fade_all_disabled || item_pool is UI_ModulePool) // ui_module pool fonctionne toujours en mode fade_all_disbled
            {
                if (item_pool.EnabledCount > 0 && !transitioner.Shown) { transitioner.Show(duration); log_msg += " -> fading in"; }
                else if (item_pool.EnabledCount == 0 && !transitioner.Hidden) { transitioner.Hide(duration); log_msg += " -> fading out"; }
            }
            else if (!item_pool.DoNotDisableEmptySlots) // si on est donotdisableemptyslots ça veut dire qu'on veut que ça soit toujours affiché
            {
                if (!transitioner.Shown && (item_pool.EnabledCount > 0 || item_pool.FullCount > 0)) { transitioner.Show(duration); log_msg += " -> fading in"; }
                else if (!transitioner.Hidden && item_pool.EnabledCount == 0 && item_pool.FullCount == 0) { transitioner.Hide(duration); log_msg += " -> fading out"; }
            }
            else if (!transitioner.Shown) { transitioner.Show(duration); log_msg += " -> always fading in"; } // on affiche toujours la pool si elle n'est pas affichée
        }

        if (log) { Debug.Log(log_msg); }

        // on refresh les indicators
        // PanelManager.RefreshIndicators(duration);
    }
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
        List<UI_ItemPool> pools = new List<UI_ItemPool>(/* ui_inventory.pools */);
        // pools.AddRange(ui_laptop.pools);

        // find the pools in the ui_elements
        for (int i = 0; i < ui_elements.Count; i++)
        {
            GameObject ui = ui_elements[i];
            List<UI_ItemPool> ui_item_pools = new List<UI_ItemPool>(ui.GetComponentsInChildren<UI_ItemPool>(includeInactive: true));
            if (ui_item_pools.Count == 0) { continue; }
            for (int j = 0; j < ui_item_pools.Count; j++)
            {
                UI_ItemPool ui_item_pool = ui_item_pools[j];
                if (ui_item_pool != null) { pools.Add(ui_item_pool); }
            }
        }
        return pools;
    }
    private List<GameObject> get_all_indicators()
    {
        List<GameObject> indicators = new List<GameObject>();
        foreach (GameObject ui in ui_elements)
        {
            UI_PanelIndicator indicator = ui.GetComponent<UI_PanelIndicator>();
            if (indicator != null) { indicators.Add(ui); }
        }
        return indicators;
    }

    // IF SWITCHING
    private void handleUI_ItemHoverEnter(UI_Slot slot)
    {
        if (!Showed) { return; }
        if (slot is not UI_Item ui_item) { return; }

        if (log) { Debug.Log($"(UI_InventoryMenu) bwaaaa handleUI_ItemHoverEnter for slot {slot.gameObject.name}"); }

        // we handle the DROP (activate it only if it is a UI_Item that has Item & not a UI_Module)
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
    private void update_activate_if(UI_Item ui_item)
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
        if (slot == null || slot is not UI_Item ui_item) { return; }

        // we update the activate if needed
        if (ui_item.Stack.Item != item) { return; }
        update_activate_if(ui_item);
    }
}

public interface Panelable
{
    UI_PanelManager PanelManager { get;  }
}