using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// This class is used to give feedback to the player when they are pressing a button
/// </summary>
public class ButtonFeedback : InputImageFeedback
{

    [Header("Button Feedback")]
    [SerializeField] protected string button_reference;
    [SerializeField] protected bool always_full = false;


    // START
    public override void InitializeWithAction(InputAction action)
    {
        base.InitializeWithAction(action);

        // we get the sprite from the bank
        Sprite sprite = bank.GetInputFeedbackSprite(button_reference, !always_full);

        // we set the sprite to the image
        image.sprite = sprite;
    }

    // INPUT / RESET
    public override void OnInput()
    {
        base.OnInput();

        // we get the sprite from the bank
        Sprite sprite = bank.GetInputFeedbackSprite(button_reference, false);

        // we set the sprite to the image
        image.sprite = sprite;
    }
    public override void OnReset()
    {
        base.OnReset();

        // we get the sprite from the bank
        Sprite sprite = bank.GetInputFeedbackSprite(button_reference, !always_full);

        // we set the sprite to the image
        image.sprite = sprite;
    }

    // SETTERS
    public void SetAlwaysFull(bool new_value = true)
    {
        always_full = new_value;

        // we get the sprite from the bank
        Sprite sprite = bank.GetInputFeedbackSprite(button_reference, !always_full);

        // we set the sprite to the image
        image.sprite = sprite;
    }
}