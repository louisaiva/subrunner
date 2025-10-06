using System.Collections.Generic;
using UnityEngine;

public class UI_Device : UI_Pool, I_UI_Slottable
{
    private Device device;

    [Header("UI Buttons")]
    public List<UI_Button> buttons;


    // DEVICE MANAGEMENT
    public void SetDevice(Device dev)
    {
        device = dev;
    }
    public Device GetDevice() { return device; }

    // POOL
    public override async Awaitable Show(float duration, List<GameObject> dont_show = null)
    {
        await base.Show(duration, dont_show);
        UI_XboxNavigator.Instance.Enable(this);
    }
    public override async Awaitable Hide(float duration, List<GameObject> dont_hide = null)
    {
        UI_XboxNavigator.Instance.Disable(this);
        await base.Hide(duration, dont_hide);
    }

    // SLOTTABLE
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {
        // on récupère les slots
        List<GameObject> slots = new List<GameObject>();

        // on récupère les slots des boutons
        foreach (UI_Button button in buttons)
        {
            if (!button.gameObject.activeSelf) { continue; }
            slots.Add(button.gameObject);
        }

        return slots;
    }
    public bool IsYourSlot(GameObject slot)
    {
        // on regarde si le slot est dans les slots
        if (slot.GetComponent<UI_Button>() == null) { return false; }
        if (buttons.Contains(slot.GetComponent<UI_Button>())) { return true; }
        return false;
    }
    public Vector2 SavedPosition { get; private set; } = Vector2.zero;
}