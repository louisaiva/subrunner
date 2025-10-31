
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Sink : UI_Inventory
{

    [Header("Icons")]
    public UI_OnOffToggle onoff_toggle;

    private Sink Sink { get { return Inventory.capable as Sink; } }

    // INIT
    public override void Init()
    {
        base.Init();

        // we subscribe to the sink toggle
        onoff_toggle.OnOn += () => Sink.PowerOn();
        onoff_toggle.OnOff += () => Sink.PowerOff();
    }

    // SLOTTABLE
    public override List<UI_Slot> GetSlots()
    {
        List<UI_Slot> slots = base.GetSlots();
        slots.Add(onoff_toggle);
        return slots;
    }
    public override bool IsYourSlot(UI_Slot slot)
    {
        if (slot == onoff_toggle) { return true; }
        return base.IsYourSlot(slot);
    }
    // public Vector2 SavedPosition { get => new Vector2(0f, Screen.height); }

}