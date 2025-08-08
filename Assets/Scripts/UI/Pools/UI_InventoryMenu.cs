#pragma warning disable 4014
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class UI_InventoryMenu : UI_Pool, I_UI_Slottable
{
    private List<GameObject> saved_slots = new List<GameObject>();
    [Header("Inventory Menu Components")]
    [SerializeField] private UI_Inventory ui_inventory;
    [SerializeField] private UI_Inventory ui_laptop;
    [SerializeField] private Transform no_inventory_panel;
    // [SerializeField] private UI_ModulePool motherboard_pool;


    [Header("Base Item Pool Transitions")]
    [SerializeField] private float base_transition = 0.2f;
    [SerializeField] private bool fade_all_disabled = true;

    [Header("Input Feedbacks")]
    [SerializeField] private ButtonFeedback drop_feedback;
    [SerializeField] private ButtonFeedback use_feedback;
    [SerializeField] private ButtonFeedback move_feedback;

    // AWAKE START
    protected void Awake()
    {
        // on récupère le navigator
        // navigator = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();

        // on récupère les composants
        // ui_inventory = transform.Find("ui_inventory").GetComponent<UI_Inventory>();
        if (ui_inventory == null)
        {
            Debug.LogError("(UI_InventoryMenu) missing ui_inventory on " + name);
        }
        // ui_laptop = transform.Find("ui_laptop").GetComponent<UI_Inventory>();
        if (ui_laptop == null)
        {
            Debug.LogError("(UI_InventoryMenu) missing ui_laptop on " + name);
        }

        // we save the current ui_elements state in saved_state
        saved_slots = new List<GameObject>(ui_elements);
    }
    protected override void Start()
    {
        UI_XboxNavigator.Instance.OnSlotHoverEnter += handleUI_ItemHoverEnter;
        base.Start();
    }

    // SHOW / HIDE
    public override async Awaitable Show(float duration)
    {
        // vérifie si on a des items dans notre inventaire
        ui_elements.Clear();
        if (ui_inventory.Inventory.Count == 0) { ui_elements.Add(no_inventory_panel.gameObject); }
        else { ui_elements.AddRange(saved_slots); }

        await base.Show(duration);
        if (ui_inventory.Inventory.Count == 0) { return; }

        UI_XboxNavigator.Instance.Enable(this);

        // on met à jour l'angle treshold du UI_XboxNavigator.Instance
        UI_XboxNavigator.Instance.angle_threshold = base.angle_threshold;
    }
    public override async Awaitable Hide(float duration)
    {
        // on récupère la position du slot actuel (pour le remettre quand on reouvre l'inventaire)
        SavedPosition = UI_XboxNavigator.Instance.GetCurrentSlotPosition();

        // on désactive le navigator
        UI_XboxNavigator.Instance.Disable(this);

        await base.Hide(duration);
    }

    // LOW SHOWING
    protected override async Awaitable show_pool(float duration)
    {
        // on affiche tous les éléments
        if (log) { Debug.Log("(UI_InventoryMenu) showing pool : " + Reference); }
        foreach (GameObject ui in ui_elements) { ui.SetActive(true); }

        // refresh pools
        await RefreshItemPools(duration);
        Showed = true;

        // on désactive les inputs.perso
        InputManager.Instance.inputs.perso.Disable();
    }
    protected override async Awaitable hide_pool(float duration)
    {
        await FadeOutAllPools(duration);
        base.hide_pool(duration);
    }

    // ITEM POOL TRANSITIONS
    public async Awaitable RefreshItemPools(float duration = default)
    {
        if (duration == default) { duration = base_transition; }
        if (log) { Debug.Log($"(UI_InventoryMenu) refreshing item pools with duration {duration}"); }


        // on fade out les item pools qui sont vides & fade in ceux qui sont pleins
        foreach (GameObject ui in ui_elements)
        {
            // on récupère l'item pool
            UI_ItemPool item_pool = ui.GetComponentInChildren<UI_ItemPool>();
            if (item_pool == null) { ui.SetActive(true); continue; }
            if (log)
            {
                Debug.Log("(UI_InventoryMenu) investigating ui_itempool " + item_pool.name + $" ({(item_pool.Faded ? "faded" : "visible")}) with "
                + item_pool.Count + " slots and " + item_pool.FullCount + " items slots " + $"and {item_pool.EnabledCount} enabled slots");
            }

            // on regarde si la pool doit être affichée ou non
            if (fade_all_disabled)
            {
                if (item_pool.EnabledCount > 0 && item_pool.Faded) { item_pool.Fade(duration, fade_in: true); }
                else if (item_pool.EnabledCount == 0 && !item_pool.Faded) { item_pool.Fade(duration, fade_in: false); }
            }
            else
            {
                if ((item_pool.EnabledCount > 0 || item_pool.FullCount > 0) && item_pool.Faded) { item_pool.Fade(duration, fade_in: true); }
                else if (item_pool.EnabledCount == 0 && item_pool.FullCount == 0 && !item_pool.Faded) { item_pool.Fade(duration, fade_in: false); }
            }
        }
        await Task.Delay((int)(duration * 1000));
    }
    public async Awaitable FadeOutAllPools(float duration = default)
    {
        if (duration == default) { duration = base_transition; }
        if (log) { Debug.Log($"(UI_InventoryMenu) fading out all item pools with duration {duration}"); }

        // on fade out tous les item pools
        foreach (GameObject ui in ui_elements)
        {
            UI_ItemPool item_pool = ui.GetComponentInChildren<UI_ItemPool>();
            if (item_pool == null || item_pool.Faded) { continue; }
            if (log) { Debug.Log("(UI_InventoryMenu) fading out ui_itempool " + item_pool.name); }
            item_pool.Fade(duration, fade_in: false);
        }
        await Task.Delay((int)(duration * 1000));
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