using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_TMP_Button : UI_Button, Colorant, Descriptable
{

    [Header("Color Button Settings")]
    [SerializeField] protected Color baseColor = Color.red;
    [SerializeField] protected Color hoverColor = Color.white;
    [SerializeField] protected Color textHoverColor = Color.white;

    [Header("Colorers")]
    public List<UI_Colorer> colorers = new List<UI_Colorer>();
    public List<UI_Colorer> Colorers => colorers;
    public System.Action<UI_TMP_Button, List<UI_Colorer>> OnColored = delegate { };
    public System.Action<UI_TMP_Button, List<UI_Colorer>> OnUncolored = delegate { };

    [Header("Descriptable")]
    [SerializeField] protected string description;
    public string Name => name;
    public string Description => description;


    [Header("Event")]
    public UnityEvent onClickedEvent;

    protected virtual void Start()
    {
        // we set the base color
        image.color = baseColor;

        // we color all colorers
        for (int i = 0; i < colorers.Count; i++) { colorers[i].RevertColor(); }
        OnUncolored?.Invoke(this, colorers);
    }

    // POINTER HANDLER
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        image.color = hoverColor;

        // we color all colorers
        for (int i = 0; i < colorers.Count; i++) { colorers[i].ApplyColor(textHoverColor); }
        OnColored?.Invoke(this, colorers);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        image.color = baseColor;

        // we color all colorers
        for (int i = 0; i < colorers.Count; i++) { colorers[i].RevertColor(); }
        OnUncolored?.Invoke(this, colorers);
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // we invoke the event
        onClickedEvent.Invoke();
    }

    // COLORANT
    public Color HoverColor => hoverColor;
    public void SetColors(Color base_color, Color hover_color, Color? text_hover_color = null)
    {
        baseColor = base_color;
        hoverColor = hover_color;
        image.color = Hovered ? baseColor : hoverColor;
        if (text_hover_color != null) { textHoverColor = text_hover_color.Value; }

        // we color all colorers
        if (!Hovered) { return; } // no need to update colorers if not hovered since colorers handle their own reset color
        for (int i = 0; i < colorers.Count; i++) { colorers[i].ApplyColor(textHoverColor); }
    }
    public void SetColor(Color color) { SetColors(this.baseColor, color); }
    public void SetColors(Color base_color, Color clicked_color) { SetColors(base_color, clicked_color, null); }
}