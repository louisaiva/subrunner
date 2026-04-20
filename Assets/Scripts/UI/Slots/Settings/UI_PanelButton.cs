#pragma warning disable 4014
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 
/// this specific button is used for working with UI_Panel.
/// when the UI_Panel is shown, the button is disabled BUT hovered,
/// which makes it not clickable (but this is what we want since we are
/// already in the panel)
/// 
/// then when the panel manager switches to another panel, this button
/// enables AND unhovers itself.
/// 
/// then when we go back to the button and click on it it tweens to the right panel
/// 
/// and when we go back to the right panel it also disabled + hover it self
/// 
/// </summary>
public class UI_PanelButton : UI_EventButton
{
    [Header("Panel Settings")]
    public Sprite panel_sprite;
    [SerializeField] private UI_Panel target_panel;
    public UI_Panel TargetPanel => target_panel;
    [SerializeField] private UI_PanelManager panel_manager;


    // START
    protected override void Start()
    {
        base.Start();

        // we register to the panel manager events
        if (panel_manager == null)
        {
            if (log) { Debug.LogWarning("(UI_PanelButton) No panel manager set, we try to get one from the parent"); }
            return;
        }
        panel_manager.OnPanelChanged += on_panel_changed;
    }
    
    // ON PANEL CHANGED
    protected virtual void on_panel_changed(string panel_name, float _)
    {
        // checks if our panel has the same name
        if (target_panel.name == panel_name)
        {
            // we disable and hover the button
            Disable();
            OnPointerEnter(null);
            image.sprite = panel_sprite;
            UI_Navigator.Instance?.UpdateSlots();
            if (log) { Debug.Log($"(UI_PanelButton) panel changed to {panel_name}, disabling and hovering button"); }
            return;
        }

        // else we enable and unhover the button
        Enable();
        base.OnPointerExit(null); // base otherwise the this.OnPointerExit(null) may return early, which we don't want
        if (log) { Debug.Log($"(UI_PanelButton) panel changed to {panel_name}, enabling and unhovering button"); }
    }

    // POINTER HANDLER

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        // if we are not on our panel right now, it means
        // that another ui_panelbutton is disabled but HOVERED
        // -> its colorer is shown.
        // we want to hide all 
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // we tween to the target panel
        panel_manager.TweenToPanel(target_panel);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        // we don't exit if we are still the current panel
        if (panel_manager.CurrentPanel == target_panel.name) { return; }
        
        // otherwise we exit
        base.OnPointerExit(eventData);
    }
}