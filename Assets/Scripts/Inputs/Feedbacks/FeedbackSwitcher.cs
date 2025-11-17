using UnityEngine;

public class FeedbackSwitcher : MonoBehaviour
{
    
    [Header("Feedback GameObjects")]
    [SerializeField] private GameObject keyboardFeedback;
    [SerializeField] private GameObject gamepadFeedback;


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
        keyboardFeedback.SetActive(input_type == "keyboard");
        gamepadFeedback.SetActive(input_type == "gamepad");
    }
}