using UnityEngine;
/// <summary>
/// This class is used to give feedback to the player when they are pressing a mouse button
/// </summary>
public class MouseButtonFeedback : InputFeedback
{

    [Header("Mouse Button Feedback")]
    [SerializeField] protected string mouse_reference;

    // START
    protected override void Start()
    {
        base.Start();

        // we get the sprite from the bank
        Sprite sprite = bank.GetMouseFeedbackIcon(mouse_reference);

        // we set the sprite to the image
        image.sprite = sprite;
    }
}