using UnityEngine;

/// <summary>
/// This class is used to give feedback to the player when they are pressing a key
/// works with the new UI_Button class which is a slot used to render the pressing
/// it works by transmitting the event to the ui_button component
/// </summary>
[RequireComponent(typeof(UI_Button))]
public class EventFeedback : InputFeedback
{
    [Header("UI Button Feedback")]
    private UI_Button _ui_button;
    private UI_Button ui_button
    {
        get
        {
            if (_ui_button != null) { return _ui_button; }
            _ui_button = GetComponent<UI_Button>();
            return _ui_button;
        }
    }

    // INPUT / RESET
    public override void OnInput()
    {
        base.OnInput();
        ui_button.OnPointerEnter(null);
    }
    public override void OnReset()
    {
        base.OnReset();
        ui_button.OnPointerExit(null);
    }
}