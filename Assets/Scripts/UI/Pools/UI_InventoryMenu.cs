#pragma warning disable 4014
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class UI_InventoryMenu : UI_Pool
{
    private List<GameObject> saved_slots = new List<GameObject>();
    [Header("Inventory Menu Components")]
    [SerializeField] private UI_Inventory ui_inventory;
    [SerializeField] private UI_Inventory ui_laptop;
    [SerializeField] private UI_XboxNavigator navigator;

    [Header("No Inventory")]
    [SerializeField] private Transform no_inventory_panel;

    [Header("Base Item Pool Transitions")]
    [SerializeField] private float base_transition = 0.2f;
    [SerializeField] private bool fade_all_disabled = true;

    protected void Awake()
    {
        // on récupère le navigator
        navigator = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();

        // on récupère les composants
        ui_inventory = GetComponent<UI_Inventory>();
        if (ui_inventory == null)
        {
            Debug.LogError("(UI_InventoryMenu) missing ui_inventory on " + name);
        }
        ui_laptop = transform.Find("ui_laptop").GetComponent<UI_Inventory>();
        if (ui_laptop == null)
        {
            Debug.LogError("(UI_InventoryMenu) missing ui_laptop on " + name);
        }

        // we save the current ui_elements state in saved_state
        saved_slots = new List<GameObject>(ui_elements);
    }

    public override async Awaitable Show(float duration)
    {
        // vérifie si on a des items dans notre inventaire
        ui_elements.Clear();
        if (ui_inventory.Inventory.Count == 0) { ui_elements.Add(no_inventory_panel.gameObject); }
        else { ui_elements.AddRange(saved_slots); }

        await base.Show(duration);
        if (ui_inventory.Inventory.Count == 0) { return; }

        // on active le navigator
        navigator.Enable(ui_inventory);

        // on regarde si le perso a le laptop
        if (ui_inventory.Inventory.GetItem("hardware:laptop") != null)
        {
            ui_laptop.gameObject.SetActive(true);
            navigator.Enable(ui_laptop);
        }
        else { ui_laptop.gameObject.SetActive(false); }

        // on met à jour l'angle treshold du navigator
        navigator.angle_threshold = base.angle_threshold;
    }
    public override async Awaitable Hide(float duration)
    {
        // no_inventory_panel.gameObject.SetActive(false);

        // on désactive le navigator
        navigator.Disable(ui_inventory);
        navigator.Disable(ui_laptop);

        await base.Hide(duration);
    }
    
    protected override async Awaitable show_pool(float duration)
    {
        // on affiche tous les éléments
        if (log) { Debug.Log("(UI_InventoryMenu) showing pool : " + Reference); }
        foreach (GameObject ui in ui_elements)
        {
            ui.SetActive(true);
        }
        await RefreshItemPools(duration);
        Showed = true;

        // s'il a une activate action, on désactive les inputs.perso
        if (UsePersoInputs) { InputManager.Instance.inputs.perso.Enable(); }
        else { InputManager.Instance.inputs.perso.Disable(); }
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
}