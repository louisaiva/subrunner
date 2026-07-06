using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// This class is different than InputImageFeedback because it carries its own images
/// : no need for sprite bank anymore
/// it also colors its image when OnInput and uncolor when OnReset
/// + has colorers involved
/// </summary>
public class ManualImageFeedback : InputFeedback, Colorant
{

    [Header("Image")]
    [SerializeField] protected Image image;

    [Header("Colors")]
    [SerializeField] protected Color base_color = new Color(1f, 1f, 1f, 1f);
    [SerializeField] protected Color clicked_color = new Color(1f, 1f, 0f, 1f);
    [SerializeField] protected List<UI_Colorer> colorers = new List<UI_Colorer>();
    public List<UI_Colorer> Colorers { get => colorers; }

    [Header("Sprites")]
    [SerializeField] protected Sprite base_sprite;
    // only has base sprite, please inherit the class to add sprites

    public override void InitializeWithAction(InputAction action)
    {
        // we verify the image & the input
        if (log)
        {
            if (image == null) { Debug.LogWarning("(InputImageFeedback : " + name + " ) image is not set ! you should assign it in the inspector"); }
        }

        base.InitializeWithAction(action);
    }

    public override void OnInput()
    {
        base.OnInput();

        // we set the color
        image.color = clicked_color;

        // we apply the colorers color if we have any
        foreach (UI_Colorer colorer in colorers)
        {
            colorer.ApplyColor(clicked_color);
        }
    }
    public override void OnReset()
    {
        base.OnReset();

        // we set the color
        image.color = base_color;

        // and sprite
        image.sprite = base_sprite;

        // we revert the colorers color if we have any
        foreach (UI_Colorer colorer in colorers)
        {
            colorer.RevertColor();
        }
    }


    // SETTERS
    public Color HoverColor => clicked_color;
    public void SetColors(Color base_color, Color clicked_color)
    {
        this.base_color = base_color;
        this.clicked_color = clicked_color;

        // we apply the base color immediately
        image.color = is_pressed ? clicked_color : base_color;

        // we color all colorers
        if (!is_pressed) { return; } // no need to update colorers if not hovered since colorers handle their own reset color
        for (int i = 0; i < colorers.Count; i++)
        {
            colorers[i].ApplyColor(clicked_color);
        }
    }
    public void SetColor(Color color)
    {
        SetColors(base_color, color);
    }
}