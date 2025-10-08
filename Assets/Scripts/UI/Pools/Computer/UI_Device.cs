using System.Collections.Generic;
using UnityEngine;

public class UI_Device : UI_Pool, I_UI_Slottable
{
    private Device device;

    [Header("Windows")]
    [SerializeField] private Transitioner windows_parent; // parent of the windows to show/hide with the pool
    [SerializeField] private List<UI_Window> windows; // list of windows used by the current device

    // DEVICE MANAGEMENT
    public void SetDevice(Device dev)
    {
        device = dev;

        // we go through our windows parent and we enable only the windows that are of the device's windows types
        List<WindowType> device_windows_types = device.WindowsTypes;
        windows.Clear();
        for (int i = 0; i < windows_parent.transform.childCount; i++)
        {
            Transform child = windows_parent.transform.GetChild(i);
            UI_Window window = child.GetComponent<UI_Window>();
            if (window == null) { continue; }

            // we check if the window type is in the device's windows types
            if (!device_windows_types.Contains(window.Type)) { continue; }
            window.gameObject.SetActive(true);
            window.InitWindow();
            windows.Add(window);
        }

        // we show the windows transitioner
        windows_parent.Show();
    }
    public void ClearDevice()
    {
        device = null;

        // we disable all the windows
        windows.Clear();
        for (int i = 0; i < windows_parent.transform.childCount; i++)
        {
            Transform child = windows_parent.transform.GetChild(i);
            UI_Window window = child.GetComponent<UI_Window>();
            if (window == null) { continue; }
            window.gameObject.SetActive(false);
        }

        // we hide the windows transitioner
        windows_parent.Hide();
    }
    public Device GetDevice() { return device; }

    // POOL
    public override async Awaitable Show(List<GameObject> dont_show = null)
    {
        await base.Show(dont_show);
        UI_XboxNavigator.Instance.Enable(this);
    }
    public override async Awaitable Hide(List<GameObject> dont_hide = null)
    {
        UI_XboxNavigator.Instance.Disable(this);
        await base.Hide(dont_hide);
    }

    // SLOTTABLE
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {
        // on récupère les slots
        List<GameObject> slots = new List<GameObject>();

        // on récupère les slots des boutons
        for (int i = 0; i < windows.Count; i++)
        {
            UI_Window window = windows[i];
            List<UI_Button> buttons = window.Buttons;
            if (buttons.Count == 0) { continue; }
            for (int j = 0; j < buttons.Count; j++)
            {
                UI_Button button = buttons[j];
                slots.Add(button.gameObject);
            }
        }
        return slots;
    }
    public bool IsYourSlot(GameObject slot)
    {
        // on regarde si le slot est dans les slots
        if (slot.GetComponent<UI_Button>() == null) { return false; }
        UI_Button btn = slot.GetComponent<UI_Button>();
        for (int i = 0; i < windows.Count; i++)
        {
            UI_Window window = windows[i];
            if (window.IsYourButton(btn)) { return true; }
        }
        
        return false;
    }
    public Vector2 SavedPosition { get; private set; } = Vector2.zero;


    // BUTTONS MANAGEMENT


}