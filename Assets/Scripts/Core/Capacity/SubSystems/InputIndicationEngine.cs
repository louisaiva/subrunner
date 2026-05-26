using UnityEngine;
using System;

public class InputIndicationEngine : MonoBehaviour
{
    // REFERENCES
    [SerializeField] private Transform input_indication;
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
    
    // MAIN METHODS
    public void OnCapableHovered(Capable capable)
    {
        if (input_indication == null) { return; }

        // we get the IF data from the capable
        InputIndicationCapacity iic = capable.GetCapacity<InputIndicationCapacity>();
        if (iic == null)
        {
            log.LogExtended($"No InputIndicationCapacity found on {capable.ID}, can't show input indication");
            return;
        }

        // we set the IF position and color
        input_indication.position = iic.transform.position;

        // we show the IF
        input_indication.gameObject.SetActive(true);
        log.Log($"Showing input indication for {capable.ID} with input {iic.InputName} and color {iic.Color}");
    }
    public void OnCapableHoverLost(Capable capable)
    {
        if (input_indication == null) { return; }

        // we hide the IF
        input_indication.gameObject.SetActive(false);
        log.Log($"Hiding input indication for {capable.ID}");
    }
}