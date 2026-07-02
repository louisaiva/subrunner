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
    public List<Transform> SlotsParents { get { return slots_parents; } set { slots_parents = value; } }

    [Header("Parameters")]
    [SerializeField] protected bool deep_search = false; // if true, GetSlots will search for slots in all children, not only direct ones

    // GETTERS
    public override List<UI_Slot> GetSlots()
    {
        List<UI_Slot> slots = new List<UI_Slot>();
        Transform parent;
        for (int i = 0; i < slots_parents.Count; i++)
        {
            parent = slots_parents[i];
            if (deep_search) { add_deep_slots(parent, ref slots); }
            else { add_direct_slots(parent, ref slots); }
        }
        return slots;
    }
    private void add_direct_slots(Transform parent, ref List<UI_Slot> slots)
    {
        Transform child;
        UI_Slot slot;
        for (int j = 0; j < parent.childCount; j++)
        {
            child = parent.GetChild(j);
            if (child.gameObject.activeSelf == false) { continue; }
            slot = child.GetComponent<UI_Slot>();
            if (slot == null) { continue; }
            if (slot.Disabled) { continue; }
            slots.Add(slot);
        }
    }
    private void add_deep_slots(Transform parent, ref List<UI_Slot> active_slots)
    {
        // Debug.Log("Adding deep slots from parent : " + parent.name);
        UI_Slot[] slots = parent.GetComponentsInChildren<UI_Slot>(includeInactive: true);
        for (int i = 0; i < slots.Length; i++)
        {
            // Debug.Log("Checking slot : " + slots[i].name);
            if (slots[i] == null) { continue; }
            if (slots[i].gameObject.activeSelf == false) { continue; }
            if (!slots[i].gameObject.activeInHierarchy) { continue; }
            if (slots[i].gameObject.layer != LayerMask.NameToLayer("UI_Slot")) { continue; }
            if (slots[i].Disabled) { continue; }
            active_slots.Add(slots[i]);
        }
    }
    public override bool IsYourSlot(UI_Slot slot) { return GetSlots().Contains(slot); }
}