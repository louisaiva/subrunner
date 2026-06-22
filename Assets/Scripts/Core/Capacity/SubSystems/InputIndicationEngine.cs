using UnityEngine;
using System;
using System.Collections.Generic;

public class InputIndicationEngine : MonoBehaviour
{
    [Header("Input Indication Capacity")]
    [SerializeField] private Transform input_indication;
    [SerializeField] private InputIndicationCapacity current_iic = null;

    [Header("UI_EventFeedbacks")]
    [SerializeField] private List<UI_EventButton> IFs;

    public Loggable<InputIndicationEngine> log;
    
    // SETTING & CALLBACK
    private void Start()
    {
        SettingsManager.Instance.RegisterCallback("show_interaction_feedbacks", ToggleEngine);
    }
    private void OnDestroy()
    {
        SettingsManager.Instance.UnregisterCallback("show_interaction_feedbacks", ToggleEngine);
    }
    private void ToggleEngine(Setting setting) { gameObject.SetActive(setting.Value > 0.5f); }
    
    private void Update()
    {
        if (input_indication == null) { return; }
        if (current_iic == null) { return; }

        // update position
        input_indication.position = current_iic.transform.position;
    }

    // MAIN METHODS
    public void OnCapableHovered(Capable capable)
    {
        if (input_indication == null) { return; }

        // we get the IF data from the capable
        if (!capable.TryGetCapacity(out current_iic))
        {
            log.LogExtended($"No InputIndicationCapacity found on {capable.ID}, can't show input indication");
            return;
        }

        // we set the IF position and color
        input_indication.position = current_iic.transform.position;
        foreach (UI_EventButton IF in IFs)
        {
            IF.SetColor(current_iic.Color);
        }

        // todo : we set the feedback action

        // we show the IF
        input_indication.gameObject.SetActive(true);
        log.Log($"Showing input indication for {capable.ID} with input {current_iic.InputName} and color {current_iic.Color}");
    }
    public void OnCapableHoverLost(Capable capable)
    {
        if (input_indication == null) { return; }

        // we hide the IF
        input_indication.gameObject.SetActive(false);
        log.Log($"Hiding input indication for {capable.ID}");
    }
}