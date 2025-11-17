using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Button : UI_ImageSlot, Droppable
{

    [Header("Button Events")]
    public System.Action OnClick; // event to trigger on click

    // AWAKE
    private void Awake() { Enable(); } // just to reset the image

    // POINTER HANDLER
    public override void OnPointerClick(PointerEventData eventData)
    {
        // Call the base class method
        base.OnPointerClick(eventData);
        OnClick?.Invoke(); // trigger the onClick event if it's assigned
    }
    public void OnPointerDropped(PointerEventData eventData)
    {
        // we click
        OnPointerClick(eventData);
    }
}