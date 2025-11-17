using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_EventButton : UI_Button
{

    [Header("Color Button Settings")]
    [SerializeField] private Color baseColor = Color.red;
    [SerializeField] private Color hoverColor = Color.white;
    [SerializeField] private Color iconHoverColor = Color.white;
    [SerializeField] private Image btn_icon;

    [Header("Colorers")]
    public List<UI_Colorer> colorers = new List<UI_Colorer>();


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
}