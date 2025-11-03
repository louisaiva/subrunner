using System.Collections;
using UnityEngine;

public class UI_PauseMenu : UI_Pool/* , Slottable */
{
    [Header("Slottable")]
    [SerializeField] private Transform slots_parent;
    [SerializeField] private UI_Slottable slottable;

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        // on active le navigator
        // UI_Navigator.Instance.Enable(this);
        slottable.Enable(ingame: false);
        yield break;
    }
    protected override IEnumerator disable_coroutine()
    {
        // on désactive le navigator
        // UI_Navigator.Instance.Disable(this);
        slottable.Disable();
        yield break;
    }

    // SLOTTABLE
    /* public List<UI_Slot> GetSlots()
    {
        // on récupère les slots
        List<UI_Slot> slots = new List<UI_Slot>();

        // on récupère les slots des texts
        for (int i = 0; i < slots_parent.childCount; i++)
        {
            Transform slot = slots_parent.GetChild(i);
            if (!slot.gameObject.activeSelf) { continue; }
            UI_Text text = slot.gameObject.GetComponent<UI_Text>();
            if (text == null) { continue; }
            slots.Add(text);
        }

        return slots;
    }
    public bool IsYourSlot(UI_Slot slot)
    {
        // on regarde si le slot est dans les slots
        if (slot.transform.IsChildOf(slots_parent))
        {
            return true;
        }
        return false;
    } */
    // public Vector2 SavedPosition { get => base_position; }
}