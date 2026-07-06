using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ExitButton : UI_EventButton, Descriptable
{
    public string Name => "";
    public string desc = "close";
    public Color color = Color.red;
    public string Description => desc.AddColor(color);

    // POINTER HANDLER

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        // we change the icon color
        btn_icon.color = hoverColor;
    }
}