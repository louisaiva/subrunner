using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI_Slottable is the highest UI representation of any Slottable UI.
/// can handle the showing of multiple ui_slots gameobjects
/// it is the mother of the UI_Inventory, UI_Slotter & UI_SlottableMixer
/// </summary>
public abstract class UI_Slottable : MonoBehaviour, Slottable
{
    [Header("Starting Slot")]
    [SerializeField] protected UI_Slot starting_slot;
    public UI_Slot StartingSlot { get { return starting_slot; } }

    [Header("Logs")]
    public bool log = false;
    public bool log_starting_slot = false;

    // ENABLE - DISABLE
    public virtual void Enable(bool ingame = false)
    {
        // on register to navigator's hover slot
        UI_Navigator.Instance.OnSlotHoverEnter += HandleSlotHover;

        // on enregistre le slottable
        UI_Navigator.Instance.AddSlottable(this, ingame_navigation: ingame);
    }
    public virtual void Disable()
    {
        // on unregister to Navigator's hover slot
        UI_Navigator.Instance.OnSlotHoverEnter -= HandleSlotHover;

        // on remove le slottable
        UI_Navigator.Instance.RemoveSlottable(this);
    }
    public virtual void HandleSlotHover(UI_Slot slot)
    {
        if (!IsYourSlot(slot)) { return; } // we check if the slot belongs to us
        if (starting_slot == slot) { return; } // if it's already the starting slot we do nothing

        // on unregister le callback de disabling starting slot
        remove_starting_slot();

        // on stocke le dernier slot selectionné
        set_starting_slot(slot);
    }
    protected void remove_starting_slot(UI_Slot slot = null)
    {
        if (starting_slot == null) { return; }
        starting_slot.OnSlotDisabled -= remove_starting_slot;
        starting_slot = null;
        if (log_starting_slot) { Debug.Log("(UI_Slottable) removed starting slot"); }
    }
    protected void set_starting_slot(UI_Slot slot)
    {
        starting_slot = slot;
        starting_slot.OnSlotDisabled += remove_starting_slot;
        if (log_starting_slot) { Debug.Log("(UI_Slottable) set starting slot to : " + slot.name); }
    }


    // GETTERS
    public virtual List<UI_Slot> GetSlots() { return new List<UI_Slot>(); } // for now we don't have any slots
    public virtual bool IsYourSlot(UI_Slot slot) { return false; } // same so always false
}