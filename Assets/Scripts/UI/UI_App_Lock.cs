using UnityEngine;
using UnityEngine.EventSystems;

public class UI_App_Lock : UI_App
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        
        this.ui_computer.GetComponent<UI_Computer>().Lock();
    }
}