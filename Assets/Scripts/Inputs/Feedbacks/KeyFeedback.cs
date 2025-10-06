using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// This class is used to give feedback to the player when they are pressing a key
/// (on keyboard but also with gamepad in particular cases)
/// </summary>
public class KeyFeedback : InputFeedback
{

    [Header("Key Feedback")]
    [SerializeField] protected string key_reference;
    [SerializeField] protected bool upper_case = false;
    [SerializeField] protected bool dark = false;
    public bool UsingIcon = false; // whether the KF is using a text ("B", "X") or an icon (left icon, right icon, enter, ...)

    [Header("Text Movement")]
    // todo : would be better to calculate dynamically how much we need the text to go down
    [SerializeField] protected Vector2 text_movement = new Vector2(0f, -2f); // how much to move the text when pressed

    [Header("Components")]
    [SerializeField] protected Image icon;
    [SerializeField] protected TextMeshProUGUI key_label;


    // START
    protected override void Start()
    {
        base.Start();

        // we get the sprite from the bank
        Sprite sprite = bank.GetInputFeedbackSprite(dark ? "key_dark" : "key_light");

        // we set the sprite to the image
        image.sprite = sprite;

        // we check if we can have an icon from the bank
        sprite = bank.GetKeyFeedbackIcon(key_reference);
        if (sprite != null)
        {
            icon.sprite = sprite;
            icon.gameObject.SetActive(true);
            key_label.gameObject.SetActive(false);
            UsingIcon = true;
            return;
        }

        // we do not have any icon, we use the text
        icon.gameObject.SetActive(false);
        key_label.gameObject.SetActive(true);
        key_label.text = upper_case ? key_reference.ToUpper() : key_reference.ToLower();
        key_label.color = dark ? Color.black : this.base_color;
        UsingIcon = false;
    }

    // INPUT / RESET
    public override void OnInput()
    {
        base.OnInput();

        // we get the sprite from the bank
        Sprite sprite = bank.GetInputFeedbackSprite(dark ? "key_dark" : "key_light", empty: false);

        // we set the sprite to the image
        image.sprite = sprite;

        // we move the icon/key_label
        RectTransform rectTransform = UsingIcon ? icon.rectTransform : key_label.rectTransform;
        rectTransform.anchoredPosition += text_movement;

        if (!dark)
        {
            key_label.color = this.clicked_color;
            icon.color = this.clicked_color;
        }
    }
    public override void OnReset()
    {
        base.OnReset();

        // we get the sprite from the bank
        Sprite sprite = bank.GetInputFeedbackSprite(dark ? "key_dark" : "key_light");

        // we set the sprite to the image
        image.sprite = sprite;

        // we move the icon/key_label
        RectTransform rectTransform = UsingIcon ? icon.rectTransform : key_label.rectTransform;
        rectTransform.anchoredPosition = Vector2.zero;

        if (!dark)
        {
            key_label.color = this.base_color;
            icon.color = this.base_color;
        }
    }

    // PRESS & RELEASE MODE
    public override void HandlePressAndReleaseInput(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.5f) // means we pressed the input
        {
            OnInput();
        }
        else // means we released the input
        {
            OnReset();
        }
    }
}

