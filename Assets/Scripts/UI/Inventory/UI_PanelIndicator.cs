#pragma warning disable 4014
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles if the indicator of the panel should be shown or not
/// example for the motherboard panel indicator we want it to be shown
/// only when the player has the laptop
/// </summary>
[RequireComponent(typeof(Transitioner))]
public class UI_PanelIndicator : MonoBehaviour
{
    [Header("Indicator parameters")]
    [SerializeField] private string panel_name = "Motherboard";
    [SerializeField] private bool require_laptop = true;
    [SerializeField] private List<UI_ItemPool> required_enabled_item_pools = new List<UI_ItemPool>();

    [Header("Components")]
    private UI_PanelManager panel_manager;
    private Transitioner transitioner;

    [Header("Logs")]
    [SerializeField] private bool log = false;

    // INIT START
    public void InitStart(UI_PanelManager panelManager)
    {
        panel_manager = panelManager;
        transitioner = GetComponent<Transitioner>();

        if (require_laptop) { UI_LaptopItemSlot.Instance.OnItemChanged += HandleLaptopChanged; }

        panel_manager.OnPanelChanged += Refresh;

        if (log) { Debug.Log($"(UI_PanelIndicator) {name} initialized for panel {panel_name}"); }
    }

    // LAPTOP, PANEL CHANGING & INVENTORY REFRESH HANDLING
    protected void HandleLaptopChanged(List<Item> items)
    {
        // if (transitioner == null || panel_manager == null) { return; }
        if (panel_manager.CurrentPanel != panel_name) { return; }
        if (items.Count > 0 && items[0].Reference == "hardware:laptop") { show(); return; }
        hide();
    }
    public void Refresh(string panel,float duration = -99f)
    {
        if (require_laptop && (UI_LaptopItemSlot.Instance == null || !UI_LaptopItemSlot.Instance.HasLaptop)) { return; } // in all cases we are not shown so we don't do anything

        // if the panel is not the one we are on, we hide ourselves
        if (panel != panel_name)
        {
            if (log) { Debug.Log($"(UI_PanelIndicator) refreshing indicator {this.name} for panel {panel} : not our panel, hiding indicator"); }
            hide(duration);
            return;
        }

        // we verify if we don't have any required enabled item pools, we show
        if (required_enabled_item_pools.Count == 0)
        {
            if (log) { Debug.Log($"(UI_PanelIndicator) refreshing indicator {this.name} for panel {panel} : it has no required enabled item pools, showing indicator"); }
            show(duration);
            return;
        }

        // we check if at least one item pools has enabled ui_item we show
        foreach (UI_ItemPool item_pool in required_enabled_item_pools)
        {
            if (item_pool == null) { continue; }
            if (item_pool.EnabledCount > 0)
            {
                if (log) { Debug.Log($"(UI_PanelIndicator) refreshing indicator {this.name} for panel {panel} : enabled item pool found ! showing indicator"); }
                show(duration);
                return;
            }
        }

        // if we did not find any enabled ui_item we hide
        if (log) { Debug.Log($"(UI_PanelIndicator) refreshing indicator {this.name} for panel {panel} : no enabled item_pools found, hiding indicator"); }
        hide(duration);
    }

    // SHOW / HIDE
    private void show(float duration = -99f)
    {
        if (transitioner == null) { return; }
        transitioner.Show(duration);
        transitioner.ShouldBeVisibleOnEnable = true;
    }
    private void hide(float duration = -99f)
    {
        if (transitioner == null) { return; }
        transitioner.Hide(duration);
        transitioner.ShouldBeVisibleOnEnable = false;
    }
}