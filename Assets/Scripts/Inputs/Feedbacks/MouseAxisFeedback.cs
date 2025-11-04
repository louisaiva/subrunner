using UnityEngine;
/// <summary>
/// This class is used to give feedback to the player we are moving the mouse
/// </summary>
public class MouseAxisFeedback : InputFeedback
{

    [Header("Mouse Axis Feedback")]
    [SerializeField] protected string axis_reference = "mouse_move";
    [SerializeField] protected bool vertical = false; // will add _L / _R or _D / _U to the reference to get the right icon
    [SerializeField] protected float input_value = 0f;
    [SerializeField] protected float input_threshold = 5f; // minimal input value to consider the input as valid
    [SerializeField] protected float reset_delay = 0.1f;

    // CALLBACKS
    protected override void defineCallbacks()
    {
        // we define the callback
        input_callback =
            ctx =>
            {
                input_value = ctx.ReadValue<float>();
                OnInput();
            };
        reset_callback =
            ctx =>
            {
                input_value = ctx.ReadValue<float>();
                OnReset();
            };
    }

    // ON INPUT / RESET 
    public override void OnInput()
    {
        if (Mathf.Abs(input_value) <= input_threshold) { reset_later(); return; }

        base.OnInput();

        // we get the sprite from the bank
        image.sprite = bank.GetMouseFeedbackIcon(convertInputToRef(input_value));
    }
    public override void OnReset()
    {
        base.OnReset();

        // we get the sprite from the bank
        image.sprite = bank.GetMouseFeedbackIcon(axis_reference);
    }
    private void reset_later()
    {
        CancelInvoke("OnReset");
        Invoke("OnReset", reset_delay);
    }

    // ON DISABLE
    protected override void OnDisable()
    {
        base.OnDisable();
        CancelInvoke("OnReset");
        OnReset();
    }

    // CONVERT DIRECTION TO REFERENCE
    protected string convertInputToRef(float input)
    {
        if (Mathf.Abs(input) <= input_threshold) { return axis_reference; }

        // we get the string we need to add corresponding to the input + verticality
        string reference = axis_reference;
        if (input > 0)
        {
            reference += vertical ? "_U" : "_R";
        }
        else
        {
            reference += vertical ? "_D" : "_L";
        }

        return reference;
    }

}