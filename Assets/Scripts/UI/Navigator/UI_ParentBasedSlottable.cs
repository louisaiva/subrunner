using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI_ParentBasedSlottable is a UI_Slottable that does not store
/// any UI_Slot BUT find them as children of a specific transform
/// when GetSlots() is called
/// </summary>
public class UI_ParentBasedSlottable : UI_Slottable
{
    [Header("Parents of Slots")]
    [SerializeField] protected List<Transform> slots_parents = new List<Transform>();

    // GETTERS
    public override List<UI_Slot> GetSlots()
    {
        List<UI_Slot> slots = new List<UI_Slot>();
        Transform parent;
        Transform child;
        UI_Slot slot;
        for (int i= 0; i < slots_parents.Count; i++)
        {
            parent = slots_parents[i];
            for (int j= 0; j < parent.childCount; j++)
            {
                child = parent.GetChild(j);
                if (child.gameObject.activeSelf == false) { continue; }
                slot = child.GetComponent<UI_Slot>();
                if (slot == null) { continue; }
                if (slot.Disabled) { continue; }
                slots.Add(slot);
            }
        }
        return slots;
    }
    public override bool IsYourSlot(UI_Slot slot) { return GetSlots().Contains(slot); }
}