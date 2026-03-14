using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Events;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class UI_Text : UI_Slot
{

    // hover
    [Header("Hover")]
    public Color hover_color = new Color(1, 1, 0, 1);
    public Color down_color = new Color(1, 1, 1, 1);
    // public bool is_hovered { get; set; }

    [Header("Text")]
    private TextMeshProUGUI _tmp;
    protected TextMeshProUGUI tmp
    {
        get
        {
            if (_tmp == null)
            {
                _tmp = GetComponent<TextMeshProUGUI>();
                base_text = tmp.text;
            }
            return _tmp;
        }
    }
    [SerializeField] protected string base_text;

    [Header("Events")]
    [SerializeField] protected UnityEvent activateEvent;

    // TEXT FUNCTIONS
    public void SetText(string new_text)
    {
        tmp.text = new_text;
        base_text = new_text;
    }


    // MAIN CLICK FUNCTIONS
    public void play()
    {
        // transform.parent.parent.GetComponent<UI_PauseMenu>().hide();
        UI_Manager.Instance.SwitchToHUD();
    }
    public void exit()
    {
        #if UNITY_EDITOR
                Debug.Log("exiting playmode...");
                UnityEditor.EditorApplication.ExitPlaymode();
        #endif
        Application.Quit();
    }
    public void fullscreen()
    {
        #if UNITY_EDITOR
                EditorWindow window = EditorWindow.focusedWindow;
                // Assume the game view is focused.
                window.maximized = !window.maximized;
        #else
                Screen.fullScreen = !Screen.fullScreen;
        #endif
    }
    public void ghost_mode()
    {
        if (Perso.Instance == null) { return; }
        Perso.Instance.ToggleGhost();
    }
    public void metamorph()
    {
        if (Perso.Instance == null) { return; }
        Perso.Instance.Metamorph();
    }
    public void heal()
    {
        if (Perso.Instance == null) { return; }
        Perso.Instance.HealMax();
    }
    public void toggle_vsync()
    {
        AppManager.Instance.useVSync = !AppManager.Instance.useVSync;
    }
    public void credits()
    {
        UI_Manager.Instance.SwitchTo("credits");
    }

    // interface functions
    public override void OnPointerEnter(PointerEventData eventData)
    {
        tmp.color = hover_color;
        tmp.text = "> " + base_text;
        // tmp.fontStyle = FontStyles.Bold;

        base.OnPointerEnter(eventData);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        tmp.color = new Color(1, 1, 1, 1);
        tmp.text = base_text;
        // tmp.fontStyle = FontStyles.Normal;

        base.OnPointerExit(eventData);
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (log) Debug.Log("clicking on " + base_text);

        // reset the color & text
        tmp.text = base_text;
        tmp.color = new Color(1, 1, 1, 1);

        // invoke the event
        activateEvent?.Invoke();
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        tmp.color = down_color;

        if (log) { Debug.Log("downing " + gameObject.name); }
    }
}