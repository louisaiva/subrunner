using UnityEngine.EventSystems;

public class UI_ExitButton : UI_EventButton
{
    // POINTER HANDLER
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        // we change the icon color
        btn_icon.color = hoverColor;
    }
}