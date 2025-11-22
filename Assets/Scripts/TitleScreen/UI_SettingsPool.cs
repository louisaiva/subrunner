#pragma warning disable 4014

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SettingsPool : UI_SlottablePool, Panelable
{
    [Header("Panel Manager")]
    [SerializeField] private UI_PanelManager panel_manager;
    public UI_PanelManager PanelManager => panel_manager;

    [Header("Input Feedbacks Builder")]
    [SerializeField] protected FeedbackPoolBuilder IFB;

    [Header("Components")]
    protected List<UI_PanelButton> panel_buttons;
    protected RectTransform panel_bar;

    // START
    protected void Start()
    {
        UI_Navigator.Instance.OnSlotHoverEnter += update_feedbacks;
    }

    // SHOW
    protected override IEnumerator show_coroutine(List<GameObject> dont_show = null, float duration_override = -1f, bool was_stacked = false)
    {
        // we get the last slot's panel or the general one
        UI_Slot last_slot = slottable.StartingSlot;
        UI_Panel panel;
        if (last_slot != null) { panel = last_slot.GetComponentInParent<UI_Panel>(includeInactive: true); }
        else { panel = panel_manager.GetPanel("general"); }

        // we tween to the panel
        panel_manager.TweenToPanel(panel, 0f);

        yield return base.show_coroutine(dont_show, duration_override, was_stacked);
    }

    protected override IEnumerator enable_coroutine()
    {
        yield return base.enable_coroutine();

        // we enable manually the dedicated UI_PanelButton
        UI_PanelButton panel_btn = get_panel_button(panel_manager.CurrentPanel);
        if (panel_btn != null) { panel_btn.OnPointerEnter(null); panel_btn.OnPointerClick(null); }
        yield return null;
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel_bar);
    }

    // UI_PanelButton GETTER
    private UI_PanelButton get_panel_button(string panel_name)
    {
        // we ensure we have some panel buttons
        if (panel_buttons == null || panel_buttons.Count == 0)
        {
            panel_buttons = new List<UI_PanelButton>();
            panel_bar = transform.Find("panel_bar") as RectTransform;
            for (int i = 0; i < panel_bar.childCount; i++)
            {
                UI_PanelButton button = panel_bar.GetChild(i).GetComponent<UI_PanelButton>();
                if (button == null) { continue; }
                panel_buttons.Add(button);
            }
        }

        // we search for the button with the given panel name
        for (int i = 0; i < panel_buttons.Count; i++)
        {
            UI_PanelButton button = panel_buttons[i];
            if (button == null) { continue; }
            if (button.name == panel_name) { return button; }
        }
        return null;
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