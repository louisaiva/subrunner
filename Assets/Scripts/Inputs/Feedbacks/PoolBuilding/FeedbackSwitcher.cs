using UnityEngine;

// todo rename into "InputsSwitcher"
public class FeedbackSwitcher : MonoBehaviour
{
    
    [Header("Feedback GameObjects")]
    [SerializeField] private GameObject keyboardFeedback;
    [SerializeField] private GameObject gamepadFeedback;
    [SerializeField] private string show_feedback_setting_name = "show_input_feedbacks";
    private Setting show_feedbacks;

    // START
    private void Start()
    {
        // register to setting
        show_feedbacks = SettingsManager.Instance.GetSetting(show_feedback_setting_name);
        if (show_feedbacks != null)
        {
            show_feedbacks.OnValueChanged += toggle_feedbacks;
            // Initial toggle based on setting value
            toggle_feedbacks(show_feedbacks.Value);
        }
    }
    private void OnDestroy()
    {
        if (show_feedbacks != null) { show_feedbacks.OnValueChanged -= toggle_feedbacks; }
    }
    private void toggle_feedbacks(float value)
    {
        switch_feedback(InputManager.Instance.CurrentInputType);
    }

    // SUBSCRIBE / UNSUBSCRIBE TO INPUT TYPE CHANGES
    void OnEnable()
    {
        InputManager.Instance.OnInputTypeChanged += switch_feedback;
        
        // Initial switch based on current input type
        switch_feedback(InputManager.Instance.CurrentInputType);
    }
    void OnDisable()
    {
        // Unsubscribe from input type change event
        InputManager.Instance.OnInputTypeChanged -= switch_feedback;
    }

    // METHOD TO SWITCH FEEDBACK BASED ON INPUT TYPE
    private void switch_feedback(string input_type)
    {
        if (show_feedbacks != null && show_feedbacks.Value <= 0.5f)
        {
            // If feedbacks are disabled in settings, turn both off
            keyboardFeedback.SetActive(false);
            gamepadFeedback.SetActive(false);
            return;
        }

        keyboardFeedback.SetActive(input_type == "keyboard");
        gamepadFeedback.SetActive(input_type == "gamepad");
    }
}