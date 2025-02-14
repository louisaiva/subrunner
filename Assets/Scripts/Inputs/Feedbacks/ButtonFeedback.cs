using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// This class is used to give feedback to the player when they are pressing a button
/// </summary>
public class ButtonFeedback : InputFeedback
{

    [Header("Button Feedback")]
    [SerializeField] protected string button_reference;
    [SerializeField] protected bool always_full = false;
    
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
}