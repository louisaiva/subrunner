#pragma warning disable 4014

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SettingsPool : UI_SlottablePool
{
    [Header("Panel Manager")]
    [SerializeField] private UI_PanelManager panel_manager;

    // enable pool
    protected override IEnumerator show_coroutine(List<GameObject> dont_show = null, float duration_override = -1f, bool was_stacked = false)
    {
        // we get the last slot
        UI_Slot last_slot = slottable.StartingSlot;
        if (last_slot != null)
        {
            // we get the panel of this slot
            UI_Panel slot_panel = last_slot.GetComponentInParent<UI_Panel>(includeInactive: true);
            if (slot_panel != null) { panel_manager.TweenToPanel(slot_panel,0f); }        
        }
        else
        {
            UI_Panel default_panel = panel_manager.GetPanel("general");
            if (default_panel != null) { panel_manager.TweenToPanel(default_panel,0f); }
        }

        yield return base.show_coroutine(dont_show, duration_override, was_stacked);
    }
}