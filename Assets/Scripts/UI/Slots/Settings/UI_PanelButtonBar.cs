// #pragma warning disable 4014
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// this ui class handles a set of UI_PanelButton
/// and can apply multiple modifications to their
/// behaviours, to make them work together well
/// </summary>
public class UI_PanelButtonBar : MonoBehaviour
{

    [Header("Panel Buttons")]
    private List<UI_PanelButton> panel_buttons;
    [SerializeField] private bool get_buttons_in_awake = true;

    [Header("Modifications Behaviours")]
    [SerializeField] private bool show_only_one_hovered_colorer = true;

    [Header("Panel Manager")]
    [SerializeField] private UI_PanelManager panel_manager;

    [Header("Active Colorers")]
    private Dictionary<UI_EventButton, List<UI_Colorer>> active_colorers = new Dictionary<UI_EventButton, List<UI_Colorer>>();
    private Dictionary<UI_EventButton, List<UI_Colorer>> waiting_to_reactivate_colorers = new Dictionary<UI_EventButton, List<UI_Colorer>>();

    [Header("Logs")]
    [SerializeField] private bool log = false;

    // AWAKE & DESTROY
    private void Awake()
    {
        if (panel_manager == null)
        {
            Debug.LogError("(UI_PanelButtonBar) No panel manager set, please assign one in the inspector.");
            return;
        }

        // we get the buttons
        if (get_buttons_in_awake)
        {
            panel_buttons = new List<UI_PanelButton>(GetComponentsInChildren<UI_PanelButton>(includeInactive: true));
        }
        else if (panel_buttons == null) { panel_buttons = new List<UI_PanelButton>(); }

        // we register to the buttons events
        for (int i = 0; i < panel_buttons.Count; i++)
        {
            panel_buttons[i].OnColored += on_button_colored;
            panel_buttons[i].OnUncolored += on_button_uncolored;
        }
    }
    private void OnDestroy()
    {
        // we unregister to the buttons events
        for (int i = 0; i < panel_buttons.Count; i++)
        {
            panel_buttons[i].OnColored -= on_button_colored;
            panel_buttons[i].OnUncolored -= on_button_uncolored;
        }
    }


    // CALLBACKS
    private void on_button_colored(UI_EventButton button, List<UI_Colorer> colorers)
    {
        if (!show_only_one_hovered_colorer) { return; }

        // we check if we already have some buttons colored
        if (active_colorers.Count <= 0)
        {
            active_colorers.Add(button, colorers);
            return;
        }

        if (active_colorers.ContainsKey(button)) { return; }

        if (log) { Debug.Log($"(UI_PanelButtonBar) multiple buttons colored : {button.name} + {string.Join(", ", active_colorers.Keys)}"); }

        // if we have already some, and that the UI_EventButton previously colored has its panel shown,
        // then we want to disable its colorers and move them into the waiting pool
        foreach (KeyValuePair<UI_EventButton, List<UI_Colorer>> entry in active_colorers)
        {
            UI_EventButton active_button = entry.Key;
            List<UI_Colorer> active_colorers_list = entry.Value;

            // we check if the button's panel is currently shown
            if (active_button is UI_PanelButton panel_button && panel_manager.IsCurrentPanel(panel_button.TargetPanel))
            {
                // we disable its colorers and move them to the waiting pool
                foreach (UI_Colorer colorer in active_colorers_list) { colorer.RevertColor(); }
                waiting_to_reactivate_colorers.Add(active_button, active_colorers_list);
                if (log) { Debug.Log($"(UI_PanelButtonBar) disabling colorers of button {active_button.name} and moving them to the waiting pool"); }
            }
        }
        active_colorers.Clear();

        // we add the new button to the active pool
        active_colorers.Add(button, colorers);
    }
    private void on_button_uncolored(UI_EventButton button, List<UI_Colorer> colorers)
    {
        if (!show_only_one_hovered_colorer) { return; }

        // we check if we have this button in our active colorers
        if (active_colorers.ContainsKey(button)) { active_colorers.Remove(button); }

        // or in the waiting colorers
        if (waiting_to_reactivate_colorers.ContainsKey(button)) { waiting_to_reactivate_colorers.Remove(button); }

        // now we check if the active dic is empty (should be) and if
        // we have waitings colorers -> if yes we reactivate them
        if (active_colorers.Count <= 0 && waiting_to_reactivate_colorers.Count > 0)
        {
            foreach (KeyValuePair<UI_EventButton, List<UI_Colorer>> entry in waiting_to_reactivate_colorers)
            {
                List<UI_Colorer> colorers_to_reactivate = entry.Value;
                foreach (UI_Colorer colorer in colorers_to_reactivate) { colorer.ApplyColor(entry.Key.HoverColor); }

                // we also move them back to the active pool
                active_colorers.Add(entry.Key, colorers_to_reactivate);

                if (log) { Debug.Log($"(UI_PanelButtonBar) reactivating colorers of button {entry.Key.name} from the waiting pool and moving them to the active pool"); }
            }
            waiting_to_reactivate_colorers.Clear();
        }
    }

}