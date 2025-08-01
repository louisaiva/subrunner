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
    [SerializeField] private bool require_laptop = true;

    [Header("Components")]
    /* [SerializeField]  */private UI_PanelManager panel_manager;
    [SerializeField] private string panel_name = "Motherboard";
    private Transitioner transitioner;

    [Header("Logs")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_laptop = false;

    // AWAKE START
    public void InitStart(UI_PanelManager panelManager)
    {
        panel_manager = panelManager;
        transitioner = GetComponent<Transitioner>();

        if (require_laptop) { UI_LaptopItemSlot.Instance.OnItemChanged += HandleLaptopChanged; }

        panel_manager.OnPanelChanged += HandlePanelChanged;

        if (log) { Debug.Log($"(UI_PanelIndicator) {name} initialized for panel {panel_name}"); }
    }

    // LAPTOP & PANEL CHANGING HANDLING
    protected void HandleLaptopChanged(List<Item> items)
    {
        if (log) { Debug.Log($"(UI_PanelIndicator) Laptop handle called on {name}, we have :\n transitioner :{transitioner}\n panel_manager :{panel_manager} (panel : {panel_manager.CurrentPanel})"); }
        if (transitioner == null || panel_manager == null) { return; }
        if (panel_manager.CurrentPanel != panel_name) { return; }
        if (items.Count > 0 && items[0].Reference == "hardware:laptop")
        {
            if (log) { Debug.Log($"(UI_PanelIndicator) Laptop grabbed for {panel_name}, showing indicator"); }
            transitioner.Show();
            transitioner.ShouldBeVisibleOnEnable = true;
            return;
        }
        if (log) { Debug.Log($"(UI_PanelIndicator) Laptop dropped for {panel_name}, hiding indicator"); }
        transitioner.Hide();
        transitioner.ShouldBeVisibleOnEnable = false;

        /* // otherwise we have no laptop -> we hide the indicator
        if (panel_manager.CurrentPanel == panel_name)
        {
            transitioner.Hide();
            transitioner.ShouldBeVisibleOnEnable = false;
        }

            HasLaptop = false;
        if (!Faded) { Fade(fade_in: false); }
        
        // if (log_laptop) { Debug.Log($"(UI_PanelIndicator) Checking laptop presence for {panel_name} : {UI_LaptopItemSlot.Instance.HasLaptop}"); }

        // CHECK THAT CURRENT PANEL IS OURS
        if (panel_manager.CurrentPanel != panel_name)
        {
            if (!transitioner.Hidden)
            {
                transitioner.Hide();
                transitioner.ShouldBeVisibleOnEnable = false;
            }
            return;
        } */
    }
    protected void HandlePanelChanged(string new_panel_name,float duration = default)
    {
        if (transitioner == null || UI_LaptopItemSlot.Instance == null) { return; }
        if (require_laptop && !UI_LaptopItemSlot.Instance.HasLaptop) { return; } // in all cases we are not shown so we don't do anything

        if (new_panel_name == panel_name)
        {
            if (log) { Debug.Log($"(UI_PanelIndicator) Panel changed to {new_panel_name}, showing indicator"); }
            transitioner.Show(duration);
            transitioner.ShouldBeVisibleOnEnable = true;
            return;
        }
        if (log) { Debug.Log($"(UI_PanelIndicator) Panel changed to {new_panel_name}, hiding indicator"); }
        transitioner.Hide(duration);
        transitioner.ShouldBeVisibleOnEnable = false;
        

        /* // CHECKS HAS LAPTOP
        if (require_laptop)
        {
            if (!transitioner.Hidden && !UI_LaptopItemSlot.Instance.HasLaptop)
            {
                transitioner.Hide();
                transitioner.ShouldBeVisibleOnEnable = false;
            }
            else if (!transitioner.Shown && UI_LaptopItemSlot.Instance.HasLaptop)
            {
                transitioner.Show();
                transitioner.ShouldBeVisibleOnEnable = true;
            }
            return;
        }

        // OTHERWISE IT'S OUR PANEL -> WE SHOW IT
        if (!transitioner.Shown)
        {
            transitioner.Show();
            transitioner.ShouldBeVisibleOnEnable = true;
        } */
    }
}