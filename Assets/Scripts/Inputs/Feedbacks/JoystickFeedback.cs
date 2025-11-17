using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class is used to give feedback to the player when they are pressing a Joystick
/// </summary>
public class JoystickFeedback : InputImageFeedback
{
    [Header("Joystick Feedback")]
    [SerializeField] protected Vector2 joystick_direction;

    // CALLBACKS
    protected override void defineCallbacks()
    {
        // we define the callback
        input_callback =
            ctx =>
            {
                joystick_direction = ctx.ReadValue<Vector2>();
                OnInput();
            };
        reset_callback =
            ctx =>
            {
                joystick_direction = ctx.ReadValue<Vector2>();
                OnReset();
            };

        // and the complexe callback for press & release
        press_and_release_callback = ctx =>
        {
            joystick_direction = ctx.ReadValue<Vector2>();
            if (ctx.phase == InputActionPhase.Performed)
            {
                OnInput();
            }
            else if (ctx.phase == InputActionPhase.Canceled)
            {
                OnReset();
            }
        };
    }

    // ON INPUT / RESET 
    public override void OnInput()
    {
        if (joystick_direction.magnitude <= input_manager.JOYSTICK_MIN_THRESHOLD) { OnReset(); return; }

        base.OnInput();

        // we get the sprite from the bank
        Sprite sprite = bank.GetInputFeedbackSprite(convertDirectionToReference(joystick_direction), false);

        // we set the sprite to the image
        image.sprite = sprite;
    }
    public override void OnReset()
    {
        base.OnReset();

        // we get the sprite from the bank
        Sprite sprite = bank.GetInputFeedbackSprite("joy", true);

        // we set the sprite to the image
        image.sprite = sprite;
    }

    // CONVERT DIRECTION TO REFERENCE
    protected string convertDirectionToReference(Vector2 direction)
    {
        if (direction.magnitude <= input_manager.JOYSTICK_MIN_THRESHOLD) { return "joy"; }

        // we get the angle
        float angle = Vector2.SignedAngle(Vector2.up, direction);

        // we get the reference
        if (angle >= -22.5f && angle < 22.5f) { return "joyU"; }
        if (angle >= 22.5f && angle < 67.5f) { return "joyUL"; }
        if (angle >= 67.5f && angle < 112.5f) { return "joyL"; }
        if (angle >= 112.5f && angle < 157.5f) { return "joyDL"; }
        if (angle >= 157.5f || angle < -157.5f) { return "joyD"; }
        if (angle >= -157.5f && angle < -112.5f) { return "joyDR"; }
        if (angle >= -112.5f && angle < -67.5f) { return "joyR"; }
        if (angle >= -67.5f && angle < -22.5f) { return "joyUR"; }

        return "joy";
    }
}