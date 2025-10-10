using System.Collections.Generic;
using UnityEngine;

public class UI_Window : MonoBehaviour
{
    [Header("Window Settings")]
    [SerializeField] private WindowType type = WindowType.Null;
    public WindowType Type { get { return type; } }

    [Header("UI Buttons")]
    [SerializeField] private Transform content_parent; // parent of the buttons to find in this window
    [SerializeField] private List<UI_Button> buttons = new List<UI_Button>(); // list of buttons in this window
    public List<UI_Button> Buttons { get { return buttons; } }

    public void InitWindow()
    {
        buttons.Clear();
        for (int i = 0; i < content_parent.childCount; i++)
        {
            Transform child = content_parent.GetChild(i);

            // todo here we will check for future ui elements

            // here we check for btn
            UI_Button button = child.GetComponent<UI_Button>();
            if (button == null) { continue; }
            buttons.Add(button);
        }
    }

    public bool IsYourButton(UI_Button btn)
    {
        return buttons.Contains(btn);
    }
}

public enum WindowType
{
    Null,
    Password,
    Device,
    Connection,
    Explorer,
    Exit,
    Camera
}