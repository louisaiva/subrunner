using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI_SlottableMixer is a UI_Slottable that can unify multiple
/// UI_Slottable and make them act as a single UI_Slottable.
/// Used mainly in Menus that need to have multiple slottables.
/// </summary>
public class UI_SlottableMixer : UI_Slottable, Awakable
{
    [Header("UI_Slottables")]
    [SerializeField] protected List<UI_Slottable> slottables = new List<UI_Slottable>();
    [SerializeField] protected UI_Slottable master_slottable; // for knowing which slottable gives its starting slot
    public int Count => slottables.Count;
    public List<UI_Slottable> Slottables { get => slottables; }

    public void InitAwake()
    {
        for (int i = 0; i < slottables.Count; i++)
        {
            slottables[i].Mixer = this;
        }
    }

    // ENABLE - DISABLE
    public override void HandleSlotHover(UI_Slot slot)
    {
        if (!IsYourSlot(slot)) { return; } // we check if the slot belongs to us
        // we check if this ain't the exit btn because we don't want it to register as starting slot if possible
        if (slot is UI_ExitButton) { return; }

        // we call HandleSlotHover on all slottables
        for (int i = 0; i < slottables.Count; i++)
        {
            slottables[i].HandleSlotHover(slot);

            if (master_slottable != null && master_slottable == slottables[i] && slottables[i].IsYourSlot(slot))
            {
                // we store the last selected slot only from the master slottable
                set_starting_slot(slot);
            }
        }

        // if master_slottable is null then we register the slot regardless of the slottable
        if (master_slottable == null) { set_starting_slot(slot); }
    }

    // ADD REMOVE SLOTTABLES
    public void AddSlottable(UI_Slottable slottable, bool is_master = false)
    {
        // on ajoute le slottable
        if (slottable == null) { return; }
        if (slottables.Contains(slottable)) { return; }
        slottables.Add(slottable);

        // si on veut pas override le master on s'arrete la !
        if (!is_master) { return; }

        // sinon on met le master
        master_slottable = slottable;
        if (starting_slot != null) { return; }

        // on essaie de mettre le starting slot du master
        if (slottable.StartingSlot != null) { starting_slot = slottable.StartingSlot; return; }

        // si le master n'en a pas on essaie de définir le starting slot sur le 1er slot du master
        List<UI_Slot> slots = slottable.GetSlots();
        if (slots.Count == 0) { return; }
        UI_Slot start_slot = slots[0];
        slottable.HandleSlotHover(start_slot);
        starting_slot = start_slot;
    }
    public void RemoveSlottable(UI_Slottable slottable)
    {
        if (slottable == null) { return; }
        if (!slottables.Contains(slottable)) { return; }

        slottables.Remove(slottable);
        slottable.Disable();

        if (master_slottable == slottable) { master_slottable = null; }
        if (starting_slot != null && slottable.IsYourSlot(starting_slot)) { starting_slot = null; }
    }

    // GET SLOTS & IS YOUR SLOT
    public override List<UI_Slot> GetSlots()
    {
        if (log) { Debug.Log($"(UI_SlottableMixer) {name} getting slots"); }
        List<UI_Slot> slots = new List<UI_Slot>();

        // on ajoute tous les slots de tous les slottables
        for (int i = 0; i < slottables.Count; i++)
        {
            if (log) { Debug.Log($"(UI_SlottableMixer) {name} getting slots from slottable {i} : {slottables[i].name}"); }
            slots.AddRange(slottables[i].GetSlots());
        }

        return slots;
    }
    public override bool IsYourSlot(UI_Slot slot)
    {
        for (int i = 0; i < slottables.Count; i++)
        {
            if (slottables[i].IsYourSlot(slot)) { return true; }
        }
        return false;
    }

    // GET BUTTONS & TOGGLES
    public override UI_Button GetButtonByName(string button_name)
    {
        for (int i = 0; i < slottables.Count; i++)
        {
            UI_Button button = slottables[i].GetButtonByName(button_name);
            if (button != null) { return button; }
        }
        return null;
    }
    public override UI_Toggle GetToggleByName(string toggle_name)
    {
        for (int i = 0; i < slottables.Count; i++)
        {
            UI_Toggle toggle = slottables[i].GetToggleByName(toggle_name);
            if (toggle != null) { return toggle; }
        }
        return null;
    }
}