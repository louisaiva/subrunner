
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
    public override List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {
        List<GameObject> slots = base.GetSlots(ref base_position, ref angle_threshold, ref angle_multiplicator);
        slots.Add(onoff_toggle.gameObject);
        return slots;
    }
    public override bool IsYourSlot(GameObject slot)
    {
        if (slot == onoff_toggle.gameObject) { return true; }
        return base.IsYourSlot(slot);
    }
    // public Vector2 SavedPosition { get => new Vector2(0f, Screen.height); }

}