
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Oven : UI_Inventory
{
    [Header("Specials UI_ItemPool")]
    [SerializeField] private UI_ItemPool hob_pool;
    [SerializeField] private UI_ItemPool food_pool;

    [Header("Icons")]
    public Image fire;
    public UI_OnOffToggle onoff_toggle;

    private Oven Oven { get { return Inventory.capable as Oven; } }

    // INIT
    public override void Init()
    {
        base.Init();

        // we subscribe to the oven toggle
        onoff_toggle.OnOn += () => Oven.PowerOn();
        onoff_toggle.OnOff += () => Oven.PowerOff();
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