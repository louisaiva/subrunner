using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ExitButton : UI_Button
{

    [Header("Exit Button Settings")]
    [SerializeField] private Color baseColor = Color.red;
    [SerializeField] private Color hoverColor = Color.white;
    [SerializeField] private Image btn_icon;

    // POINTER HANDLER
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        
        // we change the icon color
        btn_icon.color = hoverColor;
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        
        // we change the icon color
        btn_icon.color = baseColor;
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        // we change the icon color
        btn_icon.color = hoverColor;
    }
}