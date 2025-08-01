#pragma warning disable 4014
using UnityEngine;
using PrimeTween;
using System.Collections.Generic;
using System;


public class UI_PanelManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private string current_panel = "ui_inventory";
    public string CurrentPanel { get => current_panel; }
    [SerializeField] private List<UI_Panel> panels;
    public event Action<string,float> OnPanelChanged = delegate { };

    [Header("Panels Indicators")]
    [SerializeField] private List<UI_PanelIndicator> indicators = new List<UI_PanelIndicator>();

    [Header("Eases")]
    [SerializeField] private Ease showing_ease = Ease.Default;
    [SerializeField] private Ease hiding_ease = Ease.Default;
    private float default_duration = 0.2f;

    [Header("Logs")]
    [SerializeField] private bool log = false;

    // AWAKE
    private void Start()
    {
        UI_XboxNavigator.Instance.OnSlotOutOfScreen += HandleSlotOutOfScreen;

        if (panels == null || panels.Count == 0)
        {
            Debug.LogWarning("(UI_PanelManager) No panels defined in the inspector.");
            return;
        }

        // Initialize each panel indicator
        foreach (UI_PanelIndicator indicator in indicators)
        {
            if (indicator == null) { continue; }
            indicator.InitStart(this);
        }

        // we tween to the current panel
        TweenToPanel(get_panel(current_panel));
    }

    // HANDLER
    private async void HandleSlotOutOfScreen(I_UI_Slot slot)
    {
        // Handle the case when a slot is out of screen
        string log_msg = "";
        log_msg += $"(UI_PanelManager) Slot {slot.gameObject.name} is out of screen";
        log_msg += $"\n\t current panel is {current_panel}";
        log_msg += $"\n\t is an ui_item ? {slot is UI_Item}";

        // we check if it's a UI_Item
        if (slot is not UI_Item uiItem || uiItem.ItemPool == null) { if (log) { Debug.Log(log_msg); } return; }
        UI_Panel ui_panel = uiItem.ItemPool.GetComponentInParent<UI_Panel>();
        log_msg += $"\n\t has an ui_panel ? {ui_panel != null}";
        if (ui_panel == null || !panels.Contains(ui_panel)) { if (log) { Debug.Log(log_msg); } return; }

        // get the ui_inventory name
        /* string inventoryName = ui_inventory.name;
        log_msg += $"\n\t inventory is {ui_inventory.name} " + (ui_inventory.name == current_panel ? " (current)" : "");
        if (log) { Debug.Log(log_msg); }
        if (current_panel == inventoryName) { return; } */
        
        // we switch to the inventory panel
        await TweenToPanel(ui_panel);
        current_panel = ui_panel.name;
        if (log) { Debug.Log($"(UI_PanelManager) switched to panel: {ui_panel.name} with success !!!"); }
    }

    // TWEENING
    public async Awaitable TweenToPanel(UI_Panel targetPanel, float duration = default)
    {
        int targetIndex = panels.IndexOf(targetPanel);
        if (targetPanel == null || panels.Count == 0) { return; }

        if (duration == default) { duration = default_duration; }

        if (log) { Debug.Log($"(UI_PanelManager) Tweening to panel: {targetPanel.name}"); }

        // we call the event
        OnPanelChanged?.Invoke(targetPanel.name, duration);

        for (int i = 1; i < panels.Count; i++)
        {
            TweenPanelToPositionIndex(panels[i], targetIndex, duration);
        }
        await TweenPanelToPositionIndex(panels[0], targetIndex, duration);
    }
    private async Awaitable TweenPanelToPositionIndex(UI_Panel ui_panel, int index, float duration = default)
    {
        if (index < 0 || index >= ui_panel.anchors.Count) { return; }
        Vector2 target_anchor = ui_panel.anchors[index];
        RectTransform panel = ui_panel.panelTransform;

        // get the ease if we want to show ourself we get the nice ease
        Ease ease = (ui_panel == panels[index]) ? showing_ease : hiding_ease;

        if (duration == default) { duration = default_duration; }

        if (log) { Debug.Log($"(UI_PanelManager) Tweening panel {ui_panel.name} to anchors : {target_anchor}"); }

        // we tween the anchorMin & anchorMax
        await Sequence.Create(useUnscaledTime: true)
            .Group(Tween.Custom(panel.anchorMin.x, /* start -> end */ target_anchor.x, duration,
                onValueChange: ctx => panel.anchorMin = new Vector2(ctx, panel.anchorMin.y)))
            .Group(Tween.Custom(panel.anchorMax.x, /* start -> end */ target_anchor.y, duration,
                onValueChange: ctx => panel.anchorMax = new Vector2(ctx, panel.anchorMax.y)));

    }

    // LOW GETTERS
    private UI_Panel get_panel(string panelName)
    {
        return panels.Find(p => p.name == panelName);
    }
}
