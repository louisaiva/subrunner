#pragma warning disable 4014
using UnityEngine;
using PrimeTween;
using System.Collections.Generic;
using System;


public class UI_PanelManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private string current_panel = "ui_inventory";
    [SerializeField] private string destination = "";
    public string CurrentPanel
    {
        get
        {
            if (destination != "") { return destination; }
            return current_panel;
        }
    }
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
        UI_Navigator.Instance.OnSlotOutOfScreen += TweenToSlot;

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

        // initialize the sequence list to keep track of sequences happening
        sequences = new List<Sequence?>();
        foreach (UI_Panel panel in panels) { sequences.Add(null); }

        // we tween to the current panel
        TweenToPanel(get_panel(current_panel));
    }

    // ROLLING & TWEENING HIGH LEVEL
    private void TweenToSlot(UI_Slot slot)
    {
        // Handle the case when a slot is out of screen
        string log_msg = "";
        log_msg += $"(UI_PanelManager) Slot {slot.gameObject.name} is out of screen";
        log_msg += $"\n\t current panel is {current_panel}";
        log_msg += $"\n\t is an ui_item ? {slot is UI_Item}";

        // we check if it's a UI_Item & if it belongs to one of our panels
        // if (slot is not UI_Item uiItem || uiItem.ItemPool == null) { if (log) { Debug.Log(log_msg); } return; }

        // checks if it has a UI_Panel in its above hierarchy
        UI_Panel ui_panel = slot.GetComponentInParent<UI_Panel>(/* includeInactive: true */);
        log_msg += $"\n\t has an ui_panel ? {ui_panel != null}";
        if (ui_panel == null || !panels.Contains(ui_panel)) { if (log) { Debug.Log(log_msg); } return; }

        // we switch to the panel
        TweenToPanel(ui_panel);
    }
    public void RollPanel(int direction = 1)
    {
        // if (HasRunningSequence) { return; }
        // todo faire en sorte qu'on annule sequence et qu'on en recrée une adéquate
        // todo type si on est en train de bouger pour aller au panel inventory bah on continue vers le laptop

        // direction > 0 means we scroll up
        // direction < 0 we scroll down


        // get current panel
        UI_Panel currentPanel = get_panel(CurrentPanel);
        if (currentPanel == null) { Debug.LogError($"(UI_PanelManager) Current panel not found: {CurrentPanel}"); return; }

        // get next panel
        UI_Panel targetPanel = null;
        int next_index = panels.IndexOf(currentPanel) - direction;
        next_index = Mathf.Clamp(next_index, 0, panels.Count - 1);
        targetPanel = panels[next_index];
        if (targetPanel == currentPanel) { return; } // we are already on/moving to the target panel

        if (log) { Debug.Log($"(UI_PanelManager) Rolling : {currentPanel.name} --> {targetPanel.name}"); }

        // tween to it
        TweenToPanel(targetPanel);
    }
    public void TweenToPanel(string panel_name)
    {
        UI_Panel targetPanel = get_panel(panel_name);
        if (targetPanel == null)
        {
            Debug.LogError($"(UI_PanelManager) TweenToPanel failed: panel not found: {panel_name}");
            return;
        }
        TweenToPanel(targetPanel);
    }

    // REFRESH
    public void RefreshIndicators(float duration = -99f)
    {
        if (indicators == null || indicators.Count == 0) { return; }
        if (duration == -99f) { duration = default_duration; }

        // refresh all indicators
        foreach (UI_PanelIndicator indicator in indicators)
        {
            indicator.Refresh(CurrentPanel, duration);
        }
    }

    // TWEENING
    private List<Sequence?> sequences = new List<Sequence?>();
    private bool HasRunningSequence
    {
        get
        {
            foreach (Sequence? seq in sequences)
            {
                if (seq != null && seq.Value.isAlive) { return true; }
            }
            return false;
        }
    }
    public async Awaitable TweenToPanel(UI_Panel targetPanel, float duration = -99f)
    {
        int targetIndex = panels.IndexOf(targetPanel);
        if (targetPanel == null || panels.Count == 0) { return; }

        if (duration == -99f) { duration = default_duration; }

        if (log) { Debug.Log($"(UI_PanelManager) Tweening to panel: {targetPanel.name}"); }

        // we call the event
        OnPanelChanged?.Invoke(targetPanel.name, duration);
        destination = targetPanel.name;

        for (int i = 1; i < panels.Count; i++)
        {
            TweenPanelToPositionIndex(i, targetIndex, duration);
        }
        await TweenPanelToPositionIndex(0, targetIndex, duration);
        if (HasRunningSequence) { return; } // if we still have a running sequence it means that we are still tweening so another TweenToPanel() was called during this one
        current_panel = targetPanel.name;
        destination = "";
        if (log) { Debug.Log($"(UI_PanelManager) tweened to panel: {targetPanel.name} with success !!!"); }
    }
    private async Awaitable TweenPanelToPositionIndex(int panel_index, int destination_index, float duration = -99f)
    {
        UI_Panel ui_panel = panels[panel_index];

        if (destination_index < 0 || destination_index >= ui_panel.anchors.Count) { return; }
        Vector2 target_anchor = ui_panel.anchors[destination_index];
        RectTransform panel = ui_panel.panelTransform;

        // get the ease if we want to show ourself we get the nice ease
        Ease ease = (ui_panel == panels[destination_index]) ? showing_ease : hiding_ease;

        if (duration == -99f) { duration = default_duration; }

        if (log) { Debug.Log($"(UI_PanelManager) Tweening panel {ui_panel.name} to anchors : {target_anchor}"); }

        // we stop the last sequence
        if (sequences[panel_index] != null && sequences[panel_index].Value.isAlive)
        {
            sequences[panel_index].Value.Stop();
            sequences[panel_index] = null;
        }

        // we tween the anchorMin & anchorMax
        sequences[panel_index] = Sequence.Create(useUnscaledTime: true)
           .Group(Tween.Custom(panel.anchorMin.x, /* start -> end */ target_anchor.x, duration,
               onValueChange: ctx => panel.anchorMin = new Vector2(ctx, panel.anchorMin.y)))
           .Group(Tween.Custom(panel.anchorMax.x, /* start -> end */ target_anchor.y, duration,
               onValueChange: ctx => panel.anchorMax = new Vector2(ctx, panel.anchorMax.y)));

        while (sequences[panel_index].Value.isAlive) { await System.Threading.Tasks.Task.Yield(); }
    }

    // LOW GETTERS
    private UI_Panel get_panel(string panelName)
    {
        for (int i = 0; i < panels.Count; i++)
        {
            if (panels[i].name == panelName) { return panels[i]; }
        }
        return null;
    }
    public UI_Panel GetPanel(string panelName) => get_panel(panelName);
}
