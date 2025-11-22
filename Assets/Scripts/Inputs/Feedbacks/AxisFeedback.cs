using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// special type of ManualImage Feedback that adds 2 sprites
/// to the base sprite for having full axis cover
/// (useful for mouse scrolling)
/// </summary>
public class AxisFeedback : ManualImageFeedback
{
    [SerializeField] protected Sprite positive_sprite;
    [SerializeField] protected Sprite negative_sprite;

    [Header("Axis Input parameters")]
    [SerializeField] protected float input_value = 0f;
    [SerializeField] protected float input_threshold = 5f;
    // minimal input value to consider the input as valid : 5f for mouse is good, -7f to get min threshold for joystick from input manager
    [SerializeField] protected float reset_delay = 0.1f;

    // START & CALLBACKS
    protected override void Start()
    {
        base.Start();

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
        if (Mathf.Abs(input_value) <= input_threshold)
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
    protected Sprite get_sprite(float direction)
    {
        if (log) { Debug.Log("(AxisFeedback) get_sprite : direction = " + direction); }
        if (direction > 0f)
        {
            return positive_sprite;
        }
        else
        {
            return negative_sprite;
        }
    }
}