using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI_Slotter is a UI_Slottable that can hold
/// some UI_Slots (the type of slots is not specified)
/// this is the most basic UI_Slottable. used for buttons ?
/// </summary>
public class UI_Slotter : UI_Slottable
{
    [SerializeField] protected bool log_btn_check = false;

    [Header("UI_Slots")]
    [SerializeField] protected List<UI_Slot> slots = new List<UI_Slot>();

    // GETTERS
    public override List<UI_Slot> GetSlots() { return slots; }
    public override bool IsYourSlot(UI_Slot slot) { return slots.Contains(slot); }


    // GET BUTTONS & TOGGLES
    public override UI_Button GetButtonByName(string button_name)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null) { continue; }
            if (slots[i] is not UI_Button ui_btn) { continue; }
            if (ui_btn.name != button_name) { continue; }
            return ui_btn;
        }
        return null;
    }
    public override UI_Toggle GetToggleByName(string toggle_name)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null) { continue; }

            if (log_btn_check) { Debug.Log($"(UI_Slotter) {name} checking slot {slots[i].name} for toggle {toggle_name} (type is {slots[i].GetType()})"); }

            if (slots[i] is not UI_Toggle ui_toggle) { continue; }
            if (ui_toggle.name != toggle_name) { continue; }
            return ui_toggle;
        }
        return null;
    }
}