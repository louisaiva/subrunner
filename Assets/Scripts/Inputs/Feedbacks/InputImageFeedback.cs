using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// This specific input feedback handles its own rendering by applying a color
/// to the image component + has colorers involved
/// </summary>
public class InputImageFeedback : InputFeedback, Colorant
{

    [Header("Image")]
    [SerializeField] protected Image image;
    [SerializeField]
    protected SpriteBank bank
    {
        get
        {
            if (_bank != null) { return _bank; }

            // try to get the bank
            try { _bank = AnimBank.Instance?.GetComponent<SpriteBank>(); }
            catch { _bank = null; }
            
            return _bank;
        }
    }
    private SpriteBank _bank;

    [Header("Colors & Label")]
    [SerializeField] protected Color base_color = new Color(1f, 1f, 1f, 1f);
    [SerializeField] protected Color clicked_color = new Color(1f, 1f, 0f, 1f);
    [SerializeField] protected List<UI_Colorer> colorers = new List<UI_Colorer>();
    public List<UI_Colorer> Colorers { get => colorers; }
    [SerializeField] private TextMeshProUGUI label;


    protected override void Start()
    {
        // we verify the image & the input
        if (log)
        {
            if (image == null) { Debug.LogWarning("(InputImageFeedback : " + name + " ) image is not set ! you should assign it in the inspector"); }
            if (input == null) { Debug.LogWarning("(InputImageFeedback : " + name + " ) input is not set ! you should assign it in the inspector"); }
        }

        base.Start();
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

        // we revert the colorers color if we have any
        foreach (UI_Colorer colorer in colorers)
        {
            colorer.RevertColor();
        }
    }


    // SETTERS
    public void SetLabel(string text)
    {
        if (label == null)
        {
            if (log) { Debug.LogWarning("(InputFeedback : " + name + " ) label is not set ! you should assign it in the inspector"); }
            return;
        }
        label.text = text;
    }
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
}

public interface Colorant
{
    public List<UI_Colorer> Colorers { get; }
    public void SetColors(Color base_color, Color clicked_color);
}