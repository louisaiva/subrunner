using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_EventButton : UI_Button, Colorant
{

    [Header("Color Button Settings")]
    [SerializeField] protected Color baseColor = Color.red;
    [SerializeField] protected Color hoverColor = Color.white;
    [SerializeField] protected Color iconHoverColor = Color.white;
    [SerializeField] protected Image btn_icon;

    [Header("Colorers")]
    public List<UI_Colorer> colorers = new List<UI_Colorer>();
    public List<UI_Colorer> Colorers => colorers;


    [Header("Event")]
    public UnityEvent onClickedEvent;

    // POINTER HANDLER
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        // we change the icon color
        btn_icon.color = iconHoverColor;
        image.color = baseColor; // and the main image color

        // we color all colorers
        for (int i = 0; i < colorers.Count; i++)
        {
            colorers[i].ApplyColor(baseColor);
        }
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        // we change the icon color
        btn_icon.color = baseColor;
        image.color = hoverColor; // and the main image color

        // we color all colorers
        for (int i = 0; i < colorers.Count; i++)
        {
            colorers[i].RevertColor();
        }
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // we invoke the event
        onClickedEvent.Invoke();
    }

    // COLORANT
    public Color HoverColor => baseColor;
    public void SetColors(Color base_color, Color clicked_color)
    {
        // special case for UI_EventButton the colors are inversed (the base color is the base color of the icon when not hovered -> means its the clicked one)
        baseColor = clicked_color;
        hoverColor = base_color;

        // we change the icon color
        btn_icon.color = iconHoverColor;
        image.color = Hovered ? baseColor : hoverColor; // and the main image color

        // we color all colorers
        if (!Hovered) { return; } // no need to update colorers if not hovered since colorers handle their own reset color
        for (int i = 0; i < colorers.Count; i++)
        {
            colorers[i].ApplyColor(baseColor);
        }
    }
}