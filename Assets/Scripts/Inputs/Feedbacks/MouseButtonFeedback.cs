using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// This class is used to give feedback to the player when they are pressing a mouse button
/// </summary>
public class MouseButtonFeedback : InputImageFeedback
{

    [Header("Mouse Button Feedback")]
    [SerializeField] protected string mouse_reference;

    // START
    public override void InitializeWithAction(InputAction action)
    {
        base.InitializeWithAction(action);

        // we get the sprite from the bank
        Sprite sprite = bank.GetMouseFeedbackIcon(mouse_reference);

        // we set the sprite to the image
        image.sprite = sprite;
    }
}