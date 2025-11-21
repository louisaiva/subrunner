#pragma warning disable 4014

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SettingsPool : UI_SlottablePool, Panelable
{
    [Header("Panel Manager")]
    [SerializeField] private UI_PanelManager panel_manager;
    public UI_PanelManager PanelManager => panel_manager;

    [Header("Input Feedbacks Builder")]
    [SerializeField] protected FeedbackPoolBuilder IFB;
    
    // START
    protected void Start()
    {
        UI_Navigator.Instance.OnSlotHoverEnter += update_feedbacks;
    }

    // SHOW
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

    // UPDATE FEEDBACKS
    private void update_feedbacks(UI_Slot slot)
    {
        if (!Showed) { return; }

        if (slot == null)
        {
            if (log) { Debug.Log($"(UI_SettingsPool) slot is null, disabling activate & slide IF rows"); }
            IFB.DisableRows("activate");
            IFB.DisableRows("slide");
            return;
        }

        if (slot is not UI_SettingSlot)
        {
            IFB.DisableRows("slide");
            IFB.EnableRows("activate");
            IFB.ResetTextOnRows("activate");
            if (log) { Debug.Log($"(UI_SettingsPool) slot {slot.gameObject.name} is not a UI_SettingSlot, activating only activate row"); }
            return;
        }
        
        if (log) { Debug.Log($"(UI_SettingsPool) updating feedbacks for slot {slot.gameObject.name}"); }

        if (slot is UI_ToggleSetting)
        {
            IFB.EnableRows("activate");
            IFB.DisableRows("slide");
            IFB.SetTextOnRows("activate", "toggle");
            return;
        }

        if (slot is UI_Slider)
        {
            IFB.EnableRows("slide");
            IFB.EnableRows("activate",kb: true, gmpd:false); // kb's activate will have slide written on it
            IFB.SetTextOnRows("activate", "slide", kb: true, gmpd: false);
            IFB.DisableRows("activate",kb:false,gmpd:true);
            return;
        }
    }
}