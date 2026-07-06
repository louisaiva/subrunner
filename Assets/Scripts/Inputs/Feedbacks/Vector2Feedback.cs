using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// special type of ManualImage Feedback that adds 8 sprites
/// to the base sprite for having full V2 covered
/// (useful for joystick & mouse movement)
/// </summary>
public class Vector2Feedback : ManualImageFeedback
{
    [SerializeField] protected Sprite[] vec2_sprites = new Sprite[8]; // from U -> UR -> ... -> L -> UL

    [Header("Vector2 Input parameters")]
    [SerializeField] protected Vector2 input_value = Vector2.zero;
    [SerializeField] protected float input_threshold = 5f;
    // minimal input value to consider the input as valid : 5f for mouse is good, -7f to get min threshold for joystick from input manager
    [SerializeField] protected float reset_delay = 0.1f;

    // START & CALLBACKS
    public override void InitializeWithAction(InputAction action)
    {
        base.InitializeWithAction(action);

        // if input_threshold is -7f it means we wants joystick input
        if (input_threshold == -7f)
        {
            input_threshold = InputManager.Instance.JOYSTICK_MIN_THRESHOLD;
        }
    }
    protected override void defineCallbacks()
    {
        // we define the callback
        input_callback =
            ctx =>
            {
                input_value = ctx.ReadValue<Vector2>();
                OnInput();
            };
        reset_callback =
            ctx =>
            {
                input_value = ctx.ReadValue<Vector2>();
                OnReset();
            };
    }

    // ON INPUT / RESET 
    public override void OnInput()
    {
        if (input_value.magnitude <= input_threshold)
        {
            StopCoroutine("reset_later");
            StartCoroutine("reset_later");
            return;
        }

        base.OnInput();

        // we set the sprite to the image
        image.sprite = get_sprite(input_value);
    }
    private IEnumerator reset_later()
    {
        if (reset_delay > 0f) { yield return new WaitForSecondsRealtime(reset_delay); }
        OnReset();
    }

    // LOW LEVEL SPRITE MANAGEMENT
    protected Sprite get_sprite(Vector2 direction)
    {
        float angle = -Vector2.SignedAngle(Vector2.up, direction);
        if (angle < 0f) { angle += 360f; }

        // we divide the circle in 8 parts of 45 degrees
        int index = Mathf.RoundToInt(angle / 45f) % 8;

        return vec2_sprites[index];
    }
}