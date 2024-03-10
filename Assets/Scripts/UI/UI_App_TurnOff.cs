using UnityEngine;
using UnityEngine.EventSystems;

public class UI_App_TurnOff : UI_App
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        
        if (this.ui_computer.GetComponent<UI_Computer>().computer != null)
        {
            this.ui_computer.GetComponent<UI_Computer>().computer.turnOff();
        }
    }
}