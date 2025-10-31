using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI_Slotter is a UI_Slottable that can hold
/// some UI_Slots (the type of slots is not specified)
/// this is the most basic UI_Slottable. used for buttons ?
/// </summary>
public class UI_Slotter : UI_Slottable
{
    [Header("UI_Slots")]
    [SerializeField] protected List<UI_Slot> slots = new List<UI_Slot>();

    // GETTERS
    public override List<UI_Slot> GetSlots() { return slots; }
    public override bool IsYourSlot(UI_Slot slot) { return slots.Contains(slot); }
}