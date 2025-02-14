using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class UI_Text : MonoBehaviour, I_UI_Slot
{

    // callback
    public System.Action<InputAction.CallbackContext> Activate_callback
    {
        get
        {
            return ctx => OnPointerClick(null);
        }
    }
    // hoover
    [Header("Hover")]
    public Color hover_color = new Color(1, 1, 0, 1);
    public bool is_hoovered { get; set; }

    [Header("Text")]
    private TextMeshProUGUI tmp;
    [SerializeField] private string base_text;

    [Header("Debug")]
    public bool debug = false;

    // unity functions
    protected void Awake()
    {
        // on récupère le tmp
        tmp = GetComponent<TextMeshProUGUI>();
        base_text = tmp.text;
    }


    // MAIN CLICK FUNCTIONS
    public void play()
    {
        transform.parent.parent.GetComponent<UI_PauseMenu>().hide();
    }

    public void exit()
    {
        #if UNITY_EDITOR
        Debug.Log("exiting playmode...");
        UnityEditor.EditorApplication.ExitPlaymode();
        #endif
        Application.Quit();

        transform.parent.parent.GetComponent<UI_PauseMenu>().hide();
    }

    /* public void regenerate_world()
    {
        tmp.text = "> " + base_text + " (that can take a while oopsie ;-;)";
        GameObject.Find("/loader").GetComponent<WorldGenerator>().Start();
        transform.parent.parent.GetComponent<UI_PauseMenu>().hide();
    }

    public void cheat()
    {
        transform.parent.parent.GetComponent<UI_PauseMenu>().hide();
        Debug.Log("cheating...");
        // GameObject.Find("/perso").GetComponent<Perso>().cheat();
    } */



    // Descriptable
    public string getDescription() {return "";}
    public bool shouldDescriptionBeShown() {return false;}


    // interface functions
    public void OnPointerEnter(PointerEventData eventData)
    {

        tmp.color = hover_color;
        tmp.text = "> " + base_text;

        // on met à jour le fait qu'on est survolé
        is_hoovered = true;

        if (debug) Debug.Log("hovering " + base_text);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tmp.color = new Color(1, 1, 1, 1);
        tmp.text = base_text;

        // on met à jour le fait qu'on est survolé
        is_hoovered = false;

        if (debug) Debug.Log("unhovering " + base_text);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (debug) Debug.Log("clicking on " + base_text);

        switch (base_text)
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
                break; */
        }
    }

    // reset hoover
    /* public void resetHoover()
    {
        if (!is_hoovered) return;
        OnPointerExit(null);
    } */
}