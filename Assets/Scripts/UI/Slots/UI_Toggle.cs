using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Toggle : UI_ImageSlot
{

    [Header("Toggle Events")]
    public System.Action OnOn; // when we turn on
    public System.Action OnOff; // when we turn off
    [SerializeField] protected bool is_on = false; // is the toggle on or off?
    public bool IsOn { get { return is_on; } }

    // AWAKE
    protected virtual void Awake() { Enable(); } // just to reset the image

    // POINTER HANDLER
    public override void OnPointerClick(PointerEventData eventData)
    {
        // Call the base class method
        base.OnPointerClick(eventData);

        // we toggle the state
        is_on = !is_on;
        if (is_on) OnOn?.Invoke();
        else OnOff?.Invoke();

        if (log) { Debug.Log("(UI_Toggle) Toggle clicked - " + (is_on ? "ON" : "OFF")); }
    }
}