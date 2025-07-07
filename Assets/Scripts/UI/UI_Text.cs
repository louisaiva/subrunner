using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Events;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class UI_Text : MonoBehaviour, I_UI_Slot
{

    // hover
    [Header("Hover")]
    public Color hover_color = new Color(1, 1, 0, 1);
    public Color down_color = new Color(1, 1, 1, 1);
    public bool is_hovered { get; set; }

    [Header("Text")]
    private TextMeshProUGUI tmp;
    [SerializeField] private string base_text;

    [Header("Events")]
    [SerializeField] private UnityEvent activateEvent;

    [Header("Logs")]
    public bool debug = false;

    // unity functions
    protected void Awake()
    {
        // on récupère le tmp
        tmp = GetComponent<TextMeshProUGUI>();
        base_text = tmp.text;

        // on met à jour le callback
        // activateCallback = ctx => OnPointerClick(null);
    }


    // MAIN CLICK FUNCTIONS
    public void play()
    {
        // transform.parent.parent.GetComponent<UI_PauseMenu>().hide();
        GameObject.Find("/ui").GetComponent<UI_Manager>().SwitchTo("hud");
    }
    public void exit()
    {
        #if UNITY_EDITOR
                Debug.Log("exiting playmode...");
                UnityEditor.EditorApplication.ExitPlaymode();
        #endif
        Application.Quit();

        // GameObject.Find("/ui").GetComponent<UI_Manager>().TogglePool("pause");
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

    // Descriptable
    public string getDescription() { return ""; }
    public bool shouldDescriptionBeShown() { return false; }


    // interface functions
    public void OnPointerEnter(PointerEventData eventData)
    {
        tmp.color = hover_color;
        tmp.text = "> " + base_text;
        // tmp.fontStyle = FontStyles.Bold;

        // on met à jour le fait qu'on est survolé
        is_hovered = true;

        if (debug) Debug.Log("hovering " + base_text);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        tmp.color = new Color(1, 1, 1, 1);
        tmp.text = base_text;
        // tmp.fontStyle = FontStyles.Normal;

        // on met à jour le fait qu'on est survolé
        is_hovered = false;

        if (debug) Debug.Log("unhovering " + base_text);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (debug) Debug.Log("clicking on " + base_text);

        // reset the color & text
        tmp.text = base_text;
        tmp.color = new Color(1, 1, 1, 1);

        // invoke the event
        activateEvent?.Invoke();


        /* switch (base_text)
        {
            case "play":
                play();
                break;
            case "exit game":
                exit();
                break;
                /* case "regenerate world":
                    regenerate_world();
                    break;
                case "cheat":
                    cheat();
                    break; 
        } */
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        tmp.color = down_color;

        if (debug) { Debug.Log("downing " + gameObject.name); }
    }
}