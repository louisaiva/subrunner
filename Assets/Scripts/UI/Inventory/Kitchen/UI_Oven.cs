
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