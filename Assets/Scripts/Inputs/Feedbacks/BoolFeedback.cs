using UnityEngine;
/// <summary>
/// special type of ManualImage Feedback that adds 1 sprite
/// to the base sprite for having a simple bool feedback
/// (useful for mouse buttons for example)
/// </summary>
public class BoolFeedback : ManualImageFeedback
{
    [SerializeField] protected Sprite clicked_sprite;

    // ON INPUT / RESET 
    public override void OnInput()
    {
        base.OnInput();

        // we set the sprite to the image
        image.sprite = clicked_sprite;
    }
}