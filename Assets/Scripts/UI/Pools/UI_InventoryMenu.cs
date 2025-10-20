#pragma warning disable 4014
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class UI_InventoryMenu : UI_Pool, I_UI_Slottable
{
    private List<GameObject> saved_slots = new List<GameObject>();
    [Header("Inventory Menu Components")]
    [SerializeField] private UI_Inventory ui_inventory;
    public UI_Inventory UI_Inventory { get { return ui_inventory; } }
    [SerializeField] private UI_Inventory ui_laptop;
    [SerializeField] private Transform no_inventory_panel;


    [Header("Base Item Pool Transitions")]
    [SerializeField] private float base_transition = 0.2f;
    [SerializeField] private bool fade_all_disabled = true;

    [Header("Input Feedbacks")]
    [SerializeField] private ButtonFeedback drop_feedback;
    [SerializeField] private ButtonFeedback use_feedback;
    [SerializeField] private ButtonFeedback move_feedback;

    [Header("Components")]
    public Descriptor Descriptor;

    // AWAKE START
    protected override void Awake()
    {
        if (ui_inventory == null)
        {
            Debug.LogError("(UI_InventoryMenu) missing ui_inventory on " + name);
        }

        if (ui_laptop == null)
        {
            Debug.LogError("(UI_InventoryMenu) missing ui_laptop on " + name);
        }

        // we set the saved position to screen center
        SavedPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // we save the current ui_elements state in saved_state
        saved_slots = new List<GameObject>(ui_elements);
        base.Awake();
    }
    protected void Start()
    {
        UI_XboxNavigator.Instance.OnSlotHoverEnter += handleUI_ItemHoverEnter;
    }

    // LOW SHOWING
    protected override IEnumerator show_coroutine(List<GameObject> dont_show = null, float duration_override = -1f, bool was_stacked = false)
    {
        // vérifie si on a des items dans notre inventaire
        ui_elements.Clear();
        if (ui_inventory.Inventory.Count == 0) { ui_elements.Add(no_inventory_panel.gameObject); }
        else { ui_elements.AddRange(saved_slots); }

        // on affiche les items pool & indicators et on les refresh
        List<GameObject> manually_shown = get_all_uis_with_item_pools();
        manually_shown.AddRange(get_all_indicators());
        for (int i = 0; i < manually_shown.Count; i++) { manually_shown[i].SetActive(true); }
        RefreshItemPools(duration_override >= 0f ? duration_override : TransitionSettings.Duration);

        // on affiche les autres elements du menu (sans s'occuper des item pool & indicators)
        if (dont_show == null) { dont_show = new List<GameObject>(); }
        dont_show.AddRange(manually_shown);
        yield return base.show_coroutine(dont_show, duration_override);
    }


    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        // on active le navigator si on a des items
        if (ui_inventory.Inventory.Count == 0) { yield break; }
        UI_XboxNavigator.Instance.Enable(this);
        yield break;
    }
    protected override IEnumerator disable_coroutine()
    {
        // on récupère la position du slot actuel (pour le remettre quand on reouvre l'inventaire)
        if (UI_Manager.Instance.CurrentPool == Reference) { SavedPosition = UI_XboxNavigator.Instance.GetCurrentSlotPosition(); }

        // on désactive le navigator
        UI_XboxNavigator.Instance.Disable(this);
        yield break;
    }

    // ITEM POOL MANAGEMENT
    public void RefreshItemPools(float duration = -99f)
    {
        if (duration == -99f) { duration = base_transition; }
        // if (log) { Debug.Log($"(UI_InventoryMenu) refreshing item pools with duration {duration}"); }

        // preparing logs
        string log_msg = $"(UI_InventoryMenu) refreshing item pools with duration {duration}";

        // on fade out les item pools qui sont vides & fade in ceux qui sont pleins
        List<GameObject> uis_with_item_pools = get_all_uis_with_item_pools();
        for (int i = 0; i < uis_with_item_pools.Count; i++)
        {
            // on récupère l'item pool & le transitioner
            UI_ItemPool item_pool = uis_with_item_pools[i].GetComponentInChildren<UI_ItemPool>();
            if (item_pool == null) { continue; }
            Transitioner transitioner = uis_with_item_pools[i].GetComponent<Transitioner>();

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
        }

        if (log) { Debug.Log(log_msg); }

        // on refresh les indicators
        GetComponent<UI_PanelManager>().RefreshIndicators(duration);
    }
    private List<GameObject> get_all_uis_with_item_pools()
    {
        List<GameObject> item_pools = new List<GameObject>();
        foreach (GameObject ui in ui_elements)
        {
            UI_ItemPool item_pool = ui.GetComponentInChildren<UI_ItemPool>();
            if (item_pool != null) { item_pools.Add(ui); }
        }
        return item_pools;
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

    // SLOTTABLE
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {
        if (log) { Debug.Log($"(UI_InventoryMenu) getting slots"); }
        List<GameObject> slots = new List<GameObject>();

        // on ajoute les items de l'UI_Inventory
        slots.AddRange(ui_inventory.GetSlots(ref base_position, ref angle_threshold, ref angle_multiplicator));
        if (UI_LaptopItemSlot.Instance == null || !UI_LaptopItemSlot.Instance.HasLaptop) { return slots; }

        // si on a le laptop, on ajoute aussi ceux de l'UI_Laptop
        slots.AddRange(ui_laptop.GetSlots(ref base_position, ref angle_threshold, ref angle_multiplicator));

        return slots;
    }
    public bool IsYourSlot(GameObject slot)
    {
        if (ui_inventory.IsYourSlot(slot)) { return true; }
        if (UI_LaptopItemSlot.Instance != null && UI_LaptopItemSlot.Instance.HasLaptop && ui_laptop.IsYourSlot(slot)) { return true; }
        return false;
    }
    public Vector2 SavedPosition { get; private set; } = Vector2.zero;

    // INPUT SWITCHING
    protected void handleUI_ItemHoverEnter(I_UI_Slot slot)
    {
        if (!Showed) { return; }
        if (slot is not UI_Item ui_slot) { return; }

        // we handle the DROP (activate it only if it is a UI_Item that has Item & not a UI_Module)
        if (ui_slot is UI_Module || ui_slot.Item == null)
        {
            drop_feedback.SetAlwaysFull(false);
            drop_feedback.SetLabel("");
            UI_XboxNavigator.Instance.ToggleInput("drop", false);
        }
        else
        {
            drop_feedback.SetAlwaysFull(true);
            drop_feedback.SetLabel("drop");
            UI_XboxNavigator.Instance.ToggleInput("drop", true);
        }

        // ACTIVATE
        if (ui_slot.Item != null && ui_slot.Item is Usable usable)
        {
            use_feedback.SetAlwaysFull(true);
            use_feedback.SetLabel(usable.UseLabel);
            UI_XboxNavigator.Instance.ToggleInput("activate", true);
        }
        else if (ui_slot is UI_Module && ui_slot.Item != null && ui_slot.Item.Reference == "module:hdd")
        {
            use_feedback.SetAlwaysFull(true);
            use_feedback.SetLabel("inspect");
            UI_XboxNavigator.Instance.ToggleInput("activate", true);
        }
        else
        {
            use_feedback.SetAlwaysFull(false);
            use_feedback.SetLabel("");
            UI_XboxNavigator.Instance.ToggleInput("activate", false);
        }

        // MOVE
        if (ui_slot.Item == null && !UI_XboxNavigator.Instance.IsMovingItem)
        {
            move_feedback.SetAlwaysFull(false);
            move_feedback.SetLabel("");
            UI_XboxNavigator.Instance.ToggleInput("move", false);
        }
        else
        {
            move_feedback.SetAlwaysFull(true);
            move_feedback.SetLabel("move");
            UI_XboxNavigator.Instance.ToggleInput("move", true);
        }
    }
}